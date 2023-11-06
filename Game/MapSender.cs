using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Net;
using Miner_Of_Duty.Game.Networking;

namespace Miner_Of_Duty.Game
{
    public class MapSender
    {
        public byte RecipentID;
        public bool HasStarted { get; private set; }
        private List<byte[]> MapPackets;

        public MapSender(MemoryStream ms)
        {
            HasStarted = false;
            MapPackets = new List<byte[]>();
            ms.Position = 0;
            byte[] buffer;
            while (true)
            {
                buffer = new byte[1000];
                int end = ms.Read(buffer, 0, 1000);

                if (end == 0)//no bytes need to be sent
                    break;
                else if (end != 1000)
                {
                    byte[] tmpBuffer = new byte[end];
                    Array.Copy(buffer, tmpBuffer, end);

                    MapPackets.Add(tmpBuffer);
                    //we could break here but I'll just let it run again to make
                    //sure we got everything
                }
                else
                {
                    MapPackets.Add(buffer);
                }
            }

            ms.Close();
        }

        public void Dispose()
        {
            MapPackets.Clear();
            MapPackets = null;
        }   

        public void StartWritePacketTo(NetworkGamer gamer)
        {
            if (gamer == null)
                throw new ArgumentNullException();

            HasStarted = true;
            Packet.PacketWriter.Write(Packet.PACKETID_MAPDATA);
            Packet.PacketWriter.Write((short)0);//this is the packet u requested
            Packet.PacketWriter.Write(true);//if the packet u wanted equals 
            //the amount of packets ur done
            Packet.PacketWriter.Write((short)MapPackets[0].Length);
            Packet.PacketWriter.Write(MapPackets[0]);

            MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, gamer);
        }

        public void PacketRequest(NetworkGamer sender)
        {
            short packetWanted = Packet.PacketReader.ReadInt16();

            Packet.PacketWriter.Write(Packet.PACKETID_MAPDATA);
            Packet.PacketWriter.Write(packetWanted);//this is the packet u requested
            Packet.PacketWriter.Write((packetWanted + 1) < MapPackets.Count);//if the packet u wanted equals 
            //the amount of packets ur done
            Packet.PacketWriter.Write((short)MapPackets[packetWanted].Length);
            Packet.PacketWriter.Write(MapPackets[packetWanted]);

            MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, sender);
        }
    }

    public class MapReceiver
    {
        private MemoryStream ms;

        /// <summary>
        /// Only use if PacketReceive returned true
        /// </summary>
        public MemoryStream GetMapData { get { return ms; } }

        public MapReceiver()
        {
            ms = new MemoryStream();
          //  packetWanted = 0;
           // Packet.PacketWriter.Write(Packet.PACKETID_MAPDATAREQUEST);
           // Packet.PacketWriter.Write(packetWanted);
           // MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host); 
        }

        private short packetWanted;
        /// <summary>
        /// Use with a MAPDATA packet
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public bool PacketReceive(NetworkGamer sender)
        {
            packetWanted = Packet.PacketReader.ReadInt16();

            if (packetWanted == 6969)
            {
                ms.Dispose();
                ms.Close();
                ms = new MemoryStream();
                if (MinerOfDuty.game.justGotHere)
                {
                    MinerOfDuty.game.waitingForMatch = true;
                }
            }

            bool doIWantMore = Packet.PacketReader.ReadBoolean();

            short bytes = Packet.PacketReader.ReadInt16();
            ms.Write(Packet.PacketReader.ReadBytes(bytes), 0, bytes);

            if (doIWantMore)
            {
                Packet.PacketWriter.Write(Packet.PACKETID_MAPDATAREQUEST);
                Packet.PacketWriter.Write(packetWanted + 1);
                MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);
                return false;
            }
            else return true;
        }

    }
}
