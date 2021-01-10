using System;

namespace LCGoLOverlayProcess
{
    public class ServerInterface : MarshalByRefObject
    {
        //TODO: This really only becomes a concern once we look at LiveSplit integration. Figure out if the IPC server needs to work both ways. Or what messages need to be sent.
        public void IsInstalled(int clientPID)
        {
            Console.WriteLine("LCGoLSpeedrunOverlay has injected FileMonitorHook into process {0}.\r\n", clientPID);
        }

        /// <summary>
        /// Output messages to the console.
        /// </summary>
        /// <param name="clientPID"></param>
        /// <param name="fileNames"></param>
        public void ReportMessages(string[] messages)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                Console.WriteLine(messages[i]);
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
            Console.WriteLine("The target process has reported an error:\r\n" + e.ToString());
        }

        /// <summary>
        /// Called to confirm that the IPC channel is still open / host application has not closed
        /// </summary>
        public void Ping()
        {
        }
    }
}
