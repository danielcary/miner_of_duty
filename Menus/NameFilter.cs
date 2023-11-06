using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.Menus
{
    public static class NameFilter
    {
        #region badwords
        private static string[,] BadWords;
        static NameFilter()
        {
            BadWords = new string[,]
            {
                { "ass", "a##"},
                { "fuck", "f###"},
                { "cunt", "c###"},
                { "whore", "w####"},
                { "shit", "s###" },
	            { "bitch", "b####" },
	            { "cock", "c###" },
            	{ "pussy", "p####" },
	            { "dick", "d###" },
                { "penis", "p####" },
                { "vagina", "v#####" },
            };
        }
        #endregion

        public static string FilterName(string realName)
        {
            if (realName == ":(")
                return ":)";

            string name = realName.ToLower();
            for (int i = 0; i < BadWords.GetLength(0); i++)
            {
                name = name.Replace(BadWords[i,0], BadWords[i,1]);
            }

            char[] realNameChars = realName.ToCharArray();
            for (int i = 0; i < realName.Length; i++)
            {
                if (name[i] == '#')
                    realNameChars[i] = '#';
            }

            return new string(realNameChars);
        }
    }
}
