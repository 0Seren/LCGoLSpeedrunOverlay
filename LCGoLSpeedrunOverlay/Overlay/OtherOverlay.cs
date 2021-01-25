using System.Collections.Generic;
using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using LCGoLOverlayProcess.Overlay.SharpDxHelper;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using WinOSExtensions.Extensions;

namespace LCGoLOverlayProcess.Overlay
{
    internal class OtherOverlay : IOverlay
    {
        public void Render(GameInfo game, Device d3d9Device, LiveSplitHelper liveSplitHelper)
        {
            var y = 0;

            Text.DrawText(d3d9Device, $"{nameof(game.Current.Level)}: {game.Current.Level}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.Current.AreaCode)}: {game.Current.AreaCode}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.Current.NumberOfPlayers)}: {game.Current.NumberOfPlayers}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.Current.GameState)}: {game.Current.GameState}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.Current.GameTime)}: {game.Current.GameTime.ToTimerString()}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.Current.ValidVsyncSettings)}: {game.Current.ValidVsyncSettings}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.Current.HasControl)}: {game.Current.HasControl}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
        }
    }
}
