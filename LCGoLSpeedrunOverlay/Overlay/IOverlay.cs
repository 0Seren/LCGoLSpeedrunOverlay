using LCGoLOverlayProcess.Game;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCGoLOverlayProcess.Overlay
{
    public interface IOverlay
    {
        /// <summary>
        /// Draws the overlay to the screen.
        /// </summary>
        /// <param name="game">A GameInfo object containing the current state of the game.</param>
        /// <param name="d3d9Device">A d3d9 device to render with.</param>
        void Render(GameInfo game, Device d3d9Device);
    }
}
