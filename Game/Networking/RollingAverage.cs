using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.Game.Networking
{
    public class RollingAverage
    {
        public float RollingAveragee;
        private float sum;
        private float numberOfValues;
        private Queue<float> values;
        private const int MaxValues = 100;

        public RollingAverage()
        {
            values = new Queue<float>(MaxValues);
        }

        /// <summary>
        /// Adds a time value to the average
        /// </summary>
        /// <param name="time">Time in milliseconds</param>
        public void AddTime(float time)
        {
            if (numberOfValues == MaxValues)
            {
                sum -= values.Dequeue();
            }
            else
                numberOfValues++;

            values.Enqueue(time);

            sum += time;

            RollingAveragee = sum / numberOfValues;
        }
    }
}
