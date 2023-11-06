using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.Game
{
    public interface IHasCash
    {
        /// <summary>
        /// The actual cash you have
        /// </summary>
        int Cash { get; }
        /// <summary>
        /// This gets the cash value of trading in a weapon
        /// </summary>
        int ExtraCash { get; }
    }
}
