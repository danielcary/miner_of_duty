using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game.Editor
{
    public class EditorWeaponDrop : WeaponDropManager
    {

        public EditorWeaponDrop() : base(null) { }

      

        private short id = 0;
        public void AddSpawner(Vector3 position, byte weaponID)
        {
            weaponDrops.Add(new Spawner(position, weaponID, id++));
        }

        public void RemoveWeaponDrop(WeaponPickupable w)
        {
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                if (weaponDrops[i].ID.Number == w.ID.Number)
                {
                    weaponDrops.RemoveAt(i);
                    return;
                }
            }
        }

        public new WeaponPickupable CheckForPickup(ref Vector3 position)
        {
            if (weaponDrops.Count == 0)
            {
                return null;
            }

            int bestIndex = -1;
            float bestDistance = 69696969;
            float tmpDis;
            for (int i = 0; i < weaponDrops.Count; i++)
            {
                Vector3.Distance(ref weaponDrops[i].Position, ref position, out tmpDis);
                if (tmpDis < 1)
                {
                    if (tmpDis < bestDistance)
                    {
                        bestDistance = tmpDis;
                        bestIndex = i;
                    }
                }
            }
            if (bestIndex != -1)
                return weaponDrops[bestIndex];
            else
            {
                return null;
            }
        }
    }
}
