using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

namespace LCGoLOverlayProcess.Overlay.SharpDxHelper
{
    class Text
    {
        private static Font _font;

        // TODO: Fix the Font Class. Instance methods. Pass in fonts. Make a base font. Scaling? Have a resource manager?
        public static void DrawText(Device device, string text, int textHeight, int x, int y, RawColorBGRA textColor)
        {
            if (device != null)
            {
                if (_font == null)
                {
                    _font = new Font(device, new FontDescription()
                    {
                        Height = textHeight,
                        FaceName = "Arial",
                        Italic = false,
                        Width = 0,
                        MipLevels = 1,
                        CharacterSet = FontCharacterSet.Default,
                        OutputPrecision = FontPrecision.Default,
                        Quality = FontQuality.ClearTypeNatural,
                        PitchAndFamily = FontPitchAndFamily.Default | FontPitchAndFamily.DontCare,
                        Weight = FontWeight.Bold
                    });
                }

                _font.DrawText(null, text, x, y, textColor);
            }
        }
    }
}
