using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LCGoLOverlayProcess.Server;
using WinOSExtensions.Extensions;
using WinOSExtensions.DLLWrappers;
using System.Threading;
using LCGoLOverlayProcess.Game;
using System.Reflection;
using LCGoLOverlayProcess;

namespace LCGoLInjector
{
    public static class Program
    {
        /// <summary>
        /// Finds the Lara Croft and the Guardian of Light instance, then injects our DLL into it.
        /// </summary>
        public static void Main()
        {
            Console.WriteLine("Looking for Lara Croft and the Guardian of Light process...");

            Process lcGoLProc = WaitForAndGetProcess();

            var overlayInterface = new OverlayInterface();
            overlayInterface.ExceptionOccurred += Event_ExceptionOccured;
            overlayInterface.MessageArrived += Event_MessageArrived;
            overlayInterface.GameStateChanged += Event_GameStateChanged;
            overlayInterface.ValidVSyncSettingsChanged += Event_VsyncSettingsChanged;
            overlayInterface.LevelChanged += Event_LevelChanged;
            overlayInterface.AreaCodeChanged += Event_AreaCodeChanged;

            string channelName = null;
            var overlayServer = RemoteHooking.IpcCreateServer(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton, overlayInterface);

            string injectionLibPath = Assembly.GetAssembly(typeof(InjectionEntryPoint)).Location;
            Console.WriteLine($"Looking to inject: {injectionLibPath}");

            try
            {
                Console.WriteLine("Attempting to inject into process {0}", lcGoLProc.Id);

                RemoteHooking.Inject(lcGoLProc.Id, injectionLibPath, injectionLibPath, channelName, lcGoLProc.Id);
                SetProcessToForeground(lcGoLProc);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an error while injecting into target:");
                Console.ResetColor();
                Console.WriteLine(e.ToString());
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to stop injection>");
            Console.ResetColor();
            Console.ReadKey();

            overlayInterface.Disconnect();
            SetProcessToForeground(lcGoLProc);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to exit>");
            Console.ResetColor();
            Console.ReadKey();
            Console.WriteLine(overlayServer.ChannelName);
        }

        private static void SetProcessToForeground(Process p)
        {
            User32Dll.ShowWindowAsync(new System.Runtime.InteropServices.HandleRef(null, p.MainWindowHandle), 9);
            User32Dll.SetForegroundWindow(p.MainWindowHandle);
        }

        private static Process WaitForAndGetProcess()
        {
            Process lcGoLProc = null;
            while (lcGoLProc is null)
            {
                lcGoLProc = GetProcesses().FirstOrDefault(p => "Lara Croft and the Guardian of Light".Equals(p.GetApplicationName(), StringComparison.OrdinalIgnoreCase));
                Thread.Sleep(150);
            }
            return lcGoLProc;
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

        private static void Event_GameStateChanged(GameState currentGameState)
        {
            Console.WriteLine($"Game state changed to {currentGameState}.");
        }

        private static void Event_VsyncSettingsChanged(bool validVsyncSettings)
        {
            Console.WriteLine($"Vsync Settings are now {(validVsyncSettings ? string.Empty : "in")}valid.");
        }

        private static void Event_LevelChanged(GameLevel level)
        {
            Console.WriteLine($"Level has changed to {level}.");
        }

        private static void Event_AreaCodeChanged(string areaCode)
        {
            Console.WriteLine($"AreaCode has changed to {areaCode}.");
        }
    }
}
