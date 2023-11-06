using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Miner_Of_Duty.LobbyCode;

namespace Miner_Of_Duty.Menus
{
    public class MapMetaInfo
    {
        public string Author;
        public string MapName;
        public DateTime TimeEdited;
        public int version;
        public GameModes GameMode;

        public MapMetaInfo(string author, string mapName, long time, string filename, GameModes gameMode)
        {
            Author = author;
            MapName = mapName;
            TimeEdited = new DateTime(time);
            FileName = filename;
            GameMode = gameMode;
        }

        public string FileName;

    }
}
