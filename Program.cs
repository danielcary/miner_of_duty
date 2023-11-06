using System;

namespace Miner_Of_Duty
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {

            using (MinerOfDuty game = new MinerOfDuty())
            {
                game.Run();
            }
        }
    }
#endif
}

