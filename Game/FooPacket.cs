using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.Game
{
    public struct FooPacket
    {
        public byte PacketID;
        public byte[] Data;
        public byte SenderID;

        public FooPacket(byte id, byte[] data, byte senderID)
        {
            PacketID = id;
            Data = data;
            SenderID = senderID;
        }

    }
}
