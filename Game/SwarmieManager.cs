using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using Miner_Of_Duty.Menus;
using Miner_Of_Duty.Game.Networking;

namespace Miner_Of_Duty.Game
{
    public interface ISwarmieManager
    {
        void Dispose();
        bool CheckForPlayerCollision(ref BoundingBox bb, ref Vector3 pos);
        SwarmStoreMenu StoreMenu { get; }
       
        ISwarmie[] GetSwarmies();

        void ResetClass();

        void Unsub();
        void Update(GameTime gameTime);
        void Draw(SpriteBatch sb);
        void Render(Camera camera);
    }

    public class SwarmieManager : ISwarmieManager
    {
        private Terrain terrain;
        private AStar.AStarMap map;
        public List<Vector3[]> paths;
        public List<GoldBlock> GoldBlocks;
        public SwarmGame game;
        public Swarmie[] swarmies;
        public SwarmStoreMenu ssm;
        public SwarmStoreMenu StoreMenu { get { return ssm; } }
        private CharacterClass clas;

        public ISwarmie[] GetSwarmies()
        {
            return swarmies;
        }

        public void Dispose()
        {
            swarmies = null;
            if(map != null)
                map.Dispose();
            map = null;
        }

        public SwarmieManager(SwarmGame game)
        {
            this.game = game;
            paths = new List<Vector3[]>();
            terrain = game.Terrain;
            map = new AStar.AStarMap(128, 64, 128);
            for (int x = 0; x < 128; x++)
                for (int y = 0; y < 64; y++)
                    for (int z = 0; z < 128; z++)
                    {
                        if (IsBlockWalkable(x, y, z) == false)
                            map.map[x, y, z].Opened = false;
                    }

            swarmies = new Swarmie[200];
            for (int i = 0; i < 200; i++)
            {
                swarmies[i] = new Swarmie();
            }

            terrain.BlockChangedEvent += BlockChange;
            GoldBlocks = new List<GoldBlock>();

            ssm = new SwarmStoreMenu(MenuBack);
            clas = new CharacterClass(
                new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false),
                new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
                new ItemSlot(InventoryItem.SandBlock),
                new ItemSlot(InventoryItem.StoneBlock));
            sm = (game.TeamManager as SwarmManager);
            (game.TeamManager as SwarmManager).RoundOverEvent += RoundOver;
            (game.TeamManager as SwarmManager).RoundStartEvent += StartRound;
        }

        public void ResetClass()
        {
            clas = new CharacterClass(
               new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false),
               new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
               new ItemSlot(InventoryItem.SandBlock),
               new ItemSlot(InventoryItem.StoneBlock));
        }

        public void Unsub()
        {
            terrain.BlockChangedEvent -= BlockChange;

            try
            {
                (game.TeamManager as SwarmManager).RoundOverEvent -= RoundOver;

                (game.TeamManager as SwarmManager).RoundStartEvent -= StartRound;
            }
            catch { }
        }

        private void MenuBack(object sender)
        {
            atStore = false;
            game.player.DontUseInput();
            game.player.ClearGamePad();
            game.player.updateMe = true;
        }

        public bool CheckForPlayerCollision(ref BoundingBox bb, ref Vector3 pos)
        {
            bool result;
            float dis;
            for (int i = 0; i < swarmies.Length; i++)
            {
                working = swarmies[i];
                if (working.Dead == false)
                {
                    Vector3.DistanceSquared(ref pos, ref working.Position, out dis);
                    if (dis <= 6)
                    {
                        bb.Intersects(ref working.bb, out result);
                        if (result)
                            return true;
                    }
                }
            }
            return false;
        }

        public bool CheckForCollision(ref BoundingBox bb, Swarmie swarmie)
        {
            bool result;
            float dis;
            for (int i = 0; i < swarmies.Length; i++)
            {
                working = swarmies[i];
                if (swarmie != working && working.Dead == false)
                {
                    Vector3.DistanceSquared(ref swarmie.Position, ref working.Position, out dis);
                    if (dis <= 4)
                    {
                        bb.Intersects(ref working.bb, out result);
                        if (result)
                            return true;
                    }
                }
            }
            return false;
        }

        public bool HurtSwarmie(Player attacker, float dmg, short id)
        {
            for (int i = 0; i < swarmies.Length; i++)
            {
                if (swarmies[i].ID == id)
                {
                    return swarmies[i].Attack(attacker, dmg);
                }
            }
            return false;
        }

        private float dist;
        public void AttackBlock(float dmg, int block)
        {
            if (block < GoldBlocks.Count)//im weary of a crash
            {
                GoldBlocks[block].Health -= dmg;
                if (GoldBlocks[block].Health < 0)
                {
                    Vector3 pos = Vector3.Zero;
                    for (int i = 0; i < game.goldBlocks.Count; i++)
                    {
                        if (game.goldBlocks[i] == paths[block][paths[block].Length - 1])
                        {
                            pos = game.goldBlocks[i];
                            break;
                        }
                    }
                    game.BlockChanged(ref pos, 0, false);
                    float bestDis = 69696969;
                    byte best = 32;
                    for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                    {
                        Vector3.Distance(ref game.players[MinerOfDuty.Session.AllGamers[i].Id].position, ref pos, out dist);
                        if (dist < bestDis)
                        {
                            best = MinerOfDuty.Session.AllGamers[i].Id;
                            bestDis = dist;
                        }
                    }
                    for (int i = 0; i < swarmies.Length; i++)
                    {
                        if (!swarmies[i].Dead && swarmies[i].pathToFollow == block)
                        {
                            swarmies[i].Attack(game.players[best], 0);
                        }
                    }
                    paths[block] = null;
                }
            }
        }

        private Vector3[] goldBlocksOnHold;
        public void StartRound(int round)
        {
            this.currentRound = round;

            goldBlocksOnHold = game.goldBlocks.ToArray<Vector3>();

            roundOver = false;
            paths.Clear();
            Thread t = new Thread(poop => GetPath(GetSpawnPoint(), game.goldBlocks[0], 0, PathCallBack));
            t.IsBackground = true;
            t.Start();
            //this starts a path for the first block

            GoldBlocks.Clear();
            for (int i = 0; i < game.goldBlocks.Count; i++)
                GoldBlocks.Add(new GoldBlock(100));

            if (atStore)
            {
                atStore = false;
                game.player.DontUseInput();
                game.player.ClearGamePad();
                game.player.updateMe = true;
            }
        }

        private int GetSwarmieHealth()
        {
            return (int)((((-.7057641171 * (currentRound * currentRound)) + (17.57755717 * currentRound) + 40.087627)) *
               (MinerOfDuty.Session.AllGamers.Count == 1 ? 1 :
                MinerOfDuty.Session.AllGamers.Count == 2 ? 1.15f :
                MinerOfDuty.Session.AllGamers.Count == 3 ? 1.375f :
                MinerOfDuty.Session.AllGamers.Count == 4 ? 1.5f : 1.5f));
        }

        private bool roundOver = true;
        private bool atStore = false;
        public int currentRound;
        public void RoundOver()
        {
            float furthest = 0;
            int fIndex = 0;
            Vector3 tmp = new Vector3(64, 3, 64);
            float tmpF;
            for (int i = 0; i < goldBlocksOnHold.Length; i++)
            {
                Vector3.Distance(ref goldBlocksOnHold[i], ref tmp, out tmpF);
                if (tmpF > furthest)
                {
                    fIndex = i;
                    furthest = tmpF;
                }
            }

            List<Vector3> tmpL = goldBlocksOnHold.ToList<Vector3>();
            tmpL.RemoveAt(fIndex);
            game.KillOffGoldBlocks();
            game.SpawnGoldBlocks(tmpL);

            roundOver = true;
            for (int i = 0; i < swarmies.Length; i++)
            {
                swarmies[i].Damage(100000);
            }

            if (game.player.dead)
            {
                game.player.SetWeapons(new CharacterClass(
                new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false),
                new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
                new ItemSlot(InventoryItem.SandBlock),
                new ItemSlot(InventoryItem.StoneBlock)));


                ResetClass();

                Vector3 spwn = game.TeamManager.GetReSpawnPoint(game.PersonalRandom, game);
                Packet.WriteRespawnPacket(MinerOfDuty.Session.LocalGamers[0], ref spwn);
            }
        }

        public Vector3[] ZombieSpawns = null;
        private Vector3 GetSpawnPoint()
        {
            if (ZombieSpawns == null)
            {
                float whatToDo = (float)game.Random.NextDouble();
                Vector3 tmpSpawn;
                if (whatToDo < .25f)
                {
                    tmpSpawn = new Vector3(4 + game.Random.Next(-3, 4), 0, 4 + game.Random.Next(-3, 4));
                }
                else if (whatToDo < .5f)
                {
                    tmpSpawn = new Vector3(124 + game.Random.Next(-3, 4), 0, 4 + game.Random.Next(-3, 4));
                }
                else if (whatToDo < .75f)
                {
                    tmpSpawn = new Vector3(124 + game.Random.Next(-3, 4), 0, 124 + game.Random.Next(-3, 4));
                }
                else
                {
                    tmpSpawn = new Vector3(4 + game.Random.Next(-3, 4), 0, 124 + game.Random.Next(-3, 4));
                }

                for (int y = 62; y >= 0; y--)
                {
                    if (Block.IsBlockSolid(game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Z]))
                        return new Vector3(tmpSpawn.X, y + 1, tmpSpawn.Z);
                }
            }
            else
            {
                int whichSpawnPoint = game.Random.Next(0, ZombieSpawns.Length);
                return ZombieSpawns[whichSpawnPoint];
            }
            return Vector3.Zero;
        }

        private void PathCallBack(int threadID, Vector3[] path)
        {
            if (path == null)
            {
                //put spawner 5 away from target
                Thread t = new Thread(poop => GetPath(game.goldBlocks[threadID] - new Vector3(2, 0, 2), game.goldBlocks[threadID], threadID, PathCallBack));
                t.IsBackground = true;
                t.Start();
            }
            else
            {
                paths.Add(path);
                if (paths.Count < game.goldBlocks.Count)
                {
                    Thread t = new Thread(poop => GetPath(GetSpawnPoint(), game.goldBlocks[threadID + 1], threadID + 1, PathCallBack));
                    t.IsBackground = true;
                    t.Start();
                }
            }
        }

        private bool IsBlockWalkable(int x, int y, int z)
        {
            if (terrain.blocks[x, y, z] == Block.BLOCKID_GOLD)
                return true;

            if (terrain.blocks[x, y, z] == Block.BLOCKID_AIR || terrain.blocks[x, y, z] == Block.BLOCKID_LAVA)
            {
                if (y - 1 >= 0 && Block.IsBlockSolid(terrain.blocks[x, y - 1, z]))//one below is solid(we need something to walk on)
                {
                    if (y + 1 < 64) //we need a little head room
                    {
                        if (terrain.blocks[x, y + 1, z] == Block.BLOCKID_AIR || terrain.blocks[x, y + 1, z] == Block.BLOCKID_LAVA)
                            return true;
                        else
                            return false;
                    }
                    else
                        return true;//we know there is air up there
                }
                else
                    return false;
            }
            else
                return false;
        }

        private void BlockChange(int x, int y, int z)
        {
            if (IsBlockWalkable(x, y, z))
                map.map[x, y, z].Opened = true;
            else
                map.map[x, y, z].Opened = false;

            if (y + 1 < 64)
            {
                if (IsBlockWalkable(x, y + 1, z))
                    map.map[x, y + 1, z].Opened = true;
                else
                    map.map[x, y + 1, z].Opened = false;
            }

            if (y - 1 >= 0)
            {
                if (IsBlockWalkable(x, y - 1, z))
                    map.map[x, y - 1, z].Opened = true;
                else
                    map.map[x, y - 1, z].Opened = false;
            }
        }

        private int zombieSendCount = -1;
        private int lastAddIndex = 0;
        private int pathI = 0;
        private int delay = 0;
        private short id = 0;
        private int storeYDelay = 0;
        private SwarmManager sm;
        private int frame = 0;
        public void Update(GameTime gameTime)
        {
            if (atStore)
                ssm.Update(gameTime);

            if (sm.pause == false)
            {
                if (roundOver == false)
                {
                    if (paths.Count > 0)
                    {
                        delay -= gameTime.ElapsedGameTime.Milliseconds;
                        if (delay <= 0)
                        {
                            int amountOfLivingZombies = 0;
                            for (int i = 0; i < swarmies.Length; i++)
                            {
                                if (swarmies[i] != null && swarmies[i].Dead == false)
                                    amountOfLivingZombies++;

                                if (amountOfLivingZombies >= 25)
                                    break;
                            }

                            if (amountOfLivingZombies == 25)
                            {
                                delay = 200;
                            }
                            else
                            {
                                delay = (int)(MathHelper.Lerp(20000, 13500, currentRound / 15f) / ((float)paths.Count + currentRound)); //17500 -> 12500
                                pathI++;
                                if (pathI >= paths.Count)
                                    pathI = 0;

                                if (paths[pathI] != null)
                                {
                                    bool beenAround = false;
                                    do
                                    {
                                        if (swarmies[lastAddIndex].DeadAndDeathAnimaitionDone)
                                        {
                                            swarmies[lastAddIndex].RebuildSwarmie(GetSwarmieHealth(), this, pathI, ++id);
                                            Packet.WriteSwarmieAdded(game.Me, id, swarmies[lastAddIndex].Position);
                                            break;
                                        }
                                        else
                                        {
                                            lastAddIndex++;
                                            if (lastAddIndex >= 200)
                                            {
                                                lastAddIndex = 0;
                                                if (beenAround)
                                                    break;
                                                beenAround = true;
                                            }
                                        }
                                    }
                                    while (true);

                                    if (game.Random.Next(0, 2) == 0)
                                        Audio.PlaySound(Audio.SOUND_ZOMBIE);
                                }
                            }
                        }
                    }

                    if (++frame >= 4)
                    {
                        frame = 0;

                        Packet.PacketWriter.Write(Packet.PACKETID_SWARMIEUPDATE);
                        Packet.PacketWriter.Write((float)gameTime.TotalGameTime.TotalMilliseconds);
                        int actuallySent = 0;
                        while(++zombieSendCount < swarmies.Length)
                        {
                            if (zombieSendCount < 0)
                                continue;

                            if (swarmies[zombieSendCount] != null && swarmies[zombieSendCount].Dead == false)
                            {
                                swarmies[zombieSendCount].WriteToPacketWriter();
                                actuallySent++;
                            }

                            if (actuallySent >= 10)
                                break;
                        }
                        if (zombieSendCount >= swarmies.Length)
                            zombieSendCount = -1;

                        try
                        {
                            game.Me.SendData(Packet.PacketWriter, Microsoft.Xna.Framework.Net.SendDataOptions.None);
                        }
                        catch (Exception) { }
                    }

                }
                else
                {
                    if (atStore == false)
                    {
                        if (Input.ControllingPlayerNewGamePadState.Buttons.X == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                            storeYDelay += gameTime.ElapsedGameTime.Milliseconds;
                        else
                            storeYDelay = 0;

                        if (storeYDelay > 200 && game.player.dead == false)
                        {
                            atStore = true;
                            ssm.SetClass(clas, game.player.inventory, game.player);
                            game.player.updateMe = false;
                        }
                    }
                }


                for (int i = 0; i < swarmies.Length; i++)
                {
                    working = swarmies[i];
                    if (working.DeadAndDeathAnimaitionDone == false)
                    {
                        working.Update(gameTime);
                    }
                }
            }
        }

        private Swarmie working;
        public void Render(Camera camera)
        {
            if (atStore == false)
                for (int i = 0; i < swarmies.Length; i++)
                {
                    working = swarmies[i];
                    if (working.DeadAndDeathAnimaitionDone == false)
                        working.Render(camera);
                }
        }

        public void Draw(SpriteBatch sb)
        {
            if (atStore)
            {
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);
                ssm.Draw(sb);
                (game.TeamManager as SwarmManager).DrawTime(sb);
            }
            else if (roundOver && game.player.dead == false)
            {
                sb.DrawString(Resources.NameFont, "Press X To Go To Store", new Vector2(640, 440), Color.White, 0, Resources.NameFont.MeasureString("Press X To Go To Store") / 2f, 1, SpriteEffects.None, 0);
            }
        }

        private delegate void GetPathCallback(int threadID, Vector3[] path);
        private void GetPath(Vector3 start, Vector3 target, int threadID, GetPathCallback callbackMethod)
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif
              
                Vector3[] path = null;
                
                     path = AStar.AStarNode.FindPath(
                        map[(int)start.X, (int)start.Y, (int)start.Z],
                        map[(int)target.X, (int)target.Y, (int)target.Z],
                        map);

                
                    map.ResetMap();


                    callbackMethod.Invoke(threadID, path);
                
            }
            catch (NullReferenceException) { }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

    }

    public class FooSwarmieManager : ISwarmieManager
    {
        private Dictionary<short, FooSwarmie> swarmies;
        private SwarmStoreMenu ssm;
        public SwarmStoreMenu StoreMenu { get { return ssm; } }
        private SwarmGame game;
        private SwarmManager sm;
        private CharacterClass clas;

        public ISwarmie[] GetSwarmies()
        {
            return swarmies.Values.ToArray();
        }

        public FooSwarmieManager(SwarmGame game)
        {
            this.game = game;
            sm = game.TeamManager as SwarmManager;
            swarmies = new Dictionary<short, FooSwarmie>();
            ssm = new SwarmStoreMenu(MenuBack);
            clas = new CharacterClass(
                new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false),
                new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
                new ItemSlot(InventoryItem.SandBlock),
                new ItemSlot(InventoryItem.StoneBlock));


            (game.TeamManager as SwarmManager).RoundOverEvent += RoundOver;
            (game.TeamManager as SwarmManager).RoundStartEvent += StartRound;
        }

        public void ResetClass()
        {
            clas = new CharacterClass(
               new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false),
               new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
               new ItemSlot(InventoryItem.SandBlock),
               new ItemSlot(InventoryItem.StoneBlock));
        }

        public void Dispose()
        {
            if(swarmies != null)
                swarmies.Clear();
            swarmies = null;
        }

        public void Unsub()
        {
            try
            {
                (game.TeamManager as SwarmManager).RoundOverEvent -= RoundOver;

                (game.TeamManager as SwarmManager).RoundStartEvent -= StartRound;
            }
            catch { }
        }

        private void MenuBack(object sender)
        {
            atStore = false;
            (game.TeamManager as SwarmManager).UnPause();
            game.player.DontUseInput();
            game.player.ClearGamePad();
            game.player.updateMe = true;
        }

        public void AddSwarmie(short id, Vector3 position)
        {
            swarmies.Add(id, new FooSwarmie(id, ref position));
        }

        public void KillSwarm(short id)
        {
            swarmies[id].Kill();
        }

        private Vector3[] goldBlocksOnHold;
        private bool roundOver = false;
        public void StartRound(int round)
        {
            goldBlocksOnHold = game.goldBlocks.ToArray<Vector3>();

            swarmies.Clear();
            
            roundOver = false;

            if (atStore)
            {
                atStore = false;
                game.player.DontUseInput();
                game.player.ClearGamePad();
                game.player.updateMe = true;
            }
        }

        public void RoundOver()
        {
            float furthest = 0;
            int fIndex = 0;
            Vector3 tmp = new Vector3(64, 3, 64);
            float tmpF;
            for (int i = 0; i < goldBlocksOnHold.Length; i++)
            {
                Vector3.Distance(ref goldBlocksOnHold[i], ref tmp, out tmpF);
                if (tmpF > furthest)
                {
                    fIndex = i;
                    furthest = tmpF;
                }
            }

            List<Vector3> tmpL = goldBlocksOnHold.ToList<Vector3>();
            tmpL.RemoveAt(fIndex);
            game.KillOffGoldBlocks();
            game.SpawnGoldBlocks(tmpL);
            

            //do some clean up
            foreach (short key in swarmies.Keys)
            {
                if (swarmies[key].DeadAndDeathAnimaitionDone == false)
                    swarmies[key].Kill();
            }
            roundOver = true;

            if (game.player.dead)
            {
                game.player.SetWeapons(new CharacterClass(
                new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false),
                new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
                new ItemSlot(InventoryItem.SandBlock),
                new ItemSlot(InventoryItem.StoneBlock)));
                Vector3 spwn = game.TeamManager.GetReSpawnPoint(game.PersonalRandom, game);
                Packet.WriteRespawnPacket(MinerOfDuty.Session.LocalGamers[0], ref spwn); 

                ResetClass();

            }
        }

        public bool CheckForPlayerCollision(ref BoundingBox bb, ref Vector3 pos)
        {
            bool result;
            float dis;
            ISwarmie working;
            foreach (short key in swarmies.Keys)
            {
                working = swarmies[key];
                if (working.Dead == false)
                {
                    Vector3.DistanceSquared(ref pos, ref working.Position, out dis);
                    if (dis <= 6)
                    {
                        bb.Intersects(ref working.bb, out result);
                        if (result)
                            return true;
                    }
                }
            }
            return false;
        }

        private bool atStore = false;
        private int storeYDelay = 0;
        public void Update(GameTime gameTime)
        {
            foreach (short key in swarmies.Keys)
            {
                if (swarmies[key].DeadAndDeathAnimaitionDone == false)
                    swarmies[key].Update(gameTime);
            }

            if (atStore)
                ssm.Update(gameTime);

            if (sm.pause == false && atStore == false && roundOver == true)
            {
                if (Input.ControllingPlayerNewGamePadState.Buttons.X == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
                    storeYDelay += gameTime.ElapsedGameTime.Milliseconds;
                else
                    storeYDelay = 0;

                if (storeYDelay > 200 && game.player.dead == false)
                {
                    atStore = true;
                    ssm.SetClass(clas, game.player.inventory, game.player);
                    game.player.updateMe = false;
                }
            }
        }

        public void Render(Camera camera)
        {
            foreach (short key in swarmies.Keys)
            {
                if (swarmies[key].DeadAndDeathAnimaitionDone == false)
                    swarmies[key].Render(camera);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (atStore)
            {
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);
                ssm.Draw(sb);
                (game.TeamManager as SwarmManager).DrawTime(sb);
            }
            else if (roundOver && game.player.dead == false)
            {
                sb.DrawString(Resources.NameFont, "Press X To Go To Store", new Vector2(640, 440), Color.White, 0, Resources.NameFont.MeasureString("Press X To Go To Store") / 2f, 1, SpriteEffects.None, 0);
            }
        }

        public void ReadMovements(float time)
        {
            while (Packet.PacketReader.BaseStream.Position < Packet.PacketReader.BaseStream.Length)
            {
                short id = Packet.PacketReader.ReadInt16();
                if (swarmies.ContainsKey(id))
                    swarmies[id].ReadToPacketReaderToSwarmie(time);
                else
                {
                    Packet.PacketReader.ReadUInt64();
                    Packet.PacketReader.ReadUInt16();

                   // Packet.PacketReader.ReadVector3();
                   // Packet.PacketReader.ReadSingle();
                   // Packet.PacketReader.ReadSingle();
                  //  Packet.PacketReader.ReadBoolean();
                }
            }
        }
    }
    
    public class GoldBlock
    {
        public float Health;
        public Swarmie IsTakenBy;

        public GoldBlock(float health)
        {
            Health = health;
            IsTakenBy = null;
        }
    }
}
