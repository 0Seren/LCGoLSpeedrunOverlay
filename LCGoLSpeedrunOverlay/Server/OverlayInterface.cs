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
    public delegate void IGTPaused(TimeSpan currentTime);
    [Serializable]
    public delegate void IGTUnPaused(TimeSpan previousTime, TimeSpan currentTime);
    [Serializable]
    public delegate void IGTDecreased(TimeSpan previousTime, TimeSpan currentTime);

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
        public event IGTPaused IGTPaused;
        public event IGTUnPaused IGTUnPaused;
        public event IGTDecreased IGTDecreased;

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

        public void ReportIGTPaused(TimeSpan currentTime)
        {
            SafeInvokeReportIGTPaused(currentTime);
        }

        public void ReportIGTUnPaused(TimeSpan previousTime, TimeSpan currentTime)
        {
            SafeInvokeReportIGTUnPaused(previousTime, currentTime);
        }

        public void ReportIGTDecreased(TimeSpan previousTime, TimeSpan currentTime)
        {
            SafeInvokeReportIGTDecreased(previousTime, currentTime);
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

        private void SafeInvokeReportIGTPaused(TimeSpan currentTime)
        {
            if (IGTPaused is null)
                return;

            IGTPaused listener = null;
            var dels = IGTPaused.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (IGTPaused)del;
                    listener.Invoke(currentTime);
                }
                catch
                {
                    IGTPaused -= listener;
                }
            }
        }

        private void SafeInvokeReportIGTUnPaused(TimeSpan previousTime, TimeSpan currentTime)
        {
            if (IGTUnPaused is null)
                return;

            IGTUnPaused listener = null;
            var dels = IGTUnPaused.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (IGTUnPaused)del;
                    listener.Invoke(previousTime, currentTime);
                }
                catch
                {
                    IGTUnPaused -= listener;
                }
            }
        }

        private void SafeInvokeReportIGTDecreased(TimeSpan previousTime, TimeSpan currentTime)
        {
            if (IGTDecreased is null)
                return;

            IGTDecreased listener = null;
            var dels = IGTDecreased.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (IGTDecreased)del;
                    listener.Invoke(previousTime, currentTime);
                }
                catch
                {
                    IGTDecreased -= listener;
                }
            }
        }
    }
}
