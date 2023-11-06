using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game.Editor
{
    public class RangeMenu
    {
        public delegate void RangeSet(RangeMenu sender);
        public int Value { get { return (int)val; } }
        private float val;
        private int minVal = 3, maxVal = 50;
        RangeSet dele;

        public static int GetSize(int size)
        {
            int maxVal = (int)(size * .36f);
            return (3 + maxVal) / 2;
        }

        public RangeMenu(RangeSet dele, int worldSize)
        {
            maxVal = (int)(worldSize * .36f);
            val = (minVal + maxVal) /2;
            this.dele = dele;
        }

        private int delay;
        private int blinkers = 0;
        public void Update(GameTime gameTime)
        {
            int timeInMilliseconds = gameTime.ElapsedGameTime.Milliseconds;

            if (Input.IsThumbstickOrDPad(Input.Direction.Left) || Input.IsThumbstickOrDPad(Input.Direction.Right))
            {
                float amount = Input.IsThumbstickOrDPad(Input.Direction.Left) ? -.5f : .5f;


                if (delay > 0)
                    delay -= timeInMilliseconds;

                if (delay <= 0)
                {
                    val = MathHelper.Clamp(val + (amount * ((float)timeInMilliseconds / 20f)),
                        minVal, maxVal);

                    delay = 50;
                }
                blinkers = 500;
            }
            blinkers += timeInMilliseconds;
            if (blinkers > 600)
                blinkers = 0;

            if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                dele(this);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            sb.DrawString(Resources.NameFont, "SELECT THE RANGE", new Vector2(640, 300), Color.White, 0,
                    Resources.NameFont.MeasureString("SELECT THE RANGE") / 2f, 1, SpriteEffects.None, 0);

            if (blinkers <= 300)
            {
                sb.DrawString(Resources.TitleFont, "< " + Value.ToString() + " >", new Vector2(640, 360), Color.White, 0,
                    Resources.TitleFont.MeasureString("< " + Value.ToString() + " >") / 2f, 1, SpriteEffects.None, 0);
            }
            else
            {
                sb.DrawString(Resources.TitleFont, Value.ToString(), new Vector2(640, 360), Color.White, 0,
                  Resources.TitleFont.MeasureString(Value.ToString()) / 2f, 1, SpriteEffects.None, 0); 
            }

            sb.DrawString(Resources.DescriptionFont, "PRESS A TO CONFIRM", new Vector2(640, 410), Color.White, 0,
                    Resources.DescriptionFont.MeasureString("PRESS A TO CONFIRM") / 2f, 1, SpriteEffects.None, 0);
        }

    }
}
