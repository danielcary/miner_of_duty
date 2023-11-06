using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Game
{
    public class LightingManager
    {
        private static byte[] lights;
        public Texture3D LightMap;
        private Texture3D A, B;
        private GraphicsDevice gd;
        private Terrain Terrain;
        private List<Vector3i> lightSourcesToAdd;
        private readonly object lightSourceLock = new object();

#if XBOX
        private static bool lastThread;
#endif

        public void AddRemoveLightSource(int x, int y, int z)
        {
            lock (lightSourceLock)
                lightSourcesToAdd.Add(new Vector3i(x, y, z));
        }

        public void Dispose()
        {
            lock (LightMapLock)
            {
                if (A != null && A.IsDisposed == false)
                    A.Dispose();
                if (B != null && B.IsDisposed == false)
                    B.Dispose();
                if (LightMap != null && LightMap.IsDisposed == false)
                    LightMap.Dispose();

                A = null;
                B = null;
                LightMap = null;
            }
        }

        public LightingManager(Terrain terrain, GraphicsDevice gd)
        {
            Terrain = terrain;
            this.gd = gd;

            A = new Texture3D(gd, 128, 64, 128, false, SurfaceFormat.Alpha8);
            B = new Texture3D(gd, 128, 64, 128, false, SurfaceFormat.Alpha8);

            lights = new byte[Terrain.MAXWIDTH * Terrain.MAXHEIGHT * Terrain.MAXDEPTH];
            doneLights = new byte[Terrain.MAXWIDTH * Terrain.MAXHEIGHT * Terrain.MAXDEPTH];

            lightSourcesToAdd = new List<Vector3i>();
        }

        public void DoSunPath(int cX, int cY, int cZ, byte dontGoTo)
        {
            int x = cX + 1;
            int y = cY;
            int z = cZ;

            if (dontGoTo != 1)
                if (x < Terrain.MAXWIDTH)
                    if (Block.IsBlockTransparent(Terrain.blocks[x, y, z]) &&
                         lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] < lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 1)
                    {
                        lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 1);
                        if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 2);
                    }

            x -= 2;

            if (dontGoTo != 2)
                if (x >= 0)
                    if (Block.IsBlockTransparent(Terrain.blocks[x, y, z]) &&
                         lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] < lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 1)
                    {
                        lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 1);
                        if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 1);
                    }

            x++;
            z++;

            if (dontGoTo != 3)
                if (z < Terrain.MAXDEPTH)
                    if (Block.IsBlockTransparent(Terrain.blocks[x, y, z]) &&
                        lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] < lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 1)
                    {
                        lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 1);
                        if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 4);
                    }

            z -= 2;
            if (dontGoTo != 4)
                if (z >= 0)
                    if (Block.IsBlockTransparent(Terrain.blocks[x, y, z]) &&
                         lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] < lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 1)
                    {
                        lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 1);
                        if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 3);
                    }

            z++;
            y--;

            if (dontGoTo != 5)
                if (y >= 0)
                    if (Block.IsBlockTransparent(Terrain.blocks[x, y, z]) &&
                         lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] < lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ])
                    {
                        lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ];
                        if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 6);
                    }

            y += 2;

            if (dontGoTo != 6)
                if (y < Terrain.MAXHEIGHT)
                    if (Block.IsBlockTransparent(Terrain.blocks[x, y, z]) &&
                         lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] < lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 2)
                    {
                        lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.MAXWIDTH + cZ] - 2);
                        if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 5);
                    }
        }

        public void LightWorld()
        {
            //zero out array
            Array.Clear(lights, 0, lights.Length);

            //sunlight
            bool hitNonTransparent = false;
            for (byte x = 0; x < Terrain.MAXWIDTH; x++)
                for (byte z = 0; z < Terrain.MAXDEPTH; z++)
                {
                    for (byte y = Terrain.MAXHEIGHT - 1; y > 0; y--)
                    {
                        if (hitNonTransparent == false)
                            if (!Block.IsBlockTransparent(Terrain.blocks[x, y, z]))
                            {
                                //break;
                                hitNonTransparent = true;
                                continue; //we keep going for lava in caves to be intialized
                            }
                        lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 9;

                        if (hitNonTransparent == false)
                            lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 9;
                        else if (Block.IsLightSource(Terrain.blocks[x, y, z]))
                            lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 9;
                        else
                            lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 0;
                    }
                    hitNonTransparent = false;
                }

            //// Light The World
            for (byte x = 0; x < Terrain.MAXWIDTH; x++)
                for (byte y = Terrain.MAXHEIGHT - 1; y > 0; y--)
                    for (byte z = 0; z < Terrain.MAXDEPTH; z++)
                    {
                        if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] != 9)
                            continue;
                        if (!Block.IsBlockTransparent(Terrain.blocks[x, y, z]))
                            continue;
                        DoSunPath(x, y, z, 0);
                    }

            A.SetData<byte>(lights);
            Buffer.BlockCopy(lights, 0, doneLights, 0, lights.Length);

            LightMap = A;
        }

        public void Update()
        {
            lock (lightSourceLock)
                if (lightSourcesToAdd.Count > 0)
                {
                    Thread t = new Thread(UpdateBatch);
                    t.IsBackground = true;
                    t.Start(lightSourcesToAdd.ToArray());
                    lightSourcesToAdd.Clear();
                }
        }

        private void UpdateBatch(object spot)
        {
            try
            {
                Vector3i[] spots = spot as Vector3i[];

                if (spots.Length == 1)
                {
                    BlockAddedOrRemoved(spots[0].X, spots[0].Y, spots[0].Z);
                }
                else
                {

#if XBOX
                if ((lastThread = !lastThread))
                    Thread.CurrentThread.SetProcessorAffinity(new int[] { 5 });
                else
                    Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                    List<List<Vector3i>> groups = new List<List<Vector3i>>();

                    groups.Add(new List<Vector3i>());
                    groups[0].Add(spots[0]);

                    for (int i = 1; i < spots.Length; i++)
                    {
                        bool foundOne = false;

                        for (int j = 0; j < groups.Count; j++)
                        {
                            for (int h = 0; h < groups[j].Count; h++)
                            {
                                if (Vector3i.DistanceSquared(ref spots[i], groups[j][h]) < 5) //or with a centered radius
                                {
                                    groups[j].Add(spots[i]);
                                    foundOne = true;
                                    break;
                                }
                            }

                            if (foundOne)
                                break;
                        }

                        if (!foundOne)
                        {
                            groups.Add(new List<Vector3i>());
                            groups[groups.Count - 1].Add(spots[i]);
                        }
                    }

                    for (int i = 0; i < groups.Count; i++)
                    {
                        if (groups[i].Count == 1)
                        {
                            BlockAddedOrRemoved(groups[i][0].X, groups[i][0].Y, groups[i][0].Z);
                        }
                        else
                        {
                            int min = 0, max = 0;
                            for (int j = 0; j < groups[i].Count; j++)
                            {
                                if (Vector3i.DistanceSquared(ref Vector3i.Zero, groups[i][j]) < Vector3i.DistanceSquared(ref Vector3i.Zero, groups[i][min]))
                                    min = j;
                            }

                            for (int j = 0; j < groups[i].Count; j++)
                            {
                                if (Vector3i.DistanceSquared(ref Vector3i.Zero, groups[i][j]) > Vector3i.DistanceSquared(ref Vector3i.Zero, groups[i][max]))
                                    max = j;
                            }
                            int maxY = 0;
                            for (int j = 0; j < groups[i].Count; j++)
                            {
                                if (groups[i][j].Y > groups[i][maxY].Y)
                                    maxY = j;
                            }
                            LightUpdateMinMax(groups[i][min], groups[i][max], groups[i][maxY].Y);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }

        }

        private void LightUpdateMinMax(Vector3i min, Vector3i max, int maxY)
        {
            lock (UpdateLock)
            {
                int minX = min.X - 9;
                int minZ = min.Z - 9;
                if (minX < 0)
                    minX = 0;
                if (minZ < 0)
                    minZ = 0;
                int maxX = max.X + 9;
                int maxZ = max.Z + 9;
                if (maxX > 128)
                    maxX = 128;
                if (maxZ > 128)
                    maxZ = 128;
                maxY += 5;
                int minY = min.Y - 2;
                if (maxY > 64)
                    maxY = 64;
                // if (minY < 0)
                minY = 0;

                for (int x = minX; x <= maxX; x++)
                {
                    if (x >= Terrain.MAXWIDTH)
                        continue;
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        if (z >= Terrain.MAXDEPTH)
                            continue;
                        for (int y = maxY; y >= minY; y--)
                        {
                            if (y >= Terrain.MAXHEIGHT)
                                continue;
                            if (y == 63)
                                lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 9;
                            else if (Block.IsLightSource(Terrain.blocks[x, y, z]))
                                lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 9;
                            else
                                lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 0;
                        }
                    }
                }

                // Light The World
                for (int x = minX - 1; x <= maxX + 1; x++)
                {
                    if (x < 0 || x >= Terrain.MAXWIDTH)
                        continue;
                    for (int z = minZ - 1; z <= maxZ + 1; z++)
                    {
                        if (z < 0 || z >= Terrain.MAXWIDTH)
                            continue;
                        for (int y = maxY + 1; y >= minY - 1; y--)
                        {
                            if (y < 0 || y >= Terrain.MAXHEIGHT)
                                continue;
                            if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] == 0 || !Block.IsBlockTransparent(Terrain.blocks[x, y, z]))
                                continue;
                            DoSunPath((byte)x, (byte)y, (byte)z, 0);
                        }
                    }
                }

                lock (needsToUpdateLock)
                    needsToUpdate = true;

                lock (DoneLights)
                    Buffer.BlockCopy(lights, 0, doneLights, 0, lights.Length);
            }
        }

        /// <summary>
        /// Call after all blocks have been added or removed
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// 
        public void BlockAddedOrRemoved(int X, int Y, int Z)
        {
            try
            {
#if XBOX
            if ((lastThread = !lastThread))
                Thread.CurrentThread.SetProcessorAffinity(new int[] { 5 });
            else
                Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                lock (UpdateLock)
                {

                    lights[X * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + Y * Terrain.MAXWIDTH + Z] = 0;
                    int minX = X - 9;
                    int minZ = Z - 9;
                    if (minX < 0)
                        minX = 0;
                    if (minZ < 0)
                        minZ = 0;
                    int maxX = X + 9;
                    int maxZ = Z + 9;
                    if (maxX > 128)
                        maxX = 128;
                    if (maxZ > 128)
                        maxZ = 128;
                    int maxY = Y + 5;
                    int minY = Y - 2;
                    if (maxY > 64)
                        maxY = 64;
                    // if (minY < 0)
                    minY = 0;

                    for (int x = minX; x <= maxX; x++)
                    {
                        if (x >= Terrain.MAXWIDTH)
                            continue;
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            if (z >= Terrain.MAXDEPTH)
                                continue;
                            for (int y = maxY; y >= minY; y--)
                            {
                                if (y >= Terrain.MAXHEIGHT)
                                    continue;
                                if (y == 63)
                                    lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 9;
                                else if (Block.IsLightSource(Terrain.blocks[x, y, z]))
                                    lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 9;
                                else
                                    lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] = 0;
                            }
                        }
                    }

                    // Light The World
                    for (int x = minX - 1; x <= maxX + 1; x++)
                    {
                        if (x < 0 || x >= Terrain.MAXWIDTH)
                            continue;
                        for (int z = minZ - 1; z <= maxZ + 1; z++)
                        {
                            if (z < 0 || z >= Terrain.MAXWIDTH)
                                continue;
                            for (int y = maxY + 1; y >= minY - 1; y--)
                            {
                                if (y < 0 || y >= Terrain.MAXHEIGHT)
                                    continue;
                                if (lights[x * Terrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.MAXWIDTH + z] == 0 || !Block.IsBlockTransparent(Terrain.blocks[x, y, z]))
                                    continue;
                                DoSunPath((byte)x, (byte)y, (byte)z, 0);
                            }
                        }
                    }

                    lock (needsToUpdateLock)
                        needsToUpdate = true;

                    lock (DoneLights)
                        Buffer.BlockCopy(lights, 0, doneLights, 0, lights.Length);

                }
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private readonly object UpdateLock = new object();
        private readonly object needsToUpdateLock = new object();
        private readonly object LightMapLock = new object();
        private readonly object DoneLights = new object();
        private readonly object IsReadyLock = new object();
        public readonly object LightMapTextureLock = new object();
        private bool isBReady = false;
        private bool isAReady = false;
        private bool needsToUpdate = false;
        private bool isUpdating = false;
        private byte[] doneLights;
        private int timesRan = 0;
        public void SwitchTextures()
        {
            if (isUpdating)
                return;

            if (LightMap == A)
            {
                if (isBReady)
                {
                    lock (LightMapTextureLock)
                        LightMap = B;
                    lock (IsReadyLock)
                        isBReady = false;
                }
            }
            else if (LightMap == B)
                if (isAReady)
                {
                    lock (LightMapTextureLock)
                        LightMap = A;
                    lock (IsReadyLock)
                        isAReady = false;
                }

            lock (needsToUpdateLock)
            {
                if (needsToUpdate == false)
                    return;
                else
                {
                    needsToUpdate = false;
                    if (isUpdating)
                    {
                        needsToUpdate = true;
                        return;
                    }
                }
            }

            Thread t = new Thread(UpdateLightMap);
            t.Name = "Update Light Map: " + timesRan++;
            t.IsBackground = true;
            t.Start();
        }

        private void UpdateLightMap()
        {
            try
            {
#if XBOX
            if ((lastThread = !lastThread))
                Thread.CurrentThread.SetProcessorAffinity(new int[] { 5 });
            else
                Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                lock (LightMapLock)
                {
                    if (isUpdating)
                        return;

                    if (A == null || B == null)
                        return;

                    isUpdating = true;

                    if (LightMap == A)
                    {
                        //makeSureB isnt used
                        for (int i = 0; i < 16; i++)
                        {
                            if (gd.Textures[i] == B)
                                gd.Textures[i] = null;
                        }
                        try
                        {
                            lock (DoneLights)
                                lock (LightMapTextureLock)
                                    B.SetData<byte>(doneLights);
                            lock (IsReadyLock)
                                isBReady = true;
                        }
                        catch (Exception) { }
                    }
                    else if (LightMap == B)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            if (gd.Textures[i] == A)
                                gd.Textures[i] = null;
                        }
                        try
                        {
                            lock (DoneLights)
                                lock (LightMapTextureLock)
                                    A.SetData<byte>(doneLights);
                            lock (IsReadyLock)
                                isAReady = true;
                        }
                        catch (Exception) { }
                    }

                    isUpdating = false;
                }
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }

        }


        private static void DoSunPath(int cX, int cY, int cZ, byte dontGoTo, byte[] lights, byte[, ,] blocks)
        {
            int x = cX + 1;
            int y = cY;
            int z = cZ;

            if (dontGoTo != 1)
                if (x < Terrain.NonPlayableTerrain.MAXWIDTH)
                    if (Block.IsBlockTransparent(blocks[x, y, z]) &&
                         lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] < lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 1)
                    {
                        lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 1);
                        if (lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 2, lights, blocks);
                    }

            x -= 2;

            if (dontGoTo != 2)
                if (x >= 0)
                    if (Block.IsBlockTransparent(blocks[x, y, z]) &&
                         lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] < lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 1)
                    {
                        lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 1);
                        if (lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 1, lights, blocks);
                    }

            x++;
            z++;

            if (dontGoTo != 3)
                if (z < Terrain.NonPlayableTerrain.MAXDEPTH)
                    if (Block.IsBlockTransparent(blocks[x, y, z]) &&
                        lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] < lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 1)
                    {
                        lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 1);
                        if (lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 4, lights, blocks);
                    }

            z -= 2;
            if (dontGoTo != 4)
                if (z >= 0)
                    if (Block.IsBlockTransparent(blocks[x, y, z]) &&
                         lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] < lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 1)
                    {
                        lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 1);
                        if (lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 3, lights, blocks);
                    }

            z++;
            y--;

            if (dontGoTo != 5)
                if (y >= 0)
                    if (Block.IsBlockTransparent(blocks[x, y, z]) &&
                         lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] < lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ])
                    {
                        lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] = lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ];
                        if (lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 6, lights, blocks);
                    }

            y += 2;

            if (dontGoTo != 6)
                if (y < Terrain.MAXHEIGHT)
                    if (Block.IsBlockTransparent(blocks[x, y, z]) &&
                         lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] < lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 2)
                    {
                        lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] = (byte)(lights[cX * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + cY * Terrain.NonPlayableTerrain.MAXWIDTH + cZ] - 2);
                        if (lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] > 0)
                            DoSunPath(x, y, z, 5, lights, blocks);
                    }
        }

        public static void LightAWorld(Texture3D textureToLight, byte[, ,] blocks)
        {
            //zero out array
            byte[] lights = new byte[Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT * Terrain.NonPlayableTerrain.MAXWIDTH];
            Array.Clear(lights, 0, lights.Length);

            //sunlight
            for (int x = 0; x < Terrain.NonPlayableTerrain.MAXWIDTH; x++)
                for (int z = 0; z < Terrain.NonPlayableTerrain.MAXDEPTH; z++)
                {
                    for (int y = Terrain.MAXHEIGHT - 1; y > 0; y--)
                    {
                        if (!Block.IsBlockTransparent(blocks[x, y, z]))
                        {
                            break;
                            // hitNonTransparent = true;
                            //continue; //we keep going for lava in caves to be intialized
                        }
                        lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] = 9;
                    }
                }

            //// Light The World
            for (int x = 0; x < Terrain.NonPlayableTerrain.MAXWIDTH; x++)
                for (int y = Terrain.MAXHEIGHT - 1; y > 0; y--)
                    for (int z = 0; z < Terrain.NonPlayableTerrain.MAXDEPTH; z++)
                    {
                        if (lights[x * Terrain.NonPlayableTerrain.MAXWIDTH * Terrain.MAXHEIGHT + y * Terrain.NonPlayableTerrain.MAXWIDTH + z] != 9)
                            continue;
                        if (!Block.IsBlockTransparent(blocks[x, y, z]))
                            continue;
                        DoSunPath(x, y, z, 0, lights, blocks);
                    }

            try
            {
                textureToLight.SetData<byte>(lights);
            }
            catch (Exception)
            {

            }
        }
    }
}

