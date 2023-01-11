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
        private readonly SharpDxResourceManager _sharpDxResourceManager;

        public OtherOverlay(SharpDxResourceManager sharpDxResourceManager)
        {
            _sharpDxResourceManager = sharpDxResourceManager;
        }

        public void Render(GameInfo game, Device d3d9Device, LiveSplitHelper liveSplitHelper)
        {
            var white = new RawColorBGRA(255, 255, 255, 255);
            var lineSpacing = 36;

            var font = _sharpDxResourceManager.GetFont(d3d9Device, 34);

            var x = 0;
            var y = -lineSpacing;

            font.DrawText(null, $"{nameof(game.Level)}: {game.Level.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.AreaCode)}: {game.AreaCode.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.NumberOfPlayers)}: {game.NumberOfPlayers.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.State)}: {game.State.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.GameTime)}: {game.GameTime.Current.ToTimerString()}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.ValidVSyncSettings)}: {game.ValidVSyncSettings.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.HasControl)}: {game.HasControl.Current}", x, y += lineSpacing, white);
        }
    }
}
