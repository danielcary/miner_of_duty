using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using Miner_Of_Duty.LobbyCode;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Game.Editor
{
    public class MapGameInfoLoader
    {
        public int Version { get; set; }
        public string Author { get; private set; }
        public string MapName { get; private set; }
        public DateTime Time { get; private set; }
        public int Seed { get; private set; }
        public WorldEditor.WorldGeneration WorldGeneration { get; private set; }
        public string Password { get; private set; }
        public int Size { get; private set; }
        public GameModes GameMode { get; private set; }
        public string TeamAName { get; private set; }
        public string TeamBName { get; private set; }

        public bool GenerateTrees { get; private set; }
        public bool WeaponsEnabled { get; private set; }
        public bool EditingEnabled { get; private set; }

        //for TDM and SnM
        public Vector3[] TeamASpawnPosition { get; private set; }
        public Vector3[] TeamBSpawnPosition { get; private set; }

        //for FFA
        public Vector3[] SpawnPositions { get; private set; }

        //for Swarm
        public Vector3 SpawnPosition { get; private set; }
        public Vector3[] ZombieSpawnPositions { get; private set; }

        //for TDM and SnM
        public Vector3[] GoldBlockPostions { get; private set; }


        public Vector3 EditorPlayerPosition { get; private set; }

        /// <summary>
        /// The range of the beach
        /// </summary>
        public int Range { get; private set; }
        public Vector3 KingPoint { get; private set; }

        public static MapGameInfoLoader Load(BinaryReader br)
        {
            return Load(br, null, null);
        }

        public static MapGameInfoLoader Load(BinaryReader br, WorldEditorGameModeInfo gameInfo, GraphicsDevice gd)
        {
            MapGameInfoLoader val = new MapGameInfoLoader();

            val.Version = br.ReadInt16();//version

            val.Author = br.ReadString();//author
            val.MapName = br.ReadString();
            val.Time = new DateTime(br.ReadInt64());//time

            if (val.Version >= 3)
            {
                val.GameMode = (GameModes)br.ReadByte();
                val.TeamAName = br.ReadString();
                val.TeamBName = br.ReadString();
                val.Size = br.ReadInt32();
            }
            else
            {
                val.GameMode = GameModes.CustomTDM;
                val.TeamAName = "Team Silverback";
                val.TeamBName = "Team Wavves";
                val.Size = 128;
            }

            val.Seed = br.ReadInt32();
            val.WorldGeneration = (WorldEditor.WorldGeneration)br.ReadByte();

            if (val.Version >= 2)
            {
                if (br.ReadBoolean())
                    val.Password = br.ReadString();
                else
                    val.Password = null;
            }

            if (val.Version >= 3)
            {
                int bytesLoaded = 0;

                if (val.GameMode == GameModes.CustomTDM || val.GameMode == GameModes.CustomSNM)
                {
                    val.TeamASpawnPosition = new Vector3[3];
                    val.TeamBSpawnPosition = new Vector3[3];

                    val.TeamASpawnPosition[0] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.TeamASpawnPosition[1] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.TeamASpawnPosition[2] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                    val.TeamBSpawnPosition[0] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.TeamBSpawnPosition[1] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.TeamBSpawnPosition[2] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                    bytesLoaded += sizeof(float) * 6 * 3;
                }
                else if (val.GameMode == GameModes.CustomFFA || val.GameMode == GameModes.CustomKB)
                {
                    val.SpawnPositions = new Vector3[6];

                    val.SpawnPositions[0] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.SpawnPositions[1] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.SpawnPositions[2] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.SpawnPositions[3] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.SpawnPositions[4] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.SpawnPositions[5] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                    bytesLoaded += sizeof(float) * 6 * 3;

                    if (val.GameMode == GameModes.CustomKB)
                    {
                        val.KingPoint = br.ReadVector3();
                        val.Range = br.ReadInt32();
                        bytesLoaded += sizeof(float) * 3 + sizeof(int);
                    }
                }
                else if (val.GameMode == GameModes.CustomSM)
                {
                    val.SpawnPosition = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    bytesLoaded += sizeof(float) * 3;

                    val.ZombieSpawnPositions = new Vector3[6];

                    val.ZombieSpawnPositions[0] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.ZombieSpawnPositions[1] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.ZombieSpawnPositions[2] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.ZombieSpawnPositions[3] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.ZombieSpawnPositions[4] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    val.ZombieSpawnPositions[5] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                    bytesLoaded += sizeof(float) * 6 * 3;

                    int count = br.ReadInt32();
                    bytesLoaded += sizeof(int);

                    val.GoldBlockPostions = new Vector3[count];

                    for (int i = 0; i < count; i++)
                    {
                        val.GoldBlockPostions[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        bytesLoaded += sizeof(float) * 3;
                    }
                }

                if (val.GameMode == GameModes.CustomSNM)
                {
                    int count = br.ReadInt32();
                    bytesLoaded += sizeof(int);

                    val.GoldBlockPostions = new Vector3[count];

                    for (int i = 0; i < count; i++)
                    {
                        val.GoldBlockPostions[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        bytesLoaded += sizeof(float) * 3;
                    }
                }

                if (gameInfo != null)
                {
                    //hopefully this works
                    br.BaseStream.Position -= bytesLoaded;

                    gameInfo.Load(br, val.GameMode, gd);
                }
            }
            else
            {
                val.TeamASpawnPosition = new Vector3[3];
                val.TeamBSpawnPosition = new Vector3[3];

                val.TeamASpawnPosition[0] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                val.TeamASpawnPosition[1] = Vector3.Zero;
                val.TeamASpawnPosition[2] = Vector3.Zero;

                val.TeamBSpawnPosition[0] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                val.TeamBSpawnPosition[1] = Vector3.Zero;
                val.TeamBSpawnPosition[2] = Vector3.Zero;


                if (gameInfo != null)
                {
                    gameInfo.teamASpwn[0] = new SpawnPoint(gd, "TEAM A SPAWN 1");
                    gameInfo.teamASpwn[1] = new SpawnPoint(gd, "TEAM A SPAWN 2");
                    gameInfo.teamASpwn[2] = new SpawnPoint(gd, "TEAM A SPAWN 3");

                    gameInfo.teamBSpwn[0] = new SpawnPoint(gd, "TEAM B SPAWN 1");
                    gameInfo.teamBSpwn[1] = new SpawnPoint(gd, "TEAM B SPAWN 2");
                    gameInfo.teamBSpwn[2] = new SpawnPoint(gd, "TEAM B SPAWN 3");

                    gameInfo.teamASpwn[0].Position = val.TeamASpawnPosition[0];// new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    gameInfo.teamASpwn[1].Position = Vector3.Zero;
                    gameInfo.teamASpwn[2].Position = Vector3.Zero;

                    gameInfo.teamBSpwn[0].Position = val.TeamBSpawnPosition[0];//new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    gameInfo.teamBSpwn[1].Position = Vector3.Zero;
                    gameInfo.teamBSpwn[2].Position = Vector3.Zero;

                    gameInfo.teamASpwn[0].color = Color.Red;
                    gameInfo.teamASpwn[1].color = Color.Red;
                    gameInfo.teamASpwn[2].color = Color.Red;

                    gameInfo.teamBSpwn[0].color = Color.Blue;
                    gameInfo.teamBSpwn[1].color = Color.Blue;
                    gameInfo.teamBSpwn[2].color = Color.Blue;

                    gameInfo.drawTeamA[0] = true;
                    gameInfo.drawTeamA[1] = false;
                    gameInfo.drawTeamA[2] = false;

                    gameInfo.drawTeamB[0] = true;
                    gameInfo.drawTeamB[1] = false;
                    gameInfo.drawTeamB[2] = false;
                }
            }

            val.EditorPlayerPosition = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            if (val.Version >= 5)
            {
                val.GenerateTrees = br.ReadBoolean();
                val.WeaponsEnabled = br.ReadBoolean();
                val.EditingEnabled = br.ReadBoolean();
            }
            else
            {
                if (val.WorldGeneration == WorldEditor.WorldGeneration.Random ||
                    val.WorldGeneration == WorldEditor.WorldGeneration.Island)
                    val.GenerateTrees = true;
                else
                    val.GenerateTrees = false;

                val.WeaponsEnabled = true;
                val.EditingEnabled = true;
            }

            return val;
        }
    }
}
