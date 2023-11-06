using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game
{
    public class GrenadeType
    {
        public int LifeSpan { get; private set; }
        public bool CanCook { get; private set; }

        public float Range { get; private set; }
        public float EndRange { get; private set; }
        public float Damage { get; private set; }

        public int StartingGrenadeCount { get; private set; }

        public const byte GRENADE_EMPTY = 3;
        public const byte GRENADE_FRAG = 0;
        public const byte GRENADE_FLASH = 1;
        public const byte GRENADE_SMOKE = 2;

        private GrenadeType(int lifeSpane, bool canCook, float range, float endrange, float dmg, int startCount)
        {
            LifeSpan = lifeSpane;
            CanCook = canCook;
            Range = range;
            EndRange = endrange;
            Damage = dmg;
            StartingGrenadeCount = startCount;
        }

        public static GrenadeType[] GrenadeTypes;


        static GrenadeType()
        {
            GrenadeTypes = new GrenadeType[4];
            GrenadeTypes[GRENADE_FRAG] = new GrenadeType(3000, true, 4, 7, 150, 3);
            GrenadeTypes[GRENADE_FLASH] = new GrenadeType(1500, false, 4, 4, 100, 3);
            GrenadeTypes[GRENADE_SMOKE] = new GrenadeType(2000, false, 5, 7, 100, 3);
            GrenadeTypes[GRENADE_EMPTY] = new GrenadeType(2000, false, 5, 7, 100, 0);
        }

    }

    public class Grenade
    {
        public delegate void GrenadeCooked(Grenade sender);

        public GrenadeType GrenadeType { get { return GrenadeType.GrenadeTypes[GrenadeID]; } }
        public byte GrenadeID { get; private set; }

        private GrenadeCooked gc;

        public float GetDamage(float range)
        {
            return MathHelper.Lerp(GrenadeType.Damage, 0, range / GrenadeType.Range);
        }

        public int AmountOfGrenades { get; private set; }

        public void AddGrenade()
        {
            AmountOfGrenades++;
        }

        public Grenade(byte grenadeID, GrenadeCooked gc)
        {
            this.GrenadeID = grenadeID;
            this.gc = gc;
            time = 0;
            AmountOfGrenades = GrenadeType.StartingGrenadeCount;
        }

        public float time;
        public void RBIsDown(GameTime gameTIme)
        {
            if (GrenadeType.CanCook)
            {
                time += (float)gameTIme.ElapsedGameTime.TotalMilliseconds;
                if (time >= GrenadeType.LifeSpan)
                {
                    gc.Invoke(this);
                    AmountOfGrenades--;//not that this matters if the grenade is lethal
                    time = 0;
                }
            }
        }

        public void Throw()
        {
            AmountOfGrenades--;
            time = 0;
        }

    }
}
/*
when actual gerenad is init fire a jump and use that move dir. dont move it after

*/