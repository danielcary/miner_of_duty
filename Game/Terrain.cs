using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using Microsoft.Xna.Framework;
using System.IO;
using Miner_Of_Duty.Game.Editor;
using Miner_Of_Duty.Game.Networking;

namespace Miner_Of_Duty.Game
{
    public class Terrain
    {
        public BoundingBox blockBox = new BoundingBox(new Vector3(-.5f, -.5f, -.5f), new Vector3(.5f, .5f, .5f));

        public const int MAXWIDTH = 128, MAXHEIGHT = 64, MAXDEPTH = 128;
        public readonly object EditBlocksLock = new object();
        public delegate void BlockChanged(int x, int y, int z);
        public event BlockChanged BlockChangedEvent;
        public static byte[, ,] blockss; 
        public byte[, ,] blocks { get { return blockss; } set { blockss = value; } }
        private ITerrainOwner game;
        private GraphicsDevice gd;

        private QuadTreeNode parentNode;
        private NonPlayableTerrain npt;

        private bool Initializing = true;

        public List<PitfallBlock> pitfallBlocks = new List<PitfallBlock>();

        public Terrain(ITerrainOwner game)
        {
            this.game = game;
        }

        public void Dispose()
        {
            parentNode.Dispose();
            npt.Dispose();  
        }

        public int Size = 0;

        public void Initialize(GraphicsDevice gd, CreateBlocksDelgate createBlocks, int size, bool genTrees)
        {
            Size = size;
            createBlocks.Invoke(gd, this, true, size, genTrees);
            this.gd = gd;
            parentNode = new QuadTreeNode(0, 0, 0, 128, this);
            parentNode.InitializeBuffers(gd);
        }


        public delegate void CreateBlocksDelgate(GraphicsDevice gd, Terrain t, bool liquid, int size, bool genTrees);

        #region Is Block Visible


        public static Vector3i[] directions =
        {
            new Vector3i(0,1,0),
            new Vector3i(-1,0,0),
            new Vector3i(0,-1,0),
            new Vector3i(1,0,0),
            new Vector3i(0,0,1),
            new Vector3i(0,0,-1),    
        };// DONT MESS UP ORDER

        private static Vector3i[] collisiondirs =
        {
            new Vector3i(1,0,0),
            new Vector3i(-1,0,0),
            
            new Vector3i(0,0,-1),
            new Vector3i(0,0,1),

            new Vector3i(1,0,1),
            new Vector3i(-1,0,1),
            new Vector3i(1,0,-1),
            new Vector3i(-1,0,-1),

            new Vector3i(0,1,0),
            new Vector3i(0,-1,0),
        };

        public bool isBlockVisible(int x, int y, int z)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                if (directions[i].X + x >= 0 && directions[i].X + x < MAXWIDTH
                    && directions[i].Y + y >= 0 && directions[i].Y + y < MAXHEIGHT
                    && directions[i].Z + z >= 0 && directions[i].Z + z < MAXDEPTH)
                {
                    if (!Block.IsBlockSolidNoGlass(blocks[directions[i].X + x, directions[i].Y + y, directions[i].Z + z]))
                        return true;
                }
            }
            return false;
        }

        public void GetFaces(int x, int y, int z, bool[] faces)
        {
            if (x + 1 < Terrain.MAXWIDTH && !Block.IsBlockSolid(blocks[x + 1, y, z]))
                faces[3] = true;
            else
                faces[3] = false;

            if (x - 1 >= 0 && !Block.IsBlockSolid(blocks[x - 1, y, z]))
                faces[1] = true;
            else
                faces[1] = false;

            if (y + 1 < Terrain.MAXHEIGHT && !Block.IsBlockSolid(blocks[x, y + 1, z]))
                faces[0] = true;
            else if (y + 1 < Terrain.MAXHEIGHT)
                faces[0] = false;
            else
                faces[0] = true;

            if (y - 1 >= 0 && !Block.IsBlockSolid(blocks[x, y - 1, z]))
                faces[2] = true;
            else if (y - 1 >= 0)
                faces[2] = false;
            else
                faces[2] = true;

            if (z + 1 < Terrain.MAXDEPTH && !Block.IsBlockSolid(blocks[x, y, z + 1]))
                faces[4] = true;
            else
                faces[4] = false;

            if (z - 1 >= 0 && !Block.IsBlockSolid(blocks[x, y, z - 1]))
                faces[5] = true;
            else
                faces[5] = false;

        }

        public void GetFacesNoGlass(int x, int y, int z, bool[] faces)
        {
            if (x + 1 < Terrain.MAXWIDTH && !Block.IsBlockSolidNoGlass(blocks[x + 1, y, z]))
                faces[3] = true;
            else
                faces[3] = false;

            if (x - 1 >= 0 && !Block.IsBlockSolidNoGlass(blocks[x - 1, y, z]))
                faces[1] = true;
            else
                faces[1] = false;

            if (y + 1 < Terrain.MAXHEIGHT && !Block.IsBlockSolidNoGlass(blocks[x, y + 1, z]))
                faces[0] = true;
            else if (y + 1 < Terrain.MAXHEIGHT)
                faces[0] = false;
            else
                faces[0] = true;

            if (y - 1 >= 0 && !Block.IsBlockSolidNoGlass(blocks[x, y - 1, z]))
                faces[2] = true;
            else if (y - 1 >= 0)
                faces[2] = false;
            else
                faces[2] = true;

            if (z + 1 < Terrain.MAXDEPTH && !Block.IsBlockSolidNoGlass(blocks[x, y, z + 1]))
                faces[4] = true;
            else
                faces[4] = false;

            if (z - 1 >= 0 && !Block.IsBlockSolidNoGlass(blocks[x, y, z - 1]))
                faces[5] = true;
            else
                faces[5] = false;

        }

        private byte tempByte;
        public void GetFacesGlass(int x, int y, int z, bool[] faces)
        {
            if (x + 1 < Terrain.MAXWIDTH && (tempByte = blocks[x + 1, y, z]) >= 0 && (tempByte == Block.BLOCKID_AIR || tempByte == Block.BLOCKID_LAVA || tempByte == Block.BLOCKID_WATER))
                faces[3] = true;
            else
                faces[3] = false;

            if (x - 1 >= 0 && (tempByte = blocks[x - 1, y, z]) >= 0 && (tempByte == Block.BLOCKID_AIR || tempByte == Block.BLOCKID_LAVA || tempByte == Block.BLOCKID_WATER))
                faces[1] = true;
            else
                faces[1] = false;

            if (y + 1 < Terrain.MAXHEIGHT && (tempByte = blocks[x, y + 1, z]) >= 0 && (tempByte == Block.BLOCKID_AIR || tempByte == Block.BLOCKID_LAVA || tempByte == Block.BLOCKID_WATER))
                faces[0] = true;
            else if (y + 1 < Terrain.MAXHEIGHT)
                faces[0] = false;
            else
                faces[0] = true;

            if (y - 1 >= 0 && (tempByte = blocks[x, y - 1, z]) >= 0 && (tempByte == Block.BLOCKID_AIR || tempByte == Block.BLOCKID_LAVA || tempByte == Block.BLOCKID_WATER))
                faces[2] = true;
            else if (y - 1 >= 0)
                faces[2] = false;
            else
                faces[2] = true;

            if (z + 1 < Terrain.MAXDEPTH && (tempByte = blocks[x, y, z + 1]) >= 0 && (tempByte == Block.BLOCKID_AIR || tempByte == Block.BLOCKID_LAVA || tempByte == Block.BLOCKID_WATER))
                faces[4] = true;
            else
                faces[4] = false;

           
            if (z - 1 >= 0 && (tempByte = blocks[x, y, z - 1]) >= 0 && (tempByte == Block.BLOCKID_AIR || tempByte == Block.BLOCKID_LAVA || tempByte == Block.BLOCKID_WATER))
                faces[5] = true;
            else
                faces[5] = false;

        }


        #endregion

        private ContainmentType underResult;
        public enum UnderLiquid : byte { False, Lava, Water }
        public UnderLiquid IsUnderLiquid(ref Vector3 pos)
        {
            int minX = (int)pos.X - 2;
            if (minX < 0)
                minX = 0;
            int minY = (int)pos.Y - 2;
            if (minY < 0)
                minY = 0;
            int minZ = (int)pos.Z - 2;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)pos.X + 2;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            int maxY = (int)pos.Y + 2;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            int maxZ = (int)pos.Z + 2;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;

            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (Block.IsLiquid(blocks[x, y, z]))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            blockBox.Contains(ref pos, out underResult);
                            if (underResult == ContainmentType.Contains && game.GetLiquidManager.Liquids[x, y, z].liquidLevel > 5)
                                return blocks[x, y, z] == Block.BLOCKID_WATER ? UnderLiquid.Water : UnderLiquid.Lava;
                        }
                    }

            return UnderLiquid.False;
        }

        public UnderLiquid IsInLiquid(ref BoundingBox box)
        {
            int minX = (int)box.Min.X - 1;
            if (minX < 0)
                minX = 0;
            int minY = (int)box.Min.Y - 1;
            if (minY < 0)
                minY = 0;
            int minZ = (int)box.Min.Z - 1;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)box.Max.X + 1;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            int maxY = (int)box.Max.Y + 1;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            int maxZ = (int)box.Max.Z + 1;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;

            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (Block.IsLiquid(blocks[x, y, z]))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            blockBox.Contains(ref box, out underResult);
                            if (underResult == ContainmentType.Intersects)
                                return blocks[x, y, z] == Block.BLOCKID_WATER ? UnderLiquid.Water : UnderLiquid.Lava;
                        }
                    }

            return UnderLiquid.False;
        }

        private Queue<Vector4i> blocksToAdd = new Queue<Vector4i>();
        private readonly object blocksToAddLock = new object();

        public void RemoveBlock(int x, int y, int z)
        {
            if (x < 0 || x >= MAXWIDTH)
                return;
            if (y < 0 || y >= MAXHEIGHT)
                return;
            if (z < 0 || z >= MAXDEPTH)
                return;

            for (int i = 0; i < pitfallBlocks.Count; i++)
            {
                if (pitfallBlocks[i].X == x && pitfallBlocks[i].Y == y && pitfallBlocks[i].Z == z)
                {
                    pitfallBlocks.RemoveAt(i);
                    break;
                }
            }

            parentNode.RemoveBlock(x, y, z);
            game.GetLiquidManager.BlockAddedOrRemoved(x, y, z);
            Thread t = new Thread(poop => game.GetLightingManager.BlockAddedOrRemoved(x, y, z));
            t.IsBackground = true;
            t.Start();
        }

        public void AddBlocks(int x, int y, int z, byte id)
        {
            if (x < 0 || x >= MAXWIDTH)
                return;
            if (y < 0 || y >= MAXHEIGHT)
                return;
            if (z < 0 || z >= MAXDEPTH)
                return;
            if (blocks[x, y, z] == Block.BLOCKID_WATER)
            {
                blocks[x, y, z] = id;
                game.GetLiquidManager.RemoveWaterBlock(x, y, z);
            }
            else if (blocks[x, y, z] == Block.BLOCKID_LAVA)
            {
                blocks[x, y, z] = id;
                game.GetLiquidManager.RemoveLavaBlock(x, y, z);
            }
            else
            {
                blocks[x, y, z] = id;
                game.GetLiquidManager.BlockAddedOrRemoved(x, y, z);
            }

            if (id == Block.BLOCKID_PITFALLBLOCK)
            {
                pitfallBlocks.Add(new PitfallBlock()
                    {
                        X = (byte)x,
                        Y = (byte)y,
                        Z = (byte)z
                    });
            }

            parentNode.AddBlock(x, y, z, id);
            game.GetLightingManager.AddRemoveLightSource(x, y, z);

            if (BlockChangedEvent != null)
                BlockChangedEvent.Invoke(x, y, z);
        }

        public void RemoveBlocks(int x, int y, int z)
        {
            if (x < 0 || x >= MAXWIDTH)
                return;
            if (y < 0 || y >= MAXHEIGHT)
                return;
            if (z < 0 || z >= MAXDEPTH)
                return;


            for(int i = 0; i < pitfallBlocks.Count; i++)
            {
                if (pitfallBlocks[i].X == x && pitfallBlocks[i].Y == y && pitfallBlocks[i].Z == z)
                {
                    pitfallBlocks.RemoveAt(i);
                    break;
                }
            }

            parentNode.RemoveBlock(x, y, z);
            game.GetLiquidManager.BlockAddedOrRemoved(x, y, z);
            game.GetLightingManager.AddRemoveLightSource(x, y, z);
        }

        public void AddBlock(int x, int y, int z, byte id)
        {
            if (x < 0 || x >= MAXWIDTH)
                return;
            if (y < 0 || y >= MAXHEIGHT)
                return;
            if (z < 0 || z >= MAXDEPTH)
                return;
            if (blocks[x, y, z] == Block.BLOCKID_WATER)
            {
                blocks[x, y, z] = id;
                game.GetLiquidManager.RemoveWaterBlock(x, y, z);
            }
            else if (blocks[x, y, z] == Block.BLOCKID_LAVA)
            {
                blocks[x, y, z] = id;
                game.GetLiquidManager.RemoveLavaBlock(x, y, z);
            }
            else
            {
                blocks[x, y, z] = id;
                game.GetLiquidManager.BlockAddedOrRemoved(x, y, z);
            }

            if (id == Block.BLOCKID_PITFALLBLOCK)
            {
                pitfallBlocks.Add(new PitfallBlock()
                {
                    X = (byte)x,
                    Y = (byte)y,
                    Z = (byte)z
                });
            }


            parentNode.AddBlock(x, y, z, id);
            Thread t = new Thread(poop => game.GetLightingManager.BlockAddedOrRemoved(x, y, z));
            t.IsBackground = true;
            t.Start();


            if (BlockChangedEvent != null)
                BlockChangedEvent.Invoke(x, y, z);
        }

        public void MarkDirty(int x, int y, int z)
        {
            if (x < 0 || x >= MAXWIDTH)
                return;
            if (y < 0 || y >= MAXHEIGHT)
                return;
            if (z < 0 || z >= MAXDEPTH)
                return;
            parentNode.MarkDirty(x, y, z);

        }

        public void AddBlockToQueue(int X, int Y, int Z, byte id)
        {
            lock (blocksToAddLock)
            {
                blocksToAdd.Enqueue(new Vector4i(X, Y, Z, id));
            }
        }

        public void Update()
        {
            lock (EditBlocksLock)
            {
                lock (blocksToAddLock)
                {
                    while (blocksToAdd.Count > 0)
                    {
                        Vector4i b = blocksToAdd.Dequeue();
                        AddBlock(b.X, b.Y, b.Z, (byte)b.W);
                    }
                }
                OctTreeNode.DidSomeOneGetIt = false;
                parentNode.Update();
            }
        }

        public bool CheckForPitFall(ref BoundingBox box, ref Vector3 pos, ref Vector3 distanceFromGround, float timeInMilli)
        {
            int minX = (int)pos.X - 2;
            if (minX < 0)
                minX = 0;
            int minY = (int)pos.Y - 2;
            if (minY < 0)
                minY = 0;
            int minZ = (int)pos.Z - 2;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)pos.X + 2;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            int maxY = (int)pos.Y + 1;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            int maxZ = (int)pos.Z + 2;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;
            Vector3i blockPos;
            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (blocks[x,y,z] == Block.BLOCKID_PITFALLBLOCK)
                        {
                            blockPos.X = x;
                            blockPos.Y = y;
                            blockPos.Z = z;
                            if (PitfallBlock.IsPlayerOnBlock(ref blockPos, ref pos, ref distanceFromGround))
                            {
                                for (int i = 0; i < pitfallBlocks.Count; i++)
                                {
                                    if (pitfallBlocks[i].X == x && pitfallBlocks[i].Y == y && pitfallBlocks[i].Z == z)
                                    {
                                        if (pitfallBlocks[i].AddStandingTime(timeInMilli))
                                        {
                                        //    RemoveBlocks(x, y, z);
                                            Vector3 v = new Vector3(x,y,z);
                                            game.BlockChanged(ref v, Block.BLOCKID_AIR, false);
                                            Packet.WritePitfallBroke(MinerOfDuty.Session.LocalGamers[0], (byte)x, (byte)y, (byte)z);
                                        }
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < pitfallBlocks.Count; i++)
                                {
                                    if (pitfallBlocks[i].X == x && pitfallBlocks[i].Y == y && pitfallBlocks[i].Z == z)
                                    {
                                        pitfallBlocks[i].Reset();
                                        break;
                                    }
                                }
                            }
                        }
                    }

            return false;
        }

        public bool CheckForCollision(ref BoundingBox box, ref Vector3 pos)
        {
            int minX = (int)pos.X - 3;
            if (minX < 0)
                minX = 0;
            int minY = (int)pos.Y - 3;
            if (minY < 0)
                minY = 0;
            int minZ = (int)pos.Z - 3;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)pos.X + 3;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            int maxY = (int)pos.Y + 3;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            int maxZ = (int)pos.Z + 3;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;

            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (Block.IsBlockSolid(blocks[x, y, z]))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            if (blockBox.Contains(box) != ContainmentType.Disjoint)
                                return true;
                        }
                    }

            return false;
        }

        public bool CheckForCollisionZombie(ref BoundingBox box, ref Vector3 pos)
        {
            int minX = (int)pos.X - 1;
            if (minX < 0)
                minX = 0;
            int minY = (int)pos.Y - 1;
            if (minY < 0)
                minY = 0;
            int minZ = (int)pos.Z - 1;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)pos.X + 2;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            int maxY = (int)pos.Y + 2;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            int maxZ = (int)pos.Z + 2;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;

            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (Block.IsBlockSolid(blocks[x, y, z]))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            if (blockBox.Contains(box) != ContainmentType.Disjoint)
                                return true;
                        }
                    }

            return false;
        }

        public bool CheckForCollisionZombie(ref BoundingBox box, ref Vector3 pos, ref Vector3 target)
        {
            int minX = (int)pos.X - 1;
            if (minX < 0)
                minX = 0;
            int minY = (int)pos.Y - 1;
            if (minY < 0)
                minY = 0;
            int minZ = (int)pos.Z - 1;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)pos.X + 2;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            int maxY = (int)pos.Y + 2;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            int maxZ = (int)pos.Z + 2;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;

            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (Block.IsBlockSolid(blocks[x, y, z]))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            if (blockBox.Contains(box) != ContainmentType.Disjoint)
                            {
                                target.X = x;
                                target.Y = y;
                                target.Z = z;
                                return true;
                            }
                        }
                    }

            return false;
        }

        public bool CheckForCollisionGrenade(ref BoundingBox box, ref Vector3 pos)
        {
            int minX = (int)pos.X - 1;
            if (minX < 0)
                minX = 0;
            int minY = (int)pos.Y - 1;
            if (minY < 0)
                minY = 0;
            int minZ = (int)pos.Z - 1;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)pos.X + 2;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            int maxY = (int)pos.Y + 2;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            int maxZ = (int)pos.Z + 2;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;

            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (Block.IsBlockSolid(blocks[x, y, z]))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            if (blockBox.Contains(box) != ContainmentType.Disjoint)
                                return true;
                        }
                    }

            return false;
        }

        private const int MAXLENGTH = 6;
        public void GetSelectedBlock(ref Ray ray, out Vector3i selected, bool wantLava)
        {
            int minX = (int)ray.Position.X - MAXLENGTH;
            int minY = (int)ray.Position.Y - MAXLENGTH;
            int minZ = (int)ray.Position.Z - MAXLENGTH;
            if (minX < 0)
                minX = 0;
            if (minY < 0)
                minY = 0;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)ray.Position.X + MAXLENGTH;
            int maxY = (int)ray.Position.Y + MAXLENGTH;
            int maxZ = (int)ray.Position.Z + MAXLENGTH;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;
            float? result = null, newResult;
            selected.X = -1;
            selected.Y = -1;
            selected.Z = -1;
            Liquid l;
            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (wantLava && (Block.IsSelectable(blocks[x, y, z]) 
                            || ( (blocks[x, y, z] == Block.BLOCKID_LAVA || blocks[x, y, z] == Block.BLOCKID_WATER) &&
                            ( (l = game.GetLiquidManager.Liquids[x, y, z]) != null ? l.liquidLevel == 10 : false))
                            ) && isBlockVisible(x, y, z))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            blockBox.Intersects(ref ray, out newResult);
                            if (newResult.HasValue)
                                if (!result.HasValue)
                                {
                                    selected.X = x;
                                    selected.Y = y;
                                    selected.Z = z;
                                    result = newResult;
                                }
                                else if (newResult.Value < result.Value)
                                {
                                    selected.X = x;
                                    selected.Y = y;
                                    selected.Z = z;
                                    result = newResult;
                                }
                        }
                        else if (wantLava == false && Block.IsSelectable(blocks[x, y, z]) && isBlockVisible(x, y, z))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            blockBox.Intersects(ref ray, out newResult);
                            if (newResult.HasValue)
                                if (!result.HasValue)
                                {
                                    selected.X = x;
                                    selected.Y = y;
                                    selected.Z = z;
                                    result = newResult;
                                }
                                else if (newResult.Value < result.Value)
                                {
                                    selected.X = x;
                                    selected.Y = y;
                                    selected.Z = z;
                                    result = newResult;
                                }
                        }
                    }
        }

        public void GetSelectedBlock2(ref Ray ray, out Vector3i selected)
        {
            int minX = (int)ray.Position.X - 1;
            int minY = (int)ray.Position.Y - 2;
            int minZ = (int)ray.Position.Z - 1;
            if (minX < 0)
                minX = 0;
            if (minY < 0)
                minY = 0;
            if (minZ < 0)
                minZ = 0;
            int maxX = (int)ray.Position.X + 1;
            int maxY = (int)ray.Position.Y + 1;
            int maxZ = (int)ray.Position.Z + 1;
            if (maxX > MAXWIDTH)
                maxX = MAXWIDTH;
            if (maxY > MAXHEIGHT)
                maxY = MAXHEIGHT;
            if (maxZ > MAXDEPTH)
                maxZ = MAXDEPTH;
            float? result = null, newResult;
            selected.X = -1;
            selected.Y = -1;
            selected.Z = -1;
            for (int x = minX; x < maxX; x++)
                for (int y = minY; y < maxY; y++)
                    for (int z = minZ; z < maxZ; z++)
                    {
                        if (Block.IsSelectable(blocks[x, y, z]) && isBlockVisible(x, y, z))
                        {
                            blockBox.Max.X = x + Block.halfVector.X;
                            blockBox.Max.Y = y + Block.halfVector.Y;
                            blockBox.Max.Z = z + Block.halfVector.Z;
                            blockBox.Min.X = x - Block.halfVector.X;
                            blockBox.Min.Y = y - Block.halfVector.Y;
                            blockBox.Min.Z = z - Block.halfVector.Z;
                            blockBox.Intersects(ref ray, out newResult);
                            if (newResult.HasValue)
                                if (!result.HasValue)
                                {
                                    selected.X = x;
                                    selected.Y = y;
                                    selected.Z = z;
                                    result = newResult;
                                }
                                else if (newResult.Value < result.Value)
                                {
                                    selected.X = x;
                                    selected.Y = y;
                                    selected.Z = z;
                                    result = newResult;
                                }
                        }
                    }
        }

        public float? BulletIntersection(ref Ray bulletRay)
        {
            int minX, minY, minZ, maxX, maxY, maxZ;
            float? result = null, newResult;
            Vector3 pos = new Vector3(0, 0, 0);

            while (pos.Length() <= 128)
            {
                minX = (int)(pos.X + bulletRay.Position.X) - 2;
                minY = (int)(pos.Y + bulletRay.Position.Y) - 2;
                minZ = (int)(pos.Z + bulletRay.Position.Z) - 2;
                if (minX < 0)
                    minX = 0;
                if (minY < 0)
                    minY = 0;
                if (minZ < 0)
                    minZ = 0;
                maxX = (int)(pos.X + bulletRay.Position.X) + 2;
                maxY = (int)(pos.Y + bulletRay.Position.Y) + 2;
                maxZ = (int)(pos.Z + bulletRay.Position.Z) + 2;
                if (maxX > MAXWIDTH)
                    maxX = MAXWIDTH;
                if (maxY > MAXHEIGHT)
                    maxY = MAXHEIGHT;
                if (maxZ > MAXDEPTH)
                    maxZ = MAXDEPTH;

                for (int x = minX; x < maxX; x++)
                    for (int y = minY; y < maxY; y++)
                        for (int z = minZ; z < maxZ; z++)
                            if (Block.IsBlockSolid(blocks[x, y, z]) && isBlockVisible(x, y, z))
                            {
                                blockBox.Max.X = x + Block.halfVector.X;
                                blockBox.Max.Y = y + Block.halfVector.Y;
                                blockBox.Max.Z = z + Block.halfVector.Z;
                                blockBox.Min.X = x - Block.halfVector.X;
                                blockBox.Min.Y = y - Block.halfVector.Y;
                                blockBox.Min.Z = z - Block.halfVector.Z;
                                blockBox.Intersects(ref bulletRay, out newResult);
                                if (newResult.HasValue)
                                    if (!result.HasValue)
                                    {
                                        result = newResult;
                                    }
                                    else if (newResult.Value < result.Value)
                                    {
                                        result = newResult;
                                    }
                            }

                if (result.HasValue)
                    return result;

                pos += bulletRay.Direction * 2;
            }
            return result;
        }

        public BoundingFrustum bf = new BoundingFrustum(Matrix.Identity);
        private Matrix mat;

        public void Render(Camera camera)
        {
            Initializing = false;

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["TexturedLighting"];
            Resources.BlockEffect.Parameters["underWater"].SetValue(game.GetUnderLiquid == UnderLiquid.Water);
            Resources.BlockEffect.Parameters["underLava"].SetValue(game.GetUnderLiquid == UnderLiquid.Lava);
            Resources.BlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            Resources.BlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);
            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockTextures);
            Resources.BlockEffect.Parameters["LightMap"].SetValue(game.GetLightingManager.LightMap);
            Matrix.Multiply(ref camera.ViewMatrix, ref camera.ProjMatrix, out mat);
            bf.Matrix = mat;

            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();

            parentNode.Render(bf);

            

            if (npt.isDone)
                npt.Render(bf);

            PitfallBlock.BeginRender(gd);
            for (int i = 0; i < pitfallBlocks.Count; i++)
            {
                pitfallBlocks[i].Render(gd, camera);
            }
            PitfallBlock.EndRender(gd);

           
        }

        public void RenderGlass(Camera camera)
        {
            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques[game.GetUnderLiquid == UnderLiquid.Lava ? "LavaFogLight" : game.GetUnderLiquid == UnderLiquid.Water ? "WaterFogLight" : "TexturedLighting"];
            Resources.BlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);
            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockTextures);
            Resources.BlockEffect.Parameters["LightMap"].SetValue(game.GetLightingManager.LightMap);

            //render the soild parts of the glass
            gd.RasterizerState = RasterizerState.CullNone;
            Resources.BlockEffect.Parameters["DiscardSolid"].SetValue(false);
            Resources.BlockEffect.Parameters["DiscardAlpha"].SetValue(true);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            parentNode.RenderGlass();

            //render the transparent part of it
            Resources.BlockEffect.Parameters["DiscardSolid"].SetValue(true);
            Resources.BlockEffect.Parameters["DiscardAlpha"].SetValue(false);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.DepthStencilState = DepthStencilState.DepthRead;
            parentNode.RenderGlass();


            //set back the defaults
            gd.DepthStencilState = DepthStencilState.Default;
            gd.RasterizerState = RasterizerState.CullCounterClockwise;
            Resources.BlockEffect.Parameters["DiscardSolid"].SetValue(false);
            Resources.BlockEffect.Parameters["DiscardAlpha"].SetValue(false);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();

        }

        internal class OctTreeNode
        {
            private const int WIDTH = 16, HEIGHT = 16, DEPTH = 16;
            private int X, Y, Z;
            private BoundingBox box;
            private BoundingBox bigBox;
            private Terrain octTree;
            private OctTreeNode[] children;
            private GraphicsDevice gd;
            private VertexBuffer vertexBuffer;
            private IndexBuffer indexBuffer;
            private VertexBuffer glassVertexBuffer;
            private IndexBuffer glassIndexBuffer;

            public void Dispose()
            {
                if (children != null)
                {
                    children[0].Dispose();
                    children[1].Dispose();
                    children[2].Dispose();
                    children[3].Dispose();
                    children[4].Dispose();
                    children[5].Dispose();
                    children[6].Dispose();
                    children[7].Dispose();
                }
                else
                {
                    if (vertexBuffer != null && vertexBuffer.IsDisposed == false)
                    {
                        vertexBuffer.Dispose();
                        vertexBuffer = null;
                    }
                    if (indexBuffer != null && indexBuffer.IsDisposed == false)
                    {
                        indexBuffer.Dispose();
                        indexBuffer = null;
                    }
                    if (glassVertexBuffer != null && glassVertexBuffer.IsDisposed == false)
                    {
                        glassVertexBuffer.Dispose();
                        glassVertexBuffer = null;
                    }
                    if (glassIndexBuffer != null && glassIndexBuffer.IsDisposed == false)
                    {
                        glassIndexBuffer.Dispose();
                        glassIndexBuffer = null;
                    }
                    
                }
            }

            public OctTreeNode(int x, int y, int z, int size, Terrain octTree)
            {
                if (size != WIDTH)
                {
                    children = new OctTreeNode[8];
                    int newSize = size / 2;
                    children[0] = new OctTreeNode(x, y, z, newSize, octTree);
                    children[1] = new OctTreeNode(x + newSize, y, z, newSize, octTree);
                    children[2] = new OctTreeNode(x, y, z + newSize, newSize, octTree);
                    children[3] = new OctTreeNode(x + newSize, y, z + newSize, newSize, octTree);
                    children[4] = new OctTreeNode(x, y + newSize, z, newSize, octTree);
                    children[5] = new OctTreeNode(x + newSize, y + newSize, z, newSize, octTree);
                    children[6] = new OctTreeNode(x, y + newSize, z + newSize, newSize, octTree);
                    children[7] = new OctTreeNode(x + newSize, y + newSize, z + newSize, newSize, octTree);
                }
                X = x;
                Y = y;
                Z = z;
                this.octTree = octTree;
                box = new BoundingBox(new Vector3(X, Y, Z), new Vector3(X + size - 1, Y + size - 1, Z + size - 1));
                bigBox = new BoundingBox(new Vector3(X - 2, Y - 2, Z - 2), new Vector3(X + size + 1, Y + size + 1, Z + size + 1));
            }

            public void Update()
            {
                if (children != null)
                {
                    children[0].Update();
                    children[1].Update();
                    children[2].Update();
                    children[3].Update();
                    children[4].Update();
                    children[5].Update();
                    children[6].Update();
                    children[7].Update();
                }
                if (ImDirty)
                {
                    if (octTree.Initializing == false)
                    {
                        if (DidSomeOneGetIt == false)
                        {
                            DidSomeOneGetIt = true;
                            RebuildBuffers();
                            ImDirty = false;
                        }
                    }
                    else
                    {
                        RebuildBuffers();
                        ImDirty = false;
                    }
                    // Thread t = 
                    // new Thread(ThreadedRebuildBuffers).Start();
                    // t.IsBackground = true;
                    // t.Start();

                }
            }
            public static bool DidSomeOneGetIt = false;
            public bool RemoveBlock(int x, int y, int z)
            {
                if (children != null)
                {
                    if (box.Contains(new Vector3(x, y, z)) == ContainmentType.Contains)
                    {
                        if (children[0].RemoveBlock(x, y, z))
                            return true;
                        else if (children[1].RemoveBlock(x, y, z))
                            return true;
                        else if (children[2].RemoveBlock(x, y, z))
                            return true;
                        else if (children[3].RemoveBlock(x, y, z))
                            return true;
                        else if (children[4].RemoveBlock(x, y, z))
                            return true;
                        else if (children[5].RemoveBlock(x, y, z))
                            return true;
                        else if (children[6].RemoveBlock(x, y, z))
                            return true;
                        else if (children[7].RemoveBlock(x, y, z))
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (box.Contains(new Vector3(x, y, z)) != ContainmentType.Contains)
                    return false;



                //  byte oldID = octTree.blocks[x, y, z];
                octTree.blocks[x, y, z] = Block.BLOCKID_AIR;


                for (int i = 0; i < 6; i++)
                {
                    octTree.MarkDirty(directions[i].X + x, directions[i].Y + y, directions[i].Z + z);
                }

                return true;
            }

            public bool AddBlock(int x, int y, int z, byte id)
            {
                if (children != null)
                {
                    if (box.Contains(new Vector3(x, y, z)) == ContainmentType.Contains)
                    {
                        if (children[0].AddBlock(x, y, z, id))
                            return true;
                        else if (children[1].AddBlock(x, y, z, id))
                            return true;
                        else if (children[2].AddBlock(x, y, z, id))
                            return true;
                        else if (children[3].AddBlock(x, y, z, id))
                            return true;
                        else if (children[4].AddBlock(x, y, z, id))
                            return true;
                        else if (children[5].AddBlock(x, y, z, id))
                            return true;
                        else if (children[6].AddBlock(x, y, z, id))
                            return true;
                        else if (children[7].AddBlock(x, y, z, id))
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (box.Contains(new Vector3(x, y, z)) != ContainmentType.Contains)
                    return false;


                octTree.blocks[x, y, z] = id;

                for (int i = 0; i < 6; i++)
                {
                    octTree.MarkDirty(directions[i].X + x, directions[i].Y + y, directions[i].Z + z);
                }

                return true;
            }

            private bool ImDirty;
            public bool MarkDirty(int x, int y, int z)
            {
                if (children != null)
                {
                    if (box.Contains(new Vector3(x, y, z)) == ContainmentType.Contains)
                    {
                        if (children[0].MarkDirty(x, y, z))
                            return true;
                        else if (children[1].MarkDirty(x, y, z))
                            return true;
                        else if (children[2].MarkDirty(x, y, z))
                            return true;
                        else if (children[3].MarkDirty(x, y, z))
                            return true;
                        else if (children[4].MarkDirty(x, y, z))
                            return true;
                        else if (children[5].MarkDirty(x, y, z))
                            return true;
                        else if (children[6].MarkDirty(x, y, z))
                            return true;
                        else if (children[7].MarkDirty(x, y, z))
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (box.Contains(new Vector3(x, y, z)) != ContainmentType.Contains)
                    return false;

                ImDirty = true;
                return true;
            }

            private ContainmentType type;
            public void MarkDirty(ref BoundingBox box)
            {
                box.Contains(ref this.box, out type);
                if (children != null)
                {
                    if (type != ContainmentType.Disjoint)
                    {
                        children[0].MarkDirty(ref box);
                        children[1].MarkDirty(ref box);
                        children[2].MarkDirty(ref box);
                        children[3].MarkDirty(ref box);
                        children[4].MarkDirty(ref box);
                        children[5].MarkDirty(ref box);
                        children[6].MarkDirty(ref box);
                        children[7].MarkDirty(ref box);
                    }
                    return;
                }
                else if (type == ContainmentType.Disjoint)
                    return;

                ImDirty = true;
            }

            public void AllDirty()
            {
                if (children != null)
                {
                    children[0].AllDirty();
                    children[1].AllDirty();
                    children[2].AllDirty();
                    children[3].AllDirty();
                    children[4].AllDirty();
                    children[5].AllDirty();
                    children[6].AllDirty();
                    children[7].AllDirty();
                }
                else
                    ImDirty = true;
            }

            public void RebuildBuffers()
            {
                VertexPositionTextureSideLight[] verticeData;

                int[] indices;
                bool[] faces = new bool[6];

                int glassFaceCount = 0;
                int faceCount = 0;

                for (int x = X; x < X + WIDTH; x++)
                    for (int y = Y; y < Y + HEIGHT; y++)
                        for (int z = Z; z < Z + DEPTH; z++)
                        {
                            if (Block.IsGlass(octTree.blocks[x, y, z]) == false && octTree.isBlockVisible(x, y, z) && Block.IsBlockSolidNoGlass(octTree.blocks[x, y, z]))
                            {
                                octTree.GetFacesNoGlass(x, y, z, faces);
                                if (faces[0])
                                    faceCount++;
                                if (faces[1])
                                    faceCount++;
                                if (faces[2])
                                    faceCount++;
                                if (faces[3])
                                    faceCount++;
                                if (faces[4])
                                    faceCount++;
                                if (faces[5])
                                    faceCount++;
                            }
                            else if (Block.IsGlass(octTree.blocks[x, y, z]) == true)
                            {
                                octTree.GetFacesGlass(x, y, z, faces);
                                if (faces[0])
                                    glassFaceCount++;
                                if (faces[1])
                                    glassFaceCount++;
                                if (faces[2])
                                    glassFaceCount++;
                                if (faces[3])
                                    glassFaceCount++;
                                if (faces[4])
                                    glassFaceCount++;
                                if (faces[5])
                                    glassFaceCount++;
                            }
                        }


                if (faceCount == 0)
                {
                    if (vertexBuffer != null)
                    {
                        vertexBuffer.Dispose();
                        indexBuffer.Dispose();
                        indexBuffer = null;
                        vertexBuffer = null;
                    }
                }
                else
                {
                    if (gd == null || gd.IsDisposed)
                        return;


                    vertexBuffer = new VertexBuffer(gd, typeof(VertexPositionTextureSideLight), faceCount * 4, BufferUsage.WriteOnly);
                    verticeData = new VertexPositionTextureSideLight[faceCount * 4];
                    indexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, faceCount * 6, BufferUsage.WriteOnly);
                    indices = new int[faceCount * 6];

                    int vert = 0, indice = 0;

                    for (int x = X; x < X + WIDTH; x++)
                        for (int y = Y; y < Y + HEIGHT; y++)
                            for (int z = Z; z < Z + DEPTH; z++)
                                if (Block.IsBlockSolidNoGlass(octTree.blocks[x, y, z]) && Block.IsGlass(octTree.blocks[x, y, z]) == false && octTree.isBlockVisible(x, y, z))
                                {
                                    octTree.GetFacesNoGlass(x, y, z, faces);
                                    Block.CreateCubeIndexed(x, y, z, octTree.blocks[x, y, z], ref vert, ref indice, faces, indices, verticeData);
                                }

                    if (vertexBuffer != null && indexBuffer != null)
                    {
                        vertexBuffer.SetData<VertexPositionTextureSideLight>(verticeData);
                        indexBuffer.SetData<int>(indices);
                    }
                }

                if (glassFaceCount == 0)
                {
                    if (glassVertexBuffer != null)
                    {
                        glassVertexBuffer.Dispose();
                        glassIndexBuffer.Dispose();
                        glassVertexBuffer = null;
                        glassIndexBuffer = null;
                    }
                }
                else
                {
                    if (gd == null || gd.IsDisposed)
                        return;

                    glassVertexBuffer = new VertexBuffer(gd, typeof(VertexPositionTextureSideLight), glassFaceCount * 4, BufferUsage.WriteOnly);
                    verticeData = new VertexPositionTextureSideLight[glassFaceCount * 4];
                    glassIndexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, glassFaceCount * 6, BufferUsage.WriteOnly);
                    indices = new int[glassFaceCount * 6];

                    int vert = 0, indice = 0;

                    for (int x = X; x < X + WIDTH; x++)
                        for (int y = Y; y < Y + HEIGHT; y++)
                            for (int z = Z; z < Z + DEPTH; z++)
                                if (Block.IsGlass(octTree.blocks[x, y, z]) == true && octTree.isBlockVisible(x, y, z))
                                {
                                    octTree.GetFacesGlass(x, y, z, faces);
                                    Block.CreateCubeIndexed(x, y, z, octTree.blocks[x, y, z], ref vert, ref indice, faces, indices, verticeData);
                                }

                    if (glassVertexBuffer != null)
                    {
                        glassVertexBuffer.SetData<VertexPositionTextureSideLight>(verticeData);
                        glassIndexBuffer.SetData<int>(indices);
                    }
                }

            }

            public void InitializeBuffers(GraphicsDevice gd)
            {
                if (children == null)
                {
                    this.gd = gd;
                    RebuildBuffers();
                }
                else
                {
                    children[0].InitializeBuffers(gd);
                    children[1].InitializeBuffers(gd);
                    children[2].InitializeBuffers(gd);
                    children[3].InitializeBuffers(gd);
                    children[4].InitializeBuffers(gd);
                    children[5].InitializeBuffers(gd);
                    children[6].InitializeBuffers(gd);
                    children[7].InitializeBuffers(gd);
                }
            }

            private bool renderGlass = false;
            private bool result = false;
            public void Render(BoundingFrustum bf)
            {
                bf.Intersects(ref bigBox, out result);

                renderGlass = false;


                if (!result)
                {
                    return;
                }

                renderGlass = true;

                if (children == null)
                {
                    if (vertexBuffer != null)
                    {
                        gd.SetVertexBuffer(vertexBuffer);
                        gd.Indices = indexBuffer;
                        gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
                    }
                }
                else
                {
                    children[0].Render(bf);
                    children[1].Render(bf);
                    children[2].Render(bf);
                    children[3].Render(bf);
                    children[4].Render(bf);
                    children[5].Render(bf);
                    children[6].Render(bf);
                    children[7].Render(bf);
                }
            }

            public void RenderGlass()
            {
                if (renderGlass)
                {

                    if (children == null)
                    {
                        if (glassVertexBuffer != null)
                        {
                            gd.SetVertexBuffer(glassVertexBuffer);
                            gd.Indices = glassIndexBuffer;
                            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, glassVertexBuffer.VertexCount, 0, glassIndexBuffer.IndexCount / 3);
                        }
                    }
                    else
                    {
                        children[0].RenderGlass();
                        children[1].RenderGlass();
                        children[2].RenderGlass();
                        children[3].RenderGlass();
                        children[4].RenderGlass();
                        children[5].RenderGlass();
                        children[6].RenderGlass();
                        children[7].RenderGlass();
                    }

                }
            }
        }

        internal class QuadTreeNode
        {
            private const int WIDTH = 64, DEPTH = 64;
            private int X, Y, Z;
            private BoundingBox box;
            private BoundingBox bigBox;
            private OctTreeNode[] children;

            public void Dispose()
            {
                children[0].Dispose();
                children[1].Dispose();
                children[2].Dispose();
                children[3].Dispose();
            }

            public QuadTreeNode(int x, int y, int z, int size, Terrain octTree)
            {
                if (size != WIDTH)
                {
                    children = new OctTreeNode[4];
                    int newSize = size / 2;
                    children[0] = new OctTreeNode(x, y, z, newSize, octTree);
                    children[1] = new OctTreeNode(x + newSize, y, z, newSize, octTree);
                    children[2] = new OctTreeNode(x, y, z + newSize, newSize, octTree);
                    children[3] = new OctTreeNode(x + newSize, y, z + newSize, newSize, octTree);
                }
                X = x;
                Y = y;
                Z = z;
                box = new BoundingBox(new Vector3(X, Y, Z), new Vector3(X + size - 1, Y + Terrain.MAXHEIGHT - 1, Z + size - 1));
                bigBox = new BoundingBox(new Vector3(X - 2, Y - 2, Z - 2), new Vector3(X + size + 1, Y + Terrain.MAXHEIGHT + 1, Z + size + 1));
            }

            public void Update()
            {
                children[0].Update();
                children[1].Update();
                children[2].Update();
                children[3].Update();
            }

            public bool RemoveBlock(int x, int y, int z)
            {
                if (box.Contains(new Vector3(x, y, z)) == ContainmentType.Contains)
                {
                    if (children[0].RemoveBlock(x, y, z))
                        return true;
                    else if (children[1].RemoveBlock(x, y, z))
                        return true;
                    else if (children[2].RemoveBlock(x, y, z))
                        return true;
                    else if (children[3].RemoveBlock(x, y, z))
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }

            public bool MarkDirty(int x, int y, int z)
            {
                if (box.Contains(new Vector3(x, y, z)) == ContainmentType.Contains)
                {
                    if (children[0].MarkDirty(x, y, z))
                        return true;
                    else if (children[1].MarkDirty(x, y, z))
                        return true;
                    else if (children[2].MarkDirty(x, y, z))
                        return true;
                    else if (children[3].MarkDirty(x, y, z))
                        return true;
                    else
                        return false;
                }
                else return false;
            }

            private ContainmentType type;
            public void MarkDirty(ref BoundingBox box)
            {
                box.Contains(ref this.box, out type);
                if (type != ContainmentType.Disjoint)
                {
                    children[0].MarkDirty(ref box);
                    children[1].MarkDirty(ref box);
                    children[2].MarkDirty(ref box);
                    children[3].MarkDirty(ref box);
                    return;
                }
            }

            public void AllDirty()
            {
                children[0].AllDirty();
                children[1].AllDirty();
                children[2].AllDirty();
                children[3].AllDirty();
            }

            public bool AddBlock(int x, int y, int z, byte id)
            {
                if (box.Contains(new Vector3(x, y, z)) == ContainmentType.Contains)
                {
                    if (children[0].AddBlock(x, y, z, id))
                        return true;
                    else if (children[1].AddBlock(x, y, z, id))
                        return true;
                    else if (children[2].AddBlock(x, y, z, id))
                        return true;
                    else if (children[3].AddBlock(x, y, z, id))
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }

            public void InitializeBuffers(GraphicsDevice gd)
            {
                children[0].InitializeBuffers(gd);
                children[1].InitializeBuffers(gd);
                children[2].InitializeBuffers(gd);
                children[3].InitializeBuffers(gd);
            }

            private bool renderGlass;
            private bool result;
            public void Render(BoundingFrustum bf)
            {
                bf.Intersects(ref bigBox, out result);

                renderGlass = false;
                if (!result)
                {
                    return;
                }

                renderGlass = true;
                children[0].Render(bf);
                children[1].Render(bf);
                children[2].Render(bf);
                children[3].Render(bf);
            }

            public void RenderGlass()
            {
                if (renderGlass)
                {
                    children[0].RenderGlass();
                    children[1].RenderGlass();
                    children[2].RenderGlass();
                    children[3].RenderGlass();
                }
            }
        }

        internal class NonPlayableTerrain
        {
            public const int MAXWIDTH = 256, MAXDEPTH = 256;
            private QuadTree parentNode;
            private Texture3D lightMap;

            public bool isDone = false;

            public NonPlayableTerrain()
            {
                parentNode = new QuadTree(0, 0, 256);
            }

            public void Dispose()
            {
                if (isDone == false)
                {
                    Thread t = new Thread(delegate()
                    {
#if XBOX
                Thread.CurrentThread.SetProcessorAffinity(new int[] { 5 });
#endif
                            while (isDone == false)
                                ;

                            if (lightMap != null && lightMap.IsDisposed == false)
                            {
                                lightMap.Dispose();
                            }
                            parentNode.Dispose();
                        });

                    t.IsBackground = true;
                    t.Name = "dispose thread";
                    t.Start();
                }

                if (lightMap != null && lightMap.IsDisposed == false)
                {
                    lightMap.Dispose();
                }
                parentNode.Dispose();
            }

            public void InitializeBuffers(GraphicsDevice gd, byte[, ,] world)
            {
                try
                {
#if XBOX
                Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                    parentNode.InitializeBuffers(gd, world);

                    lightMap = new Texture3D(gd, 256, 64, 256, false, SurfaceFormat.Alpha8);
                    LightingManager.LightAWorld(lightMap, world);

                    isDone = true;
                }
                catch (Exception e)
                {
                    isDone = false;
                    lock (MinerOfDuty.ExceptionsLock)
                        MinerOfDuty.Exceptions.Enqueue(e);
                }

            }

            public void Render(BoundingFrustum bf)
            {
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["NonPlayableTextureLighting"];
                Resources.BlockEffect.Parameters["NonPlayableLightMap"].SetValue(lightMap);
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(-64, 0, -64));
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                parentNode.Render(bf);

                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            }

            internal class QuadTree
            {
                private const int MAXWIDTH = 16, MAXDEPTH = 16;
                private int X, Z;
                private QuadTree[] children;
                private BoundingBox box;
                private static readonly Rectangle middle = new Rectangle(64, 64, 128, 128);
                private VertexBuffer vertexBuffer;
                private IndexBuffer indexBuffer;

                public void Dispose()
                {
                    if (children != null)
                    {
                        children[0].Dispose();
                        children[1].Dispose();
                        children[2].Dispose();
                        children[3].Dispose();
                    }
                    else
                    {
                        if (vertexBuffer != null && vertexBuffer.IsDisposed == false)
                        {
                            vertexBuffer.Dispose();
                            vertexBuffer = null;
                        }
                        if (indexBuffer != null && indexBuffer.IsDisposed == false)
                        {
                            indexBuffer.Dispose();
                            indexBuffer = null;
                        }
                    }
                }

                private bool unused = false;

                public QuadTree(int x, int z, int size)
                {


                    if (size != MAXWIDTH)
                    {
                        children = new QuadTree[4];
                        int newSize = size / 2;
                        children[0] = new QuadTree(x, z, newSize);
                        children[1] = new QuadTree(x + newSize, z, newSize);
                        children[2] = new QuadTree(x, z + newSize, newSize);
                        children[3] = new QuadTree(x + newSize, z + newSize, newSize);
                    }
                    else
                    {
                        children = null;
                        if (middle.Contains(x, z))
                        {
                            unused = true;
                            return;
                        }
                    }
                    X = x;
                    Z = z;
                    box = new BoundingBox(new Vector3(x - 64, 0, z - 64), new Vector3(x - 64 + size, 64, z - 64 + size));
                }

                public void InitializeBuffers(GraphicsDevice gd, byte[, ,] world)
                {
                    if (unused)
                        return;

                    if (children == null)
                    {
                        VertexPositionTextureSideLight[] verticeData;

                        int[] indices;
                        bool[] faces = new bool[6];
                        int used = 0;
                        int faceCount = 0;

                        for (int x = X; x < X + MAXWIDTH; x++)
                            for (int y = 0; y < 64; y++)
                                for (int z = Z; z < Z + MAXDEPTH; z++)
                                    if (Block.IsBlockSolidNoGlass(world[x, y, z]) && Block.IsGlass(world[x, y, z]) == false && isBlockVisible(x, y, z, world))
                                    {
                                        used++;
                                        GetFaces(x, y, z, faces, world);
                                        if (faces[0])
                                            faceCount++;
                                        if (faces[1])
                                            faceCount++;
                                        if (faces[2])
                                            faceCount++;
                                        if (faces[3])
                                            faceCount++;
                                        if (faces[4])
                                            faceCount++;
                                        if (faces[5])
                                            faceCount++;
                                    }


                        if (used == 0)
                        {
                            return;
                        }


                        if (gd == null || gd.IsDisposed)
                            return;


                        vertexBuffer = new VertexBuffer(gd, typeof(VertexPositionTextureSideLight), faceCount * 4, BufferUsage.WriteOnly);
                        verticeData = new VertexPositionTextureSideLight[faceCount * 4];
                        indexBuffer = new IndexBuffer(gd, IndexElementSize.ThirtyTwoBits, faceCount * 6, BufferUsage.WriteOnly);
                        indices = new int[faceCount * 6];


                        int vert = 0, indice = 0;

                        for (int x = X; x < X + MAXWIDTH; x++)
                            for (int y = 0; y < 64; y++)
                                for (int z = Z; z < Z + MAXDEPTH; z++)
                                    if (Block.IsBlockSolidNoGlass(world[x, y, z]) && Block.IsGlass(world[x, y, z]) == false && isBlockVisible(x, y, z, world))
                                    {
                                        GetFaces(x, y, z, faces, world);
                                        Block.CreateCubeIndexed(x, y, z, world[x, y, z], ref vert, ref indice, faces, indices, verticeData);
                                    }

                        if (vertexBuffer == null)
                            return;

                        vertexBuffer.SetData<VertexPositionTextureSideLight>(verticeData);
                        if (indexBuffer == null)
                            return;
                        indexBuffer.SetData<int>(indices);
                    }
                    else
                    {
                        children[0].InitializeBuffers(gd, world);
                        children[1].InitializeBuffers(gd, world);
                        children[2].InitializeBuffers(gd, world);
                        children[3].InitializeBuffers(gd, world);
                    }
                }

                private ContainmentType result;
                public void Render(BoundingFrustum bf)
                {
                    if (unused)
                        return;

                    bf.Contains(ref box, out result);

                    if (result == ContainmentType.Disjoint)
                        return;

                    if (children != null)
                    {
                        children[0].Render(bf);
                        children[1].Render(bf);
                        children[2].Render(bf);
                        children[3].Render(bf);
                    }
                    else
                    {
                        if (vertexBuffer != null)
                        {
                            vertexBuffer.GraphicsDevice.SetVertexBuffer(vertexBuffer);
                            vertexBuffer.GraphicsDevice.Indices = indexBuffer;
                            vertexBuffer.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3);
                        }
                    }

                }

                public static void GetFaces(int x, int y, int z, bool[] faces, byte[, ,] blocks)
                {
                    if (x + 1 < NonPlayableTerrain.MAXWIDTH && !Block.IsBlockSolid(blocks[x + 1, y, z]) && x <= 192)
                        faces[3] = true;
                    else
                        faces[3] = false;

                    if (x - 1 >= 0 && !Block.IsBlockSolid(blocks[x - 1, y, z]) && x >= 64)
                        faces[1] = true;
                    else
                        faces[1] = false;

                    if (y + 1 < Terrain.MAXHEIGHT && !Block.IsBlockSolid(blocks[x, y + 1, z]))
                        faces[0] = true;
                    else if (y + 1 < Terrain.MAXHEIGHT)
                        faces[0] = false;

                    if (y - 1 >= 0 && !Block.IsBlockSolid(blocks[x, y - 1, z]))
                        faces[2] = true;
                    else if (y - 1 >= 0)
                        faces[2] = false;

                    if (z + 1 < NonPlayableTerrain.MAXDEPTH && !Block.IsBlockSolid(blocks[x, y, z + 1]) && z <= 192)
                        faces[4] = true;
                    else
                        faces[4] = false;

                    if (z - 1 >= 0 && !Block.IsBlockSolid(blocks[x, y, z - 1]) && z >= 64)
                        faces[5] = true;
                    else
                        faces[5] = false;

                }

                public static bool isBlockVisible(int x, int y, int z, byte[, ,] blocks)
                {
                    for (int i = 0; i < directions.Length; i++)
                    {
                        if (directions[i].X + x >= 0 && directions[i].X + x < NonPlayableTerrain.MAXWIDTH
                            && directions[i].Y + y >= 0 && directions[i].Y + y < MAXHEIGHT
                            && directions[i].Z + z >= 0 && directions[i].Z + z < NonPlayableTerrain.MAXDEPTH)
                        {
                            if (!Block.IsBlockSolid(blocks[directions[i].X + x, directions[i].Y + y, directions[i].Z + z]))
                                return true;
                        }
                    }
                    return false;
                }
            }

        }

        public WorldEditor.WorldGeneration WorldGen { get; private set; }
        public static void CreateBlocksIsland(GraphicsDevice gd, Terrain terrain, bool useLiquids, int size, bool genTrees)
        {
            terrain.WorldGen = WorldEditor.WorldGeneration.Island;


            byte[,] data = new byte[256, 256];
            float[] colorData;

            colorData = TerrainGenerator.GenerateBeachWorld(terrain.game.GetRandom);

            terrain.blocks = new byte[256, 64, 256];

            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 256; y++)
                {
                    data[x, y] = (byte)((((colorData[x + y * 256] - .025f) * 255) * .19f) + 11);
                }

            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (y < data[x, z] && y < 31)
                            terrain.blocks[x, y, z] = Block.BLOCKID_STONE;
                        else if (y < data[x, z])
                            terrain.blocks[x, y, z] = Block.BLOCKID_DIRT;

                        else
                            terrain.blocks[x, y, z] = Block.BLOCKID_AIR;
                    }
                }

            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (y < data[x, z] && y < 30 && NonPlayableTerrain.QuadTree.isBlockVisible(x, y, z, terrain.blocks)) // it isnt an air block
                            terrain.blocks[x, y, z] = Block.BLOCKID_SAND;
                    }
                }

            for (int x = 102; x < 154; x++)
                for (int z = 102; z < 154; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (y < data[x, z] && NonPlayableTerrain.QuadTree.isBlockVisible(x, y, z, terrain.blocks)) // it isnt an air block
                            terrain.blocks[x, y, z] = Block.BLOCKID_SAND;
                    }
                }


            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (terrain.blocks[x, y, z] == Block.BLOCKID_DIRT && y + 1 < Terrain.MAXHEIGHT && terrain.blocks[x, y + 1, z] == Block.BLOCKID_AIR) // it isnt an air block
                        {
                            terrain.blocks[x, y, z] = Block.BLOCKID_GRASS;
                        }
                    }
                }

            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    terrain.blocks[x, 0, z] = Block.BLOCKID_BEDROCK;
                }

            if(genTrees)
                TerrainGenerator.GenerateTrees(terrain.blocks, size, 502, terrain.game.GetRandom);

            for (int i = 0; i < 1; i += 2)
            {
                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[x, y, i] = Block.BLOCKID_DIRT;

                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[x, y, 255 - i] = Block.BLOCKID_DIRT;

                for (int z = 0; z < 256; z++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[i, y, z] = Block.BLOCKID_DIRT;

                for (int z = 0; z < 256; z++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[255 - i, y, z] = Block.BLOCKID_DIRT;
            }



            terrain.npt = new NonPlayableTerrain();
            byte[, ,] oldBlocks = terrain.blocks;
            Thread t = new Thread(poop => terrain.npt.InitializeBuffers(gd, oldBlocks));
            t.Name = "Edge World";
            t.IsBackground = true;
            t.Start();


            terrain.blocks = new byte[128, 64, 128];


            for (int x = 64; x < 128 + 64; x++)
                for (int y = 0; y < 64; y++)
                    for (int z = 64; z < 128 + 64; z++)
                    {
                        terrain.blocks[x - 64, y, z - 64] = oldBlocks[x, y, z];
                    }

            TerrainGenerator.GenerateWall(terrain, size);

            if (useLiquids)
                for (int x = 0; x < 128; x++)
                    for (int z = 0; z < 128; z++)
                    {
                        for (int y = 28; y >= 0; y--)
                        {
                            if (terrain.blocks[x, y, z] == Block.BLOCKID_AIR)
                                terrain.game.GetLiquidManager.AddPreInit(true, x, y, z);
                        }
                    }



        }

        public static void CreateBlocksRandom(GraphicsDevice gd, Terrain terrain, bool useLiquids, int size, bool genTrees)
        {
            terrain.WorldGen = WorldEditor.WorldGeneration.Random;


            byte[,] data = new byte[256, 256];
            float[] colorData;

            colorData = TerrainGenerator.GenerateHeightMap(terrain.game.GetRandom);

            terrain.blocks = new byte[256, 64, 256];

            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 256; y++)
                {
                    data[x, y] = (byte)((((colorData[x + y * 256] - .025f) * 255) * .19f) + 11);
                }


            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (y < data[x, z] && y < 31)
                            terrain.blocks[x, y, z] = Block.BLOCKID_STONE;
                        else if (y < data[x, z])
                            terrain.blocks[x, y, z] = Block.BLOCKID_DIRT;

                        else
                            terrain.blocks[x, y, z] = Block.BLOCKID_AIR;
                    }
                }

            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (y < data[x, z] && y < 30 && NonPlayableTerrain.QuadTree.isBlockVisible(x, y, z, terrain.blocks)) // it isnt an air block
                            terrain.blocks[x, y, z] = Block.BLOCKID_SAND;
                    }
                }

            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (terrain.blocks[x, y, z] == Block.BLOCKID_DIRT && y + 1 < Terrain.MAXHEIGHT && terrain.blocks[x, y + 1, z] == Block.BLOCKID_AIR) // it isnt an air block
                        {
                            terrain.blocks[x, y, z] = Block.BLOCKID_GRASS;
                        }
                    }
                }




            TerrainGenerator.GenerateCaves(terrain.game.GetRandom, terrain.blocks, 256);

            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    terrain.blocks[x, 0, z] = Block.BLOCKID_BEDROCK;
                }



            if (genTrees)
                TerrainGenerator.GenerateTrees(terrain.blocks, size, 502, terrain.game.GetRandom);

            for (int i = 0; i < 1; i += 2)
            {
                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[x, y, i] = Block.BLOCKID_DIRT;

                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[x, y, 255 - i] = Block.BLOCKID_DIRT;

                for (int z = 0; z < 256; z++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[i, y, z] = Block.BLOCKID_DIRT;

                for (int z = 0; z < 256; z++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[255 - i, y, z] = Block.BLOCKID_DIRT;
            }



            terrain.npt = new NonPlayableTerrain();
            byte[, ,] oldBlocks = terrain.blocks;
            Thread t = new Thread(poop => terrain.npt.InitializeBuffers(gd, oldBlocks));
            t.Name = "Edge World";
            t.IsBackground = true;
            t.Start();


            terrain.blocks = new byte[128, 64, 128];


            for (int x = 64; x < 128 + 64; x++)
                for (int y = 0; y < 64; y++)
                    for (int z = 64; z < 128 + 64; z++)
                    {
                        terrain.blocks[x - 64, y, z - 64] = oldBlocks[x, y, z];
                    }

            TerrainGenerator.GenerateWall(terrain, size);
           
            if (useLiquids)
                for (int x = 0; x < 128; x++)
                    for (int z = 0; z < 128; z++)
                    {
                        for (int y = 8; y >= 0; y--)
                        {
                            if (terrain.blocks[x, y, z] == Block.BLOCKID_AIR)
                                terrain.game.GetLiquidManager.AddPreInit(false, x, y, z);
                        }
                    }
            if (useLiquids)
            {
                for (int a = terrain.game.GetRandom.Next(2, 5); a >= 0; a--)
                {
                    int xPoint = terrain.game.GetRandom.Next(5, size - 5);
                    int yPoint = terrain.game.GetRandom.Next(5, size - 5);
                    
                    int height = 0;
                    for(int i = 63; i > 0; i--)
                        if (terrain.blocks[xPoint, i, yPoint] != Block.BLOCKID_AIR)
                        {
                            height = i;
                            break;
                        }

                    if (terrain.blocks[xPoint, height, yPoint] == Block.BLOCKID_GRASS)
                    {
                        int maxX = terrain.game.GetRandom.Next(2, 4);
                        for (int i = 0; i < 3 && height - i > 0; i++)
                        {
                            for (int x = xPoint - terrain.game.GetRandom.Next(1, 3); x < xPoint + maxX; x++)
                            {
                                int maxY = terrain.game.GetRandom.Next(2, 4);
                                for (int y = yPoint - terrain.game.GetRandom.Next(1, 3); y < yPoint + maxY; y++)
                                {
                                    terrain.blocks[x, height - i, y] = Block.BLOCKID_AIR;
                                    if (i == 2)
                                        terrain.game.GetLiquidManager.AddPreInit(true, x, height - i, y);
                                }
                            }
                        }
                    }
                    
                }
            }
        }

        public static void CreateBlocksFlat(GraphicsDevice gd, Terrain terrain, bool useLiquids, int size, bool genTrees)
        {
            terrain.WorldGen = WorldEditor.WorldGeneration.Flat;

            byte[,] data = new byte[256, 256];
            float[] colorData;

            colorData = TerrainGenerator.GenerateFlatHeightMap(.12f);

            terrain.blocks = new byte[256, 64, 256];

            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 256; y++)
                {
                    data[x, y] = (byte)((((colorData[x + y * 256] - .025f) * 255) * .19f) + 11);
                }


            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (y < 8)
                            terrain.blocks[x, y, z] = Block.BLOCKID_STONE;
                        else if (y < 9 + terrain.game.GetRandom.Next(-1, 2) && terrain.blocks[x, y - 1, z] == Block.BLOCKID_STONE)
                            terrain.blocks[x, y, z] = Block.BLOCKID_STONE;
                        else if (y < data[x, z])
                            terrain.blocks[x, y, z] = Block.BLOCKID_DIRT;
                        else
                            terrain.blocks[x, y, z] = Block.BLOCKID_AIR;
                    }
                }


            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (terrain.blocks[x, y, z] == Block.BLOCKID_DIRT && y + 1 < Terrain.MAXHEIGHT && terrain.blocks[x, y + 1, z] == Block.BLOCKID_AIR) // it isnt an air block
                            terrain.blocks[x, y, z] = Block.BLOCKID_GRASS;
                    }
                }


            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    terrain.blocks[x, 0, z] = Block.BLOCKID_BEDROCK;
                }


            for (int i = 0; i < 1; i += 2)
            {
                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[x, y, i] = Block.BLOCKID_DIRT;

                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[x, y, 255 - i] = Block.BLOCKID_DIRT;

                for (int z = 0; z < 256; z++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[i, y, z] = Block.BLOCKID_DIRT;

                for (int z = 0; z < 256; z++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[255 - i, y, z] = Block.BLOCKID_DIRT;
            }




            if (genTrees)
                TerrainGenerator.GenerateTrees(terrain.blocks, size, 502, terrain.game.GetRandom);


            terrain.npt = new NonPlayableTerrain();
            byte[, ,] oldBlocks = terrain.blocks;
            Thread t = new Thread(poop => terrain.npt.InitializeBuffers(gd, oldBlocks));
            t.Name = "Edge World";
            t.IsBackground = true;
            t.Start();


            terrain.blocks = new byte[128, 64, 128];


            for (int x = 64; x < 128 + 64; x++)
                for (int y = 0; y < 64; y++)
                    for (int z = 64; z < 128 + 64; z++)
                    {
                        terrain.blocks[x - 64, y, z - 64] = oldBlocks[x, y, z];
                    }

            TerrainGenerator.GenerateWall(terrain, size);

           

             
        }

        public static void CreateBlocksFlatCaves(GraphicsDevice gd, Terrain terrain, bool useLiquids, int size, bool genTrees)
        {
            terrain.WorldGen = WorldEditor.WorldGeneration.FlatWithCaves;

            byte[,] data = new byte[256, 256];
            float[] colorData;

            colorData = TerrainGenerator.GenerateFlatHeightMap(.4f);

            terrain.blocks = new byte[256, 64, 256];

            for (int x = 0; x < 256; x++)
                for (int y = 0; y < 256; y++)
                {
                    data[x, y] = (byte)((((colorData[x + y * 256] - .025f) * 255) * .19f) + 11);
                }



            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (y < 24)
                            terrain.blocks[x, y, z] = Block.BLOCKID_STONE;
                        else if (y < 26 + terrain.game.GetRandom.Next(-2,3) && terrain.blocks[x,y - 1,z] == Block.BLOCKID_STONE)
                            terrain.blocks[x, y, z] = Block.BLOCKID_STONE;
                        else if (y < data[x, z])
                            terrain.blocks[x, y, z] = Block.BLOCKID_DIRT;
                        else
                            terrain.blocks[x, y, z] = Block.BLOCKID_AIR;
                    }
                }


            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        if (terrain.blocks[x, y, z] == Block.BLOCKID_DIRT && y + 1 < Terrain.MAXHEIGHT && terrain.blocks[x, y + 1, z] == Block.BLOCKID_AIR) // it isnt an air block
                            terrain.blocks[x, y, z] = Block.BLOCKID_GRASS;
                    }
                }



            TerrainGenerator.GenerateCaves(terrain.game.GetRandom, terrain.blocks, 256);

            for (int x = 0; x < 256; x++)
                for (int z = 0; z < 256; z++)
                {
                    terrain.blocks[x, 0, z] = Block.BLOCKID_BEDROCK;
                }


            for (int i = 0; i < 1; i += 2)
            {
                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[x, y, i] = Block.BLOCKID_DIRT;

                for (int x = 0; x < 256; x++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[x, y, 255 - i] = Block.BLOCKID_DIRT;

                for (int z = 0; z < 256; z++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[i, y, z] = Block.BLOCKID_DIRT;

                for (int z = 0; z < 256; z++)
                    for (int y = 0; y < 64 - i; y++)
                        terrain.blocks[255 - i, y, z] = Block.BLOCKID_DIRT;
            }



            if (genTrees)
                TerrainGenerator.GenerateTrees(terrain.blocks, size, 502, terrain.game.GetRandom);


            terrain.npt = new NonPlayableTerrain();
            byte[, ,] oldBlocks = terrain.blocks;
            Thread t = new Thread(poop => terrain.npt.InitializeBuffers(gd, oldBlocks));
            t.Name = "Edge World";
            t.IsBackground = true;
            t.Start();


            terrain.blocks = new byte[128, 64, 128];


            for (int x = 64; x < 128 + 64; x++)
                for (int y = 0; y < 64; y++)
                    for (int z = 64; z < 128 + 64; z++)
                    {
                        terrain.blocks[x - 64, y, z - 64] = oldBlocks[x, y, z];
                    }

            TerrainGenerator.GenerateWall(terrain, size);


            if (useLiquids)
                for (int x = 0; x < 128; x++)
                    for (int z = 0; z < 128; z++)
                    {
                        for (int y = 8; y >= 0; y--)
                        {
                            if (terrain.blocks[x, y, z] == Block.BLOCKID_AIR)
                                terrain.game.GetLiquidManager.AddPreInit(false, x, y, z);
                        }
                    }

        }

        public void CreateBlocksFromSave(GraphicsDevice gd, WorldEditor.WorldGeneration worldGen, BinaryReader br, int size, bool genTrees, int version= 999)
        {
            Size = size;

            if (worldGen == WorldEditor.WorldGeneration.Random)
            {
                CreateBlocksRandom(gd, this, false, size, genTrees);
            }
            else if (worldGen == WorldEditor.WorldGeneration.Flat)
            {
                CreateBlocksFlat(gd, this, false, size, genTrees);
            }
            else if( worldGen == WorldEditor.WorldGeneration.FlatWithCaves)
            {
                CreateBlocksFlatCaves(gd, this, false, size, genTrees);
            }
            else if (worldGen == WorldEditor.WorldGeneration.Island)
            {
                CreateBlocksIsland(gd, this, false, size, genTrees);
            }

            for (int x = 0; x < 128; x++)
            {
                for (int z = 0; z < 128; z++)
                {
                    int y = 63;
                    while (y >= 0)
                    {
                        byte lastBlock = br.ReadByte();
                        int howMany = br.ReadByte();

                        for (int j = 0; j < howMany; j++)
                        {
                            if (lastBlock == Block.BLOCKID_WATER || lastBlock == Block.BLOCKID_LAVA)
                            {
                                game.GetLiquidManager.AddPreInit(lastBlock == Block.BLOCKID_WATER, x, y, z);
                            }
                            else if (lastBlock == Block.BLOCKID_PITFALLBLOCK)
                            {
                                pitfallBlocks.Add(new PitfallBlock()
                                {
                                    X = (byte)x,
                                    Y = (byte)y,
                                    Z = (byte)z
                                });
                            }
                           
                            if (lastBlock != Block.BLOCKID_GLASSWALL)
                                blocks[x, y, z] = lastBlock;
                            y--;
                        }
                    }
                }
            }

            if (version <= 3)
            {
                int liquids = br.ReadInt32();
                for (int i = 0; i < liquids; i++)
                {
                    game.GetLiquidManager.AddPreInit(false, br.ReadInt16(), br.ReadInt16(), br.ReadInt16());
                }
            }

            this.gd = gd;
            parentNode = new QuadTreeNode(0, 0, 0, 128, this);
            parentNode.InitializeBuffers(gd);
        }
    }
}
