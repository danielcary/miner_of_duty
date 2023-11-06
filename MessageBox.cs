using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty
{
    public static class MessageBox
    {
        private const int Width = 600;

        public delegate void MessageBoxResult(int result);
        private static MessageBoxResult toCall;

        private static string[] msg;
        private static string[] options;
        private static Vector2[] optionsPos;
        private static int selected;

        public static void ShowMessageBox(MessageBoxResult callBack, string[] options, int defaultSelected, string[] msg)
        {
            IsMessageBeingShown = true;
            toCall = callBack;

            MessageBox.options = options;
            selected = defaultSelected;
            MessageBox.msg = msg;

            float widest = 0;

            for (int i = 0; i < options.Length; i++)
            {
                if (Resources.Font.MeasureString(options[i]).X > widest)
                    widest = Resources.Font.MeasureString(options[i]).X;
            }

            optionsPos = new Vector2[options.Length];

            Vector2 startingPos = new Vector2((1280 / 2) - ((widest * options.Length) / 2), 400);
            for (int i = 0; i < options.Length; i++)
            {
                optionsPos[i] = new Vector2((widest - Resources.Font.MeasureString(options[i]).X) / 2f, 0) + startingPos;
                startingPos.X += widest;
            }
        }

        public static void CloseMessageBox()
        {
            IsMessageBeingShown = false;
        }

        private static int delay;
        public static void Update(GameTime gameTime)
        {
            if (delay > 0)
                delay -= gameTime.ElapsedGameTime.Milliseconds;

            if (delay <= 0)
            {
                if (Input.IsThumbstickOrDPad(Input.Direction.Left))
                {
                    if (--selected < 0)
                    {
                        selected = 0;
                    }
                    else
                        delay = 180;
                }
                else if (Input.IsThumbstickOrDPad(Input.Direction.Right))
                {
                    if (++selected >= options.Length)
                    {
                        selected = options.Length - 1;
                    }
                    else
                        delay = 180;
                }
            }

            if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                IsMessageBeingShown = false;
                if(toCall != null)
                    toCall.Invoke(selected);
            }

        }

        public static bool IsMessageBeingShown { get; private set; }

        public static void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.MessageBoxBackTexture, new Vector2(289, 221), Color.White);

            Vector2 startingPos = new Vector2(640, 275 + (Resources.Font.LineSpacing / 2));
            for (int i = 0; i < msg.Length; i++)
            {
                sb.DrawString(Resources.Font, msg[i], startingPos, Color.White, 0, Resources.Font.MeasureString(msg[i]) / 2f, 1, SpriteEffects.None, 0);
                startingPos.Y += Resources.Font.LineSpacing;
            }

            for (int i = 0; i < optionsPos.Length; i++)
            {
                sb.DrawString(Resources.Font, options[i], optionsPos[i], Color.White);
            }

            sb.DrawString(Resources.Font, options[selected], optionsPos[selected], Color.Green);

        }

    }
}
