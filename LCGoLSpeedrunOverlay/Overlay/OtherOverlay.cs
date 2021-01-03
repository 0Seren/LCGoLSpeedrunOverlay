using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using LCGoLOverlayProcess.Overlay.SharpDxHelper;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

namespace LCGoLOverlayProcess.Overlay
{
    class OtherOverlay : IOverlay
    {
        public void Render(GameInfo game, Device d3d9Device)
        {
            int y = 0;

            Text.DrawText(d3d9Device, $"{nameof(game.Level)}: {game.Level}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.IsOnEndScreen)}: {game.IsOnEndScreen.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.NumberOfPlayers)}: {game.NumberOfPlayers.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.GameState)}: {game.GameState}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.GameTime)}: {game.GameTime.ToTimerString()}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.ValidVsyncSettings)}: {game.ValidVsyncSettings}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
        }
    }
}
