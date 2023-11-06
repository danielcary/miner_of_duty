using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game
{
    public interface ITerrainOwner
    {
        Random GetRandom { get; }
        Terrain GetTerrain { get; }
        LightingManager GetLightingManager { get; }
        LiquidManager GetLiquidManager { get; }
        Terrain.UnderLiquid GetUnderLiquid { get; }
        bool BlockChanged(ref Vector3 pos, byte blockID, bool added);
    }
}
