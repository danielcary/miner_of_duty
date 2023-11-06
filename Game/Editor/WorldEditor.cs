using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Threading;
using Miner_Of_Duty.Menus;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Miner_Of_Duty.LobbyCode;
using Miner_Of_Duty.Game.Networking;
using Lzf;
//using Miner_Of_Duty.BinaryWriterExtension;
//using Miner_Of_Duty.BinaryReaderExtension;

namespace Miner_Of_Duty.Game.Editor
{
    public class WorldEditorGameModeInfo
    {
        public enum SpawnPoints { Spawn, TeamSpawnA, TeamSpawnB, ZombieSpawn, King }

        private LocalNetworkGamer Me;
        private WorldEditor we;
        public WorldEditorGameModeInfo(WorldEditor we, LocalNetworkGamer me)
        {
            this.Me = me;
            this.we = we;
            KingPointSet += we.KingPointSet;
        }

        public void Init(GameModes gameMode, GraphicsDevice gd)
        {
            if (gameMode == GameModes.CustomTDM || gameMode == GameModes.CustomSNM)
            {
                teamASpwn[0] = new SpawnPoint(gd, "TEAM A SPAWN 1");
                teamASpwn[1] = new SpawnPoint(gd, "TEAM A SPAWN 2");
                teamASpwn[2] = new SpawnPoint(gd, "TEAM A SPAWN 3");

                teamBSpwn[0] = new SpawnPoint(gd, "TEAM B SPAWN 1");
                teamBSpwn[1] = new SpawnPoint(gd, "TEAM B SPAWN 2");
                teamBSpwn[2] = new SpawnPoint(gd, "TEAM B SPAWN 3");
            }
            else if (gameMode == GameModes.CustomFFA || gameMode == GameModes.CustomKB)
            {
                spawns[0] = new SpawnPoint(gd, "SPAWN 1");
                spawns[1] = new SpawnPoint(gd, "SPAWN 2");
                spawns[2] = new SpawnPoint(gd, "SPAWN 3");
                spawns[3] = new SpawnPoint(gd, "SPAWN 4");
                spawns[4] = new SpawnPoint(gd, "SPAWN 5");
                spawns[5] = new SpawnPoint(gd, "SPAWN 6");

                if (gameMode == GameModes.CustomKB)
                    kingPoint = new SpawnPoint(gd, "KING POINT");
            }
            else if (gameMode == GameModes.CustomSM)
            {
                spawns[0] = new SpawnPoint(gd, "SPAWN 1");
                spawns[1] = new SpawnPoint(gd, "SPAWN 1");
                spawns[2] = new SpawnPoint(gd, "SPAWN 1");
                spawns[3] = new SpawnPoint(gd, "SPAWN 1");
                spawns[4] = new SpawnPoint(gd, "SPAWN 1");
                spawns[5] = new SpawnPoint(gd, "SPAWN 1");
                zombieSpawns[0] = new SpawnPoint(gd, "ZOMBIE SPAWN 1");
                zombieSpawns[1] = new SpawnPoint(gd, "ZOMBIE SPAWN 2");
                zombieSpawns[2] = new SpawnPoint(gd, "ZOMBIE SPAWN 3");
                zombieSpawns[3] = new SpawnPoint(gd, "ZOMBIE SPAWN 4");
                zombieSpawns[4] = new SpawnPoint(gd, "ZOMBIE SPAWN 5");
                zombieSpawns[5] = new SpawnPoint(gd, "ZOMBIE SPAWN 6");
            }
        }

        // team spawns
        public SpawnPoint[] teamASpwn = new SpawnPoint[3], teamBSpwn = new SpawnPoint[3];
        public bool[] drawTeamA = new bool[3] { false, false, false }, drawTeamB = new bool[3] { false, false, false };
        public void SetTeamASpawnPoint(int spwnPoint, Vector3 point)
        {
            SetTeamASpawnPoint(spwnPoint, ref point, true);
        }

        public void SetTeamASpawnPoint(int spwnPoint, ref Vector3 point, bool sendMessage)
        {
            drawTeamA[spwnPoint] = true;
            teamASpwn[spwnPoint].Name = "TEAM A SPAWN " + (spwnPoint + 1);
            teamASpwn[spwnPoint].color = Color.Red;
            teamASpwn[spwnPoint].Position = point;

            if (sendMessage)
                Packet.WriteSpawnPlaced(Me, SpawnPoints.TeamSpawnA, spwnPoint, point);
        }

        public void SetTeamBSpawnPoint(int spwnPoint, Vector3 point)
        {
            SetTeamBSpawnPoint(spwnPoint, ref point, true);
        }

        public void SetTeamBSpawnPoint(int spwnPoint, ref Vector3 point, bool sendMessage)
        {

            drawTeamB[spwnPoint] = true;
            teamBSpwn[spwnPoint].Name = "TEAM B SPAWN " + (spwnPoint + 1);
            teamBSpwn[spwnPoint].color = Color.Blue;
            teamBSpwn[spwnPoint].Position = point;

            if (sendMessage)
                Packet.WriteSpawnPlaced(Me, SpawnPoints.TeamSpawnB, spwnPoint, point);
        }
        //

        //spawns
        public SpawnPoint[] spawns = new SpawnPoint[6];
        public bool[] drawSpawns = new bool[6] { false, false, false, false, false, false };
        public void SetSpawnPoint(int spawnPoint, Vector3 point)
        {
            SetSpawnPoint(spawnPoint, ref point, true);
        }

        public void SetSpawnPoint(int spawnPoint, ref Vector3 point, bool sendMessage)
        {
            drawSpawns[spawnPoint] = true;
            spawns[spawnPoint].Name = "SPAWN " + (spawnPoint + 1);
            spawns[spawnPoint].color = Color.Red;
            spawns[spawnPoint].Position = point;

            if (sendMessage)
                Packet.WriteSpawnPlaced(Me, SpawnPoints.Spawn, spawnPoint, point);
        }
        
        //zombie spawn
        public bool[] drawZombieSpawns = new bool[6] { false, false, false, false, false, false };
        public SpawnPoint[] zombieSpawns = new SpawnPoint[6];
        public void SetZombieSpawn(int spawnPoint, Vector3 point)
        {
            SetZombieSpawn(spawnPoint, ref point, true);
        }

        public void SetZombieSpawn(int spawnPoint, ref Vector3 point, bool sendMessage)
        {
            drawZombieSpawns[spawnPoint] = true;
            zombieSpawns[spawnPoint].Name = "ZOMBIE SPAWN " + (spawnPoint + 1);
            zombieSpawns[spawnPoint].color = new Color(64, 106, 63);
            zombieSpawns[spawnPoint].Position = point;

            if (sendMessage)
                Packet.WriteSpawnPlaced(Me, SpawnPoints.ZombieSpawn, spawnPoint, point);
        }

        public bool drawKing = false;
        public SpawnPoint kingPoint;
        public int range = 0;
        public void SetKingPoint(int range, Vector3 point)
        {
            SetKingPoint(range, ref point, true);
            if (KingPointSet != null)
                KingPointSet.Invoke(this, new EventArgs());
        }

        public event EventHandler KingPointSet;

        public void SetKingPoint(int range, ref Vector3 point, bool sendMessage)
        {
            drawKing = true;
            kingPoint.Name = "King Of The BEACH".ToUpper();
            kingPoint.color = Color.Red;
            kingPoint.Position = point;
            this.range = range;

            if (sendMessage)
                Packet.WriteSpawnPlaced(Me, SpawnPoints.King, range, point);
        }

        public List<Vector3> goldblocks = new List<Vector3>();

        public void Save(BinaryWriter bw, GameModes gameMode)
        {
            if (gameMode == GameModes.CustomTDM)
            {
                bw.Write(teamASpwn[0].Position.X);
                bw.Write(teamASpwn[0].Position.Y);
                bw.Write(teamASpwn[0].Position.Z);
                bw.Write(teamASpwn[1].Position.X);
                bw.Write(teamASpwn[1].Position.Y);
                bw.Write(teamASpwn[1].Position.Z);
                bw.Write(teamASpwn[2].Position.X);
                bw.Write(teamASpwn[2].Position.Y);
                bw.Write(teamASpwn[2].Position.Z);

                bw.Write(teamBSpwn[0].Position.X);
                bw.Write(teamBSpwn[0].Position.Y);
                bw.Write(teamBSpwn[0].Position.Z);
                bw.Write(teamBSpwn[1].Position.X);
                bw.Write(teamBSpwn[1].Position.Y);
                bw.Write(teamBSpwn[1].Position.Z);
                bw.Write(teamBSpwn[2].Position.X);
                bw.Write(teamBSpwn[2].Position.Y);
                bw.Write(teamBSpwn[2].Position.Z);
            }
            else if (gameMode == GameModes.CustomFFA || gameMode == GameModes.CustomKB)
            {
                bw.Write(spawns[0].Position.X);
                bw.Write(spawns[0].Position.Y);
                bw.Write(spawns[0].Position.Z);
                bw.Write(spawns[1].Position.X);
                bw.Write(spawns[1].Position.Y);
                bw.Write(spawns[1].Position.Z);
                bw.Write(spawns[2].Position.X);
                bw.Write(spawns[2].Position.Y);
                bw.Write(spawns[2].Position.Z);
                bw.Write(spawns[3].Position.X);
                bw.Write(spawns[3].Position.Y);
                bw.Write(spawns[3].Position.Z);
                bw.Write(spawns[4].Position.X);
                bw.Write(spawns[4].Position.Y);
                bw.Write(spawns[4].Position.Z);
                bw.Write(spawns[5].Position.X);
                bw.Write(spawns[5].Position.Y);
                bw.Write(spawns[5].Position.Z);

                if (gameMode == GameModes.CustomKB)
                {
                    bw.Write(ref kingPoint.Position);
                    bw.Write(range);
                }
            }
            else if (gameMode == GameModes.CustomSNM)
            {
                bw.Write(teamASpwn[0].Position.X);
                bw.Write(teamASpwn[0].Position.Y);
                bw.Write(teamASpwn[0].Position.Z);
                bw.Write(teamASpwn[1].Position.X);
                bw.Write(teamASpwn[1].Position.Y);
                bw.Write(teamASpwn[1].Position.Z);
                bw.Write(teamASpwn[2].Position.X);
                bw.Write(teamASpwn[2].Position.Y);
                bw.Write(teamASpwn[2].Position.Z);

                bw.Write(teamBSpwn[0].Position.X);
                bw.Write(teamBSpwn[0].Position.Y);
                bw.Write(teamBSpwn[0].Position.Z);
                bw.Write(teamBSpwn[1].Position.X);
                bw.Write(teamBSpwn[1].Position.Y);
                bw.Write(teamBSpwn[1].Position.Z);
                bw.Write(teamBSpwn[2].Position.X);
                bw.Write(teamBSpwn[2].Position.Y);
                bw.Write(teamBSpwn[2].Position.Z);

                bw.Write(goldblocks.Count);
                for (int i = 0; i < goldblocks.Count; i++)
                {
                    bw.Write(goldblocks[i].X);
                    bw.Write(goldblocks[i].Y);
                    bw.Write(goldblocks[i].Z);
                }
            }
            else if (gameMode == GameModes.CustomSM)
            {
                bw.Write(spawns[0].Position.X);
                bw.Write(spawns[0].Position.Y);
                bw.Write(spawns[0].Position.Z);

                bw.Write(zombieSpawns[0].Position.X);
                bw.Write(zombieSpawns[0].Position.Y);
                bw.Write(zombieSpawns[0].Position.Z);
                bw.Write(zombieSpawns[1].Position.X);
                bw.Write(zombieSpawns[1].Position.Y);
                bw.Write(zombieSpawns[1].Position.Z);
                bw.Write(zombieSpawns[2].Position.X);
                bw.Write(zombieSpawns[2].Position.Y);
                bw.Write(zombieSpawns[2].Position.Z);
                bw.Write(zombieSpawns[3].Position.X);
                bw.Write(zombieSpawns[3].Position.Y);
                bw.Write(zombieSpawns[3].Position.Z);
                bw.Write(zombieSpawns[4].Position.X);
                bw.Write(zombieSpawns[4].Position.Y);
                bw.Write(zombieSpawns[4].Position.Z);
                bw.Write(zombieSpawns[5].Position.X);
                bw.Write(zombieSpawns[5].Position.Y);
                bw.Write(zombieSpawns[5].Position.Z);

                bw.Write(goldblocks.Count);
                for (int i = 0; i < goldblocks.Count; i++)
                {
                    bw.Write(goldblocks[i].X);
                    bw.Write(goldblocks[i].Y);
                    bw.Write(goldblocks[i].Z);
                }
            }

        }

        public void Load(BinaryReader br, GameModes gameMode, GraphicsDevice gd)
        {
            if (gameMode == GameModes.CustomTDM)
            {
                teamASpwn[0] = new SpawnPoint(gd, "TEAM A SPAWN 1");
                teamASpwn[1] = new SpawnPoint(gd, "TEAM A SPAWN 2");
                teamASpwn[2] = new SpawnPoint(gd, "TEAM A SPAWN 3");

                teamBSpwn[0] = new SpawnPoint(gd, "TEAM B SPAWN 1");
                teamBSpwn[1] = new SpawnPoint(gd, "TEAM B SPAWN 2");
                teamBSpwn[2] = new SpawnPoint(gd, "TEAM B SPAWN 3");

                teamASpwn[0].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                teamASpwn[1].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                teamASpwn[2].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                teamBSpwn[0].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                teamBSpwn[1].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                teamBSpwn[2].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                teamASpwn[0].color = Color.Red;
                teamASpwn[1].color = Color.Red;
                teamASpwn[2].color = Color.Red;

                teamBSpwn[0].color = Color.Blue;
                teamBSpwn[1].color = Color.Blue;
                teamBSpwn[2].color = Color.Blue;

                drawTeamA[0] = teamASpwn[0].Position == Vector3.Zero ? false : true;
                drawTeamA[1] = teamASpwn[1].Position == Vector3.Zero ? false : true;
                drawTeamA[2] = teamASpwn[2].Position == Vector3.Zero ? false : true;

                drawTeamB[0] = teamBSpwn[0].Position == Vector3.Zero ? false : true;
                drawTeamB[1] = teamBSpwn[1].Position == Vector3.Zero ? false : true;
                drawTeamB[2] = teamBSpwn[2].Position == Vector3.Zero ? false : true;
            }
            else if (gameMode == GameModes.CustomFFA || gameMode == GameModes.CustomKB)
            {
                spawns[0] = new SpawnPoint(gd, "SPAWN 1");
                spawns[1] = new SpawnPoint(gd, "SPAWN 2");
                spawns[2] = new SpawnPoint(gd, "SPAWN 3");
                spawns[3] = new SpawnPoint(gd, "SPAWN 4");
                spawns[4] = new SpawnPoint(gd, "SPAWN 5");
                spawns[5] = new SpawnPoint(gd, "SPAWN 6");

                spawns[0].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                spawns[1].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                spawns[2].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                spawns[3].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                spawns[4].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                spawns[5].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                spawns[0].color = Color.Red;
                spawns[1].color = Color.Red; 
                spawns[2].color = Color.Red;
                spawns[3].color = Color.Red;
                spawns[4].color = Color.Red;
                spawns[5].color = Color.Red;

                drawSpawns[0] = spawns[0].Position == Vector3.Zero ? false : true;
                drawSpawns[1] = spawns[1].Position == Vector3.Zero ? false : true;
                drawSpawns[2] = spawns[2].Position == Vector3.Zero ? false : true;
                drawSpawns[3] = spawns[3].Position == Vector3.Zero ? false : true;
                drawSpawns[4] = spawns[4].Position == Vector3.Zero ? false : true;
                drawSpawns[5] = spawns[5].Position == Vector3.Zero ? false : true;

                if (gameMode == GameModes.CustomKB)
                {
                    kingPoint = new SpawnPoint(gd, "KING OF THE BEACH");
                    kingPoint.Position = br.ReadVector3();
                    kingPoint.color = Color.Red;
                    drawKing = kingPoint.Position == Vector3.Zero ? false : true;
                    range = br.ReadInt32();
                }
            }
            else if (gameMode == GameModes.CustomSNM)
            {
                teamASpwn[0] = new SpawnPoint(gd, "TEAM A SPAWN 1");
                teamASpwn[1] = new SpawnPoint(gd, "TEAM A SPAWN 2");
                teamASpwn[2] = new SpawnPoint(gd, "TEAM A SPAWN 3");

                teamBSpwn[0] = new SpawnPoint(gd, "TEAM B SPAWN 1");
                teamBSpwn[1] = new SpawnPoint(gd, "TEAM B SPAWN 2");
                teamBSpwn[2] = new SpawnPoint(gd, "TEAM B SPAWN 3");

                teamASpwn[0].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                teamASpwn[1].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                teamASpwn[2].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                teamBSpwn[0].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                teamBSpwn[1].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                teamBSpwn[2].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                teamASpwn[0].color = Color.Red;
                teamASpwn[1].color = Color.Red;
                teamASpwn[2].color = Color.Red;

                teamBSpwn[0].color = Color.Blue;
                teamBSpwn[1].color = Color.Blue;
                teamBSpwn[2].color = Color.Blue;

                drawTeamA[0] = teamASpwn[0].Position == Vector3.Zero ? false : true;
                drawTeamA[1] = teamASpwn[1].Position == Vector3.Zero ? false : true;
                drawTeamA[2] = teamASpwn[2].Position == Vector3.Zero ? false : true;

                drawTeamB[0] = teamBSpwn[0].Position == Vector3.Zero ? false : true;
                drawTeamB[1] = teamBSpwn[1].Position == Vector3.Zero ? false : true;
                drawTeamB[2] = teamBSpwn[2].Position == Vector3.Zero ? false : true;

                int count = br.ReadInt32();
                goldblocks = new List<Vector3>();
                for (int i = 0; i < count; i++)
                {
                    goldblocks.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                }
            }
            else if (gameMode == GameModes.CustomSM)
            {
                spawns[0] = new SpawnPoint(gd, "SPAWN 1");
                spawns[0].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                spawns[0].color = Color.Red;
                drawSpawns[0] = spawns[0].Position == Vector3.Zero ? false : true;

                zombieSpawns[0] = new SpawnPoint(gd, "ZOMBIE SPAWN 1");
                zombieSpawns[1] = new SpawnPoint(gd, "ZOMBIE SPAWN 2");
                zombieSpawns[2] = new SpawnPoint(gd, "ZOMBIE SPAWN 3");
                zombieSpawns[3] = new SpawnPoint(gd, "ZOMBIE SPAWN 4");
                zombieSpawns[4] = new SpawnPoint(gd, "ZOMBIE SPAWN 5");
                zombieSpawns[5] = new SpawnPoint(gd, "ZOMBIE SPAWN 6");

                zombieSpawns[0].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                zombieSpawns[1].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                zombieSpawns[2].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                zombieSpawns[3].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                zombieSpawns[4].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                zombieSpawns[5].Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                zombieSpawns[0].color = new Color(64, 106, 63);
                zombieSpawns[1].color = new Color(64, 106, 63);
                zombieSpawns[2].color = new Color(64, 106, 63);
                zombieSpawns[3].color = new Color(64, 106, 63);
                zombieSpawns[4].color = new Color(64, 106, 63);
                zombieSpawns[5].color = new Color(64, 106, 63);

                drawZombieSpawns[0] = zombieSpawns[0].Position == Vector3.Zero ? false : true;
                drawZombieSpawns[1] = zombieSpawns[1].Position == Vector3.Zero ? false : true;
                drawZombieSpawns[2] = zombieSpawns[2].Position == Vector3.Zero ? false : true;
                drawZombieSpawns[3] = zombieSpawns[3].Position == Vector3.Zero ? false : true;
                drawZombieSpawns[4] = zombieSpawns[4].Position == Vector3.Zero ? false : true;
                drawZombieSpawns[5] = zombieSpawns[5].Position == Vector3.Zero ? false : true;

                int count = br.ReadInt32();
                goldblocks = new List<Vector3>();
                for (int i = 0; i < count; i++)
                {
                    goldblocks.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                }
            }
        }

        public void Render(GameModes gameMode, SpriteBatch SB, Camera camera)
        {
            if (gameMode == GameModes.CustomSNM || gameMode == GameModes.CustomTDM)
            {
                //draw team spawns
                for (int i = 0; i < 3; i++)
                {
                    if (drawTeamA[i])
                        teamASpwn[i].Draw(camera, SB);
                    if (drawTeamB[i])
                        teamBSpwn[i].Draw(camera, SB);
                }
            }

            if (gameMode == GameModes.CustomFFA || gameMode == GameModes.CustomKB)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (drawSpawns[i])
                        spawns[i].Draw(camera, SB);
                }
            }

            if (gameMode == GameModes.CustomSM)
            {
                if (drawSpawns[0])
                    spawns[0].Draw(camera, SB);

                for (int i = 0; i < 6; i++)
                {
                    if (drawZombieSpawns[i])
                        zombieSpawns[i].Draw(camera, SB);
                }
            }

            if (gameMode == GameModes.CustomKB)
                if (drawKing)
                    kingPoint.Draw(camera, SB);
        }

        public void Dispose()
        {
            try
            {
                KingPointSet -= we.KingPointSet;
            }
            catch { }
        }
    }

    public class WorldEditor : IGameScreen, ITerrainOwner
    {
        private Terrain terrain;
        private Random random;
        private LightingManager lightingManager;
        private LiquidManager liquidManager;
        private PlayerEditor player;
        public enum WorldGeneration { Flat, FlatWithCaves, Random, Island }
        public Random GetRandom { get { return random; } }
        public LightingManager GetLightingManager { get { return lightingManager; } }
        public LiquidManager GetLiquidManager { get { return liquidManager; } }
        public Terrain.UnderLiquid GetUnderLiquid { get { return player.underLiquid; } }
        private SpriteBatch SB;
        public Terrain GetTerrain { get { return terrain; } }
        public EditorWeaponDrop editorWeaponDrop;
        private RangeMenu rm;

        public Dictionary<byte, PlayerEditor> players;
        public Dictionary<byte, PlayerBody> playerBodies;
        public Dictionary<byte, bool> whoIsDone;
        protected Dictionary<byte, MovementPacketState[]> movementPacketStates;
        protected Dictionary<byte, RollingAverage> rollingAvgs;
        protected List<Packet.BlockChange> changes;
        public bool isGenerated = false;
        protected bool everyOnesDone = false;
        public LocalNetworkGamer Me;
        public EditorInfoScreen InfoScreen;

        public void Dispose()
        {
            try
            {
                MinerOfDuty.Session.GamerLeft -= Session_GamerLeft;
            }
            catch { }

            try
            {
                MinerOfDuty.Session.GamerJoined += Session_GamerJoined;
            }
            catch { }

            terrain.Dispose();
            lightingManager.Dispose();
            liquidManager.Dispose();
            var keys = playerBodies.Keys.ToArray();
            foreach (var key in keys)
                playerBodies[key].Dispose();
            keys = players.Keys.ToArray();
            foreach (var key in keys)
                players[key].armAnimation.Dispose();
            gameInfo.Dispose();
        }

        private InEditorMenu inEditorMenu;

        public string mapName = "";
        private int seed;
        public int size;
        public string password;
        private WorldGeneration worldGen;
        public string filename = ":(";
        public string teamAName, teamBName;
        public GameModes gameMode;
        public bool trees, weapons, editing;

        private int dot, dotDelay;
        public WorldEditorGameModeInfo gameInfo;

        private FPSCamera useThisCam;
        public void KingPointSet(object sender2, EventArgs e)
        {
            useThisCam = new FPSCamera(MinerOfDuty.Self.GraphicsDevice, new Vector3(64, 128, 64), Vector3.Forward);
            Vector3 v = new Vector3(64, 134, -21.5f);
            useThisCam.Update(.00001f, -(float)Math.PI / 1.4f, ref v);
            rm = new RangeMenu(new RangeMenu.RangeSet(delegate(RangeMenu sender)
                {
                    //sender.Value
                    gameInfo.SetKingPoint(rm.Value, gameInfo.kingPoint.Position);
                    rm = null;
                    useThisCam = null;
                    Resources.BlockEffect.Parameters["FogSwitch"].SetValue(1);
                    
                }), terrain.Size);
        }

        public bool AreGoldBlocksSet()
        {
            if (gameMode == GameModes.CustomFFA || gameMode == GameModes.CustomTDM || gameMode == GameModes.CustomKB)
                return true;



            if (gameInfo.goldblocks.Count < (gameMode == GameModes.CustomSM ? 15 : 4))
            {
                return false;
            }
            return true;
        }

        public bool IsKingSet()
        {
            if (gameMode == GameModes.CustomKB)
                return gameInfo.drawKing;
            else
                return true;
        }

        public bool AreSpawnsSet(out string[] failReason)
        {
            failReason = new string[] { "", "", };

            bool isATeamSet = false;
            bool isBTeamSet = false;

            for (int i = 0; i < 3; i++)
            {
                if (gameInfo.drawTeamA[i])
                    isATeamSet = true;
            }

            for (int i = 0; i < 3; i++)
            {
                if (gameInfo.drawTeamB[i])
                    isBTeamSet = true;
            }

            bool isSpawnSet = false;

            for (int i = 0; i < 6; i++)
            {
                if (gameInfo.drawSpawns[i])
                    isSpawnSet = true;
            }

            if(gameMode == GameModes.CustomTDM || gameMode == GameModes.CustomSNM)
                if ((isATeamSet && isBTeamSet) == false)
                {
                    failReason = new string[] { "BOTH TEAM SPAWNS MUST BE", "PLACE BEFORE YOU CAN SAVE." };
                    return false;
                }

            if (gameMode == GameModes.CustomSM || gameMode == GameModes.CustomFFA || gameMode == GameModes.CustomKB)
            {
                if (isSpawnSet == false)
                {
                    failReason = new string[] { "A SPAWN MUST BE PLACED", "BEFORE YOU CAN SAVE." };
                    return false;
                }
            }


            if (gameMode == GameModes.CustomSM)
            {
                bool zombieSet = false;

                for (int i = 0; i < 6; i++)
                {
                    if (gameInfo.drawZombieSpawns[i])
                        zombieSet = true;
                }

                if (zombieSet == false)
                {
                    failReason = new string[] { "A ZOMBIE SPAWN MUST BE", "PLACED BEFORE YOU CAN SAVE" };
                    return false;
                }
            }

            return true;
        }

        public bool CanPlaceBlock(Vector3i pos)
        {
            if (gameMode == GameModes.CustomTDM || gameMode == GameModes.CustomSNM)
            {
                for (int i = 0; i < 3; i++)
                {

                    for (int x = pos.X - 2; x <= pos.X + 2; x++)
                    {
                        for (int z = pos.Z - 2; z <= pos.Z + 2; z++)
                        {
                            if ((gameInfo.teamASpwn[i].Position.X == x && gameInfo.teamASpwn[i].Position.Y == pos.Y && gameInfo.teamASpwn[i].Position.Z == z)
                                ||
                                (gameInfo.teamBSpwn[i].Position.X == x && gameInfo.teamBSpwn[i].Position.Y == pos.Y && gameInfo.teamBSpwn[i].Position.Z == z))
                                return false;
                        }
                    }

                    for (int x = pos.X - 2; x <= pos.X + 2; x++)
                    {
                        for (int z = pos.Z - 2; z <= pos.Z + 2; z++)
                        {
                            if ((gameInfo.teamASpwn[i].Position.X == x && gameInfo.teamASpwn[i].Position.Y == pos.Y - 1 && gameInfo.teamASpwn[i].Position.Z == z)
                                 ||
                                 (gameInfo.teamBSpwn[i].Position.X == x && gameInfo.teamBSpwn[i].Position.Y == pos.Y - 1 && gameInfo.teamBSpwn[i].Position.Z == z))
                                return false;
                        }
                    }

                }
            }
            else if (gameMode == GameModes.CustomFFA || gameMode == GameModes.CustomSM || gameMode == GameModes.CustomKB)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (gameInfo.spawns[i] == null)
                        continue;

                    for (int x = pos.X - 2; x <= pos.X + 2; x++)
                    {
                        for (int z = pos.Z - 2; z <= pos.Z + 2; z++)
                        {
                            if ((gameInfo.spawns[i].Position.X == x && gameInfo.spawns[i].Position.Y == pos.Y && gameInfo.spawns[i].Position.Z == z))
                                return false;
                        }
                    }

                    for (int x = pos.X - 2; x <= pos.X + 2; x++)
                    {
                        for (int z = pos.Z - 2; z <= pos.Z + 2; z++)
                        {
                            if ((gameInfo.spawns[i].Position.X == x && gameInfo.spawns[i].Position.Y == pos.Y - 1 && gameInfo.spawns[i].Position.Z == z))
                                return false;
                        }
                    }
                }
            }


            return true;
        }

        private bool doneInit = false;
        private bool showMenu = false;


        private void Back(object sender)
        {
            showMenu = false;
        }

        private Queue<FooPacket> fooPackets;
        private MapReceiver mr;
        private MapSender mapSender;
        public WorldEditor(int NadajustneededAnOverload)
        {
            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                MinerOfDuty.Session.AllGamers[i].Tag = new GamePlayerStats();
            }
            Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);
            Packet.WriteIJoinedSess(MinerOfDuty.Session.LocalGamers[0]);
            Packet.WritePlayerBodyLooksPacket(MinerOfDuty.Session.LocalGamers[0], MinerOfDuty.CurrentPlayerProfile);

            InfoScreen = new EditorInfoScreen();

            playerBodies = new Dictionary<byte, PlayerBody>();
            fooPackets = new Queue<FooPacket>();
            //should add events here
            justGotHere = true;
            mr = new MapReceiver();

            MinerOfDuty.Session.GamerLeft += Session_GamerLeft;

        }

        void Session_GamerLeft(object sender, GamerLeftEventArgs e)
        {
            if (InfoScreen != null)
                InfoScreen.AddKillText(new InfoText(e.Gamer.Gamertag + " Left"));
        }

        private bool justGotHere = false;
        private void OtherPacketHandling(NetworkGamer sender, byte PacketID)
        {
            switch (PacketID)
            {
                case Packet.PACKETID_INSESSIONIJOINED:
                    //makes a stream of data to send un compressed to save memory in  an ew thread
                    Thread t = new Thread(delegate()
                        {
                            MemoryStream ms = CreateSave();
                            mapSender = new MapSender(ms);
                            mapSender.StartWritePacketTo(sender);
                        });
                    t.IsBackground = true;
                    t.Name = "Joiner data";
                    t.Start();
                    break;
                case Packet.PACKETID_MAPDATAREQUEST:
                    if (mapSender != null)
                    {
                        mapSender.PacketRequest(sender);
                    }
                    else
                    {
                        t = new Thread(delegate()
                        {
                            MemoryStream ms = CreateSave();
                            mapSender = new MapSender(ms);
                            mapSender.PacketRequest(sender);
                        });
                        t.IsBackground = true;
                        t.Name = "Joiner data";
                        t.Start();
                    }
                    break;
            }
        }


        private void Session_GamerJoined(object sender, GamerJoinedEventArgs e)
        {

            if(InfoScreen != null && MinerOfDuty.Session.SessionType == NetworkSessionType.PlayerMatch)
                InfoScreen.AddKillText(new InfoText(e.Gamer.Gamertag + " Joined"));

            if (players.ContainsKey(e.Gamer.Id) == false)
            {
                e.Gamer.Tag = new GamePlayerStats();

                players.Add(e.Gamer.Id, new PlayerEditor(this, new Vector3(-20, -64, -20), MinerOfDuty.Self.GraphicsDevice, size));/*, MinerOfDuty.Session.AllGamers[i].Id));*/
                movementPacketStates.Add(e.Gamer.Id, new MovementPacketState[]{ 
                    new MovementPacketState(players[e.Gamer.Id].position, Vector2.Zero, 0, 0,0),
                    new MovementPacketState(players[e.Gamer.Id].position, Vector2.Zero, 0, 0,0)});
                rollingAvgs[e.Gamer.Id] = new Networking.RollingAverage();

                playerBodies.Add(e.Gamer.Id, new PlayerBody(MinerOfDuty.Self.GraphicsDevice, e.Gamer.Id, e.Gamer.Gamertag, Color.Gray));
                playerBodies[e.Gamer.Id].CreateParts(Color.Black, Color.Black, Color.Wheat, Color.Wheat, Color.Wheat);
            }

            if (e.Gamer.IsLocal == false)
            {
                //send him our levels, clan tag, playerboides
                Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);
                Packet.WritePlayerBodyLooksPacket(Me, MinerOfDuty.CurrentPlayerProfile, e.Gamer);
            }
        }

        public WorldEditor(GraphicsDevice gd, WorldGeneration worldGen, string password, int? size, GameModes? gameMode, string teamAName, string teamBName, bool? trees, bool? weapons, bool? editing)
        {
            this.password = password;
            this.worldGen = worldGen;

            InfoScreen = new EditorInfoScreen();
            Me = MinerOfDuty.Session.LocalGamers[0];
            if (MinerOfDuty.Session.Host == Me)
            {
                Packet.WriteSeedPacket(MinerOfDuty.Session.LocalGamers[0], new Random().Next(), size.Value, trees.Value);
                Packet.WriteCustomMapInfo(Me, gameMode.Value, teamAName, teamBName, trees.Value, weapons.Value, editing.Value);
            }

            if (password != null)
            {
                Packet.WritePassword(Me, password);
            }

            if (Me.IsHost)
                MinerOfDuty.Session.AllowHostMigration = true;

            whoIsDone = new Dictionary<byte, bool>();


            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                whoIsDone.Add(MinerOfDuty.Session.AllGamers[i].Id, false);
            }

            players = new Dictionary<byte, PlayerEditor>();
            playerBodies = new Dictionary<byte, PlayerBody>();
            movementPacketStates = new Dictionary<byte, MovementPacketState[]>();
            rollingAvgs = new Dictionary<byte, RollingAverage>();
            changes = new List<Networking.Packet.BlockChange>();

            MinerOfDuty.Session.GamerLeft += Session_GamerLeft;
        }

        private int lagoff = 0;
        private void Initialize(GraphicsDevice gd, WorldGeneration worldGen, int seed, int size, bool gentree)
        {

            try
            {
#if XBOX
    Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif


                random = new Random(seed);
                terrain = new Terrain(this);
                Liquid.game = null;
                lightingManager = new LightingManager(terrain, gd);
                liquidManager = new LiquidManager(this, gd);

                SetUpPlayers(MinerOfDuty.Self.GraphicsDevice);
                //player = new PlayerEditor(this, new Vector3(20, 64, 20), gd);
                player.Camera = new FPSCamera(gd, new Vector3(20, 64, 20), Vector3.Forward);
                //put this in thread
                if (worldGen == WorldGeneration.Flat)
                    terrain.Initialize(gd, Terrain.CreateBlocksFlat, size, gentree);
                else if (worldGen == WorldGeneration.FlatWithCaves)
                    terrain.Initialize(gd, Terrain.CreateBlocksFlatCaves, size, gentree);
                else if (worldGen == WorldGeneration.Random)
                    terrain.Initialize(gd, Terrain.CreateBlocksRandom, size, gentree);
                else if (worldGen == WorldGeneration.Island)
                    terrain.Initialize(gd, Terrain.CreateBlocksIsland, size, gentree);
                liquidManager.Start();
                lightingManager.LightWorld();

                gameInfo = new WorldEditorGameModeInfo(this, Me);
                gameInfo.Init(gameMode, gd);

                SB = new SpriteBatch(gd);

                editorWeaponDrop = new EditorWeaponDrop();

                inEditorMenu = new InEditorMenu(Back, this);

                Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);

                System.GC.Collect();

                doneInit = true;
                isGenerated = true;

                try
                {
                    MinerOfDuty.Session.AllowJoinInProgress = true;
                }
                catch { }
                MinerOfDuty.Session.GamerJoined += (Session_GamerJoined);

                lagoff = 1000;

                Audio.Stop();
                Audio.SetPlaylist(new Playlist(new byte[] { Audio.SONG_PULSEROCK, Audio.SONG_RAW }));
                Audio.PlaySong();
                Audio.SetFading(Audio.Fading.In);
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        public WorldEditor()
        {
            Me = MinerOfDuty.Session.LocalGamers[0];
            InfoScreen = new EditorInfoScreen();
            whoIsDone = new Dictionary<byte, bool>();

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                whoIsDone.Add(MinerOfDuty.Session.AllGamers[i].Id, false);
            }

            players = new Dictionary<byte, PlayerEditor>();
            playerBodies = new Dictionary<byte, PlayerBody>();
            movementPacketStates = new Dictionary<byte, MovementPacketState[]>();
            rollingAvgs = new Dictionary<byte, RollingAverage>();
            changes = new List<Networking.Packet.BlockChange>();

            MinerOfDuty.Session.GamerLeft += Session_GamerLeft;
        }

        public void Initialize(GraphicsDevice gd, Stream s)
        {

#if XBOX
    Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif

            MemoryStream gameStream = new MemoryStream();

            MemoryStream input = new MemoryStream();
            s.ReadByte();
            s.ReadByte();
            s.ReadByte();
            s.ReadByte();
            byte[] buffer = new byte[s.Length - 4];
            s.Read(buffer, 0, buffer.Length);

            input.Write(buffer, 0, buffer.Length);
            input.Position = 0;
            LZF.lzf.Decompress(input, gameStream);

            s.Dispose();
            input.Dispose();

            gameStream.Position = 0;
            BinaryReader br = new BinaryReader(gameStream);

            gameInfo = new WorldEditorGameModeInfo(this, Me);

            MapGameInfoLoader data = MapGameInfoLoader.Load(br, gameInfo, gd);

            mapName = data.MapName;
            password = data.Password;
            worldGen = data.WorldGeneration;
            size = data.Size;
            teamAName = data.TeamAName;
            teamBName = data.TeamBName;
            gameMode = data.GameMode;
            trees = data.GenerateTrees;
            weapons = data.WeaponsEnabled;
            editing = data.EditingEnabled;

            random = new Random(data.Seed);
            this.seed = data.Seed; 
            terrain = new Terrain(this);
            Liquid.game = null;
            lightingManager = new LightingManager(terrain, gd);
            liquidManager = new LiquidManager(this, gd);
            //player = new PlayerEditor(this, data.EditorPlayerPosition, gd);
            SetUpPlayers(MinerOfDuty.Self.GraphicsDevice, data.EditorPlayerPosition);
            player.Camera = new FPSCamera(gd, data.EditorPlayerPosition, Vector3.Forward);

            editorWeaponDrop = new EditorWeaponDrop();

            terrain.CreateBlocksFromSave(gd, worldGen, br, size, data.GenerateTrees, data.Version);

            if(data.Version >= 2)
                editorWeaponDrop.LoadSpawners(br);

            br.Close();
            br.Dispose();
            br = null;

            liquidManager.Start();
            lightingManager.LightWorld();


            SB = new SpriteBatch(gd);

            inEditorMenu = new InEditorMenu(Back, this);

            System.GC.Collect();


            Packet.WriteDoneGeneratingWorldPacket(Me, MinerOfDuty.CurrentPlayerProfile);

            isGenerated = true;


            doneInit = true;


            try
            {
                MinerOfDuty.Session.AllowJoinInProgress = true;
            }
            catch { }
            MinerOfDuty.Session.GamerJoined += (Session_GamerJoined);

            lagoff = 1000;

            Audio.Stop();
            Audio.SetPlaylist(new Playlist(new byte[] { Audio.SONG_PULSEROCK, Audio.SONG_RAW }));
            Audio.PlaySong();
            Audio.SetFading(Audio.Fading.In);
        }

        /// <summary>
        /// use with people who have joined after the match has started
        /// </summary>
        /// <param name="s"></param>
        public void Initialize(Stream s)
        {
#if XBOX
    Thread.CurrentThread.SetProcessorAffinity(new int[] { 4 });
#endif
            Me = MinerOfDuty.Session.LocalGamers[0];

            players = new Dictionary<byte, PlayerEditor>();
            movementPacketStates = new Dictionary<byte, MovementPacketState[]>();
            rollingAvgs = new Dictionary<byte, RollingAverage>();
            changes = new List<Networking.Packet.BlockChange>();

            s.Position = 0;
            BinaryReader br = new BinaryReader(s);
            
            GraphicsDevice gd = MinerOfDuty.Self.GraphicsDevice;

            gameInfo = new WorldEditorGameModeInfo(this, Me);

            MapGameInfoLoader data = MapGameInfoLoader.Load(br, gameInfo, gd);

            mapName = data.MapName;
            password = data.Password;
            worldGen = data.WorldGeneration;
            size = data.Size;
            teamAName = data.TeamAName;
            teamBName = data.TeamBName;
            gameMode = data.GameMode;
            trees = data.GenerateTrees;
            weapons = data.WeaponsEnabled;
            editing = data.EditingEnabled;

            random = new Random(data.Seed);
            this.seed = data.Seed;
            terrain = new Terrain(this);
            Liquid.game = null;
            lightingManager = new LightingManager(terrain, gd);
            liquidManager = new LiquidManager(this, gd);
            //player = new PlayerEditor(this, data.EditorPlayerPosition, gd);
            SetUpPlayers(MinerOfDuty.Self.GraphicsDevice, data.EditorPlayerPosition);
            player.Camera = new FPSCamera(gd, data.EditorPlayerPosition, Vector3.Forward);


            editorWeaponDrop = new EditorWeaponDrop();

            terrain.CreateBlocksFromSave(gd, worldGen, br, size, data.GenerateTrees, data.Version);

            if (data.Version >= 2)
                editorWeaponDrop.LoadSpawners(br);

            br.Close();
            br.Dispose();
            br = null;

            liquidManager.Start();
            lightingManager.LightWorld();


            SB = new SpriteBatch(gd);

            inEditorMenu = new InEditorMenu(Back, this);

            while (fooPackets.Count > 0)
            {
                FooPacket p = fooPackets.Dequeue();
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

                                    terrain.blockBox.Max = changes2[i].Position + Block.halfVector;
                                    terrain.blockBox.Min = changes2[i].Position - Block.halfVector;

                                    bool hit = playerBodies[Me.Id].CheckForIntersection(ref terrain.blockBox);

                                    if (hit)
                                        BlockChanged(ref changes2[i].Position, Block.BLOCKID_AIR, false);
                                    else
                                    {
                                        terrain.AddBlocks((int)changes2[i].Position.X, (int)changes2[i].Position.Y, (int)changes2[i].Position.Z, changes2[i].ID);
                                        if (changes2[i].ID == Block.BLOCKID_GOLD)
                                        {
                                            if (gameMode == GameModes.CustomSM || gameMode == GameModes.CustomSNM)
                                                gameInfo.goldblocks.Add(changes2[i].Position);
                                        }
                                    }

                                    terrain.blockBox.Max = Block.halfVector;
                                    terrain.blockBox.Min = -Block.halfVector;

                                }
                                else if (changes2[i].ID == Block.BLOCKID_WATER)
                                {
                                    liquidManager.AddSourceWaterBlock((int)changes2[i].Position.X, (int)changes2[i].Position.Y,
                                        (int)changes2[i].Position.Z);
                                }
                                else
                                {
                                    liquidManager.AddSourceLavaBlock((int)changes2[i].Position.X, (int)changes2[i].Position.Y,
                                       (int)changes2[i].Position.Z);
                                }
                            }
                            else
                            {
                                terrain.RemoveBlocks((int)changes2[i].Position.X, (int)changes2[i].Position.Y, (int)changes2[i].Position.Z);

                                if (gameMode == GameModes.CustomSM || gameMode == GameModes.CustomSNM)
                                    if (gameInfo.goldblocks.Contains(changes2[i].Position))
                                        gameInfo.goldblocks.Remove(changes2[i].Position);

                            }
                        }
                        #endregion
                        break;
                    case Packet.PACKETID_SPAWNPLACED:
                        Vector3 position;
                        int spawnNumber;
                        WorldEditorGameModeInfo.SpawnPoints sp;

                        Packet.ReadSpawnPlaced(out sp, out spawnNumber, out position, PacketReader);

                        if (sp == WorldEditorGameModeInfo.SpawnPoints.TeamSpawnA)
                            gameInfo.SetTeamASpawnPoint(spawnNumber, ref position, false);
                        else if (sp == WorldEditorGameModeInfo.SpawnPoints.TeamSpawnB)
                            gameInfo.SetTeamBSpawnPoint(spawnNumber, ref position, false);
                        else if (sp == WorldEditorGameModeInfo.SpawnPoints.Spawn)
                            gameInfo.SetSpawnPoint(spawnNumber, ref position, false);
                        else if (sp == WorldEditorGameModeInfo.SpawnPoints.ZombieSpawn)
                            gameInfo.SetZombieSpawn(spawnNumber, ref  position, false);
                        else if (sp == WorldEditorGameModeInfo.SpawnPoints.King)
                            gameInfo.SetKingPoint(spawnNumber, ref position, false);

                        break;
                    case Packet.PACKETID_WEAPONSPAWNERCHANGED:
                        position = PacketReader.ReadVector3();
                        if (PacketReader.ReadBoolean())
                        {
                            InventoryItem weaponAdded = (InventoryItem)PacketReader.ReadByte();
                            editorWeaponDrop.AddSpawner(position, Inventory.InventoryItemToID(weaponAdded));
                        }
                        else
                        {
                            EditorWeaponDrop.WeaponPickupable wd;
                            if ((wd = editorWeaponDrop.CheckForPickup(ref position)) != null)
                            {
                                editorWeaponDrop.RemoveWeaponDrop(wd);
                            }
                        }
                        break;
                }
            }

            System.GC.Collect();

            everyOnesDone = true;
            isGenerated = true;
            justGotHere = false;
            doneInit = true;


            lagoff = 1000;


            try
            {
                MinerOfDuty.Session.AllowJoinInProgress = true;
            }
            catch { }
            MinerOfDuty.Session.GamerJoined += (Session_GamerJoined);
            
            Audio.Stop();
            Audio.SetPlaylist(new Playlist(new byte[] { Audio.SONG_PULSEROCK, Audio.SONG_RAW }));
            Audio.PlaySong();
            Audio.SetFading(Audio.Fading.In);
        }

        private TimeSpan WaitingForTimeSpan = new TimeSpan();
        private int frameCounter;
        public void Update(GameTime gameTime)
        {
            if (MinerOfDuty.Session == null || (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed))
            {
                return;
            }

            if (showErrorMilli > 0)
                showErrorMilli -= gameTime.ElapsedGameTime.Milliseconds;

            if (justGotHere)
            {
                
                    dotDelay += gameTime.ElapsedGameTime.Milliseconds;
                    if (dotDelay > 1000)
                    {
                        dot++;
                        dotDelay = 0;
                    }
                    if (dot == 4)
                        dot = 0;
                

                MinerOfDuty.Session.Update();

                if (MinerOfDuty.Session.AllGamers.Count < 2)
                {
                    MinerOfDuty.DrawMenu(true);
                    MessageBox.ShowMessageBox(delegate(int selected)
                    {
                        Audio.PlaySound(Audio.SOUND_UICLICK);
                    }, new string[] { "OKAY" }, 0, new string[] { "ERROR JOINING!" });
                    return;
                }

                while (MinerOfDuty.Session.LocalGamers[0].IsDataAvailable)
                {
                    byte packetid;
                    NetworkGamer sender;
                    MinerOfDuty.Session.LocalGamers[0].ReceiveData(Packet.PacketReader, out sender);
                    packetid = Packet.PacketReader.ReadByte();
                    switch (packetid)
                    {
                        case Packet.PACKETID_MAPDATA:
                            if (mr.PacketReceive(sender))
                            {
                                Thread t = new Thread(poop => Initialize(mr.GetMapData));
                                t.IsBackground = true;
                                t.Name = "Make map";
                                t.Start();
                            }
                            break;
                        case Packet.PACKETID_PLAYERLEVEL:
                            if (MinerOfDuty.Session.FindGamerById(sender.Id) != null)
                            {
                                Packet.ReadPlayerLevel(MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats);
                            }
                            break;
                        case Packet.PACKETID_PLAYERBODYLOOKS:
                            playerBodies.Add(sender.Id, new PlayerBody(MinerOfDuty.Self.GraphicsDevice, sender.Id, sender.Gamertag, Color.Green));
                            Packet.ReadPlayerBodyLooksPacket(playerBodies[sender.Id]);
                            break;
                        case Packet.PACKETID_BLOCKCHANGES:
                            fooPackets.Enqueue(new FooPacket(packetid, Packet.PacketReader.ReadBytes(Packet.PacketReader.Length - Packet.PacketReader.Position),sender.Id));
                            break;
                        case Packet.PACKETID_SPAWNPLACED:
                            fooPackets.Enqueue(new FooPacket(packetid, Packet.PacketReader.ReadBytes(Packet.PacketReader.Length - Packet.PacketReader.Position),sender.Id));
                            break;
                        case Packet.PACKETID_WEAPONSPAWNERCHANGED:
                            fooPackets.Enqueue(new FooPacket(packetid, Packet.PacketReader.ReadBytes(Packet.PacketReader.Length - Packet.PacketReader.Position),sender.Id));
                            break;
                    }
                }
                return;
            }

            if (lagoff > 0)
                lagoff -= gameTime.ElapsedGameTime.Milliseconds;

            if (isGenerated && everyOnesDone && lagoff <= 0)
            {
                if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.X))
                    useGoggles = !useGoggles;

                InfoScreen.Update(gameTime);

                lightingManager.SwitchTextures();

                terrain.Update();

                lightingManager.Update();

                editorWeaponDrop.Update(gameTime);
                if (rm != null)
                {
                    gameInfo.range = rm.Value;
                    rm.Update(gameTime);
                    player.DontUseInput();
                    Input.Flush();
                }
                else if (showMenu == false)
                {
                    if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Start))
                    {
                        showMenu = true;
                        inEditorMenu.SelectFirst();
                    }


                }
                else
                {
                    inEditorMenu.Update((short)gameTime.ElapsedGameTime.Milliseconds);

                    if (showMenu == false)
                    {
                        player.DontUseInput();
                        Input.Flush();
                    }
                }


                UpdatePlayers(gameTime);

                while (Me.IsDataAvailable)
                {
                    NetworkGamer sender;
                    Me.ReceiveData(Packet.PacketReader, out sender);
                    byte packetID = Packet.GetPacketID();
                    switch (packetID)
                    {
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
                            if (sender != Me)
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
                            #region code
                            Packet.BlockChange[] changes = Packet.ReadBlockPacket();
                            for (int i = 0; i < changes.Length; i++)
                            {
                                if (changes[i].Added)
                                {
                                    if (!Block.IsLiquid(changes[i].ID))
                                    {
                                        //check if can place

                                        terrain.blockBox.Max = changes[i].Position + Block.halfVector;
                                        terrain.blockBox.Min = changes[i].Position - Block.halfVector;

                                        bool hit = playerBodies[Me.Id].CheckForIntersection(ref terrain.blockBox);

                                        if (hit)
                                            BlockChanged(ref changes[i].Position, Block.BLOCKID_AIR, false);
                                        else
                                        {
                                            if (changes[i].ID == Block.BLOCKID_GOLD)
                                            {
                                                if (gameInfo.goldblocks.Count < 15)
                                                {
                                                    terrain.AddBlocks((int)changes[i].Position.X, (int)changes[i].Position.Y, (int)changes[i].Position.Z, changes[i].ID);
                                                    if (gameMode == GameModes.CustomSM || gameMode == GameModes.CustomSNM)
                                                        gameInfo.goldblocks.Add(changes[i].Position);
                                                }
                                                else if(sender.Id == Me.Id)
                                                {
                                                    //errpr
                                                    error = "CAN'T PLACE MORE THAN 15 GOLD BLOCKS";
                                                    showErrorMilli = 1000;

                                                }
                                            }
                                            else
                                                terrain.AddBlocks((int)changes[i].Position.X, (int)changes[i].Position.Y, (int)changes[i].Position.Z, changes[i].ID);
                                            
                                        }

                                        terrain.blockBox.Max = Block.halfVector;
                                        terrain.blockBox.Min = -Block.halfVector;

                                    }
                                    else if (changes[i].ID == Block.BLOCKID_WATER)
                                    {
                                        liquidManager.AddSourceWaterBlock((int)changes[i].Position.X, (int)changes[i].Position.Y,
                                            (int)changes[i].Position.Z);
                                    }
                                    else
                                    {
                                        liquidManager.AddSourceLavaBlock((int)changes[i].Position.X, (int)changes[i].Position.Y,
                                           (int)changes[i].Position.Z);
                                    }
                                }
                                else
                                {
                                    terrain.RemoveBlocks((int)changes[i].Position.X, (int)changes[i].Position.Y, (int)changes[i].Position.Z);
                                    if (gameMode == GameModes.CustomSM || gameMode == GameModes.CustomSNM)
                                        if (gameInfo.goldblocks.Contains(changes[i].Position))
                                            gameInfo.goldblocks.Remove(changes[i].Position);
                                }
                            }
#endregion
                            break;
                        case Packet.PACKETID_SPAWNPLACED:
                           Vector3 position;
                        int spawnNumber;
                        WorldEditorGameModeInfo.SpawnPoints sp;

                        Packet.ReadSpawnPlaced(out sp, out spawnNumber, out position);

                        if (sp == WorldEditorGameModeInfo.SpawnPoints.TeamSpawnA)
                            gameInfo.SetTeamASpawnPoint(spawnNumber, ref position, false);
                        else if (sp == WorldEditorGameModeInfo.SpawnPoints.TeamSpawnB)
                            gameInfo.SetTeamBSpawnPoint(spawnNumber, ref position, false);
                        else if (sp == WorldEditorGameModeInfo.SpawnPoints.Spawn)
                            gameInfo.SetSpawnPoint(spawnNumber, ref position, false);
                        else if (sp == WorldEditorGameModeInfo.SpawnPoints.ZombieSpawn)
                            gameInfo.SetZombieSpawn(spawnNumber, ref  position, false);
                        else if (sp == WorldEditorGameModeInfo.SpawnPoints.King)
                            gameInfo.SetKingPoint(spawnNumber, ref position, false);
                            break;
                        case Packet.PACKETID_WEAPONSPAWNERCHANGED:
                            position = Packet.PacketReader.ReadVector3();
                            if (Packet.PacketReader.ReadBoolean())
                            {
                                InventoryItem weaponAdded = (InventoryItem)Packet.PacketReader.ReadByte();
                                editorWeaponDrop.AddSpawner(position, Inventory.InventoryItemToID(weaponAdded));
                            }
                            else
                            {
                                EditorWeaponDrop.WeaponPickupable wd;
                                if ((wd = editorWeaponDrop.CheckForPickup(ref position)) != null)
                                {
                                    editorWeaponDrop.RemoveWeaponDrop(wd);
                                }
                            }
                            break;
                        case Packet.PACKETID_DONEGENERATINGWORLD:
                            whoIsDone[sender.Id] = true;
                            playerBodies.Add(sender.Id, new PlayerBody(MinerOfDuty.Self.GraphicsDevice, sender.Id, sender.Gamertag, Color.Green));
                            Packet.ReadDoneGeneratingWorldPacket(playerBodies[sender.Id]);
                            break;
                        case Packet.PACKETID_PLAYERBODYLOOKS:
                            if (playerBodies.ContainsKey(sender.Id))
                                playerBodies.Remove(sender.Id);
                            playerBodies.Add(sender.Id, new PlayerBody(MinerOfDuty.Self.GraphicsDevice, sender.Id, sender.Gamertag, Color.Green));
                            Packet.ReadPlayerBodyLooksPacket(playerBodies[sender.Id]);
                            break;
                        case Packet.PACKETID_EDITPERMISSIONCHANGE:
                            bool viewOnly;
                            byte playerID;
                            Packet.ReadPermissionPacket(out viewOnly, out playerID);

                            if(sender.IsLocal == false)
                                inEditorMenu.SetPerm(playerID, viewOnly);

                            if (Me.Id == playerID)
                            {
                                player.ViewOnly = viewOnly;
                            }

                            break;
                        default:
                            OtherPacketHandling(sender, packetID);
                            break;
                    }
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

            }
            else if (isGenerated == false || everyOnesDone == false)
            {
                while (Me.IsDataAvailable)
                {
                    NetworkGamer sender;
                    Me.ReceiveData(Packet.PacketReader, out sender);
                    byte packetID = Packet.GetPacketID();
                    switch (packetID)
                    {
                        case Packet.PACKETID_PLAYERLEVEL:
                            if (MinerOfDuty.Session.FindGamerById(sender.Id) != null)
                            {
                                Packet.ReadPlayerLevel(MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats);
                            }
                            break;
                        case Packet.PACKETID_MUTE:
                            MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(sender, Packet.PacketReader.ReadBoolean());
                            break;
                        case Packet.PACKETID_SEED:
                            int seed = Packet.GetSeed();
                            //this cant happen until seed is recived
                            this.seed = seed;
                            this.size = Packet.PacketReader.ReadInt32();
                            trees = Packet.PacketReader.ReadBoolean();
                            Thread t = new Thread(poop => Initialize(MinerOfDuty.Self.GraphicsDevice, worldGen, seed, this.size, trees));
                            t.Name = "gen";
                            t.IsBackground = true;
                            t.Start();
                            break;
                        case Packet.PACKETID_DONEGENERATINGWORLD:
                            whoIsDone[sender.Id] = true;
                            playerBodies.Add(sender.Id, new PlayerBody(MinerOfDuty.Self.GraphicsDevice, sender.Id, sender.Gamertag, Color.Green));
                            Packet.ReadDoneGeneratingWorldPacket(playerBodies[sender.Id]);
                            break;
                        case Packet.PACKETID_PASSWORD:
                            this.password = Packet.PacketReader.ReadString();
                            break;
                        case Packet.PACKETID_CUSTOMMAPINFO:
                            Packet.ReadCustomInfoPacket(out gameMode, out teamAName, out teamBName, out trees, out weapons, out editing);
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
                WaitingForTimeSpan = WaitingForTimeSpan.Add(gameTime.ElapsedGameTime);
                if (WaitingForTimeSpan.TotalSeconds > 15)
                {
                    MinerOfDuty.DrawMenu(true);
                    MessageBox.ShowMessageBox(delegate(int selected)
                    {
                        Audio.PlaySound(Audio.SOUND_UICLICK);
                    }, new string[] { "OK" }, 0, new string[] { "UNABLE TO START MATCH" });
                }
            }

            

            if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                MinerOfDuty.Session.Update();
        }

        private Camera backUp;
        private bool useGoggles  = false;
        public void Render(GraphicsDevice gd)
        {
            Resources.BlockEffect.Parameters["GrayAmount"].SetValue(0f);
            if (isGenerated && everyOnesDone)
            {
                lock (lightingManager.LightMapTextureLock)
                {
                    if (useGoggles)
                        Resources.BlockEffect.Parameters["Brightness"].SetValue(new Vector3(.2f, .3f, .2f));
                    else
                        Resources.BlockEffect.Parameters["Brightness"].SetValue(new Vector3(.0f, .0f, .0f));

                    
                    if (useThisCam != null)
                    {
                        backUp = player.Camera;
                        player.Camera = useThisCam as FPSCamera;
                        Resources.BlockEffect.Parameters["FogSwitch"].SetValue(0);
                    }

                    player.RenderArm();
                    terrain.Render(player.Camera);
                    liquidManager.Render(terrain.bf);
                    for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                    {
                        if (MinerOfDuty.Session.AllGamers[i].Id != Me.Id)
                        {
                            playerBodies[MinerOfDuty.Session.AllGamers[i].Id].Render(player.Camera);
                        }
                    }
                    editorWeaponDrop.Render(player.Camera);


                    terrain.RenderGlass(player.Camera);

                    gameInfo.Render(gameMode, SB, player.Camera);

                    if (gameInfo.drawKing)
                    {
                        //render the transparent part of it
                        Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["AreaWallTechnique"];
                        Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateScale(gameInfo.range, 40, gameInfo.range)
                            * Matrix.CreateTranslation(gameInfo.kingPoint.Position.X, gameInfo.kingPoint.Position.Y - 30, gameInfo.kingPoint.Position.Z));
                        float val = MathHelper.Lerp(.2f, 0, Vector3.DistanceSquared(useThisCam != null ? backUp.Position : player.Camera.Position, gameInfo.kingPoint.Position) / 64);
                        Resources.BlockEffect.Parameters["WallAlpha"].SetValue(Math.Max(val, 0f));
                        Resources.BlockEffect.Parameters["DiscardSolid"].SetValue(true);
                        Resources.BlockEffect.Parameters["DiscardAlpha"].SetValue(false);
                        gd.RasterizerState = RasterizerState.CullNone;
                        Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                        gd.DepthStencilState = DepthStencilState.DepthRead;

                        Resources.AreaWallThingModel.Meshes[0].Draw();

                        //set back the defaults

                        Resources.BlockEffect.Parameters["World"].SetValue(Matrix.Identity);
                        gd.DepthStencilState = DepthStencilState.Default;
                        gd.RasterizerState = RasterizerState.CullCounterClockwise;
                        Resources.BlockEffect.Parameters["DiscardSolid"].SetValue(false);
                        Resources.BlockEffect.Parameters["DiscardAlpha"].SetValue(false);
                        Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();

                    }
                    player.Render();

                    for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                    {
                        if (MinerOfDuty.Session.AllGamers[i].Id != Me.Id)
                            playerBodies[MinerOfDuty.Session.AllGamers[i].Id].RenderName(player.Camera, SB);
                    }

                    if (useThisCam != null)
                    {
                        player.Camera = backUp as FPSCamera;
                    }
                }
            }
        }

        private int showErrorMilli = 0;
        private string error = "";
        public void Draw(SpriteBatch sb)
        {
            if (isGenerated && everyOnesDone)
            {
                if (useThisCam == null)
                {
                    player.Draw(sb);

                    if (showErrorMilli > 0)
                        sb.DrawString(Resources.Font, error, new Vector2(640, 400), Color.Red, 0, Resources.Font.MeasureString(error) / 2f, 1, SpriteEffects.None, 0);

                    InfoScreen.Draw(sb);
                }
                if (rm != null)
                    rm.Draw(sb);
                if (showMenu)
                    inEditorMenu.Draw(sb);
            }
            else
            {
                sb.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, "INITIALIZING MAP" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""),
                    new Vector2(640 - (Resources.Font.MeasureString("INITIALIZING MAP").X / 2),
                        320 - (Resources.Font.LineSpacing / 2f)), Color.White);
            }
        }

        private void SetUpPlayers(GraphicsDevice gd, Vector3 pos)
        {
            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                players.Add(MinerOfDuty.Session.AllGamers[i].Id, new PlayerEditor(this, pos, gd, size));/*, MinerOfDuty.Session.AllGamers[i].Id));*/
                movementPacketStates.Add(MinerOfDuty.Session.AllGamers[i].Id, new MovementPacketState[]{ 
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0),
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0)});
                rollingAvgs[MinerOfDuty.Session.AllGamers[i].Id] = new Networking.RollingAverage();
            }

            player = players[Me.Id];

            player.Camera = new FPSCamera(gd, player.position, Vector3.Forward);
        }

        private void SetUpPlayers(GraphicsDevice gd)
        {
            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                players.Add(MinerOfDuty.Session.AllGamers[i].Id, new PlayerEditor(this, new Vector3(64, 64, 64), gd, size));/*, MinerOfDuty.Session.AllGamers[i].Id));*/
                movementPacketStates.Add(MinerOfDuty.Session.AllGamers[i].Id, new MovementPacketState[]{ 
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0),
                    new MovementPacketState(players[MinerOfDuty.Session.AllGamers[i].Id].position, Vector2.Zero, 0, 0,0)});
                rollingAvgs[MinerOfDuty.Session.AllGamers[i].Id] = new Networking.RollingAverage();
            }

            player = players[Me.Id];

            player.Camera = new FPSCamera(gd, player.position, Vector3.Forward);
        }

        private void UpdatePlayers(GameTime gameTime)
        {
            if (showMenu)
                player.Update(gameTime, Input.Empty);
            else
                player.Update(gameTime, Input.ControllingPlayerNewGamePadState);

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                if (MinerOfDuty.Session.AllGamers[i].Id != Me.Id)
                    MovementPacketState.InterpolatePlayer(gameTime,
                        ref movementPacketStates[MinerOfDuty.Session.AllGamers[i].Id][0],
                        ref movementPacketStates[MinerOfDuty.Session.AllGamers[i].Id][1],
                        players[MinerOfDuty.Session.AllGamers[i].Id]);

                playerBodies[MinerOfDuty.Session.AllGamers[i].Id].Update(gameTime, players[MinerOfDuty.Session.AllGamers[i].Id]);
            }
        }

        public void Save(Stream s)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {

                        bw.Write((short)5);//version

                        bw.Write(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag);
                        bw.Write(mapName);
                        bw.Write(DateTime.Now.Ticks);
                        //dont mess with the placement of these three 

                        bw.Write((byte)gameMode);
                        //and this one too

                        bw.Write(teamAName);
                        bw.Write(teamBName);
                        bw.Write(size);

                        bw.Write(seed);
                        bw.Write((byte)worldGen);
                        bw.Write(password != null);
                        if (password != null)
                            bw.Write(password);

                        gameInfo.Save(bw, gameMode);

                        bw.Write(player.position.X);
                        bw.Write(player.position.Y - (player.stance == Player.Stance.Prone ? (1.4f - .8f) : player.stance == Player.Stance.Crouching ? (1.4f - .475f) : 1.4f));
                        bw.Write(player.position.Z);

                        bw.Write(trees);
                        bw.Write(weapons);
                        bw.Write(editing);

                        for (int x = 0; x < 128; x++)
                        {
                            for (int z = 0; z < 128; z++)
                            {
                                int y = 63;
                                byte lastBlock;
                                while (true)
                                {
                                    int howMany = 0;
                                    lastBlock = (Block.IsLiquid(terrain.blocks[x, y, z]) && liquidManager.Liquids[x, y, z].liquidLevel != 10) ? Block.BLOCKID_AIR : terrain.blocks[x, y, z]; //we dont want liquids
                                    while (lastBlock == ((Block.IsLiquid(terrain.blocks[x, y, z]) && liquidManager.Liquids[x, y, z].liquidLevel != 10) ? Block.BLOCKID_AIR : terrain.blocks[x, y, z]))
                                    {
                                        howMany++;
                                        y--;
                                        if (y == -1)
                                            break;
                                        if (lastBlock !=
                                            ((Block.IsLiquid(terrain.blocks[x, y, z]) && liquidManager.Liquids[x, y, z].liquidLevel != 10)
                                            ? Block.BLOCKID_AIR : terrain.blocks[x, y, z]))
                                            break;
                                    }

                                    bw.Write(lastBlock);
                                    bw.Write((byte)howMany);

                                    if (y == -1)
                                        break;
                                }
                            }
                        }


                        editorWeaponDrop.SaveItems(bw);

                        ms.Position = 0;
                        System.GC.Collect();

                        s.Write(new byte[] { 6, 9, 69, 69 }, 0, 4);
                        Lzf.LZF.lzf.Compress(ms, s);
                    }
                }
                System.GC.Collect();
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        public MemoryStream CreateSave()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write((short)5);//version

            bw.Write(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag);
            bw.Write(mapName);
            bw.Write(DateTime.Now.Ticks);

            bw.Write((byte)gameMode);
            bw.Write(teamAName);
            bw.Write(teamBName);

            bw.Write(size);
            bw.Write(seed);
            bw.Write((byte)worldGen);
            bw.Write(password != null);
            if (password != null)
                bw.Write(password);

            gameInfo.Save(bw, gameMode);

            bw.Write(player.position.X);
            bw.Write(player.position.Y - (player.stance == Player.Stance.Prone ? (1.4f - .8f) : player.stance == Player.Stance.Crouching ? (1.4f - .475f) : 1.4f));
            bw.Write(player.position.Z);


            bw.Write(trees);
            bw.Write(weapons);
            bw.Write(editing);

            for (int x = 0; x < 128; x++)
            {
                for (int z = 0; z < 128; z++)
                {
                    int y = 63;
                    byte lastBlock;
                    while (true)
                    {
                        int howMany = 0;
                        lastBlock = (Block.IsLiquid(terrain.blocks[x, y, z]) && liquidManager.Liquids[x, y, z].liquidLevel != 10) ? Block.BLOCKID_AIR : terrain.blocks[x, y, z]; //we dont want liquids
                        while (lastBlock == ((Block.IsLiquid(terrain.blocks[x, y, z]) && liquidManager.Liquids[x, y, z].liquidLevel != 10) ? Block.BLOCKID_AIR : terrain.blocks[x, y, z]))
                        {
                            howMany++;
                            y--;
                            if (y == -1)
                                break;
                            if (lastBlock !=
                                ((Block.IsLiquid(terrain.blocks[x, y, z]) && liquidManager.Liquids[x, y, z].liquidLevel != 10)
                                ? Block.BLOCKID_AIR : terrain.blocks[x, y, z]))
                                break;
                        }

                        bw.Write(lastBlock);
                        bw.Write((byte)howMany);

                        if (y == -1)
                            break;
                    }
                }
            }

            editorWeaponDrop.SaveItems(bw);

            ms.Position = 0;
            System.GC.Collect();

            bw = null;
            return ms;
        }

        public bool BlockChanged(ref Vector3 pos, byte blockID, bool added)
        {
            changes.Add(new Networking.Packet.BlockChange(pos, blockID, added));
            return true;
        }


        public void Activated()
        {
            if (inEditorMenu != null)
            { 
                if (inEditorMenu.ShouldDeactiveate())
                {
                    showMenu = iWasShowing;
                }
            }
        }

        private bool iWasShowing;
        public void Deactivated()
        {
            if (inEditorMenu != null)
            {
                if (inEditorMenu.ShouldDeactiveate())
                {
                    iWasShowing = showMenu;
                    showMenu = true;
                    inEditorMenu.SelectFirst();
                }
            }
        }
    }
}
