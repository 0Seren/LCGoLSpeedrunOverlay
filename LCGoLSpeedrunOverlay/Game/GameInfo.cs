using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using LiveSplit.ComponentUtil;

namespace LCGoLOverlayProcess.Game
{
    public class GameInfo
    {
        private readonly Process _lcgolProcess;
        private readonly ConcurrentDictionary<string, GameLevel> _levelNameLookup;

        private const string _lcgolEXEBase = "lcgol.exe";

        private readonly StringWatcher _checkpoint;
        private readonly MemoryWatcher<byte> _spLoading;
        private readonly MemoryWatcher<byte> _mpLoading;
        private readonly MemoryWatcher<byte> _mpLoading2;
        private readonly MemoryWatcher<uint> _gameTime;
        private readonly MemoryWatcher<int> _refreshRate;
        private readonly MemoryWatcher<int> _vSyncPresentationInterval;

        public readonly MemoryWatcher<bool> IsOnEndScreen;
        public readonly MemoryWatcher<byte> NumberOfPlayers;

        public GameLevel Level
        {
            get => _levelNameLookup.TryGetValue(_checkpoint.Current, out var level) ? level : GameLevel.Unknown;
        }

        public GameState GameState
        {
            get; private set;
        }

        public GameState OldGameState
        {
            get; private set;
        }

        public TimeSpan GameTime
        {
            get => TimeSpan.FromMilliseconds(_gameTime.Current);
        }

        public TimeSpan OldGameTime
        {
            get => TimeSpan.FromMilliseconds(_gameTime.Old);
        }

        public bool ValidVsyncSettings
        {
            get; private set;
        }

        public bool OldValidVsyncSettings
        {
            get; private set;
        }

        public GameInfo(Process lcgolProcess)
        {
            _lcgolProcess = lcgolProcess;
            // TODO: Populate LevelNameLookup with more checkpoints or find a pattern in checkpoint names. These are only the start level checkpoints.
            _levelNameLookup = new ConcurrentDictionary<string, GameLevel>(StringComparer.OrdinalIgnoreCase)
            {
                ["alc_1_it_beginning"] = GameLevel.TempleOfLight,
                ["alc_2_cy_east"] = GameLevel.TempleGrounds,
                ["alc_3_st_entrance"] = GameLevel.SpiderTomb,
                ["alc_bossfight_trex"] = GameLevel.TheSummoning,
                ["alc_4_lc_main_hall"] = GameLevel.ForgottenGate,
                ["alc_6_ld_start_zone"] = GameLevel.ToxicSwamp,
                ["alc_5_lt_hall01"] = GameLevel.FloodedPassage,
                ["alc_5_lt_arrow_shrine"] = GameLevel.TheJawsOfDeath,
                ["alc_10_bridge_maze"] = GameLevel.TwistingBridge,
                ["alc_11_lt_lava_tomb_main_chamber_a"] = GameLevel.FieryDepths,
                ["alc_bossfight_lava_trex"] = GameLevel.BellyOfTheBeast,
                ["alc_13_lc_main_hall"] = GameLevel.StrongholdPassage,
                ["alc_14_gf_royal_road"] = GameLevel.TheMirrorsWake,
                ["alc_bossfight_xolotl_final"] = GameLevel.XolotlsStronghold,
            };

            // Memory Watchers:
            DeepPointer isOnEndScreen = new DeepPointer(_lcgolEXEBase, 0x7C0DD0); //bool
            DeepPointer numPlayers = new DeepPointer(_lcgolEXEBase, 0xD7F8EC, 0x10); //byte

            IsOnEndScreen = new MemoryWatcher<bool>(isOnEndScreen);
            NumberOfPlayers = new MemoryWatcher<byte>(numPlayers);

            DeepPointer currentMap = new DeepPointer(_lcgolEXEBase, 0xCA8E1C); //string
            DeepPointer gameTime = new DeepPointer(_lcgolEXEBase, 0xCA8EE4); //uint32
            DeepPointer spLoading = new DeepPointer(_lcgolEXEBase, 0xA84CAC); //byte
            DeepPointer mpLoading = new DeepPointer(_lcgolEXEBase, 0xCEB5F8); //byte
            DeepPointer mpLoading2 = new DeepPointer(_lcgolEXEBase, 0xCA8D0B); //byte
            DeepPointer refreshRate = new DeepPointer(_lcgolEXEBase, 0x0884554, 0x228); //int
            DeepPointer vSyncPresentationInterval = new DeepPointer(_lcgolEXEBase, 0x0884554, 0x22C); //int

            _checkpoint = new StringWatcher(currentMap, 1000);
            _gameTime = new MemoryWatcher<uint>(gameTime);
            _spLoading = new MemoryWatcher<byte>(spLoading);
            _mpLoading = new MemoryWatcher<byte>(mpLoading);
            _mpLoading2 = new MemoryWatcher<byte>(mpLoading2);
            _refreshRate = new MemoryWatcher<int>(refreshRate);
            _vSyncPresentationInterval = new MemoryWatcher<int>(vSyncPresentationInterval);

            GameState = GameState.Other;
        }

        public void Update()
        {
            _checkpoint.Update(_lcgolProcess);
            IsOnEndScreen.Update(_lcgolProcess);
            NumberOfPlayers.Update(_lcgolProcess);

            _gameTime.Update(_lcgolProcess);
            _refreshRate.Update(_lcgolProcess);
            _vSyncPresentationInterval.Update(_lcgolProcess);
            _spLoading.Update(_lcgolProcess);
            _mpLoading.Update(_lcgolProcess);
            _mpLoading2.Update(_lcgolProcess);

            UpdateValidVSyncSettings();
            UpdateGameState();
        }

        // TODO: Find all the game states
        private void UpdateGameState()
        {
            OldGameState = GameState;

            if (IsOnEndScreen.Current)
            {
                GameState = GameState.InEndScreen;
            } else if (OldGameState == GameState.InLoadScreen)
            {
                bool inLoad = !((NumberOfPlayers.Current == 1 && _spLoading.Current != 1 && _spLoading.Old == 1 && !IsOnEndScreen.Current)
                              || (NumberOfPlayers.Current > 1 && _mpLoading.Current == 1 && _mpLoading.Old == 3));

                GameState = inLoad ? GameState.InLoadScreen : GameState.Other;
            } else
            {
                bool inLoad = (NumberOfPlayers.Current == 1 && _spLoading.Current == 1  && _spLoading.Old != 1 && !IsOnEndScreen.Current)
                           || (NumberOfPlayers.Current > 1  && _mpLoading.Current == 2  && _mpLoading.Old == 7)   // new game
                           || (NumberOfPlayers.Current > 1  && _mpLoading2.Current == 1 && _mpLoading2.Old == 0)  // death
                           || (NumberOfPlayers.Current > 1  && _mpLoading.Current == 2  && _mpLoading.Old == 1);

                GameState = inLoad ? GameState.InLoadScreen : GameState.Other;
            }
        }

        private void UpdateValidVSyncSettings()
        {
            OldValidVsyncSettings = ValidVsyncSettings;
            ValidVsyncSettings = true;

            // Valid settings are VSync ON, RefreshRate = 59-60
            if ((_vSyncPresentationInterval.Current != 0x00000001 || (_refreshRate.Current != 59 && _refreshRate.Current != 60))
                // && GameTime.Current > 0 // avoid false detection on game startup and zeroed memory on exit
                // && RefreshRate.Current != 0
                )
            {
                ValidVsyncSettings = false;
            }
        }
    }
}
