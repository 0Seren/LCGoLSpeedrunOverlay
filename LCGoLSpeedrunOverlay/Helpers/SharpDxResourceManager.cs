using SharpDX;
using SharpDX.Direct3D9;
using System.Collections.Generic;

namespace LCGoLOverlayProcess.Helpers
{
    internal static class SharpDxResourceManager
    {
        private static readonly FontDescription DefaultFontDescription;
        private static readonly System.Drawing.ImageConverter _converter;
        private static readonly DisposeCollector _disposeCollector;

        private static readonly IDictionary<FontDescription, Font> _fontLookup;
        private static readonly IDictionary<string, Sprite> _spriteLookup;
        private static readonly IDictionary<string, Texture> _textureLookup;

        static SharpDxResourceManager()
        {
            _converter = new System.Drawing.ImageConverter();
            _disposeCollector = new DisposeCollector();
            _fontLookup = new Dictionary<FontDescription, Font>();
            _spriteLookup = new Dictionary<string, Sprite>();
            _textureLookup = new Dictionary<string, Texture>();

            DefaultFontDescription = new FontDescription
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
        }

        public static Font GetFont(this Device device)
        {
            return device.GetFont(DefaultFontDescription);
        }

        public static Font GetFont(this Device device, int textHeight)
        {
            return device.GetFont(new FontDescription
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

        public static Font GetFont(this Device device, FontDescription fontDescription)
        {
            if (_fontLookup.TryGetValue(fontDescription, out var font) && !(font is null) && !font.IsDisposed)
            {
                return font;
            }
            else
            {
                return device.CreateAndRegisterNewFont(fontDescription);
            }
        }

        public static Sprite GetSprite(this Device device, string spriteName)
        {
            if (_spriteLookup.TryGetValue(spriteName, out var sprite) && !(sprite is null) && !sprite.IsDisposed)
            {
                return sprite;
            } else
            {
                return device.CreateAndRegisterNewSprite(spriteName);
            }
        }

        public static Sprite ResetSprite(this Device device, string spriteName)
        {
            if (_spriteLookup.TryGetValue(spriteName, out var sprite) && !(sprite is null) && !sprite.IsDisposed)
            {
                _disposeCollector.RemoveAndDispose(ref sprite);
                _spriteLookup.Remove(spriteName);
            }

            return device.GetSprite(spriteName);
        }

        public static Texture GetTexture(this Device device, string textureName)
        {
            if (_textureLookup.TryGetValue(textureName, out var texture) && !(texture is null) && !texture.IsDisposed)
            {
                return texture;
            }

            return null;
        }

        public static Texture SetTexture(this Device device, string textureName, System.Drawing.Bitmap bitmap)
        {
            if (_textureLookup.TryGetValue(textureName, out var texture) && !(texture is null) && !texture.IsDisposed)
            {
                _disposeCollector.RemoveAndDispose(ref texture);
                _spriteLookup.Remove(textureName);
            }

            return device.CreateAndRegisterTexture(textureName, bitmap);
        }

        private static Texture CreateAndRegisterTexture(this Device device, string textureName, System.Drawing.Bitmap bitmap)
        {
            var bytes = (byte[])_converter.ConvertTo(bitmap, typeof(byte[]));
            var texture = Texture.FromMemory(device, bytes, bitmap.Width, bitmap.Height, 1, Usage.RenderTarget, Format.BinaryBuffer, Pool.Default, Filter.Default, Filter.Default, 0);
            _disposeCollector.Collect(texture);
            _textureLookup[textureName] = texture;
            return texture;
        }

        private static Sprite CreateAndRegisterNewSprite(this Device device, string spriteName)
        {
            var sprite = new Sprite(device);
            _disposeCollector.Collect(sprite);
            _spriteLookup[spriteName] = sprite;
            return sprite;
        }

        private static Font CreateAndRegisterNewFont(this Device device, FontDescription fontDescription)
        {
            var font = new Font(device, fontDescription);
            _disposeCollector.Collect(font);
            _fontLookup[fontDescription] = font;
            return font;
        }

        public static void Reset()
        {
            _disposeCollector.DisposeAndClear();
        }

        private static readonly Destructor _finalize = new Destructor();

        private sealed class Destructor
        {
            ~Destructor()
            {
                Reset();
            }
        }
    }
}
