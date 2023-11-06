using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.Game.Networking;
using Miner_Of_Duty.LobbyCode;
using System.Threading;
using Miner_Of_Duty.Menus;
using System.IO;
using Miner_Of_Duty.Game.Editor;
using Lzf;

namespace Miner_Of_Duty.Game
{

    public partial class MultiplayerGame : IGameScreen, ITerrainOwner
    {
        public bool DrawHud = true;
        public virtual bool IsCustom { get { return false; } }
        public CountedRandom Random;
        public Random PersonalRandom;
        public Terrain Terrain;
        public LiquidManager LiquidManager;
        public LightingManager LightingManager;
        public Camera cam;
        public TeamManager TeamManager;
        public Dictionary<byte, Player> players;
        public Dictionary<byte, PlayerBody> playerBodies;
        public Player player;
        protected int numberOfPlayers;
        protected FreeRoamCamera frCam;
        public LocalNetworkGamer Me;
        protected GraphicsDevice gd;
        public bool isGenerated = false;
        protected bool everyOnesDone = false;
        public InfoScreen InfoScreen;
        private DeathCamera deathCam;
        public EffectsManager EffectsManager;
        public WeaponDropManager WeaponDropManager;
        protected InGameMenu inGameMenu;
        protected bool isShowingInGameMenu;
        private bool paused = false;
        public GrenadeManager grenadeManager;

        public bool EditingEnabled = true;
        public bool WeaponsEnabled = true;

        public int timeTillRespawn;
        public const int respawntime = 4000;
        public Dictionary<byte, bool> whoIsDone;
        protected Dictionary<byte, MovementPacketState[]> movementPacketStates;
        protected Dictionary<byte, RollingAverage> rollingAvgs;

        protected ChooseClass choseClass;
        protected TimeSpan CountDown;

        public bool GameOver = false;
        protected bool hasStarted = false;

        private bool hasAsked = false;
        private MinerOfDuty game1;
        protected SpriteBatch SB;
        public GameModes type { get; private set; }
        public MultiplayerGame(MinerOfDuty game1, GameModes type, TeamManager tm, GraphicsDevice gd, out EventHandler<GamerLeftEventArgs> gamerLeft, bool sendSeed = true)
            : base()
        {
            this.game1 = game1;
            goldBlocks = new List<Vector3>();
            this.type = type;
            SB = new SpriteBatch(gd);
            TeamManager = tm;
            this.gd = gd;
            Me = MinerOfDuty.Session.LocalGamers[0];

            if (sendSeed)
                if (MinerOfDuty.Session.Host == Me)
                {
                    Packet.WriteSeedPacket(MinerOfDuty.Session.LocalGamers[0], new Random().Next());
                }

            MinerOfDuty.CurrentPlayerProfile.LevelUpEvent += CurrentPlayerProfile_LevelUpEvent;

            whoIsDone = new Dictionary<byte, bool>();

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                whoIsDone.Add(MinerOfDuty.Session.AllGamers[i].Id, false);
                (MinerOfDuty.Session.AllGamers[i].Tag as GamePlayerStats).ClearStats();
            }

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

            gamerLeft = GamerLeft;


            TeamManager.GameOverEvent += TeamManager_GameOverEvent;

            inGameMenu = new InGameMenu(MenuBack, MinerOfDuty.Session, this is SwarmGame);
            inGameMenu.SetClassEvent += new InGameMenu.SetClass(inGameMenu_SetClassEvent);

            
        }

        void CurrentPlayerProfile_LevelUpEvent()
        {
            Packet.WritePlayerLevel(Me, (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);
        }

        protected CharacterClass toSetToAfterDeath;
        protected void inGameMenu_SetClassEvent(CharacterClass classToSet)
        {
            toSetToAfterDeath = classToSet;
        }

        private void MenuBack(object sender)
        {
            isShowingInGameMenu = false;
            paused = false;
        }

        protected string gameOverText;
        protected Color gameOverColor;
        protected int gameOverSeconds;
        private void TeamManager_GameOverEvent(string text, Color color)
        {
            UnSub();
            try
            {
                MinerOfDuty.Session.AllowJoinInProgress = false;
            }
            catch { }

            gameOverText = text;
            gameOverColor = color;
            gameOverSeconds = 10000;
            isShowingInGameMenu = false;
            GameOver = true;

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                if (MinerOfDuty.Session.AllGamers[i].IsLocal == false)
                {
                    if ((MinerOfDuty.Session.AllGamers[i].Tag as GamePlayerStats).IsMuted == false)
                    {
                        Packet.WriteMutePacket(MinerOfDuty.Session.LocalGamers[0], MinerOfDuty.Session.AllGamers[i], false);
                        MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(MinerOfDuty.Session.AllGamers[i], true);
                    }
                    else
                    {
                        Packet.WriteMutePacket(MinerOfDuty.Session.LocalGamers[0], MinerOfDuty.Session.AllGamers[i], true);
                        MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(MinerOfDuty.Session.AllGamers[i], false);
                    }
                }
            }

            Audio.SetFading(Audio.Fading.Out);
        }

        private void GamerLeft(object sender, GamerLeftEventArgs e)
        {
            if(whoIsDone.ContainsKey(e.Gamer.Id))
                whoIsDone.Remove(e.Gamer.Id);
            if (InfoScreen != null)
                InfoScreen.AddKillText(new InfoText(e.Gamer.Gamertag + " Left"));
        }

        private TimeSpan WaitingForTimeSpan = new TimeSpan();
        protected virtual void GenerateWorld()
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif


                Terrain = new Terrain(this);
                LightingManager = new LightingManager(Terrain, gd);
                Liquid.game = null;
                LiquidManager = new LiquidManager(this, gd);
                if (type == GameModes.KingOfTheBeach)
                    Terrain.Initialize(gd, Terrain.CreateBlocksIsland, 128, true);
                else
                    Terrain.Initialize(gd, Terrain.CreateBlocksRandom, 128, true);

                LiquidManager.Start();
                LightingManager.LightWorld();

                frCam = new FreeRoamCamera(gd, new Vector3(106, 64, 57), Vector3.Forward);

                SetUpPlayers(gd);

                Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);

               
                System.GC.Collect();

                isGenerated = true;
                dot = 0;
            }
            catch (Exception e)
            {
                isGenerated = false;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private bool sayCantPlaceNextToGold;
        private int sayCantPlaceNextToGoldTime;

        protected List<Packet.BlockChange> changes;
        public bool BlockChanged(ref Vector3 pos, byte blockID, bool added)
        {
            if (Terrain.blocks[(int)pos.X, (int)pos.Y, (int)pos.Z] != Block.BLOCKID_GOLD)
                for (int i = 0; i < goldBlocks.Count; i++)
                    if (Vector3.Distance(pos, goldBlocks[i]) < 5)
                    {
                        sayCantPlaceNextToGold = true;
                        sayCantPlaceNextToGoldTime = 0;
                        return false;
                    }
            sayCantPlaceNextToGold = false;
            changes.Add(new Networking.Packet.BlockChange(pos, blockID, added));
            return true;
        }

        public void SetDeathCamOnMe()
        {
            deathCam.SetLook(player.position, player.position - new Vector3(.25f, 1, .25f));
            cam = deathCam;
        }

        private string killedBy;
        public bool isPaused = false;
        private int frameCounter = 0;
        private int cantPlaceBlockTime;
        public virtual void Update(GameTime gameTime)
        {
            if (justGotHere)
            {
                JoinerUpdate(gameTime);
                return;
            }


            if (CantPlaceBlockWarning == false)
                cantPlaceBlockTime = 0;
            else
            {
                cantPlaceBlockTime += gameTime.ElapsedGameTime.Milliseconds;
                if (cantPlaceBlockTime >= 500)
                    CantPlaceBlockWarning = false;
            }


            if (GameOver)
            {
                gameOverSeconds -= gameTime.ElapsedGameTime.Milliseconds;

                if (gameOverSeconds <= 0)
                {
                    if (Me.IsHost)
                        MinerOfDuty.Session.EndGame();////////////////////////////////////////////////////
                    LiquidManager.KillWaterManagerThread();
                    if (this is SwarmGame && MinerOfDuty.Session.SessionType == NetworkSessionType.Local)
                    {
                        MinerOfDuty.DrawLobby();
                        MinerOfDuty.DrawMenu();
                        if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                        {
                            MinerOfDuty.Session.Dispose();
                            return;
                        }
                    }
                }
            }

            if (MinerOfDuty.Session == null || (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed))
            {
                return;
            }



            if (isGenerated && everyOnesDone)
            {

                if (Me.IsHost)
                {
                    lock (mapSendersLock)
                        if (mapSenders.Count > 0)
                            foreach(byte i in mapSenders.Keys)
                                if (mapSenders[i].HasStarted == false)
                                {
                                    try
                                    {
                                        mapSenders[i].StartWritePacketTo(MinerOfDuty.Session.FindGamerById(mapSenders[i].RecipentID));
                                    }
                                    catch (ArgumentNullException)
                                    {
                                        mapSenders[i].Dispose();
                                        mapSenders.Remove(i);
                                    }
                                }
                }

                if (!hasStarted)
                {
                    if (CountDown.TotalSeconds > 0)
                    {
                        CountDown = CountDown.Subtract(gameTime.ElapsedGameTime);
                    }
                    else
                    {
                        if (!IsCustom)
                        {
                            try
                            {
                                MinerOfDuty.Session.AllowJoinInProgress = true;
                            }
                            catch { }
                            MinerOfDuty.Session.GamerJoined += new EventHandler<GamerJoinedEventArgs>(Session_GamerJoined);
                        }
                        hasStarted = true;
                        TeamManager.StartGame(this);
                    }
                }

                if (sayCantPlaceNextToGold)
                {
                    sayCantPlaceNextToGoldTime += gameTime.ElapsedGameTime.Milliseconds;
                    if (sayCantPlaceNextToGoldTime > 1000)
                    {
                        sayCantPlaceNextToGold = false;
                        sayCantPlaceNextToGoldTime = 0;
                    }
                }


                if (choseClass != null)
                {
                    choseClass.Update((short)gameTime.ElapsedGameTime.Milliseconds);
                    player.DontUseInput();
                }

                LightingManager.SwitchTextures();

                Terrain.Update();

                // if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.RightShoulder))
                //    if (cam == player.Camera) { frCam.Position = cam.Position + 2 * Vector3.UnitY; cam = frCam; } else cam = player.Camera;

                if (cam == frCam)
                {
                    frCam.Update(gameTime, Input.ControllingPlayerNewGamePadState.ThumbSticks.Right.X, Input.ControllingPlayerNewGamePadState.ThumbSticks.Right.Y, new Vector3(Input.ControllingPlayerNewGamePadState.ThumbSticks.Left.X, 0, -Input.ControllingPlayerNewGamePadState.ThumbSticks.Left.Y));
                }

                LightingManager.Update();




                while (Me.IsDataAvailable)
                {
                    NetworkGamer sender;
                    Me.ReceiveData(Packet.PacketReader, out sender);
                    byte packetid = Packet.GetPacketID();
                    switch (packetid)
                    {
                        case Packet.PACKETID_KINGOFHILLSCORED:
                            Packet.ReadKingOfHill(TeamManager);
                            break;
                        case Packet.PACKETID_PITFALLBROKE:
                            byte xP = Packet.PacketReader.ReadByte();
                            byte yP = Packet.PacketReader.ReadByte();
                            byte zP = Packet.PacketReader.ReadByte();
                            for (int i = 0; i < Terrain.pitfallBlocks.Count; i++)
                            {
                                if (Terrain.pitfallBlocks[i].X == xP && Terrain.pitfallBlocks[i].Y == yP && Terrain.pitfallBlocks[i].Z == zP)
                                {
                                    Terrain.pitfallBlocks.RemoveAt(i);
                                    break;
                                }
                            }
                            break;
                        case Packet.PACKETID_GETMEATEAMMR:
                            TeamManager.Team t = TeamManager.CalculateWhichTeamForPlayer(sender.Id);
                            Packet.WriteToAddToTeamManager(MinerOfDuty.Session.LocalGamers[0], sender.Id, t);
                            break;
                        case Packet.PACKETID_TOADDTOTEAM:
                            byte newPersonID;
                            TeamManager.Team team;
                            Packet.ReadToAddToTeamManager(out newPersonID, out team);
                            TeamManager.AddPlayer(newPersonID, team);
                            if(playerBodies.ContainsKey(newPersonID))
                                playerBodies[newPersonID].color = TeamManager.IsOnMyTeam(newPersonID) ? Color.Green : Color.Red;
                            break;
                        case Packet.PACKETID_WEAPONSPAWNERTAKEN:
                            short spawnerID = Packet.PacketReader.ReadInt16();
                            WeaponDropManager.TakeSpawner(spawnerID);
                            break;
                        case Packet.PACKETID_PLAYERLEVEL:
                            if (MinerOfDuty.Session.FindGamerById(sender.Id) != null)
                            {
                                Packet.ReadPlayerLevel(MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats);
                            }
                            break;
                        case Packet.PACKETID_MUTE:
                            MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(sender, Packet.PacketReader.ReadBoolean());
                            break;
                        case Packet.PACKETID_PLAYERMOVEMENT:
                            if (sender.Id != Me.Id)
                            {
                                // gamePadStates[sender.Id] = Packet.ReadMovementPacket(players[sender.Id]);
                                InventoryItem holdingItem;
                                bool leftTrigger;
                                Player.Stance stance;
                                bool knifing;
                                bool holdingGrenade;
                                byte grenadeId;

                                uint packetNumber;

                                MovementPacketState mps = Packet.ReadMovementPacket(gameTime, rollingAvgs[sender.Id], sender, out holdingItem, out leftTrigger, out stance, out knifing,
                                    out holdingGrenade, out grenadeId, out packetNumber);

                                if (mps.PacketNumber >= movementPacketStates[sender.Id][1].PacketNumber)
                                {
                                    movementPacketStates[sender.Id][0] = movementPacketStates[sender.Id][1];
                                    movementPacketStates[sender.Id][1] = mps;
                                    players[sender.Id].inventory.SetSelectedTo(holdingItem);
                                    players[sender.Id].leftTriggerDown = leftTrigger;
                                    players[sender.Id].stance = stance;
                                    playerBodies[sender.Id].stance = stance;
                                    playerBodies[sender.Id].knifing = knifing;
                                    playerBodies[sender.Id].HoldingGrenade = holdingGrenade;
                                    playerBodies[sender.Id].GrenadeID = grenadeId;
                                }
                            }
                            else
                                Packet.ReadMovementPacketEmpty();
                            break;
                        case Packet.PACKETID_BLOCKCHANGES:
                            Packet.BlockChange[] changes = Packet.ReadBlockPacket();
                            for (int i = 0; i < changes.Length; i++)
                            {
                                if (changes[i].Added)
                                {
                                    if (!Block.IsLiquid(changes[i].ID))
                                    {
                                        //check if can place

                                        Terrain.blockBox.Max = changes[i].Position + Block.halfVector;
                                        Terrain.blockBox.Min = changes[i].Position - Block.halfVector;

                                        bool hit = playerBodies[Me.Id].CheckForIntersection(ref Terrain.blockBox);

                                        //for (int j = 0; j < MinerOfDuty.Session.AllGamers.Count; j++)
                                        //{
                                        //    if (playerBodies[MinerOfDuty.Session.AllGamers[i].Id].CheckForIntersection(ref Terrain.blockBox))
                                        //    {
                                        //        hit = true;
                                        //        break;
                                        //    }
                                        //}
                                        if (hit)
                                            BlockChanged(ref changes[i].Position, Block.BLOCKID_AIR, false);
                                        else
                                        {
                                            Terrain.AddBlocks((int)changes[i].Position.X, (int)changes[i].Position.Y, (int)changes[i].Position.Z, changes[i].ID);
                                        }

                                        Terrain.blockBox.Max = Block.halfVector;
                                        Terrain.blockBox.Min = -Block.halfVector;

                                    }
                                    else if (changes[i].ID != Block.BLOCKID_WATER)
                                    {
                                        LiquidManager.AddSourceWaterBlock((int)changes[i].Position.X, (int)changes[i].Position.Y,
                                            (int)changes[i].Position.Z);
                                    }
                                    else
                                    {
                                        LiquidManager.AddSourceLavaBlock((int)changes[i].Position.X, (int)changes[i].Position.Y,
                                           (int)changes[i].Position.Z);
                                    }
                                }
                                else
                                {
                                    if (Terrain.blocks[(int)changes[i].Position.X, (int)changes[i].Position.Y, (int)changes[i].Position.Z] == Block.BLOCKID_GOLD)
                                    {
                                        for (int j = 0; j < goldBlocks.Count; j++)
                                        {
                                            if (goldBlocks[j] == changes[i].Position)
                                            {
                                                goldBlocks.RemoveAt(j);
                                                break;
                                            }
                                        }
                                        if (TeamManager is SearchNMineManager)
                                            (TeamManager as SearchNMineManager).PlayerMinedGoldBlock(sender.Id);
                                    }
                                    Terrain.RemoveBlocks((int)changes[i].Position.X, (int)changes[i].Position.Y, (int)changes[i].Position.Z);
                                }
                            }
                            break;
                        case Packet.PACKETID_KNIFE:
                            if (player.dead == false)
                            {
                                player.DropWeapon();
                                Packet.WriteIDiedPacket(Me, sender.Gamertag, sender.Id, 69, KillText.DeathType.Knife);
                                timeTillRespawn = 4000;
                                Vector3 normed = players[sender.Id].position;
                                normed.Normalize();
                                deathCam.SetLook(-normed + player.position, player.position - new Vector3(0, 1, 0));
                                cam = deathCam;
                                InfoScreen.ClearShotFrom();
                                player.dead = true;
                            }
                            break;
                        case Packet.PACKETID_BULLETFIRED:
                            Ray bulletRay;
                            byte gunFired;
                            float dis;
                            Packet.ReadBulletFired(out bulletRay, out gunFired, out dis);
                            if (sender.IsLocal == false)
                            {
                                EffectsManager.AddFlare(playerBodies[sender.Id], gunFired);
                                EffectsManager.AddBulletStreak(ref bulletRay, dis);
                                Audio.PlaySound(Audio.SOUND_FIRE, MathHelper.Clamp(1 - ((Vector3.Distance(player.position, bulletRay.Position)) / 50f), 0, .9f));
                            }
                            InfoScreen.miniMap.EllaFired(sender.Id);
                            break;
                        case Packet.PACKETID_PLAYERSHOTAT:
                            byte id;
                            float dmg;
                            KillText.DeathType type;
                            byte gunID;
                            Vector3 pos = Packet.ReadPlayerShotPacket(out dmg, out id, out type, out gunID);
                            if (Me.Id == id)
                            {
                                InfoScreen.AddShotFrom(ref pos);
                            }
                            if (players[id].useInvincibleityYeahBitchSpeellingISForLosers)
                            {

                            }
                            else if (players[id].thickSkin)
                                players[id].health -= dmg * .85f;
                            else
                                players[id].health -= dmg;

                            if (players[id].health <= 0)
                            {
                                if (id == Me.Id)
                                {
                                    if (player.dead == false)
                                    {
                                        player.DropWeapon();
                                        Packet.WriteIDiedPacket(Me, sender.Gamertag, sender.Id, gunID, type);
                                        timeTillRespawn = 4000 + TeamManager.GetSpawnDelay();
                                        Vector3 normed = pos;
                                        normed.Normalize();
                                        deathCam.SetLook(-normed + player.position, player.position - new Vector3(0, 1, 0));
                                        cam = deathCam;
                                        InfoScreen.ClearShotFrom();
                                        player.dead = true;
                                    }
                                }
                                // players[id].dead = true;
                            }
                            break;
                        case Packet.PACKETID_IDIED:
                            byte killerID;
                            KillText.DeathType deathType;
                            bool wasRevenge; 
                            Packet.ReadIDiedPacket("", out deathType, out killerID, out gunID, out wasRevenge);

                            string tmpKilledBy = "";

                            if (deathType == KillText.DeathType.Lava)
                            {
                                tmpKilledBy = "LAVA";
                            }
                            else if (deathType == KillText.DeathType.Fall)
                            {
                                tmpKilledBy = "Gravity".ToUpper();
                            }
                            else if (deathType == KillText.DeathType.Zombie)
                            {
                                tmpKilledBy = "Zombie".ToUpper();
                            }
                            else if (deathType == KillText.DeathType.Water)
                            {
                                tmpKilledBy = "WATER";
                            }
                            else 
                                tmpKilledBy = MinerOfDuty.Session.FindGamerById(killerID).Gamertag;

                            if (killerID == Me.Id && gunID != 69)
                                MinerOfDuty.CurrentPlayerProfile.AddGunKill(this.type, gunID, Lobby.IsPrivateLobby());

                            if (Me.Id == sender.Id)
                                killedBy = tmpKilledBy;

                            if (deathType == KillText.DeathType.Zombie)
                            {
                                //nada
                            }
                            else if (deathType != KillText.DeathType.Lava || deathType != KillText.DeathType.Fall)
                                InfoScreen.AddKillText(new KillText(tmpKilledBy, TeamManager.IsOnMyTeam(killerID) ? Color.LightSalmon : Color.Red,
                                    sender.Gamertag, TeamManager.IsOnMyTeam(sender.Id) ? Color.LightSalmon : Color.Red,
                                    deathType));
                            else
                                InfoScreen.AddKillText(new KillText(tmpKilledBy, Color.Red,
                                    sender.Gamertag, TeamManager.IsOnMyTeam(sender.Id) ? Color.LightSalmon : Color.Red,
                                    deathType));

                            TeamManager.KilledPlayer(killerID, sender.Id, deathType, wasRevenge);
                            players[sender.Id].dead = true;

                            if (player.dead)
                            {
                                Packet.PacketWriter.Write(Packet.PACKETID_SWARMIEIDIED);
                                Me.SendData(Packet.PacketWriter, SendDataOptions.Reliable);
                            }

                            break;
                        case Packet.PACKETID_RESPAWNPACKET:
                            Vector3 respawnPos;
                            Packet.ReadRespawnPacket(out respawnPos);
                            players[sender.Id].dead = false;
                            players[sender.Id].Respawn(ref respawnPos);
                            movementPacketStates[sender.Id][0].Position = respawnPos;
                            movementPacketStates[sender.Id][1].Position = respawnPos;
                            if (sender == Me)
                                cam = player.Camera;
                            break;
                        case Packet.PACKETID_WEAPONDROP:
                            Vector3 position;
                            byte weaponID;
                            short ammoLeft;
                            bool extnededmags;
                            WeaponDropManager.WeaponID idd;
                            Packet.ReadWeaponDropPacket(out position, out weaponID, out ammoLeft, out extnededmags, out idd);
                            WeaponDropManager.AddWeaponDrop(ref position, weaponID, ammoLeft, extnededmags, idd);
                            break;
                        case Packet.PACKETID_WEAPONDROPAMMOCHANGE:
                            Packet.ReadWeaponDropAmmoChange(out idd, out ammoLeft);
                            WeaponDropManager.AmmoChange(idd, ammoLeft);
                            break;
                        case Packet.PACKETID_WEAPONDROPSWITCH:
                            Packet.ReadWeapinDropSwitch(out idd, out weaponID, out ammoLeft, out extnededmags);
                            WeaponDropManager.SwitchOut(idd, weaponID, ammoLeft, extnededmags);
                            if (sender.Id == Me.Id)
                            {
                                Audio.PlaySound(Audio.SOUND_EQUIP);
                            }
                            break;
                        case Packet.PACKETID_GRENADETHROWN:
                            Vector3 grenadePosition;
                            byte grenadeID;
                            float leftRot, upDownRot;
                            int life;
                            Packet.ReadGrenadeThrown(out grenadePosition, out grenadeID, out leftRot, out upDownRot, out life);
                            if (sender.IsLocal == false)
                            {
                                grenadeManager.AddGrenade(grenadePosition, grenadeID, leftRot, upDownRot, life, sender);
                                playerBodies[sender.Id].ThrowGrenade(grenadeID);
                            }
                            break;
                        case Packet.PACKETID_INGAMETIME:
                            byte state = Packet.PacketReader.ReadByte();
                            long time = Packet.PacketReader.ReadInt64();
                            if (!(Me.IsHost && Me.Id == sender.Id))
                                TeamManager.ReciveTimeToSend(state, time);
                            break;
                        default:
                            if(HandleJoinersPackets(gameTime, packetid, sender) == false)
                                HandleUnknownPacketID(sender, packetid, gameTime);
                            break;
                    }
                }

                if (isShowingInGameMenu)
                {
                    inGameMenu.Update((short)gameTime.ElapsedGameTime.Milliseconds);
                    player.ClearGamePad();
                    player.DontUseInput();
                }
                else
                {
                    if (GameOver == false)
                        if (Input.WasButtonPressed(Buttons.Start) && choseClass == null)
                        {
                            inGameMenu.SelectFirst();
                            isShowingInGameMenu = true;
                            player.ClearGamePad();
                            player.Paused();
                            if (this is SwarmGame && MinerOfDuty.Session.AllGamers.Count == 1)
                                paused = true;
                        }
                }

                if ((this is SwarmGame && paused == false) || !(this is SwarmGame))
                {
                    Update2(gameTime);
                    InfoScreen.Update(gameTime, player);
                    TeamManager.Update(gameTime);
                    UpdatePlayers(gameTime);
                    EffectsManager.Update(gameTime);
                    WeaponDropManager.Update(gameTime);
                    grenadeManager.Update(gameTime);
                }



                if (++frameCounter == 6)
                {
                    Packet.WriteMovementPacket(player, gameTime, Me);
                    frameCounter = 0;
                }
                else if (frameCounter == 3)
                {
                    Packet.WriteBlockPacket(Me, changes);
                    changes.Clear();
                }

                if (frameCounter == 0 || frameCounter == 3)
                {
                    if (Me.IsHost)
                        TeamManager.SendTimeToSend();
                }

                if (timeTillRespawn > 0)
                {
                    timeTillRespawn -= gameTime.ElapsedGameTime.Milliseconds;

                    if (timeTillRespawn <= 0)
                    {

                        if (this is SwarmGame)
                        {
                            (this as SwarmGame).IRespawned();
                        }
                        else
                        {
                            if (toSetToAfterDeath != null)
                                player.SetWeapons(toSetToAfterDeath);
                            Vector3 spwn = TeamManager.GetReSpawnPoint(PersonalRandom, this);
                            Packet.WriteRespawnPacket(Me, ref spwn);
                        }
                    }
                }

            }
            else
            {
                while (Me.IsDataAvailable)
                {
                    NetworkGamer sender;
                    Me.ReceiveData(Packet.PacketReader, out sender);
                    switch (Packet.GetPacketID())
                    {
                        case Packet.PACKETID_GETMEATEAMMR:
                            TeamManager.Team ttt = TeamManager.CalculateWhichTeamForPlayer(sender.Id);
                            Packet.WriteToAddToTeamManager(MinerOfDuty.Session.LocalGamers[0], sender.Id, ttt);
                            break;
                        case Packet.PACKETID_TOADDTOTEAM:
                            byte newPersonID;
                            TeamManager.Team team;
                            Packet.ReadToAddToTeamManager(out newPersonID, out team);
                            TeamManager.AddPlayer(newPersonID, team);
                            break;
                        case Packet.PACKETID_PLAYERLEVEL:
                            if (MinerOfDuty.Session.FindGamerById(sender.Id) != null)
                            {
                                Packet.ReadPlayerLevel(MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats);
                            }
                            break;
                        case Packet.PACKETID_SEED:
                            int seed = Packet.GetSeed();
                            Random = new CountedRandom(seed);
                            Console.WriteLine("Seed: " + seed);
                            Thread t = new Thread(GenerateWorld);
                            t.IsBackground = true;
                            t.Name = "World Gen";
                            t.Start();
                            break;
                        case Packet.PACKETID_DONEGENERATINGWORLD:
                            whoIsDone[sender.Id] = true;
                            playerBodies.Add(sender.Id, new PlayerBody(gd, sender.Id, sender.Gamertag, TeamManager.IsOnMyTeam(sender.Id) ? Color.Green : Color.Red));
                            Packet.ReadDoneGeneratingWorldPacket(playerBodies[sender.Id]);
                            break;
                    }
                }

                int num = 0;
                for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                {
                    if (whoIsDone[MinerOfDuty.Session.AllGamers[i].Id])
                    {
                        num++;
                    }
                }
                if (num == MinerOfDuty.Session.AllGamers.Count)
                {
                    CountDown = new TimeSpan(0, 0, 0, 5, 0);
                    everyOnesDone = true;
                }
            }

            if (isGenerated == false || everyOnesDone == false)
            {
                dotDelay += gameTime.ElapsedGameTime.Milliseconds;
                if (dotDelay > 1000)
                {
                    dot++;
                    dotDelay = 0;
                }
                if (dot == 4)
                    dot = 0;
            }

            if (isGenerated == true && everyOnesDone == false)
            {
#if XBOX
                WaitingForTimeSpan = WaitingForTimeSpan.Add(gameTime.ElapsedGameTime);
#endif

                if (WaitingForTimeSpan.TotalSeconds > 15)
                {
                    MinerOfDuty.DrawMenu(true);
                    MessageBox.ShowMessageBox(delegate(int selected)
                    {
                        Audio.PlaySound(Audio.SOUND_UICLICK);
                    }, new string[] { "OK" }, 0, new string[] { "UNABLE TO START MATCH" });
                }
            }

            if (showWeaponsDisabled > 0)
            {
                showWeaponsDisabled -= gameTime.ElapsedGameTime.Milliseconds;
            }

            if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                MinerOfDuty.Session.Update();
        }

        protected virtual void HandleUnknownPacketID(NetworkGamer sender, byte packetID, GameTime gameTime)
        {
           // OtherPacketHandling(sender, packetID);
        }

        protected virtual void Update2(GameTime gameTime)
        {

        }

        public static Vector3 goggles = new Vector3(.2f, .3f, .2f);
        public void Render(GraphicsDevice gd)
        {
            if (isGenerated && everyOnesDone)
            {
                lock (LightingManager.LightMapTextureLock)
                {
                    if (CountDown.TotalSeconds < 1.5)
                        Resources.BlockEffect.Parameters["GrayAmount"].SetValue((float)(CountDown.TotalSeconds / 1.5f));
                    else
                        Resources.BlockEffect.Parameters["GrayAmount"].SetValue(1f);

                    if (player.IsUsingGoggles)
                        Resources.BlockEffect.Parameters["Brightness"].SetValue(goggles);
                    else
                        Resources.BlockEffect.Parameters["Brightness"].SetValue(Vector3.Zero);

                    player.RenderArm();
                    Terrain.Render(cam);
                    LiquidManager.Render(Terrain.bf);
                    for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                    {
                        if (MinerOfDuty.Session.AllGamers[i].Id == Me.Id)
                        {
                            //if (cam == player.Camera)
                            //   
                            //else
                            //    playerBodies[Me.Id].Render(cam);
                        }
                        else
                            playerBodies[MinerOfDuty.Session.AllGamers[i].Id].Render(cam);
                    }
                    grenadeManager.Render(cam);
                    OtherRendering();
                    WeaponDropManager.Render(cam);

                    Terrain.RenderGlass(cam);
                    player.Render(gd);
                    ////////yea
                    Resources.BlockEffect.Parameters["Brightness"].SetValue(Vector3.Zero);
                    EffectsManager.Render(cam);


                    for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                    {
                        if (MinerOfDuty.Session.AllGamers[i].Id == Me.Id)
                        {
                            if (cam != player.Camera)
                                playerBodies[Me.Id].RenderName(cam, SB);
                        }
                        else
                            playerBodies[MinerOfDuty.Session.AllGamers[i].Id].RenderName(cam, SB);
                    }

                }
            }
        }

        public void UnSub()
        {
            try
            {
                TeamManager.GameOverEvent -= TeamManager_GameOverEvent;
            }
            catch (Exception) { }
            try
            {
                MinerOfDuty.CurrentPlayerProfile.LevelUpEvent -= CurrentPlayerProfile_LevelUpEvent;
            }
            catch (Exception) { }
            try
            {
                inGameMenu.SetClassEvent -= inGameMenu_SetClassEvent;
            }
            catch (Exception) { }
            try
            {

                MinerOfDuty.Session.HostChanged -= HostChanged;
            }
            catch (Exception) { }
            try
            {
                MinerOfDuty.Session.GamerJoined -= Session_GamerJoined;
            }
            catch (Exception) { }
            if (this is SwarmGame)
            {
                (this as SwarmGame).SwarmManager.Unsub();
            }
        }

        public virtual void Dispose()
        {
            lock (ThreadLocks)
                for (int i = 0; i < threads.Count; i++)
                    if (threads[i].IsAlive) threads[i].Abort();

            for (int i = 0; i < threads.Count; i++)
                while (threads[i].IsAlive) ;

            if (LightingManager != null)
            {
                LightingManager.Dispose();
                LightingManager = null;
            }
            if (LiquidManager != null)
            {
                LiquidManager.Dispose();
                LiquidManager = null;
            }
            if (Terrain != null)
            {
                Terrain.Dispose();
                Terrain = null;
            }
            if (playerBodies != null)
            {
                var keys = playerBodies.Keys.ToArray();
                foreach (var key in keys)
                    playerBodies[key].Dispose();
                playerBodies.Clear();
            }
            if (players != null)
            {
                var keys = players.Keys.ToArray();
                foreach (var key in keys)
                    players[key].armAnimation.Dispose();
                players.Clear();
            }

            Resources.BlockEffect.Parameters["GrayAmount"].SetValue(0f);
        }

        protected virtual void OtherRendering()
        {

        }

        protected int dot;
        protected int dotDelay;

        protected string specialtext1 = "DONTUSE";
        protected string specialtext2 = "DONTUSE";

        public bool CantPlaceBlockWarning = false;
        public void Draw(SpriteBatch sb)
        {
            if (GameOver)
            {
                TeamManager.DrawLeadboard(sb);
                TeamManager.DrawHits(sb);
                sb.DrawString(Resources.TitleFont, gameOverText, new Vector2(640 - Resources.TitleFont.MeasureString(gameOverText).X / 2f, 55), gameOverColor);
            }
            else if (isGenerated && everyOnesDone && !hasStarted)
            {
                if (choseClass != null)
                    choseClass.Draw(sb);
                sb.DrawString(Resources.TitleFont, CountDown.Seconds.ToString(), new Vector2(640 - (Resources.TitleFont.MeasureString(CountDown.Seconds.ToString()).X / 2f), 320 - (Resources.TitleFont.MeasureString(CountDown.Seconds.ToString()).Y / 2f)),
                    Color.Yellow);
            }
            else if (isGenerated && everyOnesDone)
            {
                if (choseClass != null)
                    choseClass.Draw(sb);

                if (!player.dead && choseClass == null)
                {
                    player.Draw(sb);
                    if (DrawHud)
                        InfoScreen.DrawMinimap(sb);
                }



                if (choseClass == null)
                {
                    if (DrawHud)
                    {
                        InfoScreen.Draw(sb);
                        TeamManager.Draw(sb);
                    }
                }

                if (DrawHud)
                {
                    if (player.dead && this is SwarmGame && (this as SwarmGame).isGhosting)
                    {
                        sb.DrawString(Resources.Font, "YOU'RE DEAD", new Vector2(640 - (Resources.Font.MeasureString("YOU'RE DEAD").X / 2f), 320 - (Resources.Font.MeasureString("YOU'RE DEAD").Y / 2f)),
                        Color.DarkRed);
                    }
                    else if (player.dead)
                    {
                        sb.DrawString(Resources.Font, "KILLED BY: " + killedBy, new Vector2(640 - (Resources.Font.MeasureString("KILLED BY: " + killedBy).X / 2f), 320 - (Resources.Font.MeasureString("Killed By: " + killedBy).Y / 2f)),
                        Color.DarkRed);
                    }
                }


                OtherDrawing(sb);

                if (DrawHud)
                {
                    if (sayCantPlaceNextToGold)
                        sb.DrawString(Resources.Font, "Can't Change Block Near A Gold Block".ToUpper(), new Vector2(640, 400), Color.Red, 0, Resources.Font.MeasureString("Can't Place Block Next To Gold Block".ToUpper()) / 2f, 1, SpriteEffects.None, 0);
                    else if (CantPlaceBlockWarning)
                        sb.DrawString(Resources.Font, "Can't Change Block NOW".ToUpper(), new Vector2(640, 400), Color.Red, 0, Resources.Font.MeasureString("Can't Place Block NOW".ToUpper()) / 2f, 1, SpriteEffects.None, 0);
                }

                if (DrawHud)
                    if (timeTillRespawn > 0)
                    {
                        sb.DrawString(Resources.Font, ((int)(timeTillRespawn / 1000f)).ToString(), new Vector2(640, 360), Color.White, 0, Resources.Font.MeasureString(((int)(timeTillRespawn / 1000f)).ToString()) / 2f, 1, SpriteEffects.None, 0);
                    }

                if (isShowingInGameMenu)
                    inGameMenu.Draw(sb);
                else if (Input.ControllingPlayerNewGamePadState.Buttons.Back == ButtonState.Pressed)
                    if (DrawHud)
                        TeamManager.DrawLeadboard(sb);

            }
            else if (isGenerated)
            {
                sb.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, "WAITING FOR OTHERS" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""),
                    new Vector2(640 - (Resources.Font.MeasureString("WAITING FOR OTHERS").X / 2),
                        320 - (Resources.Font.LineSpacing / 2f)), Color.White);
                string text = "";
                string text2 = "";
                if (type == GameModes.TeamDeathMatch)
                {
                    text = "FIGHT THE OTHER TEAM TILL";
                    text2 = "THE SCORE OR TIME LIMIT IS REACHED";
                }
                else if (type == GameModes.FortWars)
                {
                    text = "QUICKLY BUILD UP A FORT";
                    text2 = "AND THEN DEFEND IT";
                }
                else if (type == GameModes.FreeForAll)
                {
                    text = "FIGHT FOR YOURSELF AS IT'S";
                    text2 = "YOU VERSUS EVERYONE";
                }
                else if (type == GameModes.SearchNMine)
                {
                    text = "EITHER DEFEND OR MINE GOLD BLOCKS";
                    text2 = "THAT ARE PLACED AROUND THE MAP";
                }
                else if (type == GameModes.SwarmMode)
                {
                    text = "DEFEND THE GOLD BLOCKS WHILE";
                    text2 = "KILLING OFF ZOMBIES";
                }
                else if (type == GameModes.CustomMap)
                {
                    text = "FIGHT THE OTHER TEAM TILL";
                    text2 = "THE SCORE OR TIME LIMIT IS REACHED";
                }
                else if (type == GameModes.KingOfTheBeach)
                {
                    text = "BE THE ONLY PLAYER ON THE";
                    text2 = "ISLAND TO BE THE KING";
                }

                if (specialtext1 != "DONTUSE")
                    text = specialtext1;
                if (specialtext2 != "DONTUSE")
                    text2 = specialtext2;

                sb.DrawString(Resources.NameFont, text, new Vector2(640, 520), Color.White, 0, Resources.NameFont.MeasureString(text) / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.NameFont, text2, new Vector2(640, 520 + (Resources.NameFont.LineSpacing * 1.2f)), Color.White, 0, Resources.NameFont.MeasureString(text2) / 2f, 1, SpriteEffects.None, 0);
            }
            else
            {
                
                sb.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                if (waitingForMatch)
                {
                    sb.DrawString(Resources.Font, "WAITING FOR MATCH TO END" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""),
                        new Vector2(640 - (Resources.Font.MeasureString("WAITING FOR MATCH TO END").X / 2),
                            320 - (Resources.Font.LineSpacing / 2f)), Color.White);
                }
                else
                {
                    sb.DrawString(Resources.Font, "GENERATING WORLD" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""),
                                            new Vector2(640 - (Resources.Font.MeasureString("GENERATING WORLD").X / 2),
                                                320 - (Resources.Font.LineSpacing / 2f)), Color.White);
                }
                string text = "";
                string text2 = "";
                if (type == GameModes.TeamDeathMatch || type == GameModes.CustomTDM)
                {
                    text = "FIGHT THE OTHER TEAM TILL";
                    text2 = "THE SCORE OR TIME LIMIT IS REACHED";
                }
                else if (type == GameModes.FortWars)
                {
                    text = "QUICKLY BUILD UP A FORT";
                    text2 = "AND THEN DEFEND IT";
                }
                else if (type == GameModes.FreeForAll || type == GameModes.CustomFFA)
                {
                    text = "FIGHT FOR YOURSELF AS IT'S";
                    text2 = "YOU VERSUS EVERYONE";
                }
                else if (type == GameModes.SearchNMine || type == GameModes.CustomSNM)
                {
                    text = "EITHER DEFEND OR MINE GOLD BLOCKS";
                    text2 = "THAT ARE PLACED AROUND THE MAP";
                }
                else if (type == GameModes.SwarmMode || type == GameModes.CustomSM)
                {
                    text = "DEFEND THE GOLD BLOCKS WHILE";
                    text2 = "KILLING OFF ZOMBIES";
                }
                else if (type == GameModes.CustomMap)
                {
                    text = "FIGHT THE OTHER TEAM TILL";
                    text2 = "THE SCORE OR TIME LIMIT IS REACHED";
                }
                else if (type == GameModes.KingOfTheBeach)
                {
                    text = "BE THE ONLY PLAYER ON THE";
                    text2 = "ISLAND TO BE THE KING";
                }

                if (specialtext1 != "DONTUSE")
                    text = specialtext1;
                if (specialtext2 != "DONTUSE")
                    text2 = specialtext2;

                sb.DrawString(Resources.NameFont, text, new Vector2(640, 520), Color.White, 0, Resources.NameFont.MeasureString(text) / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.NameFont, text2, new Vector2(640, 520 + (Resources.NameFont.LineSpacing * 1.2f)), Color.White, 0, Resources.NameFont.MeasureString(text2) / 2f, 1, SpriteEffects.None, 0);
            }
        }

        public int showWeaponsDisabled = 0;
        protected virtual void OtherDrawing(SpriteBatch sb)
        {
            if(showWeaponsDisabled > 0)
            {
                sb.DrawString(Resources.Font, "Weapons Disabled".ToUpper(), new Vector2(640, 280), Color.Red, 0, Resources.Font.MeasureString("Weapons Disabled".ToUpper()) / 2f, 1, SpriteEffects.None, 0);
            }
        }

        protected virtual void SetUpPlayers(GraphicsDevice gd)
        {


            Dictionary<byte, Vector3> spwns = TeamManager.GetTeamSpawn(Random, this);

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
              //  if (spwns.ContainsKey(MinerOfDuty.Session.AllGamers[i].Id))
              //  {
                    players.Add(MinerOfDuty.Session.AllGamers[i].Id, new Player(this, spwns[MinerOfDuty.Session.AllGamers[i].Id] + new Vector3(0, 1.4f, 0), gd, MinerOfDuty.Session.AllGamers[i].Id));
                    //    playerBodies.Add(MinerOfDuty.Session.AllGamers[i].Id, new PlayerBody(gd, MinerOfDuty.Session.AllGamers[i].Id));
                    movementPacketStates.Add(MinerOfDuty.Session.AllGamers[i].Id, new MovementPacketState[]{ 
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0, 0),
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0, 0)});
                    rollingAvgs[MinerOfDuty.Session.AllGamers[i].Id] = new Networking.RollingAverage();
               // }
               // else
               // {
                //    throw new Exception("KEY MISSING, spawns count" + spwns.Count);
               // }
            }

            player = players[Me.Id];

            player.Camera = new FPSCamera(gd, player.position, Vector3.Forward);
            cam = player.Camera;

            choseClass = new ChooseClass();
            choseClass.SetClassEvent += new ChooseClass.SetClass(choseClass_SetClassEvent);
        }

        void choseClass_SetClassEvent(CharacterClass classToSet)
        {
            choseClass.SetClassEvent -= choseClass_SetClassEvent;
            choseClass = null;
            player.SetWeapons(classToSet);
        }

        private void UpdatePlayers(GameTime gameTime)
        {
            if (GameOver == false)
                if (cam == player.Camera)
                {
                    if (DontUpdateplayerMovementsTillReset)
                        player.Update(gameTime, Input.Empty);
                    else if (choseClass != null)
                        player.Update(gameTime, Input.Empty);
                    else if (isShowingInGameMenu)
                        player.Update(gameTime, Input.Empty);
                    else if (hasStarted)
                        player.Update(gameTime, Input.ControllingPlayerNewGamePadState);
                    else
                        player.Update(gameTime, Input.Empty);
                }
            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                if (MinerOfDuty.Session.AllGamers[i].Id != Me.Id && players[MinerOfDuty.Session.AllGamers[i].Id].dead == false)
                    MovementPacketState.InterpolatePlayer(gameTime,
                        ref movementPacketStates[MinerOfDuty.Session.AllGamers[i].Id][0],
                        ref movementPacketStates[MinerOfDuty.Session.AllGamers[i].Id][1],
                        players[MinerOfDuty.Session.AllGamers[i].Id]);

                playerBodies[MinerOfDuty.Session.AllGamers[i].Id].Update(gameTime, players[MinerOfDuty.Session.AllGamers[i].Id]);
            }
        }

        public void Reset(Dictionary<byte, Vector3> pos)
        {
            DontUpdateplayerMovementsTillReset = false;
            players = new Dictionary<byte, Player>();
            movementPacketStates = new Dictionary<byte, MovementPacketState[]>();

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                players.Add(MinerOfDuty.Session.AllGamers[i].Id, new Player(this, pos[MinerOfDuty.Session.AllGamers[i].Id] + new Vector3(0, 1.4f, 0), gd, MinerOfDuty.Session.AllGamers[i].Id));
                movementPacketStates.Add(MinerOfDuty.Session.AllGamers[i].Id, new MovementPacketState[]{ 
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0),
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0)});
            }

            CountDown = new TimeSpan(0, 0, 5);
            hasStarted = false;

            player = players[Me.Id];

            player.Camera = new FPSCamera(gd, player.position, Vector3.Forward);
            cam = player.Camera;

            choseClass = new ChooseClass();
            choseClass.SetClassEvent += new ChooseClass.SetClass(choseClass_SetClassEvent);
        }

        private bool DontUpdateplayerMovementsTillReset;
        public void DontUpdatePlayerMovementsTillReset()
        {
            DontUpdateplayerMovementsTillReset = true;
        }

        public List<Vector3> goldBlocks;
        public void SpawnGoldBlocks(int amount)
        {
            List<Vector3> blocks = new List<Vector3>();

            Vector2 tmp;
            for (int i = 0; i < amount; i++)
            {
                tmp = new Vector2(
                    Random.Next(4, 124),
                    Random.Next(4, 124));

                if (IsNear(blocks, tmp))
                {
                    i--;
                }
                else
                {
                    for (int y = 62; y > 0; y--)
                        if (Terrain.blocks[(int)tmp.X, y, (int)tmp.Y] != Block.BLOCKID_AIR)
                        {
                            if (Terrain.blocks[(int)tmp.X, y, (int)tmp.Y] == Block.BLOCKID_GRASS)
                            {
                                blocks.Add(new Vector3(tmp.X, y + 1, tmp.Y));
                                break;
                            }
                            else
                            {
                                i--;
                                break;
                            }
                        }
                }
            }

            goldBlocks = blocks;

            for (int i = 0; i < goldBlocks.Count; i++)
            {
                Terrain.AddBlocks((int)goldBlocks[i].X, (int)goldBlocks[i].Y, (int)goldBlocks[i].Z, Block.BLOCKID_GOLD);
            }
        }

        public void SpawnGoldBlocks(List<Vector3> blocks)
        {
            goldBlocks = blocks;

            for (int i = 0; i < goldBlocks.Count; i++)
            {
                Terrain.AddBlocks((int)goldBlocks[i].X, (int)goldBlocks[i].Y, (int)goldBlocks[i].Z, Block.BLOCKID_GOLD);
            }
        }

        public void KillOffGoldBlocks()
        {
            for (int i = 0; i < goldBlocks.Count; i++)
            {
                if (Terrain.blocks[(int)goldBlocks[i].X, (int)goldBlocks[i].Y, (int)goldBlocks[i].Z] == Block.BLOCKID_GOLD)
                    Terrain.RemoveBlocks((int)goldBlocks[i].X, (int)goldBlocks[i].Y, (int)goldBlocks[i].Z);
            }
        }

        private bool IsNear(List<Vector3> others, Vector2 tmp)
        {
            for (int i = 0; i < others.Count; i++)
            {
                if (Vector2.Distance(tmp, new Vector2(others[i].X, others[i].Y)) < 15)
                    return true;
            }
            return false;
        }

        public void Deactivated()
        {
            if (inGameMenu != null)
            {
                inGameMenu.SelectFirst();
                isShowingInGameMenu = true;
                if (this is SwarmGame && MinerOfDuty.Session.AllGamers.Count == 1)
                {
                    paused = true;
                }
            }
        }

        public void Activated()
        {
            if (inGameMenu != null)
            {
                isShowingInGameMenu = false;
                paused = false;
            }
        }

        public Random GetRandom
        {
            get { return Random; }
        }

        public LightingManager GetLightingManager
        {
            get { return LightingManager; }
        }

        public LiquidManager GetLiquidManager
        {
            get { return LiquidManager; }
        }

        public Terrain.UnderLiquid GetUnderLiquid
        {
            get { return player.underLiquid; }
        }

        public Terrain GetTerrain
        {
            get { return Terrain; }
        }

    }

    public class SwarmGame : MultiplayerGame
    {
        public ISwarmieManager SwarmManager;
        public bool isGhosting = false;

        public SwarmGame(MinerOfDuty game1, TeamManager tm, GraphicsDevice gd, out EventHandler<GamerLeftEventArgs> gamerLeft, bool sendSeed = true)
            : base(game1, GameModes.SwarmMode, tm, gd, out gamerLeft, sendSeed)
        {
            MinerOfDuty.Session.HostChanged += new EventHandler<HostChangedEventArgs>(Session_HostChanged);

        }

        private void Session_HostChanged(object sender, HostChangedEventArgs e)
        {
            MinerOfDuty.game.UnSub();
            MinerOfDuty.game.Dispose();
            MinerOfDuty.DrawMenu();

            MessageBox.ShowMessageBox(delegate(int selected) { }, new string[] { "OKAY" }, 0, new string[] { "HOST LEFT." });

            if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                MinerOfDuty.Session.Dispose();
        }

        public override void Dispose()
        {
            SwarmManager.Dispose();
            base.Dispose();
            
        }

        public void IRespawned()
        {
            player.health = 1000;
            isGhosting = true;
            cam = player.Camera;
        }

        protected override void GenerateWorld()
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif


                Terrain = new Terrain(this);
                LightingManager = new LightingManager(Terrain, gd);
                Liquid.game = null;
                LiquidManager = new LiquidManager(this, gd);
                Terrain.Initialize(gd, Terrain.CreateBlocksFlat, 128, false);
                SpawnGoldBlocks(15);

                LiquidManager.Start();
                LightingManager.LightWorld();

                frCam = new FreeRoamCamera(gd, new Vector3(106, 64, 57), Vector3.Forward);

                SetUpPlayers(gd);

                player.inventory.GetSpecialGrenade.Throw();
                player.inventory.GetSpecialGrenade.Throw();
                player.inventory.GetSpecialGrenade.Throw();

                

                if (MinerOfDuty.Session.IsHost)
                    SwarmManager = new SwarmieManager(this);
                else
                    SwarmManager = new FooSwarmieManager(this);

                Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);

                System.GC.Collect();

                isGenerated = true;
                dot = 0;
            }
            catch (Exception e)
            {
                isGenerated = false;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        protected override void SetUpPlayers(GraphicsDevice gd)
        {
            Dictionary<byte, Vector3> spwns = TeamManager.GetTeamSpawn(Random, this);

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                if (spwns.ContainsKey(MinerOfDuty.Session.AllGamers[i].Id))
                {
                    players.Add(MinerOfDuty.Session.AllGamers[i].Id, new Player(this, spwns[MinerOfDuty.Session.AllGamers[i].Id] + new Vector3(0, 1.4f, 0), gd, MinerOfDuty.Session.AllGamers[i].Id));
                    movementPacketStates.Add(MinerOfDuty.Session.AllGamers[i].Id, new MovementPacketState[]{ 
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0),
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0)});
                    rollingAvgs[MinerOfDuty.Session.AllGamers[i].Id] = new Networking.RollingAverage();
                }
                else
                {
                    players.Add(MinerOfDuty.Session.AllGamers[i].Id, new Player(this, spwns[MinerOfDuty.Session.AllGamers[i].Id] + new Vector3(0, 1.4f, 0), gd, MinerOfDuty.Session.AllGamers[i].Id));
                    movementPacketStates.Add(MinerOfDuty.Session.AllGamers[i].Id, new MovementPacketState[]{ 
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0),
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0)});
                    rollingAvgs[MinerOfDuty.Session.AllGamers[i].Id] = new Networking.RollingAverage();
                }
            }

            player = players[Me.Id];

            player.Camera = new FPSCamera(gd, player.position, Vector3.Forward);
            cam = player.Camera;

            player.SetWeapons(new CharacterClass(
                new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false),
                new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
                new ItemSlot(InventoryItem.SandBlock),
                new ItemSlot(InventoryItem.StoneBlock))
                );
            (player.inventory.useableItems[0] as Gun).MaxFillAmmo();
            (player.inventory.useableItems[1] as Gun).MaxFillAmmo();
        }

        protected override void Update2(GameTime gameTime)
        {
            if (player.dead )
                player.Camera.Update(0, 0, ref player.position);

            SwarmManager.Update(gameTime);
            if (showPurchase > 0)
                showPurchase -= gameTime.ElapsedGameTime.Milliseconds;
        }

        protected override void HandleUnknownPacketID(NetworkGamer sender, byte packetID, GameTime gameTime)
        {
            switch (packetID)
            {
                case Packet.PACKETID_SWARMIESHOTAT:
                    short id;
                    float damage;
                    KillText.DeathType type;
                    PlayerBody.Hit hit;
                    Packet.ReadSwarmieShotPacket(out damage, out id, out type, out hit);

                    if (type == KillText.DeathType.Knife)
                    {
                        if (sender.IsLocal)
                            SwarmManager.StoreMenu.cash += (TeamManager as SwarmManager).AddZombieKnifed();
                        else
                            (TeamManager as SwarmManager).AddZombieKnifed(sender.Id);
                    }
                    else
                    {
                        if (sender.IsLocal)
                            SwarmManager.StoreMenu.cash += (TeamManager as SwarmManager).AddZombieShot(hit);
                        else
                            (TeamManager as SwarmManager).AddZombieShot(sender.Id, hit);
                    }

                    if (MinerOfDuty.Session.IsHost)
                    {
                        if ((SwarmManager as SwarmieManager).HurtSwarmie(players[sender.Id], damage, id))
                            Packet.WriteSwarmieKilled(Me, sender.Id, id);
                    }
                    break;
                case Packet.PACKETID_SWARMIEKILLED:

                    byte killer;
                    short swarmieID = Packet.ReadSwarmieKilled(out killer);

                    if (MinerOfDuty.Session.IsHost == false)
                        (SwarmManager as FooSwarmieManager).KillSwarm(swarmieID);

                    (TeamManager as SwarmManager).AddKill(killer);

                    break;
                case Packet.PACKETID_SWARMIEGRENADED:

                    byte grenader;
                    int swarmiesKilled;
                    short[] swarmiesKilledID = Packet.ReadSwarmieGrenaded(out grenader, out swarmiesKilled);

                    if (grenader == Me.Id)
                        (TeamManager as SwarmManager).AddZombieGrenaded(swarmiesKilled);
                    else
                        (TeamManager as SwarmManager).AddZombieGrenaded(grenader, swarmiesKilled);

                    if (MinerOfDuty.Session.IsHost == false)
                    {
                        for (int i = 0; i < swarmiesKilledID.Length; i++)
                            (SwarmManager as FooSwarmieManager).KillSwarm(swarmiesKilledID[i]);
                    }

                    break;
                case Packet.PACKETID_SWARMIEMADE:
                    Vector3 pos;
                    Packet.ReadSwarmieAdded(out id, out pos);
                    if (MinerOfDuty.Session.IsHost == false)
                        (SwarmManager as FooSwarmieManager).AddSwarmie(id, pos);
                    break;
                case Packet.PACKETID_SWARMIEUPDATE:
                    if (MinerOfDuty.Session.Host.IsLocal)
                        return;

                    float time = Packet.PacketReader.ReadSingle();//timestamp in total milliseconds
                 //   float timeDelta = (float)gameTime.TotalGameTime.TotalMilliseconds - time;
                 ////   rollingAvgs[sender.Id].AddTime(timeDelta);
                 //   float timeDeviation = timeDelta - rollingAvgs[sender.Id].RollingAveragee;
                 //   time -= ((float)sender.RoundtripTime.TotalMilliseconds / 2f) - timeDeviation;

                    (SwarmManager as FooSwarmieManager).ReadMovements(time);
                    break;
                case Packet.PACKETID_SWARMIEATTACKEDPLAYER:
                    player.Attack(Packet.PacketReader.ReadSingle());

                    if (player.dead)
                    {
                        Packet.PacketWriter.Write(Packet.PACKETID_SWARMIEIDIED);
                        Me.SendData(Packet.PacketWriter, SendDataOptions.Reliable);
                    }

                    break;
                case Packet.PACKETID_SWARMIEIDIED:
                    if ((MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats).Deaths == 0)
                        (MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats).AddDeath();

                    for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                    {
                        if ((MinerOfDuty.Session.AllGamers[i].Tag as GamePlayerStats).Deaths == 0)
                            return;
                    }

                    //okay everyone is dead.... game over gg no re
                    (TeamManager as SwarmManager).InvokeGameOver();

                    break;
            }
        }

        protected override void OtherRendering()
        {
            SwarmManager.Render(cam);
        }

        public int showPurchase = 0;
        public int showError = 0;
        protected override void OtherDrawing(SpriteBatch sb)
        {
            SwarmManager.Draw(sb);

            if (showPurchase > 0)
            {
                sb.DrawString(Resources.Font, "$" + (SwarmManager.StoreMenu.Cash + 20), new Vector2(640, 280), Color.White, 0, Resources.Font.MeasureString("$" + SwarmManager.StoreMenu.Cash) / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.Font, "-$20", new Vector2(640, 280 + Resources.Font.LineSpacing), Color.Red, 0, Resources.Font.MeasureString("-$20") / 2f, 1, SpriteEffects.None, 0);
            }

            if (showError > 0)
            {
                sb.DrawString(Resources.Font, "insufficient funds".ToUpper(), new Vector2(640, 280), Color.Red, 0, Resources.Font.MeasureString("insufficient funds".ToUpper()) / 2f, 1, SpriteEffects.None, 0);
            }

            
        }
    }

    public class CustomSwarmGame : SwarmGame
    {

        public override bool IsCustom
        {
            get
            {
                return true;
            } 
        }

        private MemoryStream ms;

        public CustomSwarmGame(MinerOfDuty game1, TeamManager tm, GraphicsDevice gd, out EventHandler<GamerLeftEventArgs> gamerLeft,
            MemoryStream mapData)
            : base(game1, tm, gd, out gamerLeft, false)
        {
            specialtext1 = "";
            specialtext2 = "";
            this.ms = mapData;
            Thread t = new Thread(GenerateWorld);
            t.IsBackground = true;
            t.Start();
        }

        protected override void GenerateWorld()
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                //decode
                using (MemoryStream mapData = new MemoryStream())
                {
                    ms.Position = 0;
                    MemoryStream input = new MemoryStream();
                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();
                    byte[] buffer = new byte[ms.Length - 4];
                    ms.Read(buffer, 0, buffer.Length);

                    input.Write(buffer, 0, buffer.Length);
                    input.Position = 0;
                    LZF.lzf.Decompress(input, mapData);
                    input.Close();
                    mapData.Position = 0;
                    ms.Dispose();
                    ms = null;

                    using (BinaryReader br = new BinaryReader(mapData))
                    {
                        MapGameInfoLoader data = MapGameInfoLoader.Load(br);

                        specialtext1 = "MAP NAME: " + data.MapName;
                        specialtext2 = "MAP AUTHOR: " + data.Author;

                        
                        //load spawnPoints and set them as ponts
                        (TeamManager as CustomSMManager).SpawnPoint = data.SpawnPosition;


                        Random = new CountedRandom(data.Seed);

                        Terrain = new Terrain(this);
                        LightingManager = new LightingManager(Terrain, gd);
                        Liquid.game = null;
                        LiquidManager = new LiquidManager(this, gd);
                        Terrain.CreateBlocksFromSave(gd, data.WorldGeneration, br, data.Size, data.GenerateTrees, data.Version);
                        EditingEnabled = data.EditingEnabled;
                        WeaponsEnabled = data.WeaponsEnabled;

                        SpawnGoldBlocks(data.GoldBlockPostions.ToList());/////////////Set blocks in swarm maangaer here

                        if (data.Version >= 2)//technically nnot need girl
                            WeaponDropManager.LoadSpawners(br);

                        LiquidManager.Start();
                        LightingManager.LightWorld();

                        frCam = new FreeRoamCamera(gd, new Vector3(106, 64, 57), Vector3.Forward);

                        SetUpPlayers(gd);

                        player.inventory.GetSpecialGrenade.Throw();
                        player.inventory.GetSpecialGrenade.Throw();
                        player.inventory.GetSpecialGrenade.Throw();

                        if (MinerOfDuty.Session.IsHost)
                        {
                            SwarmManager = new SwarmieManager(this);
                            List<Vector3> zombieSpawns = data.ZombieSpawnPositions.ToList();

                            for (int i = 0; i < zombieSpawns.Count; i++)
                            {
                                if (zombieSpawns[i] == Vector3.Zero)
                                {
                                    zombieSpawns.RemoveAt(i);
                                    i--;
                                }
                            }

                            (SwarmManager as SwarmieManager).ZombieSpawns = data.ZombieSpawnPositions;
                        }
                        else
                            SwarmManager = new FooSwarmieManager(this);




                        Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);


                    }
                }

                System.GC.Collect();

                isGenerated = true;
                dot = 0;
            }
            catch (Exception e)
            {
                isGenerated = false;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

    }

    public class CustomTDM : MultiplayerGame
    {

        public override bool IsCustom
        {
            get
            {
                return true;
            }
        }

        public CustomTDM(MinerOfDuty game1, CustomTDMManager tm, GraphicsDevice gd, out EventHandler<GamerLeftEventArgs> gamerLeft,
            MemoryStream mapData)
            : base(game1, GameModes.CustomTDM, tm, gd, out gamerLeft, false)
        {
            specialtext1 = "";
            specialtext2 = "";
            this.ms = mapData;
            Thread t = new Thread(GenerateWorld);
            t.IsBackground = true;
            t.Start();
        }

        private MemoryStream ms;
        protected override void GenerateWorld()
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                //decode
                using (MemoryStream mapData = new MemoryStream())
                {
                    ms.Position = 0;
                    MemoryStream input = new MemoryStream();
                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();
                    byte[] buffer = new byte[ms.Length - 4];
                    ms.Read(buffer, 0, buffer.Length);

                    input.Write(buffer, 0, buffer.Length);
                    input.Position = 0;
                    LZF.lzf.Decompress(input, mapData);
                    input.Close();
                    mapData.Position = 0;
                    ms.Dispose();
                    ms = null;

                    using (BinaryReader br = new BinaryReader(mapData))
                    {
                        MapGameInfoLoader data = MapGameInfoLoader.Load(br);

                        specialtext1 = "MAP NAME: " + data.MapName;
                        specialtext2 = "MAP AUTHOR: " + data.Author;

                        List<Vector3> teamASpawns = new List<Vector3>(data.TeamASpawnPosition);
                        for (int i = 0; i < teamASpawns.Count; i++)
                        {
                            if (teamASpawns[i] == Vector3.Zero)
                            {
                                teamASpawns.RemoveAt(i);
                                i--;
                            }
                        }

                        List<Vector3> teamBSpawns = new List<Vector3>(data.TeamBSpawnPosition);
                        for (int i = 0; i < teamBSpawns.Count; i++)
                        {
                            if (teamBSpawns[i] == Vector3.Zero)
                            {
                                teamBSpawns.RemoveAt(i);
                                i--;
                            }
                        }

                        (TeamManager as CustomTDMManager).TeamASpawnPoint = teamASpawns.ToArray();
                        (TeamManager as CustomTDMManager).TeamBSpawnPoint = teamBSpawns.ToArray();

                        Random = new CountedRandom(data.Seed);

                        (TeamManager as TeamTeamManager).teamAName = data.TeamAName;
                        (TeamManager as TeamTeamManager).teamBName = data.TeamBName;

                        Terrain = new Terrain(this);
                        LightingManager = new LightingManager(Terrain, gd);
                        Liquid.game = null;
                        LiquidManager = new LiquidManager(this, gd);
                        Terrain.CreateBlocksFromSave(gd, data.WorldGeneration, br, data.Size, data.GenerateTrees, data.Version);
                        EditingEnabled = data.EditingEnabled;
                        WeaponsEnabled = data.WeaponsEnabled;

                        if (data.Version >= 2)
                            WeaponDropManager.LoadSpawners(br);

                        LiquidManager.Start();
                        LightingManager.LightWorld();

                        frCam = new FreeRoamCamera(gd, new Vector3(106, 64, 57), Vector3.Forward);

                        SetUpPlayers(gd);

                        Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);


                    }
                }

                System.GC.Collect();

                isGenerated = true;
                dot = 0;
            }
            catch (Exception e)
            {
                isGenerated = false;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

    }

    public class CustomFFA : MultiplayerGame
    {
         public override bool IsCustom
        {
            get
            {
                return true;
            }
        }

         public CustomFFA(MinerOfDuty game1, CustomFFAManager tm, GraphicsDevice gd, out EventHandler<GamerLeftEventArgs> gamerLeft,
            MemoryStream mapData)
            : base(game1, GameModes.CustomFFA, tm, gd, out gamerLeft, false)
        {
            specialtext1 = "";
            specialtext2 = "";
            this.ms = mapData;
            Thread t = new Thread(GenerateWorld);
            t.IsBackground = true;
            t.Start();
        }

        private MemoryStream ms;
        protected override void GenerateWorld()
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                //decode
                using (MemoryStream mapData = new MemoryStream())
                {


                    ms.Position = 0;
                    MemoryStream input = new MemoryStream();

                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();

                    byte[] buffer = new byte[ms.Length - 4];
                    ms.Read(buffer, 0, buffer.Length);

                    input.Write(buffer, 0, buffer.Length);
                    input.Position = 0;
                    LZF.lzf.Decompress(input, mapData);
                    input.Close();
                    mapData.Position = 0;
                    ms.Dispose();
                    ms = null;

                    using (BinaryReader br = new BinaryReader(mapData))
                    {
                        MapGameInfoLoader data = MapGameInfoLoader.Load(br);

                        specialtext1 = "MAP NAME: " + data.MapName;
                        specialtext2 = "MAP AUTHOR: " + data.Author;

                        List<Vector3> spawns = new List<Vector3>(data.SpawnPositions);
                        for (int i = 0; i < spawns.Count; i++)
                        {
                            if (spawns[i] == Vector3.Zero)
                            {
                                spawns.RemoveAt(i);
                                i--;
                            }
                        }

                        (TeamManager as CustomFFAManager).playerSpawns = spawns.ToArray();

                        Random = new CountedRandom(data.Seed);

                        Terrain = new Terrain(this);
                        LightingManager = new LightingManager(Terrain, gd);
                        Liquid.game = null;
                        LiquidManager = new LiquidManager(this, gd);
                        Terrain.CreateBlocksFromSave(gd, data.WorldGeneration, br, data.Size, data.GenerateTrees, data.Version);
                        EditingEnabled = data.EditingEnabled;
                        WeaponsEnabled = data.WeaponsEnabled;

                        if (data.Version >= 2)
                            WeaponDropManager.LoadSpawners(br);

                        LiquidManager.Start();
                        LightingManager.LightWorld();

                        frCam = new FreeRoamCamera(gd, new Vector3(106, 64, 57), Vector3.Forward);

                        SetUpPlayers(gd);

                        Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);


                    }
                }

                System.GC.Collect();

                isGenerated = true;
                dot = 0;
            }
            catch (Exception e)
            {
                isGenerated = false;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }
    }

    public class CustomKB : MultiplayerGame
    {
        public override bool IsCustom
        {
            get
            {
                return true;
            }
        }

        public CustomKB(MinerOfDuty game1, CustomKBManager tm, GraphicsDevice gd, out EventHandler<GamerLeftEventArgs> gamerLeft,
           MemoryStream mapData)
            : base(game1, GameModes.CustomKB, tm, gd, out gamerLeft, false)
        {
            specialtext1 = "";
            specialtext2 = "";
            this.ms = mapData;
            Thread t = new Thread(GenerateWorld);
            t.IsBackground = true;
            t.Start();
        }

        private MemoryStream ms;
        protected override void GenerateWorld()
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                //decode
                using (MemoryStream mapData = new MemoryStream())
                {


                    ms.Position = 0;
                    MemoryStream input = new MemoryStream();

                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();

                    byte[] buffer = new byte[ms.Length - 4];
                    ms.Read(buffer, 0, buffer.Length);

                    input.Write(buffer, 0, buffer.Length);
                    input.Position = 0;
                    LZF.lzf.Decompress(input, mapData);
                    input.Close();
                    mapData.Position = 0;
                    ms.Dispose();
                    ms = null;

                    using (BinaryReader br = new BinaryReader(mapData))
                    {
                        MapGameInfoLoader data = MapGameInfoLoader.Load(br);

                        specialtext1 = "MAP NAME: " + data.MapName;
                        specialtext2 = "MAP AUTHOR: " + data.Author;

                        List<Vector3> spawns = new List<Vector3>(data.SpawnPositions);
                        for (int i = 0; i < spawns.Count; i++)
                        {
                            if (spawns[i] == Vector3.Zero)
                            {
                                spawns.RemoveAt(i);
                                i--;
                            }
                        }

                        (TeamManager as CustomKBManager).playerSpawns = spawns.ToArray();
                        (TeamManager as CustomKBManager).SetData((int)data.KingPoint.X, (int)data.KingPoint.Z, data.Range);

                        Random = new CountedRandom(data.Seed);

                        Terrain = new Terrain(this);
                        LightingManager = new LightingManager(Terrain, gd);
                        Liquid.game = null;
                        LiquidManager = new LiquidManager(this, gd);
                        Terrain.CreateBlocksFromSave(gd, data.WorldGeneration, br, data.Size, data.GenerateTrees, data.Version);
                        EditingEnabled = data.EditingEnabled;
                        WeaponsEnabled = data.WeaponsEnabled;

                        if (data.Version >= 2)
                            WeaponDropManager.LoadSpawners(br);

                        LiquidManager.Start();
                        LightingManager.LightWorld();

                        frCam = new FreeRoamCamera(gd, new Vector3(106, 64, 57), Vector3.Forward);

                        SetUpPlayers(gd);

                        Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);


                    }
                }

                System.GC.Collect();

                isGenerated = true;
                dot = 0;
            }
            catch (Exception e)
            {
                isGenerated = false;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }
    }

    public class CustomSNM : MultiplayerGame
    {
        public override bool IsCustom
        {
            get
            {
                return true;
            }
        }

        public CustomSNM(MinerOfDuty game1, CustomSearchNMiner tm, GraphicsDevice gd, out EventHandler<GamerLeftEventArgs> gamerLeft,
            MemoryStream mapData)
            : base(game1, GameModes.CustomSNM, tm, gd, out gamerLeft, false)
        {
            specialtext1 = "";
            specialtext2 = "";
            this.ms = mapData;
            Thread t = new Thread(GenerateWorld);
            t.IsBackground = true;
            t.Start();
        }

        private MemoryStream ms;
        protected override void GenerateWorld()
        {
            try
            {
#if XBOX
            Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

                //decode
                using (MemoryStream mapData = new MemoryStream())
                {
                    ms.Position = 0;
                    MemoryStream input = new MemoryStream();

                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();
                    ms.ReadByte();

                    byte[] buffer = new byte[ms.Length - 4];
                    ms.Read(buffer, 0, buffer.Length);

                    input.Write(buffer, 0, buffer.Length);
                    input.Position = 0;
                    LZF.lzf.Decompress(input, mapData);
                    input.Close();
                    mapData.Position = 0;
                    ms.Dispose();
                    ms = null;

                    using (BinaryReader br = new BinaryReader(mapData))
                    {
                        MapGameInfoLoader data = MapGameInfoLoader.Load(br);

                        specialtext1 = "MAP NAME: " + data.MapName;
                        specialtext2 = "MAP AUTHOR: " + data.Author;

                        List<Vector3> teamASpawns = new List<Vector3>(data.TeamASpawnPosition);
                        for (int i = 0; i < teamASpawns.Count; i++)
                        {
                            if (teamASpawns[i] == Vector3.Zero)
                            {
                                teamASpawns.RemoveAt(i);
                                i--;
                            }
                        }

                        List<Vector3> teamBSpawns = new List<Vector3>(data.TeamBSpawnPosition);
                        for (int i = 0; i < teamBSpawns.Count; i++)
                        {
                            if (teamBSpawns[i] == Vector3.Zero)
                            {
                                teamBSpawns.RemoveAt(i);
                                i--;
                            }
                        }

                        (TeamManager as CustomSearchNMiner).TeamASpawnPoint = teamASpawns.ToArray();
                        (TeamManager as CustomSearchNMiner).TeamBSpawnPoint = teamBSpawns.ToArray();
                        (TeamManager as CustomSearchNMiner).GoldBlocks = data.GoldBlockPostions;

                        Random = new CountedRandom(data.Seed);

                        (TeamManager as TeamTeamManager).teamAName = data.TeamAName;
                        (TeamManager as TeamTeamManager).teamBName = data.TeamBName;

                        Terrain = new Terrain(this);
                        LightingManager = new LightingManager(Terrain, gd);
                        Liquid.game = null;
                        LiquidManager = new LiquidManager(this, gd);
                        Terrain.CreateBlocksFromSave(gd, data.WorldGeneration, br, data.Size, data.GenerateTrees, data.Version);
                        EditingEnabled = data.EditingEnabled;
                        WeaponsEnabled = data.WeaponsEnabled;

                        if (data.Version >= 2)
                            WeaponDropManager.LoadSpawners(br);

                        LiquidManager.Start();
                        LightingManager.LightWorld();

                        frCam = new FreeRoamCamera(gd, new Vector3(106, 64, 57), Vector3.Forward);

                        SetUpPlayers(gd);

                        Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);


                    }
                }

                System.GC.Collect();

                isGenerated = true;
                dot = 0;
            }
            catch (Exception e)
            {
                isGenerated = false;
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }
    }


}
