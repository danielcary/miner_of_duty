using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Game.ParticleSystem
{
    public class SmokeGrenadeParticleSystem
    {
        private SmokeParticleSystem smokePS;
        private Vector3 spwnPos;

        public SmokeGrenadeParticleSystem(Vector3 position)
        {
            spwnPos = position;
            ts = new TimeSpan();

            smokePS = new SmokeParticleSystem(MinerOfDuty.mainMenu.minerOfDuty.Content);

            for (int i = 0; i < 5; i++)
                smokePS.AddParticle(position, Vector3.Zero);
        }

        private TimeSpan ts;
        public bool Dead { get { return ts.TotalSeconds > 18; } }

        private int add;
        public void Update(GameTime gameTime)
        {
            smokePS.Update(gameTime);
            add++;
            if (add == 3)
            {
                if (ts.TotalSeconds < 14)
                    smokePS.AddParticle(spwnPos, Vector3.Zero);
                add = 0;
            }
            ts = ts.Add(gameTime.ElapsedGameTime);
        }

        public void Render(Camera camera)
        {
            smokePS.SetCamera(camera);
            smokePS.Draw();
        }

        class SmokeParticleSystem : ParticleSystem
        {

            public SmokeParticleSystem(ContentManager content)
                : base(content)
            {

            }

            protected override void InitializeSettings(ParticleSettings settings)
            {
                settings.TextureName = "ParticleTextures/Cloud001";

                settings.MaxParticles = 75;

                settings.Duration = TimeSpan.FromSeconds(4);
                settings.DurationRandomness = .2f;

                settings.MinHorizontalVelocity = 3.5f;
                settings.MaxHorizontalVelocity = 7;

                settings.MinVerticalVelocity = -1.5f;
                settings.MaxVerticalVelocity = 7;

                settings.EndVelocity = 0;

                settings.MinColor = new Color(0.5019608f, 0.5019608f, 0.5019608f);
                settings.MaxColor = new Color(0.5019608f, 0.5019608f, 0.5019608f);

                settings.MinRotateSpeed = -1;
                settings.MaxRotateSpeed = 1;

                settings.MinStartSize = 5;
                settings.MaxStartSize = 5;

                settings.MinEndSize = 10;
                settings.MaxEndSize = 12;

                // Use additive blending.
                settings.BlendState = BlendState.NonPremultiplied;
            }

        }

       
    }
}
