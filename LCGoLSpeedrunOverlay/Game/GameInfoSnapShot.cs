using System;

namespace LCGoLOverlayProcess.Game
{
    //TODO: Use A Custom Source Generator to build GameInfoSnapShot dynamically.
    public class GameInfoSnapShot
    {
        public GameInfoSnapShot(GameInfo gameInfo)
        {
            Level = InformationHolder.FromMemoryWatcher(gameInfo.Level);
            AreaCode = InformationHolder.FromMemoryWatcher(gameInfo.AreaCode);
            SpLoading = InformationHolder.FromMemoryWatcher(gameInfo.SpLoading);
            MpLoading = InformationHolder.FromMemoryWatcher(gameInfo.MpLoading);
            MpLoading2 = InformationHolder.FromMemoryWatcher(gameInfo.MpLoading2);
            RefreshRate = InformationHolder.FromMemoryWatcher(gameInfo.RefreshRate);
            VSyncPresentationInterval = InformationHolder.FromMemoryWatcher(gameInfo.VSyncPresentationInterval);
            IsOnEndScreen = InformationHolder.FromMemoryWatcher(gameInfo.IsOnEndScreen);
            NumberOfPlayers = InformationHolder.FromMemoryWatcher(gameInfo.NumberOfPlayers);
            HasControl = InformationHolder.FromMemoryWatcher(gameInfo.HasControl);
            ValidVSyncSettings = gameInfo.ValidVSyncSettings.Clone();
            State = gameInfo.State.Clone();
            GameTime = gameInfo.GameTime.Clone();
        }

        public readonly InformationHolder<byte> Level;
        public readonly InformationHolder<string> AreaCode;
        public readonly InformationHolder<byte> SpLoading;
        public readonly InformationHolder<byte> MpLoading;
        public readonly InformationHolder<byte> MpLoading2;
        public readonly InformationHolder<int> RefreshRate;
        public readonly InformationHolder<int> VSyncPresentationInterval;
        public readonly InformationHolder<bool> IsOnEndScreen;
        public readonly InformationHolder<byte> NumberOfPlayers;
        public readonly InformationHolder<bool> HasControl;
        public readonly InformationHolder<bool> ValidVSyncSettings;
        public readonly InformationHolder<GameState> State;
        public readonly InformationHolder<TimeSpan> GameTime;
    }
}
