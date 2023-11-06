using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty
{
    public class EndianBitConverter
    {
        private static EndianBitConverter converter = CreateForBigEndian();

        public static EndianBitConverter CreateForLittleEndian()
        {
            return new EndianBitConverter(!BitConverter.IsLittleEndian);
        }

        public static EndianBitConverter CreateForBigEndian()
        {
            return new EndianBitConverter(BitConverter.IsLittleEndian);
        }

        bool swap;
        private EndianBitConverter(bool swapBytes)
        {
            swap = swapBytes;
        }

        public static Int32 ToInt32(byte[] data)
        {
            byte[] corrected;
            if (converter.swap)
            {
                corrected = data.Clone() as byte[];
                Array.Reverse(corrected, 0, 4);
            }
            else
            {
                corrected = data;
            }
            return BitConverter.ToInt32(corrected, 0);
        }

        public static byte[] GetBytes(int data)
        {
            byte[] corrected = BitConverter.GetBytes(data);

            if (converter.swap)
            {
                Array.Reverse(corrected, 0, 4);
            }

            return corrected;
        }

        // And similar methods for GetBytes(Int32) and all other types needed.
    }
}
