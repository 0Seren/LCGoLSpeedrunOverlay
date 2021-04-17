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

            Text.DrawText(d3d9Device, $"{nameof(game.Level.Current)}: {game.Level.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.AreaCode.Current)}: {game.AreaCode.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.NumberOfPlayers.Current)}: {game.NumberOfPlayers.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.State.Current)}: {game.State.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.GameTime.Current)}: {game.GameTime.Current.ToTimerString()}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.ValidVSyncSettings.Current)}: {game.ValidVSyncSettings.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.HasControl.Current)}: {game.HasControl.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
        }
    }
}
