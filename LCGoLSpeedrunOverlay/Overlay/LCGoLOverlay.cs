﻿using LCGoLOverlayProcess.Game;
using SharpDX.Direct3D9;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCGoLOverlayProcess.Overlay
{
    public class LCGoLOverlay : IOverlay
    {
        private readonly ConcurrentDictionary<GameState, IOverlay> _overlayLookup;

        public LCGoLOverlay()
        {
            _overlayLookup = new ConcurrentDictionary<GameState, IOverlay>()
            {
                [GameState.InEndScreen] = new EndScreenOverlay(),
                [GameState.InLoadScreen] = new LoadingScreenOverlay(),
                [GameState.Other] = new OtherOverlay(),
            };
        }

        public void Render(GameInfo game, Device d3d9Device)
        {
            // TODO: Pass in a scaling factor here? At least figure our how overlay scaling will work.
            // TODO: Change OverlayPicker to use the correct Overlay
            if (false)//_overlayLookup.TryGetValue(game.GameState, out var overlay))
            {
                //overlay.Render(game, d3d9Device);
            } else
            {
                _overlayLookup[GameState.Other].Render(game, d3d9Device);
            }
        }
    }
}
