using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Threading;

namespace Miner_Of_Duty.Game
{
    public class LiquidManager
    {
        public static Texture2D WaterTexture, LavaTexture;
        public static Liquid[, ,] Liquidss; // for fast indexing
        public Liquid[, ,] Liquids { get { return Liquidss; } set { Liquidss = value; } }
        private ITerrainOwner game;
        private GraphicsDevice gd;

        private Queue<Vector3i> RemoveWaterQueue, RemoveLavaQueue;
        private Queue<Vector4i> AddWaterQueue, AddLavaQueue;
        private Queue<Vector3i> BlockPlacedOrRemoved;

        private LiquidOctTree<Water> waterParentNode;
        private LiquidOctTree<Lava> lavaParentNode;

        public void Dispose()
        {
            KillWaterManagerThread();
            lavaParentNode.Dispose();
            waterParentNode.Dispose();
        }

        public LiquidManager(ITerrainOwner game, GraphicsDevice gd)
        {
            this.gd = gd;
            this.game = game;
            Liquid.game = game;
            Liquids = new Liquid[128, 64, 128];
            AddWaterQueue = new Queue<Vector4i>();
            RemoveWaterQueue = new Queue<Vector3i>();
            AddLavaQueue = new Queue<Vector4i>();
            RemoveLavaQueue = new Queue<Vector3i>();
            BlockPlacedOrRemoved = new Queue<Vector3i>();

            waterParentNode = new LiquidOctTree<Water>(0, 0, 0, 128, gd);
            lavaParentNode = new LiquidOctTree<Lava>(0, 0, 0, 128, gd);

            rs.CullMode = CullMode.None;
        }
        Thread updateThread;
        public void Start()
        {
            updateThread = new Thread(DoUpdate);
            updateThread.IsBackground = true;
            updateThread.Name = "Water Thread";
            updateThread.Start();
        }

        private readonly object removeWaterLock = new object(), addWaterLock = new object(), removeLavaLock = new object(), addLavaLock = new object(), addremoveBlock = new object();

        public void AddPreInit(bool water, int x, int y, int z)
        {
            if (water)
            {
                game.GetTerrain.blocks[x, y, z] = Block.BLOCKID_WATER;
                Liquids[x, y, z] = new Water((byte)x, (byte)y, (byte)z, (sbyte)10);
                waterParentNode.AddLiquid(Liquids[x, y, z] as Water);
            }
            else
            {
                game.GetTerrain.blocks[x, y, z] = Block.BLOCKID_LAVA;
                Liquids[x, y, z] = new Lava((byte)x, (byte)y, (byte)z, (sbyte)10);
                lavaParentNode.AddLiquid(Liquids[x, y, z] as Lava);
            }
        }

        public void BlockAddedOrRemoved(int x, int y, int z)
        {
            lock (addremoveBlock)
                BlockPlacedOrRemoved.Enqueue(new Vector3i(x, y, z));
        }

        public void RemoveWaterBlock(int x, int y, int z)
        {
            lock (removeWaterLock)
                RemoveWaterQueue.Enqueue(new Vector3i(x, y, z));
        }

        public void RemoveLavaBlock(int x, int y, int z)
        {
            lock (removeLavaLock)
                RemoveLavaQueue.Enqueue(new Vector3i(x, y, z));
        }

        public void AddSourceWaterBlock(int x, int y, int z)
        {
            lock (addWaterLock)
                AddWaterQueue.Enqueue(new Vector4i(x, y, z, 10));
        }

        public void AddSourceLavaBlock(int x, int y, int z)
        {
            lock (addLavaLock)
                AddLavaQueue.Enqueue(new Vector4i(x, y, z, 10));
        }

        public void AddWaterBlock(int x, int y, int z, int waterLevel)
        {
            lock (addWaterLock)
                AddWaterQueue.Enqueue(new Vector4i(x, y, z, waterLevel));
        }

        public void AddLavaBlock(int x, int y, int z, int lavaLevel)
        {
            lock (addLavaLock)
                AddLavaQueue.Enqueue(new Vector4i(x, y, z, lavaLevel));
        }

        public bool IsSpaceOnGround(byte X, byte Y, byte Z)
        {
            if (Y - 1 == -1)
                return true;
            if (Block.IsBlockSolid(game.GetTerrain.blocks[X, Y - 1, Z]))
                return true;
            else
                return false;
        }

        private Vector4i water4;
        private Vector3i water3;

        public void RemoveNow(int x, int y, int z)
        {
            pos.X = x;
            pos.Y = y;
            pos.Z = z;
            if (Liquids[x, y, z] is Water)
                waterParentNode.RemoveLiquid(ref pos);
            else if (Liquids[x, y, z] is Lava)
                lavaParentNode.RemoveLiquid(ref pos);
            //else
            //     throw new Exception();

            Liquids[x, y, z] = null;
            lock (game.GetTerrain.EditBlocksLock) game.GetTerrain.blocks[x, y, z] = Block.BLOCKID_AIR;
            game.GetLightingManager.AddRemoveLightSource(x, y, z);
        }

        private Vector3 pos = Vector3.Zero;
        public void MarkWaterDirty(int x, int y, int z)
        {
            pos.X = x;
            pos.Y = y;
            pos.Z = z;
            waterParentNode.MarkDirty(ref pos);

            pos.X += 1;
            waterParentNode.MarkDirty(ref pos);

            pos.X -= 2;
            waterParentNode.MarkDirty(ref pos);

            pos.X += 1;

            pos.Z += 1;
            waterParentNode.MarkDirty(ref pos);

            pos.Z -= 2;
            waterParentNode.MarkDirty(ref pos);
        }

        /// <summary>
        /// NOT THREAD SAFE
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void MarkLavaDirty(int x, int y, int z)
        {
            pos.X = x;
            pos.Y = y;
            pos.Z = z;
            lavaParentNode.MarkDirty(ref pos);

            pos.X += 1;
            lavaParentNode.MarkDirty(ref pos);

            pos.X -= 2;
            lavaParentNode.MarkDirty(ref pos);

            pos.X += 1;

            pos.Z += 1;
            lavaParentNode.MarkDirty(ref pos);

            pos.Z -= 2;
            lavaParentNode.MarkDirty(ref pos);
        }

        private bool stopUpdating = false;
        public void KillWaterManagerThread()
        {
            stopUpdating = true;
        }

        private void DoUpdate()
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 5 });
#endif
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                while (stopUpdating == false)
                {
                    lock (addWaterLock)
                        while (AddWaterQueue.Count > 0)
                        {
                            water4 = AddWaterQueue.Dequeue();
                           // if (!(Liquids[water4.X, water4.Y, water4.Z] is Water))
                            //    game.GetLightingManager.AddRemoveLightSource(water4.X, water4.Y, water4.Z);
                            if (Liquids[water4.X, water4.Y, water4.Z] != null)
                            {
                                pos.X = water4.X;
                                pos.Y = water4.Y;
                                pos.Z = water4.Z;
                                if (Liquids[water4.X, water4.Y, water4.Z] is Water)
                                    waterParentNode.RemoveLiquid(ref pos);
                                else
                                    lavaParentNode.RemoveLiquid(ref pos);
                                Liquids[water4.X, water4.Y, water4.Z] = null;
                            }
                            lock (game.GetTerrain.EditBlocksLock) game.GetTerrain.blocks[water4.X, water4.Y, water4.Z] = Block.BLOCKID_WATER;
                            Liquids[water4.X, water4.Y, water4.Z] = new Water((byte)water4.X, (byte)water4.Y, (byte)water4.Z, (sbyte)water4.W);
                            waterParentNode.AddLiquid(Liquids[water4.X, water4.Y, water4.Z] as Water);
                        }
                    lock (addLavaLock)
                        while (AddLavaQueue.Count > 0)
                        {
                            water4 = AddLavaQueue.Dequeue();
                            if (!(Liquids[water4.X, water4.Y, water4.Z] is Lava))
                                game.GetLightingManager.AddRemoveLightSource(water4.X, water4.Y, water4.Z);
                            if (Liquids[water4.X, water4.Y, water4.Z] != null)
                            {
                                pos.X = water4.X;
                                pos.Y = water4.Y;
                                pos.Z = water4.Z;
                                if (Liquids[water4.X, water4.Y, water4.Z] is Water)
                                    waterParentNode.RemoveLiquid(ref pos);
                                else
                                    lavaParentNode.RemoveLiquid(ref pos);
                                Liquids[water4.X, water4.Y, water4.Z] = null;
                            }
                            lock (game.GetTerrain.EditBlocksLock) game.GetTerrain.blocks[water4.X, water4.Y, water4.Z] = Block.BLOCKID_LAVA;
                            Liquids[water4.X, water4.Y, water4.Z] = new Lava((byte)water4.X, (byte)water4.Y, (byte)water4.Z, (sbyte)water4.W);
                            lavaParentNode.AddLiquid(Liquids[water4.X, water4.Y, water4.Z] as Lava);
                        }
                    lock (removeWaterLock)
                        while (RemoveWaterQueue.Count > 0)
                        {
                            water3 = RemoveWaterQueue.Dequeue();
                            if (game.GetTerrain.blocks[water3.X, water3.Y, water3.Z] == Block.BLOCKID_WATER)
                                lock (game.GetTerrain.EditBlocksLock) game.GetTerrain.blocks[water3.X, water3.Y, water3.Z] = Block.BLOCKID_AIR;
                            //game.GetLightingManager.AddRemoveLightSource(water3.X, water3.Y, water3.Z);
                            pos.X = water3.X;
                            pos.Y = water3.Y;
                            pos.Z = water3.Z;
                            waterParentNode.RemoveLiquid(ref pos);
                            Liquids[water3.X, water3.Y, water3.Z] = null;
                        }
                    lock (removeLavaLock)
                        while (RemoveLavaQueue.Count > 0)
                        {
                            water3 = RemoveLavaQueue.Dequeue();
                            if (game.GetTerrain.blocks[water3.X, water3.Y, water3.Z] == Block.BLOCKID_LAVA)
                                lock (game.GetTerrain.EditBlocksLock) game.GetTerrain.blocks[water3.X, water3.Y, water3.Z] = Block.BLOCKID_AIR;
                            game.GetLightingManager.AddRemoveLightSource(water3.X, water3.Y, water3.Z);
                            pos.X = water3.X;
                            pos.Y = water3.Y;
                            pos.Z = water3.Z;
                            lavaParentNode.RemoveLiquid(ref pos);
                            Liquids[water3.X, water3.Y, water3.Z] = null;

                        }
                    lock (addremoveBlock)
                        while (BlockPlacedOrRemoved.Count > 0)
                        {
                            water3 = BlockPlacedOrRemoved.Dequeue();
                            Vector3 pos = new Vector3(water3.X, water3.Y, water3.Z);
                            waterParentNode.MarkDirty(ref pos);
                            lavaParentNode.MarkDirty(ref pos);

                            pos.X += 1;
                            waterParentNode.MarkDirty(ref pos);
                            lavaParentNode.MarkDirty(ref pos);

                            pos.X -= 2;
                            waterParentNode.MarkDirty(ref pos);
                            lavaParentNode.MarkDirty(ref pos);

                            pos.X += 1;

                            pos.Z += 1;
                            waterParentNode.MarkDirty(ref pos);
                            lavaParentNode.MarkDirty(ref pos);

                            pos.Z -= 2;
                            waterParentNode.MarkDirty(ref pos);
                            lavaParentNode.MarkDirty(ref pos);
                        }
                    sw.Stop();
                    short delta = (short)sw.Elapsed.Milliseconds;
                    sw.Reset();
                    sw.Start();
                    waterParentNode.Update(delta);
                    lavaParentNode.Update(delta);
                    waterParentNode.RebuildBuffers();
                    lavaParentNode.RebuildBuffers();
                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private RasterizerState rs = new RasterizerState();
        public void Render(BoundingFrustum bf)
        {
            rs.Tag = gd.RasterizerState;
            gd.RasterizerState = rs;

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques[game.GetUnderLiquid == Terrain.UnderLiquid.Water ? "FogWaterLight" : "FogLightLiquid"];


            Resources.BlockEffect.Parameters["Texture0"].SetValue(WaterTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            waterParentNode.Render(bf);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques[game.GetUnderLiquid == Terrain.UnderLiquid.Lava ? "FogLavaNoLight" : "FogLightLiquid"];

            Resources.BlockEffect.Parameters["Texture0"].SetValue(LavaTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            lavaParentNode.Render(bf);


            gd.RasterizerState = rs.Tag as RasterizerState;
        }


        internal class LiquidOctTree<T> where T : Liquid
        {
            private const int MAXSIZE = 16;
            private LiquidOctTree<T>[] children;
            private VertexBuffer vertexBuffer;
            private GraphicsDevice gd;
            private bool isDirty;
            private List<T> liquids;
            private BoundingBox box;
            private ContainmentType result;

            private readonly object liquidBufferLock = new object();
            private VertexBuffer newVB;

            public LiquidOctTree(int x, int y, int z, int size, GraphicsDevice gd)
            {
                if (size == MAXSIZE)
                {
                    this.gd = gd;
                    liquids = new List<T>(8);
                }
                else
                {
                    int newSize = size / 2;
                    children = new LiquidOctTree<T>[8];
                    children[0] = new LiquidOctTree<T>(x, y, z, newSize, gd);
                    children[1] = new LiquidOctTree<T>(x + newSize, y, z, newSize, gd);
                    children[2] = new LiquidOctTree<T>(x, y, z + newSize, newSize, gd);
                    children[3] = new LiquidOctTree<T>(x + newSize, y, z + newSize, newSize, gd);
                    children[4] = new LiquidOctTree<T>(x, y + (newSize / 2), z, newSize, gd);
                    children[5] = new LiquidOctTree<T>(x + newSize, y + (newSize / 2), z, newSize, gd);
                    children[6] = new LiquidOctTree<T>(x, y + (newSize / 2), z + newSize, newSize, gd);
                    children[7] = new LiquidOctTree<T>(x + newSize, y + (newSize / 2), z + newSize, newSize, gd);
                }
                box = new BoundingBox(new Vector3(x, y, z), new Vector3(x + size, y + (size / 2), z + size));
            }


            public void Update(short delta)
            {
                if (children == null)
                {
                    for (int i = 0; i < liquids.Count; i++)
                    {
                        liquids[i].Update(delta);
                    }
                }
                else
                {
                    children[0].Update(delta);
                    children[1].Update(delta);
                    children[2].Update(delta);
                    children[3].Update(delta);
                    children[4].Update(delta);
                    children[5].Update(delta);
                    children[6].Update(delta);
                    children[7].Update(delta);
                }
            }

            public void RebuildBuffers()
            {
                if (isDirty)
                {
                    isDirty = false;
                    if (children != null)
                    {
                        children[0].RebuildBuffers();
                        children[1].RebuildBuffers();
                        children[2].RebuildBuffers();
                        children[3].RebuildBuffers();
                        children[4].RebuildBuffers();
                        children[5].RebuildBuffers();
                        children[6].RebuildBuffers();
                        children[7].RebuildBuffers();
                    }
                    else
                    {
                        Rebuild();
                    }
                }
            }

            private void Rebuild()
            {
                if (liquids.Count == 0)
                {

                    if (newVB != null)
                    {
                        newVB.Dispose();
                        newVB = null;
                    }

                }
                else
                {

                    bool[] faces = new bool[6];
                    int usedFaces = 0;

                    for (int i = 0; i < liquids.Count; i++)
                    {
                        liquids[i].GetFaces(faces);

                        if (faces[0])
                            usedFaces++;
                        if (faces[1])
                            usedFaces++;
                        if (faces[2])
                            usedFaces++;
                        if (faces[3])
                            usedFaces++;
                        if (faces[4])
                            usedFaces++;
                        if (faces[5])
                            usedFaces++;
                    }

                    VertexWaterTextureLight[] data = new VertexWaterTextureLight[6 * usedFaces];
                    int vert = 0;
                    for (int i = 0; i < liquids.Count; i++)
                    {
                        liquids[i].GetFaces(faces);
                        liquids[i].CreateLiquidBlock(ref vert, data, faces);
                    }

                    if (newVB != null)
                    {
                        newVB.Dispose();
                    }

                    if (gd.IsDisposed)
                        return;

                    if (usedFaces == 0)
                    {
                        newVB = null;
                    }
                    else
                    {
                        newVB = new VertexBuffer(gd, typeof(VertexWaterTextureLight), 6 * usedFaces, BufferUsage.WriteOnly);
                        newVB.SetData<VertexWaterTextureLight>(data);
                    }
                }

                lock (liquidBufferLock)
                {
                    VertexBuffer tmp = vertexBuffer;
                    vertexBuffer = newVB;
                    newVB = tmp;
                }
            }

            public void AddLiquid(T liquid)
            {
                pos.X = liquid.X;
                pos.Y = liquid.Y;
                pos.Z = liquid.Z;
                AddLiquid(liquid, ref pos);
            }

            private bool AddLiquid(T liquid, ref Vector3 pos)
            {
                box.Contains(ref pos, out result);
                if (result == ContainmentType.Disjoint)
                    return false;

                isDirty = true;

                if (children != null)
                {
                    if (children[0].AddLiquid(liquid, ref pos))
                        return true;
                    else if (children[1].AddLiquid(liquid, ref pos))
                        return true;
                    else if (children[2].AddLiquid(liquid, ref pos))
                        return true;
                    else if (children[3].AddLiquid(liquid, ref pos))
                        return true;
                    else if (children[4].AddLiquid(liquid, ref pos))
                        return true;
                    else if (children[5].AddLiquid(liquid, ref pos))
                        return true;
                    else if (children[6].AddLiquid(liquid, ref pos))
                        return true;
                    else if (children[7].AddLiquid(liquid, ref pos))
                        return true;
                    else
                        return true;
                }
                else
                {
                    liquids.Add(liquid);
                    return true;
                }
            }

            private Vector3 pos = Vector3.Zero;
            public void RemoveLiquid(ref Vector3 pos)
            {
                removeLiquid(ref pos);
            }

            private bool removeLiquid(ref Vector3 pos)
            {
                box.Contains(ref pos, out result);

                if (result == ContainmentType.Disjoint)
                    return false;

                isDirty = true;

                if (children != null)
                {
                    if (children[0].removeLiquid(ref pos))
                        return true;
                    else if (children[1].removeLiquid(ref pos))
                        return true;
                    else if (children[2].removeLiquid(ref pos))
                        return true;
                    else if (children[3].removeLiquid(ref pos))
                        return true;
                    else if (children[4].removeLiquid(ref pos))
                        return true;
                    else if (children[5].removeLiquid(ref pos))
                        return true;
                    else if (children[6].removeLiquid(ref pos))
                        return true;
                    else if (children[7].removeLiquid(ref pos))
                        return true;
                    else
                        return true;
                }
                else
                {
                    for (int i = 0; i < liquids.Count; i++)
                    {
                        Liquid l = liquids[i];
                        if (l.X == pos.X && l.Y == pos.Y && l.Z == pos.Z)
                        {
                            liquids.RemoveAt(i);
                            return true;
                        }
                    }

                    return true;
                }
            }

            public bool MarkDirty(ref Vector3 pos)
            {
                box.Contains(ref pos, out result);

                if (result == ContainmentType.Disjoint)
                    return false;
                else
                    isDirty = true;

                if (children != null)
                {
                    if (children[0].MarkDirty(ref pos))
                        return true;
                    else if (children[1].MarkDirty(ref pos))
                        return true;
                    else if (children[2].MarkDirty(ref pos))
                        return true;
                    else if (children[3].MarkDirty(ref pos))
                        return true;
                    else if (children[4].MarkDirty(ref pos))
                        return true;
                    else if (children[5].MarkDirty(ref pos))
                        return true;
                    else if (children[6].MarkDirty(ref pos))
                        return true;
                    else if (children[7].MarkDirty(ref pos))
                        return true;
                    else
                        return true;
                }
                else
                {
                    return true;
                }
            }


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
                }
            }


            private ContainmentType renderResults;
            public void Render(BoundingFrustum bf)
            {
                bf.Contains(ref box, out renderResults);

                if (renderResults != ContainmentType.Disjoint)
                {
                    if (children != null)
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
                    else
                    {
                        lock (liquidBufferLock)
                        {
                            if (vertexBuffer != null)
                            {
                                gd.SetVertexBuffer(vertexBuffer);
                                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, vertexBuffer.VertexCount / 3);
                            }
                        }
                    }
                }
            }


        }

    }


}
