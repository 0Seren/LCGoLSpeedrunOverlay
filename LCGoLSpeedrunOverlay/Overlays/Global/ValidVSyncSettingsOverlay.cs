using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using SharpDX.Direct3D9;
using System;
using System.Drawing;

namespace LCGoLOverlayProcess.Overlays.Global
{
    class ValidVSyncSettingsOverlay : IOverlay
    {
        private readonly string _vsyncStatusSpriteName = $"{nameof(ValidVSyncSettingsOverlay)}|{nameof(_vsyncStatusSpriteName)}|{Guid.NewGuid():X}";
        private readonly string _validVsyncTextureName = $"{nameof(ValidVSyncSettingsOverlay)}|{nameof(_validVsyncTextureName)}|{Guid.NewGuid():X}";
        private readonly string _invalidVsyncTextureName = $"{nameof(ValidVSyncSettingsOverlay)}|{nameof(_invalidVsyncTextureName)}|{Guid.NewGuid():X}";
        private static readonly SharpDX.ColorBGRA _white = new SharpDX.ColorBGRA(255, 255, 255, 255);
        private static readonly int _size = 15;
        private static readonly Bitmap _validVSyncSettingsImage = new Bitmap(_size, _size);
        private static readonly Bitmap _invalidVSyncSettingsImage = new Bitmap(_size, _size);

        private float _rotation = 0f;

        public ValidVSyncSettingsOverlay()
        {
            FillBitmap(_validVSyncSettingsImage, Color.FromArgb(0, 145, 18));
            FillBitmap(_invalidVSyncSettingsImage, Color.FromArgb(175, 3, 3));
        }

        public void Render(GameInfo game, Device d3d9Device)
        {
            var vsyncStatusSprite = d3d9Device.GetSprite(_vsyncStatusSpriteName);

            var validVsyncTexture = d3d9Device.GetTexture(_validVsyncTextureName);
            var invalidVsyncTexture = d3d9Device.GetTexture(_invalidVsyncTextureName);

            if (validVsyncTexture is null)
            {
                validVsyncTexture = d3d9Device.SetTexture(_validVsyncTextureName, _validVSyncSettingsImage);
            }

            if (invalidVsyncTexture is null)
            {
                invalidVsyncTexture = d3d9Device.SetTexture(_invalidVsyncTextureName, _invalidVSyncSettingsImage);
            }

            var texture = invalidVsyncTexture;
            if (game.ValidVSyncSettings.Current)
            {
                texture = validVsyncTexture;
            }

            var pos = new SharpDX.Vector3(_size, d3d9Device.Viewport.Height - _size, 0);
            _rotation = (_rotation + .05f) % 360;

            vsyncStatusSprite.Begin();
            vsyncStatusSprite.Draw(texture, _white, null, new SharpDX.Vector3(_size/2.0f, _size/2.0f, 0), new SharpDX.Vector3(0,0,0));
            vsyncStatusSprite.Transform = SharpDX.Matrix.RotationZ(_rotation) * SharpDX.Matrix.Translation(pos);
            vsyncStatusSprite.End();
        }

        private void FillBitmap(Bitmap bitmap, Color color)
        {
            using (Graphics gfx = Graphics.FromImage(bitmap))
            using (SolidBrush brush = new SolidBrush(color))
            {
                gfx.FillRectangle(brush, 0, 0, bitmap.Width, bitmap.Height);
            }
        }
    }
}
