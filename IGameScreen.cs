using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty
{
    public interface IGameScreen
    {
        void Update(GameTime gameTime);
        void Render(GraphicsDevice gd);
        void Draw(SpriteBatch sb);
        void Activated();
        void Deactivated();
    }
}
