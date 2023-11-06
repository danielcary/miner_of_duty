using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Miner_Of_Duty.Game.ParticleSystem;

namespace Miner_Of_Duty.Game
{
    public class EffectsManager
    {
        private Flare[] flarePool;
        private BulletStreak[] bulletStreakPool;
        private Explosion[] explosionPools;

        public EffectsManager()
        {
            flarePool = new Flare[16];
            bulletStreakPool = new BulletStreak[32];
            explosionPools = new Explosion[40];
        }

        public void AddExplosion(Vector3 pos, byte grenadeID)
        {
            for (int i = 0; i < 40; i++)
            {
                if (explosionPools[i] == null || explosionPools[i].Dead)
                {
                    if (grenadeID == GrenadeType.GRENADE_FRAG)
                        explosionPools[i] = new FragExplosion(ref pos);
                    else if (grenadeID == GrenadeType.GRENADE_FLASH)
                        explosionPools[i] = new FlashBangExplosion(ref pos);
                    else if (grenadeID == GrenadeType.GRENADE_SMOKE)
                        explosionPools[i] = new SmokeGrenadeExplosion(ref pos);
                    break;
                }
            }
        }

        public void AddFlare(PlayerBody ownerPlayer, byte gunShotWith)
        {
            for (int i = 0; i < 16; i++)
            {
                if (flarePool[i] == null)
                {
                    flarePool[i] = new Flare(ownerPlayer, gunShotWith);
                    break;
                }
                else if (flarePool[i].CanBeReused)
                {
                    flarePool[i].Reuse(ownerPlayer, gunShotWith);
                    break;
                }
            }
        }

        public void AddBulletStreak(ref Ray ray, float dis)
        {
            for (int i = 0; i < 32; i++)
            {
                if (bulletStreakPool[i] == null)
                {
                    bulletStreakPool[i] = new BulletStreak(ref ray, dis);
                    break;
                }
                else if (bulletStreakPool[i].IsDone)
                {
                    bulletStreakPool[i].Reuse(ref ray, dis);
                    break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < 16; i++)
            {
                if (flarePool[i] != null)
                {
                    flarePool[i].Update(gameTime);
                }
            }
            for (int i = 0; i < 32; i++)
            {
                if (bulletStreakPool[i] != null)
                {
                    bulletStreakPool[i].Update(gameTime);
                }
            }

            for (int i = 0; i < 40; i++)
            {
                if (explosionPools[i] != null && explosionPools[i].Dead == false)
                    explosionPools[i].Update(gameTime);
            }
        }

        public void Render(Camera cam)
        {
            for (int i = 0; i < 16; i++)
            {
                if (flarePool[i] != null)
                {
                    flarePool[i].Render(cam);
                }
            }
            for (int i = 0; i < 32; i++)
            {
                if (bulletStreakPool[i] != null)
                {
                    bulletStreakPool[i].Render(cam);
                }
            }

            for (int i = 0; i < 40; i++)
            {
                if (explosionPools[i] != null && explosionPools[i].Dead == false)
                    explosionPools[i].Render(cam);
            }
        }

        internal class Flare
        {
            private PlayerBody ownerPlayer;
            private int duration;
            private byte GunFiredBy;

            public Flare(PlayerBody owner, byte gunFiredBy)
            {
                ownerPlayer = owner;
                this.GunFiredBy = gunFiredBy;
            }

            public bool CanBeReused { get { return duration >= 80; } }

            public void Reuse(PlayerBody newOwner, byte gunFiredBy)
            {
                ownerPlayer = newOwner;
                duration = 0;
                GunFiredBy = gunFiredBy;
            }

            public void Update(GameTime gameTime)
            {
                if (duration < 80)
                    duration += gameTime.ElapsedGameTime.Milliseconds;
            }

            public void Render(Camera camera)
            {
                if (duration < 80)
                {
                    foreach (ModelMesh mesh in Resources.MuzzleFlare[GunFiredBy].Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            effect.CurrentTechnique = effect.Techniques["ModelFog"];
                            effect.Parameters["World"].SetValue(ownerPlayer.GetBarrelMat());
                            effect.Parameters["Texture0"].SetValue(Resources.FlareTexture);
                            effect.Parameters["CameraPosition"].SetValue(camera.Position);
                            effect.Parameters["View"].SetValue(camera.ViewMatrix);
                            effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                        }
                        mesh.Draw();
                    }
                }

            }
        }

        internal class BulletStreak
        {
            private Ray bulletDir;
            private Vector3 position;
            private float distanceToTravel;

            public bool IsDone { get { return distanceToTravel < 0; } }

            public BulletStreak(ref Ray ray, float disToTravel)
            {
                bulletDir = ray;
                bulletDir.Position.Y -= .2f;
                distanceToTravel = disToTravel;
                position = ray.Position;
                mat = Matrix.CreateScale(1, 1, 3) * Matrix.CreateWorld(Vector3.Zero, bulletDir.Direction, Vector3.UnitX);
            }

            public void Update(GameTime gameTime)
            {
                if (distanceToTravel > 0)
                {
                    distanceToTravel -= (float)(256f * gameTime.ElapsedGameTime.TotalSeconds);
                    position += bulletDir.Direction * (float)(256f * gameTime.ElapsedGameTime.TotalSeconds);
                }
            }

            public void Reuse(ref Ray ray, float disToTravel)
            {
                bulletDir = ray;
                bulletDir.Position.Y -= .2f;
                distanceToTravel = disToTravel;
                position = ray.Position;
                mat = Matrix.CreateScale(1, 1, 3) * Matrix.CreateWorld(Vector3.Zero, bulletDir.Direction, Vector3.UnitX);
            }

            private Matrix mat;
            private static Vector3 Up = Vector3.UnitX;
            public static Vector3 Zero = new Vector3(0, 0, 0);
            public void Render(Camera camera)
            {
                if (distanceToTravel > 0)
                {
                    foreach (ModelMesh mesh in Resources.BulletStreakModel.Meshes)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            effect.CurrentTechnique = effect.Techniques["ModelFog"];
                            effect.Parameters["World"].SetValue(mat * Matrix.CreateTranslation(position));
                            effect.Parameters["Texture0"].SetValue(Resources.BulletStreakTexture);
                            effect.Parameters["CameraPosition"].SetValue(camera.Position);
                            effect.Parameters["View"].SetValue(camera.ViewMatrix);
                            effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                        }
                        mesh.Draw();
                    }
                }
            }
        }

        internal interface Explosion
        {
            void Update(GameTime gameTime);
            void Render(Camera cam);
            bool Dead { get; }
        }

        internal class FragExplosion : Explosion
        {
            private Vector3 pos;
            private FragParticleSystem geps;

            public FragExplosion(ref Vector3 pos)
            {
                this.pos = pos;
                geps = new FragParticleSystem(pos);
            }

            public void Update(GameTime gameTime)
            {
                geps.Update(gameTime);
                
            }

            public void Render(Camera cam)
            {
                geps.Render(cam);
            }

            public bool Dead
            {
                get { return geps.Dead; }
            }

            
        }

        internal class FlashBangExplosion : Explosion
        {
            private Vector3 pos;
            private FlashBangParticleSystem ps;

            public FlashBangExplosion(ref Vector3 pos)
            {
                this.pos = pos;
                ps = new FlashBangParticleSystem(pos);
            }

            public void Update(GameTime gameTime)
            {
                ps.Update(gameTime);

            }

            public void Render(Camera cam)
            {
                ps.Render(cam);
            }

            public bool Dead
            {
                get { return ps.Dead; }
            }


        }

        internal class SmokeGrenadeExplosion : Explosion
        {
            private Vector3 pos;
            private SmokeGrenadeParticleSystem ps;

            public SmokeGrenadeExplosion(ref Vector3 pos)
            {
                this.pos = pos;
                ps = new SmokeGrenadeParticleSystem(pos);
            }

            public void Update(GameTime gameTime)
            {
                ps.Update(gameTime);

            }

            public void Render(Camera cam)
            {
                ps.Render(cam);
            }

            public bool Dead
            {
                get { return ps.Dead; }
            }


        }
    }


}

