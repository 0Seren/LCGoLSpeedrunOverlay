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
        private readonly MemoryWatcher<int> _menuIndicator = new MemoryWatcher<int>(new DeepPointer(_lcgolEXEBase, 0x77B1C8));

        public readonly InformationHolder<bool> ValidVSyncSettings;
        public readonly InformationHolder<GameState> State;
        public readonly InformationHolder<TimeSpan> GameTime;
        public readonly InformationHolder<GameLevel> Level;

        private readonly OverlayInterface _overlayInterface;

        private bool _igtHasBeenPaused = true;
        private bool _igtHasBeenPaused2 = true;

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

            // For some reason, there appear to be multiple frames with the same game time. I wonder if this is a triple buffer thing...
            if (!_igtHasBeenPaused && !GameTime.Changed && State.Current != GameState.InLoadScreen)
            {
                _igtHasBeenPaused = true;
            }
            else if (_igtHasBeenPaused && !_igtHasBeenPaused2 && !GameTime.Changed)
            {
                _igtHasBeenPaused2 = true;
                _overlayInterface.ReportIGTPaused(GameTime.Current);
            }
            else if (_igtHasBeenPaused && GameTime.Current > GameTime.Old)
            {
                if (_igtHasBeenPaused2)
                {
                    _overlayInterface.ReportIGTUnPaused(GameTime.Old, GameTime.Current);
                }
                _igtHasBeenPaused = false;
                _igtHasBeenPaused2 = false;
            }

            if (GameTime.Current < GameTime.Old)
            {
                _overlayInterface.ReportIGTDecreased(GameTime.Old, GameTime.Current);
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

            if (string.IsNullOrWhiteSpace(AreaCode.Current))
            {
                return GameState.InMainMenu;
            }

            if (_menuIndicator.Current == 14) // This is the thing I am least sure about here.
                                              // It'd be nice to know what the values of this flag actually mean...
            {
                return GameState.InLevel;
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
