using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LCGoLOverlayProcess.Server;
using LiveSplit.ComponentUtil;

namespace LCGoLOverlayProcess.Game
{
    // TODO: GameInfo needs a way to hold previous values (i.e. GameTime.Old vs OldGameTime). Maybe make Interface with current/old values?
    public class GameInfo : MemoryWatcherList
    {
        private const string _lcgolEXEBase = "lcgol.exe";
        private readonly Process _lcgolProcess;
        private readonly IEnumerable<IInformationHolder> _informationHolders;

        public readonly StringWatcher AreaCode = new StringWatcher(new DeepPointer(_lcgolEXEBase, 0xCA8E1C), 1000);
        public readonly MemoryWatcher<byte> SpLoading = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0xA84CAC));
        public readonly MemoryWatcher<byte> MpLoading = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0xCEB5F8));
        public readonly MemoryWatcher<byte> MpLoading2 = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0xCA8D0B));
        public readonly MemoryWatcher<int> RefreshRate = new MemoryWatcher<int>(new DeepPointer(_lcgolEXEBase, 0x0884554, 0x228));
        public readonly MemoryWatcher<int> VSyncPresentationInterval = new MemoryWatcher<int>(new DeepPointer(_lcgolEXEBase, 0x0884554, 0x22C));
        public readonly MemoryWatcher<bool> IsOnEndScreen = new MemoryWatcher<bool>(new DeepPointer(_lcgolEXEBase, 0x7C0DD0));
        public readonly MemoryWatcher<byte> NumberOfPlayers = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0xD7F8EC, 0x10));
        public readonly MemoryWatcher<bool> HasControl = new MemoryWatcher<bool>(new DeepPointer(_lcgolEXEBase, 0x64F3EE));
        private readonly MemoryWatcher<int> _gameTime = new MemoryWatcher<int>(new DeepPointer(_lcgolEXEBase, 0xCA8EE4));
        private readonly MemoryWatcher<byte> LevelId = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0x65C548));

        public readonly InformationHolder<bool> ValidVSyncSettings;
        public readonly InformationHolder<GameState> State;
        public readonly InformationHolder<TimeSpan> GameTime;
        public readonly InformationHolder<GameLevel> Level;

        private readonly OverlayInterface _overlayInterface;

        public GameInfo(Process lcgolProcess, OverlayInterface overlayInterface)
        {
            _lcgolProcess = lcgolProcess;
            _overlayInterface = overlayInterface;

            var fields = typeof(GameInfo).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var memoryWatchers = fields.Where(field => typeof(MemoryWatcher).IsAssignableFrom(field.FieldType) && !field.FieldType.IsInterface && !field.FieldType.IsAbstract).Select(fi => fi.GetValue(this) as MemoryWatcher);

            AddRange(memoryWatchers);

            ValidVSyncSettings = new InformationHolder<bool>(CurrentValidVSyncSettings);
            Level = new InformationHolder<GameLevel>(() => (GameLevel)LevelId.Current, Game.GameLevel.None, Game.GameLevel.None);
            GameTime = new InformationHolder<TimeSpan>(() => TimeSpan.FromMilliseconds(_gameTime.Current));
            State = new InformationHolder<GameState>(CurrentGameState, GameState.Other, GameState.Other);

            var infoHolders = fields.Where(field => typeof(IInformationHolder).IsAssignableFrom(field.FieldType) && !field.FieldType.IsInterface && !field.FieldType.IsAbstract).Select(fi => fi.GetValue(this) as IInformationHolder);
            _informationHolders = new List<IInformationHolder>(infoHolders);

            Update();

            _overlayInterface.ReportGameStateChanged(State.Current);
            _overlayInterface.ReportValidVSyncSettingsChanged(ValidVSyncSettings.Current);
            _overlayInterface.ReportLevelChanged(Level.Current);
            _overlayInterface.ReportAreaCodeChanged(AreaCode.Current);
        }

        public void UpdateAndReportChanges()
        {
            Update();

            if (State.Changed)
            {
                _overlayInterface.ReportGameStateChanged(State.Current);
            }

            if (ValidVSyncSettings.Changed)
            {
                _overlayInterface.ReportValidVSyncSettingsChanged(ValidVSyncSettings.Current);
            }

            if (Level.Changed)
            {
                _overlayInterface.ReportLevelChanged(Level.Current);
            }

            if (AreaCode.Changed)
            {
                _overlayInterface.ReportAreaCodeChanged(AreaCode.Current);
            }
        }

        public GameInfoSnapShot GetGameInfoSnapShot()
        {
            return new GameInfoSnapShot(this);
        }

        private void Update()
        {
            UpdateAll(_lcgolProcess);

            foreach (var informationHolder in _informationHolders)
            {
                informationHolder.Update();
            }
        }

        // TODO: Find all the game states
        private GameState CurrentGameState()
        {
            if (IsOnEndScreen.Current)
            {
                return GameState.InEndScreen;
            }

            if (State.Old == GameState.InLoadScreen
                && (   !((NumberOfPlayers.Current == 1 && SpLoading.Current != 1 && SpLoading.Old == 1 && !IsOnEndScreen.Current)
                     || (NumberOfPlayers.Current > 1 && MpLoading.Current == 1 && MpLoading.Old == 3))
                   )
               )
            {
                return GameState.InLoadScreen;
            }
            else if ((NumberOfPlayers.Current == 1 && SpLoading.Current == 1 && SpLoading.Old != 1 && !IsOnEndScreen.Current)
                           || (NumberOfPlayers.Current > 1 && MpLoading.Current == 2 && MpLoading.Old == 7)   // new game
                           || (NumberOfPlayers.Current > 1 && MpLoading2.Current == 1 && MpLoading2.Old == 0)  // death
                           || (NumberOfPlayers.Current > 1 && MpLoading.Current == 2 && MpLoading.Old == 1))
            {
                return GameState.InLoadScreen;
            }

            if (!HasControl.Current)
            {
                return GameState.InCutscene;
            }

            return GameState.Other;
        }

        private bool CurrentValidVSyncSettings()
        {
            // Valid settings are VSync ON, RefreshRate = 59-60
            return VSyncPresentationInterval.Current == 0x00000001
                   && (RefreshRate.Current == 59 || RefreshRate.Current == 60);
        }
    }
}
