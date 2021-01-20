using System;
using System.Collections.Generic;

namespace LCGoLOverlayProcess
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ServerInterface : MarshalByRefObject
    {
        //TODO: This really only becomes a concern once we look at LiveSplit integration. Figure out if the IPC server needs to work both ways. Or what messages need to be sent.
        public void IsInstalled(int clientPID)
        {
            Console.WriteLine("LCGoLSpeedrunOverlay has injected into process {0}.\r\n", clientPID);
        }

        /// <summary>
        /// Output messages to the console.
        /// </summary>
        public void ReportMessages(IEnumerable<string> messages)
        {
            foreach (string message in messages)
            {
                Console.WriteLine(message);
            }
        }

        public void ReportMessage(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Report exception
        /// </summary>
        /// <param name="e"></param>
        public void ReportException(Exception e)
        {
            Console.WriteLine("The target process has reported an error:\r\n" + e);
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
        }
    }
}
