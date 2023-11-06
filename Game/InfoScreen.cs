using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game
{

    public interface IText
    {
        void Draw(SpriteBatch sb, ref Vector2 pos, Color color);
    }

    public class InfoText : IText
    {
        private string text;

        public InfoText(string text)
        {
            this.text = text;
        }

        public void Draw(SpriteBatch sb, ref Vector2 pos, Color color)
        {
            sb.DrawString(Resources.Font, text, pos, Color.White);
        }
    }

    public class KillText : IText
    {
        private string PlayerAName;
        private string PlayerBName;
        public enum DeathType { HeadShot, Normal, Lava, Fall, Knife, Zombie, Grenade, Tool, Water }//grenade, fall dmg
        private DeathType deathType;
        private Color doerColor, reciverColor;

        public KillText(string doer, Color doerColor, string reciever, Color reciverColor, DeathType deathType)
        {
            PlayerAName = doer;
            PlayerBName = reciever;
            this.doerColor = doerColor;
            this.reciverColor = reciverColor;
            this.deathType = deathType;

            if (deathType == DeathType.Lava)
            {
                this.doerColor = Color.OrangeRed;
            }
            else if (deathType == DeathType.Fall)
            {
                this.doerColor = Color.LightGray;
            }
        }

        public void Draw(SpriteBatch sb, ref Vector2 pos, Color color)
        {
            sb.DrawString(Resources.Font, PlayerAName, pos, doerColor);
            sb.Draw(((deathType == DeathType.Normal || deathType == DeathType.Zombie) ? Resources.NormalDeath : deathType == DeathType.HeadShot ? Resources.HeadShotDeath : deathType == DeathType.Fall ? Resources.FallDeath : deathType == DeathType.Knife ? Resources.KnifeDeath : deathType == DeathType.Grenade  ? Resources.GrenadeDeath : deathType == DeathType.Lava ? Resources.LavaDeath : Resources.WaterDeath), pos + new Vector2(Resources.Font.MeasureString(PlayerAName).X + 2, 0), Color.White);
            sb.DrawString(Resources.Font, PlayerBName, pos + new Vector2(Resources.Font.MeasureString(PlayerAName).X + 37, 0), reciverColor);
        }
    }

    public class EditorInfoScreen
    {
        private IText[] kills;
        private int[] killFadeTimes;

        public EditorInfoScreen()
        {
            kills = new IText[5];
            killFadeTimes = new int[5];
        }


        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < 5; i++)
            {
                if (killFadeTimes[i] > 0)
                {
                    killFadeTimes[i] -= gameTime.ElapsedGameTime.Milliseconds;
                    if (killFadeTimes[i] < 0)
                    {
                        killFadeTimes[i] = 0;
                        kills[i] = null;
                    }
                }
            }
        }

        public void AddKillText(IText text)
        {
            killFadeTimes[4] = killFadeTimes[3];
            kills[4] = kills[3];

            killFadeTimes[3] = killFadeTimes[2];
            kills[3] = kills[2];

            killFadeTimes[2] = killFadeTimes[1];
            kills[2] = kills[1];

            killFadeTimes[1] = killFadeTimes[0];
            kills[1] = kills[0];

            killFadeTimes[0] = 2500;
            kills[0] = text;
        }

        public void Draw(SpriteBatch sb)
        {
            Vector2 pos = new Vector2(60, 400);
            for (int i = 0; i < 5; i++)
            {
                if (kills[i] == null)
                    break;

                kills[i].Draw(sb, ref pos, Color.IndianRed);
                pos.Y += Resources.Font.LineSpacing;
            }
        }

    }

    public class InfoScreen
    {
        public static Texture2D ShotFromTexture;
        public Vector2 CenterPos = new Vector2(ShotFromTexture.Width / 2, ShotFromTexture.Height / 2);
        public Vector2 CenterPos2 = new Vector2(Resources.GrenadeAim.Width / 2, Resources.GrenadeAim.Height / 2);
        private MultiplayerGame game;

        private IText[] kills;
        private int[] killFadeTimes;

        private float shotFromRot;
        private Vector3 shotFrom;
        private float time;
        private Vector2 shotFromDrawPosition;

        public MiniMap miniMap;

        public InfoScreen(MultiplayerGame game)
        {
            this.game = game;
            kills = new IText[5];
            killFadeTimes = new int[5];
            miniMap = new MiniMap(game);
            grenadeDraws = new List<Vector2>();
            grenades = new List<GrenadeManager.Grenade>();
            grenadeRots = new List<float>();
        }

        public void AddKillText(IText text)
        {
            killFadeTimes[4] = killFadeTimes[3];
            kills[4] = kills[3];

            killFadeTimes[3] = killFadeTimes[2];
            kills[3] = kills[2];

            killFadeTimes[2] = killFadeTimes[1];
            kills[2] = kills[1];

            killFadeTimes[1] = killFadeTimes[0];
            kills[1] = kills[0];

            killFadeTimes[0] = 2500;
            kills[0] = text;
        }

        public void Update(GameTime gameTime, Player localPlayer)
        {
            for (int i = 0; i < 5; i++)
            {
                if (killFadeTimes[i] > 0)
                {
                    killFadeTimes[i] -= gameTime.ElapsedGameTime.Milliseconds;
                    if (killFadeTimes[i] < 0)
                    {
                        killFadeTimes[i] = 0;
                        kills[i] = null;
                    }
                }
            }

            if (time > 0)
            {
                UpdateShotFrom(localPlayer);
                time -= gameTime.ElapsedGameTime.Milliseconds;
            }

            miniMap.Update(gameTime);
            UpdateGrenade(gameTime);
        }

        private Vector2 dir;
        private Vector2 shotFrom2, playerPos;
        private void UpdateShotFrom(Player localPlayer)
        {
            playerPos.X = game.player.position.X;
            playerPos.Y = game.player.position.Z;
            shotFrom2.X = shotFrom.X;
            shotFrom2.Y = shotFrom.Z;

            Vector2.Subtract(ref shotFrom2, ref playerPos, out dir);
            dir.Normalize();


            shotFromRot = (float)Math.Atan(dir.Y / dir.X);//migh wanna test Atan2
            if (dir.X < 0 && dir.Y < 0)
            {
                shotFromRot += MathHelper.Pi;
            }
            else if (dir.X < 0)
            {
                shotFromRot += MathHelper.Pi;
            }
            else if (dir.Y < 0)
            {
                shotFromRot += MathHelper.Pi + MathHelper.Pi;
            }

            shotFromRot += localPlayer.leftRightRot;

            shotFromDrawPosition.X = 640;
            shotFromDrawPosition.Y = 360;

            dir = Vector2.Transform(dir, Matrix.CreateRotationZ(localPlayer.leftRightRot));

            shotFromDrawPosition.X += dir.X * (1280f / 2.5f);
            shotFromDrawPosition.Y += dir.Y * (720f / 2.5f);
        }

        public void ClearShotFrom()
        {
            time = 0;
        }

        public void AddShotFrom(ref Vector3 bulletPos)
        {
            shotFrom = bulletPos;
            time = 2000;
        }

        private void UpdateGrenade(GameTime gameTime)
        {
            for (int i = 0; i < grenades.Count; i++)
            {
                if (grenades[i].Dead)
                {
                    grenades.RemoveAt(i);
                    grenadeDraws.RemoveAt(i);
                    grenadeRots.RemoveAt(i);
                    i--;
                    continue;
                }

                playerPos.X = game.player.position.X;
                playerPos.Y = game.player.position.Z;
                shotFrom2.X = grenades[i].Position.X;
                shotFrom2.Y = grenades[i].Position.Z;

                Vector2.Subtract(ref shotFrom2, ref playerPos, out dir);
                dir.Normalize();

                grenadeRots[i] = (float)Math.Atan2(dir.Y, dir.X);
                grenadeRots[i] += game.player.leftRightRot;


                dir = Vector2.Transform(dir, Matrix.CreateRotationZ(game.player.leftRightRot));

                grenadeDraws[i] = new Vector2(640 + dir.X * (1280f / 2.5f),
                    320 + dir.Y * (720f / 2.5f));
            }
        }

        private List<GrenadeManager.Grenade> grenades;
        private List<Vector2> grenadeDraws;
        private List<float> grenadeRots;
        public void AddGrenade(GrenadeManager.Grenade grenade)
        {
            if (grenade.ID == GrenadeType.GRENADE_FRAG)
            {
                grenades.Add(grenade);
                grenadeDraws.Add(new Vector2(-100,-100));
                grenadeRots.Add(0);
            }
        }

        private static Vector2 MiniMapPosition = new Vector2(1000, 100);
        public void Draw(SpriteBatch sb)
        {
            Vector2 pos = new Vector2(60, 400);
            for (int i = 0; i < 5; i++)
            {
                if (kills[i] == null)
                    break;

                kills[i].Draw(sb, ref pos, Color.IndianRed);
                pos.Y += Resources.Font.LineSpacing;
            }

            if (time > 0)
                sb.Draw(ShotFromTexture, shotFromDrawPosition, null, Color.White, shotFromRot, CenterPos, 1, SpriteEffects.None, 0);

            for (int i = 0; i < grenadeDraws.Count; i++)
                sb.Draw(Resources.GrenadeAim, grenadeDraws[i], null, 
                    new Color(Color.White.ToVector4() * (1 - MathHelper.Clamp(Vector3.Distance(game.player.position, grenades[i].Position) / 15f, 0, 1))),
                    grenadeRots[i], CenterPos2 / 2f, 1, SpriteEffects.None, 0);

        }

        public void DrawMinimap(SpriteBatch sb)
        {
            miniMap.Draw(sb, ref MiniMapPosition);
        }
    }

    public class MiniMap
    {
        private Vector2 PlayerDotTextureOrigin =
            new Vector2(Resources.PlayerDotTexture.Width / 2, Resources.PlayerDotTexture.Height / 2);

        private MultiplayerGame game;

        private Dictionary<short, Vector2> zombies;
        private Dictionary<byte, Vector2> playerPoints;
        private Dictionary<byte, int> blinkTimes;

        public MiniMap(MultiplayerGame game)
        {
            this.game = game;
            zombies = new Dictionary<short, Vector2>();
            playerPoints = new Dictionary<byte, Vector2>();
            blinkTimes = new Dictionary<byte, int>();
            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                blinkTimes.Add(MinerOfDuty.Session.AllGamers[i].Id, 1501);
            goldBlocks = new List<Vector2>();
        }

        public void EllaFired(byte id)
        {
            blinkTimes[id] = 0;
        }

        public List<Vector2> goldBlocks;

        private Vector2 tmp, playerPos;
        public void Update(GameTime gameTime)
        {
            playerPos.X = game.player.position.X;
            playerPos.Y = game.player.position.Z;

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                if (game.players.ContainsKey(MinerOfDuty.Session.AllGamers[i].Id) == false)
                    continue;

                tmp.X = game.players[MinerOfDuty.Session.AllGamers[i].Id].position.X;
                tmp.Y = game.players[MinerOfDuty.Session.AllGamers[i].Id].position.Z;
                tmp.X -= playerPos.X;
                tmp.Y -= playerPos.Y;


                float x = tmp.X;
                tmp.X = (float)(x * Math.Cos(game.player.leftRightRot) - tmp.Y * Math.Sin(game.player.leftRightRot));
                tmp.Y = (float)(x * Math.Sin(game.player.leftRightRot) + tmp.Y * Math.Cos(game.player.leftRightRot));

                //now the orgin is fixed at us
                tmp.X += 50;
                tmp.Y += 50;

                if (playerPoints.ContainsKey(MinerOfDuty.Session.AllGamers[i].Id) == false)
                    playerPoints.Add(MinerOfDuty.Session.AllGamers[i].Id, tmp);
                else
                    playerPoints[MinerOfDuty.Session.AllGamers[i].Id] = tmp;
            }

            if (game.goldBlocks.Count > 0)
            {
                while (goldBlocks.Count < game.goldBlocks.Count)
                {
                    goldBlocks.Add(Vector2.Zero);
                }
                while (goldBlocks.Count > game.goldBlocks.Count)
                {
                    goldBlocks.RemoveAt(0);
                }
                for (int i = 0; i < goldBlocks.Count; i++)
                {
                    tmp.X = game.goldBlocks[i].X;
                    tmp.Y = game.goldBlocks[i].Z;
                    tmp.X -= playerPos.X;
                    tmp.Y -= playerPos.Y;


                    float x = tmp.X;
                    tmp.X = (float)(x * Math.Cos(game.player.leftRightRot) - tmp.Y * Math.Sin(game.player.leftRightRot));
                    tmp.Y = (float)(x * Math.Sin(game.player.leftRightRot) + tmp.Y * Math.Cos(game.player.leftRightRot));

                    //now the orgin is fixed at us
                    tmp.X += 50;
                    tmp.Y += 50;

                    goldBlocks[i] = tmp;
                }
            }

            if (game is SwarmGame)
            {
                ISwarmieManager sm = (game as SwarmGame).SwarmManager;
                ISwarmie[] fs = sm.GetSwarmies();

                for (int i = 0; i < fs.Length; i++)
                {
                    if (fs[i] == null)
                    {
                        continue;
                    }

                    if (fs[i].Dead)
                    {
                        if (zombies.ContainsKey(fs[i].ID) == false)
                        {
                            zombies.Remove(fs[i].ID);
                        }
                        continue;
                    }



                    tmp.X = fs[i].Position.X;
                    tmp.Y = fs[i].Position.Z;
                    tmp.X -= playerPos.X;
                    tmp.Y -= playerPos.Y;


                    float x = tmp.X;
                    tmp.X = (float)(x * Math.Cos(game.player.leftRightRot) - tmp.Y * Math.Sin(game.player.leftRightRot));
                    tmp.Y = (float)(x * Math.Sin(game.player.leftRightRot) + tmp.Y * Math.Cos(game.player.leftRightRot));

                    //now the orgin is fixed at us
                    tmp.X += 50;
                    tmp.Y += 50;

                    if (zombies.ContainsKey(fs[i].ID) == false)
                        zombies.Add(fs[i].ID, tmp);
                    else
                        zombies[fs[i].ID] = tmp;
                }
            }

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                if (blinkTimes.ContainsKey(MinerOfDuty.Session.AllGamers[i].Id))
                {
                    if (blinkTimes[MinerOfDuty.Session.AllGamers[i].Id] < 1500)
                        blinkTimes[MinerOfDuty.Session.AllGamers[i].Id] += gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                    blinkTimes.Add(MinerOfDuty.Session.AllGamers[i].Id, 0);

        }

        private static readonly Rectangle rect = new Rectangle(0, 0, 100, 100);
        public void Draw(SpriteBatch sb, ref Vector2 drawPos)
        {

            sb.Draw(Resources.MiniMapTexture, drawPos, new Rectangle((int)playerPos.X, (int)playerPos.Y, 100, 100), Color.White);//, game.player.leftRightRot, Vector2.Zero, 1, SpriteEffects.None, 0);
            if (game is SwarmGame)
            {
                ISwarmie[] swarmies = (game as SwarmGame).SwarmManager.GetSwarmies();
                ISwarmie s;
                for (int i = 0; i < swarmies.Length; i++)
                {
                    s = swarmies[i];
                    if (s != null && s.Dead == false)
                        if (zombies.ContainsKey(s.ID))
                            if (rect.Contains((int)zombies[s.ID].X, (int)zombies[s.ID].Y))
                            {
                                sb.Draw(Resources.PlayerDotTexture, zombies[s.ID] + drawPos, null, Color.Red, 0, PlayerDotTextureOrigin, .75f, SpriteEffects.None, 0);
                            }
                }
            }
            for (int i = 0; i < goldBlocks.Count; i++)
            {
                if (rect.Contains((int)goldBlocks[i].X, (int)goldBlocks[i].Y))
                    sb.Draw(Resources.PlayerDotTexture, goldBlocks[i] + drawPos, null, Color.Gold,
                                0, PlayerDotTextureOrigin, .9f, SpriteEffects.None, 0);
            }

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                if (playerPoints.ContainsKey(MinerOfDuty.Session.AllGamers[i].Id))
                    if (rect.Contains((int)playerPoints[MinerOfDuty.Session.AllGamers[i].Id].X, (int)playerPoints[MinerOfDuty.Session.AllGamers[i].Id].Y))
                    {
                        if (MinerOfDuty.Session.AllGamers[i].IsLocal)
                            sb.Draw(Resources.PlayerDotTexture, playerPoints[MinerOfDuty.Session.AllGamers[i].Id] + drawPos, null, Color.Yellow, 0, PlayerDotTextureOrigin, 1, SpriteEffects.None, 0);
                        else if (game.TeamManager.IsOnMyTeam(MinerOfDuty.Session.AllGamers[i].Id) == false && blinkTimes[MinerOfDuty.Session.AllGamers[i].Id] < 1500)
                            sb.Draw(Resources.PlayerDotTexture, playerPoints[MinerOfDuty.Session.AllGamers[i].Id] + drawPos, null, game.TeamManager.IsOnMyTeam(MinerOfDuty.Session.AllGamers[i].Id) ? Color.Green : Color.Red,
                                0, PlayerDotTextureOrigin, .9f, SpriteEffects.None, 0);
                        else if (game.TeamManager.IsOnMyTeam(MinerOfDuty.Session.AllGamers[i].Id))
                            sb.Draw(Resources.PlayerDotTexture, playerPoints[MinerOfDuty.Session.AllGamers[i].Id] + drawPos, null, game.TeamManager.IsOnMyTeam(MinerOfDuty.Session.AllGamers[i].Id) ? Color.Green : Color.Red,
                            0, PlayerDotTextureOrigin, .9f, SpriteEffects.None, 0);
                    }
            }

            sb.Draw(Resources.MiniMapBorderTexture, drawPos - new Vector2(3, 3), Color.White);

        }

    }
}
