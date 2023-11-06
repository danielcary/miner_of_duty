using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.Game
{
    public struct Vector3i
    {
        public int X, Y, Z;
        public static Vector3i NULL = new Vector3i(-1, -1, -1);
        public static Vector3i Zero = new Vector3i(0, 0, 0);
        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static int DistanceSquared(ref Vector3i a, Vector3i b)
        {
            return (b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y) + (b.Z - a.Z) * (b.Z - a.Z);
        }

        public static bool operator !=(Vector3i a, Vector3i b)
        {
            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
                return false;
            else
                return true;
        }

        public static bool operator ==(Vector3i a, Vector3i b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
    }

    public struct Vector4i
    {
        public int X, Y, Z, W;
        public static Vector4i NULL = new Vector4i(-1, -1, -1, -1);
        public Vector4i(int x, int y, int z, int w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

    }
}
