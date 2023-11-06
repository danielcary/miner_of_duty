using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Miner_Of_Duty
{
    /// <summary>
    /// http://blogs.msdn.com/b/shawnhar/archive/2007/06/08/displaying-the-framerate.aspx
    /// </summary>
    public class FrameRateCounter : DrawableGameComponent
    {
        SpriteBatch spriteBatch;

        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;


        public FrameRateCounter(MinerOfDuty game)
            : base(game)
        {
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }


        protected override void UnloadContent()
        {
        }


        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }


        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            string fps = string.Format("fps: {0}", frameRate);

            spriteBatch.Begin();

            spriteBatch.DrawString(Resources.Font, fps, new Vector2(133, 133), Color.Black);
            spriteBatch.DrawString(Resources.Font, fps, new Vector2(132, 132), Color.White);

            spriteBatch.End();
        }
    }
}
