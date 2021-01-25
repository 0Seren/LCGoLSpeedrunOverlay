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
        private GameData _prevLevelInfo;
        private Texture _liveSplitTexture;
        private Sprite _liveSplitSprite;
        private Rectangle _liveSplitRectangle;

        private readonly ImageConverter _converter;

        public LoadingScreenOverlay()
        {
            _prevLevelInfo = new GameData();
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
            if (game.Previous.GameState != GameState.InEndScreen && game.Current.GameState == GameState.InEndScreen)
            {
                _prevLevelInfo = new GameData(game.Current);
                GenerateLiveSplitSprite(d3d9Device, liveSplitHelper);
            }

            if (game.Previous.GameState != GameState.InLoadScreen &&
                game.Current.GameState == GameState.InLoadScreen)
            {
                GenerateLiveSplitSprite(d3d9Device, liveSplitHelper);
            }

            var y = 0;

            Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.Level)}: {_prevLevelInfo.Level}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.AreaCode)}: {_prevLevelInfo.AreaCode}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.NumberOfPlayers)}: {_prevLevelInfo.NumberOfPlayers}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.GameState)}: {_prevLevelInfo.GameState}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.GameTime)}: {_prevLevelInfo.GameTime.ToTimerString()}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.ValidVsyncSettings)}: {_prevLevelInfo.ValidVsyncSettings}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;
            Text.DrawText(d3d9Device, $"{nameof(_prevLevelInfo.HasControl)}: {_prevLevelInfo.HasControl}", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));

            y += 36;
            Text.DrawText(d3d9Device, $"------------------", 34, 0, y, new RawColorBGRA(255, 255, 255, 255));
            y += 36;

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
