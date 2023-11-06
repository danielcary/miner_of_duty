using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game
{
    public class Goggles : IUseableInventoryItem
    {
        public bool IsTool { get { return false; } }
        public bool IsGoggle { get { return true; } }
        public bool IsGun { get { return false; } }

        public Goggles()
        {
            lifeSpan = MaxLifeSpan;
            IsEquiped = false;
        }

        /// <summary>
        /// In Milliseconds
        /// </summary>
        private float lifeSpan;
        private const float MaxLifeSpan = 45000;
        public void Update(GameTime gameTime)
        {
            if(IsEquiped)
                lifeSpan -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public bool IsEquiped { get; private set; }
        public void Equip()
        {
            IsEquiped = true;
        }

        public void UnEquip()
        {
            IsEquiped = false;
        }

        public bool IsEmpty { get { return lifeSpan < 0 ? true : false; } }

        public string Percent { get { return (int)((lifeSpan / MaxLifeSpan) * 100) + "%"; } }
    }
}
