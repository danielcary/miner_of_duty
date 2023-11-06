using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Miner_Of_Duty.Game
{
    public class ToolType
    {
        public float DamageTowardsSoft { get; private set; }
        public float DamageTowardsHard { get; private set; }

        public float GetDamageTowardsSoftWithSharp
        {
            get
            {
                return DamageTowardsSoft * 1.1f;
            }
        }

        public float GetDamageTowardsHardWithSharp
        {
            get
            {
                return DamageTowardsHard * 1.1f;
            }
        }

        public float Damage { get; private set; }

        public int Uses { get; private set; }

        public int GetUsesWithHarden
        {
            get
            {
                return (int)(Uses * 1.25f);
            }
        }

        public ToolType(float damage, float dmgTowardsSoft, float dmgTowardsHard, int uses)
        {
            DamageTowardsSoft = dmgTowardsSoft;
            DamageTowardsHard = dmgTowardsHard;
            Uses = uses;
        }

        public static ToolType[] ToolTypes;
        static ToolType()
        {
            ToolTypes = new ToolType[]
            {
                new ToolType(5, 120, 80, 55), //rock shovel
                new ToolType(7, 150, 90, 75), //steel shovel
                new ToolType(10, 175, 100, 100), //diamond shovel
                new ToolType(5, 80, 120, 50), //rock pick
                new ToolType(7, 90, 150, 70), //steel pick
                new ToolType(10, 100, 175, 100), //diamond pick
            };
        }
        public const byte TOOLID_ROCKSHOVEL = 0;
        public const byte TOOLID_STEELSHOVEL = 1;
        public const byte TOOLID_DIAMONDSHOVEL = 2;
        public const byte TOOLID_ROCKPICK = 3;
        public const byte TOOLID_STEELPICK = 4;
        public const byte TOOLID_DIAMONDPICK = 5;
    }

    public class Tool : IUseableInventoryItem
    {
        public ToolType ToolType { get { return ToolType.ToolTypes[ToolTypeID]; } }
        public byte ToolTypeID { get; private set; }
        private bool isHarden;
        public Tool(byte toolID, bool sharped, bool harden)
        {
            ToolTypeID = toolID;

            isHarden = harden;
            isSharpened = sharped;

            if (harden)
                Uses = ToolType.GetUsesWithHarden;
            else
                Uses = ToolType.Uses;
        }

        public int Uses { get; private set; }
        public int MaxUses
        {
            get
            {
                return isHarden ?
                    ToolType.GetUsesWithHarden :
                    ToolType.Uses;
            }
        }
        private bool isSharpened;

         /// <summary>
         /// used for hurting people
         /// </summary>
         /// <returns></returns>
        public float GetDamage { get { return ToolType.Damage; } }

        public float GetDamageTowardsHard()
        {
            if (isSharpened)
                return !IsDull ? ToolType.GetDamageTowardsHardWithSharp : ToolType.GetDamageTowardsHardWithSharp * .5f;
            else
                return !IsDull ? ToolType.DamageTowardsHard : ToolType.DamageTowardsHard * .5f;
        }

        public float GetDamageTowardsSoft()
        {
            if (isSharpened)
                return !IsDull ? ToolType.GetDamageTowardsSoftWithSharp : (ToolType.GetDamageTowardsSoftWithSharp * .5f);
            else
                return !IsDull ? ToolType.DamageTowardsSoft : (ToolType.DamageTowardsSoft * .5f);
        }

        /// <summary>
        /// use for differned sound?
        /// </summary>
        public bool IsDull { get { return Uses <= 0; } }

        /// <summary>
        /// call this when block is destroyed
        /// </summary>
        public void Use()
        {
            if(Uses > 0)
                Uses--;
        }


        public bool IsTool
        {
            get { return true; }
        }

        public bool IsGoggle
        {
            get { return false; }
        }

        public bool IsGun
        {
            get { return false; }
        }
    }
}



