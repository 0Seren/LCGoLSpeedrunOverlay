using System;
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
        private readonly string _liveSplitSpriteName = nameof(LoadingScreenOverlay) + "|" + nameof(_liveSplitSpriteName) + "|" + Guid.NewGuid().ToString("X");
        private readonly string _liveSplitTextureName = nameof(LoadingScreenOverlay) + "|" + nameof(_liveSplitTextureName) + "|" + Guid.NewGuid().ToString("X");

        private Rectangle _liveSplitRectangle;
        private GameInfoSnapShot _prevLevelInfo;

        private readonly SharpDxResourceManager _sharpDxResourceManager;

        public LoadingScreenOverlay(SharpDxResourceManager sharpDxResourceManager)
        {
            _sharpDxResourceManager = sharpDxResourceManager;
        }

        private void GenerateLiveSplitSprite(Device d3d9Device, LiveSplitHelper liveSplitHelper)
        {
            var bitmap = liveSplitHelper.WindowBitmap;

            if (!(bitmap is null))
            {
                _liveSplitRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                _sharpDxResourceManager.ResetSprite(d3d9Device, _liveSplitSpriteName);
                _sharpDxResourceManager.SetTexture(d3d9Device, _liveSplitTextureName, bitmap);

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

            var white = new RawColorBGRA(255, 255, 255, 255);
            var lineSpacing = 36;

            var font = _sharpDxResourceManager.GetFont(d3d9Device, 34);

            var x = 0;
            var y = -lineSpacing;

            if (_prevLevelInfo != null)
            {
                font.DrawText(null, $"{nameof(_prevLevelInfo.Level)}: {_prevLevelInfo.Level.Current}", x, y += lineSpacing, white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.AreaCode)}: {_prevLevelInfo.AreaCode.Current}", x, y += lineSpacing, white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.NumberOfPlayers)}: {_prevLevelInfo.NumberOfPlayers.Current}", x, y += lineSpacing, white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.State)}: {_prevLevelInfo.State.Current}", x, y += lineSpacing, white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.GameTime)}: {_prevLevelInfo.GameTime.Current.ToTimerString()}", x, y += lineSpacing, white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.ValidVSyncSettings)}: {_prevLevelInfo.ValidVSyncSettings.Current}", x, y += lineSpacing, white);
                font.DrawText(null, $"{nameof(_prevLevelInfo.HasControl)}: {_prevLevelInfo.HasControl.Current}", x, y += lineSpacing, white);

                font.DrawText(null, $"------------------", x, y += lineSpacing, white);
            }

            font.DrawText(null, $"{nameof(game.Level)}: {game.Level.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.AreaCode)}: {game.AreaCode.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.NumberOfPlayers)}: {game.NumberOfPlayers.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.State)}: {game.State.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.GameTime)}: {game.GameTime.Current.ToTimerString()}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.ValidVSyncSettings)}: {game.ValidVSyncSettings.Current}", x, y += lineSpacing, white);
            font.DrawText(null, $"{nameof(game.HasControl)}: {game.HasControl.Current}", x, y += lineSpacing, white);

            var liveSplitSprite = _sharpDxResourceManager.GetSprite(d3d9Device, _liveSplitSpriteName);
            var liveSplitTexture = _sharpDxResourceManager.GetTexture(_liveSplitTextureName);

            if (liveSplitTexture is null)
            {
                GenerateLiveSplitSprite(d3d9Device, liveSplitHelper);
            }

            if (liveSplitSprite != null && liveSplitTexture != null)
            {
                liveSplitSprite.Begin();

                var w = d3d9Device.Viewport.Width;
                var h = d3d9Device.Viewport.Height;

                var pos = new RawVector3(w-_liveSplitRectangle.Width, h-_liveSplitRectangle.Height, 0);

                liveSplitSprite.Draw(liveSplitTexture, white, null, null, pos);

                liveSplitSprite.End();
            }
        }
    }
}
