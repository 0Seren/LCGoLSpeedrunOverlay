using EasyHook;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D9;
using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Overlay;
using LCGoLOverlayProcess.Helpers;
using static EasyHook.RemoteHooking;
using System.Diagnostics;
using LCGoLOverlayProcess.Server;
using WinOSExtensions.Extensions;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace LCGoLOverlayProcess
{
    // ReSharper disable once UnusedType.Global
    public class InjectionEntryPoint : IEntryPoint
    {
        /// <summary>
        /// Bi-Directional communication with the injector.
        /// </summary>
        private readonly OverlayInterface _overlayInterface;
        private readonly ClientOverlayInterfaceEventProxy _clientEventProxy = new ClientOverlayInterfaceEventProxy();
        private readonly IpcServerChannel _clientServerChannel = null;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly Process _injectorProcess;
        private readonly LiveSplitHelper _liveSplitHelper;

        // LCGoL Items:
        private readonly Process _lcgolProcess;
        private readonly GameInfo _lcgolInfo;
        private readonly IOverlay _overlay;

        /// <summary>
        /// The steps run upon creation, but before injection.
        /// 
        /// The parameters for Run and the Constructor must be the same.
        /// </summary>
        /// <param name="context">Some context information about the environment in which this method is invoked.</param>
        /// <param name="channelName">The IPC Channel Name for communication.</param>
        /// <param name="lcGoLProcessId">The LCGoL Process Id.</param>
        public InjectionEntryPoint(IContext context, string channelName, int lcGoLProcessId)
        {
            // Get reference to IPC to host application
            // Note: any methods called or events triggered against _interface will execute in the host process.
            _overlayInterface = IpcConnectClient<OverlayInterface>(channelName);

            // Ping immediately to see if injection was successful
            _overlayInterface.Ping();

            // Attempt to create a IpcServerChannel so that any event handlers on the client will function correctly (bi-directional ipc)
            IDictionary properties = new Hashtable
            {
                ["name"] = channelName,
                ["portName"] = channelName + Guid.NewGuid().ToString("N")
            };

            var binaryProv = new BinaryServerFormatterSinkProvider
            {
                TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full
            };

            _clientServerChannel = new IpcServerChannel(properties, binaryProv);
            ChannelServices.RegisterChannel(_clientServerChannel, false);

            // Setup "global" variables
            _lcgolProcess = Process.GetProcessById(lcGoLProcessId);
            _lcgolInfo = new GameInfo(_lcgolProcess);
            _overlay = new LCGoLOverlay();

            _injectorProcess = Process.GetProcessById(context.HostPID);

            // TODO: Once integrated into livesplit, the livesplit process should be the _injectorProcess
            var livesplitprocess = _injectorProcess;
            //livesplitprocess = Process.GetProcessesByName("LiveSplit").FirstOrDefault();
            _liveSplitHelper = new LiveSplitHelper(livesplitprocess, _overlayInterface);
            //_speedRunComHelper = new SpeedRunComHelper(_server);
        }

#pragma warning disable IDE0060 // Remove unused parameter

        /// <summary>
        /// Run immediately after injection.
        /// Will be run in its own thread (I think), but under the target executable.
        /// 
        /// The parameters for Run and the Constructor must be the same.
        /// </summary>
        /// <param name="context">Some context information about the environment in which this method is invoked.</param>
        /// <param name="channelName">The IPC Channel Name for communication.</param>
        /// <param name="lcGoLProcessId">The LCGoL Process Id.</param>
        public void Run(IContext context, string channelName, int lcGoLProcessId)
        {
            // When not using GAC there can be issues with remoting assemblies resolving correctly
            // this is a workaround that ensures that the current assembly is correctly associated
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += (sender, args) =>
            {
                return GetType().Assembly.FullName == args.Name ? GetType().Assembly : null;
            };

            // Report Installed
            _overlayInterface.IsInstalled(GetCurrentProcessId());

            try
            {
                // Get d3d9 device addresses
                var d3d9FunctionAddresses = GetD3D9VTableAddresses();

                // Install Hook(s) to EndScene.
                var endSceneHook = LocalHook.Create(
                                                d3d9FunctionAddresses[(int)Direct3DDevice9FunctionOrdinals.EndScene],   // EndScene Function Address
                                                new EndSceneDelegate(EndSceneHook),                             // Our delegate/function to hook
                                                this
                                            );

                // Activate hooks
                endSceneHook.ThreadACL.SetExclusiveACL(new int[1]);

                // Report Hooks Installed
                _overlayInterface.SendMessage("EndScene Hook Installed");

                _overlayInterface.SendMessage($"Context's Process: {_injectorProcess.Id}:{_injectorProcess.GetApplicationName()}");
                _overlayInterface.SendMessage($"LCGOL Process:     {_lcgolProcess.Id}:{_lcgolProcess.GetApplicationName()}");


                _overlayInterface.Disconnected += _clientEventProxy.DisconnectedProxyHandler;

                // Important Note:
                // accessing the _interface from within a _clientEventProxy event handler must always 
                // be done on a different thread otherwise it will cause a deadlock
                _clientEventProxy.Disconnected += () =>
                {
                    // We can now signal the exit of the Run method
                    _cancellationTokenSource.Cancel();
                };

                StartCheckHostIsAliveThread();

                _cancellationTokenSource.Token.WaitHandle.WaitOne();

                StopCheckHostIsAliveThread();

                // Stop Injection if main thread ends
                endSceneHook.Dispose();
                LocalHook.Release();
            } catch (Exception e)
            {
                _overlayInterface.ReportException(e);
            } finally
            {
                try
                {
                    _overlayInterface.SendMessage($"Disconnecting from process {GetCurrentProcessId()}");
                }
                catch
                {
                }

                // Remove the client server channel (that allows client event handlers)
                ChannelServices.UnregisterChannel(_clientServerChannel);
                _cancellationTokenSource?.Dispose();

                // Always sleep long enough for any remaining messages to complete sending
                Thread.Sleep(100);
            }
        }
        
#pragma warning restore IDE0060 // Remove unused parameter

        /// <summary>
        /// This method is what is hooked onto the D3D9 EndScene Function.
        /// 
        /// Updates our GameInfo object, Renders our custom overlay, Reports our errors.
        /// Once our custom logic is done, we call the original EndScene function, and return Ok.
        /// </summary>
        /// <param name="device">A pointer to the D3D9 Device of the base executabe.</param>
        /// <returns>An OK Code.</returns>
        private int EndSceneHook(IntPtr device)
        {
            var dev = (Device)device;

            try
            {
                _lcgolInfo.Update();

                _overlay.Render(_lcgolInfo, dev, _liveSplitHelper);
            }
            catch (Exception e)
            {
                _overlayInterface.ReportException(e);
            }

            dev.EndScene();
            return Result.Ok.Code;
        }

        /// <summary>
        /// Populates a fake D3D9 device to get VTableAddresses.
        /// </summary>
        /// <returns>A list of the VTableAddresses for D3D9.</returns>
        private static IntPtr[] GetD3D9VTableAddresses()
        {
            using (var d3d = new Direct3D())
            {
                using (var renderform = new System.Windows.Forms.Form())
                {
                    using (var fakeDevice = new Device(d3d, 0, DeviceType.NullReference, IntPtr.Zero, CreateFlags.HardwareVertexProcessing, new PresentParameters() { BackBufferWidth = 1, BackBufferHeight = 1, DeviceWindowHandle = renderform.Handle }))
                    {
                        return GetVTblAddresses(fakeDevice.NativePointer, 119);
                    }
                }
            }
        }

        /// <summary>
        /// Reads in the VTable Addresses.
        /// </summary>
        /// <param name="pointer">Pointer to the Address of the VTable.</param>
        /// <param name="numberOfMethods">Number of methods to read from the VTable.</param>
        /// <returns>The Addresses in the VTable.</returns>
        private static IntPtr[] GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            var vtblAddresses = new List<IntPtr>();
            var vTable = Marshal.ReadIntPtr(pointer);
            for (var i = 0; i < numberOfMethods; i++)
                vtblAddresses.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size));
            return vtblAddresses.ToArray();
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        private delegate int EndSceneDelegate(IntPtr device);

        // _checkAlive needs to be a field to stay alive
        private Task _checkAlive;
        private long _stopCheckAlive = 0;
        /// <summary>
        /// This simply handles messages that need to be sent back to the injector.
        /// </summary>
        private void StartCheckHostIsAliveThread()
        {
            _checkAlive = new Task(() =>
            {
                try
                {
                    while (Interlocked.Read(ref _stopCheckAlive) == 0)
                    {
                        // Monitor for messages to send every 1/2 second
                        Thread.Sleep(500);

                        _overlayInterface.Ping();
                    }
                }
                catch
                {
                    // Can no longer contact the injector, so stop the injection.
                    _cancellationTokenSource.Cancel();
                }
            });

            _checkAlive.Start();
        }

        private void StopCheckHostIsAliveThread()
        {
            Interlocked.Increment(ref _stopCheckAlive);
        }
    }
}
