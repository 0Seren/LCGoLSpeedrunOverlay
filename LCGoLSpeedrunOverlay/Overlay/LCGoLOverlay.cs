using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using LCGoLOverlayProcess.Overlay.SharpDxHelper;
using SharpDX.Direct3D9;
using System.Collections.Concurrent;

namespace LCGoLOverlayProcess.Overlay
{
    internal class LCGoLOverlay : IOverlay
    {
        private readonly ConcurrentDictionary<GameState, IOverlay> _overlayLookup;

        public LCGoLOverlay(SharpDxResourceManager sharpDxResourceManager)
        {
            var loadingOverlay = new LoadingScreenOverlay(sharpDxResourceManager);
            var otherOverlay = new OtherOverlay(sharpDxResourceManager);

            _overlayLookup = new ConcurrentDictionary<GameState, IOverlay>
            {
                [GameState.InEndScreen] = loadingOverlay,
                [GameState.InLoadScreen] = loadingOverlay,
                [GameState.Other] = otherOverlay,
            };
        }

        public void Render(GameInfo game, Device d3d9Device, LiveSplitHelper liveSplitHelper)
        {
            // TODO: Pass in a scaling factor here? At least figure our how overlay scaling will work.
            if (_overlayLookup.TryGetValue(game.State.Current, out var overlay))
            {
                overlay.Render(game, d3d9Device, liveSplitHelper);
            } else
            {
                _overlayLookup[GameState.Other].Render(game, d3d9Device, liveSplitHelper);
            }
        }
    }
}
