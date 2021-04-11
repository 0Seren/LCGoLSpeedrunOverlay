using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LiveSplit.ComponentUtil;

namespace LCGoLOverlayProcess.Game
{
    // TODO: GameInfo needs a way to hold previous values (i.e. GameTime.Old vs OldGameTime). Maybe make Interface with current/old values?
    public class GameInfo
    {
        private readonly Process _lcgolProcess;
        private const string _lcgolEXEBase = "lcgol.exe";
        private readonly IEnumerable<FieldInfo> _memoryWatcherFields;

        private readonly MemoryWatcher<byte> _level;
        private readonly StringWatcher _area;
        private readonly MemoryWatcher<byte> _spLoading;
        private readonly MemoryWatcher<byte> _mpLoading;
        private readonly MemoryWatcher<byte> _mpLoading2;
        private readonly MemoryWatcher<int> _gameTime;
        private readonly MemoryWatcher<int> _refreshRate;
        private readonly MemoryWatcher<int> _vSyncPresentationInterval;
        private readonly MemoryWatcher<bool> _isOnEndScreen;
        private readonly MemoryWatcher<byte> _numberOfPlayers;
        private readonly MemoryWatcher<bool> _hasControl;

        public GameData Current { get; private set; }
        public GameData Previous { get; private set; }

        public GameInfo(Process lcgolProcess)
        {
            _lcgolProcess = lcgolProcess;
            Current = new GameData();
            Previous = new GameData();

            // Memory Watchers:
            _numberOfPlayers = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0xD7F8EC, 0x10));
            _level = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0x65C548));
            _area = new StringWatcher(new DeepPointer(_lcgolEXEBase, 0xCA8E1C), 1000);
            _hasControl = new MemoryWatcher<bool>(new DeepPointer(_lcgolEXEBase, 0x64F3EE));
            _isOnEndScreen = new MemoryWatcher<bool>(new DeepPointer(_lcgolEXEBase, 0x7C0DD0));
            _gameTime = new MemoryWatcher<int>(new DeepPointer(_lcgolEXEBase, 0xCA8EE4));
            _spLoading = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0xA84CAC));
            _mpLoading = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0xCEB5F8));
            _mpLoading2 = new MemoryWatcher<byte>(new DeepPointer(_lcgolEXEBase, 0xCA8D0B));
            _refreshRate = new MemoryWatcher<int>(new DeepPointer(_lcgolEXEBase, 0x0884554, 0x228));
            _vSyncPresentationInterval = new MemoryWatcher<int>(new DeepPointer(_lcgolEXEBase, 0x0884554, 0x22C));

            // Find Memory Watchers:
            var fields = typeof(GameInfo).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            _memoryWatcherFields = fields.Where(field => typeof(MemoryWatcher).IsAssignableFrom(field.FieldType) && !field.FieldType.IsInterface && !field.FieldType.IsAbstract).ToList();
        }

        public void Update()
        {
            Previous = Current;
            Current = new GameData();

            UpdateMemoryWatchers();

            Current.GameTime = TimeSpan.FromMilliseconds(_gameTime.Current);
            Current.AreaCode = _area.Current;
            Current.Level = _level.Current <= 13 ? (GameLevel)_level.Current : GameLevel.Unknown;
            Current.NumberOfPlayers = _numberOfPlayers.Current;
            Current.HasControl = _hasControl.Current;
            Current.GameState = UpdateGameState();
            Current.ValidVsyncSettings = UpdateValidVSyncSettings();
        }

        // TODO: Find all the game states
        private GameState UpdateGameState()
        {
            if (_isOnEndScreen.Current)
            {
                return GameState.InEndScreen;
            }
            if (Previous.GameState == GameState.InLoadScreen)
            {
                bool inLoad = !((_numberOfPlayers.Current == 1 && _spLoading.Current != 1 && _spLoading.Old == 1 && !_isOnEndScreen.Current)
                              || (_numberOfPlayers.Current > 1 && _mpLoading.Current == 1 && _mpLoading.Old == 3));

                return inLoad ? GameState.InLoadScreen : GameState.Other;
            }
            else
            {
                bool inLoad = (_numberOfPlayers.Current == 1 && _spLoading.Current == 1 && _spLoading.Old != 1 && !_isOnEndScreen.Current)
                           || (_numberOfPlayers.Current > 1 && _mpLoading.Current == 2 && _mpLoading.Old == 7)   // new game
                           || (_numberOfPlayers.Current > 1 && _mpLoading2.Current == 1 && _mpLoading2.Old == 0)  // death
                           || (_numberOfPlayers.Current > 1 && _mpLoading.Current == 2 && _mpLoading.Old == 1);

                return inLoad ? GameState.InLoadScreen : GameState.Other;
            }
        }

        private bool UpdateValidVSyncSettings()
        {
            // Valid settings are VSync ON, RefreshRate = 59-60
            return _vSyncPresentationInterval.Current == 0x00000001
                   && (_refreshRate.Current == 59 || _refreshRate.Current == 60);
        }

        private void UpdateMemoryWatchers()
        {
            foreach (var memoryWatcherField in _memoryWatcherFields)
            {
                if (memoryWatcherField.GetValue(this) is MemoryWatcher mw)
                {
                    mw.Update(_lcgolProcess);
                }
            }
        }
    }
}
