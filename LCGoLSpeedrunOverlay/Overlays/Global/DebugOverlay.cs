using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using System;
using System.Linq;
using WinOSExtensions.Extensions;

namespace LCGoLOverlayProcess.Overlays.Global
{
    class DebugOverlay : IOverlay
    {
        private const int _lineSpacing = 36;
        private static readonly RawColorBGRA _white = new RawColorBGRA(255, 255, 255, 255);
        private static readonly GameState[] _showPreviousLevelInfoStates = new GameState[]
        {
            GameState.InLoadScreen,
            //GameState.InEndScreen,
        };

        private readonly string _debugTextSpriteName = $"{nameof(DebugOverlay)}|{nameof(_debugTextSpriteName)}|{Guid.NewGuid():X}";

        private GameInfoSnapShot _prevLevelInfo = null;

        public void Render(GameInfo game, Device d3d9Device)
        {
            if (game.State.Old != GameState.InLoadScreen && string.IsNullOrWhiteSpace(game.AreaCode.Old))
            {
                _prevLevelInfo = null;
            }

            if (game.State.Changed && game.State.Current == GameState.InEndScreen)
            {
                _prevLevelInfo = game.GetGameInfoSnapShot();
            }

            var font = d3d9Device.GetFont(34);

            var x = 0;
            var y = -_lineSpacing;

            if (_showPreviousLevelInfoStates.Contains(game.State.Current) && !(_prevLevelInfo is null))
            {
                font.DrawText(null, $"{nameof(_prevLevelInfo.Level)}: {_prevLevelInfo.Level.Current}", x, y += _lineSpacing, _white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.AreaCode)}: {_prevLevelInfo.AreaCode.Current}", x, y += _lineSpacing, _white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.NumberOfPlayers)}: {_prevLevelInfo.NumberOfPlayers.Current}", x, y += _lineSpacing, _white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.State)}: {_prevLevelInfo.State.Current}", x, y += _lineSpacing, _white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.GameTime)}: {_prevLevelInfo.GameTime.Current.ToTimerString()}", x, y += _lineSpacing, _white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.ValidVSyncSettings)}: {_prevLevelInfo.ValidVSyncSettings.Current}", x, y += _lineSpacing, _white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.HasControl)}: {_prevLevelInfo.HasControl.Current}", x, y += _lineSpacing, _white);

                font.DrawText(null, $"------------------", x, y += _lineSpacing, _white);
            }

            font.DrawText(null, $"{nameof(game.Level)}: {game.Level.Current}", x, y += _lineSpacing, _white);
            font.DrawText(null, $"{nameof(game.AreaCode)}: {game.AreaCode.Current}", x, y += _lineSpacing, _white);
            font.DrawText(null, $"{nameof(game.NumberOfPlayers)}: {game.NumberOfPlayers.Current}", x, y += _lineSpacing, _white);
            font.DrawText(null, $"{nameof(game.State)}: {game.State.Current}", x, y += _lineSpacing, _white);
            font.DrawText(null, $"{nameof(game.GameTime)}: {game.GameTime.Current.ToTimerString()}", x, y += _lineSpacing, _white);
            font.DrawText(null, $"{nameof(game.ValidVSyncSettings)}: {game.ValidVSyncSettings.Current}", x, y += _lineSpacing, _white);
            font.DrawText(null, $"{nameof(game.HasControl)}: {game.HasControl.Current}", x, y += _lineSpacing, _white);
        }
    }
}
