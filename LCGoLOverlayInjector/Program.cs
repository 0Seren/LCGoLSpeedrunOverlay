using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WinOSExtensions.Extensions;

namespace LCGoLInjector
{
    class Program
    {
        /// <summary>
        /// Finds the Lara Croft and the Guardian of Light instance, then injects our DLL into it.
        /// </summary>
        static void Main()
        {
            string channelName = null;
            var lcGoLProc = GetProcesses().FirstOrDefault(p => "Lara Croft and the Guardian of Light".Equals(p.GetApplicationName(), StringComparison.OrdinalIgnoreCase));

            if (lcGoLProc != null)
            {
                // Setup Communication Server
                RemoteHooking.IpcCreateServer<LCGoLOverlayProcess.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);
                string injectionLibPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "LCGoLOverlayProcess.dll");

                try
                {
                    Console.WriteLine("Attempting to inject into process {0}", lcGoLProc.Id);

                    RemoteHooking.Inject(lcGoLProc.Id, injectionLibPath, injectionLibPath, channelName, lcGoLProc.Id);
                } catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There was an error while injecting into target:");
                    Console.ResetColor();
                    Console.WriteLine(e.ToString());
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to exit>");
            Console.ResetColor();
            Console.ReadKey();
        }

        /// <summary>
        /// Gets all the running processes.
        /// </summary>
        /// <returns>A list of all major running processes.</returns>
        public static IEnumerable<Process> GetProcesses()
        {
            return Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle));
        }
    }
}
