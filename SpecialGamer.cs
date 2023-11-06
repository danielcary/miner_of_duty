using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.GamerServices;

namespace Miner_Of_Duty
{
    public static class SpecialGamer
    {
        public static string[] DevGamerTags =
        {
            "beatlefan796",
            "Nickev Dev",
            "LJ x John1 x LJ",
            "LJ x John x LJ",
            "almantux11",
            "almantux12",
        };

        public static string[] TestGamerTags = 
        {
            "LJ x John1 x LJ",
            "LJ x John x LJ",
            "almantux11",
            "almantux12",
        };

        public static string[] FriendGamerTags =
        {
            "beatlefan796",
            "Nickev Dev",
            "Doctor TBOMB",
            "BUBBLEJACKS1089",
            "Echoes963",
            "Doctor Bearkat",
            "Plantmanz",
            "EagleBane",
            "Christothegr8",
            "Foxxy Kyle",
            "elc2838",
            "gogogamer888",
            "PiggyHitman",
            "Steve481",
            "xTHExASSETx",
            "Danny",
            "MasterlilDylan"
        };

        public static bool IsTest(string name)
        {
            for (int i = 0; i < TestGamerTags.Length; i++)
                if (name.Equals(TestGamerTags[i]))
                    return true;
            return false;
        }

        public static bool IsDev(Gamer gamer)
        {
            for (int i = 0; i < DevGamerTags.Length; i++)
                if (gamer.Gamertag.Equals(DevGamerTags[i]))
                    return true;
            return false;
        }

        

        public static bool IsSwordOwner(Gamer gamer)
        {
            if(MinerOfDuty.CurrentPlayerProfile.HasSword)
                return true;

            for (int i = 0; i < TestGamerTags.Length; i++)
                if (gamer.Gamertag.Equals(TestGamerTags[i]))
                    return true;


            for (int i = 0; i < FriendGamerTags.Length; i++)
                if (gamer.Gamertag.Equals(FriendGamerTags[i]))
                    return true;

            if (gamer.Gamertag.Equals("TheTIM3BOMB"))
                return true;

            return false;
        }
    }
}
