using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game.ParticleSystem
{
    public class FlashBangParticleSystem
    {
        private FlashParticleSystem flashPS;
        private SparkParticleSystem sparkPS;
        private Vector3 spwnPos;
        public FlashBangParticleSystem(Vector3 position)
        {
            flashPS = new FlashParticleSystem(MinerOfDuty.mainMenu.minerOfDuty.Content);

            spwnPos = position;
            flashPS.AddParticle(position, Vector3.Zero);
            flashPS.AddParticle(position, Vector3.Zero);
            flashPS.AddParticle(position, Vector3.Zero);
            flashPS.AddParticle(position, Vector3.Zero);
            flashPS.AddParticle(position, Vector3.Zero);
            flashPS.AddParticle(position, Vector3.Zero);
            flashPS.AddParticle(position, Vector3.Zero);
            flashPS.AddParticle(position, Vector3.Zero);
            flashPS.AddParticle(position, Vector3.Zero);

            sparkPS = new SparkParticleSystem(MinerOfDuty.mainMenu.minerOfDuty.Content);

            for (int i = 0; i < 10; i++)
                sparkPS.AddParticle(spwnPos, Vector3.Zero);

            ts = new TimeSpan();
        }

        private TimeSpan ts;
        public bool Dead { get { return ts.TotalSeconds > .9f; } }

        public void Update(GameTime gameTime)
        {
            flashPS.Update(gameTime);
            sparkPS.Update(gameTime);

            ts = ts.Add(gameTime.ElapsedGameTime);
        }

        public void Render(Camera camera)
        {
            flashPS.SetCamera(camera);
            flashPS.Draw();
            sparkPS.SetCamera(camera);
            sparkPS.Draw();
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

                settings.MaxParticles = 50;

                settings.Duration = TimeSpan.FromSeconds(.5f);
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

                settings.MinEndSize = 35;
                settings.MaxEndSize = 35;

                // Use additive blending.
                settings.BlendState = BlendState.Additive;
            }

        }

        class SparkParticleSystem : ParticleSystem
        {

            public SparkParticleSystem(ContentManager content)
                : base(content)
            {

            }

            protected override void InitializeSettings(ParticleSettings settings)
            {
                settings.TextureName = "ParticleTextures/Particle005";

                settings.MaxParticles = 50;

                settings.Duration = TimeSpan.FromSeconds(.9f);
                settings.DurationRandomness = 0;

                settings.MinHorizontalVelocity = 50;
                settings.MaxHorizontalVelocity = 100;

                settings.MinVerticalVelocity = -2;
                settings.MaxVerticalVelocity = 100;

                settings.EndVelocity = 0;

                settings.MinColor = Color.DarkGray;
                settings.MaxColor = Color.Gray;

                settings.MinRotateSpeed = -1;
                settings.MaxRotateSpeed = 1;

                settings.MinStartSize = 4;
                settings.MaxStartSize = 4;

                settings.MinEndSize = 4;
                settings.MaxEndSize = 4;

                // Use additive blending.
                settings.BlendState = BlendState.Additive;
            }
        }
    }

   

}
