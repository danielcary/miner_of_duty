using Miner_Of_Duty.LobbyCode;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Miner_Of_Duty.Menus;
using Miner_Of_Duty.Game.Networking;
using Microsoft.Xna.Framework.Net;
using System.IO;
using System.Threading;
using Miner_Of_Duty.Game.Editor;

namespace Miner_Of_Duty.Game
{

    public partial class MultiplayerGame
    {
        /// <summary>
        /// Use for joining
        /// </summary>
        public MultiplayerGame(GameModes type, TeamManager tm, GraphicsDevice gd, out EventHandler<GamerLeftEventArgs> gamerLeft)
        {
            this.game1 = MinerOfDuty.Self;
            this.type = type;
            this.gd = gd;
            this.TeamManager = tm;
            gamerLeft = GamerLeft;

            goldBlocks = new List<Vector3>();
            SB = new SpriteBatch(gd);
            Me = MinerOfDuty.Session.LocalGamers[0];
            MinerOfDuty.CurrentPlayerProfile.LevelUpEvent += CurrentPlayerProfile_LevelUpEvent;
            TeamManager.GameOverEvent += TeamManager_GameOverEvent;
            inGameMenu = new InGameMenu(MenuBack, MinerOfDuty.Session, this is SwarmGame);
            inGameMenu.SetClassEvent += new InGameMenu.SetClass(inGameMenu_SetClassEvent);

            players = new Dictionary<byte, Player>();
            playerBodies = new Dictionary<byte, PlayerBody>();
            movementPacketStates = new Dictionary<byte, MovementPacketState[]>();
            rollingAvgs = new Dictionary<byte, RollingAverage>();
            changes = new List<Networking.Packet.BlockChange>();
            InfoScreen = new InfoScreen(this);
            deathCam = new DeathCamera(gd, Vector3.Zero, Vector3.UnitX);
            EffectsManager = new EffectsManager();
            WeaponDropManager = new WeaponDropManager(this);
            grenadeManager = new GrenadeManager(this);
            PersonalRandom = new System.Random();

            whoIsDone = new Dictionary<byte, bool>();

            //sends level packet (might do this before) just make sure that the player
            //body packet gets sent
            Packet.WritePlayerBodyLooksPacket(Me, MinerOfDuty.CurrentPlayerProfile);

            MinerOfDuty.Session.GamerJoined += new EventHandler<GamerJoinedEventArgs>(Session_GamerJoined);

            fooPackets = new Queue<FooPacket>();
            mr = new MapReceiver();
            justGotHere = true;

            //initialize players including self and camera
            //for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            //{
            //    players.Add(MinerOfDuty.Session.AllGamers[i].Id, new Player(this, new Vector3(0, 1.4f, 0), gd, MinerOfDuty.Session.AllGamers[i].Id));
            //    movementPacketStates.Add(MinerOfDuty.Session.AllGamers[i].Id, new MovementPacketState[]{ 
            //        new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0, 0),
            //        new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0, 0)});
            //    rollingAvgs[MinerOfDuty.Session.AllGamers[i].Id] = new Networking.RollingAverage();
            //}

            player = players[Me.Id];

            player.Camera = new FPSCamera(gd, player.position, Vector3.Forward);
            cam = player.Camera;

            MinerOfDuty.Session.GamerJoined += new EventHandler<GamerJoinedEventArgs>(Session_GamerJoined);
            MinerOfDuty.Session.HostChanged += HostChanged;

            TeamManager.AddGame(this);
        }

        private Queue<FooPacket> fooPackets;
        private MapReceiver mr;
        public bool justGotHere;
        public bool waitingForMatch = false;
        private double waitingForMatchTime = 0;
        private void JoinerUpdate(GameTime gameTime)
        {
            if (MinerOfDuty.Session == null || (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed))
            {
                return;
            }

            
            dotDelay += gameTime.ElapsedGameTime.Milliseconds;
            if (dotDelay > 1000)
            {
                dot++;
                dotDelay = 0;
            }
            if (dot == 4)
                dot = 0;

            if (waitingForMatch)
            {
                waitingForMatchTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (waitingForMatchTime > 20)
                {
                    Packet.WriteWaitOver(Me);
                    waitingForMatch = false;
                }
            }

            while (Me.IsDataAvailable)
            {
                NetworkGamer sender;
                Me.ReceiveData(Packet.PacketReader, out sender);
                byte packetid = Packet.GetPacketID();

                switch (packetid)
                {
                    case Packet.PACKETID_WAIT:
                        waitingForMatch = true;
                        waitingForMatchTime = 0;
                        break;
                    case Packet.PACKETID_MAPDATA:
                        //check for "world data" packets
                        if (mr.PacketReceive(sender))
                        {
                            Thread t = new Thread(poop => GenerateJoinerWorld(mr.GetMapData));
                            t.IsBackground = true;
                            t.Start();
                            lock (ThreadLocks)
                                threads.Add(t);
                        }
                        break;
                    case Packet.PACKETID_BLOCKCHANGES:
                        byte[] buffer = new byte[Packet.PacketReader.Length - 1];
                        Packet.PacketReader.Read(buffer, 0, buffer.Length);
                        fooPackets.Enqueue(new FooPacket(
                            Packet.PACKETID_BLOCKCHANGES,
                            buffer, sender.Id));
                        break;
                    case Packet.PACKETID_PITFALLBROKE:
                        buffer = new byte[Packet.PacketReader.Length - 1];
                        Packet.PacketReader.Read(buffer, 0, buffer.Length);
                        fooPackets.Enqueue(new FooPacket(
                            Packet.PACKETID_PITFALLBROKE,
                            buffer, sender.Id));
                        break;
                    case Packet.PACKETID_PLAYERBODYLOOKS:
                        if (playerBodies.ContainsKey(sender.Id))
                            playerBodies.Remove(sender.Id);
                        playerBodies.Add(sender.Id, new PlayerBody(MinerOfDuty.Self.GraphicsDevice, sender.Id, sender.Gamertag, Color.Green));
                        Packet.ReadPlayerBodyLooksPacket(playerBodies[sender.Id]);
                        break;
                    /* to store
                     * PACKETID_PITFALLBROKE
                     * PACKETID_BLOCKCHANGES
                     * 
                     */
                    case Packet.PACKETID_PLAYERLEVEL:
                        if (MinerOfDuty.Session.FindGamerById(sender.Id) != null)
                        {
                            Packet.ReadPlayerLevel(MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats);
                        }
                        break;
                    case Packet.PACKETID_TOADDTOTEAM:
                        byte newPersonID;
                        TeamManager.Team team;
                        Packet.ReadToAddToTeamManager(out newPersonID, out team);
                        TeamManager.AddPlayer(newPersonID, team);
                        if (playerBodies.ContainsKey(newPersonID))
                            playerBodies[newPersonID].color = TeamManager.IsOnMyTeam(newPersonID) ? Color.Green : Color.Red;
                        break;
                    case Packet.PACKETID_MUTE:
                        MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(sender, Packet.PacketReader.ReadBoolean());
                        break;
                    case Packet.PACKETID_IDIED:
                        byte killerID;
                        byte gunID;
                        KillText.DeathType deathType;
                        bool wasRevenge; 
                        Packet.ReadIDiedPacket("", out deathType, out killerID, out gunID, out wasRevenge);
                        TeamManager.KilledPlayer(killerID, sender.Id, deathType, wasRevenge);
                        players[sender.Id].dead = true;
                        break;
                    case Packet.PACKETID_RESPAWNPACKET:
                        Vector3 respawnPos;
                        Packet.ReadRespawnPacket(out respawnPos);
                        players[sender.Id].dead = false;
                        players[sender.Id].Respawn(ref respawnPos);
                        movementPacketStates[sender.Id][0].Position = respawnPos;
                        movementPacketStates[sender.Id][1].Position = respawnPos;
                        break;
                    case Packet.PACKETID_INGAMETIME:
                        byte state = Packet.PacketReader.ReadByte();
                        long time = Packet.PacketReader.ReadInt64();
                        if (!(Me.IsHost && Me.Id == sender.Id))
                            TeamManager.ReciveTimeToSend(state, time);
                        break;
                    case Packet.PACKETID_KINGOFHILLSCORED:
                        Packet.ReadKingOfHill(TeamManager);
                        break;
                    //process
                    /*
                     * PACKETID_PLAYERLEVEL
                     * PACKETID_TOADDTOTEAM - nevermind": a player might leave and this packet wont makes sense so check
                     * PACKETID_MUTE
                     * PACKETID_IDIED 
                     * PACKETID_RESPAWNPACKET -only use to change player from dead
                     * PACKETID_INGAMETIME
                     */

                    //make sure to process anything with randoms

                    //check and store any world changing packets
                    //store kills and deaths
                    //might want to process instread of store round/match overs
                }
            }
            MinerOfDuty.Session.Update();
        }

        private Dictionary<byte, MapSender> mapSenders = new Dictionary<byte, MapSender>();
        private readonly object mapSendersLock = new object();
        //not for use with actual joiner
        private bool HandleJoinersPackets(GameTime gameTime, byte packetID, NetworkGamer sender)
        {
            //send mapsender packets
            //maybe process playerbody packets here?
            switch (packetID)
            {
                case Packet.PACKETID_MAPDATAREQUEST:
                    mapSenders[sender.Id].PacketRequest(sender);
                    return true;
                case Packet.PACKETID_PLAYERBODYLOOKS:
                    if (playerBodies.ContainsKey(sender.Id))
                        playerBodies.Remove(sender.Id);
                    playerBodies.Add(sender.Id, new PlayerBody(MinerOfDuty.Self.GraphicsDevice, sender.Id, sender.Gamertag, TeamManager.IsOnMyTeam(sender.Id) ? Color.Green : Color.Red));
                    Packet.ReadPlayerBodyLooksPacket(playerBodies[sender.Id]);
                    return true;
                case Packet.PACKETID_WAITOVER:
                    if (TeamManager.AcceptNewPlayer())
                    {
                        //thread
                        Thread thread = new Thread(delegate()
                        {
#if XBOX
                        Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                            MapSender mapSender = new MapSender(SendWorld());
                            mapSender.RecipentID = sender.Id;
                            lock (mapSendersLock)
                                mapSenders.Add(sender.Id, mapSender);
                        });
                        thread.IsBackground = true;
                        thread.Start();

                        lock (ThreadLocks)
                            threads.Add(thread);
                    }
                    else
                    {
                        Packet.WriteWait(Me, sender);
                    }
                    return true;
            }
            return false;
        }

        private void GenerateJoinerWorld(MemoryStream ms)
        {
            try
            {
                //create the world
                MemoryStream ms2 = new MemoryStream();
                Lzf.LZF.lzf.Decompress(ms, ms2);
                ms.Close();
                ms.Dispose();
                using (BinaryReader br = new BinaryReader(ms2))
                {
                    int terrainSize = br.ReadInt32();
                    int seed = br.ReadInt32();
                    int timesUsed = br.ReadInt32();
                    WorldEditor.WorldGeneration worldGen = (WorldEditor.WorldGeneration)br.ReadByte();

                    goldBlocks = br.ReadVector3List();

                    Random = new CountedRandom(seed);

                    Terrain = new Game.Terrain(this);
                    Liquid.game = null;
                    LightingManager = new LightingManager(Terrain, gd);
                    LiquidManager = new LiquidManager(this, gd);
                    Terrain.CreateBlocksFromSave(gd, worldGen, br, terrainSize, true);//multiplayer games only use tree worlds and no custom join in

                    Random = new CountedRandom(seed, timesUsed);

                    LiquidManager.Start();
                    LightingManager.LightWorld();

                    frCam = new FreeRoamCamera(gd, new Vector3(106, 64, 57), Vector3.Forward);

                    TeamManager.ReadInfo(br);

                    foreach (byte key in playerBodies.Keys)
                    {
                        if (TeamManager.PlayingGamers.Contains(key))
                            if (TeamManager.IsOnMyTeam(key))
                                playerBodies[key].color = Color.Green;
                            else
                                playerBodies[key].color = Color.Red;
                    }

                    TeamManager.MuteTeam();
                    TeamManager.StartGame(this);

                    while (fooPackets.Count > 0)
                    {
                        FooPacket p = fooPackets.Dequeue();
                        byte senderID = p.SenderID;
                        PacketReader PacketReader = new PacketReader();
                        PacketReader.BaseStream.Write(p.Data, 0, p.Data.Length);
                        PacketReader.BaseStream.Position = 0;
                        switch (p.PacketID)
                        {
                            case Packet.PACKETID_BLOCKCHANGES:
                                #region code
                                Packet.BlockChange[] changes2 = Packet.ReadBlockPacket(PacketReader);
                                for (int i = 0; i < changes2.Length; i++)
                                {
                                    if (changes2[i].Added)
                                    {
                                        if (!Block.IsLiquid(changes2[i].ID))
                                        {
                                            //check if can place

                                            Terrain.blockBox.Max = changes2[i].Position + Block.halfVector;
                                            Terrain.blockBox.Min = changes2[i].Position - Block.halfVector;

                                            bool hit = playerBodies[Me.Id].CheckForIntersection(ref Terrain.blockBox);

                                            if (hit)
                                                BlockChanged(ref changes2[i].Position, Block.BLOCKID_AIR, false);
                                            else
                                            {
                                                Terrain.AddBlocks((int)changes2[i].Position.X, (int)changes2[i].Position.Y, (int)changes2[i].Position.Z, changes2[i].ID);
                                                if (changes2[i].ID == Block.BLOCKID_GOLD)
                                                {
                                                    if (type == GameModes.CustomSM || type == GameModes.CustomSNM)
                                                        goldBlocks.Add(changes2[i].Position);
                                                }
                                            }

                                            Terrain.blockBox.Max = Block.halfVector;
                                            Terrain.blockBox.Min = -Block.halfVector;

                                        }
                                        else if (changes2[i].ID == Block.BLOCKID_WATER)
                                        {
                                            LiquidManager.AddSourceWaterBlock((int)changes2[i].Position.X, (int)changes2[i].Position.Y,
                                                (int)changes2[i].Position.Z);
                                        }
                                        else
                                        {
                                            LiquidManager.AddSourceLavaBlock((int)changes2[i].Position.X, (int)changes2[i].Position.Y,
                                               (int)changes2[i].Position.Z);
                                        }
                                    }
                                    else
                                    {
                                        if (Terrain.blocks[(int)changes2[i].Position.X, (int)changes2[i].Position.Y, (int)changes2[i].Position.Z] == Block.BLOCKID_GOLD)
                                        {
                                            for (int j = 0; j < goldBlocks.Count; j++)
                                            {
                                                if (goldBlocks[j] == changes2[i].Position)
                                                {
                                                    goldBlocks.RemoveAt(j);
                                                    break;
                                                }
                                            }
                                            if (TeamManager is SearchNMineManager)
                                                (TeamManager as SearchNMineManager).PlayerMinedGoldBlock(senderID);
                                        }
                                        Terrain.RemoveBlocks((int)changes2[i].Position.X, (int)changes2[i].Position.Y, (int)changes2[i].Position.Z);

                                        //if (type == GameModes.CustomSM || type == GameModes.CustomSNM)
                                        //    if (goldBlocks.Contains(changes2[i].Position))
                                        //        goldBlocks.Remove(changes2[i].Position);

                                    }
                                }
                                #endregion
                                break;
                            case Packet.PACKETID_PITFALLBROKE:
                                byte xP = PacketReader.ReadByte();
                                byte yP = PacketReader.ReadByte();
                                byte zP = PacketReader.ReadByte();
                                for (int i = 0; i < Terrain.pitfallBlocks.Count; i++)
                                {
                                    if (Terrain.pitfallBlocks[i].X == xP && Terrain.pitfallBlocks[i].Y == yP && Terrain.pitfallBlocks[i].Z == zP)
                                    {
                                        Terrain.pitfallBlocks.RemoveAt(i);
                                        break;
                                    }
                                }
                                break;
                        }
                    }

                    System.GC.Collect();

                    dot = 0;
                    MinerOfDuty.Session.HostChanged -= HostChanged;

                    everyOnesDone = true;
                    isGenerated = true;
                    justGotHere = false;

                    choseClass = new ChooseClass();
                    choseClass.SetClassEvent += new ChooseClass.SetClass(choseClass_SetClassEvent);
                    Vector3 spwn = TeamManager.GetReSpawnPoint(PersonalRandom, this);
                    Packet.WriteRespawnPacket(Me, ref spwn);
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                isGenerated = false;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private void HostChanged(object sender, HostChangedEventArgs e)
        {
            MinerOfDuty.DrawMenu();
            MessageBox.ShowMessageBox(delegate(int selected) { Audio.PlaySound(Audio.SOUND_UICLICK); },
                new string[] { "OKAY" }, 0, new string[] { "THE HOST LEFT THE SESSION" });
        }

        private void Session_GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            if (players.ContainsKey(e.Gamer.Id) == false)
            {
                if (InfoScreen != null && MinerOfDuty.Session.SessionType == NetworkSessionType.PlayerMatch)
                    InfoScreen.AddKillText(new InfoText(e.Gamer.Gamertag + " Joined"));

                //add the joined player to the players list
                //movementpacketstates, rolling averages, player body

                e.Gamer.Tag = new GamePlayerStats();

                players.Add(e.Gamer.Id, new Player(this, new Vector3(1000, 1000, 1000), gd, e.Gamer.Id));

                movementPacketStates.Add(e.Gamer.Id, new MovementPacketState[]{ 
                            new MovementPacketState(players[ e.Gamer.Id].position, Vector2.Zero, 0, 0, 0),
                            new MovementPacketState(players[ e.Gamer.Id].position, Vector2.Zero, 0, 0, 0)});
                rollingAvgs[e.Gamer.Id] = new Networking.RollingAverage();

                playerBodies.Add(e.Gamer.Id, new PlayerBody(gd, e.Gamer.Id, e.Gamer.Gamertag, Color.Gray));
                playerBodies[e.Gamer.Id].CreateParts(Color.Black, Color.Black, Color.Wheat, Color.Wheat, Color.Wheat);
                // }

                if (e.Gamer.Id != Me.Id)//send this to the new player
                {
                    Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);
                    Packet.WritePlayerBodyLooksPacket(Me, MinerOfDuty.CurrentPlayerProfile, e.Gamer);
                }

                if (Me.IsHost)
                {
                    //send world packet
                    //add them to team manager

                    if (TeamManager.AcceptNewPlayer())
                    {
                        //thread
                        Thread thread = new Thread(delegate()
                        {
#if XBOX
                        Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                            MapSender mapSender = new MapSender(SendWorld());
                            mapSender.RecipentID = e.Gamer.Id;
                            lock (mapSendersLock)
                                mapSenders.Add(e.Gamer.Id, mapSender);
                        });
                        thread.IsBackground = true;
                        thread.Start();

                        lock (ThreadLocks)
                            threads.Add(thread);
                    }
                    else
                    {
                        Packet.WriteWait(Me, e.Gamer);
                    }
                    TeamManager.Team t = TeamManager.CalculateWhichTeamForPlayer(e.Gamer.Id);
                    Packet.WriteToAddToTeamManager(MinerOfDuty.Session.LocalGamers[0], e.Gamer.Id, t);
                }
            }
        }

        private readonly object ThreadLocks = new object();
        private List<Thread> threads = new List<Thread>();

        private MemoryStream SendWorld()
        {
            if (Terrain == null)
                return new MemoryStream(BitConverter.GetBytes((short)6969));

            MemoryStream ms = new MemoryStream();
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(Terrain.Size);
                bw.Write(Random.Seed);
                bw.Write(Random.TimesUsed);
                bw.Write((byte)Terrain.WorldGen);

                bw.Write(this.goldBlocks);

                for (int x = 0; x < 128; x++)
                {
                    for (int z = 0; z < 128; z++)
                    {
                        int y = 63;
                        byte lastBlock;
                        while (true)
                        {
                            int howMany = 0;
                            lastBlock =  (Block.IsLiquid(Terrain.blocks[x, y, z]) && LiquidManager.Liquids[x, y, z].liquidLevel != 10) ? Block.BLOCKID_AIR : Terrain.blocks[x, y, z]; //we dont want liquids
                            while (lastBlock == ((Block.IsLiquid(Terrain.blocks[x, y, z]) && LiquidManager.Liquids[x, y, z].liquidLevel != 10) ? Block.BLOCKID_AIR : Terrain.blocks[x, y, z]))
                            {
                                howMany++;
                                y--;
                                if (y == -1)
                                    break;
                                if (lastBlock != 
                                    ((Block.IsLiquid(Terrain.blocks[x, y, z]) && LiquidManager.Liquids[x, y, z].liquidLevel != 10) 
                                    ? Block.BLOCKID_AIR : Terrain.blocks[x, y, z]))
                                    break;
                            }

                            bw.Write(lastBlock);
                            bw.Write((byte)howMany);

                            if (y == -1)
                                break;
                        }
                    }
                }

                //bw.Write(liquids.Count);
                //for (int i = 0; i < liquids.Count; i++)
                //{
                //    bw.Write(liquids[i].W == 0);
                //    bw.Write((short)liquids[i].X);
                //    bw.Write((short)liquids[i].Y);
                //    bw.Write((short)liquids[i].Z);
                //}

                TeamManager.SaveInfo(bw);

                MemoryStream ms2 = new MemoryStream();
                Lzf.LZF.lzf.Compress(ms, ms2);
                ms = ms2;
            }


            return ms;
        }
    }


}