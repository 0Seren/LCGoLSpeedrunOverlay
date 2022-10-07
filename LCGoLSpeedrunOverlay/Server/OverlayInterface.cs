using System;

namespace LCGoLOverlayProcess.Server
{
    [Serializable]
    public delegate void DisconnectedEvent();

    [Serializable]
    public delegate void MessageArrivedEvent(string message);
    [Serializable]
    public delegate void ExceptionOccurredEvent(Exception e);

    [Serializable]
    public class OverlayInterface : MarshalByRefObject
    {
        public event DisconnectedEvent Disconnected;

        public event MessageArrivedEvent MessageArrived;
        public event ExceptionOccurredEvent ExceptionOccurred;

        public void Disconnect()
        {
            SafeInvokeDisconnected();
        }

        public DateTime Ping()
        {
            return DateTime.Now;
        }

        public void IsInstalled(int processId)
        {
            SendMessage($"Injected DLL has been installed into {processId}.");
        }

        public void SendMessage(string message)
        {
            SafeInvokeReportMessage(message);
        }

        public void ReportException(Exception e)
        {
            SafeInvokeReportException(e);
        }

        private void SafeInvokeDisconnected()
        {
            if (Disconnected == null)
                return;         //No Listeners

            DisconnectedEvent listener = null;
            var dels = Disconnected.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (DisconnectedEvent)del;
                    listener.Invoke();
                }
                catch (Exception)
                {
                    //Could not reach the destination, so remove it
                    //from the list
                    Disconnected -= listener;
                }
            }
        }

        private void SafeInvokeReportMessage(string eventArgs)
        {
            if (MessageArrived is null)
            {
                return;
            }

            MessageArrivedEvent listener = null;
            var dels = MessageArrived.GetInvocationList();

            foreach(Delegate del in dels)
            {
                try
                {
                    listener = (MessageArrivedEvent)del;
                    listener.Invoke(eventArgs);
                } catch
                {
                    MessageArrived -= listener;
                }
            }
        }

        private void SafeInvokeReportException(Exception eventArgs)
        {
            if (ExceptionOccurred is null)
            {
                return;
            }

            ExceptionOccurredEvent listener = null;
            var dels = ExceptionOccurred.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (ExceptionOccurredEvent)del;
                    listener.Invoke(eventArgs);
                }
                catch
                {
                    ExceptionOccurred -= listener;
                }
            }
        }
    }
}
