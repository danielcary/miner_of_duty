using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Miner_Of_Duty
{
    public class CrashedGame : IGameScreen
    {
        private Exception crashException;

        public CrashedGame(Exception e)
        {
            crashException = e;
        }

        public bool Restart { get; private set; }
         
        public void Update(GameTime gameTime)
        {
            Input.Update();
            if (Input.WasButtonPressed(Buttons.A))
            {
                Restart = true;
            }
        }

        public void Render(GraphicsDevice gd)
        {
           
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.MainMenuTexture, Vector2.Zero, new Color(100,100,100));
            sb.DrawString(Resources.TitleFont, "GAME CRASHED! :(", new Vector2(640, 110), Color.Red, 0, Resources.TitleFont.MeasureString("GAME CRASHED! :(") / 2f,
                1, SpriteEffects.None, 0);

            sb.DrawString(Resources.Font, "PLEASE REPORT THIS ON THE MINER OF DUTY FORUMS", new Vector2(640, 175), Color.Red, 0, Resources.Font.MeasureString("PLEASE REPORT THIS ON THE MINER OF DUTY FORUMS") / 2f, 
                1, SpriteEffects.None, 0);
            sb.DrawString(Resources.DescriptionFont, "(minerofdutyforums.com)", new Vector2(925, 165 + Resources.Font.LineSpacing), Color.Red, 0, Resources.DescriptionFont.MeasureString("(minerofdutyforums.com)") / 2f,
               1, SpriteEffects.None, 0);

            int i = 0, i2 = 0;
            string text = crashException.Message.Replace('\n', ' ');
            text = text.Replace('\r', ' ');
            text = text.Replace("  ", " ");
            while (true)
            {

                string poop = (i == 0 ? "MESSAGE: " : "") + text.Substring(i, (int)MathHelper.Clamp(text.Length - i, 0, 50 - (i == 0 ? "MESSAGE: ".Length : 0)));

                sb.DrawString(Resources.NameFont, poop, new Vector2(640, 250 + (i2 * Resources.NameFont.LineSpacing)), Color.Red, 0, Resources.NameFont.MeasureString(poop) / 2f,
                    1, SpriteEffects.None, 0);

                i2++;
                i += 50 - (i == 0 ? "MESSAGE: ".Length : 0);
                if (i >= text.Length)
                    break;

                if (i >= 100)
                    break;
            }

            i = 0; i2 = 0;
            text = crashException.StackTrace.Replace('\n', ' ');
            text = text.Replace('\r', ' ');
            text = text.Replace("  ", " ");
            while (true)
            {

                string poop = (i == 0 ? "STACKTRACE:" : "") + text.Substring(i, (int)MathHelper.Clamp(text.Length - i, 0, 70 - (i == 0 ? "STACKTRACE:".Length : 0)));

                sb.DrawString(Resources.DescriptionFont, poop, new Vector2(640, 390 + (i2 * Resources.DescriptionFont.LineSpacing)), Color.Red, 0, Resources.DescriptionFont.MeasureString(poop) / 2f,
                    1, SpriteEffects.None, 0);

                i2++;
                i += 70 - (i == 0 ? "STACKTRACE:".Length : 0);
                if (i >= text.Length)
                    break;

                if (i > 330)
                    break;
            }

            sb.DrawString(Resources.Font, "PRESS A TO CONTINUE", new Vector2(640, 600), Color.Green, 0, Resources.Font.MeasureString("PRESS A TO CONTINUE") / 2f, 1,
                SpriteEffects.None, 0);
        }

        public void Activated()
        {

        }

        public void Deactivated()
        {

        }
    }
}
