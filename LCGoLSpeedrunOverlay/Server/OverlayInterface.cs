using LCGoLOverlayProcess.Game;
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
    public delegate void GameStateChangedEvent(GameState gameState);
    [Serializable]
    public delegate void ValidVSyncSettingsChanged(bool vsyncSettingsAreValid);
    [Serializable]
    public delegate void LevelChanged(GameLevel level);
    [Serializable]
    public delegate void AreaCodeChanged(string areaCode);

    [Serializable]
    public class OverlayInterface : MarshalByRefObject
    {
        public event DisconnectedEvent Disconnected;

        public event MessageArrivedEvent MessageArrived;
        public event ExceptionOccurredEvent ExceptionOccurred;
        public event GameStateChangedEvent GameStateChanged;
        public event ValidVSyncSettingsChanged ValidVSyncSettingsChanged;
        public event LevelChanged LevelChanged;
        public event AreaCodeChanged AreaCodeChanged;

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

        public void ReportGameStateChanged(GameState gameState)
        {
            SafeInvokeReportGameStateChanged(gameState);
        }

        public void ReportValidVSyncSettingsChanged(bool vsyncSettingsAreValid)
        {
            SafeInvokeReportValidVsyncSettingsChanged(vsyncSettingsAreValid);
        }

        public void ReportLevelChanged(GameLevel level)
        {
            SafeInvokeReportLevelChanged(level);
        }

        public void ReportAreaCodeChanged(string areaCode)
        {
            SafeInvokeReportAreaCodeChanged(areaCode);
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

        private void SafeInvokeReportGameStateChanged(GameState gameState)
        {
            if (GameStateChanged is null)
                return;

            GameStateChangedEvent listener = null;
            var dels = GameStateChanged.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (GameStateChangedEvent)del;
                    listener.Invoke(gameState);
                }
                catch
                {
                    GameStateChanged -= listener;
                }
            }
        }

        private void SafeInvokeReportValidVsyncSettingsChanged(bool valid)
        {
            if (ValidVSyncSettingsChanged is null)
                return;

            ValidVSyncSettingsChanged listener = null;
            var dels = ValidVSyncSettingsChanged.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (ValidVSyncSettingsChanged)del;
                    listener.Invoke(valid);
                }
                catch
                {
                    ValidVSyncSettingsChanged -= listener;
                }
            }
        }

        private void SafeInvokeReportLevelChanged(GameLevel level)
        {
            if (LevelChanged is null)
                return;

            LevelChanged listener = null;
            var dels = LevelChanged.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (LevelChanged)del;
                    listener.Invoke(level);
                }
                catch
                {
                    LevelChanged -= listener;
                }
            }
        }

        private void SafeInvokeReportAreaCodeChanged(string areaCode)
        {
            if (AreaCodeChanged is null)
                return;

            AreaCodeChanged listener = null;
            var dels = AreaCodeChanged.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (AreaCodeChanged)del;
                    listener.Invoke(areaCode);
                }
                catch
                {
                    AreaCodeChanged -= listener;
                }
            }
        }
    }
}
