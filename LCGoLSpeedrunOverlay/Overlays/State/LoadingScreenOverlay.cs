using System;
using System.Drawing;
using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

namespace LCGoLOverlayProcess.Overlays.State
{
    internal class LoadingScreenOverlay : IOverlay
    {
        private readonly string _liveSplitSpriteName = nameof(LoadingScreenOverlay) + "|" + nameof(_liveSplitSpriteName) + "|" + Guid.NewGuid().ToString("X");
        private readonly string _liveSplitTextureName = nameof(LoadingScreenOverlay) + "|" + nameof(_liveSplitTextureName) + "|" + Guid.NewGuid().ToString("X");

        private static readonly RawColorBGRA _white = new RawColorBGRA(255, 255, 255, 255);

        private Rectangle _liveSplitRectangle;
        private readonly LiveSplitHelper _liveSplitHelper;

        public LoadingScreenOverlay(LiveSplitHelper liveSplitHelper)
        {
            _liveSplitHelper = liveSplitHelper;
        }

        private void GenerateLiveSplitSprite(Device d3d9Device)
        {
            var bitmap = _liveSplitHelper.WindowBitmap;

            if (!(bitmap is null))
            {
                _liveSplitRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

                d3d9Device.ResetSprite(_liveSplitSpriteName);
                d3d9Device.SetTexture(_liveSplitTextureName, bitmap);

                bitmap.Dispose();
            }
        }

        public void Render(GameInfo game, Device d3d9Device)
        {
            if (game.State.Changed && game.State.Current == GameState.InEndScreen)
            {
                GenerateLiveSplitSprite(d3d9Device);
            }

            var liveSplitSprite = d3d9Device.GetSprite(_liveSplitSpriteName);
            var liveSplitTexture = d3d9Device.GetTexture(_liveSplitTextureName);

            if (liveSplitTexture is null)
            {
                GenerateLiveSplitSprite(d3d9Device);
                liveSplitSprite = d3d9Device.GetSprite(_liveSplitSpriteName);
                liveSplitTexture = d3d9Device.GetTexture(_liveSplitTextureName);
            }

            if (!(liveSplitSprite is null) && !(liveSplitTexture is null))
            {
                var w = d3d9Device.Viewport.Width;
                var h = d3d9Device.Viewport.Height;
                var pos = new RawVector3(w - _liveSplitRectangle.Width, h - _liveSplitRectangle.Height, 0);

                liveSplitSprite.Begin();

                liveSplitSprite.Draw(liveSplitTexture, _white, null, null, pos);

                liveSplitSprite.End();
            }
        }
    }
}
