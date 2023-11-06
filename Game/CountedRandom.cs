using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.Game
{
    public class CountedRandom : Random
    {
        /// <summary>
        /// The amount of times Next() has been called.
        /// </summary>
        public uint TimesUsed { get; private set; }
        public int Seed { get; private set; }
        public CountedRandom(int seed)
            : base(seed)
        {
            Seed = seed;
        }

        public CountedRandom(int seed, int uses)
            : base(seed)
        {
            Seed = seed;
            for (int i = 0; i < uses; i++)
                Next();
        }

        public CountedRandom()
            : base()
        {

        }

        public override int Next()
        {
            TimesUsed++;
            return base.Next();
        }

        public override int Next(int maxValue)
        {
            TimesUsed++;
            return base.Next(maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            TimesUsed++;
            return base.Next(minValue, maxValue);
        }

        public override double NextDouble()
        {
            TimesUsed++;
            return base.NextDouble();
        }

    }
}
