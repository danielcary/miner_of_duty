using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Game.ParticleSystem
{
    public class FragParticleSystem
    {
        private FlashParticleSystem flashPS;
        private FlamesParticleSystem flamesPS;
        private SmokeParticleSystem smokePS;
        private Vector3 spwnPos;

        public FragParticleSystem(Vector3 position)
        {
            spwnPos = position;
            ts = new TimeSpan();

            flashPS = new FlashParticleSystem(MinerOfDuty.ContentManager);

            
            for(int i = 0; i < 8; i++)
                flashPS.AddParticle(position, Vector3.Zero);

            flamesPS = new FlamesParticleSystem(MinerOfDuty.ContentManager);

            for (int i = 0; i < 15; i++)
                flamesPS.AddParticle(position, Vector3.Zero);

            smokePS = new SmokeParticleSystem(MinerOfDuty.ContentManager);

            for (int i = 0; i < 20; i++)
                smokePS.AddParticle(position, Vector3.Zero);
        }

        private TimeSpan ts;
        public bool Dead { get { return ts.TotalSeconds > 2; } }

        public void Update(GameTime gameTime)
        {
            flashPS.Update(gameTime);
            flamesPS.Update(gameTime);
            smokePS.Update(gameTime);
            ts = ts.Add(gameTime.ElapsedGameTime);
        }

        public void Render(Camera camera)
        {
            smokePS.SetCamera(camera);
            smokePS.Draw();
            flamesPS.SetCamera(camera);
            flamesPS.Draw();
            flashPS.SetCamera(camera);
            flashPS.Draw();
        }

        class FlashParticleSystem : ParticleSystem
        {

            public FlashParticleSystem(ContentManager content)
                : base(content)
            {

            }

            protected override void InitializeSettings(ParticleSettings settings)
            {
                settings.TextureName = "ParticleTextures/Cloud001";

                settings.MaxParticles = 10;

                settings.Duration = TimeSpan.FromSeconds(.1);
                settings.DurationRandomness = 0;

                settings.MinHorizontalVelocity = 10;
                settings.MaxHorizontalVelocity = 20;

                settings.MinVerticalVelocity = -2;
                settings.MaxVerticalVelocity = 10;

                settings.EndVelocity = 0;

                settings.MinColor = Color.DarkGray;
                settings.MaxColor = Color.Gray;

                settings.MinRotateSpeed = -1;
                settings.MaxRotateSpeed = 1;

                settings.MinStartSize = 7;
                settings.MaxStartSize = 7;

                settings.MinEndSize = 10;
                settings.MaxEndSize = 10;

                // Use additive blending.
                settings.BlendState = BlendState.Additive;
            }

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

                settings.MaxParticles = 20;

                settings.Duration = TimeSpan.FromSeconds(2);
                settings.DurationRandomness = .2f;

                settings.MinHorizontalVelocity = 2;
                settings.MaxHorizontalVelocity = 5;

                settings.MinVerticalVelocity = -2;
                settings.MaxVerticalVelocity = 5;

                settings.EndVelocity = 0;

                settings.MinColor = new Color(0.5019608f, 0.5019608f, 0.5019608f);
                settings.MaxColor = new Color(0.5019608f, 0.5019608f, 0.5019608f);

                settings.MinRotateSpeed = -1;
                settings.MaxRotateSpeed = 1;

                settings.MinStartSize = 3;
                settings.MaxStartSize = 3;

                settings.MinEndSize = 6;
                settings.MaxEndSize = 6;

                // Use additive blending.
                settings.BlendState = BlendState.NonPremultiplied;
            }

        }

        class FlamesParticleSystem : ParticleSystem
        {

            public FlamesParticleSystem(ContentManager content)
                : base(content)
            {
            }

            protected override void InitializeSettings(ParticleSettings settings)
            {
                settings.TextureName = "ParticleTextures/Particle004";

                settings.MaxParticles = 15;

                settings.Duration = TimeSpan.FromSeconds(.75);
                settings.DurationRandomness = .2f;

                settings.MinHorizontalVelocity = 3;
                settings.MaxHorizontalVelocity = 5;

                settings.MinVerticalVelocity = -2;
                settings.MaxVerticalVelocity = 5;

                settings.EndVelocity = 0;

                settings.MinColor = new Color(1, 0.5019608f, 0);
                settings.MaxColor = new Color(1, 0.5019608f, 0);

                settings.MinRotateSpeed = -1;
                settings.MaxRotateSpeed = 1;

                settings.MinStartSize = 7;
                settings.MaxStartSize = 7;

                settings.MinEndSize = 15;
                settings.MaxEndSize = 15;

                // Use additive blending.
                settings.BlendState = BlendState.Additive;
            }

        }
    }
}
