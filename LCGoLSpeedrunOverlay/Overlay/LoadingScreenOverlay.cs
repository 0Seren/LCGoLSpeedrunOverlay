using System.Drawing;
using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using LCGoLOverlayProcess.Overlay.SharpDxHelper;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using WinOSExtensions.Extensions;

namespace LCGoLOverlayProcess.Overlay
{
    internal class LoadingScreenOverlay : IOverlay
    {
        private Texture _liveSplitTexture;
        private Sprite _liveSplitSprite;
        private Rectangle _liveSplitRectangle;
        private GameInfoSnapShot _prevLevelInfo;

        private readonly ImageConverter _converter;

        public LoadingScreenOverlay()
        {
            _converter = new ImageConverter();
        }

        private void GenerateLiveSplitSprite(Device d3d9Device, LiveSplitHelper liveSplitHelper)
        {
            _liveSplitTexture?.Dispose();
            _liveSplitSprite?.Dispose();

            var bitmap = liveSplitHelper.WindowBitmap;

            if (bitmap != null)
            {
                var bytes = (byte[])_converter.ConvertTo(bitmap, typeof(byte[]));

                _liveSplitRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                _liveSplitTexture = Texture.FromMemory(d3d9Device, bytes);
                _liveSplitSprite = new Sprite(d3d9Device);
                bitmap.Dispose();
            }
        }

        public void Render(GameInfo game, Device d3d9Device, LiveSplitHelper liveSplitHelper)
        {
            if (game.State.Old != GameState.InLoadScreen && string.IsNullOrWhiteSpace(game.AreaCode.Old))
            {
                _prevLevelInfo = null;
            }

            if (game.State.Changed && game.State.Current == GameState.InEndScreen)
            {
                _prevLevelInfo = game.GetGameInfoSnapShot();
                GenerateLiveSplitSprite(d3d9Device, liveSplitHelper);
            }

            if (game.State.Old != GameState.InLoadScreen &&
                game.State.Current == GameState.InLoadScreen)
            {
                GenerateLiveSplitSprite(d3d9Device, liveSplitHelper);
            }

            var y = 0;
            if (_prevLevelInfo != null)
            {
                Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.Level)}: {_prevLevelInfo.Level.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
                y += 36;
                Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.AreaCode)}: {_prevLevelInfo.AreaCode.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
                y += 36;
                Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.NumberOfPlayers)}: {_prevLevelInfo.NumberOfPlayers.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
                y += 36;
                Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.State)}: {_prevLevelInfo.State.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
                y += 36;
                Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.GameTime)}: {_prevLevelInfo.GameTime.Current.ToTimerString()}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
                y += 36;
                Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.ValidVSyncSettings)}: {_prevLevelInfo.ValidVSyncSettings.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
                y += 36;
                Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.HasControl)}: {_prevLevelInfo.HasControl.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));

                y += 36;
                Text.DrawText(d3d9Device, $"------------------", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
                y += 36;
            }

            Text.DrawText(d3d9Device, $"{nameof(game.Level)}: {game.Level.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.AreaCode)}: {game.AreaCode.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.NumberOfPlayers)}: {game.NumberOfPlayers.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.State)}: {game.State.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.GameTime)}: {game.GameTime.Current.ToTimerString()}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.ValidVSyncSettings)}: {game.ValidVSyncSettings.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(game.HasControl)}: {game.HasControl.Current}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));

            if (_liveSplitSprite != null && _liveSplitTexture != null)
            {
                _liveSplitSprite.Begin();

                var w = d3d9Device.Viewport.Width;
                var h = d3d9Device.Viewport.Height;

                var pos = new RawVector3(w-_liveSplitRectangle.Width, h-_liveSplitRectangle.Height, 0);
                var color = new RawColorBGRA(255, 255, 255, 255);

                _liveSplitSprite.Draw(_liveSplitTexture, color, null, null, pos);

                _liveSplitSprite.End();
            }
        }
    }
}
