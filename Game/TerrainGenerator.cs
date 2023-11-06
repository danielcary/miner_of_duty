using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game
{
    public static class TerrainGenerator
    {

        private static void GetSurrondingPoints(ref Vector3i point, ref Vector3i[] surronding)
        {
            int counter = 0;
            for (int x = -1; x < 2; x++)
                for (int y = -1; y < 2; y++)
                    for (int z = -1; z < 2; z++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;
                        surronding[counter].X = point.X + x;
                        surronding[counter].Y = point.Y + y;
                        surronding[counter++].Z = point.Z + z;
                    }
        }

        private static int Distance(ref Vector3i a, ref Vector3i b)
        {
            return ((b.X - a.X) * (b.X - a.X)) + ((b.Y - a.Y) * (b.Y - a.Y)) + ((b.Z - a.Z) * (b.Z - a.Z));
        }

        private static void GenerateLine(Vector3i start, Vector3i end, List<Vector3i> points)
        {
            Vector3i[] surronding = new Vector3i[26];
            Vector3i workingNode = start;
            int best;
            int bestDist;
            int tmpDist;
            while (true)
            {
                if (workingNode == end)
                    return;

                GetSurrondingPoints(ref workingNode, ref surronding);

                best = 0;
                bestDist = Distance(ref workingNode, ref end);
                for (int i = 0; i < 26; i++)
                {
                    tmpDist = Distance(ref surronding[i], ref end);
                    if (tmpDist < bestDist)
                    {
                        bestDist = tmpDist;
                        best = i;
                    }
                }

                points.Add(surronding[best]);
                workingNode = surronding[best];
            }
        }

        private static BoundingSphere bs = new BoundingSphere(Vector3.Zero, 3);
        private static void FillAroundPoint(Vector3i point, byte[, ,] world, int size, Random ran)
        {
            bs.Radius = (3 + (float)(ran.NextDouble() * 3.0));
            bs.Center.X = point.X;
            bs.Center.Y = point.Y;
            bs.Center.Z = point.Z;
            Vector3 fillpoint = Vector3.Zero;
            Vector3i newPoint;
            ContainmentType type;
            for (int x = -(int)bs.Radius + 1; x < bs.Radius; x++)
                for (int y = -(int)bs.Radius + 1; y < bs.Radius; y++)
                    for (int z = -(int)bs.Radius + 1; z < bs.Radius; z++)
                    {
                        fillpoint.X = x + point.X;
                        fillpoint.Y = y + point.Y;
                        fillpoint.Z = z + point.Z;

                        bs.Contains(ref fillpoint, out type);

                        newPoint.X = (int)fillpoint.X;
                        newPoint.Y = (int)fillpoint.Y;
                        newPoint.Z = (int)fillpoint.Z;

                        if (type == ContainmentType.Contains
                            && newPoint.X >= 0 && newPoint.X < size &&
                               newPoint.Y >= 0 && newPoint.Y < Terrain.MAXHEIGHT &&
                               newPoint.Z >= 0 && newPoint.Z < size)
                            world[newPoint.X, newPoint.Y, newPoint.Z] = Block.BLOCKID_AIR;
                    }

        }

        public static void GenerateCaves(Random ran, byte[, ,] world, int size)
        {
            List<Vector3i> cave1 = new List<Vector3i>(), cave2 = new List<Vector3i>();
            int start1 = ran.Next(size, size * 15);
            int start2 = ran.Next(size, size * 15);

            //generate caves
            for (int i = 1; i < size; i++)
            {
                start1++;
                cave1.Add(new Vector3i(
                    i,
                    (int)(9.0 * Math.Sin((Math.PI * 2.0 / 26.0) * (double)start1) + 17.0),
                    (int)(30.0 * Math.Sin((2.0 * Math.PI / (42.0 + (2.0 / 3.0))) * (double)start1) + (double)i)));
            }

            for (int i = size; i > 0; i--)
            {
                start2++;
                cave2.Add(new Vector3i(
                    i,
                    (int)(9.0 * Math.Sin((Math.PI * 2.0 / 26.0) * (double)start2) + 17.0),
                    (int)(30.0 * Math.Sin((2.0 * Math.PI / (42.0 + (2.0 / 3.0))) * (double)start2) + (double)size - (double)i)));
            }

            //generate lines between caves
            List<Vector3i> linepoints = new List<Vector3i>(); //lets put all are points into one list
            linepoints.Add(cave1[0]);
            for (int i = 0; i < cave1.Count - 1; i++)
            {
                GenerateLine(cave1[i], cave1[i + 1], linepoints);
            }
            linepoints.Add(cave2[0]);
            for (int i = 0; i < cave2.Count - 1; i++)
            {
                GenerateLine(cave2[i], cave2[i + 1], linepoints);
            }

            //generateSpaceAround
            for (int i = 0; i < linepoints.Count; i++)
            {
                FillAroundPoint(linepoints[i], world, size, ran);
            }

        }

        private static float[] ScaleUp(int size, int maxSize, float[] data)
        {
            float[] finalColor = new float[maxSize * maxSize];
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    finalColor[(x * 2) + (y * 2) * maxSize] = data[x + y * size];
                    finalColor[((x * 2) + 1) + (y * 2) * maxSize] = data[x + y * size];
                    finalColor[(x * 2) + ((y * 2) + 1) * maxSize] = data[x + y * size];
                    finalColor[((x * 2) + 1) + ((y * 2) + 1) * maxSize] = data[x + y * size];
                }
            return finalColor;
        }

        private static void Blend(int times, int size, float[] data)
        {
            for (int i = 0; i < times; i++)
                for (int x = 0; x < size; x++)
                    for (int y = 0; y < size; y++)
                    {
                        if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                            continue;
                        int combos = 0;
                        if (y != size - 1)
                        {
                            data[x + y * size] += data[x + (y + 1) * size];
                            combos++;
                        }
                        if (x != size - 1)
                        {
                            data[x + y * size] += data[(x + 1) + y * size];
                            combos++;
                        }
                        if (y != 0)
                        {
                            data[x + y * size] += data[x + (y - 1) * size];
                            combos++;
                        }
                        if (x != 0)
                        {
                            data[x + y * size] += data[(x - 1) + y * size];
                            combos++;
                        }

                        if (y != size - 1 && x != size - 1)
                        {
                            data[x + y * size] += data[(x + 1) + (y + 1) * size];
                            combos++;
                        }
                        if (x != size - 1 && y != 0)
                        {
                            data[x + y * size] += data[(x + 1) + (y - 1) * size];
                            combos++;
                        }
                        if (y != 0 && x != 0)
                        {
                            data[x + y * size] += data[(x - 1) + (y - 1) * size];
                            combos++;
                        }
                        if (x != 0 && y != size - 1)
                        {
                            data[x + y * size] += data[(x - 1) + (y + 1) * size];
                            combos++;
                        }
                        data[x + y * size] /= combos + 1;
                    }
        }

        private static float[] GenerateLayer(int size, int maxSize, Random ran)
        {
            float[] data = new float[size * size];

            for (int i = 0; i < size * size; i++)
                data[i] = (float)ran.NextDouble();

            if (size != maxSize)
            {
                int workingSize = size;
                while (workingSize != maxSize)
                {
                    data = ScaleUp(workingSize, workingSize * 2, data);
                    workingSize *= 2;
                }
            }

            Blend(maxSize / size, maxSize, data);

            return data;
        }

        public static float[] GenerateHeightMap(Random ran)
        {
            float[] map = new float[256 * 256];

            float[] layer1 = GenerateLayer(8, 256, ran);
            float[] layer2 = GenerateLayer(16, 256, ran);
            float[] layer3 = GenerateLayer(32, 256, ran);
            float[] layer4 = GenerateLayer(64, 256, ran);
            float[] layer5 = GenerateLayer(128, 256, ran);
            float[] layer6 = GenerateLayer(256, 256, ran);

            Blend(3, 256, layer6);

            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 256; y++)
                {
                    map[x + y * 256] =
                        layer1[x + y * 256] +
                        layer2[x + y * 256] +
                        layer3[x + y * 256] +
                        layer4[x + y * 256] +
                        layer5[x + y * 256] +
                        layer6[x + y * 256];
                    map[x + y * 256] /= 6f;
                }

            Blend(15, 256, map);

            return map;
        }

        public static float[] GenerateFlatHeightMap(float height)
        {
            float[] map = new float[256 * 256];

            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 256; y++)
                {
                    map[x + y * 256] = height;                    
                }

            return map;
        }

        public static T[] GetChunkOfData<T>(T[] data, int offset, int size, int orginalSize)
        {
            T[] chunk = new T[size * size];

            for (int x = offset; x < offset + size; x++)
                for (int y = offset; y < offset + size; y++)
                {
                    chunk[(x - offset) + (y - offset) * size] = data[x + y * orginalSize];
                }

            return chunk;
        }

        public static void GenerateTrees(byte[, ,] world, int size, int amountOfTrees, Random ran)
        {
            for (int i = 0; i < amountOfTrees; i++)
            {
                int x = ran.Next(255);
                int z = ran.Next(255);

                int startLength = ((128 - size) / 2) + 64;
                int endLength = 128 - (startLength - 64) + 64;

                if (x > (startLength - 6) && x < (startLength + 6))
                    continue;
                if (z > (startLength - 6) && z < (startLength + 6))
                    continue;

                if (x > (endLength - 6) && x < (endLength + 6))
                    continue;
                if (z > (endLength - 6) && z < (endLength + 6))
                    continue;

                int treebase = 0;
                for (int y = 54; y >= 0; y--)
                {
                    if (world[x, y, z] == Block.BLOCKID_GRASS)
                        break;
                    treebase = y;
                }
                if (treebase <= 0)
                    continue;

                int treeheight = ran.Next(3);
                treeheight += 6;

                for (int j = 0; j < treeheight; j++)
                {
                    world[x, treebase + j, z] = Block.BLOCKID_WOOD;
                }

                for (int j = 0; j < treeheight - 1; j++)
                {
                    AddLeavesAround(x, treebase + (treeheight - j), z, world, (int)MathHelper.Clamp(j, 0, treeheight - 5));
                }
            }

        }

        private static void AddLeavesAround(int X, int Y, int Z, byte[, ,] world, int howmnay)
        {
            for (int x = X - howmnay; x <= X + howmnay; x++)
                for (int z = Z - howmnay; z <= Z + howmnay; z++)
                {
                    if (x >= 0 && x < 256)
                        if (z >= 0 && z < 256)
                            if (world[x, Y, z] == Block.BLOCKID_AIR)
                                world[x, Y, z] = Block.BLOCKID_LEAF;
                }
        }


        public static void GenerateWall(Terrain terrain, int size)
        {
            int startLength = (128 - size) / 2;
            int endLength = 128 - startLength;

            bool hitRock = false;
            for (int x = startLength; x < endLength; x++)
            {
                for (int y = 63; y >= 0; y--)
                {
                    if (hitRock)
                    {
                        if (!Block.IsBlockSolid(terrain.blocks[x, y, 0]))
                        {
                            terrain.blocks[x, y, 0] = Block.BLOCKID_BEDROCK;
                        }
                    }
                    else if (Block.IsBlockSolid(terrain.blocks[x, y, 0]) && terrain.blocks[x, y, 0] != Block.BLOCKID_LEAF)
                        hitRock = true;
                }
                hitRock = false;
            }

            for (int x = startLength; x < endLength; x++)
            {
                for (int y = 63; y >= 0; y--)
                {
                    if (hitRock)
                    {
                        if (!Block.IsBlockSolid(terrain.blocks[x, y, 127]))
                        {
                            terrain.blocks[x, y, 127] = Block.BLOCKID_BEDROCK;
                        }
                    }
                    else if (Block.IsBlockSolid(terrain.blocks[x, y, 127]) && terrain.blocks[x, y, 127] != Block.BLOCKID_LEAF)
                        hitRock = true;
                }
                hitRock = false;
            }

            for (int z = startLength; z < endLength; z++)
            {
                for (int y = 63; y >= 0; y--)
                {
                    if (hitRock)
                    {
                        if (!Block.IsBlockSolid(terrain.blocks[0, y, z]))
                        {
                            terrain.blocks[0, y, z] = Block.BLOCKID_BEDROCK;
                        }
                    }
                    else if (Block.IsBlockSolid(terrain.blocks[0, y, z]) && terrain.blocks[0, y, z] != Block.BLOCKID_LEAF)
                        hitRock = true;
                }
                hitRock = false;
            }

            for (int z = startLength; z < endLength; z++)
            {
                for (int y = 63; y >= 0; y--)
                {
                    if (hitRock)
                    {
                        if (!Block.IsBlockSolid(terrain.blocks[127, y, z]))
                        {
                            terrain.blocks[127, y, z] = Block.BLOCKID_BEDROCK;
                        }
                    }
                    else if (Block.IsBlockSolid(terrain.blocks[127, y, z]) && terrain.blocks[127, y, z] != Block.BLOCKID_LEAF)
                        hitRock = true;
                }
                hitRock = false;
            }



            for (int x = startLength; x < endLength; x++)
                for (int y = 0; y < 64; y++)
                {
                    if (terrain.blocks[x, y, startLength] != Block.BLOCKID_AIR && terrain.blocks[x, y, startLength] != Block.BLOCKID_LEAF && terrain.blocks[x, y, startLength] != Block.BLOCKID_GLASSWALL)
                    {
                        terrain.blocks[x, y, startLength] = Block.BLOCKID_BEDROCK;
                    }
                    else
                    {
                        terrain.blocks[x, y, startLength] = Block.BLOCKID_GLASSWALL;
                    }
                }

            for (int x = startLength; x < endLength; x++)
                for (int y = 0; y < 64; y++)
                {
                    if (terrain.blocks[x, y, endLength - 1] != Block.BLOCKID_AIR && terrain.blocks[x, y, endLength - 1] != Block.BLOCKID_LEAF && terrain.blocks[x, y, endLength - 1] != Block.BLOCKID_GLASSWALL)
                    {
                        terrain.blocks[x, y, endLength - 1] = Block.BLOCKID_BEDROCK;
                    }
                    else
                    {
                        terrain.blocks[x, y, endLength - 1] = Block.BLOCKID_GLASSWALL;
                    }
                }

            for (int z = startLength; z < endLength; z++)
                for (int y = 0; y < 64; y++)
                {
                    if (terrain.blocks[startLength, y, z] != Block.BLOCKID_AIR && terrain.blocks[startLength, y, z] != Block.BLOCKID_LEAF && terrain.blocks[startLength, y, z] != Block.BLOCKID_GLASSWALL)
                    {
                        terrain.blocks[startLength, y, z] = Block.BLOCKID_BEDROCK;
                    }
                    else
                    {
                        terrain.blocks[startLength, y, z] = Block.BLOCKID_GLASSWALL;
                    }
                }

            for (int z = startLength; z < endLength; z++)
                for (int y = 0; y < 64; y++)
                {
                    if (terrain.blocks[endLength - 1, y, z] != Block.BLOCKID_AIR && terrain.blocks[endLength - 1, y, z] != Block.BLOCKID_LEAF && terrain.blocks[endLength - 1, y, z] != Block.BLOCKID_GLASSWALL)
                    {
                        terrain.blocks[endLength - 1, y, z] = Block.BLOCKID_BEDROCK;
                    }
                    else
                    {
                        terrain.blocks[endLength - 1, y, z] = Block.BLOCKID_GLASSWALL;
                    }
                }

        }

        private static float GetCircleHeight(float val)
        {
            return (float)((.1 * Math.Cos(2 * Math.PI * val)) + .4);
        }

        public static float[] GenerateBeachWorld(Random ran)
        {
            float[] val = GenerateHeightMap(ran);

            Vector2 centerPoint = new Vector2(128, 128);
            Vector2 otherPoint = new Vector2();
            float dis;

            float minDis = 20;
            float maxDis = 50;
            

             for (int x = 0; x < 256; x++)
                 for (int y = 0; y < 256; y++)
                 {

                     otherPoint.X = x;
                     otherPoint.Y = y;

                     minDis += ran.Next(-4, 5);
                     minDis = MathHelper.Clamp(minDis, 13, 27);

                     maxDis += ran.Next(-4, 5);
                     maxDis = MathHelper.Clamp(maxDis, 43, 57);

                     Vector2.Distance(ref centerPoint, ref otherPoint, out dis);

                     if (dis <= maxDis && dis >= minDis)
                     {
                         val[x + y * 256] = GetCircleHeight((dis - minDis) / (maxDis - minDis));
                     }

                 }


             Blend(5, 256, val);

             return val;
        }
    }
}
