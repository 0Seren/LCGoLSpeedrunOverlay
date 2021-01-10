using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using LCGoLOverlayProcess.Overlay.SharpDxHelper;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

namespace LCGoLOverlayProcess.Overlay
{
    class OtherOverlay : IOverlay
    {
        public void Render(GameInfo game, Device d3d9Device, LiveSplitHelper liveSplitHelper)
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

            // TODO: Drawing window to screen is so slow as to not be usable. We should load textures in a seperate threrad similar to how the liveSplitHelper WindowBitmap works?
            using (Texture texture = Texture.FromMemory(d3d9Device, liveSplitHelper.WindowBitmap))
            {
                using (Sprite sprite = new Sprite(d3d9Device))
                {
                    sprite.Begin();
                    var pos = new RawVector3(0, 0, 0);
                    var color = new RawColorBGRA(255, 255, 255, 255);

                    sprite.Draw(texture, color, null, null, pos);

                    sprite.End();
                }
            }
        }
    }
}
