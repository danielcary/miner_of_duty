using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using Miner_Of_Duty.Game.Networking;

namespace Miner_Of_Duty.Game
{
    public class GrenadeManager
    {
        public class Grenade
        {
            public static GrenadeManager gm;
            public static MultiplayerGame game;
            private static BoundingBox grenadeBox = new BoundingBox(new Vector3(-.075f), new Vector3(.075f));

            public Vector3 Position;
            private Vector3 moveDir;
            private bool jumping;
            public byte ID;
            private int life;
            public NetworkGamer owner;
            public bool Dead { get; private set; }

            public Grenade(ref Vector3 pos, byte id, float leftRight, float upDown, int life, NetworkGamer owner)
            {
                Position = pos;
                this.ID = id;
                moveDir = Vector3.TransformNormal(Vector3.Forward, Matrix.CreateRotationX(upDown) * Matrix.CreateRotationY(leftRight));
                this.owner = owner;
                Dead = false;
                jumping = true;
                this.life = life;
            }


            public void ReuseGrenade(ref Vector3 pos, byte id, float leftRight, float upDown, int life, NetworkGamer owner)
            {
                Position = pos;

                this.ID = id;
                moveDir = Vector3.TransformNormal(Vector3.Forward, Matrix.CreateRotationX(upDown) * Matrix.CreateRotationY(leftRight));
                this.owner = owner;
                Dead = false;
                jumping = true;
                this.life = life;
            }


            private bool onGround = true;
            private static Vector3 translation, dir;
            private static BoundingBox test, bb;
            private static readonly Vector3 MaxVelocity = new Vector3(4, 4f, 4);
            private static readonly Vector3 MinVelocity = new Vector3(-4, -4f, -4);
            private const float speed = 15;
            private Vector3 velocity = Vector3.Zero;
            private float timeInAir;
            public void Update(GameTime gameTime)
            {
                if ((life += gameTime.ElapsedGameTime.Milliseconds) >= GrenadeType.GrenadeTypes[ID].LifeSpan)
                {
                    Dead = true;
                    gm.AddExplosion(this);
                    return;
                }

                if (jumping)
                {
                    velocity.X += moveDir.X * (speed + 10)  * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    velocity.Y += (moveDir.Y + 1) * 2 * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity    
                    velocity.Z += moveDir.Z * (speed + 10)  * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                    velocity.Y += -1f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity

                translation.X = MathHelper.Clamp(velocity.X, MinVelocity.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                velocity.X -= translation.X;

                translation.Y = MathHelper.Clamp(velocity.Y, MinVelocity.Y * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.Y * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                velocity.Y -= translation.Y;

                translation.Z = MathHelper.Clamp(velocity.Z, MinVelocity.Z * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.Z * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                velocity.Z -= translation.Z;

                bb.Min.X += Position.X;
                bb.Min.Y += Position.Y;
                bb.Min.Z += Position.Z;
                bb.Max.X += Position.X;
                bb.Max.Y += Position.Y;
                bb.Max.Z += Position.Z;

                //y cehck
                dir.X = 0;
                dir.Y = translation.Y;
                dir.Z = 0;
                test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
                if (!game.GetTerrain.CheckForCollisionGrenade(ref test, ref Position))
                {
                    bb.Min += dir;
                    bb.Max += dir;
                    Position += dir;
                    onGround = false;
                }
                else
                    onGround = true;

                if (onGround == false)
                {
                    timeInAir += .5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (jumping)
                        velocity.Y += -.06f * speed * 3 * timeInAir; //gravity
                    else
                        velocity.Y += -.06f * speed * timeInAir; //gravity
                }
                else
                {
                    jumping = false;
                    timeInAir = 0;
                }

                if (translation.X != 0)
                {
                    dir.X = translation.X;
                    dir.Y = 0;
                    dir.Z = 0;
                    test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
                    if (!game.GetTerrain.CheckForCollisionGrenade(ref test, ref Position))
                    {
                        bb.Min += dir;
                        bb.Max += dir;
                        Position += dir;
                    }
                    else
                        moveDir.X *= -1;
                }

                if (translation.Z != 0)
                {
                    dir.X = 0;
                    dir.Y = 0;
                    dir.Z = translation.Z;
                    test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
                    if (!game.GetTerrain.CheckForCollisionGrenade(ref test, ref Position))
                    {
                        bb.Min += dir;
                        bb.Max += dir;
                        Position += dir;
                    }
                    else
                        moveDir.Z *= -1;
                }

                bb.Min.X = -.075f;
                bb.Min.Y = -.075f;
                bb.Min.Z = -.075f;
                bb.Max.X = .075f;
                bb.Max.Y = .075f;
                bb.Max.Z = .075f;

            }

            public void Render(Camera camera)
            {
                foreach (ModelMesh mesh in (ID == GrenadeType.GRENADE_FRAG ? Resources.FragModel : Resources.SmokeFlashModel).Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["Texture0"].SetValue(ID == GrenadeType.GRENADE_FRAG ? Resources.FragModelTexture : ID == GrenadeType.GRENADE_SMOKE 
                            ? Resources.SmokeModelTexture : Resources.FlashModelTexture);

                        float rot = 0;
                        float rot2 = 0;
                        float rot3 = 0;

                        if (onGround == false)
                        {
                            rot = (float)(.5f * Math.Sin(((MathHelper.Pi * 2) / 250f) * life) + .5f);
                            rot2 = (float)(.5f * Math.Sin(((MathHelper.Pi * 2) / 250f) * life + 300) + .5f);
                            rot3 = (float)(.5f * Math.Cos(((MathHelper.Pi * 2) / 250f) * life + 300) + .5f);
                        }
                        
                        effect.Parameters["World"].SetValue(Matrix.CreateRotationX(rot) * Matrix.CreateRotationY(rot2) * Matrix.CreateRotationZ(rot3) * Matrix.CreateTranslation(Position));
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }

        }

        private Grenade[] grenades;

        public GrenadeManager(MultiplayerGame game)
        {
            Grenade.game = game;
            Grenade.gm = this;

            grenades = new Grenade[30];
        }

        public void AddGrenade(Vector3 position, byte id, float leftRightRot, float upDownRot, int life, NetworkGamer owner)
        {
            for (int i = 0; i < grenades.Length; i++)
            {
                if (grenades[i] == null)
                {
                    grenades[i] = new Grenade(ref position, id, leftRightRot, upDownRot, life, owner);
                    Grenade.game.InfoScreen.AddGrenade(grenades[i]);
                    break;
                }
                else if (grenades[i].Dead)
                {
                    grenades[i].ReuseGrenade(ref position, id, leftRightRot, upDownRot, life, owner);
                    Grenade.game.InfoScreen.AddGrenade(grenades[i]);
                    break;
                }
            }


        }

        private Grenade g;
        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < grenades.Length; i++)
            {
                g = grenades[i];
                if (g != null && g.Dead == false)
                    g.Update(gameTime);
            }
        }

        public void Render(Camera cam)
        {
            for (int i = 0; i < grenades.Length; i++)
            {
                g = grenades[i];
                if (g != null && g.Dead == false)
                    g.Render(cam);
            }
        }

        public void AddExplosion(Grenade sender)
        {
            float volume = MathHelper.Lerp(0, 1, MathHelper.Clamp(1 - ((Vector3.Distance(Grenade.game.player.position, sender.Position)) / 100f), 0, 1));
            if (sender.ID == GrenadeType.GRENADE_SMOKE)
                Audio.PlaySound(Audio.SOUND_SMOKEGRENADE, volume);
            else
                Audio.PlaySound(Audio.SOUND_GRENADE, volume);

            Grenade.game.EffectsManager.AddExplosion(sender.Position, sender.ID);

            if (Grenade.game.GameOver)
                return;

            //when we calcuates the damage
            if (Grenade.game is SwarmGame)
            {
                if (sender.ID == GrenadeType.GRENADE_FRAG)
                {
                    BoundingSphere small = new BoundingSphere(sender.Position, GrenadeType.GrenadeTypes[sender.ID].Range);
                    BoundingSphere big = new BoundingSphere(sender.Position, GrenadeType.GrenadeTypes[sender.ID].EndRange);

                    if (Grenade.game.player.dead == false && sender.owner.Id == Grenade.game.Me.Id)
                    {
                        if (small.Contains(Grenade.game.player.position) != ContainmentType.Disjoint)
                        {
                            Grenade.game.player.GrenadeHurt(125, sender.owner);
                        }
                        else if (big.Contains(Grenade.game.player.position) != ContainmentType.Disjoint)
                        {
                            float dis;
                            Vector3.Distance(ref Grenade.game.player.position, ref sender.Position, out dis);

                            Grenade.game.player.GrenadeHurt(MathHelper.Lerp(0, GrenadeType.GrenadeTypes[sender.ID].Damage,
                                1 - (dis / GrenadeType.GrenadeTypes[sender.ID].EndRange)), sender.owner);
                        }
                    }

                    if (MinerOfDuty.Session.IsHost)
                    {
                        SwarmieManager sm = (Grenade.game as SwarmGame).SwarmManager as SwarmieManager;
                        Swarmie s;

                        int amountOfZombieHits = 0;
                        List<short> deadSwarmies = new List<short>();

                        for (int i = 0; i < sm.swarmies.Length; i++)
                        {
                            s = sm.swarmies[i];
                            if (s == null || s.Dead)
                                continue;

                            if (small.Contains(s.Position) != ContainmentType.Disjoint)
                            {
                                sm.HurtSwarmie(Grenade.game.players[sender.owner.Id], 200, s.ID);
                                deadSwarmies.Add(s.ID);
                                amountOfZombieHits++;
                            }
                            else if (big.Contains(s.Position) != ContainmentType.Disjoint)
                            {
                                float dis;
                                Vector3.DistanceSquared(ref s.Position, ref sender.Position, out dis);

                                if(sm.HurtSwarmie(Grenade.game.players[sender.owner.Id], MathHelper.Lerp(0, GrenadeType.GrenadeTypes[sender.ID].Damage,
                                    1 - (dis / (GrenadeType.GrenadeTypes[sender.ID].EndRange * GrenadeType.GrenadeTypes[sender.ID].EndRange))), s.ID))
                                    deadSwarmies.Add(s.ID);
                                amountOfZombieHits++;
                            }
                        }

                        Packet.WriteSwarmieGrenaded(Grenade.game.Me, sender.owner.Id, (byte)amountOfZombieHits, deadSwarmies.ToArray());
                    }


                }

            }
            else
            {
                if (new BoundingSphere(sender.Position, GrenadeType.GrenadeTypes[sender.ID].Range).Contains(Grenade.game.player.position) != ContainmentType.Disjoint)
                {
                    if ((sender.owner.IsLocal || Grenade.game.TeamManager.GetKillablePlayers().Contains(sender.owner.Id)) && Grenade.game.player.dead == false)
                    {
                        if (sender.ID == GrenadeType.GRENADE_FRAG)
                        {
                            Grenade.game.player.GrenadeHurt(125, sender.owner);
                        }
                        else if (sender.ID == GrenadeType.GRENADE_FLASH)
                        {
                            Grenade.game.player.DrawWhiteScreen();
                        }
                    }
                }//use this for otuside ramges
                else if (new BoundingSphere(sender.Position, GrenadeType.GrenadeTypes[sender.ID].EndRange).Contains(Grenade.game.player.position) != ContainmentType.Disjoint)
                {
                    if ((sender.owner.IsLocal || Grenade.game.TeamManager.GetKillablePlayers().Contains(sender.owner.Id))&& Grenade.game.player.dead == false)
                    {
                        if (sender.ID == GrenadeType.GRENADE_FRAG)
                        {
                            float dis;
                            Vector3.Distance(ref Grenade.game.player.position, ref sender.Position, out dis);

                            Grenade.game.player.GrenadeHurt(MathHelper.Lerp(0, GrenadeType.GrenadeTypes[sender.ID].Damage,
                                1 - (dis / GrenadeType.GrenadeTypes[sender.ID].EndRange)), sender.owner);
                        }
                    }
                }
            }
        }
    }
}
