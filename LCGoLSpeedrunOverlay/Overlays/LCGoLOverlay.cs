using LCGoLOverlayProcess.Game;
using LCGoLOverlayProcess.Helpers;
using LCGoLOverlayProcess.Overlays.Global;
using LCGoLOverlayProcess.Overlays.State;
using SharpDX.Direct3D9;
using System.Collections.Concurrent;

namespace LCGoLOverlayProcess.Overlays
{
    internal class LCGoLOverlay : IOverlay
    {
        private readonly IOverlay _debugOverlay;
        private readonly ConcurrentDictionary<GameState, IOverlay> _overlayLookup;

        public LCGoLOverlay(LiveSplitHelper liveSplitHelper)
        {
            _debugOverlay = new DebugOverlay();

            var loadingOverlay = new LoadingScreenOverlay(liveSplitHelper);
            _overlayLookup = new ConcurrentDictionary<GameState, IOverlay>
            {
                [GameState.InEndScreen] = loadingOverlay,
                [GameState.InLoadScreen] = loadingOverlay,
            };
        }

        public void Render(GameInfo game, Device d3d9Device)
        {
            // TODO: Pass in a scaling factor here? At least figure our how overlay scaling will work.
            if (_overlayLookup.TryGetValue(game.State.Current, out var overlay))
            {
                overlay.Render(game, d3d9Device);
            }

            _debugOverlay.Render(game, d3d9Device);
        }
    }
}
