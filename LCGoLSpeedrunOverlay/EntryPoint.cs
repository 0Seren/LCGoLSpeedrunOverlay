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
using WinOSExtensions.Extensions;

namespace LCGoLOverlayProcess
{
    // ReSharper disable once UnusedType.Global
    public class InjectionEntryPoint : IEntryPoint
    {
        /// <summary>
        /// Used to send messages to the injector.
        /// </summary>
        private readonly ServerInterface _server;

        /// <summary>
        /// Message Queue to send to the injector.
        /// </summary>
        private readonly Queue<string> _messageQueue = new Queue<string>();

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
            _server = IpcConnectClient<ServerInterface>(channelName);

            _lcgolProcess = Process.GetProcessById(lcGoLProcessId);
            _lcgolInfo = new GameInfo(_lcgolProcess);
            _overlay = new LCGoLOverlay();

            _server.Ping();
            _injectorProcess = Process.GetProcessById(context.HostPID);
            // TODO: Once integrated into livesplit, the livesplit process should be the _injectorProcess
            var livesplitprocess = _injectorProcess;
            //livesplitprocess = Process.GetProcessesByName("LiveSplit").FirstOrDefault();
            _liveSplitHelper = new LiveSplitHelper(livesplitprocess, channelName, _server);
            //_speedRunComHelper = new SpeedRunComHelper(_server);
        }

#pragma warning disable IDE0060 // Remove unused parameter
// ReSharper disable once UnusedMember.Global
// ReSharper disable UnusedParameter.Global

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
            // Report Installed
            _server.IsInstalled(GetCurrentProcessId());

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
            _server.ReportMessage("EndScene Hook Installed");

            _server.ReportMessage($"Context's Process: {_injectorProcess.Id}:{_injectorProcess.GetApplicationName()}");
            _server.ReportMessage($"LCGOL Process:     {_lcgolProcess.Id}:{_lcgolProcess.GetApplicationName()}");

            // Main Thread Loop
            PerformMainThreadLoop();

            // Stop Injection if main thread ends
            endSceneHook.Dispose();
            LocalHook.Release();
        }

// ReSharper restore UnusedParameter.Global
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
            var changed = false;

            try
            {
                _lcgolInfo.Update();

                if (_lcgolInfo.Current.Level != _lcgolInfo.Previous.Level)
                {
                    changed = true;
                    _server.ReportMessage($"New Level: {_lcgolInfo.Current.Level}");
                }

                if (_lcgolInfo.Current.GameState != _lcgolInfo.Previous.GameState)
                {
                    changed = true;
                    _server.ReportMessage($"New State: {_lcgolInfo.Current.GameState}");
                }

                if (changed)
                {
                    _server.ReportMessage("End Frame.\n");
                }


                _overlay.Render(_lcgolInfo, dev, _liveSplitHelper);
            }
            catch (Exception e)
            {
                _server.ReportException(e);
            }

            dev.EndScene();
            return Result.Ok.Code;
        }

        /// <summary>
        /// This simply handles messages that need to be sent back to the injector.
        /// </summary>
        private void PerformMainThreadLoop()
        {
            try
            {
                while (true)
                {
                    // Monitor for messages to send every 1/2 second
                    System.Threading.Thread.Sleep(500);

                    string[] queued;

                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    if (queued.Length > 0)
                    {
                        _server.ReportMessages(queued);
                    }
                    else
                    {
                        _server.Ping();
                    }
                }
            }
            catch
            {
                // Can no longer contact the injector, so stop the injection.
            }
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
    }
}
