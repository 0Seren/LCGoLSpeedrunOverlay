using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;

namespace LCGoLOverlayProcess.Overlay.SharpDxHelper
{
    internal class SharpDxResourceManager : IDisposable
    {
        private static readonly FontDescription DefaultFontDescription = new FontDescription
        {
            Height = 34,
            FaceName = "Arial",
            Italic = false,
            Width = 0,
            MipLevels = 1,
            CharacterSet = FontCharacterSet.Default,
            OutputPrecision = FontPrecision.Default,
            Quality = FontQuality.ClearTypeNatural,
            PitchAndFamily = FontPitchAndFamily.Default | FontPitchAndFamily.DontCare,
            Weight = FontWeight.Bold
        };

        private readonly System.Drawing.ImageConverter _converter;
        private readonly DisposeCollector _disposeCollector;
        private bool _disposedValue;
        public bool IsDisposed { get => _disposedValue; }

        private readonly IDictionary<FontDescription, Font> _fontLookup;
        private readonly IDictionary<string, Sprite> _spriteLookup;
        private readonly IDictionary<string, Texture> _textureLookup;

        public SharpDxResourceManager()
        {
            _converter = new System.Drawing.ImageConverter();
            _disposeCollector = new DisposeCollector();
            _fontLookup = new Dictionary<FontDescription, Font>();
            _spriteLookup = new Dictionary<string, Sprite>();
            _textureLookup = new Dictionary<string, Texture>();
        }

        public Font GetFont(Device device)
        {
            return GetFont(device, DefaultFontDescription);
        }

        public Font GetFont(Device device, int textHeight)
        {
            return GetFont(device, new FontDescription
            {
                Height = textHeight,
                FaceName = DefaultFontDescription.FaceName,
                Italic = DefaultFontDescription.Italic,
                Width = DefaultFontDescription.Width,
                MipLevels = DefaultFontDescription.MipLevels,
                CharacterSet = DefaultFontDescription.CharacterSet,
                OutputPrecision = DefaultFontDescription.OutputPrecision,
                Quality = DefaultFontDescription.Quality,
                PitchAndFamily = DefaultFontDescription.PitchAndFamily,
                Weight = DefaultFontDescription.Weight
            });
        }

        public Font GetFont(Device device, FontDescription fontDescription)
        {
            if (IsDisposed)
                return null;

            if (_fontLookup.TryGetValue(fontDescription, out var font) && !(font is null) && !font.IsDisposed)
            {
                return font;
            }
            else
            {
                return CreateAndRegisterNewFont(device, fontDescription);
            }
        }

        public Sprite GetSprite(Device device, string spriteName)
        {
            if (IsDisposed)
                return null;

            if (_spriteLookup.TryGetValue(spriteName, out var sprite) && !(sprite is null) && !sprite.IsDisposed)
            {
                return sprite;
            } else
            {
                return CreateAndRegisterNewSprite(device, spriteName);
            }
        }

        public Sprite ResetSprite(Device device, string spriteName)
        {
            if (IsDisposed)
                return null;

            if (_spriteLookup.TryGetValue(spriteName, out var sprite) && !(sprite is null) && !sprite.IsDisposed)
            {
                _disposeCollector.RemoveAndDispose(ref sprite);
                _spriteLookup.Remove(spriteName);
            }

            return GetSprite(device, spriteName);
        }

        public Texture GetTexture(string textureName)
        {
            if (IsDisposed)
                return null;

            if (_textureLookup.TryGetValue(textureName, out var texture) && !(texture is null) && !texture.IsDisposed)
            {
                return texture;
            }

            return null;
        }

        public Texture SetTexture(Device device, string textureName, System.Drawing.Bitmap bitmap)
        {
            if (IsDisposed)
                return null;

            if (_textureLookup.TryGetValue(textureName, out var texture) && !(texture is null) && !texture.IsDisposed)
            {
                _disposeCollector.RemoveAndDispose(ref texture);
                _spriteLookup.Remove(textureName);
            }

            return CreateAndRegisterTexture(device, textureName, (byte[])_converter.ConvertTo(bitmap, typeof(byte[])));
        }

        private Texture CreateAndRegisterTexture(Device device, string textureName, byte[] bytes)
        {
            if (IsDisposed)
                return null;

            var texture = Texture.FromMemory(device, bytes);
            _disposeCollector.Collect(texture);
            _textureLookup[textureName] = texture;
            return texture;
        }

        private Sprite CreateAndRegisterNewSprite(Device device, string spriteName)
        {
            if (IsDisposed)
                return null;

            var sprite = new Sprite(device);
            _disposeCollector.Collect(sprite);
            _spriteLookup[spriteName] = sprite;
            return sprite;
        }

        private Font CreateAndRegisterNewFont(Device device, FontDescription fontDescription)
        {
            if (IsDisposed)
                return null;

            var font = new Font(device, fontDescription);
            _disposeCollector.Collect(font);
            _fontLookup[fontDescription] = font;
            return font;
        }

        public void Cleanup()
        {
            _disposeCollector.DisposeAndClear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposeCollector.DisposeAndClear(disposing);
                _disposedValue = true;
            }
        }

        ~SharpDxResourceManager()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
