using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;

namespace LiveSplit.LCGoL
{
    internal class GoLSplitComponent : LogicComponent
    {
		private readonly TimerModel _timer;

		private readonly GameMemory _gameMemory;

		private readonly LogForm _logForm;

		private DateTime _lastSplit;

		private readonly Timer _updateTimer;

		public override string ComponentName => "Lara Croft: GoL";

		public GoLSplitComponent(LiveSplitState state)
		{
            _logForm = new LogForm();
            _lastSplit = DateTime.MinValue;
            _gameMemory = new GameMemory();

			_timer = new TimerModel
            {
                CurrentState = state,
            };
			_timer.CurrentState.OnStart += Timer_OnStart;

			_gameMemory.OnFirstLevelLoading += GameMemory_OnFirstLevelLoading;
			_gameMemory.OnFirstLevelStarted += GameMemory_OnFirstLevelStarted;
			_gameMemory.OnLevelFinished += GameMemory_OnLevelFinished;
			_gameMemory.OnLoadStart += GameMemory_OnLoadStart;
			_gameMemory.OnLoadFinish += GameMemory_OnLoadFinish;
			_gameMemory.OnInvalidSettingsDetected += GameMemory_OnInvalidSettingsDetected;
			_gameMemory.OnNewIlPersonalBest += GameMemory_OnNewILPersonalBest;

            ContextMenuControls = new Dictionary<string, Action>
            {
                ["Lara Croft: GoL - IL PB Log"] = _logForm.Show,
            };

            _updateTimer = new Timer
            {
                Interval = 15,
                Enabled = true,
            };
            _updateTimer.Tick += UpdateTimer_Tick;
		}

		public override void Dispose()
		{
			_timer.CurrentState.OnStart -= Timer_OnStart;
			_logForm?.Dispose();
			_updateTimer?.Dispose();
		}

		private void UpdateTimer_Tick(object sender, EventArgs eventArgs)
		{
			try
			{
				_gameMemory.Update();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.ToString());
			}
		}

		private void Timer_OnStart(object sender, EventArgs e)
		{
			_timer.InitializeGameTime();
		}

		private void GameMemory_OnLevelFinished(object sender, string level)
		{
			if (!(DateTime.Now - _lastSplit < TimeSpan.FromSeconds(10.0)))
			{
				_lastSplit = DateTime.Now;
				_timer.Split();
			}
		}

		private void GameMemory_OnFirstLevelStarted(object sender, EventArgs e)
		{
			_timer.Start();
		}

		private void GameMemory_OnFirstLevelLoading(object sender, EventArgs e)
		{
			_timer.Reset();
		}

		private void GameMemory_OnLoadStart(object sender, EventArgs e)
		{
			_timer.CurrentState.IsGameTimePaused = true;
		}

		private void GameMemory_OnLoadFinish(object sender, EventArgs e)
		{
			_timer.CurrentState.IsGameTimePaused = false;
		}

		private void GameMemory_OnInvalidSettingsDetected(object sender, EventArgs e)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			if (_timer.CurrentState.CurrentPhase == TimerPhase.Running)
			{
				MessageBox.Show("Invalid settings detected. VSync must be ON and refresh rate must be set to 60hz. Stopping timer.", "LiveSplit.LaraCroftGoL", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				_timer.Reset(false);
			}
		}

		private void GameMemory_OnNewILPersonalBest(object sender, string level, TimeSpan time, TimeSpan oldTime)
		{
			var timeSpan = oldTime - time;
			_logForm.AddMessage($"{level}: {time:m\\:ss\\.fff} - {timeSpan:m\\:ss\\.fff} improvement");
			try
			{
				//TODO: Add Sound to OnNewILPersonalBest
				//new SoundPlayer(Resources.UI_reward_b_05_left).Play();
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.ToString());
			}
		}

		public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
		{
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
	}
}
