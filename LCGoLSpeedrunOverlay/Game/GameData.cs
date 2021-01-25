using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCGoLOverlayProcess.Game
{
    public class GameData
    {
        public GameData()
        {
            GameTime = new TimeSpan();
            AreaCode = string.Empty;
            Level = GameLevel.Unknown;
            NumberOfPlayers = 1;
            HasControl = true;
            GameState = GameState.Other;
            ValidVsyncSettings = true;
        }

        public GameData(GameData otherData)
        {
            GameTime = otherData.GameTime;
            AreaCode = otherData.AreaCode;
            Level = otherData.Level;
            NumberOfPlayers = otherData.NumberOfPlayers;
            HasControl = otherData.HasControl;
            GameState = otherData.GameState;
            ValidVsyncSettings = otherData.ValidVsyncSettings;
        }

        public TimeSpan GameTime { get; set; }
        public string AreaCode { get; set; }
        public GameLevel Level { get; set; }
        public int NumberOfPlayers { get; set; }
        public bool HasControl { get; set; }
        public GameState GameState { get; set; }
        public bool ValidVsyncSettings { get; set; }
    }
}
