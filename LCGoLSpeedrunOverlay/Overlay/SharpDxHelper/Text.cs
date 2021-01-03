using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCGoLOverlayProcess.Overlay.SharpDxHelper
{
    class Text
    {
        // TODO: Fix the Font Class. Instance methods. Pass in fonts. Make a base font. Scaling?
        public static void DrawText(Device device, string text, int textHeight, int x, int y, RawColorBGRA textColor)
        {
            using (Font font = new Font(device, new FontDescription()
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
            }))
            {
                font.DrawText(null, text, x, y, textColor);
            }
        }
    }
}
