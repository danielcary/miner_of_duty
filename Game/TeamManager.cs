using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.LobbyCode;
using Miner_Of_Duty.Game.Networking;
using System.IO;
using Miner_Of_Duty;

namespace Miner_Of_Duty.Game
{
    public abstract class TeamManager
    {
        public delegate void GameOver(string text, Color color);
        public event GameOver GameOverEvent;

        public enum Team { None = 0, TeamA = 1, TeamB = 2 }

        //public abstract Team GetTeam(byte id);
        public abstract void SaveInfo(BinaryWriter bw);
        public abstract void ReadInfo(BinaryReader br);

        public abstract void MixUpTeams();
        public abstract Team CalculateWhichTeamForPlayer(byte id);
        public void AddPlayer(byte id, Team team)
        {
            if(PlayingGamers.Contains(id) == false)
                PlayingGamers.Add(addPlayer(id, team));
        }
        public void RemovePlayer(byte id)
        {
            PlayingGamers.Remove(removePlayer(id));
        }
        public abstract Team WhatTeam(byte playerID);

        public bool ContainsPlayer(byte id)
        {
            return PlayingGamers.Contains(id);
        }

        public abstract bool AcceptNewPlayer();

        /// <returns>the player id</returns>
        protected abstract byte addPlayer(byte id, Team team);
        protected abstract byte removePlayer(byte id);

        /// <summary>
        /// Do Not add manualy to this, Use as NetworkSession.AllGamers
        /// </summary>
        public List<byte> PlayingGamers;

        protected void InvokeGameOverEvent(string text, Color color)
        {
            if(GameOverEvent != null)
                GameOverEvent.Invoke(text, color);
            MinerOfDuty.CurrentPlayerProfile.LevelUpEvent -= CurrentPlayerProfile_LevelUpEvent;
        }

        protected byte localPlayerID;
        protected bool GameHasStarted = false;
        protected MultiplayerGame game;

        protected TeamManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayerID)
        {
            PlayingGamers = new List<byte>();

            this.localPlayerID = localPlayerID;
            gamerLeft = GamerLeft;
            MinerOfDuty.CurrentPlayerProfile.LevelUpEvent += CurrentPlayerProfile_LevelUpEvent;
            hits = new Queue<Hits>();
        }

        public void AddGame(MultiplayerGame game)
        {
            this.game = game;
        }

        public void StartGame(MultiplayerGame game)
        {
            this.game = game;
            GameHasStarted = true;
        }

        public abstract bool IsOnMyTeam(byte playerID);

        private void GamerLeft(object sender, GamerLeftEventArgs e)
        {
            RemovePlayer(e.Gamer.Id);
        }

        public abstract byte[] GetKillablePlayers();

        public abstract void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge);

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                (MinerOfDuty.Session.AllGamers[i].Tag as GamePlayerStats).TimeSinceLastKill += gameTime.ElapsedGameTime.Milliseconds;
            }

            if (timeWithHit >= 0)
                timeWithHit -= gameTime.ElapsedGameTime.Milliseconds;
            if (timeWithHit < 0)
            {
                workingHit = null;
                if (hits.Count == 0)
                {
                    timeWithHit = 0;
                }
                else
                {
                    workingHit = hits.Dequeue();
                    timeWithHit = GetTimeWithHit(workingHit.type);
                }
            }

            update(gameTime);
        }

        protected abstract void update(GameTime gameTime);

        public void Draw(SpriteBatch sb)
        {
            if (workingHit != null)
                workingHit.Draw(sb);

            draw(sb);
        }

        protected abstract void draw(SpriteBatch sb);

        public abstract void DrawLeadboard(SpriteBatch sb);

        public abstract Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game);

        public abstract Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game);

        public virtual int GetSpawnDelay()
        {
            return 0;
        }

        public virtual bool CanPlaceBlocks()
        {
            return game.EditingEnabled;
        }

        protected abstract void CurrentPlayerProfile_LevelUpEvent();

        /// <summary>
        /// Use when game over
        /// </summary>
        public abstract void DrawHits(SpriteBatch sb);

        public abstract void MuteTeam();


        public virtual bool CanMineGoldBlocks()
        {
            return false;
        }

        public class Hits
        {
            public string Title;
            public string XP;
            private Vector2 titlePos, xpPos;

            public HitType type;

            public enum HitType { NormalKill, HeadShot, NimbleKill, LevelUp, MatchBonus, RoundBonus, Mine, Revenge, Blank, GoldBlockBouns, King }

            public Hits(HitType type, int xp)
            {
                this.type = type;
                switch (type)
                {
                    case HitType.NormalKill:
                        Title = "KILL";
                        break;
                    case HitType.HeadShot:
                        Title = "BOOM, HEADSHOT!";
                        break;
                    case HitType.NimbleKill:
                        Title = "NIMBLE KILL";
                        break;
                    case HitType.LevelUp:
                        Title = "LEVEL UP";
                        break;
                    case HitType.MatchBonus:
                        Title = "MATCH BONUS";
                        break;
                    case HitType.RoundBonus:
                        Title = "ROUND BONUS";
                        break;
                    case HitType.GoldBlockBouns:
                        Title = "GOLD BLOCK BONUS";
                        break;
                    case HitType.Mine:
                        Title = "BLOCK DESTROYED";
                        break;
                    case HitType.Revenge:
                        Title = "REVENGE, BABY!";
                        break;
                    case HitType.Blank:
                        Title = "";
                        break;
                    case HitType.King:
                        Title = "KING";
                        break;
                }
                if (xp > 0)
                    XP = "+" + xp.ToString();
                else
                    XP = "";

                titlePos = new Vector2((1280 / 2f) - (Resources.Font.MeasureString(Title).X / 2f), 320);
                xpPos = new Vector2((1280 / 2f) - (Resources.NameFont.MeasureString(XP).X / 2f), 360);
            }

            public void Draw(SpriteBatch sb)
            {
                sb.DrawString(Resources.Font, Title, titlePos, Color.White);
                sb.DrawString(Resources.NameFont, XP, xpPos, Color.Yellow);
            }


        }

        protected Queue<Hits> hits;
        protected Hits workingHit;
        protected int timeWithHit;
        protected abstract int GetTimeWithHit(Hits.HitType type);
        protected void AddWorkingHit(Hits.HitType type, int xp)
        {
            if (workingHit == null)
            {
                workingHit = new Hits(type, xp);
                timeWithHit = GetTimeWithHit(type);
            }
            else
            {
                hits.Enqueue(new Hits(type, xp));
            }
        }


        public abstract void SendTimeToSend();
        public abstract void ReciveTimeToSend(byte state, long time);
    }

    public abstract class TeamTeamManager : TeamManager
    {
        protected List<byte> TeamA, TeamB;
        protected int teamAScore, teamBScore;
        protected bool localIsOnTeamA;
        protected bool updateScoreBoard;

        public string teamAName = "Team Silverback";
        public string teamBName = "Team Wavves";

        public override void SaveInfo(BinaryWriter bw)
        {
            bw.Write((byte)TeamA.Count);
            bw.Write(TeamA.ToArray());
            bw.Write((byte)TeamB.Count);
            bw.Write(TeamB.ToArray());

            bw.Write(teamAScore);
            bw.Write(teamBScore);

            bw.Write(teamAName);
            bw.Write(teamBName);

            byte times = 0;
            for (int i = 0; i < base.PlayingGamers.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(PlayingGamers[i]) != null)
                {
                    times++;
                }
            }

            bw.Write(times);
            for (int i = 0; i < base.PlayingGamers.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(PlayingGamers[i]) != null)
                {
                    GamePlayerStats stats = MinerOfDuty.Session.FindGamerById(PlayingGamers[i]).Tag as GamePlayerStats;
                    bw.Write(PlayingGamers[i]);
                    bw.Write(stats.Score);
                    bw.Write(stats.Kills);
                    bw.Write(stats.Deaths);
                }
            }
        }

        public override void ReadInfo(BinaryReader br)
        {
            //TeamA = new List<byte>();
            byte[] tempTeamA = br.ReadBytes(br.ReadByte());
            for (int i = 0; i < tempTeamA.Length; i++)
            {
                if (TeamA.Contains(tempTeamA[i]) == false && MinerOfDuty.Session.FindGamerById(tempTeamA[i]) != null)
                    TeamA.Add(tempTeamA[i]);
            }
           // TeamB = new List<byte>(br.ReadBytes(br.ReadByte()));
            byte[] tempTeamB = br.ReadBytes(br.ReadByte());
            for (int i = 0; i < tempTeamB.Length; i++)
            {
                if (TeamB.Contains(tempTeamB[i]) == false && MinerOfDuty.Session.FindGamerById(tempTeamB[i]) != null)
                    TeamB.Add(tempTeamB[i]);
            }

            teamAScore = br.ReadInt32();
            teamBScore = br.ReadInt32();

            teamAName = br.ReadString();
            teamBName = br.ReadString();

            int times = br.ReadByte();

            for (int i = 0; i < times; i++)
            {
                byte id = br.ReadByte();
                if (MinerOfDuty.Session.FindGamerById(id) != null)
                {
                    (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).Score += br.ReadInt32();
                    (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).Kills += br.ReadInt32();
                    (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).Deaths += br.ReadInt32();
                }
                else
                {
                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                }
            }
        }

        public TeamTeamManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {
            TeamA = new List<byte>();
            TeamB = new List<byte>();

            updateScoreBoard = true;
        }

        public override void MixUpTeams()
        {
            //make sure to update localplayerID
            throw new NotImplementedException();
        }

        public override TeamManager.Team WhatTeam(byte playerID)
        {
            if (TeamA.Contains(playerID))
                return Team.TeamA;
            else
                return Team.TeamB;
        }

        private int TeamACount = 0, TeamBCount = 0;
        private Dictionary<byte, Team> CalTeam = new Dictionary<byte, Team>();
        public override TeamManager.Team CalculateWhichTeamForPlayer(byte id)
        {
           // if (CalTeam.ContainsKey(id))
            //    return CalTeam[id];

            if (TeamACount <= TeamBCount)
            {
                TeamACount++;
               // CalTeam.Add(id, Team.TeamA);
                return Team.TeamA;
            }
            else
            {
                TeamBCount++;
              //  CalTeam.Add(id, Team.TeamB);
                return Team.TeamB;
            }
        }

        protected override byte addPlayer(byte id, TeamManager.Team team)
        {
            if (id == localPlayerID)
            {
                if (team == Team.TeamA)
                    localIsOnTeamA = true;
                else
                    localIsOnTeamA = false;
            }

            if (team == Team.TeamA)
                TeamA.Add(id);
            else
                TeamB.Add(id);

            return id;
        }

        protected override byte removePlayer(byte id)
        {
            if (TeamA.Contains(id))
            {
                TeamACount--;
                TeamA.Remove(id);

                if (TeamA.Count == 0)
                {
                    if (GameHasStarted && game.GameOver == false)
                    {
                        InvokeGameOverEvent("VICTORY", Color.Green);
                        MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 0, Lobby.IsPrivateLobby());
                    }
                }
            }
            else if (TeamB.Contains(id))
            {
                TeamBCount--;
                TeamB.Remove(id);

                if (TeamB.Count == 0)
                {
                    if (GameHasStarted && game.GameOver == false)
                    {
                        InvokeGameOverEvent("VICTORY", Color.Green);
                        MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 0, Lobby.IsPrivateLobby());
                    }
                }

            }
            return id;
        }

        public override bool IsOnMyTeam(byte playerID)
        {
            if (localIsOnTeamA)
            {
                for (int i = 0; i < TeamA.Count; i++)
                {
                    if (TeamA[i] == playerID)
                        return true;
                }
                return false;
            }
            else
            {
                for (int i = 0; i < TeamA.Count; i++)
                {
                    if (TeamA[i] == playerID)
                        return false;
                }
                return true;
            }
        }

        public override byte[] GetKillablePlayers()
        {
            if (localIsOnTeamA)
                return TeamB.ToArray();
            else
                return TeamA.ToArray();
        }

        public override void DrawLeadboard(SpriteBatch sb)
        {
            sb.Draw(Resources.ScoreboardBack, Vector2.Zero, Color.White);

            sb.DrawString(Resources.NameFont, "SCORE", new Vector2(725 + 75 - Resources.NameFont.MeasureString("SCORE").X, 100), Color.White);
            sb.DrawString(Resources.NameFont, "KILLS", new Vector2(825 + 50 - 55, 100), Color.White);
            sb.DrawString(Resources.NameFont, "DEATHS", new Vector2(925 + 50 - 35, 100), Color.White);

            sb.DrawString(Resources.Font, teamAName, new Vector2(200, 100), Color.White);
            string text;
            Vector2 startPos = new Vector2(225, 100 + Resources.Font.LineSpacing * 1.5f);
            for (int i = 0; i < TeamA.Count; i++)
            {
                if (localIsOnTeamA)
                {
                    if (MinerOfDuty.Session.FindGamerById(TeamA[i]).IsMutedByLocalUser || (MinerOfDuty.Session.FindGamerById(TeamA[i]).Tag as GamePlayerStats).IsMuted)
                        sb.Draw(Resources.Muted, startPos - new Vector2(45, 0), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(TeamA[i]).IsTalking)
                        sb.Draw(Resources.IsTalking, startPos - new Vector2(45, 0), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(TeamA[i]).HasVoice && !MinerOfDuty.Session.FindGamerById(TeamA[i]).IsTalking)
                        sb.Draw(Resources.CanTalk, startPos - new Vector2(45, 0), Color.White);
                }
                else
                {
                    if (MinerOfDuty.Session.FindGamerById(TeamA[i]).IsMutedByLocalUser || (MinerOfDuty.Session.FindGamerById(TeamA[i]).Tag as GamePlayerStats).IsMuted)
                        sb.Draw(Resources.Muted, startPos - new Vector2(45, 0), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(TeamA[i]).IsTalking)
                        sb.Draw(game.GameOver == false ? Resources.Muted : Resources.IsTalking, startPos - new Vector2(45, 0), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(TeamA[i]).HasVoice && !MinerOfDuty.Session.FindGamerById(TeamA[i]).IsTalking)
                        sb.Draw(game.GameOver == false ? Resources.Muted : Resources.CanTalk, startPos - new Vector2(45, 0), Color.White);
                }
                sb.DrawString(Resources.NameFont, (MinerOfDuty.Session.FindGamerById(TeamA[i]).Tag as GamePlayerStats).Level + " " +
                            ((MinerOfDuty.Session.FindGamerById(TeamA[i]).Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (MinerOfDuty.Session.FindGamerById(TeamA[i]).Tag as GamePlayerStats).ClanTag + "]" : "")
                             + MinerOfDuty.Session.FindGamerById(TeamA[i]).Gamertag, startPos, localPlayerID == TeamA[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(TeamA[i]).Tag as GamePlayerStats).Score.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(475 + 105 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == TeamA[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(TeamA[i]).Tag as GamePlayerStats).Kills.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(575 + 125 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == TeamA[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(TeamA[i]).Tag as GamePlayerStats).Deaths.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(700 + 125 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == TeamA[i] ? Color.Yellow : Color.White);
                startPos.Y += Resources.NameFont.LineSpacing;
            }

            sb.DrawString(Resources.Font, teamBName, new Vector2(200, 100 + (Resources.Font.LineSpacing * 1.5f) + (Resources.NameFont.LineSpacing * 5)), Color.White);

            startPos = new Vector2(225, 100 + (Resources.Font.LineSpacing * 1.5f) + (Resources.NameFont.LineSpacing * 5) + Resources.Font.LineSpacing * 1.5f);
            for (int i = 0; i < TeamB.Count; i++)
            {
                if (!localIsOnTeamA)
                {
                    if (MinerOfDuty.Session.FindGamerById(TeamB[i]).IsMutedByLocalUser || (MinerOfDuty.Session.FindGamerById(TeamB[i]).Tag as GamePlayerStats).IsMuted)
                        sb.Draw(Resources.Muted, startPos - new Vector2(45, 0), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(TeamB[i]).IsTalking)
                        sb.Draw(Resources.IsTalking, startPos - new Vector2(45, 0), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(TeamB[i]).HasVoice && !MinerOfDuty.Session.FindGamerById(TeamB[i]).IsTalking)
                        sb.Draw(Resources.CanTalk, startPos - new Vector2(45, 0), Color.White);
                }
                else
                {
                    if (MinerOfDuty.Session.FindGamerById(TeamB[i]).IsMutedByLocalUser || (MinerOfDuty.Session.FindGamerById(TeamB[i]).Tag as GamePlayerStats).IsMuted)
                        sb.Draw(Resources.Muted, startPos - new Vector2(45, 0), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(TeamB[i]).IsTalking)
                        sb.Draw(game.GameOver == false ? Resources.Muted : Resources.IsTalking, startPos - new Vector2(45, 0), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(TeamB[i]).HasVoice && !MinerOfDuty.Session.FindGamerById(TeamB[i]).IsTalking)
                        sb.Draw(game.GameOver == false ? Resources.Muted : Resources.CanTalk, startPos - new Vector2(45, 0), Color.White);
                }
                sb.DrawString(Resources.NameFont, (MinerOfDuty.Session.FindGamerById(TeamB[i]).Tag as GamePlayerStats).Level + " " +
                            ((MinerOfDuty.Session.FindGamerById(TeamB[i]).Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (MinerOfDuty.Session.FindGamerById(TeamB[i]).Tag as GamePlayerStats).ClanTag + "]" : "")
                             + MinerOfDuty.Session.FindGamerById(TeamB[i]).Gamertag, startPos, localPlayerID == TeamB[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(TeamB[i]).Tag as GamePlayerStats).Score.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(475 + 105 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == TeamB[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(TeamB[i]).Tag as GamePlayerStats).Kills.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(575 + 125 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == TeamB[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(TeamB[i]).Tag as GamePlayerStats).Deaths.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(700 + 125 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == TeamB[i] ? Color.Yellow : Color.White);
                startPos.Y += Resources.NameFont.LineSpacing;
            }
        }

        protected override void CurrentPlayerProfile_LevelUpEvent()
        {
            if (workingHit == null)
            {
                workingHit = new Hits(Hits.HitType.LevelUp, 0);
            }
            else
            {
                hits.Enqueue(new Hits(Hits.HitType.LevelUp, 0));
            }
        }

        public override void DrawHits(SpriteBatch sb)
        {
            if (workingHit != null)
                workingHit.Draw(sb);
        }

        public override void MuteTeam()
        {
            byte[] teamToMute;
            if (localIsOnTeamA)
                teamToMute = TeamB.ToArray();
            else
                teamToMute = TeamA.ToArray();

            for (int i = 0; i < teamToMute.Length; i++)
            {
                MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(MinerOfDuty.Session.FindGamerById(teamToMute[i]), false);
            }
        }

        protected override int GetTimeWithHit(TeamManager.Hits.HitType type)
        {
            switch (type)
            {
                case Hits.HitType.HeadShot:
                case Hits.HitType.NimbleKill:
                case Hits.HitType.NormalKill:
                case Hits.HitType.Revenge:
                    return 1000;
                case Hits.HitType.LevelUp:
                    return 1500;
                case Hits.HitType.MatchBonus:
                    return 3000;
            }
            return 0;
        }

    }

    public class TeamDeathmatchManager : TeamTeamManager
    {
        protected bool over;
        protected TimeSpan timer;
        protected TimeSpan gameOverTime = new TimeSpan(0, 0, 2);
        protected const int maxScore = 7500;

        public override bool AcceptNewPlayer()
        {
            if (timer.TotalSeconds < 40)
                return false;
            else
                return true;
        }

        public TeamDeathmatchManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {
            timer = new TimeSpan(0, 15, 0);
        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            if (killedID == killerID && deathType == KillText.DeathType.Grenade)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                }
            }
            else if (deathType != KillText.DeathType.Lava && deathType != KillText.DeathType.Water && deathType != KillText.DeathType.Fall)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = killerID;

                bool gotNimble = false;
                if ((MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).TimeSinceLastKill < 750)
                {
                    gotNimble = true;
                }

                (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddKill();

                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                }
                else if (MinerOfDuty.Session.FindGamerById(killerID).IsLocal)
                {
                    MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Kill, 0, Lobby.IsPrivateLobby());
                }

                bool teamA = false;
                for (int i = 0; i < TeamA.Count; i++)
                {
                    if (TeamA[i] == killerID)
                    {
                        teamA = true;
                        break;
                    }
                }

                if (teamA)
                    teamAScore += 100;
                else
                    teamBScore += 100;


                if (deathType == KillText.DeathType.HeadShot)
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(150);
                else
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(100);


                if (killerID == localPlayerID)
                {
                    if (deathType == KillText.DeathType.HeadShot)
                    {
                        AddWorkingHit(Hits.HitType.HeadShot, 150);
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Headshot, 150, Lobby.IsPrivateLobby());
                    }
                    else if (deathType == KillText.DeathType.Normal || deathType == KillText.DeathType.Knife || deathType == KillText.DeathType.Grenade )
                    {
                        AddWorkingHit(Hits.HitType.NormalKill, 100);
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, deathType == KillText.DeathType.Normal ? PlayerProfile.KillType.Normal :
                            deathType == KillText.DeathType.Knife ? PlayerProfile.KillType.Knife : PlayerProfile.KillType.Grenade, 100, Lobby.IsPrivateLobby());
                    }
                }

                if (wasRevenge)
                {
                    if (killerID == localPlayerID)
                    {
                        AddWorkingHit(Hits.HitType.Revenge, 50);
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Revenge, 50, Lobby.IsPrivateLobby());
                    }
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(50);
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                }

                //if (gotNimble)
                //{
                //    if (killerID == localPlayerID)
                //    {
                //       //AddWorkingHit(Hits.HitType.NimbleKill, 50);
                //    }
                //    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(75);
                //}

            }
            else
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                }
            }
            updateScoreBoard = true;
        }

        protected override void update(GameTime gameTime)
        {
            #region updateScoreboard
            if (updateScoreBoard)
            {
                updateScoreBoard = false;

                List<byte> open, closed;
                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < TeamA.Count; i++)
                {
                    open.Add(TeamA[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                TeamA = closed.ToArray().ToList();

                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < TeamB.Count; i++)
                {
                    open.Add(TeamB[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                TeamB = closed.ToArray().ToList();
            #endregion
            }

            if (over == false)
            {
                if (GameHasStarted)
                    timer = timer.Subtract(gameTime.ElapsedGameTime);

                over = true;
                if (teamAScore == maxScore)
                {
                    InvokeGameOverEvent(localIsOnTeamA ? "VICTORY" : "DEFEAT", localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                    if (game.IsCustom == false)
                    {
                        if (localIsOnTeamA)
                        {
                            MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200, Lobby.IsPrivateLobby());
                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200);
                        }
                        else
                        {
                            MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100, Lobby.IsPrivateLobby());
                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100);
                        }
                    }
                }
                else if (teamBScore == maxScore)
                {
                    InvokeGameOverEvent(!localIsOnTeamA ? "VICTORY" : "DEFEAT", !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                    if (game.IsCustom == false)
                    {
                        if (!localIsOnTeamA)
                        {
                            MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200, Lobby.IsPrivateLobby());
                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200);
                        }
                        else
                        {
                            MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100, Lobby.IsPrivateLobby());
                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100);
                        }
                    }
                }
                else if (timer.TotalSeconds <= 0)
                {
                    timer = new TimeSpan(0, 0, 0);
                    gameOverTime = gameOverTime.Subtract(gameTime.ElapsedGameTime);
                    game.DontUpdatePlayerMovementsTillReset();

                    if (gameOverTime.TotalSeconds <= 1)
                    {
                        if (teamAScore > teamBScore)
                        {
                            InvokeGameOverEvent(localIsOnTeamA ? "VICTORY" : "DEFEAT", localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                            if (game.IsCustom == false)
                            {
                                if (localIsOnTeamA)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200, Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200);
                                }
                                else
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100, Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100);
                                }
                            }
                        }
                        else if (teamBScore > teamAScore)
                        {
                            InvokeGameOverEvent(!localIsOnTeamA ? "VICTORY" : "DEFEAT", !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                            if (game.IsCustom == false)
                            {
                                if (!localIsOnTeamA)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200, Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200);
                                }
                                else
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100, Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100);
                                }
                            }
                        }
                        else
                        {
                            InvokeGameOverEvent("TIE", Color.Yellow);
                            if (game.IsCustom == false)
                            {
                                
                                AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 35) + 75);
                                MinerOfDuty.CurrentPlayerProfile.AddTie(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 35) + 75, Lobby.IsPrivateLobby());
                            }
                        }
                        over = true;
                    }
                    else over = false;
                }
                else
                    over = false;
            }
        }

        protected override void draw(SpriteBatch sb)
        {
            string text;
            Color color;
            if (localIsOnTeamA)
            {
                if (teamAScore > teamBScore)
                {
                    text = "WINNING BY " + (teamAScore - teamBScore);
                    color = Color.Green;
                }
                else if (teamAScore == teamBScore)
                {
                    text = "TIED";
                    color = Color.Yellow;
                }
                else
                {
                    text = "LOSING BY " + (teamBScore - teamAScore);
                    color = Color.Red;
                }
            }
            else
            {
                if (teamAScore < teamBScore)
                {
                    text = "WINNING BY " + (teamBScore - teamAScore);
                    color = Color.Green;
                }
                else if (teamAScore == teamBScore)
                {
                    text = "TIED";
                    color = Color.Yellow;
                }
                else
                {
                    text = "LOSING BY " + (teamAScore - teamBScore);
                    color = Color.Red;
                }
            }

            sb.DrawString(Resources.Font, timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);

            sb.DrawString(Resources.Font, text, new Vector2(975 - (Resources.Font.MeasureString(text).X / 2f), 570 - Resources.Font.LineSpacing), color);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            float x = ((float)teamAScore / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            //team A
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                localIsOnTeamA ? Color.Green : Color.Red);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 602) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            x = ((float)teamBScore / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                !localIsOnTeamA ? Color.Green : Color.Red);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 643) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);


            sb.Draw(Resources.XPBarTexture, new Vector2(485, 610), Color.White);
            sb.Draw(Resources.XPYellowBarTexture, new Vector2(485, 610), new Rectangle(0, 0, (int)(Resources.XPYellowBarTexture.Width * ((float)MinerOfDuty.CurrentPlayerProfile.XP / (float)MinerOfDuty.CurrentPlayerProfile.XPTillLevel)), Resources.XPYellowBarTexture.Height), Color.White);

            if (timer.TotalSeconds <= 5)
            {
                sb.DrawString(Resources.NameFont, (timer.Seconds).ToString(), new Vector2(642.5f, 362.5f), Color.Red, 0, Resources.NameFont.MeasureString((timer.Seconds).ToString()) / 2f, 1, SpriteEffects.None, 0);
            }
        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            Vector2 teamASpawn, teamBSpawn;

            teamASpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));

            do
            {
                teamBSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
            }
            while (Vector2.Distance(teamASpawn, teamBSpawn) < 50);

            for (int i = 0; i < TeamA.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    ran.Next((int)MathHelper.Clamp(teamASpawn.X - 10, 4, 124), (int)MathHelper.Clamp(teamASpawn.X + 10, 4, 124)),
                     ran.Next((int)MathHelper.Clamp(teamASpawn.Y - 10, 4, 124), (int)MathHelper.Clamp(teamASpawn.Y + 10, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamA.Count; j++)
                {
                    if (dict.ContainsKey(TeamA[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[TeamA[j]].X, dict[TeamA[j]].Z), tmpSpwn) < 2)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    for (int y = 62; y > 0; y--)
                        if (game.Terrain.blocks[(int)tmpSpwn.X, y, (int)tmpSpwn.Y] != Block.BLOCKID_AIR)
                        {
                            dict.Add(TeamA[i], new Vector3(tmpSpwn.X, y + 1, tmpSpwn.Y));
                            break;
                        }
                }
                else
                {
                    i--;
                }

            }

            for (int i = 0; i < TeamB.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    ran.Next((int)MathHelper.Clamp(teamBSpawn.X - 10, 4, 124), (int)MathHelper.Clamp(teamBSpawn.X + 10, 4, 124)),
                     ran.Next((int)MathHelper.Clamp(teamBSpawn.Y - 10, 4, 124), (int)MathHelper.Clamp(teamBSpawn.Y + 10, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamB.Count; j++)
                {
                    if (dict.ContainsKey(TeamB[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[TeamB[j]].X, dict[TeamB[j]].Z), tmpSpwn) < 2)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    for (int y = 62; y > 0; y--)
                        if (game.Terrain.blocks[(int)tmpSpwn.X, y, (int)tmpSpwn.Y] != Block.BLOCKID_AIR)
                        {
                            dict.Add(TeamB[i], new Vector3(tmpSpwn.X, y + 1, tmpSpwn.Y));
                            break;
                        }
                }
                else
                {
                    i--;
                }

            }

            return dict;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            Vector2 tmpSpawn;
            do
            {
                tmpSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
            }
            while (AwayFromTeam(ref tmpSpawn, game) == false);

            for (int y = 62; y > 0; y--)
                if (game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Y] != Block.BLOCKID_AIR)
                {
                    return new Vector3(tmpSpawn.X, y + 1, tmpSpawn.Y);
                }
            return new Vector3(tmpSpawn.X, 61, tmpSpawn.Y);
        }

        private bool AwayFromTeam(ref Vector2 pos, MultiplayerGame game)
        {
            byte[] teamToUse = localIsOnTeamA ? TeamB.ToArray() : TeamA.ToArray();
            for (int i = 0; i < teamToUse.Length; i++)
            {
                if (game.players[teamToUse[i]].dead)
                    continue;
                if (Vector2.Distance(new Vector2(game.players[teamToUse[i]].position.X, game.players[teamToUse[i]].position.Z), pos) < 20)
                    return false;
            }
            return true;
        }

        public override void SendTimeToSend()
        {
            if (GameHasStarted == false)
                return;

            Packet.PacketWriter.Write(Packet.PACKETID_INGAMETIME);
            Packet.PacketWriter.Write((byte)0);
            Packet.PacketWriter.Write(timer.Ticks);
            game.Me.SendData(Packet.PacketWriter, SendDataOptions.Reliable);
        }

        public override void ReciveTimeToSend(byte state, long time)
        {
            timer = new TimeSpan(time);
        }
    }

    public class FortWarsManager : TeamTeamManager
    {
        private TimeSpan timer;
        private const int maxScore = 7500;
        private TimeSpan time;
        private bool fighintingTIme;
        private TimeSpan gameOverTime = new TimeSpan(0, 0, 2);
        private bool over = false;
        private bool updateScoreBoardPos;

        public override bool AcceptNewPlayer()
        {
            if (fighintingTIme == false && time.TotalSeconds < 40)
                return false;
            else if (fighintingTIme && timer.TotalSeconds < 40)
                return false;
            else
                return true;
        }

        public override void SaveInfo(BinaryWriter bw)
        {
            base.SaveInfo(bw);
            
            bw.Write(ref TeamASpawn);
            bw.Write(ref TeamBSpawn);
            bw.Write(ref TeamASpwn);
            bw.Write(ref TeamBSpwn);
        }

        public override void ReadInfo(BinaryReader br)
        {
            base.ReadInfo(br);

            br.Read(out TeamASpawn);
            br.Read(out TeamBSpawn);
            br.Read(out TeamASpwn);
            br.Read(out TeamBSpwn);
        }

        public FortWarsManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {
            fighintingTIme = false;
            time = new TimeSpan(0, 6, 00);
        }

        public bool IsBlockPlaceAbleHere(int X, int Y, int Z)
        {
            //this is the main spawns
            for (int x = (int)TeamASpawn.X - 3; x < TeamASpawn.X + 4; x++)
            {
                if (X != x)
                    continue;

                for (int y = (int)TeamASpawn.Y - 1; y < TeamASpawn.Y + 4; y++)
                {
                    if (Y != y)
                        continue;

                    for (int z = (int)TeamASpawn.Z - 3; z < TeamASpawn.Z + 4; z++)
                    {
                        if (z == Z)
                            return false;
                    }
                }
            }

            for (int x = (int)TeamBSpawn.X - 3; x < TeamBSpawn.X + 4; x++)
            {
                if (X != x)
                    continue;

                for (int y = (int)TeamBSpawn.Y - 1; y < TeamBSpawn.Y + 4; y++)
                {
                    if (Y != y)
                        continue;

                    for (int z = (int)TeamBSpawn.Z - 3; z < TeamBSpawn.Z + 4; z++)
                    {
                        if (z == Z)
                            return false;
                    }
                }
            }

            //surrounding area
            if (fighintingTIme == false)
            {
                Vector3 spawnToTest = localIsOnTeamA ? TeamBSpawn : TeamASpawn;
                for (int x = (int)spawnToTest.X - 10; x < spawnToTest.X + 11; x++)
                {
                    if (X != x)
                        continue;

                    for (int y = (int)spawnToTest.Y - 1; y < 63; y++)
                    {
                        if (Y != y)
                            continue;

                        for (int z = (int)spawnToTest.Z - 10; z < spawnToTest.Z + 11; z++)
                        {
                            if (z == Z)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        public override byte[] GetKillablePlayers()
        {
            if (fighintingTIme)
            {
                return base.GetKillablePlayers();
            }
            else
                return new byte[] { };
        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            if (killedID == killerID && deathType == KillText.DeathType.Grenade)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                }
            }
            else if (deathType != KillText.DeathType.Lava && deathType != KillText.DeathType.Water && deathType != KillText.DeathType.Fall)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = killerID;

                bool gotNimble = false;
                if ((MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).TimeSinceLastKill < 750)
                {
                    gotNimble = true;
                }

                (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddKill();

                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                }
                else if (MinerOfDuty.Session.FindGamerById(killerID).IsLocal)
                {
                    MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Kill, 0, Lobby.IsPrivateLobby());
                }

                bool teamA = false;
                for (int i = 0; i < TeamA.Count; i++)
                {
                    if (TeamA[i] == killerID)
                    {
                        teamA = true;
                        break;
                    }
                }

                if (teamA)
                    teamAScore += 100;
                else
                    teamBScore += 100;


                if (deathType == KillText.DeathType.HeadShot)
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(150);
                else
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(100);


                if (killerID == localPlayerID)
                {
                    if (deathType == KillText.DeathType.HeadShot)
                    {
                        AddWorkingHit(Hits.HitType.HeadShot, 150);
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Headshot, 150, Lobby.IsPrivateLobby());
                    }
                    else if (deathType == KillText.DeathType.Normal || deathType == KillText.DeathType.Knife || deathType == KillText.DeathType.Grenade)
                    {
                        AddWorkingHit(Hits.HitType.NormalKill, 100);
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, deathType == KillText.DeathType.Normal ? PlayerProfile.KillType.Normal :
                           deathType == KillText.DeathType.Knife ? PlayerProfile.KillType.Knife : PlayerProfile.KillType.Grenade, 100, Lobby.IsPrivateLobby());
                    }
                }

                if (wasRevenge)
                {
                    if (killerID == localPlayerID)
                    {
                        AddWorkingHit(Hits.HitType.Revenge, 50);
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Revenge, 50, Lobby.IsPrivateLobby());
                    }
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(50);
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                }

                //if (gotNimble)
                //{
                //    if (killerID == localPlayerID)
                //    {
                //      //  AddWorkingHit(Hits.HitType.NimbleKill, 50);
                //      //  MinerOfDuty.CurrentPlayerProfile.AddXP(75);
                //    }
                //    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(75);
                //}

            }
            else
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                }
            }
            updateScoreBoard = true;
        }

        protected override void update(GameTime gameTime)
        {
            #region updateScoreboard
            if (updateScoreBoardPos)
            {
                updateScoreBoardPos = false;

                List<byte> open, closed;
                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < TeamA.Count; i++)
                {
                    open.Add(TeamA[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                TeamA = closed.ToArray().ToList();

                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < TeamB.Count; i++)
                {
                    open.Add(TeamB[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                TeamB = closed.ToArray().ToList();
            #endregion
            }

            if (GameHasStarted)
                if (fighintingTIme == false)
                {
                    time = time.Subtract(gameTime.ElapsedGameTime);
                    if (time.TotalSeconds < 1)
                    {
                        fighintingTIme = true;
                        GameHasStarted = false;
                        game.Reset(GetTeamReSpawn());
                        timer = new TimeSpan(0, 15, 0);
                    }
                }

            if (fighintingTIme && GameHasStarted)
                if (over == false)
                {
                    if (GameHasStarted)
                        timer = timer.Subtract(gameTime.ElapsedGameTime);

                    over = true;
                    if (teamAScore == maxScore)
                    {
                        InvokeGameOverEvent(localIsOnTeamA ? "VICTORY" : "DEFEAT", localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                        if (game.IsCustom == false)
                        {
                            if (localIsOnTeamA)
                            {
                                MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200, Lobby.IsPrivateLobby());
                                AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200);
                            }
                            else
                            {
                                MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100, Lobby.IsPrivateLobby());
                                AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100);
                            }
                        }
                    }
                    else if (teamBScore == maxScore)
                    {
                        InvokeGameOverEvent(!localIsOnTeamA ? "VICTORY" : "DEFEAT", !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                        if (game.IsCustom == false)
                        {
                            if (!localIsOnTeamA)
                            {
                                MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200, Lobby.IsPrivateLobby());
                                AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200);
                            }
                            else
                            {
                                MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100, Lobby.IsPrivateLobby());
                                AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100);
                            }
                        }
                    }
                    else if (timer.TotalSeconds <= 1)
                    {
                        timer = new TimeSpan(0, 0, 0);
                        gameOverTime = gameOverTime.Subtract(gameTime.ElapsedGameTime);
                        game.DontUpdatePlayerMovementsTillReset();

                        if (gameOverTime.TotalSeconds <= 0)
                        {
                            if (teamAScore > teamBScore)
                            {
                                InvokeGameOverEvent(localIsOnTeamA ? "VICTORY" : "DEFEAT", localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                                if (game.IsCustom == false)
                                {
                                    if (localIsOnTeamA)
                                    {
                                        MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200, Lobby.IsPrivateLobby());
                                        AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200);
                                    }
                                    else
                                    {
                                        MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100, Lobby.IsPrivateLobby());
                                        AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100);
                                    }
                                }
                            }
                            else if (teamBScore > teamAScore)
                            {
                                InvokeGameOverEvent(!localIsOnTeamA ? "VICTORY" : "DEFEAT", !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                                if (game.IsCustom == false)
                                {
                                    if (!localIsOnTeamA)
                                    {
                                        MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200, Lobby.IsPrivateLobby());
                                        AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200);
                                    }
                                    else
                                    {
                                        MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100, Lobby.IsPrivateLobby());
                                        AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100);
                                    }
                                }
                            }
                            else
                            {
                                InvokeGameOverEvent("TIE", Color.Yellow);
                                if (game.IsCustom == false)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddTie(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 35) + 75, Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 35) + 75);
                                }
                            }
                            over = true;
                        }
                        else over = false;
                    }
                    else
                        over = false;
                }
        }

        protected override void draw(SpriteBatch sb)
        {
            if (fighintingTIme)
            {
                string text;
                Color color;
                if (localIsOnTeamA)
                {
                    if (teamAScore > teamBScore)
                    {
                        text = "WINNING BY " + (teamAScore - teamBScore);
                        color = Color.Green;
                    }
                    else if (teamAScore == teamBScore)
                    {
                        text = "TIED";
                        color = Color.Yellow;
                    }
                    else
                    {
                        text = "LOSING BY " + (teamBScore - teamAScore);
                        color = Color.Red;
                    }
                }
                else
                {
                    if (teamAScore < teamBScore)
                    {
                        text = "WINNING BY " + (teamBScore - teamAScore);
                        color = Color.Green;
                    }
                    else if (teamAScore == teamBScore)
                    {
                        text = "TIED";
                        color = Color.Yellow;
                    }
                    else
                    {
                        text = "LOSING BY " + (teamAScore - teamBScore);
                        color = Color.Red;
                    }
                }

                sb.DrawString(Resources.Font, timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);

                sb.DrawString(Resources.Font, text, new Vector2(975 - (Resources.Font.MeasureString(text).X / 2f), 570 - Resources.Font.LineSpacing), color);


                sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

                float x = ((float)teamAScore / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
                //team A
                sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                    new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                    localIsOnTeamA ? Color.Green : Color.Red);
                sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 602) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);


                sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

                x = ((float)teamBScore / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
                sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                    new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                    !localIsOnTeamA ? Color.Green : Color.Red);
                sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 643) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
            }
            else
            {
                sb.DrawString(Resources.Font, time.Minutes + ":" + (time.Seconds > 9 ? time.Seconds.ToString() : "0" + time.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(time.Minutes + ":" + (time.Seconds > 9 ? time.Seconds.ToString() : "0" + time.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);
                sb.DrawString(Resources.Font, "TILL START", new Vector2(975 - (Resources.Font.MeasureString("TILL START").X / 2f), 570 - (1f * Resources.Font.LineSpacing)), Color.White);
            }

            if (fighintingTIme)
            {
                if (timer.TotalSeconds <= 5)
                {
                    sb.DrawString(Resources.NameFont, (timer.Seconds).ToString(), new Vector2(642.5f, 362.5f), Color.Red, 0, Resources.NameFont.MeasureString((timer.Seconds).ToString()) / 2f, 1, SpriteEffects.None, 0);
                }
            }
            else
            {
                if (time.TotalSeconds <= 5)
                {
                    sb.DrawString(Resources.NameFont, (time.Seconds).ToString(), new Vector2(642.5f, 362.5f), Color.Red, 0, Resources.NameFont.MeasureString((time.Seconds).ToString()) / 2f, 1, SpriteEffects.None, 0);
                }
            }

            sb.Draw(Resources.XPBarTexture, new Vector2(485, 610), Color.White);
            sb.Draw(Resources.XPYellowBarTexture, new Vector2(485, 610), new Rectangle(0, 0, (int)(Resources.XPYellowBarTexture.Width * ((float)MinerOfDuty.CurrentPlayerProfile.XP / (float)MinerOfDuty.CurrentPlayerProfile.XPTillLevel)), Resources.XPYellowBarTexture.Height), Color.White);
        }

        public Vector3 TeamASpawn, TeamBSpawn; Vector2 TeamASpwn, TeamBSpwn;
        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();


            TeamASpwn = new Vector2(ran.Next(16, 110), ran.Next(16, 110));

            do
            {
                TeamBSpwn = new Vector2(ran.Next(16, 110), ran.Next(16, 110));
            }
            while (Vector2.Distance(TeamASpwn, TeamBSpwn) < 60);

            for (int y = 62; y > 0; y--)
                if (game.Terrain.blocks[(int)TeamASpwn.X, y, (int)TeamASpwn.Y] != Block.BLOCKID_AIR
                    && game.Terrain.blocks[(int)TeamASpwn.X, y, (int)TeamASpwn.Y] != Block.BLOCKID_LEAF
                    && game.Terrain.blocks[(int)TeamASpwn.X, y, (int)TeamASpwn.Y] != Block.BLOCKID_WOOD)
                {
                    TeamASpawn = new Vector3(TeamASpwn.X, y + 1, TeamASpwn.Y);
                    break;
                }

            for (int x = (int)TeamASpawn.X - 10; x < TeamASpawn.X + 11; x++)
                for (int y = (int)TeamASpawn.Y - 1; y < TeamASpawn.Y + 5 && y < 62; y++)
                    for (int z = (int)TeamASpawn.Z - 10; z < TeamASpawn.Z + 11; z++)
                    {
                        if ((int)TeamASpawn.Y - 1 == y)
                            game.Terrain.AddBlocks(x, y, z, Block.BLOCKID_STONEBRICKS);
                        else
                            game.Terrain.RemoveBlocks(x, y, z);
                    }

            for (int x = (int)TeamASpawn.X - 3; x < TeamASpawn.X + 4; x++)
                for (int y = (int)TeamASpawn.Y - 1; y < TeamASpawn.Y + 5 && y < 62; y++)
                    for (int z = (int)TeamASpawn.Z - 3; z < TeamASpawn.Z + 4; z++)
                    {
                        if ((int)TeamASpawn.Y - 1 == y)
                            game.Terrain.AddBlocks(x, y, z, Block.BLOCKID_BEDROCK);
                        else
                            game.Terrain.RemoveBlocks(x, y, z);
                    }

            for (int y = 62; y > 0; y--)
                if (game.Terrain.blocks[(int)TeamBSpwn.X, y, (int)TeamBSpwn.Y] != Block.BLOCKID_AIR
                    && game.Terrain.blocks[(int)TeamBSpwn.X, y, (int)TeamBSpwn.Y] != Block.BLOCKID_LEAF
                    && game.Terrain.blocks[(int)TeamBSpwn.X, y, (int)TeamBSpwn.Y] != Block.BLOCKID_WOOD)
                {
                    TeamBSpawn = new Vector3(TeamBSpwn.X, y + 1, TeamBSpwn.Y);
                    break;
                }

            for (int x = (int)TeamBSpawn.X - 10; x < TeamBSpawn.X + 11; x++)
                for (int y = (int)TeamBSpawn.Y - 1; y < TeamBSpawn.Y + 5 && y < 62; y++)
                    for (int z = (int)TeamBSpawn.Z - 10; z < TeamBSpawn.Z + 11; z++)
                    {
                        if ((int)TeamBSpawn.Y - 1 == y)
                            game.Terrain.AddBlocks(x, y, z, Block.BLOCKID_STONEBRICKS);
                        else
                            game.Terrain.RemoveBlocks(x, y, z);
                    }

            for (int x = (int)TeamBSpawn.X - 3; x < TeamBSpawn.X + 4; x++)
                for (int y = (int)TeamBSpawn.Y - 1; y < TeamBSpawn.Y + 5 && y < 62; y++)
                    for (int z = (int)TeamBSpawn.Z - 3; z < TeamBSpawn.Z + 4; z++)
                    {
                        if ((int)TeamBSpawn.Y - 1 == y)
                            game.Terrain.AddBlocks(x, y, z, Block.BLOCKID_BEDROCK);
                        else
                            game.Terrain.RemoveBlocks(x, y, z);
                    }


            game.Terrain.Update();

            for (int i = 0; i < TeamA.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    ran.Next((int)MathHelper.Clamp(TeamASpwn.X - 2, 4, 124), (int)MathHelper.Clamp(TeamASpwn.X + 2, 4, 124)),
                     ran.Next((int)MathHelper.Clamp(TeamASpwn.Y - 2, 4, 124), (int)MathHelper.Clamp(TeamASpwn.Y + 2, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamA.Count; j++)
                {
                    if (dict.ContainsKey(TeamA[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[TeamA[j]].X, dict[TeamA[j]].Z), tmpSpwn) < 1)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    dict.Add(TeamA[i], new Vector3(tmpSpwn.X, TeamASpawn.Y + 1, tmpSpwn.Y));
                }
                else
                {
                    i--;
                }

            }

            for (int i = 0; i < TeamB.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    ran.Next((int)MathHelper.Clamp(TeamBSpwn.X - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpwn.X + 2, 4, 124)),
                     ran.Next((int)MathHelper.Clamp(TeamBSpwn.Y - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpwn.Y + 2, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamB.Count; j++)
                {
                    if (dict.ContainsKey(TeamB[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[TeamB[j]].X, dict[TeamB[j]].Z), tmpSpwn) < 1)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    dict.Add(TeamB[i], new Vector3(tmpSpwn.X, TeamBSpawn.Y + 1, tmpSpwn.Y));
                }
                else
                {
                    i--;
                }

            }

            return dict;
        }

        private Dictionary<byte, Vector3> GetTeamReSpawn()
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();


            for (int i = 0; i < TeamA.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    game.Random.Next((int)MathHelper.Clamp(TeamASpwn.X - 2, 4, 124), (int)MathHelper.Clamp(TeamASpwn.X + 2, 4, 124)),
                     game.Random.Next((int)MathHelper.Clamp(TeamASpwn.Y - 2, 4, 124), (int)MathHelper.Clamp(TeamASpwn.Y + 2, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamA.Count; j++)
                {
                    if (dict.ContainsKey(TeamA[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[TeamA[j]].X, dict[TeamA[j]].Z), tmpSpwn) < 1.5)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    dict.Add(TeamA[i], new Vector3(tmpSpwn.X, TeamASpawn.Y + 1, tmpSpwn.Y));
                }
                else
                {
                    i--;
                }

            }

            for (int i = 0; i < TeamB.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    game.Random.Next((int)MathHelper.Clamp(TeamBSpwn.X - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpwn.X + 2, 4, 124)),
                     game.Random.Next((int)MathHelper.Clamp(TeamBSpwn.Y - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpwn.Y + 2, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamB.Count; j++)
                {
                    if (dict.ContainsKey(TeamB[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[TeamB[j]].X, dict[TeamB[j]].Z), tmpSpwn) < 1.5)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    dict.Add(TeamB[i], new Vector3(tmpSpwn.X, TeamBSpawn.Y + 1, tmpSpwn.Y));
                }
                else
                {
                    i--;
                }

            }

            return dict;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            if (localIsOnTeamA)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    game.PersonalRandom.Next((int)MathHelper.Clamp(TeamASpwn.X - 2, 4, 124), (int)MathHelper.Clamp(TeamASpwn.X + 2, 4, 124)),
                     game.PersonalRandom.Next((int)MathHelper.Clamp(TeamASpwn.Y - 2, 4, 124), (int)MathHelper.Clamp(TeamASpwn.Y + 2, 4, 124)));

                return new Vector3(tmpSpwn.X, TeamASpawn.Y + 1, tmpSpwn.Y);
            }
            else
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    game.PersonalRandom.Next((int)MathHelper.Clamp(TeamBSpwn.X - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpwn.X + 2, 4, 124)),
                     game.PersonalRandom.Next((int)MathHelper.Clamp(TeamBSpwn.Y - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpwn.Y + 2, 4, 124)));

                return new Vector3(tmpSpwn.X, TeamBSpawn.Y + 1, tmpSpwn.Y);
            }
        }

        public override void SendTimeToSend()
        {
            if (GameHasStarted == false)
                return;

            Packet.PacketWriter.Write(Packet.PACKETID_INGAMETIME);
            if (fighintingTIme == false)
            {
                Packet.PacketWriter.Write((byte)0);
                Packet.PacketWriter.Write(time.Ticks);
            }
            else
            {
                Packet.PacketWriter.Write((byte)1);
                Packet.PacketWriter.Write(timer.Ticks);
            }

            game.Me.SendData(Packet.PacketWriter, SendDataOptions.Reliable);
        }

        public override void ReciveTimeToSend(byte state, long time)
        {

            if (state == 0)
            {
                fighintingTIme = false;
                this.time = new TimeSpan(time);
            }
            else
            {
                fighintingTIme = true;
                timer = new TimeSpan(time);
            }
        }
    }

    public class SearchNMineManager : TeamTeamManager
    {
        private TimeSpan timer;
        private const int maxScore = 2;
        private int round = 1;
        private bool showRoundText;
        private string roundText;
        private TimeSpan roundTextShowTime;
        private string roundOverText;
        private TimeSpan roundOverShowTime;
        private bool showRoundOverText = false;
        private TimeSpan gameOverTime = new TimeSpan(0, 0, 2);
        private bool hasEverStarted = false;

        public SearchNMineManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {
            timer = new TimeSpan(0, 5, 0);
        }

        public override bool AcceptNewPlayer()
        {
            if (hasEverStarted == false)
                return false;
            else if (timer.TotalSeconds < 40 || showRoundOverText)
                return false;
            else
                return true;
        }

        private bool IsDefender(bool teamA)
        {
            if (teamA)
            {
                if (round == 2)
                    return true;
                else
                    return false;
            }
            else
            {
                if (round == 2)
                    return false;
                else
                    return true;
            }
        }

        public override int GetSpawnDelay()
        {
            if (IsDefender(localIsOnTeamA))
                return 11000;
            else
                return 0;
        }

        protected override int GetTimeWithHit(TeamManager.Hits.HitType type)
        {
            return base.GetTimeWithHit(type);
        }

        public override bool CanMineGoldBlocks()
        {
            return !IsDefender(localIsOnTeamA);
        }

        public void PlayerMinedGoldBlock(byte id)
        {
            if (MinerOfDuty.Session.FindGamerById(id) == null)
                return;

            (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).AddScore(150);
            if (id == localPlayerID)
            {
                if (this is CustomSearchNMiner == false)
                    MinerOfDuty.CurrentPlayerProfile.AddBlockMined(game.type, 150, Lobby.IsPrivateLobby());
                AddWorkingHit(Hits.HitType.Mine, 150);
            }
        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            KilledPlayer(killerID, killedID, deathType, wasRevenge, true);
        }

        public void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge, bool giveXP)
        {
            if (killedID == killerID && deathType == KillText.DeathType.Grenade)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (giveXP)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
            }
            else if (deathType != KillText.DeathType.Lava && deathType != KillText.DeathType.Water && deathType != KillText.DeathType.Fall)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = killerID;

                bool gotNimble = false;
                if ((MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).TimeSinceLastKill < 750)
                {
                    gotNimble = true;
                }

                (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddKill();

                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (giveXP)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
                else if (MinerOfDuty.Session.FindGamerById(killerID).IsLocal)
                {
                    if (giveXP)
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Kill, 0, Lobby.IsPrivateLobby());
                }


                if (deathType == KillText.DeathType.HeadShot)
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(150);
                else
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(100);


                if (killerID == localPlayerID)
                {
                    if (deathType == KillText.DeathType.HeadShot)
                    {
                        AddWorkingHit(Hits.HitType.HeadShot, 150);
                        if (giveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Headshot, 150, Lobby.IsPrivateLobby());
                    }
                    else if (deathType == KillText.DeathType.Normal || deathType == KillText.DeathType.Knife || deathType == KillText.DeathType.Grenade)
                    {
                        AddWorkingHit(Hits.HitType.NormalKill, 100);
                        if (giveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, deathType == KillText.DeathType.Normal ? PlayerProfile.KillType.Normal :
                                deathType == KillText.DeathType.Knife ? PlayerProfile.KillType.Knife : PlayerProfile.KillType.Grenade, 100, Lobby.IsPrivateLobby());
                    }
                }

                if (wasRevenge)
                {
                    if (killerID == localPlayerID)
                    {
                        AddWorkingHit(Hits.HitType.Revenge, 50);
                        if (giveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Revenge, 50, Lobby.IsPrivateLobby());
                    }
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(50);
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                }

                //if (gotNimble)
                //{
                //    if (killerID == localPlayerID)
                //    {
                //      //  AddWorkingHit(Hits.HitType.NimbleKill, 50);
                //       // if (giveXP)
                //        //    MinerOfDuty.CurrentPlayerProfile.AddXP(75);
                //    }
                //    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(75);
                //}

            }
            else
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (giveXP)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
            }
            updateScoreBoard = true;
        }

        protected override void update(GameTime gameTime)
        {
            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                (MinerOfDuty.Session.AllGamers[i].Tag as GamePlayerStats).TimeSinceLastKill += gameTime.ElapsedGameTime.Milliseconds;
            }

            if (timeWithHit >= 0)
                timeWithHit -= gameTime.ElapsedGameTime.Milliseconds;
            if (timeWithHit < 0)
            {
                workingHit = null;
                if (hits.Count == 0)
                {
                    timeWithHit = 0;
                }
                else
                {
                    timeWithHit = 1000;
                    workingHit = hits.Dequeue();
                }
            }

            #region updateScoreboard
            if (updateScoreBoard)
            {
                updateScoreBoard = false;

                List<byte> open, closed;
                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < TeamA.Count; i++)
                {
                    open.Add(TeamA[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                TeamA = closed.ToArray().ToList();

                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < TeamB.Count; i++)
                {
                    open.Add(TeamB[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                TeamB = closed.ToArray().ToList();
            #endregion
            }

            if (GameHasStarted)
            {
                if (hasEverStarted == false)
                {
                    roundText = "ROUND " + round + (IsDefender(localIsOnTeamA) ? " - DEFEND" : " - MINE");
                    showRoundText = true;
                    roundTextShowTime = new TimeSpan(0, 0, 3);
                    timer = new TimeSpan(0, 6, 0);//630
                    hasEverStarted = true;
                }
                else if (hasEverStarted && showRoundOverText == false)
                {
                    timer = timer.Subtract(gameTime.ElapsedGameTime);
                }

                if (game.goldBlocks.Count == 0 && timer.TotalSeconds > 0 && showRoundOverText == false)
                {
                    game.KillOffGoldBlocks();
                    game.DontUpdatePlayerMovementsTillReset();
                    if (IsDefender(true))
                    {
                        teamBScore++;
                        if (!localIsOnTeamA)
                            roundOverText = "VICTORY";
                        else
                            roundOverText = "DEFEAT";
                    }
                    else
                    {
                        if (localIsOnTeamA)
                            roundOverText = "VICTORY";
                        else
                            roundOverText = "DEFEAT";
                        teamAScore++;
                    }
                    showRoundOverText = true;
                    roundOverShowTime = new TimeSpan(0, 0, 5);
                    if (round == 3)//do this after because our score wouldnt have been added
                    {
                        if (teamAScore == maxScore)
                        {
                            InvokeGameOverEvent(localIsOnTeamA ? "VICTORY" : "DEFEAT", localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                            if (game.IsCustom == false)
                            {
                                if (localIsOnTeamA)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500));
                                }
                                else
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500));
                                }
                            }
                        }
                        else if (teamBScore == maxScore)
                        {
                            InvokeGameOverEvent(!localIsOnTeamA ? "VICTORY" : "DEFEAT", !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                            if (game.IsCustom == false)
                            {
                                if (!localIsOnTeamA)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500));
                                }
                                else
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500));
                                }
                            }
                        }

                        showRoundOverText = false;
                        return;
                    }
                    else if (round == 2 && (teamAScore == 2 || teamBScore == 2))
                    {
                        if (teamAScore == 2)
                        {
                            InvokeGameOverEvent(localIsOnTeamA ? "VICTORY" : "DEFEAT", localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                            if (game.IsCustom == false)
                            {
                                if (localIsOnTeamA)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500));
                                }
                                else
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500));
                                }
                            }
                        }
                        else
                        {
                            InvokeGameOverEvent(!localIsOnTeamA ? "VICTORY" : "DEFEAT", !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                            if (game.IsCustom == false)
                            {
                                if (!localIsOnTeamA)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500));
                                }
                                else
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500));
                                }
                            }
                        }


                        showRoundOverText = false;
                        return;
                    }
                    timer = timer.Subtract(timer);
                }


                if (showRoundText)
                {
                    roundTextShowTime = roundTextShowTime.Subtract(gameTime.ElapsedGameTime);
                    if (roundTextShowTime.TotalSeconds <= 0)
                        showRoundText = false;
                }

                if (showRoundOverText == false)
                {
                    if (timer.TotalSeconds <= 0)
                    {
                        timer = new TimeSpan(0, 0, 0);
                        gameOverTime = gameOverTime.Subtract(gameTime.ElapsedGameTime);
                        game.DontUpdatePlayerMovementsTillReset();

                        if (gameOverTime.TotalSeconds <= 0)
                        {
                            game.KillOffGoldBlocks();
                            game.DontUpdatePlayerMovementsTillReset();
                            showRoundOverText = true;
                            roundOverShowTime = new TimeSpan(0, 0, 5);
                            gameOverTime = new TimeSpan(0, 0, 2);
                            if (IsDefender(true))
                            {
                                teamAScore++;//if teamA was defending they got a point as time ran up otherwise team b was defending
                                if (localIsOnTeamA)
                                    roundOverText = "VICTORY";
                                else
                                    roundOverText = "DEFEAT";
                            }
                            else
                            {
                                if (localIsOnTeamA == false)
                                    roundOverText = "VICTORY";
                                else
                                    roundOverText = "DEFEAT";
                                teamBScore++;
                            }


                            if (round == 3)//do this after because our score wouldnt have been added
                            {
                                if (teamAScore == maxScore)
                                {
                                    InvokeGameOverEvent(localIsOnTeamA ? "VICTORY" : "DEFEAT", localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                                    if (game.IsCustom == false)
                                    {
                                        if (localIsOnTeamA)
                                        {
                                            MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500), Lobby.IsPrivateLobby());
                                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500));
                                        }
                                        else
                                        {
                                            MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500), Lobby.IsPrivateLobby());
                                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500));
                                        }
                                    }
                                }
                                else if (teamBScore == maxScore)
                                {
                                    InvokeGameOverEvent(!localIsOnTeamA ? "VICTORY" : "DEFEAT", !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                                    if (game.IsCustom == false)
                                    {
                                        if (!localIsOnTeamA)
                                        {
                                            MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500), Lobby.IsPrivateLobby());
                                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500));
                                        }
                                        else
                                        {
                                            MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500), Lobby.IsPrivateLobby());
                                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500));
                                        }
                                    }
                                }

                                showRoundOverText = false;
                            }
                            else if (round == 2 && (teamAScore == 2 || teamBScore == 2))
                            {
                                if (teamAScore == 2)
                                {
                                    InvokeGameOverEvent(localIsOnTeamA ? "VICTORY" : "DEFEAT", localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                                    if (game.IsCustom == false)
                                    {
                                        if (localIsOnTeamA)
                                        {
                                            MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500), Lobby.IsPrivateLobby());
                                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500));
                                        }
                                        else
                                        {
                                            MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500), Lobby.IsPrivateLobby());
                                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500));
                                        }
                                    }
                                }
                                else
                                {
                                    InvokeGameOverEvent(!localIsOnTeamA ? "VICTORY" : "DEFEAT", !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);
                                    if (game.IsCustom == false)
                                    {
                                        if (!localIsOnTeamA)
                                        {
                                            MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500), Lobby.IsPrivateLobby());
                                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 40) + 200 + (teamAScore * 500));
                                        }
                                        else
                                        {
                                            MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500), Lobby.IsPrivateLobby());
                                            AddWorkingHit(Hits.HitType.MatchBonus, ((MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Kills * 30) + 100 + (teamBScore * 500));
                                        }
                                    }
                                }


                                showRoundOverText = false;
                            }
                        }
                    }
                }
                else
                {
                    roundOverShowTime = roundOverShowTime.Subtract(gameTime.ElapsedGameTime);
                    if (roundOverShowTime.TotalSeconds <= 0)
                    {
                        showRoundOverText = false;
                        GameHasStarted = false;
                        round++;
                        hasEverStarted = false;
                        game.Reset(GetTeamSpawn(game.Random, game));
                    }

                }
            }

        }

        protected override void draw(SpriteBatch sb)
        {
            draw(sb, true);
        }

        protected void draw(SpriteBatch sb, bool showXP)
        {
            if (showRoundText)
            {
                sb.DrawString(Resources.Font, roundText, new Vector2(640, 400), Color.Yellow, 0, Resources.Font.MeasureString(roundText) / 2f, 1, SpriteEffects.None, 0);
               
            }


            if (showRoundOverText)
            {
                sb.DrawString(Resources.Font, "ROUND OVER", new Vector2(640, 400), Color.Yellow, 0, Resources.Font.MeasureString("ROUND OVER") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.Font, roundOverText, new Vector2(640, 400 + Resources.Font.LineSpacing), Color.Yellow, 0, Resources.Font.MeasureString(roundOverText) / 2f, 1, SpriteEffects.None, 0);
            }

            if (!showRoundOverText)
            {
                sb.DrawString(Resources.Font, CanMineGoldBlocks() ? "MINING" : "DEFENDING", new Vector2(640 - (Resources.Font.MeasureString(CanMineGoldBlocks() ? "MINING" : "DEFENDING").X / 2f), 60), Color.Yellow);
            }
            

            string text;
            Color color;
            if (localIsOnTeamA)
            {
                if (teamAScore > teamBScore)
                {
                    text = "WINNING BY " + (teamAScore - teamBScore);
                    color = Color.Green;
                }
                else if (teamAScore == teamBScore)
                {
                    text = "TIED";
                    color = Color.Yellow;
                }
                else
                {
                    text = "LOSING BY " + (teamBScore - teamAScore);
                    color = Color.Red;
                }
            }
            else
            {
                if (teamAScore < teamBScore)
                {
                    text = "WINNING BY " + (teamBScore - teamAScore);
                    color = Color.Green;
                }
                else if (teamAScore == teamBScore)
                {
                    text = "TIED";
                    color = Color.Yellow;
                }
                else
                {
                    text = "LOSING BY " + (teamAScore - teamBScore);
                    color = Color.Red;
                }
            }

            sb.DrawString(Resources.Font, timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);

            sb.DrawString(Resources.Font, text, new Vector2(975 - (Resources.Font.MeasureString(text).X / 2f), 570 - Resources.Font.LineSpacing), color);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            float x = ((float)teamAScore / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            //team A
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                localIsOnTeamA ? Color.Green : Color.Red);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 602) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            x = ((float)teamBScore / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                !localIsOnTeamA ? Color.Green : Color.Red);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 643) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);

            if (showXP)
            {
                sb.Draw(Resources.XPBarTexture, new Vector2(485, 610), Color.White);
                sb.Draw(Resources.XPYellowBarTexture, new Vector2(485, 610), new Rectangle(0, 0, (int)(Resources.XPYellowBarTexture.Width * ((float)MinerOfDuty.CurrentPlayerProfile.XP / (float)MinerOfDuty.CurrentPlayerProfile.XPTillLevel)), Resources.XPYellowBarTexture.Height), Color.White);
            }

            if (timer.TotalSeconds <= 5 && showRoundOverText == false)
            {
                sb.DrawString(Resources.NameFont, (timer.Seconds).ToString(), new Vector2(642.5f, 362.5f), Color.Red, 0, Resources.NameFont.MeasureString((timer.Seconds).ToString()) / 2f, 1, SpriteEffects.None, 0);
            }
        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            game.SpawnGoldBlocks(5);
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            Vector2 teamASpawn, teamBSpawn;

            teamASpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));

            do
            {
                teamBSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
            }
            while (Vector2.Distance(teamASpawn, teamBSpawn) < 50);

            for (int i = 0; i < TeamA.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    ran.Next((int)MathHelper.Clamp(teamASpawn.X - 10, 4, 124), (int)MathHelper.Clamp(teamASpawn.X + 10, 4, 124)),
                     ran.Next((int)MathHelper.Clamp(teamASpawn.Y - 10, 4, 124), (int)MathHelper.Clamp(teamASpawn.Y + 10, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamA.Count; j++)
                {
                    if (dict.ContainsKey(TeamA[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[TeamA[j]].X, dict[TeamA[j]].Z), tmpSpwn) < 2)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    for (int y = 62; y > 0; y--)
                        if (game.Terrain.blocks[(int)tmpSpwn.X, y, (int)tmpSpwn.Y] != Block.BLOCKID_AIR)
                        {
                            dict.Add(TeamA[i], new Vector3(tmpSpwn.X, y + 1, tmpSpwn.Y));
                            break;
                        }
                }
                else
                {
                    i--;
                }

            }

            for (int i = 0; i < TeamB.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    ran.Next((int)MathHelper.Clamp(teamBSpawn.X - 10, 4, 124), (int)MathHelper.Clamp(teamBSpawn.X + 10, 4, 124)),
                     ran.Next((int)MathHelper.Clamp(teamBSpawn.Y - 10, 4, 124), (int)MathHelper.Clamp(teamBSpawn.Y + 10, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamB.Count; j++)
                {
                    if (dict.ContainsKey(TeamB[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[TeamB[j]].X, dict[TeamB[j]].Z), tmpSpwn) < 2)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    for (int y = 62; y > 0; y--)
                        if (game.Terrain.blocks[(int)tmpSpwn.X, y, (int)tmpSpwn.Y] != Block.BLOCKID_AIR)
                        {
                            dict.Add(TeamB[i], new Vector3(tmpSpwn.X, y + 1, tmpSpwn.Y));
                            break;
                        }
                }
                else
                {
                    i--;
                }

            }

            return dict;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            Vector2 tmpSpawn;
            do
            {
                tmpSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
            }
            while (AwayFromTeam(ref tmpSpawn, game) == false);

            for (int y = 62; y > 0; y--)
                if (game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Y] != Block.BLOCKID_AIR)
                {
                    return new Vector3(tmpSpawn.X, y + 1, tmpSpawn.Y);
                }
            return new Vector3(tmpSpawn.X, 61, tmpSpawn.Y);
        }

        private bool AwayFromTeam(ref Vector2 pos, MultiplayerGame game)
        {
            byte[] teamToUse = localIsOnTeamA ? TeamB.ToArray() : TeamA.ToArray();
            for (int i = 0; i < teamToUse.Length; i++)
            {
                if (game.players[teamToUse[i]].dead)
                    continue;
                if (Vector2.Distance(new Vector2(game.players[teamToUse[i]].position.X, game.players[teamToUse[i]].position.Z), pos) < 20)
                    return false;
            }
            return true;
        }

        public override void SendTimeToSend()
        {
            if (GameHasStarted == false)
                return;


            if (showRoundOverText == false)
            {
                Packet.PacketWriter.Write(Packet.PACKETID_INGAMETIME);
                Packet.PacketWriter.Write((byte)round);
                Packet.PacketWriter.Write(timer.Ticks);
                game.Me.SendData(Packet.PacketWriter, SendDataOptions.Reliable);
            }


        }

        public override void ReciveTimeToSend(byte state, long time)
        {
            round = state;
            GameHasStarted = true;
            timer = new TimeSpan(time);
        }
    }

    public class FFAManager : TeamManager
    {
        protected List<byte> players;
        private Dictionary<byte, int> scores;
        private TimeSpan timer;
        private const int maxScore = 3000;

        public override bool AcceptNewPlayer()
        {
            if (timer.TotalSeconds < 40)
                return false;
            else
                return true;
        }

        public override void ReadInfo(BinaryReader br)
        {
            int times = br.ReadByte();
            for (int i = 0; i < times; i++)
            {
                byte id = br.ReadByte();
                if (MinerOfDuty.Session.FindGamerById(id) != null)
                {
                    GamePlayerStats stats = (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats);
                    if(players.Contains(id) == false)
                        players.Add(id);
                    stats.Score += br.ReadInt32();
                    if (scores.ContainsKey(id) == false)
                        scores.Add(id, stats.Score);
                    else
                        scores[id] = stats.Score;
                    stats.Kills += br.ReadInt32();
                    stats.Deaths += br.ReadInt32();
                }
                else
                {
                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                }
            }
        }

        public override void SaveInfo(BinaryWriter bw)
        {
            byte times = 0;
            for (int i = 0; i < base.PlayingGamers.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(PlayingGamers[i]) != null)
                {
                    times++;
                }
            }

            bw.Write(times);
            for (int i = 0; i < base.PlayingGamers.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(PlayingGamers[i]) != null)
                {
                    GamePlayerStats stats = MinerOfDuty.Session.FindGamerById(PlayingGamers[i]).Tag as GamePlayerStats;
                    bw.Write(PlayingGamers[i]);
                    bw.Write(stats.Score);
                    bw.Write(stats.Kills);
                    bw.Write(stats.Deaths);
                }
            }
        }

        public FFAManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {
            this.players = new List<byte>();
            this.scores = new Dictionary<byte, int>();

            updateScoreBoardPos = true;
            timer = new TimeSpan(0, 15, 0);
        }

        public override TeamManager.Team WhatTeam(byte playerID)
        {
            return Team.None;
        }

        public override void MixUpTeams()
        {
            
        }

        public override TeamManager.Team CalculateWhichTeamForPlayer(byte id)
        {
            return Team.None;
        }

        protected override byte addPlayer(byte id, TeamManager.Team team)
        {
            players.Add(id);
            scores.Add(id, 0);

            return id;
        }

        protected override byte removePlayer(byte id)
        {
            if (players.Contains(id))
                players.Remove(id);
            if (scores.ContainsKey(id))
                scores.Remove(id);

            if (GameHasStarted && players.Count == 1)
            {
                InvokeGameOverEvent("VICTORY", Color.Green);
                MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 0, Lobby.IsPrivateLobby());
            }
            return id;
        }

        public override void DrawHits(SpriteBatch sb)
        {
            if (workingHit != null)
                workingHit.Draw(sb);
        }

        protected override void CurrentPlayerProfile_LevelUpEvent()
        {
            if (workingHit == null)
            {
                workingHit = new Hits(Hits.HitType.LevelUp, 0);
            }
            else
            {
                hits.Enqueue(new Hits(Hits.HitType.LevelUp, 0));
            }
        }

        protected override int GetTimeWithHit(TeamManager.Hits.HitType type)
        {
            switch (type)
            {
                case Hits.HitType.HeadShot:
                case Hits.HitType.NimbleKill:
                case Hits.HitType.NormalKill:
                case Hits.HitType.Revenge:
                    return 1000;
                case Hits.HitType.LevelUp:
                    return 1500;
                case Hits.HitType.MatchBonus:
                    return 3000;
            }
            return 0;
        }

        public override bool IsOnMyTeam(byte playerID)
        {
            return false;
        }

        public override byte[] GetKillablePlayers()
        {
            return players.ToArray();
        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            KilledPlayer(killerID, killedID, deathType, wasRevenge, true);
        }

        public void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge, bool GiveXP)
        {
            if (killedID == killerID && deathType == KillText.DeathType.Grenade)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (GiveXP == true)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
            }
            else if (deathType != KillText.DeathType.Lava && deathType != KillText.DeathType.Water && deathType != KillText.DeathType.Fall)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = killerID;

                bool gotNimble = false;
                if ((MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).TimeSinceLastKill < 750)
                {
                    gotNimble = true;
                }

                (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddKill();

                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (GiveXP)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
                else if (MinerOfDuty.Session.FindGamerById(killerID).IsLocal)
                {
                    if (GiveXP)
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Kill, 0, Lobby.IsPrivateLobby());
                }


                if (deathType == KillText.DeathType.HeadShot)
                {
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(150);
                    if (scores.ContainsKey(killerID))
                        scores[killerID] += 150;
                }
                else
                {
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(100);
                    if (scores.ContainsKey(killerID)) 
                        scores[killerID] += 100;
                }


                if (killerID == localPlayerID)
                {
                    if (deathType == KillText.DeathType.HeadShot)
                    {
                        AddWorkingHit(Hits.HitType.HeadShot, 150);
                        if (GiveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Headshot, 150, Lobby.IsPrivateLobby());
                    }
                    else if (deathType == KillText.DeathType.Normal || deathType == KillText.DeathType.Knife || deathType == KillText.DeathType.Grenade)
                    {
                        AddWorkingHit(Hits.HitType.NormalKill, 100);
                        if (GiveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, deathType == KillText.DeathType.Normal ? PlayerProfile.KillType.Normal :
                                deathType == KillText.DeathType.Knife ? PlayerProfile.KillType.Knife : PlayerProfile.KillType.Grenade, 100, Lobby.IsPrivateLobby());
                    }
                }

                if (wasRevenge)
                {
                    if (killerID == localPlayerID)
                    {
                        AddWorkingHit(Hits.HitType.Revenge, 50);
                        if (GiveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Revenge, 50, Lobby.IsPrivateLobby());
                    }
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(50);
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                    if (scores.ContainsKey(killerID))
                        scores[killerID] += 50;
                }


                //if (gotNimble)
                //{
                //    if (killerID == localPlayerID)
                //    {
                //       // AddWorkingHit(Hits.HitType.NimbleKill, 50);
                //      //  if(GiveXP)
                //       //     MinerOfDuty.CurrentPlayerProfile.AddXP(75);
                //    }
                //    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(75);
                //    if (scores.ContainsKey(killerID)) 
                //        scores[killerID] += 75;
                //}

            }
            else
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (GiveXP)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
            }
            updateScoreBoardPos = true;
        }


        private enum Status { Winning, Losing, Tieing }
        private Status stat;
        private int pointsBy;
        private int winningPoints;
        private TimeSpan gameOverTime = new TimeSpan(0, 0, 2);
        private bool over = false;
        private bool updateScoreBoardPos;
        protected override void update(GameTime gameTime)
        {


            #region updateScoreboard
            if (updateScoreBoardPos)
            {
                updateScoreBoardPos = false;

                List<byte> open, closed;
                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < players.Count; i++)
                {
                    open.Add(players[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                players = closed.ToArray().ToList();

            #endregion
            }

            if (players[0] == localPlayerID)
            {
                if (players.Count > 1)
                {
                    if (scores[players[1]] < scores[localPlayerID])
                    {
                        //he has less points so we ARE winnig
                        pointsBy = scores[localPlayerID] - scores[players[1]];
                        stat = Status.Winning;
                    }
                    else
                        stat = Status.Tieing;

                }
                else
                    stat = Status.Winning;
                winningPoints = scores[players[0]];
            }
            else
            {
                if (scores[players[0]] > scores[localPlayerID])
                {
                    stat = Status.Losing;
                    pointsBy = scores[players[0]] - scores[localPlayerID];
                    winningPoints = scores[players[0]];
                }
                else
                {
                    stat = Status.Tieing;
                    winningPoints = scores[players[0]];
                }
            }


            if (over == false)
            {
                if (GameHasStarted)
                    timer = timer.Subtract(gameTime.ElapsedGameTime);


                if (timer.TotalSeconds <= 1)
                {
                    timer = new TimeSpan(0, 0, 0);
                    gameOverTime = gameOverTime.Subtract(gameTime.ElapsedGameTime);
                    game.DontUpdatePlayerMovementsTillReset();

                    if (gameOverTime.TotalSeconds <= 0)
                    {
                        int highestIndex = 0;

                        for (int i = 0; i < players.Count; i++)
                        {
                            if (scores[players[i]] > scores[players[highestIndex]])
                            {
                                highestIndex = i;
                            }
                        }

                        if (players[highestIndex] == localPlayerID)//we are the hiehgest
                        {
                            if (players.Count > 1)
                            {
                                if (scores[localPlayerID] == scores[players[1]])
                                {
                                    InvokeGameOverEvent("TIE", Color.Yellow);
                                    if (game.IsCustom == false)
                                    {
                                        MinerOfDuty.CurrentPlayerProfile.AddTie(game.type, 135 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 80), Lobby.IsPrivateLobby());
                                        AddWorkingHit(Hits.HitType.MatchBonus, 135 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 80));
                                    }
                                }
                                else
                                {
                                    InvokeGameOverEvent("VICTORY", Color.DarkGreen);
                                    if (game.IsCustom == false)
                                    {
                                        MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85), Lobby.IsPrivateLobby());
                                        AddWorkingHit(Hits.HitType.MatchBonus, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85));
                                    }
                                }
                            }
                            else
                            {
                                InvokeGameOverEvent("VICTORY", Color.DarkGreen);
                                if (game.IsCustom == false)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85));
                                }
                            }
                        }
                        else if (scores[players[highestIndex]] == scores[localPlayerID])//our score is the same
                        {
                            InvokeGameOverEvent("TIE", Color.Yellow);
                            if (game.IsCustom == false)
                            {
                                MinerOfDuty.CurrentPlayerProfile.AddTie(game.type, 135 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 80), Lobby.IsPrivateLobby());
                                AddWorkingHit(Hits.HitType.MatchBonus, 135 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 80));
                            }
                        }
                        else
                        {
                            InvokeGameOverEvent("DEFEAT", Color.DarkRed);
                            if (game.IsCustom == false)
                            {
                                MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, 100 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 60), Lobby.IsPrivateLobby());
                                AddWorkingHit(Hits.HitType.MatchBonus, 100 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 60));
                            }
                        }
                        over = true;
                    }
                    else
                        over = false;


                }
                else
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (scores[players[i]] >= maxScore)
                        {
                            if (players[i] == localPlayerID)
                            {
                                //win
                                InvokeGameOverEvent("VICTORY", Color.DarkGreen);
                                if (game.IsCustom == false)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85));
                                }
                                over = true;
                            }
                            else
                            {
                                //lose
                                InvokeGameOverEvent("DEFEAT", Color.DarkRed);
                                if (game.IsCustom == false)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, 100 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 60), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, 100 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 60));
                                }
                                over = true;
                            }
                            break;
                        }
                    }
                }

            }
        }


        protected void draw(SpriteBatch sb, bool showXPBAR)
        {

            string text;
            Color color;
            if (stat == Status.Winning)
            {
                text = "WINNING BY " + pointsBy;
                color = Color.Green;
            }
            else if (stat == Status.Tieing)
            {
                text = "TIED";
                color = Color.Yellow;
            }
            else
            {
                text = "LOSING BY " + pointsBy;
                color = Color.Red;
            }


            sb.DrawString(Resources.Font, timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);

            sb.DrawString(Resources.Font, text, new Vector2(975 - (Resources.Font.MeasureString(text).X / 2f), 570 - Resources.Font.LineSpacing), color);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            float x = ((float)scores[localPlayerID] / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            //team A
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                 Color.Green);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 602) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.DarkGreen);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            if (stat == Status.Winning)
                x = ((float)(winningPoints - pointsBy) / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            else
                x = ((float)winningPoints / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                 Color.Red);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 643) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.DarkRed);

            if (showXPBAR)
            {
                sb.Draw(Resources.XPBarTexture, new Vector2(485, 610), Color.White);
                sb.Draw(Resources.XPYellowBarTexture, new Vector2(485, 610), new Rectangle(0, 0, (int)(Resources.XPYellowBarTexture.Width * ((float)MinerOfDuty.CurrentPlayerProfile.XP / (float)MinerOfDuty.CurrentPlayerProfile.XPTillLevel)), Resources.XPYellowBarTexture.Height), Color.White);
            }

            if (timer.TotalSeconds <= 5)
            {
                sb.DrawString(Resources.NameFont, (timer.Seconds).ToString(), new Vector2(642.5f, 362.5f), Color.Red, 0, Resources.NameFont.MeasureString((timer.Seconds).ToString()) / 2f, 1, SpriteEffects.None, 0);
            }
        }

        protected override void draw(SpriteBatch sb)
        {
            draw(sb, true);
        }

        public override void DrawLeadboard(SpriteBatch sb)
        {
            sb.Draw(Resources.ScoreboardBack, Vector2.Zero, Color.White);
            sb.DrawString(Resources.NameFont, "SCORE", new Vector2(725 + 75 - Resources.NameFont.MeasureString("SCORE").X, 100), Color.White);
            sb.DrawString(Resources.NameFont, "KILLS", new Vector2(825 + 50 - 55, 100), Color.White);
            sb.DrawString(Resources.NameFont, "DEATHS", new Vector2(925 + 50 - 35, 100), Color.White);

            sb.DrawString(Resources.Font, "PLAYERS", new Vector2(200, 100), Color.White);
            string text;
            Vector2 startPos = new Vector2(225, 100 + Resources.Font.LineSpacing * 1.5f);
            for (int i = 0; i < players.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(players[i]).IsMutedByLocalUser || (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).IsMuted)
                    sb.Draw(Resources.Muted, startPos - new Vector2(45, 0), Color.White);
                else if (MinerOfDuty.Session.FindGamerById(players[i]).IsTalking)
                    sb.Draw(Resources.IsTalking, startPos - new Vector2(45, 0), Color.White);
                else if (MinerOfDuty.Session.FindGamerById(players[i]).HasVoice && !MinerOfDuty.Session.FindGamerById(players[i]).IsTalking)
                    sb.Draw(Resources.CanTalk, startPos - new Vector2(45, 0), Color.White);

                sb.DrawString(Resources.NameFont, (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Level + " " +
                            ((MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).ClanTag + "]" : "")
                             + MinerOfDuty.Session.FindGamerById(players[i]).Gamertag, startPos, localPlayerID == players[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Score.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(475 + 105 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == players[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Kills.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(575 + 125 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == players[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Deaths.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(700 + 125 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == players[i] ? Color.Yellow : Color.White);
                startPos.Y += Resources.NameFont.LineSpacing;
            }

        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            List<Vector2> spawns = new List<Vector2>();
            Vector2 tmpSpawn;
            for (int i = 0; i < players.Count; i++)
            {
                do
                {
                    tmpSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
                }
                while (CheckSpawns(spawns, ref tmpSpawn) == false);
                spawns.Add(tmpSpawn);

                for (int y = 62; y > 0; y--)
                    if (game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Y] != Block.BLOCKID_AIR)
                    {
                        dict.Add(players[i], new Vector3(tmpSpawn.X, y + 1, tmpSpawn.Y));
                        break;
                    }
            }

            return dict;
        }

        protected bool CheckSpawns(List<Vector2> spawns, ref Vector2 tmpSpawn)
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                if (Vector2.Distance(spawns[i], tmpSpawn) < 20)
                    return false;
            }
            return true;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            Vector2 tmpSpawn;
            do
            {
                tmpSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
            }
            while (AwayFromOthers(ref tmpSpawn, game) == false);

            for (int y = 62; y > 0; y--)
                if (game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Y] != Block.BLOCKID_AIR)
                {
                    return new Vector3(tmpSpawn.X, y + 1, tmpSpawn.Y);
                }

            return new Vector3(tmpSpawn.X, 61, tmpSpawn.Y);
        }

        private bool AwayFromOthers(ref Vector2 pos, MultiplayerGame game)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (game.players[players[i]].dead)
                    continue;
                if (Vector2.Distance(new Vector2(game.players[players[i]].position.X, game.players[players[i]].position.Z), pos) < 20)
                    return false;
            }
            return true;
        }


        public override void MuteTeam()
        {

        }

        public override bool CanMineGoldBlocks()
        {
            return false;
        }

        public override void SendTimeToSend()
        {
            if (GameHasStarted == false)
                return;

            Packet.PacketWriter.Write(Packet.PACKETID_INGAMETIME);
            Packet.PacketWriter.Write((byte)0);
            Packet.PacketWriter.Write(timer.Ticks);

            game.Me.SendData(Packet.PacketWriter, SendDataOptions.Reliable);
        }

        public override void ReciveTimeToSend(byte state, long time)
        {
            timer = new TimeSpan(time);
        }
    }

    public class KTBManager : TeamManager
    {
        public Vector2 CenterPos = new Vector2(64, 64);
        private int maxDis = 50;
        protected float distance = 23;
        protected List<byte> players;
        private Dictionary<byte, int> scores;
        private TimeSpan timer;
        private const int maxScore = 1750;

        public override bool AcceptNewPlayer()
        {
            if (timer.TotalSeconds < 40)
                return false;
            else
                return true;
        }

        public override void ReadInfo(BinaryReader br)
        {
            int times = br.ReadByte();
            for (int i = 0; i < times; i++)
            {
                byte id = br.ReadByte();
                if (MinerOfDuty.Session.FindGamerById(id) != null)
                {
                    GamePlayerStats stats = (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats);
                    if (players.Contains(id) == false)
                        players.Add(id);
                    stats.Score += br.ReadInt32();
                    if (scores.ContainsKey(id) == false)
                        scores.Add(id, stats.Score);
                    else
                        scores[id] += stats.Score;
                    stats.Kills += br.ReadInt32();
                    stats.Deaths += br.ReadInt32();
                }
                else
                {
                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                }
            }
        }

        public override void SaveInfo(BinaryWriter bw)
        {
            byte times = 0;
            for (int i = 0; i < base.PlayingGamers.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(PlayingGamers[i]) != null)
                {
                    times++;
                }
            }

            bw.Write(times);
            for (int i = 0; i < base.PlayingGamers.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(PlayingGamers[i]) != null)
                {
                    GamePlayerStats stats = MinerOfDuty.Session.FindGamerById(PlayingGamers[i]).Tag as GamePlayerStats;
                    bw.Write(PlayingGamers[i]);
                    //bw.Write(stats.Score);
                    bw.Write(scores[PlayingGamers[i]]);
                    bw.Write(stats.Kills);
                    bw.Write(stats.Deaths);
                }
            }
        }

        public KTBManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {
            this.players = new List<byte>();
            this.scores = new Dictionary<byte, int>();

            updateScoreBoardPos = true;
            timer = new TimeSpan(0, 15, 0);
        }

        public override void MixUpTeams()
        {
        }

        public override TeamManager.Team CalculateWhichTeamForPlayer(byte id)
        {
            return Team.None;
        }

        public override TeamManager.Team WhatTeam(byte playerID)
        {
            return Team.None;
        }

        protected override byte addPlayer(byte id, TeamManager.Team team)
        {
            players.Add(id);
            scores.Add(id, 0);

            return id;
        }

        protected override byte removePlayer(byte id)
        {
            if (players.Contains(id))
                players.Remove(id);
            if (scores.ContainsKey(id))
                scores.Remove(id);

            if (GameHasStarted && players.Count == 1)
            {
                InvokeGameOverEvent("VICTORY", Color.Green);
                MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 0, Lobby.IsPrivateLobby());
            }
            return id;
        }

        public override bool IsOnMyTeam(byte playerID)
        {
            return false;
        }

        public override byte[] GetKillablePlayers()
        {
            return players.ToArray();
        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            KilledPlayer(killerID, killedID, deathType, wasRevenge, true);
        }

        public void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge, bool GiveXP)
        {
            if (killedID == killerID && deathType == KillText.DeathType.Grenade)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (GiveXP == true)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
            }
            else if (deathType != KillText.DeathType.Lava && deathType != KillText.DeathType.Water && deathType != KillText.DeathType.Fall)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = killerID;

                bool gotNimble = false;
                if ((MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).TimeSinceLastKill < 750)
                {
                    gotNimble = true;
                }

                (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddKill();

                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (GiveXP)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
                else if (MinerOfDuty.Session.FindGamerById(killerID).IsLocal)
                {
                    if (GiveXP)
                        MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Kill, 0, Lobby.IsPrivateLobby());
                }


                if (deathType == KillText.DeathType.HeadShot)
                {
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(150);
                }
                else
                {
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(100);
                }


                if (killerID == localPlayerID)
                {
                    if (deathType == KillText.DeathType.HeadShot)
                    {
                        AddWorkingHit(Hits.HitType.HeadShot, 150);
                        if (GiveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Headshot, 150, Lobby.IsPrivateLobby());
                    }
                    else if (deathType == KillText.DeathType.Normal || deathType == KillText.DeathType.Knife || deathType == KillText.DeathType.Grenade)
                    {
                        AddWorkingHit(Hits.HitType.NormalKill, 100);
                        if (GiveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, deathType == KillText.DeathType.Normal ? PlayerProfile.KillType.Normal :
                                deathType == KillText.DeathType.Knife ? PlayerProfile.KillType.Knife : PlayerProfile.KillType.Grenade, 100, Lobby.IsPrivateLobby());
                    }
                }

                if (wasRevenge)
                {
                    if (killerID == localPlayerID)
                    {
                        AddWorkingHit(Hits.HitType.Revenge, 50);
                        if (GiveXP)
                            MinerOfDuty.CurrentPlayerProfile.AddKill(game.type, PlayerProfile.KillType.Revenge, 50, Lobby.IsPrivateLobby());
                    }
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(50);
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                }

                //if (gotNimble)
                //{
                //    if (killerID == localPlayerID)
                //    {
                //        // AddWorkingHit(Hits.HitType.NimbleKill, 50);
                //        //  if(GiveXP)
                //        //     MinerOfDuty.CurrentPlayerProfile.AddXP(75);
                //    }
                //    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(75);
                //}

            }
            else
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                if (MinerOfDuty.Session.FindGamerById(killedID).IsLocal)
                {
                    if (GiveXP)
                    {
                        MinerOfDuty.CurrentPlayerProfile.AddDeath(game.type, deathType, 0, Lobby.IsPrivateLobby());
                    }
                }
            }
            updateScoreBoardPos = true;
        }

        private enum Status { Winning, Losing, Tieing }
        private Status stat;
        private int pointsBy;
        private int winningPoints;
        private TimeSpan gameOverTime = new TimeSpan(0, 0, 2);
        private bool over = false;
        private bool updateScoreBoardPos;
        protected override void update(GameTime gameTime)
        {


            #region updateScoreboard
            if (updateScoreBoardPos)
            {
                updateScoreBoardPos = false;

                List<byte> open, closed;
                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < players.Count; i++)
                {
                    open.Add(players[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if (scores[(open[indexHigh])] < scores[(open[i])])
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                players = closed.ToArray().ToList();

            #endregion
            }

            if (players[0] == localPlayerID)
            {
                if (players.Count > 1)
                {
                    if (scores[players[1]] < scores[localPlayerID])
                    {
                        //he has less points so we ARE winnig
                        pointsBy = scores[localPlayerID] - scores[players[1]];
                        stat = Status.Winning;
                    }
                    else
                        stat = Status.Tieing;

                }
                else
                    stat = Status.Winning;
                winningPoints = scores[players[0]];
            }
            else
            {
                if (scores[players[0]] > scores[localPlayerID])
                {
                    stat = Status.Losing;
                    pointsBy = scores[players[0]] - scores[localPlayerID];
                    winningPoints = scores[players[0]];
                }
                else
                {
                    stat = Status.Tieing;
                    winningPoints = scores[players[0]];
                }
            }


            if (over == false)
            {
                if (GameHasStarted)
                {
                    timer = timer.Subtract(gameTime.ElapsedGameTime);

                    GiveKingPoints(gameTime);
                }


                if (timer.TotalSeconds <= 1)
                {
                    timer = new TimeSpan(0, 0, 0);
                    gameOverTime = gameOverTime.Subtract(gameTime.ElapsedGameTime);
                    game.DontUpdatePlayerMovementsTillReset();

                    if (gameOverTime.TotalSeconds <= 0)
                    {
                        int highestIndex = 0;

                        for (int i = 0; i < players.Count; i++)
                        {
                            if (scores[players[i]] > scores[players[highestIndex]])
                            {
                                highestIndex = i;
                            }
                        }

                        if (players[highestIndex] == localPlayerID)//we are the hiehgest
                        {
                            if (players.Count > 1)
                            {
                                if (scores[localPlayerID] == scores[players[1]])
                                {
                                    InvokeGameOverEvent("TIE", Color.Yellow);
                                    if (game.IsCustom == false)
                                    {
                                        MinerOfDuty.CurrentPlayerProfile.AddTie(game.type, 135 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 80), Lobby.IsPrivateLobby());
                                        AddWorkingHit(Hits.HitType.MatchBonus, 135 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 80));
                                    }
                                }
                                else
                                {
                                    InvokeGameOverEvent("VICTORY", Color.DarkGreen);
                                    if (game.IsCustom == false)
                                    {
                                        MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85), Lobby.IsPrivateLobby());
                                        AddWorkingHit(Hits.HitType.MatchBonus, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85));
                                    }
                                }
                            }
                            else
                            {
                                InvokeGameOverEvent("VICTORY", Color.DarkGreen);
                                if (game.IsCustom == false)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85));
                                }
                            }
                        }
                        else if (scores[players[highestIndex]] == scores[localPlayerID])//our score is the same
                        {
                            InvokeGameOverEvent("TIE", Color.Yellow);
                            if (game.IsCustom == false)
                            {
                                MinerOfDuty.CurrentPlayerProfile.AddTie(game.type, 135 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 80), Lobby.IsPrivateLobby());
                                AddWorkingHit(Hits.HitType.MatchBonus, 135 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 80));
                            }
                        }
                        else
                        {
                            InvokeGameOverEvent("DEFEAT", Color.DarkRed);
                            if (game.IsCustom == false)
                            {
                                MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, 100 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 60), Lobby.IsPrivateLobby());
                                AddWorkingHit(Hits.HitType.MatchBonus, 100 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 60));
                            }
                        }
                        over = true;
                    }
                    else
                        over = false;


                }
                else
                {

                    for (int i = 0; i < players.Count; i++)
                    {
                        if (scores[players[i]] >= maxScore)
                        {
                            if (players[i] == localPlayerID)
                            {
                                //win
                                InvokeGameOverEvent("VICTORY", Color.DarkGreen);
                                if (game.IsCustom == false)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddVictory(game.type, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, 140 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 85));
                                }
                                over = true;
                            }
                            else
                            {
                                //lose
                                InvokeGameOverEvent("DEFEAT", Color.DarkRed);
                                if (game.IsCustom == false)
                                {
                                    MinerOfDuty.CurrentPlayerProfile.AddDefeat(game.type, 100 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 60), Lobby.IsPrivateLobby());
                                    AddWorkingHit(Hits.HitType.MatchBonus, 100 + ((MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).Kills * 60));
                                }
                                over = true;
                            }
                            break;
                        }
                    }
                }

            }
        }

        private string kingName = "";
        private TimeSpan lastTime;
        private void GiveKingPoints(GameTime gameTime)
        {
            if (game.GameOver)
                return;

            lastTime = lastTime.Add(gameTime.ElapsedGameTime);

            bool givePoints = false;
            if (lastTime.TotalSeconds > 5)
            {
                givePoints = true;
                lastTime = TimeSpan.Zero;
            }

            List<byte> possilbeKings = new List<byte>();

            //check if only one player is king
            foreach (byte playerID in players)
            {
                if (game.players[playerID].dead == false && Vector2.DistanceSquared(new Vector2(game.players[playerID].position.X, game.players[playerID].position.Z), CenterPos) < (distance * distance))
                {
                    possilbeKings.Add(playerID);
                }
            }

            kingName = "";
            if (possilbeKings.Count == 1)
            {
                kingName = MinerOfDuty.Session.FindGamerById(possilbeKings[0]).Gamertag;
                if (givePoints && game.Me.IsHost)
                    Packet.WriteKingOfHill(game.Me, 50, possilbeKings[0]); //scores[possilbeKings[0]] += 50;

                if (possilbeKings[0] == localPlayerID)
                {
                    kingName = "YOU";
                    if (givePoints)
                    {
                        //AddWorkingHit(Hits.HitType.King, 50);
                       // MinerOfDuty.CurrentPlayerProfile.AddKingScore(game.type, 50, Lobby.IsPrivateLobby());
                    }
                }
                if (givePoints)
                    updateScoreBoardPos = true;
                

            }
        }

        public void GiveKingPoints(byte id, ushort points)
        {
            if (id == game.Me.Id)
            {
                AddWorkingHit(Hits.HitType.King, points);
                MinerOfDuty.CurrentPlayerProfile.AddKingScore(game.type, points, Lobby.IsPrivateLobby());
            }

            if (scores.ContainsKey(id) == false)
                scores.Add(id, 0);

            scores[id] += points;

            updateScoreBoardPos = true;
        }

        protected void draw(SpriteBatch sb, bool showXPBAR)
        {

            string text;
            Color color;
            if (stat == Status.Winning)
            {
                text = "WINNING BY " + pointsBy;
                color = Color.Green;
            }
            else if (stat == Status.Tieing)
            {
                text = "TIED";
                color = Color.Yellow;
            }
            else
            {
                text = "LOSING BY " + pointsBy;
                color = Color.Red;
            }

            sb.DrawString(Resources.NameFont, kingName == "" ? "" : kingName == "YOU" ? "YOU ARE KING" : kingName + " IS KING", new Vector2(175, 100), Color.Yellow);

            sb.DrawString(Resources.Font, timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);

            sb.DrawString(Resources.Font, text, new Vector2(975 - (Resources.Font.MeasureString(text).X / 2f), 570 - Resources.Font.LineSpacing), color);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            float x = ((float)scores[localPlayerID] / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            //team A
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                 Color.Green);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 602) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.DarkGreen);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            if (stat == Status.Winning)
                x = ((float)(winningPoints - pointsBy) / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            else
                x = ((float)winningPoints / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                 Color.Red);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 643) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.DarkRed);

            if (showXPBAR)
            {
                sb.Draw(Resources.XPBarTexture, new Vector2(485, 610), Color.White);
                sb.Draw(Resources.XPYellowBarTexture, new Vector2(485, 610), new Rectangle(0, 0, (int)(Resources.XPYellowBarTexture.Width * ((float)MinerOfDuty.CurrentPlayerProfile.XP / (float)MinerOfDuty.CurrentPlayerProfile.XPTillLevel)), Resources.XPYellowBarTexture.Height), Color.White);
            }

            if (timer.TotalSeconds <= 5)
            {
                sb.DrawString(Resources.NameFont, (timer.Seconds).ToString(), new Vector2(642.5f, 362.5f), Color.Red, 0, Resources.NameFont.MeasureString((timer.Seconds).ToString()) / 2f, 1, SpriteEffects.None, 0);
            }
        }

        protected override void draw(SpriteBatch sb)
        {
            draw(sb, true);
        }

        public override void DrawLeadboard(SpriteBatch sb)
        {
            sb.Draw(Resources.ScoreboardBack, Vector2.Zero, Color.White);
            sb.DrawString(Resources.NameFont, "SCORE", new Vector2(725 + 75 - Resources.NameFont.MeasureString("SCORE").X, 100), Color.White);
            sb.DrawString(Resources.NameFont, "KILLS", new Vector2(825 + 50 - 55, 100), Color.White);
            sb.DrawString(Resources.NameFont, "DEATHS", new Vector2(925 + 50 - 35, 100), Color.White);

            sb.DrawString(Resources.Font, "PLAYERS", new Vector2(200, 100), Color.White);
            string text;
            Vector2 startPos = new Vector2(225, 100 + Resources.Font.LineSpacing * 1.5f);
            for (int i = 0; i < players.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(players[i]).IsMutedByLocalUser || (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).IsMuted)
                    sb.Draw(Resources.Muted, startPos - new Vector2(45, 0), Color.White);
                else if (MinerOfDuty.Session.FindGamerById(players[i]).IsTalking)
                    sb.Draw(Resources.IsTalking, startPos - new Vector2(45, 0), Color.White);
                else if (MinerOfDuty.Session.FindGamerById(players[i]).HasVoice && !MinerOfDuty.Session.FindGamerById(players[i]).IsTalking)
                    sb.Draw(Resources.CanTalk, startPos - new Vector2(45, 0), Color.White);

                sb.DrawString(Resources.NameFont, (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Level + " " +
                            ((MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).ClanTag + "]" : "")
                             + MinerOfDuty.Session.FindGamerById(players[i]).Gamertag, startPos, localPlayerID == players[i] ? Color.Yellow : Color.White);
                text = scores[players[i]].ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(475 + 105 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == players[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Kills.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(575 + 125 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == players[i] ? Color.Yellow : Color.White);
                text = (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Deaths.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(700 + 125 - Resources.NameFont.MeasureString(text).X, 0), localPlayerID == players[i] ? Color.Yellow : Color.White);
                startPos.Y += Resources.NameFont.LineSpacing;
            }

        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
           

            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            List<Vector2> spawns = new List<Vector2>();
            Vector2 tmpSpawn;
            for (int i = 0; i < players.Count; i++)
            {
                do
                {
                    tmpSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
                }
                while ((Vector2.DistanceSquared(CenterPos, tmpSpawn) > (maxDis*maxDis) && CheckSpawns(spawns, ref tmpSpawn))
                    == false);
                

                for (int y = 62; y > 0; y--)
                    if (game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Y] != Block.BLOCKID_AIR)
                    {
                        if (game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Y] == Block.BLOCKID_WATER)
                            i--;
                        else
                        {
                            spawns.Add(tmpSpawn);
                            dict.Add(players[i], new Vector3(tmpSpawn.X, y + 1, tmpSpawn.Y));
                        }
                        break;
                    }
            }

            return dict;
        }

        protected bool CheckSpawns(List<Vector2> spawns, ref Vector2 tmpSpawn)
        {
            for (int i = 0; i < spawns.Count; i++)
            {
                if (Vector2.Distance(spawns[i], tmpSpawn) < 15)
                    return false;
            }
            return true;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            Vector2 tmpSpawn;
            do
            {
                tmpSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
            }
            while ((Vector2.DistanceSquared(CenterPos, tmpSpawn) > (maxDis * maxDis) && AwayFromOthers(ref tmpSpawn, game))
                    == false);
            for (int y = 62; y > 0; y--)
                if (game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Y] != Block.BLOCKID_AIR)
                {
                    return new Vector3(tmpSpawn.X, y + 1, tmpSpawn.Y);
                }

            return new Vector3(tmpSpawn.X, 61, tmpSpawn.Y);
        }

        private bool AwayFromOthers(ref Vector2 pos, MultiplayerGame game)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (game.players[players[i]].dead)
                    continue;
                if (Vector2.Distance(new Vector2(game.players[players[i]].position.X, game.players[players[i]].position.Z), pos) < 20)
                    return false;
            }
            return true;
        }

        protected override void CurrentPlayerProfile_LevelUpEvent()
        {
            if (workingHit == null)
            {
                workingHit = new Hits(Hits.HitType.LevelUp, 0);
            }
            else
            {
                hits.Enqueue(new Hits(Hits.HitType.LevelUp, 0));
            }
        }

        public override void DrawHits(SpriteBatch sb)
        {
            if (workingHit != null)
                workingHit.Draw(sb);
        }

        public override void MuteTeam()
        {
        }

        protected override int GetTimeWithHit(TeamManager.Hits.HitType type)
        {
            switch (type)
            {
                case Hits.HitType.HeadShot:
                case Hits.HitType.NimbleKill:
                case Hits.HitType.NormalKill:
                case Hits.HitType.Revenge:
                    return 1000;
                case Hits.HitType.LevelUp:
                    return 1500;
                case Hits.HitType.MatchBonus:
                    return 3000;
                case Hits.HitType.King:
                    return 1000;
            }
            return 0;
        }

        public override void SendTimeToSend()
        {
            if (GameHasStarted == false)
                return;

            Packet.PacketWriter.Write(Packet.PACKETID_INGAMETIME);
            Packet.PacketWriter.Write((byte)0);
            Packet.PacketWriter.Write(timer.Ticks);

            game.Me.SendData(Packet.PacketWriter, SendDataOptions.Reliable);
        }

        public override void ReciveTimeToSend(byte state, long time)
        {
            timer = new TimeSpan(time);
        }
    }

    public class SwarmManager : TeamManager
    {
        private TimeSpan timer;
        private const int maxRound = 15;
        private int round = 1;
        private bool over = false;

        private bool showRoundStartedText;
        private TimeSpan showRoundStartedTime;

        private bool showRoundOverText;
        private TimeSpan showRoundOverTime;

        private bool inBetween;
        private TimeSpan inBetweenRoundTime;

        private bool hasEverStarted = false;

        public bool IsInRound { get { if (inBetween == false) return true; else return false; } }
        public int Round { get { return round; } }

        public delegate void RoundOver();
        public delegate void RoundStart(int round);
        public event RoundStart RoundStartEvent;
        public event RoundOver RoundOverEvent;

        private bool updateScoreBoardPos;

        protected List<byte> players;

        public override void ReadInfo(BinaryReader br)
        {
            
        }

        public override void SaveInfo(BinaryWriter bw)
        {
            
        }

        public override bool AcceptNewPlayer()
        {
            throw new NotImplementedException();
        }

        public SwarmManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {
            players = new List<byte>();

            timer = new TimeSpan(0, 6, 00);
            updateScoreBoardPos = true;
        }

        public override TeamManager.Team WhatTeam(byte playerID)
        {
            return Team.None;
        }

        public override void MixUpTeams()
        {
            
        }

        public override TeamManager.Team CalculateWhichTeamForPlayer(byte id)
        {
            return Team.None;
        }

        protected override byte addPlayer(byte id, TeamManager.Team team)
        {
            players.Add(id);
            return id;
        }

        protected override byte removePlayer(byte id)
        {
            players.Remove(id);
            return id;
        }

        public void AddKill(byte id)
        {
            (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).AddKill();
            updateScoreBoardPos = true;
        }

        public void AddDeath(byte id)
        {
            (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).AddDeath();
            updateScoreBoardPos = true;
        }

        public void InvokeGameOver()
        {
            if (over == false)
            {
                InvokeGameOverEvent("DEFEAT", Color.DarkRed);
                over = true;
            }
        }


        public void AddZombieKnifed(byte id)
        {
            (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).AddScore(10);
            updateScoreBoardPos = true;
        }

        public void AddZombieGrenaded(byte id, int amountHits)
        {
            (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).AddScore(20 * amountHits);
            updateScoreBoardPos = true;
        }

        public void AddZombieShot(byte id, PlayerBody.Hit type)
        {
            switch (type)
            {
                case PlayerBody.Hit.Arm:
                case PlayerBody.Hit.Leg:
                    (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).AddScore(20);
                    updateScoreBoardPos = true;
                    break;
                case PlayerBody.Hit.Body:
                    (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).AddScore(25);
                    updateScoreBoardPos = true;
                    break;
                case PlayerBody.Hit.Head:
                    (MinerOfDuty.Session.FindGamerById(id).Tag as GamePlayerStats).AddScore(35);
                    updateScoreBoardPos = true;
                    break;
            }
        }

        /// <summary>
        /// Used for local player, returns amount of $$ to add and adds a hit score thing
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int AddZombieKnifed()
        {
            (MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).AddScore(10);
            AddWorkingHit(Hits.HitType.Blank, 10);
            updateScoreBoardPos = true;
            return 10;
        }

        /// <summary>
        /// Used for local player, returns amount of $$ to add and adds a hit score thing
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int AddZombieGrenaded(int amountHits)
        {
            (MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).AddScore(20 * amountHits);
            AddWorkingHit(Hits.HitType.Blank, 20 * amountHits);
            updateScoreBoardPos = true;
            return 20 * amountHits;
        }

        /// <summary>
        /// Used for local player, returns amount of $$ to add and adds a hit score thing
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int AddZombieShot(PlayerBody.Hit type)
        {
            switch (type)
            {
                case PlayerBody.Hit.Arm:
                case PlayerBody.Hit.Leg:
                    (MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).AddScore(20);
                    AddWorkingHit(Hits.HitType.Blank, 20);
                    updateScoreBoardPos = true;
                    return 20;
                case PlayerBody.Hit.Body:
                    (MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).AddScore(25);
                    AddWorkingHit(Hits.HitType.Blank, 20);
                    updateScoreBoardPos = true;
                    return 20;
                case PlayerBody.Hit.Head:
                    (MinerOfDuty.Session.FindGamerById(localPlayerID).Tag as GamePlayerStats).AddScore(35);
                    AddWorkingHit(Hits.HitType.Blank, 25);
                    updateScoreBoardPos = true;
                    return 25;
                default:
                    return 0;
            }
        }



        public void Pause()
        {
            pause = true;
        }
        public bool pause;
        public void UnPause()
        {
            pause = false;
        }


        protected override void update(GameTime gameTime)
        {
            #region updateScoreboard
            if (updateScoreBoardPos)
            {
                updateScoreBoardPos = false;

                List<byte> open, closed;
                open = new List<byte>();
                closed = new List<byte>();
                for (int i = 0; i < players.Count; i++)
                {
                    open.Add(players[i]);
                }

                while (open.Count > 0)
                {
                    int indexHigh = 0;
                    for (int i = 0; i < open.Count; i++)
                    {
                        if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                        {
                            indexHigh = i;
                        }
                    }
                    closed.Add(open[indexHigh]);
                    open.RemoveAt(indexHigh);
                }

                players = closed.ToArray().ToList();

            #endregion
            }

            if (over == false)
                if (GameHasStarted)
                {
                    if (hasEverStarted == false)
                    {
                        round = 1;
                        timer = new TimeSpan(0, 2, 00);//200
                        showRoundStartedText = true;
                        showRoundStartedTime = new TimeSpan(0, 0, 5);
                        hasEverStarted = true;
                        if (RoundStartEvent != null)
                            RoundStartEvent.Invoke(round);
                    }

                    if (game.goldBlocks.Count == 0)
                    {
                        InvokeGameOverEvent("DEFEAT", Color.DarkRed);
                        over = true;
                    }
                    else
                    {
                        if (inBetween == false)
                        {
                            if (pause == false)
                                timer = timer.Subtract(gameTime.ElapsedGameTime);
                            if (timer.TotalSeconds <= 0)
                            {
                                (MinerOfDuty.Session.AllGamers[0].Tag as GamePlayerStats).AddScore(150 * game.goldBlocks.Count);
                                AddWorkingHit(Hits.HitType.RoundBonus, 150 * game.goldBlocks.Count);
                                updateScoreBoardPos = true;

                                if (RoundOverEvent != null)
                                    RoundOverEvent.Invoke();
                                inBetween = true;
                                showRoundOverText = true;
                                showRoundOverTime = new TimeSpan(0, 0, 5);
                                inBetweenRoundTime = new TimeSpan(0, 0, 45);
                                round++;


                                (game as SwarmGame).SwarmManager.StoreMenu.cash += 150 * game.goldBlocks.Count;

                                if (round >= maxRound + 1)
                                {
                                    InvokeGameOverEvent("VICTORY", Color.DarkGreen);
                                    over = true;
                                }
                            }

                            if (showRoundStartedText)
                            {
                                if (pause == false)
                                    showRoundStartedTime = showRoundStartedTime.Subtract(gameTime.ElapsedGameTime);
                                if (showRoundStartedTime.TotalSeconds <= 0)
                                    showRoundStartedText = false;
                            }
                        }
                        else
                        {
                            if (showRoundOverText)
                            {
                                if (pause == false)
                                    showRoundOverTime = showRoundOverTime.Subtract(gameTime.ElapsedGameTime);
                                if (showRoundOverTime.TotalSeconds <= 0)
                                    showRoundOverText = false;
                            }

                            if (pause == false)
                                inBetweenRoundTime = inBetweenRoundTime.Subtract(gameTime.ElapsedGameTime);
                            if (inBetweenRoundTime.TotalSeconds <= 0)
                            {
                                inBetween = false;
                                timer = new TimeSpan(0, 2, 15);
                                showRoundStartedText = true;
                                showRoundStartedTime = new TimeSpan(0, 0, 5);
                                if (RoundStartEvent != null)
                                    RoundStartEvent.Invoke(round);
                            }

                        }
                    }

                }
        }

        protected override void draw(SpriteBatch sb)
        {
            if (showRoundOverText)
            {
                sb.DrawString(Resources.Font, "ROUND " + (round - 1) + " OVER", new Vector2(640, 400), Color.Yellow, 0, Resources.Font.MeasureString("ROUND " + (round - 1) + " OVER") / 2f, 1, SpriteEffects.None, 0);
            }
            else if (showRoundStartedText)
            {
                sb.DrawString(Resources.Font, "ROUND " + round, new Vector2(640, 400), Color.Yellow, 0, Resources.Font.MeasureString("ROUND " + round) / 2f, 1, SpriteEffects.None, 0);
            }

            if (!inBetween)
                sb.DrawString(Resources.Font, timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);
            else
                sb.DrawString(Resources.Font, inBetweenRoundTime.Minutes + ":" + (inBetweenRoundTime.Seconds > 9 ? inBetweenRoundTime.Seconds.ToString() : "0" + inBetweenRoundTime.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(inBetweenRoundTime.Minutes + ":" + (inBetweenRoundTime.Seconds > 9 ? inBetweenRoundTime.Seconds.ToString() : "0" + inBetweenRoundTime.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);
            if (!inBetween)
                sb.DrawString(Resources.Font, "ROUND " + round, new Vector2(975 - (Resources.Font.MeasureString("ROUND " + round).X / 2f), 570 - (1 * Resources.Font.LineSpacing)), Color.White);
            else
                sb.DrawString(Resources.Font, "NEXT ROUND " + round, new Vector2(975 - (Resources.Font.MeasureString("NEXT ROUND " + round).X / 2f), 570 - (1 * Resources.Font.LineSpacing)), Color.White);

            if (inBetween == false)
            {
                sb.DrawString(Resources.NameFont, "GOLD BLOCKS LEFT:", new Vector2(1000, 250), Color.White, 0, Resources.NameFont.MeasureString("GOLD BLOCKS LEFT:") / 2f, 1, SpriteEffects.None, 0);
                if (game.goldBlocks != null)
                    sb.DrawString(Resources.NameFont, game.goldBlocks.Count.ToString(), new Vector2(1000, 250 + (Resources.NameFont.LineSpacing * 1.2f)), Color.White, 0, Resources.NameFont.MeasureString(game.goldBlocks.Count.ToString()) / 2f, 1, SpriteEffects.None, 0);
            }

        }

        public void DrawTime(SpriteBatch sb)
        {

            if (!inBetween)
                sb.DrawString(Resources.Font, timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);
            else
                sb.DrawString(Resources.Font, inBetweenRoundTime.Minutes + ":" + (inBetweenRoundTime.Seconds > 9 ? inBetweenRoundTime.Seconds.ToString() : "0" + inBetweenRoundTime.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(inBetweenRoundTime.Minutes + ":" + (inBetweenRoundTime.Seconds > 9 ? inBetweenRoundTime.Seconds.ToString() : "0" + inBetweenRoundTime.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);
            if (!inBetween)
                sb.DrawString(Resources.Font, "ROUND " + round, new Vector2(975 - (Resources.Font.MeasureString("ROUND " + round).X / 2f), 570 - (1 * Resources.Font.LineSpacing)), Color.White);
            else
                sb.DrawString(Resources.Font, "NEXT ROUND " + round, new Vector2(975 - (Resources.Font.MeasureString("NEXT ROUND " + round).X / 2f), 570 - (1 * Resources.Font.LineSpacing)), Color.White);

        }

        public override void DrawLeadboard(SpriteBatch sb)
        {
            sb.Draw(Resources.ScoreboardBack, Vector2.Zero, Color.White);

            sb.DrawString(Resources.NameFont, "SCORE", new Vector2(725 - Resources.NameFont.MeasureString("SCORE").X, 175), Color.White);
            sb.DrawString(Resources.NameFont, "KILLS", new Vector2(825 - 15, 175), Color.White);

            sb.DrawString(Resources.Font, "PLAYERS", new Vector2(225, 175), Color.White);
            string text;
            Vector2 startPos = new Vector2(250, 175 + Resources.Font.LineSpacing * 1.5f);
            for (int i = 0; i < players.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(players[i]) == null)
                    continue;

                sb.DrawString(Resources.NameFont, (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Level + " " + MinerOfDuty.Session.FindGamerById(players[i]).Gamertag, startPos, Color.White);
                text = (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Score.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(475 - Resources.NameFont.MeasureString(text).X, 0), Color.White);
                text = (MinerOfDuty.Session.FindGamerById(players[i]).Tag as GamePlayerStats).Kills.ToString();
                sb.DrawString(Resources.NameFont, text, startPos + new Vector2(615 - Resources.NameFont.MeasureString(text).X, 0), Color.White);
                startPos.Y += Resources.NameFont.LineSpacing;
            }

        }

        #region unused
        public override bool IsOnMyTeam(byte playerID)
        {
            return true;
        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            Vector2 spawn = new Vector2(64, 64);

            for (int i = 0; i < players.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    ran.Next((int)MathHelper.Clamp(spawn.X - 10, 4, 124), (int)MathHelper.Clamp(spawn.X + 10, 4, 124)),
                     ran.Next((int)MathHelper.Clamp(spawn.Y - 10, 4, 124), (int)MathHelper.Clamp(spawn.Y + 10, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < players.Count; j++)
                {
                    if (dict.ContainsKey(players[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[players[j]].X, dict[players[j]].Z), tmpSpwn) < 2)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    for (int y = 20; y > 0; y--)
                        if (game.Terrain.blocks[(int)tmpSpwn.X, y, (int)tmpSpwn.Y] != Block.BLOCKID_AIR)
                        {
                            dict.Add(players[i], new Vector3(tmpSpwn.X, y + 1, tmpSpwn.Y));
                            break;
                        }
                }
                else
                {
                    i--;
                }

            }

            return dict;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            Vector2 tmpSpawn;
            do
            {
                tmpSpawn = new Vector2(ran.Next(4, 124), ran.Next(4, 124));
            }
            while (AwayFromTeam(ref tmpSpawn, game) == false);

            for (int y = 62; y > 0; y--)
                if (game.Terrain.blocks[(int)tmpSpawn.X, y, (int)tmpSpawn.Y] != Block.BLOCKID_AIR)
                {
                    return new Vector3(tmpSpawn.X, y + 1, tmpSpawn.Y);
                }
            return new Vector3(tmpSpawn.X, 61, tmpSpawn.Y);
        }

        private bool AwayFromTeam(ref Vector2 pos, MultiplayerGame game)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (game.players[players[i]].dead)
                    continue;
                if (Vector2.Distance(new Vector2(game.players[players[i]].position.X, game.players[players[i]].position.Z), pos) < 5)
                    return false;
            }
            return true;
        }

        protected override void CurrentPlayerProfile_LevelUpEvent()
        {

        }

        public override void MuteTeam()
        {

        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {

        }

        public override byte[] GetKillablePlayers()
        {
            return new byte[] { };
        }

        public override void DrawHits(SpriteBatch sb)
        {
            //for gameover use apparentyl
        }
        #endregion


        public override bool CanPlaceBlocks()
        {
            if (IsInRound)
                return false;
            else
                return base.CanPlaceBlocks();
        }

        public override bool CanMineGoldBlocks()
        {
            return false;
        }

        protected override int GetTimeWithHit(TeamManager.Hits.HitType type)
        {
            switch (type)
            {
                case Hits.HitType.Blank:
                    return 375;
                case Hits.HitType.RoundBonus:
                    return 1000;
            }
            return 375;
        }

        public override void SendTimeToSend()
        {
            if (GameHasStarted == false)
                return;

            Packet.PacketWriter.Write(Packet.PACKETID_INGAMETIME);
            Packet.PacketWriter.Write((inBetween ? (byte)1 : (byte)0));
            Packet.PacketWriter.Write((inBetween ? inBetweenRoundTime : timer).Ticks);
            game.Me.SendData(Packet.PacketWriter, SendDataOptions.Reliable);
        }

        public override void ReciveTimeToSend(byte state, long time)
        {
            if (state == 1)
                inBetweenRoundTime = new TimeSpan(time);
            else
                timer = new TimeSpan(time);
        }
    }

     public class CustomSMManager : SwarmManager
    {
        public CustomSMManager( out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base( out gamerLeft, localPlayer)
        {

        }

        public Vector3 SpawnPoint;

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            for (int i = 0; i < players.Count; i++)
            {
                Vector2 tmpSpwn;
                tmpSpwn = new Vector2(
                    ran.Next((int)MathHelper.Clamp(SpawnPoint.X - 10, 4, 124), (int)MathHelper.Clamp(SpawnPoint.X + 10, 4, 124)),
                     ran.Next((int)MathHelper.Clamp(SpawnPoint.Z - 10, 4, 124), (int)MathHelper.Clamp(SpawnPoint.Z + 10, 4, 124)));


                bool isOkay = true;
                for (int j = 0; j < players.Count; j++)
                {
                    if (dict.ContainsKey(players[j]))
                    {
                        if (Vector2.Distance(new Vector2(dict[players[j]].X, dict[players[j]].Z), tmpSpwn) < 2)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {

                    dict.Add(players[i], new Vector3(tmpSpwn.X, SpawnPoint.Y , tmpSpwn.Y));
                    //break;

                }
                else
                {
                    i--;
                }

            }

            return dict;
        }

    }

    public class CustomTDMManager : TeamDeathmatchManager
    {
        public Vector3[] TeamASpawnPoint;
        public Vector3[] TeamBSpawnPoint;

        public CustomTDMManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {

        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            if (killedID == killerID && deathType == KillText.DeathType.Grenade)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
            }
            else if (deathType != KillText.DeathType.Lava && deathType != KillText.DeathType.Water && deathType != KillText.DeathType.Fall)
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = killerID;

                bool gotNimble = false;
                if ((MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).TimeSinceLastKill < 750)
                {
                    gotNimble = true;
                }

                (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddKill();


                bool teamA = false;
                for (int i = 0; i < TeamA.Count; i++)
                {
                    if (TeamA[i] == killerID)
                    {
                        teamA = true;
                        break;
                    }
                }

                if (teamA)
                    teamAScore += 100;
                else
                    teamBScore += 100;


                if (deathType == KillText.DeathType.HeadShot)
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(150);
                else
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(100);


                if (killerID == localPlayerID)
                {
                    if (deathType == KillText.DeathType.HeadShot)
                    {
                        AddWorkingHit(Hits.HitType.HeadShot, 150);
                    }
                    else if (deathType == KillText.DeathType.Normal || deathType == KillText.DeathType.Knife || deathType == KillText.DeathType.Grenade || deathType == KillText.DeathType.Grenade)
                    {
                        AddWorkingHit(Hits.HitType.NormalKill, 100);
                    }
                }

                if (wasRevenge)
                {
                    if (killerID == localPlayerID)
                    {
                        AddWorkingHit(Hits.HitType.Revenge, 50);
                    }
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(50);
                    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                }

                //if (gotNimble)
                //{
                //    if (killerID == localPlayerID)
                //    {
                //        AddWorkingHit(Hits.HitType.NimbleKill, 50);
                //    }
                //    (MinerOfDuty.Session.FindGamerById(killerID).Tag as GamePlayerStats).AddScore(75);
                //}

            }
            else
            {
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).WhoKilledMeLast = 0;
                (MinerOfDuty.Session.FindGamerById(killedID).Tag as GamePlayerStats).AddDeath();
            }
            updateScoreBoard = true;
        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            for (int i = 0; i < TeamA.Count; i++)
            {
                int spawn = ran.Next(0, TeamASpawnPoint.Length);

                Vector3 tmpSpwn;
                tmpSpwn = new Vector3(
                    ran.Next((int)MathHelper.Clamp(TeamASpawnPoint[spawn].X - 2, 4, 124), (int)MathHelper.Clamp(TeamASpawnPoint[spawn].X + 2, 4, 124)),
                    TeamASpawnPoint[spawn].Y,
                     ran.Next((int)MathHelper.Clamp(TeamASpawnPoint[spawn].Z - 2, 4, 124), (int)MathHelper.Clamp(TeamASpawnPoint[spawn].Z + 2, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamA.Count; j++)
                {
                    if (dict.ContainsKey(TeamA[j]))
                    {
                        if (Vector3.Distance(dict[TeamA[j]], tmpSpwn) < 1f)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    dict.Add(TeamA[i], tmpSpwn);
                }
                else
                {
                    i--;
                }

            }

            for (int i = 0; i < TeamB.Count; i++)
            {
                int spawn = ran.Next(0, TeamBSpawnPoint.Length);

                Vector3 tmpSpwn;
                tmpSpwn = new Vector3(
                    ran.Next((int)MathHelper.Clamp(TeamBSpawnPoint[spawn].X - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpawnPoint[spawn].X + 2, 4, 124)),
                    TeamBSpawnPoint[spawn].Y,
                     ran.Next((int)MathHelper.Clamp(TeamBSpawnPoint[spawn].Z - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpawnPoint[spawn].Z + 2, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamB.Count; j++)
                {
                    if (dict.ContainsKey(TeamB[j]))
                    {
                        if (Vector3.Distance(dict[TeamB[j]], tmpSpwn) < 1f)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    dict.Add(TeamB[i], tmpSpwn);
                }
                else
                {
                    i--;
                }

            }

            return dict;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            int spawn = ran.Next(0, localIsOnTeamA ? TeamASpawnPoint.Length : TeamBSpawnPoint.Length);
            Vector3 point = (localIsOnTeamA ? TeamASpawnPoint[spawn] : TeamBSpawnPoint[spawn]);

            return new Vector3(
               ran.Next((int)MathHelper.Clamp(point.X - 2, 4, 124), (int)MathHelper.Clamp(point.X + 2, 4, 124)),
               point.Y,
                ran.Next((int)MathHelper.Clamp(point.Z - 2, 4, 124), (int)MathHelper.Clamp(point.Z + 2, 4, 124)));
        }
        protected override void draw(SpriteBatch sb)
        {
            string text;
            Color color;
            if (localIsOnTeamA)
            {
                if (teamAScore > teamBScore)
                {
                    text = "WINNING BY " + (teamAScore - teamBScore);
                    color = Color.Green;
                }
                else if (teamAScore == teamBScore)
                {
                    text = "TIED";
                    color = Color.Yellow;
                }
                else
                {
                    text = "LOSING BY " + (teamBScore - teamAScore);
                    color = Color.Red;
                }
            }
            else
            {
                if (teamAScore < teamBScore)
                {
                    text = "WINNING BY " + (teamBScore - teamAScore);
                    color = Color.Green;
                }
                else if (teamAScore == teamBScore)
                {
                    text = "TIED";
                    color = Color.Yellow;
                }
                else
                {
                    text = "LOSING BY " + (teamAScore - teamBScore);
                    color = Color.Red;
                }
            }

            sb.DrawString(Resources.Font, timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString()), new Vector2(975 - (Resources.Font.MeasureString(timer.Minutes + ":" + (timer.Seconds > 9 ? timer.Seconds.ToString() : "0" + timer.Seconds.ToString())).X / 2f), 570 - (2.25f * Resources.Font.LineSpacing)), Color.White);

            sb.DrawString(Resources.Font, text, new Vector2(975 - (Resources.Font.MeasureString(text).X / 2f), 570 - Resources.Font.LineSpacing), color);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            float x = ((float)teamAScore / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            //team A
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 605) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                localIsOnTeamA ? Color.Green : Color.Red);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 602) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);


            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100, 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), Color.White);

            x = ((float)teamBScore / (float)maxScore) * Resources.TeamScoreBarTexture.Width;
            sb.Draw(Resources.TeamScoreBarTexture, new Vector2(1100 + (Resources.TeamScoreBarTexture.Width - x), 645) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height),
                new Rectangle((int)(Resources.TeamScoreBarTexture.Width - x), 0, (int)x, Resources.TeamScoreBarTexture.Height),
                !localIsOnTeamA ? Color.Green : Color.Red);
            sb.Draw(Resources.EndTildeTexture, new Vector2(1100 - 8 + (Resources.TeamScoreBarTexture.Width - x), 643) - new Vector2(Resources.TeamScoreBarTexture.Width, Resources.TeamScoreBarTexture.Height), !localIsOnTeamA ? Color.DarkGreen : Color.DarkRed);


        }
    }

    public class CustomFFAManager : FFAManager
    {
        public Vector3[] playerSpawns;

        public CustomFFAManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {

        }

        public override void SaveInfo(BinaryWriter bw)
        {
            base.SaveInfo(bw);
            bw.Write((byte)playerSpawns.Length);
            for(int i = 0; i < playerSpawns.Length; i++)
                bw.Write(playerSpawns[i]);
        }

        public override void ReadInfo(BinaryReader br)
        {
            base.ReadInfo(br);
            playerSpawns = new Vector3[br.ReadByte()];
            for (int i = 0; i < playerSpawns.Length; i++)
                playerSpawns[i] = br.ReadVector3();
        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            base.KilledPlayer(killerID, killedID, deathType,wasRevenge, false);
        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            for (int i = 0; i < players.Count; i++)
            {
                int spawn = ran.Next(0, playerSpawns.Length);
                dict.Add(players[i], new Vector3(playerSpawns[spawn].X + ran.Next(-3, 4), playerSpawns[spawn].Y, playerSpawns[spawn].Z + ran.Next(-3, 4)));
            }

            return dict;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            int spawn = ran.Next(0, playerSpawns.Length);
            return new Vector3(playerSpawns[spawn].X + ran.Next(-3, 4), playerSpawns[spawn].Y, playerSpawns[spawn].Z + ran.Next(-3, 4));
        }

        protected override void draw(SpriteBatch sb)
        {
            base.draw(sb, false);
        }

    }

    public class CustomKBManager : KTBManager
    {
        public Vector3[] playerSpawns;

        public CustomKBManager(out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        {
            
        }

        public override void SaveInfo(BinaryWriter bw)
        {
            base.SaveInfo(bw);
            bw.Write((byte)playerSpawns.Length);
            for (int i = 0; i < playerSpawns.Length; i++)
                bw.Write(playerSpawns[i]);

            bw.Write(CenterPos);
            bw.Write(distance);
        }

        public override void ReadInfo(BinaryReader br)
        {
            base.ReadInfo(br);
            playerSpawns = new Vector3[br.ReadByte()];
            for (int i = 0; i < playerSpawns.Length; i++)
                playerSpawns[i] = br.ReadVector3();

            CenterPos = br.ReadVector2();
            distance = br.ReadSingle();
        }

        public void SetData(int kingPX, int kingPZ, int range)
        {
            CenterPos = new Vector2(kingPX, kingPZ);
            distance = range ;// 2f;
        }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            base.KilledPlayer(killerID, killedID, deathType, wasRevenge, false);
        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            for (int i = 0; i < players.Count; i++)
            {
                int spawn = ran.Next(0, playerSpawns.Length);
                dict.Add(players[i], new Vector3(playerSpawns[spawn].X + ran.Next(-3, 4), playerSpawns[spawn].Y, playerSpawns[spawn].Z + ran.Next(-3, 4)));
            }

            return dict;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            int spawn = ran.Next(0, playerSpawns.Length);
            return new Vector3(playerSpawns[spawn].X + ran.Next(-3, 4), playerSpawns[spawn].Y, playerSpawns[spawn].Z + ran.Next(-3, 4));
        }

        protected override void draw(SpriteBatch sb)
        {
            base.draw(sb, false);
        }

    }

    public class CustomSearchNMiner : SearchNMineManager
    {
        public Vector3[] TeamASpawnPoint;
        public Vector3[] TeamBSpawnPoint;
        public Vector3[] GoldBlocks;

        public CustomSearchNMiner( out EventHandler<GamerLeftEventArgs> gamerLeft, byte localPlayer)
            : base(out gamerLeft, localPlayer)
        { }

        public override void KilledPlayer(byte killerID, byte killedID, KillText.DeathType deathType, bool wasRevenge)
        {
            base.KilledPlayer(killerID, killedID, deathType, wasRevenge, false);
        }

        public override Dictionary<byte, Vector3> GetTeamSpawn(Random ran, MultiplayerGame game)
        {
            game.SpawnGoldBlocks(new List<Vector3>(GoldBlocks));

            Dictionary<byte, Vector3> dict = new Dictionary<byte, Vector3>();

            for (int i = 0; i < TeamA.Count; i++)
            {
                int spawn = ran.Next(0, TeamASpawnPoint.Length);

                Vector3 tmpSpwn;
                tmpSpwn = new Vector3(
                    ran.Next((int)MathHelper.Clamp(TeamASpawnPoint[spawn].X - 2, 4, 124), (int)MathHelper.Clamp(TeamASpawnPoint[spawn].X + 2, 4, 124)),
                    TeamASpawnPoint[spawn].Y,
                     ran.Next((int)MathHelper.Clamp(TeamASpawnPoint[spawn].Z - 2, 4, 124), (int)MathHelper.Clamp(TeamASpawnPoint[spawn].Z + 2, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamA.Count; j++)
                {
                    if (dict.ContainsKey(TeamA[j]))
                    {
                        if (Vector3.Distance(dict[TeamA[j]], tmpSpwn) < 1f)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    dict.Add(TeamA[i], tmpSpwn);
                }
                else
                {
                    i--;
                }

            }

            for (int i = 0; i < TeamB.Count; i++)
            {
                int spawn = ran.Next(0, TeamBSpawnPoint.Length);

                Vector3 tmpSpwn;
                tmpSpwn = new Vector3(
                    ran.Next((int)MathHelper.Clamp(TeamBSpawnPoint[spawn].X - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpawnPoint[spawn].X + 2, 4, 124)),
                    TeamBSpawnPoint[spawn].Y,
                     ran.Next((int)MathHelper.Clamp(TeamBSpawnPoint[spawn].Z - 2, 4, 124), (int)MathHelper.Clamp(TeamBSpawnPoint[spawn].Z + 2, 4, 124)));

                bool isOkay = true;
                for (int j = 0; j < TeamB.Count; j++)
                {
                    if (dict.ContainsKey(TeamB[j]))
                    {
                        if (Vector3.Distance(dict[TeamB[j]], tmpSpwn) < 1f)
                        {
                            isOkay = false;
                            break;
                        }
                    }
                }

                if (isOkay)
                {
                    dict.Add(TeamB[i], tmpSpwn);
                }
                else
                {
                    i--;
                }

            }

            return dict;
        }

        public override Vector3 GetReSpawnPoint(Random ran, MultiplayerGame game)
        {
            int spawn = ran.Next(0, localIsOnTeamA ? TeamASpawnPoint.Length : TeamBSpawnPoint.Length);
            Vector3 point = (localIsOnTeamA ? TeamASpawnPoint[spawn] : TeamBSpawnPoint[spawn]);

            return new Vector3(
               ran.Next((int)MathHelper.Clamp(point.X - 2, 4, 124), (int)MathHelper.Clamp(point.X + 2, 4, 124)),
               point.Y,
                ran.Next((int)MathHelper.Clamp(point.Z - 2, 4, 124), (int)MathHelper.Clamp(point.Z + 2, 4, 124)));
        }

        protected override void draw(SpriteBatch sb)
        {
            base.draw(sb, false);
        }

    }

}
