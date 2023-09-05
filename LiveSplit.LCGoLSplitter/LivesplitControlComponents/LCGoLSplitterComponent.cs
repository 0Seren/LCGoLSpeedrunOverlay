using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using EasyHook;
using LCGoLOverlayProcess;
using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Server;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using WinOSExtensions.Extensions;

namespace LiveSplit.LCGoLSplitter.LiveSplitControlComponents
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

        private Timer _updateTimer;
        private object _updateTimerLock = new object();
        private bool _updateTimerRunning = false;

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

            _updateTimer = new Timer
            {
                Interval = 150,
                Enabled = true
            };
            _updateTimer.Tick += (o, a) => Task.Factory.StartNew(() => PerformUpdate(o, a));
        }

        private void PerformUpdate(object source, EventArgs e)
        {
            lock (_updateTimerLock)
            {
                if (_updateTimerRunning)
                {
                    return;
                }
                _updateTimerRunning = true;
            }

            if (_lcgolProcess is null || _lcgolProcess.HasExited)
            {
                _lcgolProcess?.Dispose();
                _lcgolProcess = null;

                _overlayServer = null;
                PerformSetup();
            }

            lock(_updateTimerLock)
            {
                _updateTimerRunning = false;
            }
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
        }

        private void PerformSetup()
        {
            _lcgolProcess = GetProcesses().FirstOrDefault(p => "Lara Croft and the Guardian of Light".Equals(p.GetApplicationName(), StringComparison.OrdinalIgnoreCase));
            if (_lcgolProcess is null)
                return;

            Debug.WriteLine($"LiveSplit.LCGoLSplitter: Found LCGoL Process: {_lcgolProcess.Id}");
            Debug.WriteLine($"LiveSplit.LCGoLSplitter: Creating IpcCreateServer...");
            string channelName = null;
            _overlayServer = RemoteHooking.IpcCreateServer(ref channelName, WellKnownObjectMode.Singleton, _overlayInterface);

            string injectionLibraryPath1 = Assembly.GetAssembly(typeof(InjectionEntryPoint)).Location;

            string injectionLibraryPath2 = Path.Combine(Directory.GetParent(injectionLibraryPath1).FullName, Path.GetFileName(injectionLibraryPath1));

            //string injectionLibraryPath = Assembly.GetAssembly(typeof(InjectionEntryPoint)).Location;
            Debug.WriteLine($"LiveSplit.LCGoLSplitter: Looking to inject: {injectionLibraryPath2}");

            try
            {
                Debug.WriteLine($"LiveSplit.LCGoLSplitter: Attempting to inject into process {_lcgolProcess.Id}.");
                RemoteHooking.Inject(_lcgolProcess.Id, injectionLibraryPath2, injectionLibraryPath2, channelName, _lcgolProcess.Id);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"### LiveSplit.LCGoLSplitter: There was an error while injecting into target: {e}\n\t{e.Data}");
            }
        }

        public override void Dispose()
        {
            _lcgolProcess?.Dispose();
            _updateTimer?.Dispose();
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

        public override Control GetSettingsControl(LayoutMode mode)
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
            Debug.WriteLine($"LiveSplit.LCGoLSplitter: {message}");
        }

        private static void Event_ExceptionOccured(Exception e)
        {
            Debug.WriteLine($"### LiveSplit.LCGoLSplitter: The target process has reported an error: {e}");
        }

        private void Timer_OnStart(object sender, EventArgs e)
        {
            _timer.InitializeGameTime();
        }

        private void Event_GameStateChanged(GameState currentGameState)
        {
            Debug.WriteLine($"LiveSplit.LCGoLSplitter: Game state changed to {currentGameState}.");

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
            Debug.WriteLine($"LiveSplit.LCGoLSplitter: Vsync Settings are now {(validVsyncSettings ? string.Empty : "in")}valid.");

            if (_timer.CurrentState.CurrentPhase == TimerPhase.Running)
            {
                MessageBox.Show("Invalid settings detected. VSync must be ON and refresh rate must be set to 60hz. Stopping timer.", "LiveSplit.LaraCroftGoL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                _timer.Reset(false);
            }
        }

        private void Event_LevelChanged(GameLevel level)
        {
            Debug.WriteLine($"LiveSplit.LCGoLSplitter: Level has changed to {level}.");

            _currentLevel = level;
        }

        private void Event_AreaCodeChanged(string areaCode)
        {
            Debug.WriteLine($"LiveSplit.LCGoLSplitter: AreaCode has changed to {areaCode}.");

            _currentAreaCode = areaCode;
        }

    }
}
