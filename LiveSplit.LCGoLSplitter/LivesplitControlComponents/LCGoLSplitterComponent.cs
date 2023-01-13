using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Windows.Forms;
using System.Xml;
using EasyHook;
using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Server;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using WinOSExtensions.Extensions;

namespace LiveSplit.LCGoLSplitter.LivesplitControlComponents
{
    class LCGoLSplitterComponent : LogicComponent
    {
        public override string ComponentName => "Lara Croft: Guardian of Light - AutoSplitter Injector";
        private TimerModel _timer;

        private Process _lcgolProcess;
        private OverlayInterface _overlayInterface;
        private IpcServerChannel _overlayServer;

        private DateTime _lastSplit;
        private string _currentAreaCode;
        private GameLevel _currentLevel;
        private GameState _previousGameState;

        public LCGoLSplitterComponent(LiveSplitState state)
        {
            _timer = new TimerModel() { CurrentState = state };
            _timer.CurrentState.OnStart += Timer_OnStart;

            _overlayInterface = new OverlayInterface();
            _overlayInterface.MessageArrived += Event_MessageArrived;
            _overlayInterface.ExceptionOccurred += Event_ExceptionOccured;
            _overlayInterface.GameStateChanged += Event_GameStateChanged;
            _overlayInterface.ValidVSyncSettingsChanged += Event_VsyncSettingsChanged;
            _overlayInterface.LevelChanged += Event_LevelChanged;
            _overlayInterface.AreaCodeChanged += Event_AreaCodeChanged;

            _lcgolProcess = null;
            _overlayServer = null;

            _lastSplit = DateTime.MinValue;
            _currentAreaCode = null;
            _currentLevel = GameLevel.None;
            _previousGameState = GameState.Other;
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (_lcgolProcess is null || _lcgolProcess.HasExited)
            {
                _lcgolProcess?.Dispose();
                _lcgolProcess = null;

                _overlayServer = null;
                PerformSetup();
            }
        }

        private void PerformSetup()
        {
            _lcgolProcess = GetProcesses().FirstOrDefault(p => "Lara Croft and the Guardian of Light".Equals(p.GetApplicationName(), StringComparison.OrdinalIgnoreCase));
            if (_lcgolProcess is null)
                return;

            Debug.WriteLine($"Found LCGoL Process: {_lcgolProcess.Id}");

            string channelName = null;
            _overlayServer = RemoteHooking.IpcCreateServer(ref channelName, WellKnownObjectMode.Singleton, _overlayInterface);

            string thisPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!string.IsNullOrWhiteSpace(thisPath))
            {
                string injectionLibraryPath = Path.Combine(thisPath, "LCGoLOverlayProcess.dll");
                Debug.WriteLine($"Looking to inject: {injectionLibraryPath}");

                try
                {
                    Debug.WriteLine($"Attempting to inject into process {_lcgolProcess.Id}.");

                    RemoteHooking.Inject(_lcgolProcess.Id, injectionLibraryPath, injectionLibraryPath, channelName, _lcgolProcess.Id);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"### There was an error while injecting into target: {e}");
                }
            }
        }

        public override void Dispose()
        {
            _lcgolProcess?.Dispose();
            _timer.CurrentState.OnStart -= Timer_OnStart;
            _overlayInterface.MessageArrived -= Event_MessageArrived;
            _overlayInterface.ExceptionOccurred -= Event_ExceptionOccured;
            _overlayInterface.GameStateChanged -= Event_GameStateChanged;
            _overlayInterface.ValidVSyncSettingsChanged -= Event_VsyncSettingsChanged;
            _overlayInterface.LevelChanged -= Event_LevelChanged;
            _overlayInterface.AreaCodeChanged -= Event_AreaCodeChanged;
            _overlayInterface = null;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return document.CreateElement("Settings");
        }

        public override System.Windows.Forms.Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public override void SetSettings(XmlNode settings)
        {
        }

        /// <summary>
        /// Gets all the running processes.
        /// </summary>
        /// <returns>A list of all major running processes.</returns>
        private static IEnumerable<Process> GetProcesses()
        {
            return Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.MainWindowTitle));
        }

        private static void Event_MessageArrived(string message)
        {
            Debug.WriteLine(message);
        }

        private static void Event_ExceptionOccured(Exception e)
        {
            Debug.WriteLine($"### The target process has reported an error: {e}");
        }

        private void Timer_OnStart(object sender, EventArgs e)
        {
            _timer.InitializeGameTime();
        }

        private void Event_GameStateChanged(GameState currentGameState)
        {
            Debug.WriteLine($"Game state changed to {currentGameState}.");

            HandleLoading(currentGameState);
            HandleFinishedLevel(currentGameState);
            HandleStartingFirstLevel(currentGameState);
            HandleRestartingFirstLevel(currentGameState);

            _previousGameState = currentGameState;
        }

        private void HandleLoading(GameState currentGameState)
        {
            if (currentGameState == GameState.InLoadScreen)
            {
                _timer.CurrentState.IsGameTimePaused = true;
            }
            else
            {
                _timer.CurrentState.IsGameTimePaused = false;
            }
        }

        private void HandleFinishedLevel(GameState currentGameState)
        {
            if (currentGameState == GameState.InEndScreen)
            {
                if (!(DateTime.Now - _lastSplit < TimeSpan.FromSeconds(10.0)))
                {
                    _lastSplit = DateTime.Now;
                    _timer.Split();
                }
            }
        }

        private void HandleStartingFirstLevel(GameState currentGameState)
        {
            if (_previousGameState == GameState.InCutscene && string.Equals(_currentAreaCode, "alc_1_it_beginning"))
            {
                _timer.Start();
            }
        }

        private void HandleRestartingFirstLevel(GameState currentGameState)
        {
            if (currentGameState == GameState.InLoadScreen && _timer.CurrentState.CurrentPhase != TimerPhase.NotRunning && string.Equals(_currentAreaCode, "alc_1_it_beginning"))
            {
                _timer.Reset();
            }
        }

        private void Event_VsyncSettingsChanged(bool validVsyncSettings)
        {
            Debug.WriteLine($"Vsync Settings are now {(validVsyncSettings ? string.Empty : "in")}valid.");

            if (_timer.CurrentState.CurrentPhase == TimerPhase.Running)
            {
                MessageBox.Show("Invalid settings detected. VSync must be ON and refresh rate must be set to 60hz. Stopping timer.", "LiveSplit.LaraCroftGoL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _timer.Reset(false);
            }
        }

        private void Event_LevelChanged(GameLevel level)
        {
            Debug.WriteLine($"Level has changed to {level}.");

            _currentLevel = level;
        }

        private void Event_AreaCodeChanged(string areaCode)
        {
            Debug.WriteLine($"AreaCode has changed to {areaCode}.");

            _currentAreaCode = areaCode;
        }

    }
}
