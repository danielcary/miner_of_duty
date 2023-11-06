using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.Game
{
    public class WinChecker
    {
        public Dictionary<byte, int> TeamScores;
        public int maxScore;

        public void AddTeam(byte id)
        {
            TeamScores.Add(id, 0);
        }

        public WinChecker(int maxScore = -1)
        {
            TeamScores = new Dictionary<byte, int>();
            this.maxScore = maxScore;
        }

        public byte[] Sorted;

        public void Sort()
        {

        }


        /// <summary>
        /// Use this when time runs up
        /// </summary>
        public void DeclareWinner()
        {

        }

    }
}
