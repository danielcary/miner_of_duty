using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game
{
    public abstract class Liquid
    {
        public byte X, Y, Z;
        public sbyte liquidLevel;
        private short delay;
        public static ITerrainOwner game;

        public Liquid(byte xPos, byte yPos, byte zPos, sbyte level)
        {
            X = xPos;
            Y = yPos;
            Z = zPos;
            delay = GetDelay();
            liquidLevel = level;
        }

        protected abstract short GetDelay();

        public void Update(short delta)
        {
            delay -= delta;
            if (delay <= 0)
            {
                if (liquidLevel < 10)
                    UpdateLiquid();
                Reproduce();
                delay = GetDelay();
            }
        }

        protected static readonly Vector3i[] dirs =
        {
            new Vector3i(1,0,0),
            new Vector3i(-1,0,0),
            new Vector3i(0,0,1),
            new Vector3i(0,0,-1),
            new Vector3i(0,1,0),
            new Vector3i(0,-1,0)
        };

        protected abstract void UpdateLiquid();

        private void Reproduce()
        {
            if (game == null)
                return;
            try
            {
                if (liquidLevel > 1)
                    for (int i = 0; i < 4; i++)//just first 4 dirs again
                    {
                        if (X + dirs[i].X >= 0 && X + dirs[i].X < Terrain.MAXWIDTH
                        && Z + dirs[i].Z >= 0 && Z + dirs[i].Z < Terrain.MAXDEPTH
                        && (Block.IsLiquid(game.GetTerrain.blocks[X + dirs[i].X, Y, Z + dirs[i].Z]) || game.GetTerrain.blocks[X + dirs[i].X, Y, Z + dirs[i].Z] == Block.BLOCKID_AIR)
                            && game.GetLiquidManager.IsSpaceOnGround(X, Y, Z))
                        {
                            if (this is Water)
                            {
                                if (game.GetTerrain.blocks[X + dirs[i].X, Y, Z + dirs[i].Z] == Block.BLOCKID_LAVA)
                                {
                                    game.GetLiquidManager.RemoveNow(X, Y, Z);
                                    game.GetTerrain.AddBlockToQueue(X, Y, Z, Block.BLOCKID_COBBLESTONE);
                                    return;
                                }
                                else if (game.GetTerrain.blocks[X + dirs[i].X, Y, Z + dirs[i].Z] == Block.BLOCKID_AIR)
                                    game.GetLiquidManager.AddWaterBlock(X + dirs[i].X, Y, Z + dirs[i].Z, liquidLevel - 1);
                            }
                            else
                            {
                                if (game.GetTerrain.blocks[X + dirs[i].X, Y, Z + dirs[i].Z] == Block.BLOCKID_WATER)
                                {
                                    game.GetLiquidManager.RemoveNow(X, Y, Z);
                                    game.GetTerrain.AddBlockToQueue(X, Y, Z, Block.BLOCKID_COBBLESTONE);
                                    return;
                                }
                                else if (game.GetTerrain.blocks[X + dirs[i].X, Y, Z + dirs[i].Z] == Block.BLOCKID_AIR)
                                    game.GetLiquidManager.AddLavaBlock(X + dirs[i].X, Y, Z + dirs[i].Z, liquidLevel - 1);
                            }
                        }
                    }

                if (Y - 1 >= 0) // is in bounds
                {
                    if (Block.IsLiquid(game.GetTerrain.blocks[X, Y - 1, Z]) || game.GetTerrain.blocks[X, Y - 1, Z] == Block.BLOCKID_AIR)
                    {
                        if (this is Water)
                        {
                            if (game.GetTerrain.blocks[X, Y - 1, Z] == Block.BLOCKID_LAVA)
                            {
                                game.GetLiquidManager.RemoveNow(X, Y, Z);
                                game.GetTerrain.AddBlockToQueue(X, Y, Z, Block.BLOCKID_COBBLESTONE);
                                return;
                            }
                            else if (game.GetTerrain.blocks[X, Y - 1, Z] == Block.BLOCKID_AIR)
                            {
                                if (liquidLevel == 10)
                                    game.GetLiquidManager.AddWaterBlock(X, Y - 1, Z, liquidLevel - 1);
                                else
                                    game.GetLiquidManager.AddWaterBlock(X, Y - 1, Z, liquidLevel);
                            }
                        }
                        else
                        {
                            if (game.GetTerrain.blocks[X, Y - 1, Z] == Block.BLOCKID_WATER)
                            {
                                game.GetLiquidManager.RemoveNow(X, Y, Z);
                                game.GetTerrain.AddBlockToQueue(X, Y, Z, Block.BLOCKID_COBBLESTONE);
                                return;
                            }
                            else if (game.GetTerrain.blocks[X, Y - 1, Z] == Block.BLOCKID_AIR)
                            {
                                if (liquidLevel == 10)
                                    game.GetLiquidManager.AddLavaBlock(X, Y - 1, Z, liquidLevel - 1);
                                else
                                    game.GetLiquidManager.AddLavaBlock(X, Y - 1, Z, liquidLevel);
                            }
                        }
                    }
                }
            }
            catch (NullReferenceException e)
            {
                if (game == null || game.GetTerrain == null || game.GetLiquidManager == null)
                    return;
                else
                    throw e;
            }
        }
        public abstract void GetFaces(bool[] faces);


        protected const float ONETHIRD = 1f / 3f;
        protected const float TWOTHIRD = 2f / 3f;
        protected static readonly Vector3 FFF = new Vector3(.5f, .5f, .5f); //top right
        protected static readonly Vector3 nFF = new Vector3(-.5f, .5f, .5f); //top left
        protected static readonly Vector3 nFn = new Vector3(-.5f, .5f, -.5f); //bottom left
        protected static readonly Vector3 FFn = new Vector3(.5f, .5f, -.5f); //bottom right

        //bases
        protected static readonly Vector3 FnF = new Vector3(.5f, -.5f, .5f);
        protected static readonly Vector3 Fnn = new Vector3(.5f, -.5f, -.5f);
        protected static readonly Vector3 nnn = new Vector3(-.5f, -.5f, -.5f);
        protected static readonly Vector3 nnF = new Vector3(-.5f, -.5f, .5f);
        public abstract void CreateLiquidBlock(ref int vert, VertexWaterTextureLight[] verts, bool[] faces);

    }

    public class Water : Liquid
    {

        public Water(byte xPos, byte yPos, byte zPos, sbyte level)
            : base(xPos, yPos, zPos, level)
        {
        }

        protected override short GetDelay()
        {
            return 200;
        }

        protected override void UpdateLiquid()
        {
            if (game == null)
                return;
            sbyte oldLevel = liquidLevel;
            bool ableToLive = false;
            bool foundSource = false; //this allows us to upgrade us to asource
            //north west south east
            for (int i = 0; i < 4; i++)//this is just the first four dirs
            {
                if (X + dirs[i].X >= 0 && X + dirs[i].X < Terrain.MAXWIDTH
                && Z + dirs[i].Z >= 0 && Z + dirs[i].Z < Terrain.MAXDEPTH
                && game.GetLiquidManager.IsSpaceOnGround((byte)(X + dirs[i].X), Y, (byte)(Z + dirs[i].Z))
                && game.GetTerrain.blocks[X + dirs[i].X, Y + dirs[i].Y, Z + dirs[i].Z] == Block.BLOCKID_WATER)
                {
                    if (game.GetLiquidManager.Liquids[X + dirs[i].X, Y, Z + dirs[i].Z].liquidLevel > liquidLevel)
                    {
                        ableToLive = true;
                        if (game.GetLiquidManager.Liquids[X + dirs[i].X, Y, Z + dirs[i].Z].liquidLevel == 10)
                        {
                            if (foundSource == false)
                                foundSource = true;
                            else
                            {
                                //ive found 2 sources next to me, im a source
                                liquidLevel = 10;
                                break;
                            }
                        }
                        liquidLevel = (sbyte)(game.GetLiquidManager.Liquids[X + dirs[i].X, Y, Z + dirs[i].Z].liquidLevel - 1);
                    }
                }
            }
            //up
            if (Y + 1 < Terrain.MAXHEIGHT)
            {
                if (game.GetTerrain.blocks[X, Y + 1, Z] == Block.BLOCKID_WATER)
                {
                    if (game.GetLiquidManager.Liquids[X + dirs[4].X, Y + dirs[4].Y, Z + dirs[4].Z].liquidLevel >= liquidLevel)
                    {
                        ableToLive = true;
                        if (game.GetLiquidManager.Liquids[X + dirs[4].X, Y + dirs[4].Y, Z + dirs[4].Z].liquidLevel != 10)
                            liquidLevel = game.GetLiquidManager.Liquids[X + dirs[4].X, Y + dirs[4].Y, Z + dirs[4].Z].liquidLevel;
                        else
                            liquidLevel = (sbyte)(game.GetLiquidManager.Liquids[X + dirs[4].X, Y + dirs[4].Y, Z + dirs[4].Z].liquidLevel - 1);
                    }//mark dirty if direction changed
                }
            }

            if (oldLevel != liquidLevel)
                game.GetLiquidManager.MarkWaterDirty(X, Y, Z);

            if (ableToLive == false)
                if (--liquidLevel <= 0)
                {
                    game.GetLiquidManager.RemoveNow(X, Y, Z);
                }

        }

        public override void GetFaces(bool[] faces)
        {

            if (Y + 1 < Terrain.MAXHEIGHT && game.GetTerrain.blocks[X, Y + 1, Z] == Block.BLOCKID_WATER)
                faces[0] = false;
            //  else if (liquidLevel == 10 && Y + 1 < OctTree.MAXHEIGHT && Block.IsBlockSolid(game.GetTerrain.blocks[X, Y + 1, Z]))
            //{
            //  faces[0] = false;
            //}
            else
                faces[0] = true;
            if (Y - 1 >= 0 && (game.GetTerrain.blocks[X, Y - 1, Z] == Block.BLOCKID_WATER || Block.IsBlockSolid(game.GetTerrain.blocks[X, Y - 1, Z])))
                faces[2] = false;
            else
                faces[2] = true;

            if (Y + 1 < Terrain.MAXHEIGHT && game.GetTerrain.blocks[X, Y + 1, Z] == Block.BLOCKID_WATER)
            {
                faces[3] = !(X + 1 < Terrain.MAXWIDTH && (Block.IsBlockSolid(game.GetTerrain.blocks[X + 1, Y, Z]) || (Y + 1 >= 0 && game.GetTerrain.blocks[X + 1, Y, Z] == Block.BLOCKID_WATER && game.GetTerrain.blocks[X + 1, Y + 1, Z] == Block.BLOCKID_WATER)));
                faces[4] = !(Z + 1 < Terrain.MAXWIDTH && (Block.IsBlockSolid(game.GetTerrain.blocks[X, Y, Z + 1]) || (Y + 1 >= 0 && game.GetTerrain.blocks[X, Y, Z + 1] == Block.BLOCKID_WATER && game.GetTerrain.blocks[X, Y + 1, Z + 1] == Block.BLOCKID_WATER)));
                faces[5] = !(Z - 1 >= 0 && (Block.IsBlockSolid(game.GetTerrain.blocks[X, Y, Z - 1]) || (Y + 1 >= 0 && game.GetTerrain.blocks[X, Y + 1, Z - 1] == Block.BLOCKID_WATER)));
                faces[1] = !(X - 1 >= 0 && (Block.IsBlockSolid(game.GetTerrain.blocks[X - 1, Y, Z]) || (Y + 1 >= 0 && game.GetTerrain.blocks[X - 1, Y + 1, Z] == Block.BLOCKID_WATER)));
            }
            else
            {
                if (X + 1 < Terrain.MAXWIDTH && (game.GetTerrain.blocks[X + 1, Y, Z] == Block.BLOCKID_WATER || Block.IsBlockSolid(game.GetTerrain.blocks[X + 1, Y, Z])))
                    faces[3] = false;
                else
                    faces[3] = true;
                if (X - 1 >= 0 && (game.GetTerrain.blocks[X - 1, Y, Z] == Block.BLOCKID_WATER || Block.IsBlockSolid(game.GetTerrain.blocks[X - 1, Y, Z])))
                    faces[1] = false;
                else
                    faces[1] = true;
                if (Z + 1 < Terrain.MAXWIDTH && (game.GetTerrain.blocks[X, Y, Z + 1] == Block.BLOCKID_WATER || Block.IsBlockSolid(game.GetTerrain.blocks[X, Y, Z + 1])))
                    faces[4] = false;
                else
                    faces[4] = true;
                if (Z - 1 >= 0 && (game.GetTerrain.blocks[X, Y, Z - 1] == Block.BLOCKID_WATER || Block.IsBlockSolid(game.GetTerrain.blocks[X, Y, Z - 1])))
                    faces[5] = false;
                else
                    faces[5] = true;
            }
        }

        public override void CreateLiquidBlock(ref int vert, VertexWaterTextureLight[] verts, bool[] faces)
        {
            //determine corner hieghtsd
            float topRight, topLeft, bottomRight, bottomLeft;
            if (Y + 1 >= 0 && Y + 1 <= 63 && game.GetTerrain.blocks[X, Y + 1, Z] == Block.BLOCKID_WATER)
            {
                topLeft = topRight = bottomRight = bottomLeft = 10;
            }
            else
            {
                short topLeftC = 1, topRightC = 1, bottomLeftC = 1, bottomRightC = 1;
                topLeft = topRight = bottomLeft = bottomRight = liquidLevel;

                if (Z + 1 < Terrain.MAXDEPTH && game.GetTerrain.blocks[X, Y, Z + 1] == Block.BLOCKID_WATER)
                {
                    topLeftC++;
                    topRightC++;
                    topLeft += game.GetLiquidManager.Liquids[X, Y, Z + 1].liquidLevel;
                    topRight += game.GetLiquidManager.Liquids[X, Y, Z + 1].liquidLevel;
                }

                if (Z + 1 < Terrain.MAXDEPTH && X + 1 < Terrain.MAXDEPTH && game.GetTerrain.blocks[X + 1, Y, Z + 1] == Block.BLOCKID_WATER)
                {
                    topRightC++;
                    topRight += game.GetLiquidManager.Liquids[X + 1, Y, Z + 1].liquidLevel;
                }

                if (Z - 1 >= 0 && game.GetTerrain.blocks[X, Y, Z - 1] == Block.BLOCKID_WATER)
                {
                    bottomLeftC++;
                    bottomRightC++;
                    bottomLeft += game.GetLiquidManager.Liquids[X, Y, Z - 1].liquidLevel;
                    bottomRight += game.GetLiquidManager.Liquids[X, Y, Z - 1].liquidLevel;
                }

                if (Z - 1 >= 0 && X + 1 < Terrain.MAXDEPTH && game.GetTerrain.blocks[X + 1, Y, Z - 1] == Block.BLOCKID_WATER)
                {
                    bottomRightC++;
                    bottomRight += game.GetLiquidManager.Liquids[X + 1, Y, Z - 1].liquidLevel;
                }

                if (X + 1 < Terrain.MAXWIDTH && game.GetTerrain.blocks[X + 1, Y, Z] == Block.BLOCKID_WATER)
                {
                    topRightC++;
                    bottomRightC++;
                    topRight += game.GetLiquidManager.Liquids[X + 1, Y, Z].liquidLevel;
                    bottomRight += game.GetLiquidManager.Liquids[X + 1, Y, Z].liquidLevel;
                }

                if (X - 1 >= 0 && game.GetTerrain.blocks[X - 1, Y, Z] == Block.BLOCKID_WATER)
                {
                    topLeftC++;
                    bottomLeftC++;
                    bottomLeft += game.GetLiquidManager.Liquids[X - 1, Y, Z].liquidLevel;
                    topLeft += game.GetLiquidManager.Liquids[X - 1, Y, Z].liquidLevel;
                }

                if (Z - 1 >= 0 && X - 1 >= 0 && game.GetTerrain.blocks[X - 1, Y, Z - 1] == Block.BLOCKID_WATER)
                {
                    bottomLeftC++;
                    bottomLeft += game.GetLiquidManager.Liquids[X - 1, Y, Z - 1].liquidLevel;
                }

                if (Z + 1 < Terrain.MAXDEPTH && X - 1 >= 0 && game.GetTerrain.blocks[X - 1, Y, Z + 1] == Block.BLOCKID_WATER)
                {
                    topLeftC++;
                    topLeft += game.GetLiquidManager.Liquids[X - 1, Y, Z + 1].liquidLevel;
                }


                topLeft /= topLeftC;
                topRight /= topRightC;
                bottomLeft /= bottomLeftC;
                bottomRight /= bottomRightC;

                topLeft -= .75f;
                topRight -= .75f;
                bottomLeft -= .75f;
                bottomRight -= .75f;
            }

            topLeft /= 10f;
            topRight /= 10f;
            bottomLeft /= 10f;
            bottomRight /= 10f;

            Vector3 topLeftV = new Vector3(0, 1 - topLeft, 0);
            Vector3 topRightV = new Vector3(0, 1 - topRight, 0);
            Vector3 bottomLeftV = new Vector3(0, 1 - bottomLeft, 0);
            Vector3 bottomRightV = new Vector3(0, 1 - bottomRight, 0);



            //face 1
            if (faces[0])
            {
                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 1);
                verts[vert].Position = nFF - topLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topLeft;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].Position = nFn - bottomLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomLeft;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].Position = nFn - bottomLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomLeft;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 1);
                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 1);
            }

            if (faces[1])
            {
                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(0, 1);

                verts[vert].Position = nFn - bottomLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomLeft;
                verts[vert++].TextureUV = new Vector2(0, .5f);
                verts[vert].Position = nFF - topLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topLeft;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].Position = nFF - topLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topLeft;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);

                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 1);
                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(0, 1);
            }

            if (faces[2])
            {
                verts[vert].Position = Fnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 0);
                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 0);
                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 0);
                verts[vert].Position = FnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].Position = Fnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);
            }

            if (faces[3])
            {
                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);

                verts[vert].Position = Fnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 0);
                verts[vert].Position = FnF + new Vector3(X, Y, Z);//5
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(1, 0);
                verts[vert].Position = FnF + new Vector3(X, Y, Z);//5
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(1, 0);

                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(1, .5f);
                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
            }

            if (faces[4])
            {
                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(1, 1);

                verts[vert].Position = FnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 1);
                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);

                verts[vert].Position = nFF - topLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topLeft;
                verts[vert++].TextureUV = new Vector2(1, .5f);
                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(1, 1);
            }

            if (faces[5])
            {
                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(0, .5f);
                verts[vert].Position = nFn - bottomLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomLeft;
                verts[vert++].TextureUV = new Vector2(0, 0);

                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 0);
                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 0);
                verts[vert].Position = Fnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);

                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(0, .5f);
            }
        }

    }

    public class Lava : Liquid
    {

        public Lava(byte xPos, byte yPos, byte zPos, sbyte level)
            : base(xPos, yPos, zPos, level)
        {
        }

        protected override short GetDelay()
        {
            return 750;
        }

        protected override void UpdateLiquid()
        {
            if (game == null)
                return;
            sbyte oldLevel = liquidLevel;
            bool ableToLive = false;
            bool foundSource = false; //this allows us to upgrade us to asource
            //north west south east
            for (int i = 0; i < 4; i++)//this is just the first four dirs
            {
                if (X + dirs[i].X >= 0 && X + dirs[i].X < Terrain.MAXWIDTH
                && Z + dirs[i].Z >= 0 && Z + dirs[i].Z < Terrain.MAXDEPTH
                && game.GetLiquidManager.IsSpaceOnGround((byte)(X + dirs[i].X), Y, (byte)(Z + dirs[i].Z))
                && game.GetTerrain.blocks[X + dirs[i].X, Y + dirs[i].Y, Z + dirs[i].Z] == Block.BLOCKID_LAVA)
                {
                    if (game.GetLiquidManager.Liquids[X + dirs[i].X, Y, Z + dirs[i].Z].liquidLevel > liquidLevel)
                    {
                        ableToLive = true;
                        if (game.GetLiquidManager.Liquids[X + dirs[i].X, Y, Z + dirs[i].Z].liquidLevel == 10)
                        {
                            if (foundSource == false)
                                foundSource = true;
                            else
                            {
                                //ive found 2 sources next to me, im a source
                                liquidLevel = 10;
                                break;
                            }
                        }
                        liquidLevel = (sbyte)(game.GetLiquidManager.Liquids[X + dirs[i].X, Y, Z + dirs[i].Z].liquidLevel - 1);
                    }
                }
            }
            //up
            if (Y + 1 < Terrain.MAXHEIGHT)
            {
                if (game.GetTerrain.blocks[X, Y + 1, Z] == Block.BLOCKID_LAVA)
                {
                    if (game.GetLiquidManager.Liquids[X + dirs[4].X, Y + dirs[4].Y, Z + dirs[4].Z].liquidLevel >= liquidLevel)
                    {
                        ableToLive = true;
                        if (game.GetLiquidManager.Liquids[X + dirs[4].X, Y + dirs[4].Y, Z + dirs[4].Z].liquidLevel != 10)
                            liquidLevel = game.GetLiquidManager.Liquids[X + dirs[4].X, Y + dirs[4].Y, Z + dirs[4].Z].liquidLevel;
                        else
                            liquidLevel = (sbyte)(game.GetLiquidManager.Liquids[X + dirs[4].X, Y + dirs[4].Y, Z + dirs[4].Z].liquidLevel - 1);
                    }//mark dirty if direction changed
                }
            }

            if (oldLevel != liquidLevel)
                game.GetLiquidManager.MarkLavaDirty(X, Y, Z);

            if (ableToLive == false)
                if (--liquidLevel <= 0)
                {
                    game.GetLiquidManager.RemoveNow(X, Y, Z);
                }

        }

        public override void GetFaces(bool[] faces)
        {

            if (Y + 1 < Terrain.MAXHEIGHT && game.GetTerrain.blocks[X, Y + 1, Z] == Block.BLOCKID_LAVA)
                faces[0] = false;
            else
                faces[0] = true;
            if (Y - 1 >= 0 && (game.GetTerrain.blocks[X, Y - 1, Z] == Block.BLOCKID_LAVA || Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X, Y - 1, Z])))
                faces[2] = false;
            else
                faces[2] = true;

            if (Y + 1 < Terrain.MAXHEIGHT && game.GetTerrain.blocks[X, Y + 1, Z] == Block.BLOCKID_LAVA)
            {
                faces[3] = !(X + 1 < Terrain.MAXWIDTH && (Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X + 1, Y, Z]) || (Y + 1 >= 0 && game.GetTerrain.blocks[X + 1, Y, Z] == Block.BLOCKID_LAVA && game.GetTerrain.blocks[X + 1, Y + 1, Z] == Block.BLOCKID_LAVA)));
                faces[4] = !(Z + 1 < Terrain.MAXWIDTH && (Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X, Y, Z + 1]) || (Y + 1 >= 0 && game.GetTerrain.blocks[X, Y, Z + 1] == Block.BLOCKID_LAVA && game.GetTerrain.blocks[X, Y + 1, Z + 1] == Block.BLOCKID_LAVA)));
                faces[5] = !(Z - 1 >= 0 && (Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X, Y, Z - 1]) || (Y + 1 >= 0 && game.GetTerrain.blocks[X, Y + 1, Z - 1] == Block.BLOCKID_LAVA)));
                faces[1] = !(X - 1 >= 0 && (Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X - 1, Y, Z]) || (Y + 1 >= 0 && game.GetTerrain.blocks[X - 1, Y + 1, Z] == Block.BLOCKID_LAVA)));
            }
            else
            {
                if (X + 1 < Terrain.MAXWIDTH && (game.GetTerrain.blocks[X + 1, Y, Z] == Block.BLOCKID_LAVA || Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X + 1, Y, Z])))
                    faces[3] = false;
                else
                    faces[3] = true;
                if (X - 1 >= 0 && (game.GetTerrain.blocks[X - 1, Y, Z] == Block.BLOCKID_LAVA || Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X - 1, Y, Z])))
                    faces[1] = false;
                else
                    faces[1] = true;
                if (Z + 1 < Terrain.MAXWIDTH && (game.GetTerrain.blocks[X, Y, Z + 1] == Block.BLOCKID_LAVA || Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X, Y, Z + 1])))
                    faces[4] = false;
                else
                    faces[4] = true;
                if (Z - 1 >= 0 && (game.GetTerrain.blocks[X, Y, Z - 1] == Block.BLOCKID_LAVA || Block.IsBlockSolidNoGlass(game.GetTerrain.blocks[X, Y, Z - 1])))
                    faces[5] = false;
                else
                    faces[5] = true;
            }
        }

        public override void CreateLiquidBlock(ref int vert, VertexWaterTextureLight[] verts, bool[] faces)
        {
            //determine corner hieghtsd
            float topRight, topLeft, bottomRight, bottomLeft;
            if (Y + 1 >= 0  && Y + 1 <= 63 && game.GetTerrain.blocks[X, Y + 1, Z] == Block.BLOCKID_LAVA)
            {
                topLeft = topRight = bottomRight = bottomLeft = 10;
            }
            else
            {
                short topLeftC = 1, topRightC = 1, bottomLeftC = 1, bottomRightC = 1;
                topLeft = topRight = bottomLeft = bottomRight = liquidLevel;

                if (Z + 1 < Terrain.MAXDEPTH && game.GetTerrain.blocks[X, Y, Z + 1] == Block.BLOCKID_LAVA)
                {
                    topLeftC++;
                    topRightC++;
                    topLeft += game.GetLiquidManager.Liquids[X, Y, Z + 1].liquidLevel;
                    topRight += game.GetLiquidManager.Liquids[X, Y, Z + 1].liquidLevel;
                }

                if (Z + 1 < Terrain.MAXDEPTH && X + 1 < Terrain.MAXDEPTH && game.GetTerrain.blocks[X + 1, Y, Z + 1] == Block.BLOCKID_LAVA)
                {
                    topRightC++;
                    topRight += game.GetLiquidManager.Liquids[X + 1, Y, Z + 1].liquidLevel;
                }

                if (Z - 1 >= 0 && game.GetTerrain.blocks[X, Y, Z - 1] == Block.BLOCKID_LAVA)
                {
                    bottomLeftC++;
                    bottomRightC++;
                    bottomLeft += game.GetLiquidManager.Liquids[X, Y, Z - 1].liquidLevel;
                    bottomRight += game.GetLiquidManager.Liquids[X, Y, Z - 1].liquidLevel;
                }

                if (Z - 1 >= 0 && X + 1 < Terrain.MAXDEPTH && game.GetTerrain.blocks[X + 1, Y, Z - 1] == Block.BLOCKID_LAVA)
                {
                    bottomRightC++;
                    bottomRight += game.GetLiquidManager.Liquids[X + 1, Y, Z - 1].liquidLevel;
                }

                if (X + 1 < Terrain.MAXWIDTH && game.GetTerrain.blocks[X + 1, Y, Z] == Block.BLOCKID_LAVA)
                {
                    topRightC++;
                    bottomRightC++;
                    topRight += game.GetLiquidManager.Liquids[X + 1, Y, Z].liquidLevel;
                    bottomRight += game.GetLiquidManager.Liquids[X + 1, Y, Z].liquidLevel;
                }

                if (X - 1 >= 0 && game.GetTerrain.blocks[X - 1, Y, Z] == Block.BLOCKID_LAVA)
                {
                    topLeftC++;
                    bottomLeftC++;
                    bottomLeft += game.GetLiquidManager.Liquids[X - 1, Y, Z].liquidLevel;
                    topLeft += game.GetLiquidManager.Liquids[X - 1, Y, Z].liquidLevel;
                }

                if (Z - 1 >= 0 && X - 1 >= 0 && game.GetTerrain.blocks[X - 1, Y, Z - 1] == Block.BLOCKID_LAVA)
                {
                    bottomLeftC++;
                    bottomLeft += game.GetLiquidManager.Liquids[X - 1, Y, Z - 1].liquidLevel;
                }

                if (Z + 1 < Terrain.MAXDEPTH && X - 1 >= 0 && game.GetTerrain.blocks[X - 1, Y, Z + 1] == Block.BLOCKID_LAVA)
                {
                    topLeftC++;
                    topLeft += game.GetLiquidManager.Liquids[X - 1, Y, Z + 1].liquidLevel;
                }


                topLeft /= topLeftC;
                topRight /= topRightC;
                bottomLeft /= bottomLeftC;
                bottomRight /= bottomRightC;

                topLeft -= .75f;
                topRight -= .75f;
                bottomLeft -= .75f;
                bottomRight -= .75f;
            }

            topLeft /= 10f;
            topRight /= 10f;
            bottomLeft /= 10f;
            bottomRight /= 10f;

            Vector3 topLeftV = new Vector3(0, 1 - topLeft, 0);
            Vector3 topRightV = new Vector3(0, 1 - topRight, 0);
            Vector3 bottomLeftV = new Vector3(0, 1 - bottomLeft, 0);
            Vector3 bottomRightV = new Vector3(0, 1 - bottomRight, 0);



            //face 1
            if (faces[0] && vert + 5 < verts.Length)
            {
                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 1);
                verts[vert].Position = nFF - topLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topLeft;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].Position = nFn - bottomLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomLeft;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].Position = nFn - bottomLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomLeft;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 1);
                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 1);
            }

            if (faces[1] && vert + 5 < verts.Length)
            {
                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(0, 1);

                verts[vert].Position = nFn - bottomLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomLeft;
                verts[vert++].TextureUV = new Vector2(0, .5f);
                verts[vert].Position = nFF - topLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topLeft;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].Position = nFF - topLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topLeft;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);

                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 1);
                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(0, 1);
            }

            if (faces[2] && vert + 5 < verts.Length)
            {
                verts[vert].Position = Fnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);
                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 0);
                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 0);
                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 0);
                verts[vert].Position = FnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].Position = Fnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);
            }

            if (faces[3] && vert + 5 < verts.Length)
            {
                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);

                verts[vert].Position = Fnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 0);
                verts[vert].Position = FnF + new Vector3(X, Y, Z);//5
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(1, 0);
                verts[vert].Position = FnF + new Vector3(X, Y, Z);//5
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(1, 0);

                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(1, .5f);
                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
            }

            if (faces[4] && vert + 5 < verts.Length)
            {
                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(1, 1);

                verts[vert].Position = FnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, 1);
                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);
                verts[vert].Position = nnF + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(TWOTHIRD, .5f);

                verts[vert].Position = nFF - topLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topLeft;
                verts[vert++].TextureUV = new Vector2(1, .5f);
                verts[vert].Position = FFF - topRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - topRight;
                verts[vert++].TextureUV = new Vector2(1, 1);
            }

            if (faces[5] && vert + 5 < verts.Length)
            {
                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(0, .5f);
                verts[vert].Position = nFn - bottomLeftV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomLeft;
                verts[vert++].TextureUV = new Vector2(0, 0);

                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 0);
                verts[vert].Position = nnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, 0);
                verts[vert].Position = Fnn + new Vector3(X, Y, Z);
                verts[vert].Light = 1;
                verts[vert++].TextureUV = new Vector2(ONETHIRD, .5f);

                verts[vert].Position = FFn - bottomRightV + new Vector3(X, Y, Z);
                verts[vert].Light = 1 - bottomRight;
                verts[vert++].TextureUV = new Vector2(0, .5f);
            }
        }
    }
}
