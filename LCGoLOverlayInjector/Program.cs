using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LCGoLOverlayProcess.Server;
using WinOSExtensions.Extensions;
using System.Runtime.Remoting.Channels.Ipc;
using WinOSExtensions.DLLWrappers;

namespace LCGoLInjector
{
    public static class Program
    {
        private static OverlayInterface _overlayInterface;
        private static IpcServerChannel _overlayServer;

        /// <summary>
        /// Finds the Lara Croft and the Guardian of Light instance, then injects our DLL into it.
        /// </summary>
        public static void Main()
        {
            string channelName = null;
            var lcGoLProc = GetProcesses().FirstOrDefault(p => "Lara Croft and the Guardian of Light".Equals(p.GetApplicationName(), StringComparison.OrdinalIgnoreCase));

            if (lcGoLProc != null)
            {
                // Setup Communication Server
                _overlayInterface = new OverlayInterface();
                _overlayServer = RemoteHooking.IpcCreateServer(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, _overlayInterface);
                _overlayInterface.ExceptionOccurred += Event_ExceptionOccured;
                _overlayInterface.MessageArrived += Event_MessageArrived;

                string thisPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                if (!string.IsNullOrWhiteSpace(thisPath))
                {
                    string injectionLibPath = Path.Combine(thisPath, "LCGoLOverlayProcess.dll");

                    try
                    {
                        Console.WriteLine("Attempting to inject into process {0}", lcGoLProc.Id);

                        RemoteHooking.Inject(lcGoLProc.Id, injectionLibPath, injectionLibPath, channelName, lcGoLProc.Id);
                        User32Dll.ShowWindowAsync(new System.Runtime.InteropServices.HandleRef(null, lcGoLProc.MainWindowHandle), 9);
                        User32Dll.SetForegroundWindow(lcGoLProc.MainWindowHandle);
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("There was an error while injecting into target:");
                        Console.ResetColor();
                        Console.WriteLine(e.ToString());
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to stop injection>");
            Console.ResetColor();
            Console.ReadKey();

            _overlayInterface.Disconnect();
            User32Dll.ShowWindowAsync(new System.Runtime.InteropServices.HandleRef(null, lcGoLProc.MainWindowHandle), 9);
            User32Dll.SetForegroundWindow(lcGoLProc.MainWindowHandle);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to exit>");
            Console.ResetColor();
            Console.ReadKey();
        }

        /// <summary>
        /// Gets all the running processes.
        /// </summary>
        /// <returns>A list of all major running processes.</returns>
        private static IEnumerable<Process> GetProcesses()
        {
            return Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle));
        }

        private static void Event_MessageArrived(string message)
        {
            Console.WriteLine(message);
        }

        private static void Event_ExceptionOccured(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("### The target process has reported an error:");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.ToString());
            Console.ResetColor();
        }
    }
}
