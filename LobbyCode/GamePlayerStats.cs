using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.LobbyCode
{
    public class GamePlayerStats 
    {
        public PlayerProfile pp;

        public int Level { get; set; }
        public int Kills { get;  set; }
        public int Deaths { get;  set; }
        public int ConsecutiveKills { get; private set; }
        public int ConsecutiveDeaths { get; private set; }
        public int Score { get; set; }
        public bool IsMuted { get; set; }
        public byte WhoKilledMeLast { get; set; }
        /// <summary>
        /// In Milliseconds
        /// </summary>
        public int TimeSinceLastKill { get; set; }
        public string ClanTag { get; set; }

        public void AddScore(int score)
        {
            Score += score;
        }

        public void ClearStats()
        {
            Kills = Deaths = ConsecutiveKills = ConsecutiveDeaths = Score = 0;
            WhoKilledMeLast = 0;
        }

        public void AddKill()
        {
            ConsecutiveDeaths = 0;
            TimeSinceLastKill = 0;
            ConsecutiveKills++;
            Kills++;
        }

        public void AddDeath()
        {
            ConsecutiveDeaths++;
            ConsecutiveKills = 0;
            Deaths++;
        }

        public GamePlayerStats()
        {
            ClearStats();
            ClanTag = "";
        }

    }
}
