using System;
using System.Diagnostics;
using System.Linq;
using LCGoLOverlayProcess.Game;

namespace LiveSplit.LCGoL
{
	internal class GameMemory
	{
		public delegate void LevelFinishedEventHandler(object sender, string level);

		private Process _process;

		private GameInfo _data;

		private readonly PersonalBestIldb _pbDb;

		public event LevelFinishedEventHandler OnLevelFinished;

		public event EventHandler OnFirstLevelStarted;

		public event EventHandler OnFirstLevelLoading;

		public event EventHandler OnLoadStart;

		public event EventHandler OnLoadFinish;

		public event EventHandler OnInvalidSettingsDetected;

		public event PersonalBestIldb.NewPersonalBestEventArgs OnNewIlPersonalBest;

		public GameMemory()
		{
			_pbDb = new PersonalBestIldb();
			_pbDb.OnNewIlPersonalBest += PBDb_OnNewILPersonalBest;
		}

		public void Update()
        {
            if (!EnsureGameConnection())
                return;

			_data.Update();
			_pbDb.Update(_process);

			InvokeEvents();
		}

        private void InvokeEvents()
		{
            if (_data.IsOnEndScreen.Changed && _data.IsOnEndScreen.Current)
            {
                OnLevelFinished?.Invoke(this, _data.AreaCode.Current);
            }
            else if (_data.State.Changed && _data.State.Current == GameState.InLoadScreen)
            {
                OnLoadStart?.Invoke(this, EventArgs.Empty);
            }
            else if (_data.State.Changed && _data.State.Old == GameState.InLoadScreen)
            {
                OnLoadFinish?.Invoke(this, EventArgs.Empty);
            }

            if (_data.GameTime.Current == TimeSpan.Zero && _data.GameTime.Old != TimeSpan.Zero && _data.AreaCode.Current == "alc_1_it_beginning")
            {
                OnFirstLevelLoading?.Invoke(this, EventArgs.Empty);
            }
            else if (_data.GameTime.Current != TimeSpan.Zero && _data.GameTime.Old == TimeSpan.Zero && _data.AreaCode.Current == "alc_1_it_beginning")
            {
                OnFirstLevelStarted?.Invoke(this, EventArgs.Empty);
            }

            if (!_data.ValidVSyncSettings.Current && _data.GameTime.Current != TimeSpan.Zero && _data.RefreshRate.Current != 0)
            {
                OnInvalidSettingsDetected?.Invoke(this, EventArgs.Empty);
            }
		}

        private bool EnsureGameConnection()
        {
            if (_process == null || _process.HasExited)
            {
                _process = null;
                _data = null;
                if (TryGetGameProcess())
				{
					_data = new GameInfo(_process);
				}
            }

            return !(_process is null || _data is null);
        }

		private bool TryGetGameProcess()
		{
			var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.ToLower() == "lcgol" && !p.HasExited);
			if (process == null)
			{
				return false;
			}

			_process = process;
			return true;
		}

		private void PBDb_OnNewILPersonalBest(object sender, string level, TimeSpan time, TimeSpan oldTime)
		{
			OnNewIlPersonalBest?.Invoke(this, level, time, oldTime);
		}
	}
}
