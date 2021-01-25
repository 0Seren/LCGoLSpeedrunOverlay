using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpeedrunComSharp;

namespace LCGoLOverlayProcess.Helpers
{
    public class SpeedRunComHelper
    {
        private SpeedrunComClient _client;
        private SpeedrunComSharp.Game _game;
        private ServerInterface _server;

        //TODO: Implement SpeedRunComHelper to pull back IL leader boards
        public SpeedRunComHelper(ServerInterface server)
        {
            _server = server;
            _client = new SpeedrunComClient();

            _game = _client.Games.SearchGame("Lara Croft and the Guardian of Light", new GameEmbeds(true, true, true, true, true, true));

            var levels = _game.Levels;

            foreach (var level in levels)
            {
                _server.ReportMessage(level.Name);
            }
        }


        private void SRCGameUpdateThread()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(300000); //5 minutes
                    var game = _game = _client.Games.GetGameFromSiteUri("https://www.speedrun.com/lcgol", new GameEmbeds(true, true, true, true, true, true));
                    lock (_game)
                    {
                        _game = game;
                    }
                }
                catch (Exception e)
                {
                    _server.ReportException(e);
                }
            }
        }
    }
}
