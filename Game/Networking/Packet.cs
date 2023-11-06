using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Miner_Of_Duty.Game.Editor;
using Miner_Of_Duty.LobbyCode;

namespace Miner_Of_Duty.Game.Networking
{
    public class Packet
    {
        public const byte PACKETID_BLOCKCHANGES = 0;
        public const byte PACKETID_PLAYERMOVEMENT = 1;
        public const byte PACKETID_SEED = 2;
        public const byte PACKETID_DONEGENERATINGWORLD = 3;
        public const byte PACKETID_PLAYERSHOTAT = 4;
        public const byte PACKETID_IDIED = 5;
        public const byte PACKETID_RESPAWNPACKET = 6;
        public const byte PACKETID_BULLETFIRED = 7;
        public const byte PACKETID_WEAPONDROP = 8;
        public const byte PACKETID_WEAPONDROPAMMOCHANGE = 9;
        public const byte PACKETID_WEAPONDROPSWITCH = 10;
        public const byte PACKETID_KNIFE = 11;
        public const byte PACKETID_SWARMIESHOTAT = 12;
        public const byte PACKETID_GRENADETHROWN = 13;
        public const byte PACKETID_WEAPONSPAWNERTAKEN = 14;

        public const byte PACKETID_SWARMIEKILLED = 15;
        public const byte PACKETID_SWARMIEGRENADED = 16;
        public const byte PACKETID_SWARMIEMADE = 17;
        public const byte PACKETID_SWARMIEUPDATE = 18;
        public const byte PACKETID_SWARMIEATTACKEDPLAYER = 19;

        public const byte PACKETID_MUTE = 20;
        public const byte PACKETID_VOTE = 21;

        public const byte PACKETID_PLAYERLEVEL = 25;
        public const byte PACKETID_PLAYERSCORE = 26;

        public const byte PACKETID_FILENAMERESPONSE = 22;
        public const byte PACKETID_MAPDATA = 27;
        public const byte PACKETID_FILENAMEREQUEST = 28;
        public const byte PACKETID_MAPDATAREQUEST = 29;

        public const byte PACKETID_COUNTDOWN = 30;
        public const byte PACKETID_TOADDTOTEAM = 31;
        public const byte PACKETID_SCORES = 32;
        public const byte PACKETID_PLAYERBODY = 33;
        public const byte PACKETID_INGAMETIME = 34;
        public const byte PACKETID_PASSWORD = 35;
        public const byte PACKETID_CUSTOMMAPINFO = 36;
        public const byte PACKETID_GETMEATEAMMR = 37;

        public const byte PACKETID_SPAWNPLACED = 40;
        public const byte PACKETID_WEAPONSPAWNERCHANGED = 41;

        public const byte PACKETID_INSESSIONIJOINED = 50;
        public const byte PACKETID_PLAYERBODYLOOKS = 51;
        public const byte PACKETID_NEWTEAMS = 52;

        public const byte PACKETID_SWARMIEIDIED = 53;

        public const byte PACKETID_PITFALLBROKE = 54;

        public const byte PACKETID_EDITPERMISSIONCHANGE = 55;

        public const byte PACKETID_KINGOFHILLSCORED = 56;
        public const byte PACKETID_SPECIALKILLPOINT = 57;
        public const byte PACKETID_WAIT = 58;
        public const byte PACKETID_WAITOVER = 59;
        //public const byte PACKETID_KINGOFHILLRANGE = 57;

        //public static void WriteKingOfHillRange(LocalNetworkGamer me, int range)
        //{
        //    PacketWriter.Write(PACKETID_KINGOFHILLRANGE);
        //    PacketWriter.Write((byte)range);
        //    me.SendData(PacketWriter, SendDataOptions.Reliable);
        //}

        //public static int ReadKingOfHillRange()
        //{
        //    PacketReader.
        //}

        public static void WriteWait(LocalNetworkGamer me, NetworkGamer target)
        {
            PacketWriter.Write(PACKETID_WAIT);
            me.SendData(PacketWriter, SendDataOptions.Reliable, target);
        }

        public static void WriteWaitOver(LocalNetworkGamer me)
        {
            PacketWriter.Write(PACKETID_WAITOVER);
            me.SendData(PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);
        }

        public static void WriteSpecialKill(LocalNetworkGamer me, TeamManager.Hits.HitType type, byte points)
        {
            PacketWriter.Write(PACKETID_SPECIALKILLPOINT);
            PacketWriter.Write((byte)type);
            PacketWriter.Write(points);
            me.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteKingOfHill(LocalNetworkGamer me, ushort points, byte kingID)
        {
            PacketWriter.Write(PACKETID_KINGOFHILLSCORED);
            PacketWriter.Write(kingID);
            PacketWriter.Write(points);
            me.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadKingOfHill(TeamManager tm)
        {
            (tm as KTBManager).GiveKingPoints(PacketReader.ReadByte(), PacketReader.ReadUInt16());
        }

        public static void WritePermissionPacket(LocalNetworkGamer me, bool viewonly, byte id)
        {
            PacketWriter.Write(PACKETID_EDITPERMISSIONCHANGE);
            PacketWriter.Write(viewonly);
            PacketWriter.Write(id);
            me.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadPermissionPacket(out bool viewOnly, out byte id)
        {
            viewOnly = PacketReader.ReadBoolean();
            id = PacketReader.ReadByte();
        }

        public static void WritePitfallBroke(LocalNetworkGamer me, byte x, byte y, byte z)
        {
            PacketWriter.Write(PACKETID_PITFALLBROKE);
            PacketWriter.Write(x);
            PacketWriter.Write(y);
            PacketWriter.Write(z);
            me.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteGetMeATeam(LocalNetworkGamer gamer)
        {
            PacketWriter.Write(PACKETID_GETMEATEAMMR);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);
        }

        public static void WriteToAddToTeamManager(LocalNetworkGamer gamer, byte id, TeamManager.Team team)
        {
            PacketWriter.Write(PACKETID_TOADDTOTEAM);
            PacketWriter.Write(id);
            PacketWriter.Write((byte)team);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteToAddToTeamManager(LocalNetworkGamer gamer, byte id, TeamManager.Team team, NetworkGamer target)
        {
            PacketWriter.Write(PACKETID_TOADDTOTEAM);
            PacketWriter.Write(id);
            PacketWriter.Write((byte)team);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable, target);
        }

        public static void ReadToAddToTeamManager(out byte id, out TeamManager.Team team)
        {
            id = PacketReader.ReadByte();
            team = (TeamManager.Team)PacketReader.ReadByte();
        }

        public static void WriteSwarmieIDied(LocalNetworkGamer gamer)
        {
            PacketWriter.Write(PACKETID_SWARMIEIDIED);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteSwarmieAdded(LocalNetworkGamer gamer, short id, Vector3 postion)
        {
            PacketWriter.Write(PACKETID_SWARMIEMADE);
            PacketWriter.Write(id);
            PacketWriter.Write(postion);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadSwarmieAdded(out short id, out Vector3 pos)
        {
            id = PacketReader.ReadInt16();
            pos = PacketReader.ReadVector3();
        }

        public static void WriteSwarmieKilled(LocalNetworkGamer gamer, byte killer, short idDeadSwarmieID)
        {
            PacketWriter.Write(PACKETID_SWARMIEKILLED);
            PacketWriter.Write(killer);
            PacketWriter.Write(idDeadSwarmieID);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static short ReadSwarmieKilled(out byte Killer)
        {
            Killer = PacketReader.ReadByte();
            return PacketReader.ReadInt16();
        }

        public static void WriteSwarmieGrenaded(LocalNetworkGamer gamer, byte grenader, byte amountHit, short[] swarmiesIDKilled)
        {
            PacketWriter.Write(PACKETID_SWARMIEGRENADED);
            PacketWriter.Write(grenader);
            PacketWriter.Write(amountHit);
            PacketWriter.Write((byte)swarmiesIDKilled.Length);
            for (int i = 0; i < swarmiesIDKilled.Length; i++)
            {
                PacketWriter.Write(swarmiesIDKilled[i]);
            }
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static short[] ReadSwarmieGrenaded(out byte grenaderId, out int amountHit)
        {
            grenaderId = PacketReader.ReadByte();
            amountHit = PacketReader.ReadByte();
            short[] swarmiesKilled = new short[PacketReader.ReadByte()];
            for (int i = 0; i < swarmiesKilled.Length; i++)
            {
                swarmiesKilled[i] = PacketReader.ReadInt16();
            }
            return swarmiesKilled;
        }

        public static void WriteCustomMapInfo(LocalNetworkGamer gamer, GameModes gameMode, string teamAName, string teamBName, bool trees, bool weapons, bool editing)
        {
            PacketWriter.Write(PACKETID_CUSTOMMAPINFO);
            PacketWriter.Write((byte)gameMode);
            PacketWriter.Write(teamAName);
            PacketWriter.Write(teamBName);
            PacketWriter.Write(trees);
            PacketWriter.Write(weapons);
            PacketWriter.Write(editing);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadCustomInfoPacket(out GameModes gameMode, out string teamAName, out string teamBName, out bool trees, out bool weapons, out bool editing)
        {
            gameMode = (GameModes)PacketReader.ReadByte();
            teamAName = PacketReader.ReadString();
            teamBName = PacketReader.ReadString();
            trees = PacketReader.ReadBoolean();
            weapons = PacketReader.ReadBoolean();
            editing = PacketReader.ReadBoolean();
        }

        public static void WriteNewTeam(LocalNetworkGamer gamer)
        {

        }

        public static void WriteIJoinedSess(LocalNetworkGamer gamer)
        {
            PacketWriter.Write(PACKETID_INSESSIONIJOINED);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);
        }

        public static void WritePassword(LocalNetworkGamer gamer, string password)
        {
            PacketWriter.Write(PACKETID_PASSWORD);
            PacketWriter.Write(password);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteSpawnPlaced(LocalNetworkGamer gamer, WorldEditorGameModeInfo.SpawnPoints sp, int spawnPoint, Vector3 position)
        {
            PacketWriter.Write(PACKETID_SPAWNPLACED);
            PacketWriter.Write((byte)sp);
            PacketWriter.Write((byte)spawnPoint);
            PacketWriter.Write(position);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteSpawnerChanged(LocalNetworkGamer gamer, Vector3 position, InventoryItem weapon, bool added)
        {
            PacketWriter.Write(PACKETID_WEAPONSPAWNERCHANGED);
            PacketWriter.Write(position);
            PacketWriter.Write(added);
            if(added)
                PacketWriter.Write((byte)weapon);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadSpawnPlaced(out WorldEditorGameModeInfo.SpawnPoints sp, out int spawnPoint, out Vector3 position)
        {
            sp = (WorldEditorGameModeInfo.SpawnPoints)PacketReader.ReadByte();
            spawnPoint = PacketReader.ReadByte();
            position = PacketReader.ReadVector3();
        }

        public static void ReadSpawnPlaced(out WorldEditorGameModeInfo.SpawnPoints sp, out int spawnPoint, out Vector3 position, PacketReader PacketReader)
        {
            sp = (WorldEditorGameModeInfo.SpawnPoints)PacketReader.ReadByte();
            spawnPoint = PacketReader.ReadByte();
            position = PacketReader.ReadVector3();
        }

        public static void WriteGrenadeThrown(LocalNetworkGamer gamer, Vector3 position, byte id, float leftRot, float upDownRot, int life)
        {
            PacketWriter.Write(PACKETID_GRENADETHROWN);
            PacketWriter.Write(position);
            PacketWriter.Write(id);
            PacketWriter.Write(leftRot);
            PacketWriter.Write(upDownRot);
            PacketWriter.Write(life);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadGrenadeThrown(out Vector3 position, out byte id, out float leftRot, out float upDownRot, out int life)
        {
            position = PacketReader.ReadVector3();
            id = PacketReader.ReadByte();
            leftRot = PacketReader.ReadSingle();
            upDownRot = PacketReader.ReadSingle();
            life = PacketReader.ReadInt32();
        }

        public static void WriteKnife(LocalNetworkGamer gamer, NetworkGamer toAttack)
        {
            PacketWriter.Write(PACKETID_KNIFE);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable, toAttack);
        }

        public static void WritePlayerScore(LocalNetworkGamer gamer, NetworkGamer toSendTo, short score, byte playerID)
        {
            PacketWriter.Write(PACKETID_PLAYERSCORE);
            PacketWriter.Write(playerID);
            PacketWriter.Write(score);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable, toSendTo);
        }

        public static void WriteWeaponDropPacket(LocalNetworkGamer gamer, Vector3 position, byte weaponID, short ammoLeft, bool extendedMags, WeaponDropManager.WeaponID id)
        {
            PacketWriter.Write(PACKETID_WEAPONDROP);
            PacketWriter.Write(position);
            PacketWriter.Write(weaponID);
            PacketWriter.Write(ammoLeft);
            PacketWriter.Write(extendedMags);
            PacketWriter.Write(id.Name);
            PacketWriter.Write(id.Number);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteWeaponDropAmmoChange(LocalNetworkGamer gamer, WeaponDropManager.WeaponID id, short newAmmoLeft)
        {
            PacketWriter.Write(PACKETID_WEAPONDROPAMMOCHANGE);
            PacketWriter.Write(id.Name);
            PacketWriter.Write(id.Number);
            PacketWriter.Write(newAmmoLeft);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteWeaponSpawnerTaken(LocalNetworkGamer gamer, WeaponDropManager.Spawner spawner)
        {
            PacketWriter.Write(PACKETID_WEAPONSPAWNERTAKEN);
            PacketWriter.Write(spawner.ID.Number);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteWeaponDropSwitch(LocalNetworkGamer gamer, WeaponDropManager.WeaponID id, byte weaponID, short ammoLeft, bool extendedMags)
        {
            PacketWriter.Write(PACKETID_WEAPONDROPSWITCH);
            PacketWriter.Write(id.Name);
            PacketWriter.Write(id.Number);
            PacketWriter.Write(weaponID);
            PacketWriter.Write(ammoLeft);
            PacketWriter.Write(extendedMags);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadWeaponDropPacket(out Vector3 position, out byte weaponID, out short ammoLeft, out bool extendedMags, out WeaponDropManager.WeaponID id)
        {
            position = PacketReader.ReadVector3();
            weaponID = PacketReader.ReadByte();
            ammoLeft = PacketReader.ReadInt16();
            extendedMags = PacketReader.ReadBoolean();
            string name = PacketReader.ReadString();
            short number = PacketReader.ReadInt16();
            id = new WeaponDropManager.WeaponID(name, number);
        }

        public static void ReadWeaponDropAmmoChange(out WeaponDropManager.WeaponID id, out short newAmmoLeft)
        {
            string name = PacketReader.ReadString();
            short number = PacketReader.ReadInt16();
            id = new WeaponDropManager.WeaponID(name, number);
            newAmmoLeft = PacketReader.ReadInt16();
        }

        public static void ReadWeapinDropSwitch(out WeaponDropManager.WeaponID id, out byte weaponID, out short ammoLeft, out bool extendedMags)
        {
            string name = PacketReader.ReadString();
            short number = PacketReader.ReadInt16();
            id = new WeaponDropManager.WeaponID(name, number);
            weaponID = PacketReader.ReadByte();
            ammoLeft = PacketReader.ReadInt16();
            extendedMags = PacketReader.ReadBoolean();
        }

        public static void WritePlayerLevel(LocalNetworkGamer gamer, byte level, string clan)
        {
            PacketWriter.Write(PACKETID_PLAYERLEVEL);
            PacketWriter.Write(level);

            byte[] bytes = UTF8Encoding.UTF8.GetBytes(clan);
            byte len = (byte)bytes.Length;

            PacketWriter.Write(len);
            PacketWriter.Write(bytes);

            MinerOfDuty.CurrentPlayerProfile.WriteToPacket(PacketWriter);

            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WritePlayerLevel(LocalNetworkGamer gamer, byte level, string clan, NetworkGamer rec)
        {
            PacketWriter.Write(PACKETID_PLAYERLEVEL);
            PacketWriter.Write(level);

            byte[] bytes = UTF8Encoding.UTF8.GetBytes(clan);
            byte len = (byte)bytes.Length;

            PacketWriter.Write(len);
            PacketWriter.Write(bytes);

            MinerOfDuty.CurrentPlayerProfile.WriteToPacket(PacketWriter);

            gamer.SendData(PacketWriter, SendDataOptions.Reliable, rec);
        }

        public static void ReadPlayerLevel(GamePlayerStats stats)
        {
            stats.Level = PacketReader.ReadByte();

            try
            {
                byte[] bytes = PacketReader.ReadBytes(PacketReader.ReadByte());

                stats.ClanTag = new string(UTF8Encoding.UTF8.GetChars(bytes));
            }
            catch
            {
                stats.ClanTag = "";
            }

            stats.pp = PlayerProfile.ReadFromPacket(PacketReader);

        }

        public static void WriteMutePacket(LocalNetworkGamer gamer, NetworkGamer toMute, bool isMuted)
        {
            PacketWriter.Write(PACKETID_MUTE);
            PacketWriter.Write(!isMuted);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable, toMute);
        }

        public static PacketWriter PacketWriter = new PacketWriter();
        public static PacketReader PacketReader = new PacketReader();

        public static void WriteRespawnPacket(LocalNetworkGamer gamer, ref Vector3 position)
        {
            PacketWriter.Write(PACKETID_RESPAWNPACKET);
            PacketWriter.Write(position);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadRespawnPacket(out Vector3 position)
        {
            position = PacketReader.ReadVector3();
        }


        public static void WriteIDiedPacket(LocalNetworkGamer gamer, string killer, byte killerID, byte killerGunID, KillText.DeathType cause)
        {
            PacketWriter.Write(PACKETID_IDIED);
            PacketWriter.Write(killer);
            PacketWriter.Write(killerID);
            PacketWriter.Write((byte)cause);
            PacketWriter.Write(killerGunID);
            if (killerID != 0 && MinerOfDuty.Session.FindGamerById(killerID) != null && (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).WhoKilledMeLast == gamer.Id)
            {
                PacketWriter.Write(true);
            }
                PacketWriter.Write(false);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadIDiedPacket(string senderName, out KillText.DeathType type, out byte killerID, out byte killerGunID, out bool wasRevenge)
        {
            string killerName = PacketReader.ReadString();
            killerID = PacketReader.ReadByte();
            type = (KillText.DeathType)PacketReader.ReadByte();
            killerGunID = PacketReader.ReadByte();
            wasRevenge = PacketReader.ReadBoolean();
        }

        public static void WriteBulletFired(LocalNetworkGamer gamer, ref Ray ray, byte gunFired, float distance)
        {
            Audio.PlaySound(Audio.SOUND_FIRE);

            PacketWriter.Write(PACKETID_BULLETFIRED);
            PacketWriter.Write(ray.Position);
            PacketWriter.Write(ray.Direction);
            PacketWriter.Write(gunFired);
            PacketWriter.Write(distance);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadBulletFired(out Ray bulletRay, out byte gunFired, out float dis)
        {
            Vector3 rayPos = PacketReader.ReadVector3();
            Vector3 rayDir = PacketReader.ReadVector3();
            gunFired = PacketReader.ReadByte();
            dis = PacketReader.ReadSingle();
            bulletRay = new Ray(rayPos, rayDir);
        }

        public static void ReadSwarmieShotPacket(out float damage, out short swarmID, out KillText.DeathType type, out PlayerBody.Hit hit)
        {
            damage = PacketReader.ReadSingle();
            swarmID = PacketReader.ReadInt16();
            type = (KillText.DeathType)PacketReader.ReadByte();
            hit = (PlayerBody.Hit)PacketReader.ReadByte();
        }

        public static void WriteSwarmieShotPacket(LocalNetworkGamer gamer, float damage, short swarmID, KillText.DeathType type, PlayerBody.Hit hit)
        {
            PacketWriter.Write(PACKETID_SWARMIESHOTAT);
            PacketWriter.Write(damage);
            PacketWriter.Write(swarmID);
            PacketWriter.Write((byte)type);
            PacketWriter.Write((byte)hit);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WritePlayerShotPacket(LocalNetworkGamer gamer, float damage, byte playerID, byte gunID, ref Vector3 position, KillText.DeathType type)
        {
            PacketWriter.Write(PACKETID_PLAYERSHOTAT);
            PacketWriter.Write(playerID);
            PacketWriter.Write(damage);
            PacketWriter.Write((byte)type);
            PacketWriter.Write(gunID);
            PacketWriter.Write(position);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static Vector3 ReadPlayerShotPacket(out float damage, out byte playerIDWhoWasShot, out KillText.DeathType type, out byte gunID)
        {
            playerIDWhoWasShot = PacketReader.ReadByte();
            damage = PacketReader.ReadSingle();
            type = (KillText.DeathType)PacketReader.ReadByte();
            gunID = PacketReader.ReadByte();
            return PacketReader.ReadVector3();
        }

        public static void WriteDoneGeneratingWorldPacket(LocalNetworkGamer gamer, PlayerProfile pp)
        {
            PacketWriter.Write(PACKETID_DONEGENERATINGWORLD);

            PacketWriter.Write(pp.EyeColor.R);
            PacketWriter.Write(pp.EyeColor.G);
            PacketWriter.Write(pp.EyeColor.B);

            PacketWriter.Write(pp.HairColor.R);
            PacketWriter.Write(pp.HairColor.G);
            PacketWriter.Write(pp.HairColor.B);

            PacketWriter.Write(pp.PantsColor.R);
            PacketWriter.Write(pp.PantsColor.G);
            PacketWriter.Write(pp.PantsColor.B);

            PacketWriter.Write(pp.ShirtColor.R);
            PacketWriter.Write(pp.ShirtColor.G);
            PacketWriter.Write(pp.ShirtColor.B);

            PacketWriter.Write(pp.SkinColor.R);
            PacketWriter.Write(pp.SkinColor.G);
            PacketWriter.Write(pp.SkinColor.B);

            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WritePlayerBodyLooksPacket(LocalNetworkGamer gamer, PlayerProfile pp, NetworkGamer tosendto)
        {
            PacketWriter.Write(PACKETID_PLAYERBODYLOOKS);

            PacketWriter.Write(pp.EyeColor.R);
            PacketWriter.Write(pp.EyeColor.G);
            PacketWriter.Write(pp.EyeColor.B);

            PacketWriter.Write(pp.HairColor.R);
            PacketWriter.Write(pp.HairColor.G);
            PacketWriter.Write(pp.HairColor.B);

            PacketWriter.Write(pp.PantsColor.R);
            PacketWriter.Write(pp.PantsColor.G);
            PacketWriter.Write(pp.PantsColor.B);

            PacketWriter.Write(pp.ShirtColor.R);
            PacketWriter.Write(pp.ShirtColor.G);
            PacketWriter.Write(pp.ShirtColor.B);

            PacketWriter.Write(pp.SkinColor.R);
            PacketWriter.Write(pp.SkinColor.G);
            PacketWriter.Write(pp.SkinColor.B);

            gamer.SendData(PacketWriter, SendDataOptions.Reliable, tosendto);
        }
        public static void WritePlayerBodyLooksPacket(LocalNetworkGamer gamer, PlayerProfile pp)
        {
            PacketWriter.Write(PACKETID_PLAYERBODYLOOKS);

            PacketWriter.Write(pp.EyeColor.R);
            PacketWriter.Write(pp.EyeColor.G);
            PacketWriter.Write(pp.EyeColor.B);

            PacketWriter.Write(pp.HairColor.R);
            PacketWriter.Write(pp.HairColor.G);
            PacketWriter.Write(pp.HairColor.B);

            PacketWriter.Write(pp.PantsColor.R);
            PacketWriter.Write(pp.PantsColor.G);
            PacketWriter.Write(pp.PantsColor.B);

            PacketWriter.Write(pp.ShirtColor.R);
            PacketWriter.Write(pp.ShirtColor.G);
            PacketWriter.Write(pp.ShirtColor.B);

            PacketWriter.Write(pp.SkinColor.R);
            PacketWriter.Write(pp.SkinColor.G);
            PacketWriter.Write(pp.SkinColor.B);

            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void ReadPlayerBodyLooksPacket(PlayerBody sendersBody)
        {
            Color eye = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            Color hair = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            Color pants = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            Color shirt = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            Color skin = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            sendersBody.CreateParts(hair, eye, skin, shirt, pants);
        }


        public static void ReadDoneGeneratingWorldPacket(PlayerBody sendersBody)
        {
            Color eye = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            Color hair = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            Color pants = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            Color shirt = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            Color skin = new Color(PacketReader.ReadByte(), PacketReader.ReadByte(), PacketReader.ReadByte(), 255);
            sendersBody.CreateParts(hair, eye, skin, shirt, pants);
        }

        public static void WriteMovementPacket(Player player, GameTime gameTime, LocalNetworkGamer gamer)
        {
            PacketWriter.Write(PACKETID_PLAYERMOVEMENT);//packetheader

            PacketWriter.Write(movementPacket++);

            PacketWriter.Write((float)gameTime.TotalGameTime.TotalMilliseconds);//timestamp
            PacketWriter.Write(player.position);
            PacketWriter.Write(new Vector2(player.upDownRot, player.leftRightRot)); //for look rots
            PacketWriter.Write(player.movingSpeed);
            PacketWriter.Write((byte)player.inventory.GetSelectedItem);//item holding
            PacketWriter.Write(player.newState.Triggers.Left > .2f);//for aiming down sight

            PacketWriter.Write((byte)player.stance); //sprint, prone, crouch, walking
            PacketWriter.Write(player.Knifing);
            PacketWriter.Write(player.HoldingAGrenadeDown);
            PacketWriter.Write(player.GrenadeID);
            gamer.SendData(PacketWriter, SendDataOptions.None);
        }

        private static uint movementPacket = 0;
        public static void WriteMovementPacket(PlayerEditor player, GameTime gameTime, LocalNetworkGamer gamer)
        {
            PacketWriter.Write(PACKETID_PLAYERMOVEMENT);//packetheader

            PacketWriter.Write(movementPacket++);

            PacketWriter.Write((float)gameTime.TotalGameTime.TotalMilliseconds);//timestamp
            PacketWriter.Write(player.position);
            PacketWriter.Write(new Vector2(player.upDownRot, player.leftRightRot)); //for look rots
            PacketWriter.Write(player.movingSpeed);
            PacketWriter.Write((byte)player.inventory.GetSelectedItem);//item holding
            PacketWriter.Write(player.newState.Triggers.Left > .2f);//for aiming down sight

            PacketWriter.Write((byte)player.stance); //sprint, prone, crouch, walking
            PacketWriter.Write(false);
            PacketWriter.Write(false);
            PacketWriter.Write(0);
            gamer.SendData(PacketWriter, SendDataOptions.None);
        }


        public static void WriteSeedPacket(LocalNetworkGamer gamer, int seed)
        {
            PacketWriter.Write(PACKETID_SEED);
            PacketWriter.Write(seed);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static void WriteSeedPacket(LocalNetworkGamer gamer, int seed, int size, bool trees)
        {
            PacketWriter.Write(PACKETID_SEED);
            PacketWriter.Write(seed);
            PacketWriter.Write(size);
            PacketWriter.Write(trees);
            gamer.SendData(PacketWriter, SendDataOptions.Reliable);
        }

        public static byte GetPacketID()
        {
            return PacketReader.ReadByte();
        }

        public static int GetSeed()
        {
            return PacketReader.ReadInt32();
        }

        public static MovementPacketState ReadMovementPacket(GameTime gameTime, RollingAverage avg, NetworkGamer gamer, out InventoryItem holdingItem, 
            out bool leftTrigger, out Player.Stance stance, out bool knifing, out bool holdingGrenade, out byte grenadeID, out uint packetNumber)
        {
            packetNumber = PacketReader.ReadUInt32();

            float time = PacketReader.ReadSingle();//timestamp in total milliseconds
            //float timeDelta = (float)gameTime.TotalGameTime.TotalMilliseconds - time;
            //avg.AddTime(timeDelta);
            //float timeDeviation = timeDelta - avg.RollingAveragee;
            //time -= ((float)gamer.RoundtripTime.TotalMilliseconds / 2f) - timeDeviation;

            Vector3 newPos = PacketReader.ReadVector3();
            Vector2 rot = PacketReader.ReadVector2();
            float movingSpeed = PacketReader.ReadSingle();
            holdingItem = (InventoryItem)PacketReader.ReadByte();
            leftTrigger = PacketReader.ReadBoolean();

            stance = (Player.Stance)PacketReader.ReadByte();
            knifing = PacketReader.ReadBoolean();

            holdingGrenade = PacketReader.ReadBoolean();
            grenadeID = PacketReader.ReadByte();
            return new MovementPacketState(ref newPos, ref rot, time, movingSpeed, packetNumber);
        }

        public static void ReadMovementPacketEmpty()
        {

            PacketReader.ReadUInt32();
            PacketReader.ReadSingle();//timestamp
            PacketReader.ReadVector3();
            PacketReader.ReadVector2();
            PacketReader.ReadSingle();
            PacketReader.ReadByte();
            PacketReader.ReadBoolean();
            PacketReader.ReadByte();
            PacketReader.ReadBoolean();
            PacketReader.ReadBoolean();
            PacketReader.ReadByte();
        }

        public struct BlockChange
        {
            public Vector3 Position;
            public byte ID;
            public bool Added;

            public BlockChange(Vector3 pos, byte id, bool added)
            {
                Position = pos;
                ID = id;
                Added = added;
            }
        }

        public static void WriteBlockPacket(LocalNetworkGamer gamer, List<BlockChange> changes)
        {
            if (changes.Count == 0)
                return;

            PacketWriter.Write(PACKETID_BLOCKCHANGES);
            PacketWriter.Write((byte)changes.Count);
            for (int i = 0; i < changes.Count; i++)
            {
                PacketWriter.Write(changes[i].Position);
                PacketWriter.Write(changes[i].ID);
                PacketWriter.Write(changes[i].Added);
            }
            gamer.SendData(PacketWriter, SendDataOptions.ReliableInOrder);
        }

        public static BlockChange[] ReadBlockPacket()
        {
            BlockChange[] changes = new BlockChange[PacketReader.ReadByte()];
            for (int i = 0; i < changes.Length; i++)
            {
                changes[i].Position = PacketReader.ReadVector3();
                changes[i].ID = PacketReader.ReadByte();
                changes[i].Added = PacketReader.ReadBoolean();
            }
            return changes;
        }
        public static BlockChange[] ReadBlockPacket(PacketReader pr)
        {
            BlockChange[] changes = new BlockChange[pr.ReadByte()];
            for (int i = 0; i < changes.Length; i++)
            {
                changes[i].Position = pr.ReadVector3();
                changes[i].ID = pr.ReadByte();
                changes[i].Added = pr.ReadBoolean();
            }
            return changes;
        }

    }
}