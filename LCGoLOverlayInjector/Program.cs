using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            var lcGoLProc = GetProcesses().FirstOrDefault(p => "Lara Croft and the Guardian of Light".Equals(GetApplicationName(p), StringComparison.OrdinalIgnoreCase));

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

        /// <summary>
        /// Gets the Application Name for a Process.
        /// </summary>
        /// <param name="process">The process to find the Application Name for.</param>
        /// <returns>The Application Name of the Process.</returns>
        // TODO: Change to extension method
        public static string GetApplicationName(Process process)
        {
            string applicationName = process?.MainWindowTitle?.Split('-').Last().Trim();

            if (string.IsNullOrWhiteSpace(applicationName))
            {
                applicationName = process?.MainWindowTitle?.Trim();

                if (string.IsNullOrWhiteSpace(applicationName))
                {
                    applicationName = process?.ProcessName?.Trim();
                }
            }

            return applicationName;
        }
    }
}
