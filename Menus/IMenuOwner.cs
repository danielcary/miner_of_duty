using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Menus
{
    public interface IMenuOwner
    {
        void Update(short timePassedInMilliseconds);
        void Draw(SpriteBatch sb);
    }
}
