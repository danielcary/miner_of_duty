using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Miner_Of_Duty.Game;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Miner_Of_Duty.LobbyCode;
using Microsoft.Xna.Framework.Net;

namespace Miner_Of_Duty
{
    public class PlayerProfile//make one for each gamer
    {
        public Color EyeColor;
        public Color HairColor;
        public Color SkinColor;
        public Color ShirtColor;
        public Color PantsColor;

        public PlayerProfile()
        {
            EyeColor = Color.Blue;
            SkinColor = Color.SandyBrown;
            HairColor = Color.LightYellow;
            ShirtColor = Color.Red;
            PantsColor = Color.Blue;

            Slot1 = CharacterClass.DEFAULT;
            Slot2 = CharacterClass.DEFAULT2;
            Slot3 = CharacterClass.DEFAULT3;
            Slot4 = CharacterClass.DEFAULT4;
            Slot5 = CharacterClass.DEFAULT5;

            Slot1.name = "CUSTOM CLASS 1";
            Slot2.name = "CUSTOM CLASS 2";
            Slot3.name = "CUSTOM CLASS 3";
            Slot4.name = "CUSTOM CLASS 4";
            Slot5.name = "CUSTOM CLASS 5";

            Level = 1;
            XP = 0;
            Clan = "";

            MessagesToRead = new Queue<string[]>();
            
        }

        //include number of kills, heads, deaths
        public int Kills { get; private set; }

        public int Headshots;
        public int RevengeKills;
        public int GrenadeKills;
        public int KnifeKills;

        public int Deaths { get; private set; }
        public int GravityDeaths;
        public int LavaDeaths;

        public int Wins { get; private set; }
        public int TDMWins;
        public int SNMWins;
        public int FFAWins;
        public int FWWins;
        public int KBWins;

        public int Defeats { get; private set; }
        public int TDMDefeats;
        public int SNMDefeats;
        public int FFADefeats;
        public int FWDefeats;
        public int KBDefeats;

        public int BlocksMined;
        public int Ties;

        public GunStats[] gunStats = new GunStats[GunType.GunTypes.Length];

        public struct GunStats
        {
            public uint Kills;
            public uint Fired;
            public uint Hits;
            public float Accuracy { get { return Fired == 0 ? 0 : (float)Hits / (float)Fired; } }

            public void Save(BinaryWriter bw)
            {
                bw.Write(Kills);
                bw.Write(Fired);
                bw.Write(Hits);
            }

            public void Load(BinaryReader br)
            {
                Kills = br.ReadUInt32();
                Fired = br.ReadUInt32();
                Hits = br.ReadUInt32();
            }
        }


        public void WriteToPacket(PacketWriter pw)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(Kills);

                    bw.Write(Headshots);
                    bw.Write(RevengeKills);
                    bw.Write(GrenadeKills);
                    bw.Write(KnifeKills);

                    bw.Write(Deaths);

                    bw.Write(GravityDeaths);
                    bw.Write(LavaDeaths);

                    bw.Write(Wins);

                    bw.Write(TDMWins);
                    bw.Write(SNMWins);
                    bw.Write(FFAWins);
                    bw.Write(FWWins);
                    bw.Write(KBWins);

                    bw.Write(Defeats);

                    bw.Write(TDMDefeats);
                    bw.Write(SNMDefeats);
                    bw.Write(FFADefeats);
                    bw.Write(FWDefeats);
                    bw.Write(KBDefeats);

                    bw.Write(Ties);

                    bw.Write(XP);
                    bw.Write(Level);

                    for (int i = 0; i < gunStats.Length; i++)
                        gunStats[i].Save(bw);

                }
                pw.Write(ms.ToArray());
            }
        }

        private static GunStats[] ReadGSFromPacket(PacketReader pr)
        {
            GunStats[] val = new GunStats[MinerOfDuty.CurrentPlayerProfile.gunStats.Length];
            for (int i = 0; i < val.Length; i++)
            {
                val[i].Load(pr);
            }
            return val;
        }

        public static PlayerProfile ReadFromPacket(PacketReader pr)
        {
            return new PlayerProfile()
            {
                Kills = pr.ReadInt32(),
                Headshots = pr.ReadInt32(),
                RevengeKills = pr.ReadInt32(),
                GrenadeKills = pr.ReadInt32(),
                KnifeKills = pr.ReadInt32(),

                Deaths = pr.ReadInt32(),

                GravityDeaths = pr.ReadInt32(),
                LavaDeaths = pr.ReadInt32(),

                Wins = pr.ReadInt32(),

                TDMWins = pr.ReadInt32(),
                SNMWins = pr.ReadInt32(),
                FFAWins = pr.ReadInt32(),
                FWWins = pr.ReadInt32(),
                KBWins = pr.ReadInt32(),

                Defeats = pr.ReadInt32(),

                TDMDefeats = pr.ReadInt32(),
                SNMDefeats = pr.ReadInt32(),
                FFADefeats = pr.ReadInt32(),
                FWDefeats = pr.ReadInt32(),
                KBDefeats = pr.ReadInt32(),

                Ties = pr.ReadInt32(),

                XP = pr.ReadInt32(),
                Level = pr.ReadInt32(),

                gunStats = ReadGSFromPacket(pr),
            };
        }

        public string Clan = "";

        public void Save()
        {
            MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
        }

        public delegate void LevelUp();
        public event LevelUp LevelUpEvent;

        public int XP { get; private set; }
        public int XPTillLevel { get { return (Level * 500) + 500; } }
        private void AddXP(int amount)
        {
            if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false
                && MinerOfDuty.Session.SessionProperties[7].HasValue)
                return;

            XP += amount;

            if(Level < 50)
                if (XP >= XPTillLevel)
                {
                    Level++;

                    if (LevelUpEvent != null)
                        LevelUpEvent.Invoke();

                    if (Level == PITFALL)
                        MessagesToRead.Enqueue(new string[] { "PITFALL BLOCK UNLOCKED" });
                    if (Level == M16)
                        MessagesToRead.Enqueue(new string[] { "M16 UNLOCKED" });
                    if (Level == M16BALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "M16 BALLISTIC TIP", "UNLOCKED" });
                    if (Level == M16MOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "M16 MORE AMMO", "UNLOCKED" });
                    if (Level == M16EXTENDEDMAGS)
                        MessagesToRead.Enqueue(new string[] { "M16 EXTENDED MAGS", "UNLOCKED" });
                    if (Level == AK47)
                        MessagesToRead.Enqueue(new string[] { "AK-47 UNLOCKED" });
                    if (Level == AK47BALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "AK-47 BALLISTIC TIP", "UNLOCKED" });
                    if (Level == AK47EXTENDEDMAGS)
                        MessagesToRead.Enqueue(new string[] { "AK-47 EXTENDED MAGS", "UNLOCKED" });
                    if (Level == AK47MOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "AK-47 MORE AMMO", "UNLOCKED" });
                    if (Level == FAL)
                        MessagesToRead.Enqueue(new string[] { "FAL UNLOCKED" });
                    if (Level == FALBALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "FAL BALLISTIC TIP", "UNLOCKED" });
                    if (Level == FALMOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "FAL MORE AMMO", "UNLOCKED" });
                    if (Level == FALEXTENDEDMAGS)
                        MessagesToRead.Enqueue(new string[] { "FAL EXTENDED MAGS", "UNLOCKED" });
                    if (Level == COLT45)
                        MessagesToRead.Enqueue(new string[] { "COLT .45 UNLOCKED" });
                    if (Level == COLT45BALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "COLT .45 BALLISTIC TIP", "UNLOCKED" });
                    if (Level == COLT45MOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "COLT .45 MORE AMMO", "UNLOCKED" });
                    if (Level == MAGNUM)
                        MessagesToRead.Enqueue(new string[] { ".357 MAGNUM UNLOCKED" });
                    if (Level == MAGNUMBALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { ".357 MAGNUM BALLISTIC TIP", "UNLOCKED" });
                    if (Level == MAGNUMMOREAMMO)
                        MessagesToRead.Enqueue(new string[] { ".357 MAGNUM MORE AMMO", "UNLOCKED" });
                    if (Level == DOUBLEBARREL)
                        MessagesToRead.Enqueue(new string[] { "DOUBLE BARREL UNLOCKED" });
                    if (Level == DOUBLEBARRELBALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "DOUBLE BARREL BALLISTIC TIP", "UNLOCKED" });
                    if (Level == DOUBLEBARRELMOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "DOUBLE BARREL MORE AMMO", "UNLOCKED" });
                    if (Level == AA12)
                        MessagesToRead.Enqueue(new string[] { "AA-12 UNLOCKED" });
                    if (Level == AA12BALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "AA-12 BALLISTIC TIP", "UNLOCKED" });
                    if (Level == AA12MOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "AA-12 MORE AMMO", "UNLOCKED" });
                    if (Level == _12GUAGE)
                        MessagesToRead.Enqueue(new string[] { "12 GAUGE UNLOCKED" });
                    if (Level == _12GUAGEBALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "12 GAUGE BALLISTIC TIP", "UNLOCKED" });
                    if (Level == _12GUAGEMOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "12 GAUGE MORE AMMO", "UNLOCKED" });
                    if (Level == MP5K)
                        MessagesToRead.Enqueue(new string[] { "MP5K UNLOCKED" });
                    if (Level == MP5KBALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "MP5K BALLISTIC TIP", "UNLOCKED" });
                    if (Level == MP5KMOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "MP5K MORE AMMO", "UNLOCKED" });
                    if (Level == MP5KEXTENDEDMAGS)
                        MessagesToRead.Enqueue(new string[] { "MP5K EXTENDED MAGS", "UNLOCKED" });
                    if (Level == UMP45)
                        MessagesToRead.Enqueue(new string[] { "UMP45 UNLOCKED" });
                    if (Level == UMP45BALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "UMP45 BALLISTIC TIP", "UNLOCKED" });
                    if (Level == UMP45MOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "UMP45 MORE AMMO", "UNLOCKED" });
                    if (Level == UMP45EXTENDEDMAGS)
                        MessagesToRead.Enqueue(new string[] { "UMP45 EXTENDED MAGS", "UNLOCKED" });
                    if (Level == VECTOR)
                        MessagesToRead.Enqueue(new string[] { "VECTOR UNLOCKED" });
                    if (Level == VECTORBALLISTICTIP)
                        MessagesToRead.Enqueue(new string[] { "VECTOR BALLISTIC TIP", "UNLOCKED" });
                    if (Level == VECTOREXTENDEDMAGS)
                        MessagesToRead.Enqueue(new string[] { "VECTOR EXTENDED MAGS", "UNLOCKED" });
                    if (Level == VECTORMOREAMMO)
                        MessagesToRead.Enqueue(new string[] { "VECTOR MORE AMMO", "UNLOCKED" });
                    if (Level == ENDURANCE)
                        MessagesToRead.Enqueue(new string[] { "ENDURANCE UNLOCKED" });
                    if (Level == THICKSKIN)
                        MessagesToRead.Enqueue(new string[] { "THICK SKIN UNLOCKED" });
                    if (Level == QUICKHANDS)
                        MessagesToRead.Enqueue(new string[] { "QUICK HANDS UNLOCKED" });
                    if (Level == SHOVELBRONZE)
                        MessagesToRead.Enqueue(new string[] { "ROCK SHOVEL UNLOCKED" });
                    if (Level == SHOVELSTEEL)
                        MessagesToRead.Enqueue(new string[] { "STEEL SHOVEL", "UNLOCKED" });
                    if (Level == SHOVELDIAMOND)
                        MessagesToRead.Enqueue(new string[] { "DIAMOND SHOVEL", "UNLOCKED" });
                    if (Level == PICKBRONZE)
                        MessagesToRead.Enqueue(new string[] { "ROCK PICK UNLOCKED" });
                    if (Level == PICKSTEEL)
                        MessagesToRead.Enqueue(new string[] { "STEEL PICK UNLOCKED" });
                    if (Level == PICKDIAMOND)
                        MessagesToRead.Enqueue(new string[] { "DIAMOND PICK UNLOCKED" });
                    if (Level == SHARPEREDGES)
                        MessagesToRead.Enqueue(new string[] { "SHARPER EDGES UNLOCKED" });
                    if (Level == DURABLECONSTRUCTION)
                        MessagesToRead.Enqueue(new string[] { "HARDY MATERIALS", "UNLOCKED" });
                    if (Level == LAVABUCKET)
                        MessagesToRead.Enqueue(new string[] { "LAVA BUCKET UNLOCKED" });


                    XP = 0;
                }
            Save();
        }
        public int Level { get; private set; }

        public void AddVictory(GameModes gameMode, int xp, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;

            Wins++;
            if (gameMode == GameModes.FortWars)
                FWWins++;
            else if (gameMode == GameModes.FreeForAll)
                FFAWins++;
            else if (gameMode == GameModes.SearchNMine)
                SNMWins++;
            else if (gameMode == GameModes.TeamDeathMatch)
                TDMWins++;
            else if (gameMode == GameModes.KingOfTheBeach)
                KBWins++;

            AddXP(xp + toAddToScore);
            toAddToScore = 0;
        }

        public void AddDefeat(GameModes gameMode, int xp, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;

            Defeats++;
            if (gameMode == GameModes.FortWars)
                FWDefeats++;
            else if (gameMode == GameModes.FreeForAll)
                FFADefeats++;
            else if (gameMode == GameModes.SearchNMine)
                SNMDefeats++;
            else if (gameMode == GameModes.TeamDeathMatch)
                TDMDefeats++;
            else if (gameMode == GameModes.KingOfTheBeach)
                KBDefeats++;

            AddXP(xp + toAddToScore);
            toAddToScore = 0;
        }

        public void AddBlockMined(GameModes gameMode, int xp, bool privateMatch)
        {
            BlocksMined++;
            toAddToScore += xp;
        }

        public void AddTie(GameModes gameMode, int xp, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;

            Ties++;

            AddXP(xp + toAddToScore);
            toAddToScore = 0;
        }


        private int toAddToScore = 0;
        /// <summary>
        /// Use kill to add to the main Kill number, use others to add to special categories (excluding Normal)
        /// </summary>
        public enum KillType { Kill, Normal, Headshot, Revenge, Knife, Grenade }
        public void AddKill(GameModes gameMode, KillType kill, int xp, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;

            if (kill == KillType.Kill)
                Kills++;
            else if (kill == KillType.Normal)
                toAddToScore += xp;
            else
            {
                toAddToScore += xp;

                if (kill == KillType.Grenade)
                    GrenadeKills++;
                else if (kill == KillType.Headshot)
                    Headshots++;
                else if (kill == KillType.Revenge)
                    RevengeKills++;
                else if (kill == KillType.Knife)
                    KnifeKills++;

            }
        }

        public void AddKingScore(GameModes gameMode, int xp, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;

            toAddToScore += xp;
        }

        public void AddDeath(GameModes gameMode, KillText.DeathType kill, int xp, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;


            toAddToScore += xp;
            Deaths++;

            if (kill == KillText.DeathType.Fall)
                GravityDeaths++;
            else if (kill == KillText.DeathType.Lava)
                LavaDeaths++;

        }

        public void AddBulletFired(GameModes gameMode, byte gunID, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;

            gunStats[gunID].Fired++;

        }

        public void AddGunHit(GameModes gameMode, byte gunID, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;

            gunStats[gunID].Hits++;
        }

        public void AddGunKill(GameModes gameMode, byte gunID, bool privateMatch)
        {
            if (privateMatch)
                return;

            if (gameMode == GameModes.CustomFFA ||
                gameMode == GameModes.CustomSM ||
                gameMode == GameModes.CustomSNM ||
                gameMode == GameModes.CustomTDM ||
                gameMode == GameModes.SwarmMode)
                return;

            gunStats[gunID].Kills++;
        }


        public Queue<string[]> MessagesToRead;
        public bool HasSword = false;
        public static CharacterClass SMG = new CharacterClass(
                new WeaponSlot(GunType.GUNID_MP5K, false, false, false),
                new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
                new ItemSlot(InventoryItem.DirtBlock),
                new ItemSlot(InventoryItem.EmptyBucket));

        public static CharacterClass AssaultRifle = new CharacterClass(
                new WeaponSlot(GunType.GUNID_FAL, false, false, false),
                new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
                new ItemSlot(InventoryItem.StoneBlock),
                new ItemSlot(InventoryItem.EmptyBucket));

        public static CharacterClass Shotgun = new CharacterClass(
                new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false),
                new WeaponSlot(GunType.GUNID_COLT45, false, false, false),
                new ItemSlot(InventoryItem.SandBlock),
                new ItemSlot(InventoryItem.EmptyBucket));

        public static CharacterClass Builder = new CharacterClass(
                new ToolSlot(ToolType.TOOLID_ROCKSHOVEL, false, false),
                new ToolSlot(ToolType.TOOLID_ROCKPICK, false, false),
                new ItemSlot(InventoryItem.DirtBlock),
                new ItemSlot(InventoryItem.StoneBlock));

        public CharacterClass Slot1;
        public CharacterClass Slot2;
        public CharacterClass Slot3;
        public CharacterClass Slot4;
        public CharacterClass Slot5;

        public static void LoadPlayerProfile(Stream stream)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    int version = br.ReadInt16();//versiom

                    if (version <= 2)
                    {

                        MinerOfDuty.CurrentPlayerProfile.EyeColor = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), 255);
                        MinerOfDuty.CurrentPlayerProfile.HairColor = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), 255);
                        MinerOfDuty.CurrentPlayerProfile.SkinColor = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), 255);
                        MinerOfDuty.CurrentPlayerProfile.ShirtColor = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), 255);
                        MinerOfDuty.CurrentPlayerProfile.PantsColor = new Color(br.ReadByte(), br.ReadByte(), br.ReadByte(), 255);

                        MinerOfDuty.CurrentPlayerProfile.Kills = br.ReadInt32();
                        MinerOfDuty.CurrentPlayerProfile.Deaths = br.ReadInt32();
                        MinerOfDuty.CurrentPlayerProfile.Wins = br.ReadInt32();
                        MinerOfDuty.CurrentPlayerProfile.Defeats = br.ReadInt32();
                        MinerOfDuty.CurrentPlayerProfile.XP = br.ReadInt32();
                        MinerOfDuty.CurrentPlayerProfile.Level = br.ReadInt32();

                        try
                        {
                            if (SpecialGamer.IsDev(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer]))
                                MinerOfDuty.CurrentPlayerProfile.Level = 50;
                        }
                        catch (Exception) { }

                        if (version >= 2)
                            MinerOfDuty.CurrentPlayerProfile.Clan = br.ReadString();
                        else
                            MinerOfDuty.CurrentPlayerProfile.Clan = "";

                        MinerOfDuty.CurrentPlayerProfile.Slot1 = CharacterClass.LoadClass(br);
                        MinerOfDuty.CurrentPlayerProfile.Slot2 = CharacterClass.LoadClass(br);
                        MinerOfDuty.CurrentPlayerProfile.Slot3 = CharacterClass.LoadClass(br);
                        MinerOfDuty.CurrentPlayerProfile.Slot4 = CharacterClass.LoadClass(br);
                        MinerOfDuty.CurrentPlayerProfile.Slot5 = CharacterClass.LoadClass(br);
                    }
                    else
                    {
                        MemoryStream filiteredStream = new MemoryStream();
                        while (br.BaseStream.Position < br.BaseStream.Length)
                        {
                            br.ReadByte();
                            filiteredStream.WriteByte(br.ReadByte());
                        }

                        filiteredStream.Position = 0;
                        using (BinaryReader newBR = new BinaryReader(filiteredStream))
                        {
                            MinerOfDuty.CurrentPlayerProfile.EyeColor = new Color(newBR.ReadByte(), newBR.ReadByte(), newBR.ReadByte(), 255);
                            MinerOfDuty.CurrentPlayerProfile.HairColor = new Color(newBR.ReadByte(), newBR.ReadByte(), newBR.ReadByte(), 255);
                            MinerOfDuty.CurrentPlayerProfile.SkinColor = new Color(newBR.ReadByte(), newBR.ReadByte(), newBR.ReadByte(), 255);
                            MinerOfDuty.CurrentPlayerProfile.ShirtColor = new Color(newBR.ReadByte(), newBR.ReadByte(), newBR.ReadByte(), 255);
                            MinerOfDuty.CurrentPlayerProfile.PantsColor = new Color(newBR.ReadByte(), newBR.ReadByte(), newBR.ReadByte(), 255);

                            if (version >= 4)
                            {
                                MinerOfDuty.CurrentPlayerProfile.Kills = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.Headshots = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.RevengeKills = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.GrenadeKills = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.KnifeKills = newBR.ReadInt32();

                                MinerOfDuty.CurrentPlayerProfile.Deaths = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.GravityDeaths = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.LavaDeaths = newBR.ReadInt32();

                                MinerOfDuty.CurrentPlayerProfile.Wins = newBR.ReadInt32();

                                MinerOfDuty.CurrentPlayerProfile.TDMWins = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.SNMWins = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.FFAWins = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.FWWins = newBR.ReadInt32();
                                if (version >= 5)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.KBWins = newBR.ReadInt32();
                                }

                                MinerOfDuty.CurrentPlayerProfile.Defeats = newBR.ReadInt32();

                                MinerOfDuty.CurrentPlayerProfile.TDMDefeats = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.SNMDefeats = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.FFADefeats = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.FWDefeats = newBR.ReadInt32();
                                if (version >= 5)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.KBDefeats = newBR.ReadInt32();
                                }

                                MinerOfDuty.CurrentPlayerProfile.Ties = newBR.ReadInt32();

                                for (int i = 0; i < MinerOfDuty.CurrentPlayerProfile.gunStats.Length; i++)
                                    MinerOfDuty.CurrentPlayerProfile.gunStats[i].Load(newBR);
                            }
                            else
                            {
                                MinerOfDuty.CurrentPlayerProfile.Kills = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.Deaths = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.Wins = newBR.ReadInt32();
                                MinerOfDuty.CurrentPlayerProfile.Defeats = newBR.ReadInt32();
                            }

                            MinerOfDuty.CurrentPlayerProfile.XP = newBR.ReadInt32();
                            MinerOfDuty.CurrentPlayerProfile.Level = newBR.ReadInt32();

                            try
                            {
                                if (SpecialGamer.IsDev(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer]))
                                    MinerOfDuty.CurrentPlayerProfile.Level = 50;
                            }
                            catch (Exception) { }

                            if (version >= 2)
                                MinerOfDuty.CurrentPlayerProfile.Clan = newBR.ReadString();
                            else
                                MinerOfDuty.CurrentPlayerProfile.Clan = "";

                            MinerOfDuty.CurrentPlayerProfile.Slot1 = CharacterClass.LoadClass(newBR);
                            MinerOfDuty.CurrentPlayerProfile.Slot2 = CharacterClass.LoadClass(newBR);
                            MinerOfDuty.CurrentPlayerProfile.Slot3 = CharacterClass.LoadClass(newBR);
                            MinerOfDuty.CurrentPlayerProfile.Slot4 = CharacterClass.LoadClass(newBR);
                            MinerOfDuty.CurrentPlayerProfile.Slot5 = CharacterClass.LoadClass(newBR);
                        }
                    }

                    MinerOfDuty.CurrentPlayerProfile.MessagesToRead = new Queue<string[]>();

                    if (MinerOfDuty.CurrentPlayerProfile.LevelUpEvent != null)
                        MinerOfDuty.CurrentPlayerProfile.LevelUpEvent.Invoke();
                }

                if (PlayerProfileReloadedEvent != null)
                    PlayerProfileReloadedEvent.Invoke();
            }
            catch (EndOfStreamException e)
            {
                MessageBox.ShowMessageBox(
                    delegate(int selected)
                    {
                        if (selected == 0)
                        {
                            MinerOfDuty.CurrentPlayerProfile = new PlayerProfile();
                        }
                        else
                        {
                            lock (MinerOfDuty.ExceptionsLock)
                                MinerOfDuty.Exceptions.Enqueue(e);
                        }
                    }, new string[] { "YES", "NO" }, 1, new string[] { "PROFILE WAS CORRUPTED,", "DELETE IT?" });
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        public void Save(Stream stream)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                   // bw.Write((short)3);//v2

                    bw.Write(EyeColor.R);
                    bw.Write(EyeColor.G);
                    bw.Write(EyeColor.B);

                    bw.Write(HairColor.R);
                    bw.Write(HairColor.G);
                    bw.Write(HairColor.B);

                    bw.Write(SkinColor.R);
                    bw.Write(SkinColor.G);
                    bw.Write(SkinColor.B);

                    bw.Write(ShirtColor.R);
                    bw.Write(ShirtColor.G);
                    bw.Write(ShirtColor.B);

                    bw.Write(PantsColor.R);
                    bw.Write(PantsColor.G);
                    bw.Write(PantsColor.B);

                    bw.Write(Kills);

                    bw.Write(Headshots);
                    bw.Write(RevengeKills);
                    bw.Write(GrenadeKills);
                    bw.Write(KnifeKills);

                    bw.Write(Deaths);

                    bw.Write(GravityDeaths);
                    bw.Write(LavaDeaths);

                    bw.Write(Wins);

                    bw.Write(TDMWins);
                    bw.Write(SNMWins);
                    bw.Write(FFAWins);
                    bw.Write(FWWins);
                    bw.Write(KBWins);

                    bw.Write(Defeats);

                    bw.Write(TDMDefeats);
                    bw.Write(SNMDefeats);
                    bw.Write(FFADefeats);
                    bw.Write(FWDefeats);
                    bw.Write(KBDefeats);

                    bw.Write(Ties);

                    for (int i = 0; i < gunStats.Length; i++)
                        gunStats[i].Save(bw);

                    bw.Write(XP);
                    bw.Write(Level);

                    if (Clan != null)
                        bw.Write(Clan);
                    else
                        bw.Write("");

                    Slot1.SaveClass(bw);
                    Slot2.SaveClass(bw);
                    Slot3.SaveClass(bw);
                    Slot4.SaveClass(bw);
                    Slot5.SaveClass(bw);

                    using (BinaryWriter streamBW = new BinaryWriter(stream))
                    {
                        streamBW.Write((short)5);

                        ms.Position = 0;
                        Random ranByte = new Random();
                        while (ms.Position < ms.Length)
                        {
                            streamBW.Write((byte)ranByte.Next(0, 255));
                            streamBW.Write((byte)ms.ReadByte());
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

        public delegate void PlayerProfileReloaded();
        public static event PlayerProfileReloaded PlayerProfileReloadedEvent;
        public const int
          M16 = 20,
          M16BALLISTICTIP = 30,
          M16MOREAMMO = 35,
          M16EXTENDEDMAGS = 40,
          AK47 = 40,
          AK47BALLISTICTIP = 45,
          AK47MOREAMMO = 47,
          AK47EXTENDEDMAGS = 50,
          FAL = 0,
          FALBALLISTICTIP = 7,
          FALMOREAMMO = 12,
          FALEXTENDEDMAGS = 17,
          COLT45 = 0,
          COLT45BALLISTICTIP = 7,
          COLT45MOREAMMO = 11,
          MAGNUM = 25,
          MAGNUMBALLISTICTIP = 30,
          MAGNUMMOREAMMO = 34,
          DOUBLEBARREL = 25,
          DOUBLEBARRELBALLISTICTIP = 32,
          DOUBLEBARRELMOREAMMO = 37,
          AA12 = 37,
          AA12BALLISTICTIP = 43,
          AA12MOREAMMO = 48,
          _12GUAGE = 0,
          _12GUAGEBALLISTICTIP = 13,
          _12GUAGEMOREAMMO = 20,
          MP5K = 0,
          MP5KBALLISTICTIP = 5,
          MP5KMOREAMMO = 8,
          MP5KEXTENDEDMAGS = 12,
          UMP45 = 15,
          UMP45BALLISTICTIP = 22,
          UMP45MOREAMMO = 27,
          UMP45EXTENDEDMAGS = 35,
          VECTOR = 30,
          VECTORBALLISTICTIP = 35,
          VECTORMOREAMMO = 40,
          VECTOREXTENDEDMAGS = 43,
          ENDURANCE = 10,
          THICKSKIN = 20,
          QUICKHANDS = 40,
          SHOVELBRONZE = 0,
          SHOVELSTEEL = 15,
          SHOVELDIAMOND = 35,
          PICKBRONZE = 0,
          PICKSTEEL = 20,
          PICKDIAMOND = 40,
          SHARPEREDGES = 20,
          DURABLECONSTRUCTION = 10,
          LAVABUCKET = 25,
          PITFALL = 16;
    }

    public class CharacterClass
    {
        public delegate void CharacterClassRenamed();
        public static event CharacterClassRenamed CharacterClassRenamedEvent;

       
        public Slot Slot1, Slot2, Slot3, Slot4;
        public bool MoreStamina, ThickerSkin, QuickHands;
        public byte LethalGrenadeID;
        public byte SpecialGrenadeID;
        public string name;
        public string Name { get { return name; } set { name = value; if (CharacterClassRenamedEvent != null) CharacterClassRenamedEvent.Invoke(); } }
        public static CharacterClass DEFAULT
            = new CharacterClass(
                new WeaponSlot(GunType.GUNID_MP5K, false, false, false),
                new ToolSlot(ToolType.TOOLID_ROCKSHOVEL, false, false),
                new ItemSlot(InventoryItem.DirtBlock),
                new ItemSlot(InventoryItem.EmptyBucket));
        public static CharacterClass DEFAULT2
            = new CharacterClass(
                new WeaponSlot(GunType.GUNID_MP5K, false, false, false),
                new ToolSlot(ToolType.TOOLID_ROCKSHOVEL, false, false),
                new ItemSlot(InventoryItem.DirtBlock),
                new ItemSlot(InventoryItem.EmptyBucket));
        public static CharacterClass DEFAULT3
            = new CharacterClass(
                new WeaponSlot(GunType.GUNID_MP5K, false, false, false),
                new ToolSlot(ToolType.TOOLID_ROCKSHOVEL, false, false),
                new ItemSlot(InventoryItem.DirtBlock),
                new ItemSlot(InventoryItem.EmptyBucket));
        public static CharacterClass DEFAULT4
            = new CharacterClass(
                new WeaponSlot(GunType.GUNID_MP5K, false, false, false),
                new ToolSlot(ToolType.TOOLID_ROCKSHOVEL, false, false),
                new ItemSlot(InventoryItem.DirtBlock),
                new ItemSlot(InventoryItem.EmptyBucket));
        public static CharacterClass DEFAULT5
            = new CharacterClass(
                new WeaponSlot(GunType.GUNID_MP5K, false, false, false),
                new ToolSlot(ToolType.TOOLID_ROCKSHOVEL, false, false),
                new ItemSlot(InventoryItem.DirtBlock),
                new ItemSlot(InventoryItem.EmptyBucket));


        public CharacterClass(Slot mainWeaponTool, Slot pistolTool, ItemSlot itemA, ItemSlot itemB)
        {
            Slot1 = mainWeaponTool;
            Slot2 = pistolTool;
            Slot3 = itemA;
            Slot4 = itemB;
            LethalGrenadeID = GrenadeType.GRENADE_FRAG;
            SpecialGrenadeID = GrenadeType.GRENADE_SMOKE;
        }

        private CharacterClass()
        {
        }

        public void SaveClass(BinaryWriter bw)
        {
            bw.Write(name);

            bw.Write(MoreStamina);
            bw.Write(ThickerSkin);
            bw.Write(QuickHands);

            Slot1.SaveSlot(bw);
            Slot2.SaveSlot(bw);
            Slot3.SaveSlot(bw);
            Slot4.SaveSlot(bw);

            bw.Write(LethalGrenadeID);
            bw.Write(SpecialGrenadeID);
        }

        public static CharacterClass LoadClass(BinaryReader br)
        {
            CharacterClass c = new CharacterClass();

            c.name = br.ReadString();

            c.MoreStamina = br.ReadBoolean();
            c.ThickerSkin = br.ReadBoolean();
            c.QuickHands = br.ReadBoolean();

            switch (br.ReadByte())
            {
                case Slot.SLOTTYPE_WEAPON:
                    c.Slot1 = WeaponSlot.LoadWeaponSlot(br);
                    break;
                case Slot.SLOTTYPE_TOOL:
                    c.Slot1 = ToolSlot.LoadToolSlot(br);
                    break;
                case Slot.SLOTTYPE_ITEM:
                    c.Slot1 = ItemSlot.LoadItemSlot(br);
                    break;
            }

            switch (br.ReadByte())
            {
                case Slot.SLOTTYPE_WEAPON:
                    c.Slot2 = WeaponSlot.LoadWeaponSlot(br);
                    break;
                case Slot.SLOTTYPE_TOOL:
                    c.Slot2 = ToolSlot.LoadToolSlot(br);
                    break;
                case Slot.SLOTTYPE_ITEM:
                    c.Slot2 = ItemSlot.LoadItemSlot(br);
                    break;
            }

            switch (br.ReadByte())
            {
                case Slot.SLOTTYPE_WEAPON:
                    c.Slot3 = WeaponSlot.LoadWeaponSlot(br);
                    break;
                case Slot.SLOTTYPE_TOOL:
                    c.Slot3 = ToolSlot.LoadToolSlot(br);
                    break;
                case Slot.SLOTTYPE_ITEM:
                    c.Slot3 = ItemSlot.LoadItemSlot(br);
                    break;
            }

            switch (br.ReadByte())
            {
                case Slot.SLOTTYPE_WEAPON:
                    c.Slot4 = WeaponSlot.LoadWeaponSlot(br);
                    break;
                case Slot.SLOTTYPE_TOOL:
                    c.Slot4 = ToolSlot.LoadToolSlot(br);
                    break;
                case Slot.SLOTTYPE_ITEM:
                    c.Slot4 = ItemSlot.LoadItemSlot(br);
                    break;
            }

            c.LethalGrenadeID = br.ReadByte();
            c.SpecialGrenadeID = br.ReadByte();

            return c;
        }
    }

    public abstract class Slot
    {
        public const byte SLOTTYPE_WEAPON = 0,
            SLOTTYPE_ITEM = 1,
            SLOTTYPE_TOOL = 2;
        public abstract void SaveSlot(BinaryWriter bw);
    }

    public class WeaponSlot : Slot
    {
        public WeaponSlot(byte gunID, bool extendedMags, bool ballisticTip, bool moreAmmo)
        {
            GunID = gunID;
            ExtendedMags = extendedMags;
            BallisticTip = ballisticTip;
            MoreAmmo = moreAmmo;
        }

        public byte GunID { get; set; }
        public bool ExtendedMags { get; set; }
        public bool BallisticTip { get; set; }
        public bool MoreAmmo { get; set; }

        public override void SaveSlot(BinaryWriter bw)
        {
            bw.Write(SLOTTYPE_WEAPON);
            bw.Write(GunID);
            bw.Write(ExtendedMags);
            bw.Write(BallisticTip);
            bw.Write(MoreAmmo);
        }

        public static WeaponSlot LoadWeaponSlot(BinaryReader br)
        {
            byte gunID = br.ReadByte();
            bool extendedMags = br.ReadBoolean();
            bool ballisiticTip = br.ReadBoolean();
            bool moreAmmo = br.ReadBoolean();
            return new WeaponSlot(gunID, extendedMags, ballisiticTip, moreAmmo);
        }
    }

    public class ItemSlot : Slot
    {
        public ItemSlot(InventoryItem item)
        {
            Item = item;
        }

        public InventoryItem Item { get; set; }

        public override void SaveSlot(BinaryWriter bw)
        {
            bw.Write(SLOTTYPE_ITEM);
            bw.Write((int)Item);
        }

        public static ItemSlot LoadItemSlot(BinaryReader br)
        {
            InventoryItem i = (InventoryItem)br.ReadInt32();
            return new ItemSlot(i);
        }
    }

    public class ToolSlot : Slot
    {
        public ToolSlot(byte type, bool sharperEdges, bool durableConstruction)
        {
            ToolTypeID = type;
            SharperEdges = sharperEdges;
            DurableConstruction = durableConstruction;
        }
        public byte ToolTypeID { get; set; }
        public bool SharperEdges { get; set; }
        public bool DurableConstruction { get; set; }

        public override void SaveSlot(BinaryWriter bw)
        {
            bw.Write(SLOTTYPE_TOOL);
            bw.Write(ToolTypeID);
            bw.Write(SharperEdges);
            bw.Write(DurableConstruction);
        }

        public static ToolSlot LoadToolSlot(BinaryReader br)
        {
            byte toolID = br.ReadByte();
            bool sharperEdges = br.ReadBoolean();
            bool durableConstruction = br.ReadBoolean();
            return new ToolSlot(toolID, sharperEdges, durableConstruction);
        }
    }
}
