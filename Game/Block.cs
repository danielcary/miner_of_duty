using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game
{
    public static class Block
    {
        public static byte BLOCKID_AIR = 0;
        public static byte BLOCKID_STONE = 1;
        public static byte BLOCKID_DIRT = 2;
        public static byte BLOCKID_GRASS = 3;
        public static byte BLOCKID_BEDROCK = 4;
        public static byte BLOCKID_COBBLE = 5;
        public static byte BLOCKID_SAND = 6;
        public static byte BLOCKID_LEAF = 7;
        public static byte BLOCKID_WOOD = 8;
        public static byte BLOCKID_GOLD = 9;
        public static byte BLOCKID_GLASSWALL = 10;
        public static byte BLOCKID_BLACK = 11;
        public static byte BLOCKID_BLUE = 12;
        public static byte BLOCKID_GREEN = 13;
        public static byte BLOCKID_GREY = 14;
        public static byte BLOCKID_ORANGE = 15;
        public static byte BLOCKID_WHITE = 16;
        public static byte BLOCKID_TEAL = 17;
        public static byte BLOCKID_RED = 18;
        public static byte BLOCKID_YELLOW = 19;
        public static byte BLOCKID_GLASSUSEABLE = 20;
        public static byte BLOCKID_GLOWBLOCK = 21;
        public static byte BLOCKID_STONEBRICKS = 22;
        public static byte BLOCKID_FIREBRICKS = 23;
        public static byte BLOCKID_WOODPLANKS = 24;
        public static byte BLOCKID_COBBLESTONE = 25;
        public static byte BLOCKID_MOSSYCOBBLESTONE = 26;
        public static byte BLOCKID_PITFALLBLOCK = 27;

        public static byte BLOCKID_WATER = 240;
        public static byte BLOCKID_LAVA = 241;
        public static byte[] BLOCKIDS = { BLOCKID_AIR, BLOCKID_STONE, BLOCKID_DIRT, BLOCKID_GRASS, BLOCKID_BEDROCK, BLOCKID_COBBLE, BLOCKID_SAND, BLOCKID_LEAF, BLOCKID_WOOD, BLOCKID_GOLD, BLOCKID_GLASSWALL,
                                        BLOCKID_BLACK, BLOCKID_BLUE, BLOCKID_GREEN, BLOCKID_GREY, BLOCKID_ORANGE, BLOCKID_WHITE, BLOCKID_TEAL, BLOCKID_RED, BLOCKID_YELLOW, BLOCKID_GLASSUSEABLE, BLOCKID_GLOWBLOCK,
                                        BLOCKID_STONEBRICKS, BLOCKID_FIREBRICKS, BLOCKID_WOODPLANKS, BLOCKID_COBBLESTONE, BLOCKID_MOSSYCOBBLESTONE, BLOCKID_PITFALLBLOCK };

        public static HalfVector2[] TexCoords = 
        {
            new HalfVector2(),//AIR
            new HalfVector2(0,0), //stone
            new HalfVector2(2,0), //dirt
            new HalfVector2(1,0), //grass
            new HalfVector2(1,1), //bedroock
            new HalfVector2(0,1), //cobble
            new HalfVector2(2,2), //sand
            new HalfVector2(0,2), //leaf
            new HalfVector2(1,2), //wood
            new HalfVector2(2,1), //wall
            new HalfVector2(0,3), //BLOCKID_GLASS
            new HalfVector2(3,0), //black
            new HalfVector2(3,1), //blue
            new HalfVector2(3,2), //green
            new HalfVector2(3,3), //gray
            new HalfVector2(3,4), //orange
            new HalfVector2(2,4), //white
            new HalfVector2(1,4), //teal
            new HalfVector2(1,3), //red
            new HalfVector2(2,3),// yellow
            new HalfVector2(0,3), //BLOCKID_GLASSUSABLE
            new HalfVector2(0,1), //glowblock
            new HalfVector2(4,1),//stone
            new HalfVector2(4,0), //fire bricks
            new HalfVector2(4,2), //woodplanks
            new HalfVector2(4,3),
            new HalfVector2(4,4),
            new HalfVector2(1,0),//pitfall
        };

        public readonly static Vector3 halfVector = new Vector3(.5f, .5f, .5f);
        private static BoundingBox box = new BoundingBox();
        public static int GetIntersectionSide(ref Ray ray, ref Vector3i block)
        {
            BoundingBox[] sides = new BoundingBox[6];
            box.Max.X = block.X + halfVector.X;
            box.Max.Y = block.Y + halfVector.Y;
            box.Max.Z = block.Z + halfVector.Z;
            box.Min.X = block.X - halfVector.X;
            box.Min.Y = block.Y - halfVector.Y;
            box.Min.Z = block.Z - halfVector.Z;
            Vector3[] corners = box.GetCorners();
            sides[0] = new BoundingBox(corners[2], corners[0]);
            sides[1] = new BoundingBox(corners[7], corners[0]);
            sides[2] = new BoundingBox(corners[6], corners[1]);
            sides[3] = new BoundingBox(corners[7], corners[2]);
            sides[4] = new BoundingBox(corners[5], corners[0]);
            sides[5] = new BoundingBox(corners[7], corners[5]);

            int val = 0;
            float? result = null, newResult;
            for (int i = 0; i < 6; i++)
            {
                ray.Intersects(ref sides[i], out newResult);
                if (newResult.HasValue)
                    if (!result.HasValue)
                    {
                        val = i;
                        result = newResult;
                    }
                    else if (newResult.Value < result.Value)
                    {
                        val = i;
                        result = newResult;
                    }
            }
            return val;
        }
        private const float ONETHIRD = 1f / 3f;
        private const float TWOTHIRD = 2f / 3f;
        public const int VERTEXLENGTH = 36;

        /// <summary>
        /// Used for Selection Cube
        /// </summary>
        public static void CreateCube(int X, int Y, int Z, int nthCube, VertexTextureLight[] verts)
        {
            int vertOff = VERTEXLENGTH * nthCube;

            //face 1
            verts[0 + vertOff].Position = new Vector3(.5f + X, .5f + Y, .5f + Z);
            verts[0 + vertOff].TextureUV = new Vector2(ONETHIRD, 1);
            verts[1 + vertOff].Position = new Vector3(-.5f + X, .5f + Y, .5f + Z);
            verts[1 + vertOff].TextureUV = new Vector2(ONETHIRD, .5f);
            verts[2 + vertOff].Position = new Vector3(-.5f + X, .5f + Y, -.5f + Z);
            verts[2 + vertOff].TextureUV = new Vector2(TWOTHIRD, .5f);
            verts[3 + vertOff].Position = new Vector3(-.5f + X, .5f + Y, -.5f + Z);
            verts[3 + vertOff].TextureUV = new Vector2(TWOTHIRD, .5f);
            verts[4 + vertOff].Position = new Vector3(.5f + X, .5f + Y, -.5f + Z);
            verts[4 + vertOff].TextureUV = new Vector2(TWOTHIRD, 1);
            verts[5 + vertOff].Position = new Vector3(.5f + X, .5f + Y, .5f + Z);
            verts[5 + vertOff].TextureUV = new Vector2(ONETHIRD, 1);

            verts[6 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, -.5f + Z);
            verts[6 + vertOff].TextureUV = new Vector2(0, 1);
            verts[7 + vertOff].Position = new Vector3(-.5f + X, .5f + Y, -.5f + Z);
            verts[7 + vertOff].TextureUV = new Vector2(0, .5f);
            verts[8 + vertOff].Position = new Vector3(-.5f + X, .5f + Y, .5f + Z);
            verts[8 + vertOff].TextureUV = new Vector2(ONETHIRD, .5f);
            verts[9 + vertOff].Position = new Vector3(-.5f + X, .5f + Y, .5f + Z);
            verts[9 + vertOff].TextureUV = new Vector2(ONETHIRD, .5f);
            verts[10 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, .5f + Z);
            verts[10 + vertOff].TextureUV = new Vector2(ONETHIRD, 1);
            verts[11 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, -.5f + Z);
            verts[11 + vertOff].TextureUV = new Vector2(0, 1);

            verts[12 + vertOff].Position = new Vector3(.5f + X, -.5f + Y, -.5f + Z);
            verts[12 + vertOff].TextureUV = new Vector2(ONETHIRD, .5f);
            verts[13 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, -.5f + Z);
            verts[13 + vertOff].TextureUV = new Vector2(ONETHIRD, 0);
            verts[14 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, .5f + Z);
            verts[14 + vertOff].TextureUV = new Vector2(TWOTHIRD, 0);
            verts[15 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, .5f + Z);
            verts[15 + vertOff].TextureUV = new Vector2(TWOTHIRD, 0);
            verts[16 + vertOff].Position = new Vector3(.5f + X, -.5f + Y, .5f + Z);
            verts[16 + vertOff].TextureUV = new Vector2(TWOTHIRD, .5f);
            verts[17 + vertOff].Position = new Vector3(.5f + X, -.5f + Y, -.5f + Z);
            verts[17 + vertOff].TextureUV = new Vector2(ONETHIRD, .5f);

            verts[18 + vertOff].Position = new Vector3(.5f + X, .5f + Y, -.5f + Z);
            verts[18 + vertOff].TextureUV = new Vector2(TWOTHIRD, .5f);
            verts[19 + vertOff].Position = new Vector3(.5f + X, -.5f + Y, -.5f + Z);
            verts[19 + vertOff].TextureUV = new Vector2(TWOTHIRD, 0);
            verts[20 + vertOff].Position = new Vector3(.5f + X, -.5f + Y, .5f + Z);
            verts[20 + vertOff].TextureUV = new Vector2(1, 0);
            verts[21 + vertOff].Position = new Vector3(.5f + X, -.5f + Y, .5f + Z);
            verts[21 + vertOff].TextureUV = new Vector2(1, 0);
            verts[22 + vertOff].Position = new Vector3(.5f + X, .5f + Y, .5f + Z);
            verts[22 + vertOff].TextureUV = new Vector2(1, .5f);
            verts[23 + vertOff].Position = new Vector3(.5f + X, .5f + Y, -.5f + Z);
            verts[23 + vertOff].TextureUV = new Vector2(TWOTHIRD, .5f);

            verts[24 + vertOff].Position = new Vector3(.5f + X, .5f + Y, .5f + Z);
            verts[24 + vertOff].TextureUV = new Vector2(1, 1);
            verts[25 + vertOff].Position = new Vector3(.5f + X, -.5f + Y, .5f + Z);
            verts[25 + vertOff].TextureUV = new Vector2(TWOTHIRD, 1);
            verts[26 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, .5f + Z);
            verts[26 + vertOff].TextureUV = new Vector2(TWOTHIRD, .5f);
            verts[27 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, .5f + Z);
            verts[27 + vertOff].TextureUV = new Vector2(TWOTHIRD, .5f);
            verts[28 + vertOff].Position = new Vector3(-.5f + X, .5f + Y, .5f + Z);
            verts[28 + vertOff].TextureUV = new Vector2(1, .5f);
            verts[29 + vertOff].Position = new Vector3(.5f + X, .5f + Y, .5f + Z);
            verts[29 + vertOff].TextureUV = new Vector2(1, 1);

            verts[30 + vertOff].Position = new Vector3(.5f + X, .5f + Y, -.5f + Z);
            verts[30 + vertOff].TextureUV = new Vector2(0, .5f);
            verts[31 + vertOff].Position = new Vector3(-.5f + X, .5f + Y, -.5f + Z);
            verts[31 + vertOff].TextureUV = new Vector2(0, 0);
            verts[32 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, -.5f + Z);
            verts[32 + vertOff].TextureUV = new Vector2(ONETHIRD, 0);
            verts[33 + vertOff].Position = new Vector3(-.5f + X, -.5f + Y, -.5f + Z);
            verts[33 + vertOff].TextureUV = new Vector2(ONETHIRD, 0);
            verts[34 + vertOff].Position = new Vector3(.5f + X, -.5f + Y, -.5f + Z);
            verts[34 + vertOff].TextureUV = new Vector2(ONETHIRD, .5f);
            verts[35 + vertOff].Position = new Vector3(.5f + X, .5f + Y, -.5f + Z);
            verts[35 + vertOff].TextureUV = new Vector2(0, .5f);
        }

        public static void CreateCubeIndexed(int X, int Y, int Z, byte blockID, ref int vert, ref int indice, bool[] faces, int[] indices, VertexPositionTextureSideLight[] verts)
        {
            HalfVector2 tc = TexCoords[blockID];
            //face 1
            if (faces[0])
            {
                verts[vert].Position = new Vector3(.5f + X, .5f + Y, .5f + Z);//20
                verts[vert].TextureUV = new Vector2(ONETHIRD, 1);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 0;
                verts[vert].Position = new Vector3(-.5f + X, .5f + Y, .5f + Z);//21
                verts[vert].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 0;
                verts[vert].Position = new Vector3(-.5f + X, .5f + Y, -.5f + Z);//22
                verts[vert].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 0;
                verts[vert].Position = new Vector3(.5f + X, .5f + Y, -.5f + Z);//23
                verts[vert].TextureUV = new Vector2(TWOTHIRD, 1);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 0;
                indices[indice++] = vert - 4;
                indices[indice++] = vert - 3;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 1;
                indices[indice++] = vert - 4;
            }

            //-x is waht it faces
            if (faces[1])
            {
                verts[vert].Position = new Vector3(-.5f + X, -.5f + Y, -.5f + Z);//16
                verts[vert].TextureUV = new Vector2(0, 1);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 1;
                verts[vert].Position = new Vector3(-.5f + X, .5f + Y, -.5f + Z);//19
                verts[vert].TextureUV = new Vector2(0, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 1;
                verts[vert].Position = new Vector3(-.5f + X, .5f + Y, .5f + Z);//18
                verts[vert].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 1;
                verts[vert].Position = new Vector3(-.5f + X, -.5f + Y, .5f + Z);//17
                verts[vert].TextureUV = new Vector2(ONETHIRD, 1);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 1;
                indices[indice++] = vert - 4;
                indices[indice++] = vert - 3;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 1;
                indices[indice++] = vert - 4;
            }

            if (faces[2])
            {
                verts[vert].Position = new Vector3(.5f + X, -.5f + Y, -.5f + Z);//12
                verts[vert].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 2;
                verts[vert].Position = new Vector3(-.5f + X, -.5f + Y, -.5f + Z);//13
                verts[vert].TextureUV = new Vector2(ONETHIRD, 0);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 2;
                verts[vert].Position = new Vector3(-.5f + X, -.5f + Y, .5f + Z);//14
                verts[vert].TextureUV = new Vector2(TWOTHIRD, 0);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 2;
                verts[vert].Position = new Vector3(.5f + X, -.5f + Y, .5f + Z);//15
                verts[vert].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 2;
                indices[indice++] = vert - 4;
                indices[indice++] = vert - 3;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 1;
                indices[indice++] = vert - 4;
            }

            if (faces[3])
            {
                verts[vert].Position = new Vector3(.5f + X, .5f + Y, -.5f + Z);//11
                verts[vert].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 3;
                verts[vert].Position = new Vector3(.5f + X, -.5f + Y, -.5f + Z);//9
                verts[vert].TextureUV = new Vector2(TWOTHIRD, 0);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 3;
                verts[vert].Position = new Vector3(.5f + X, -.5f + Y, .5f + Z);//8
                verts[vert].TextureUV = new Vector2(1, 0);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 3;
                verts[vert].Position = new Vector3(.5f + X, .5f + Y, .5f + Z);//10
                verts[vert].TextureUV = new Vector2(1, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 3;
                indices[indice++] = vert - 4;
                indices[indice++] = vert - 3;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 1;
                indices[indice++] = vert - 4;
            }

            if (faces[4])
            {
                verts[vert].Position = new Vector3(.5f + X, .5f + Y, .5f + Z); //4
                verts[vert].TextureUV = new Vector2(1, 1);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 4;
                verts[vert].Position = new Vector3(.5f + X, -.5f + Y, .5f + Z); //7
                verts[vert].TextureUV = new Vector2(TWOTHIRD, 1);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 4;
                verts[vert].Position = new Vector3(-.5f + X, -.5f + Y, .5f + Z); //6
                verts[vert].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 4;
                verts[vert].Position = new Vector3(-.5f + X, .5f + Y, .5f + Z); //5
                verts[vert].TextureUV = new Vector2(1, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 4;
                indices[indice++] = vert - 4;
                indices[indice++] = vert - 3;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 1;
                indices[indice++] = vert - 4;
            }

            if (faces[5])
            {
                verts[vert].Position = new Vector3(.5f + X, .5f + Y, -.5f + Z);
                verts[vert].TextureUV = new Vector2(0, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 5;
                verts[vert].Position = new Vector3(-.5f + X, .5f + Y, -.5f + Z);
                verts[vert].TextureUV = new Vector2(0, 0);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 5;
                verts[vert].Position = new Vector3(-.5f + X, -.5f + Y, -.5f + Z);
                verts[vert].TextureUV = new Vector2(ONETHIRD, 0);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 5;
                verts[vert].Position = new Vector3(.5f + X, -.5f + Y, -.5f + Z);
                verts[vert].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].TexCoords = tc;
                verts[vert++].Lighting = 5;
                indices[indice++] = vert - 4;
                indices[indice++] = vert - 3;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 2;
                indices[indice++] = vert - 1;
                indices[indice++] = vert - 4;
            }

        }

        public static bool IsBlockSolidNoGlass(byte blockID)
        {
            if (blockID == BLOCKID_DIRT
                || blockID == BLOCKID_GRASS
                || blockID == BLOCKID_STONE
                || blockID == BLOCKID_COBBLE
                || blockID == BLOCKID_BEDROCK
                || blockID == BLOCKID_SAND
                || blockID == BLOCKID_WOOD
                || blockID == BLOCKID_LEAF
                || blockID == Block.BLOCKID_PITFALLBLOCK
                || blockID == BLOCKID_GOLD
                || blockID == BLOCKID_GLOWBLOCK
                || blockID == BLOCKID_FIREBRICKS
                || blockID == BLOCKID_STONEBRICKS
                || blockID == BLOCKID_WOODPLANKS
                || blockID == BLOCKID_COBBLESTONE
                || blockID == BLOCKID_MOSSYCOBBLESTONE
                || blockID == BLOCKID_BLACK || blockID == BLOCKID_BLUE || blockID == BLOCKID_GREEN
                || blockID == BLOCKID_GREY || blockID == BLOCKID_ORANGE || blockID == BLOCKID_YELLOW
                || blockID == BLOCKID_WHITE || blockID == BLOCKID_RED || blockID == BLOCKID_TEAL)
            {
                return true;
            }
            else return false;
        }

        public static bool IsBlockSolid(byte blockID)
        {
            if (blockID == BLOCKID_DIRT
                || blockID == BLOCKID_GRASS
                || blockID == BLOCKID_STONE
                || blockID == BLOCKID_COBBLE
                || blockID == BLOCKID_BEDROCK
                || blockID == BLOCKID_SAND
                || blockID == BLOCKID_WOOD
                || blockID == BLOCKID_LEAF
                || blockID == BLOCKID_GOLD
                || Block.BLOCKID_PITFALLBLOCK == blockID
                || BLOCKID_GLASSWALL == blockID
                || BLOCKID_GLASSUSEABLE == blockID
                || blockID == BLOCKID_GLOWBLOCK
                || blockID == BLOCKID_FIREBRICKS
                || blockID == BLOCKID_STONEBRICKS
                || blockID == BLOCKID_WOODPLANKS
                || blockID == BLOCKID_COBBLESTONE
                || blockID == BLOCKID_MOSSYCOBBLESTONE
                || blockID == BLOCKID_BLACK || blockID == BLOCKID_BLUE || blockID == BLOCKID_GREEN
                || blockID == BLOCKID_GREY || blockID == BLOCKID_ORANGE || blockID == BLOCKID_YELLOW
                || blockID == BLOCKID_WHITE || blockID == BLOCKID_RED || blockID == BLOCKID_TEAL)
            {
                return true;
            }
            else return false;
        }

        public static bool IsBlockTransparent(byte blockID)
        {
            if (blockID == BLOCKID_DIRT
                || blockID == BLOCKID_GRASS
                || blockID == BLOCKID_STONE
                || blockID == BLOCKID_COBBLE
                || blockID == BLOCKID_BEDROCK
                || BLOCKID_SAND == blockID
                || blockID == BLOCKID_WOOD
                || blockID == BLOCKID_LEAF
                || blockID == BLOCKID_GOLD
                || blockID == BLOCKID_COBBLESTONE
                || blockID == BLOCKID_MOSSYCOBBLESTONE

                || blockID == BLOCKID_FIREBRICKS
                || blockID == BLOCKID_STONEBRICKS
                || blockID == BLOCKID_WOODPLANKS
              //  || blockID == BLOCKID_GLOWBLOCK
                || blockID == BLOCKID_BLACK || blockID == BLOCKID_BLUE || blockID == BLOCKID_GREEN
                || blockID == BLOCKID_GREY || blockID == BLOCKID_ORANGE || blockID == BLOCKID_YELLOW
                || blockID == BLOCKID_WHITE || blockID == BLOCKID_RED || blockID == BLOCKID_TEAL)
            {
                return false;
            }
            else return true;
        }

        public static bool IsSelectable(byte blockID)
        {
            if (blockID == Block.BLOCKID_AIR || blockID == Block.BLOCKID_WATER || blockID == Block.BLOCKID_LAVA)
                return false;
            else
                return true;
        }

        public static bool IsDestructible(byte blockID)
        {
            if (blockID == BLOCKID_BEDROCK || blockID == BLOCKID_GLASSWALL)
                return false;
            else
                return true;
        }

        public static bool IsLiquid(byte blockID)
        {
            if (blockID == BLOCKID_WATER || blockID == BLOCKID_LAVA)
                return true;
            else
                return false;
        }

        public static bool IsLightSource(byte blockID)
        {
            if (blockID == BLOCKID_LAVA
                || blockID == BLOCKID_GLOWBLOCK)
                return true;
            else
                return false;
        }

        //value is per second
        public static int BlockHardness(byte blockID)
        {
            if (blockID == Block.BLOCKID_GRASS || blockID == Block.BLOCKID_PITFALLBLOCK)
                return 75;
            else if (blockID == Block.BLOCKID_COBBLE || blockID == Block.BLOCKID_STONEBRICKS || blockID == Block.BLOCKID_FIREBRICKS
                || blockID == BLOCKID_COBBLESTONE
                || blockID == BLOCKID_MOSSYCOBBLESTONE)
                return 175;
            else if (blockID == Block.BLOCKID_STONE)
                return 175;
            else if (blockID == Block.BLOCKID_SAND)
                return 55;
            else if (blockID == Block.BLOCKID_DIRT)
                return 75;
            else if (blockID == Block.BLOCKID_GOLD)
                return 600;
            else if (blockID == Block.BLOCKID_WOOD)
                return 125;
            else if (blockID == Block.BLOCKID_LEAF)
                return 50;
            else if (blockID == BLOCKID_BLACK || blockID == BLOCKID_BLUE || blockID == BLOCKID_GREEN
                || blockID == BLOCKID_GREY || blockID == BLOCKID_ORANGE || blockID == BLOCKID_YELLOW
                || blockID == BLOCKID_WHITE || blockID == BLOCKID_RED || blockID == BLOCKID_TEAL)
                return 50;
            else if (blockID == BLOCKID_GLOWBLOCK)
                return 100;
            else if (blockID == BLOCKID_GLASSUSEABLE)
                return 50;
            else if (blockID == BLOCKID_WOODPLANKS)
                return 110;
            else
                return 0;
        }

        public static bool IsGlass(byte blockID)
        {
            return blockID == BLOCKID_GLASSUSEABLE || blockID == BLOCKID_GLASSWALL;
        }

        public static bool IsBlockHard(byte blockID)
        {
            if (blockID == Block.BLOCKID_GRASS)
                return false;
            else if (blockID == Block.BLOCKID_COBBLE || blockID == Block.BLOCKID_STONEBRICKS || blockID == Block.BLOCKID_FIREBRICKS
                || blockID == BLOCKID_COBBLESTONE
                || blockID == BLOCKID_MOSSYCOBBLESTONE)
                return true;
            else if (blockID == Block.BLOCKID_PITFALLBLOCK)
                return false;
            else if (blockID == Block.BLOCKID_STONE)
                return true;
            else if (blockID == Block.BLOCKID_SAND)
                return false;
            else if (blockID == Block.BLOCKID_DIRT)
                return false;
            else if (blockID == BLOCKID_LEAF)
                return false;
            else if (blockID == BLOCKID_WOOD || blockID == BLOCKID_WOODPLANKS)
                return true;
            else if (blockID == BLOCKID_GLASSUSEABLE || blockID == BLOCKID_GLOWBLOCK)
                return true;
            else
                return false;
        }
    }
}
