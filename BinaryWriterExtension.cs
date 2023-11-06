using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;

namespace Miner_Of_Duty
{
    public static class BinaryWriterExtension
    {

        public static void Write(this BinaryWriter bw, ref Vector2 vec)
        {
            bw.Write(vec.X);
            bw.Write(vec.Y);
        }

        public static void Write(this BinaryWriter bw, ref Vector3 vec)
        {
            bw.Write(vec.X);
            bw.Write(vec.Y);
            bw.Write(vec.Z);
        }

        public static void Write(this BinaryWriter bw, Vector2 vec)
        {
            bw.Write(vec.X);
            bw.Write(vec.Y);
        }

        public static void Write(this BinaryWriter bw, Vector3 vec)
        {
            bw.Write(vec.X);
            bw.Write(vec.Y);
            bw.Write(vec.Z);
        }

        public static void Write(this BinaryWriter bw, List<Vector3> list)
        {
            bw.Write(list.Count);
            for (int i = 0; i < list.Count; i++)
                bw.Write(list[i]);
        }
    }

    public static class BinaryReaderExtension
    {

        public static void Read(this BinaryReader br, out Vector2 vec)
        {
            vec = new Vector2(br.ReadSingle(), br.ReadSingle());
        }

        public static void Read(this BinaryReader br, out Vector3 vec)
        {
            vec = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }

        public static void Read(this BinaryReader br, Vector3 vec)
        {
            vec = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public static List<Vector3> ReadVector3List(this BinaryReader br)
        {
            int count = br.ReadInt32();
            List<Vector3> list = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                list.Add(br.ReadVector3());
            }
            return list;
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }
    }
}
