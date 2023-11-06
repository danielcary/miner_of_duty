using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Game
{
    public interface IUseableInventoryItem
    {
        bool IsTool { get; }
        bool IsGoggle { get; }
        bool IsGun { get; }
    }

    public enum InventoryItem : byte
    {
        Empty,
        StoneBlock,
        DirtBlock,
        GrassBlock,
        SandBlock,
        LavaBucket,
        EmptyBucket,
        AK47,
        FAL,
        M16,
        Magnum,
        Colt45,
        UMP45,
        Vector,
        MP5K,
        DoubleBarrel,
        SingleBarrel,
        AA12,
        ShovelRock,
        ShovelSteel,
        ShovelDiamond,
        PickRock,
        PickSteel,
        PickDiamond,
        WoodBlock,
        LeafBlock,
        TeamASpawn1,
        TeamBSpawn1,
        FragGrenade,
        FlashBang,
        SmokeGrenade,
        GlassBlock,
        BlackBlock,
        BlueBlock,
        GreenBlock,
        GreyBlock,
        OrangeBlock,
        RedBlock,
        TealBlock,
        WhiteBlock,
        YellowBlock,
        Goggles,
        GlowBlock,
        TeamASpawn2,//watch placement
        TeamBSpawn2,
        TeamASpawn3,
        TeamBSpawn3,
        Minigun,
        Sword,
        AK47Spawn,
        FALSpawn,
        M16Spawn,
        MagnumSpawn,
        Colt45Spawn,
        UMP45Spawn,
        VectorSpawn,
        MP5KSpawn,
        DoubleBarrelSpawn,
        SingleBarrelSpawn,
        AA12Spawn,
        Firebricks,
        Stonebricks,
        WoodPlanks,
        Cobblestone,
        MossyCobblestone,
        GoldBlock,
        Spawn1,
        Spawn2,
        Spawn3,
        Spawn4,
        Spawn5,
        Spawn6,
        ZombieSpawn1,
        ZombieSpawn2,
        ZombieSpawn3,
        ZombieSpawn4,
        ZombieSpawn5,
        ZombieSpawn6,
        Pitfall,
        WaterBucket,
        KingSpawn,
    }

    public class Inventory
    {
        public InventoryItem[] items;
        public Dictionary<int, IUseableInventoryItem> useableItems;
        private Grenade lethalGrenade, specialGrenade;
        private int selectedIndex;
        private int lastAddedIndex;
        private float delay;

        private Grenade empty = new Grenade(3, null);
        public InventoryItem GetSelectedItem { get { return items[selectedIndex]; } }
        public Gun GetSelectedGun { get { return useableItems[selectedIndex] as Gun; } }
        public Tool GetSelectedTool { get { return useableItems[selectedIndex] as Tool; } }
        public Goggles GetSelectedGoggle { get { return useableItems[selectedIndex] as Goggles; } }
        public Grenade GetLethalGrenade { get { return MinerOfDuty.game.WeaponsEnabled ? lethalGrenade : empty; } }
        public Grenade GetSpecialGrenade { get { return MinerOfDuty.game.WeaponsEnabled ? specialGrenade : empty; } }

        public Goggles EquipedGoggles;

        public void EquipGoggles()
        {
            if (EquipedGoggles != null)
                EquipedGoggles.UnEquip();

            GetSelectedGoggle.Equip();

            EquipedGoggles = GetSelectedGoggle;
        }

        public void UnEquipGoggles()
        {
            if(EquipedGoggles != null)
                EquipedGoggles.UnEquip();
            EquipedGoggles = null;
        }


        public Inventory()
        {
            items = new InventoryItem[4];
            items[0] = InventoryItem.Empty;
            items[1] = InventoryItem.Empty;
            items[2] = InventoryItem.Empty;
            items[3] = InventoryItem.Empty;
            selectedIndex = 0;
            useableItems = new Dictionary<int, IUseableInventoryItem>();
        }

        public void SetLethalGrenade(Grenade grenade)
        {
            lethalGrenade = grenade;
        }

        public void SetSpecialGrenade(Grenade grenade)
        {
            specialGrenade = grenade;
        }

        public void AddTool(Tool tool)
        {
            AddItem(ToolIDToInventoryItem(tool.ToolTypeID));

            useableItems.Add(lastAddedIndex, tool);
        }

        public void AddGun(Gun gun)
        {
            AddItem(GunIDToInventoryItem(gun.GunTypeID));

            useableItems.Add(lastAddedIndex, gun);
        }

        public void AddGoggle()
        {
            AddItem(InventoryItem.Goggles);

            useableItems.Add(lastAddedIndex, new Goggles());
        }

        public void AddItem(InventoryItem item)
        {
            for (int i = 0; i < 4; i++)
            {
                if (items[i] == InventoryItem.Empty)
                {
                    lastAddedIndex = i;
                    items[i] = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Used for changing the selected item of a Networked Player
        /// </summary>
        /// <param name="item">The item to chage to</param>
        public void SetSelectedTo(InventoryItem item)
        {
            items[selectedIndex] = item;
        }

        public void ChangeSelectedTo(InventoryItem item)
        {
            items[selectedIndex] = item;
        }

        public void ChangeSelectedGun(Gun gun)
        {
            useableItems[selectedIndex] = gun;

            switch (gun.GunTypeID)
            {
                case GunType.GUNID_AK47:
                    items[selectedIndex] = InventoryItem.AK47;
                    break;
                case GunType.GUNID_COLT45:
                    items[selectedIndex] = InventoryItem.Colt45;
                    break;
                case GunType.GUNID_AA12:
                    items[selectedIndex] = InventoryItem.AA12;
                    break;
                case GunType.GUNID_DOUBLEBARREL:
                    items[selectedIndex] = InventoryItem.DoubleBarrel;
                    break;
                case GunType.GUNID_FAL:
                    items[selectedIndex] = InventoryItem.FAL;
                    break;
                case GunType.GUNID_M16:
                    items[selectedIndex] = InventoryItem.M16;
                    break;
                case GunType.GUNID_MAGNUM:
                    items[selectedIndex] = InventoryItem.Magnum;
                    break;
                case GunType.GUNID_MP5K:
                    items[selectedIndex] = InventoryItem.MP5K;
                    break;
                case GunType.GUNID_12GAUGE:
                    items[selectedIndex] = InventoryItem.SingleBarrel;
                    break;
                case GunType.GUNID_UMP45:
                    items[selectedIndex] = InventoryItem.UMP45;
                    break;
                case GunType.GUNID_VECTOR:
                    items[selectedIndex] = InventoryItem.Vector;
                    break;
                case GunType.GUNID_MINIGUN:
                    items[selectedIndex] = InventoryItem.Minigun;
                    break;
                case GunType.GUNID_SWORD:
                    items[selectedIndex] = InventoryItem.Sword;
                    break;
            }
        }

        private GamePadState oldState;
        public void Update(GameTime gameTime, ref GamePadState gamePad)
        {
            for (int i = 0; i < 4; i++)
            {
                if (useableItems.ContainsKey(i) )
                {
                    if(useableItems[i].IsGun)
                        (useableItems[i] as Gun).Update(gameTime);
                    else if (useableItems[i].IsGoggle)
                    {
                        (useableItems[i] as Goggles).Update(gameTime);

                    }
                }
            }

            if (EquipedGoggles != null)
                if (EquipedGoggles.IsEmpty)
                    UnEquipGoggles();

            for (int i = 0; i < 4; i++)
            {
                if (useableItems.ContainsKey(i) && useableItems[i].IsGun)
                {
                    if ((useableItems[i] as Gun).CanReload == false)
                    {
                        if (delay > 0)
                            delay -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                        return;
                    }
                }
            }

            if (Input.WasButtonPressed(Buttons.LeftShoulder, ref oldState, ref gamePad))
            {
                delay = 150;
                if (IsGun(GetSelectedItem))
                    GetSelectedGun.Deselected();
                if (--selectedIndex < 0)
                    selectedIndex = 3;

            }
            else if (Input.WasButtonPressed(Buttons.RightShoulder, ref oldState, ref gamePad))
            {
                delay = 150;
                if (IsGun(GetSelectedItem))
                    GetSelectedGun.Deselected();
                if (++selectedIndex > 3)
                    selectedIndex = 0;
            }

            oldState = gamePad;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.SelectionBox, new Vector2(174, 570), Color.White);
            for (int i = 0; i < 4; i++)
                if (items[i] != InventoryItem.Empty)
                    sb.Draw(Resources.ItemTextures[items[i]], new Vector2(174 + i * (75), 570), Color.White);
            sb.Draw(Resources.SelectedBox, new Vector2(174 + 3f + (selectedIndex * (75)), 573f), Color.White);

            sb.DrawString(Resources.Font, GetItemAsString(GetSelectedItem) + (GetSelectedItem == InventoryItem.Sword ? "" :  IsGun(GetSelectedItem) ?
                " ( " + GetSelectedGun.CurrentAmmoInClip + "/" + GetSelectedGun.totalAmmo + " )" : IsTool(GetSelectedItem) ?
                 " ( " + GetSelectedTool.Uses + "/" + GetSelectedTool.MaxUses + " )"
                 : items[selectedIndex] == InventoryItem.Goggles ? " ( " + GetSelectedGoggle.Percent + " ) " + (GetSelectedGoggle.IsEquiped ? "EQUIPED" : "") : ""), new Vector2(174, 570 - Resources.Font.LineSpacing * 1.75f), Color.White);

            for (int i = lethalGrenade.AmountOfGrenades; i > 0; i--)
                sb.Draw(Resources.ItemTextures[GrenadeIDToInventoryItem(lethalGrenade.GrenadeID)], new Rectangle(175 + (i * 17), 530, 35, 35), Color.White);

            for (int i = specialGrenade.AmountOfGrenades; i > 0; i--)
                sb.Draw(Resources.ItemTextures[GrenadeIDToInventoryItem(specialGrenade.GrenadeID)], new Rectangle(310 + (i * 17), 530, 35, 35), Color.White);

        }

        public static InventoryItem GunIDToInventoryItem(byte gunID)
        {
            switch (gunID)
            {
                case GunType.GUNID_AK47:
                    return InventoryItem.AK47;
                case GunType.GUNID_COLT45:
                    return InventoryItem.Colt45;
                case GunType.GUNID_AA12:
                    return InventoryItem.AA12;
                case GunType.GUNID_DOUBLEBARREL:
                    return InventoryItem.DoubleBarrel;
                case GunType.GUNID_FAL:
                    return InventoryItem.FAL;
                case GunType.GUNID_M16:
                    return InventoryItem.M16;
                case GunType.GUNID_MAGNUM:
                    return InventoryItem.Magnum;
                case GunType.GUNID_MP5K:
                    return InventoryItem.MP5K;
                case GunType.GUNID_12GAUGE:
                    return InventoryItem.SingleBarrel;
                case GunType.GUNID_UMP45:
                    return InventoryItem.UMP45;
                case GunType.GUNID_VECTOR:
                    return InventoryItem.Vector;
                case GunType.GUNID_MINIGUN:
                    return InventoryItem.Minigun;
                case GunType.GUNID_SWORD:
                    return InventoryItem.Sword;
            }
            return InventoryItem.AA12;
        }
        public static InventoryItem ToolIDToInventoryItem(byte toolID)
        {
            switch (toolID)
            {
                case ToolType.TOOLID_DIAMONDPICK:
                    return InventoryItem.PickDiamond;
                case ToolType.TOOLID_DIAMONDSHOVEL:
                    return InventoryItem.ShovelDiamond;
                case ToolType.TOOLID_ROCKPICK:
                    return InventoryItem.PickRock;
                case ToolType.TOOLID_ROCKSHOVEL:
                    return InventoryItem.ShovelRock;
                case ToolType.TOOLID_STEELPICK:
                    return InventoryItem.PickSteel;
                case ToolType.TOOLID_STEELSHOVEL:
                    return InventoryItem.ShovelSteel;
            }
            return InventoryItem.PickRock;
        }
        public static InventoryItem GrenadeIDToInventoryItem(byte grenadeID)
        {
            switch (grenadeID)
            {
                case GrenadeType.GRENADE_FLASH:
                    return InventoryItem.FlashBang;
                case GrenadeType.GRENADE_FRAG:
                    return InventoryItem.FragGrenade;
                case GrenadeType.GRENADE_SMOKE:
                    return InventoryItem.SmokeGrenade;
            }
            return InventoryItem.FragGrenade;
        }
        public static byte InventoryItemToID(InventoryItem item)
        {
            if (item == InventoryItem.DirtBlock)
                return Block.BLOCKID_DIRT;
            else if (item == InventoryItem.WoodBlock)
                return Block.BLOCKID_WOOD;
            else if (item == InventoryItem.LeafBlock)
                return Block.BLOCKID_LEAF;
            else if (item == InventoryItem.GrassBlock)
                return Block.BLOCKID_GRASS;
            else if (item == InventoryItem.StoneBlock)
                return Block.BLOCKID_STONE;
            else if (item == InventoryItem.SandBlock)
                return Block.BLOCKID_SAND;
            else if (item == InventoryItem.LavaBucket)
                return Block.BLOCKID_LAVA;
            else if (item == InventoryItem.WaterBucket)
                return Block.BLOCKID_WATER;
            else if (item == InventoryItem.AK47)
                return GunType.GUNID_AK47;
            else if (item == InventoryItem.AA12)
                return GunType.GUNID_AA12;
            else if (item == InventoryItem.Colt45)
                return GunType.GUNID_COLT45;
            else if (item == InventoryItem.DoubleBarrel)
                return GunType.GUNID_DOUBLEBARREL;
            else if (item == InventoryItem.FAL)
                return GunType.GUNID_FAL;
            else if (item == InventoryItem.M16)
                return GunType.GUNID_M16;
            else if (item == InventoryItem.Magnum)
                return GunType.GUNID_MAGNUM;
            else if (item == InventoryItem.MP5K)
                return GunType.GUNID_MP5K;
            else if (item == InventoryItem.SingleBarrel)
                return GunType.GUNID_12GAUGE;
            else if (item == InventoryItem.UMP45)
                return GunType.GUNID_UMP45;
            else if (item == InventoryItem.Vector)
                return GunType.GUNID_VECTOR;
            else if (item == InventoryItem.FragGrenade)
                return GrenadeType.GRENADE_FRAG;
            else if (item == InventoryItem.FlashBang)
                return GrenadeType.GRENADE_FLASH;
            else if (item == InventoryItem.SmokeGrenade)
                return GrenadeType.GRENADE_FLASH;
            else if (item == InventoryItem.BlackBlock)
                return Block.BLOCKID_BLACK;
            else if (item == InventoryItem.BlueBlock)
                return Block.BLOCKID_BLUE;
            else if (item == InventoryItem.GreenBlock)
                return Block.BLOCKID_GREEN;
            else if (item == InventoryItem.GreyBlock)
                return Block.BLOCKID_GREY;
            else if (item == InventoryItem.OrangeBlock)
                return Block.BLOCKID_ORANGE;
            else if (item == InventoryItem.RedBlock)
                return Block.BLOCKID_RED;
            else if (item == InventoryItem.TealBlock)
                return Block.BLOCKID_TEAL;
            else if (item == InventoryItem.WhiteBlock)
                return Block.BLOCKID_WHITE;
            else if (item == InventoryItem.YellowBlock)
                return Block.BLOCKID_YELLOW;
            else if (item == InventoryItem.GlassBlock)
                return Block.BLOCKID_GLASSUSEABLE;
            else if (item == InventoryItem.GlowBlock)
                return Block.BLOCKID_GLOWBLOCK;
            else if (item == InventoryItem.Minigun)
                return GunType.GUNID_MINIGUN;
            else if (item == InventoryItem.Sword)
                return GunType.GUNID_SWORD;
            else if (item == InventoryItem.AK47Spawn)
                return GunType.GUNID_AK47;
            else if (item == InventoryItem.AA12Spawn)
                return GunType.GUNID_AA12;
            else if (item == InventoryItem.Colt45Spawn)
                return GunType.GUNID_COLT45;
            else if (item == InventoryItem.DoubleBarrelSpawn)
                return GunType.GUNID_DOUBLEBARREL;
            else if (item == InventoryItem.FALSpawn)
                return GunType.GUNID_FAL;
            else if (item == InventoryItem.M16Spawn)
                return GunType.GUNID_M16;
            else if (item == InventoryItem.MagnumSpawn)
                return GunType.GUNID_MAGNUM;
            else if (item == InventoryItem.MP5KSpawn)
                return GunType.GUNID_MP5K;
            else if (item == InventoryItem.SingleBarrelSpawn)
                return GunType.GUNID_12GAUGE;
            else if (item == InventoryItem.UMP45Spawn)
                return GunType.GUNID_UMP45;
            else if (item == InventoryItem.VectorSpawn)
                return GunType.GUNID_VECTOR;
            else if (item == InventoryItem.Firebricks)
                return Block.BLOCKID_FIREBRICKS;
            else if (item == InventoryItem.Stonebricks)
                return Block.BLOCKID_STONEBRICKS;
            else if (item == InventoryItem.WoodPlanks)
                return Block.BLOCKID_WOODPLANKS;
            else if (item == InventoryItem.Cobblestone)
                return Block.BLOCKID_COBBLESTONE;
            else if (item == InventoryItem.MossyCobblestone)
                return Block.BLOCKID_MOSSYCOBBLESTONE;
            else if (item == InventoryItem.GoldBlock)
                return Block.BLOCKID_GOLD;
            else if (item == InventoryItem.Pitfall)
                return Block.BLOCKID_PITFALLBLOCK;
            else
                return 0;
        }
        public static string GetItemAsString(InventoryItem item)
        {
            switch (item)
            {
                case InventoryItem.AA12:
                    return "AA-12";
                case InventoryItem.AK47:
                    return "AK-47";
                case InventoryItem.Colt45:
                    return "COLT .45";
                case InventoryItem.DirtBlock:
                    return "DIRT BLOCK";
                case InventoryItem.DoubleBarrel:
                    return "DOUBLE BARREL";
                case InventoryItem.EmptyBucket:
                    return "BUCKET";
                case InventoryItem.FAL:
                    return "FAL";
                case InventoryItem.GrassBlock:
                    return "GRASS BLOCK";
                case InventoryItem.LavaBucket:
                    return "LAVA BUCKET";
                case InventoryItem.WaterBucket:
                    return "WATER BUCKET";
                case InventoryItem.M16:
                    return "M16";
                case InventoryItem.Magnum:
                    return "MAGNUM";
                case InventoryItem.MP5K:
                    return "MP5K";
                case InventoryItem.SandBlock:
                    return "SAND BLOCK";
                case InventoryItem.SingleBarrel:
                    return "12 GAUGE";
                case InventoryItem.StoneBlock:
                    return "STONE BLOCK";
                case InventoryItem.UMP45:
                    return "UMP 45";
                case InventoryItem.Vector:
                    return "VECTOR";
                case InventoryItem.PickDiamond:
                    return "DIAMOND PICK";
                case InventoryItem.PickRock:
                    return "ROCK PICK";
                case InventoryItem.PickSteel:
                    return "STEEL PICK";
                case InventoryItem.ShovelDiamond:
                    return "DIAMOND SHOVEL";
                case InventoryItem.ShovelRock:
                    return "ROCK SHOVEL";
                case InventoryItem.ShovelSteel:
                    return "STEEL SHOVEL";
                case InventoryItem.WoodBlock:
                    return "WOOD BLOCK";
                case InventoryItem.LeafBlock:
                    return "LEAF BLOCK";
                case InventoryItem.TeamASpawn1:
                    return "TEAM A SPAWN POINT 1";
                case InventoryItem.TeamASpawn2:
                    return "TEAM A SPAWN POINT 2";
                case InventoryItem.TeamASpawn3:
                    return "TEAM A SPAWN POINT 3";
                case InventoryItem.TeamBSpawn1:
                    return "TEAM B SPAWN POINT 1";
                case InventoryItem.TeamBSpawn2:
                    return "TEAM B SPAWN POINT 2";
                case InventoryItem.TeamBSpawn3:
                    return "TEAM B SPAWN POINT 3";
                case InventoryItem.Spawn1:
                    return "SPAWN 1";
                case InventoryItem.Spawn2:
                    return "SPAWN 2";
                case InventoryItem.Spawn3:
                    return "SPAWN 3";
                case InventoryItem.Spawn4:
                    return "SPAWN 4";
                case InventoryItem.Spawn5:
                    return "SPAWN 5";
                case InventoryItem.Spawn6:
                    return "SPAWN 6";
                case InventoryItem.ZombieSpawn1:
                    return "ZOMBIE SPAWN 1";
                case InventoryItem.ZombieSpawn2:
                    return "ZOMBIE SPAWN 2";
                case InventoryItem.ZombieSpawn3:
                    return "ZOMBIE SPAWN 3";
                case InventoryItem.ZombieSpawn4:
                    return "ZOMBIE SPAWN 4";
                case InventoryItem.ZombieSpawn5:
                    return "ZOMBIE SPAWN 5";
                case InventoryItem.ZombieSpawn6:
                    return "ZOMBIE SPAWN 6";
                case InventoryItem.KingSpawn:
                    return "KING OF THE BEACH";
                case InventoryItem.SmokeGrenade:
                    return "SMOKE GRENADE";
                case InventoryItem.FragGrenade:
                    return "FRAG GRENADE";
                case InventoryItem.FlashBang:
                    return "FLASH BANG";
                case InventoryItem.GlassBlock:
                    return "GLASS BLOCK";
                case InventoryItem.BlackBlock:
                    return "BLACK BLOCK";
                case InventoryItem.WhiteBlock:
                    return "WHITE BLOCK";
                case InventoryItem.TealBlock:
                    return "TEAL BLOCK";
                case InventoryItem.BlueBlock:
                    return "BLUE BLOCK";
                case InventoryItem.GreenBlock:
                    return "GREEN BLOCK";
                case InventoryItem.GreyBlock:
                    return "GREY BLOCK";
                case InventoryItem.OrangeBlock:
                    return "ORANGE BLOCK";
                case InventoryItem.RedBlock:
                    return "RED BLOCK";
                case InventoryItem.YellowBlock:
                    return "YELLOW BLOCK";
                case InventoryItem.Goggles:
                    return "NIGHT GOGGLES";
                case InventoryItem.GlowBlock:
                    return "GLOW BLOCK";
                case InventoryItem.Minigun:
                    return "MINIGUN";
                case InventoryItem.Sword:
                    return "SWORD";
                case InventoryItem.AA12Spawn:
                    return "AA-12 SPAWNER";
                case InventoryItem.AK47Spawn:
                    return "AK-47 SPAWNER";
                case InventoryItem.Colt45Spawn:
                    return "COLT .45 SPAWNER";
                case InventoryItem.DoubleBarrelSpawn:
                    return "DOUBLE BARREL SPAWNER";
                case InventoryItem.FALSpawn:
                    return "FAL SPAWNER";
                case InventoryItem.M16Spawn:
                    return "M16 SPAWNER";
                case InventoryItem.MagnumSpawn:
                    return "MAGNUM SPAWNER";
                case InventoryItem.MP5KSpawn:
                    return "MP5K SPAWNER";
                case InventoryItem.SingleBarrelSpawn:
                    return "12 GAUGE SPAWNER";
                case InventoryItem.UMP45Spawn:
                    return "UMP 45 SPAWNER";
                case InventoryItem.VectorSpawn:
                    return "VECTOR SPAWNER";
                case InventoryItem.WoodPlanks:
                    return "WOOD PLANKS";
                case InventoryItem.Firebricks:
                    return "FIRE BRICKS";
                case InventoryItem.Stonebricks:
                    return "STONE BRICKS";
                case InventoryItem.Cobblestone:
                    return "COBBLESTONE";
                case InventoryItem.MossyCobblestone:
                    return "MOSSY COBBLESTONE";
                case InventoryItem.GoldBlock:
                    return "GOLD BLOCK";
                case InventoryItem.Pitfall:
                    return "PITFALL BLOCK";
            }
            return "DAN THE MAN MADE THIS GAME";
        }

        #region is statemes
        public static bool IsTool(InventoryItem item)
        {
            if (item == InventoryItem.ShovelDiamond || item == InventoryItem.ShovelRock || item == InventoryItem.ShovelSteel
                || item == InventoryItem.PickDiamond || item == InventoryItem.PickRock || item == InventoryItem.PickSteel)
                return true;
            else
                return false;
        }

        public static bool IsPick(InventoryItem item)
        {
            if (item == InventoryItem.PickDiamond || item == InventoryItem.PickRock || item == InventoryItem.PickSteel)
                return true;
            else
                return false;
        }

        public static bool IsShovel(InventoryItem item)
        {
            if (item == InventoryItem.ShovelDiamond || item == InventoryItem.ShovelRock || item == InventoryItem.ShovelSteel)
                return true;
            else
                return false;
        }

        public static bool IsGun(InventoryItem item)
        {
            if (item == InventoryItem.AK47 || item == InventoryItem.Colt45 || item == InventoryItem.DoubleBarrel ||
                item == InventoryItem.FAL || item == InventoryItem.M16 || item == InventoryItem.Magnum || item == InventoryItem.MP5K ||
                item == InventoryItem.SingleBarrel || item == InventoryItem.UMP45 || item == InventoryItem.Vector || item == InventoryItem.AA12
                || item == InventoryItem.Minigun || item == InventoryItem.Sword)
                return true;
            else
                return false;
        }

        public static bool IsGrenade(InventoryItem item)
        {
            if (item == InventoryItem.FlashBang || item == InventoryItem.FragGrenade || item == InventoryItem.SmokeGrenade)
                return true;
            else
                return false;
        }

        public static bool IsItemBlock(InventoryItem item)
        {
            if(item == InventoryItem.StoneBlock || item == InventoryItem.GrassBlock || item == InventoryItem.DirtBlock || item == InventoryItem.SandBlock
                || item == InventoryItem.LeafBlock || item == InventoryItem.WoodBlock || item == InventoryItem.BlackBlock || item == InventoryItem.BlueBlock
                || item == InventoryItem.GreenBlock || item == InventoryItem.GreyBlock || item == InventoryItem.OrangeBlock || item == InventoryItem.RedBlock
                || item == InventoryItem.TealBlock || item == InventoryItem.WhiteBlock || item == InventoryItem.YellowBlock || item == InventoryItem.GlassBlock
                || item == InventoryItem.GlowBlock || item == InventoryItem.WoodPlanks || item == InventoryItem.Firebricks || item == InventoryItem.Stonebricks
                || item == InventoryItem.MossyCobblestone || item == InventoryItem.Cobblestone
                || item == InventoryItem.GoldBlock || item == InventoryItem.Pitfall)
                return true;
            else
                return false;
        }

        public static bool IsGunSpawner(InventoryItem item)
        {
            if (item == InventoryItem.VectorSpawn || item == InventoryItem.UMP45Spawn || item == InventoryItem.SingleBarrelSpawn
                || item == InventoryItem.MP5KSpawn || item == InventoryItem.MagnumSpawn || item == InventoryItem.M16Spawn
                || item == InventoryItem.FALSpawn || item == InventoryItem.DoubleBarrelSpawn || item == InventoryItem.Colt45Spawn
                || item == InventoryItem.AK47Spawn || item == InventoryItem.AA12Spawn)
                return true;
            else
                return false;
        }

        public static bool IsSpawn(InventoryItem item)
        {
            if (item == InventoryItem.TeamASpawn1 || item == InventoryItem.TeamASpawn2 || item == InventoryItem.TeamASpawn3
                || item == InventoryItem.TeamBSpawn1 || item == InventoryItem.TeamBSpawn2 || item == InventoryItem.TeamBSpawn3
                || item == InventoryItem.Spawn1 || item == InventoryItem.Spawn2 || item == InventoryItem.Spawn3 || item == InventoryItem.Spawn4
                || item == InventoryItem.Spawn5 || item == InventoryItem.Spawn6 || item == InventoryItem.ZombieSpawn1 
                || item == InventoryItem.ZombieSpawn2 || item == InventoryItem.ZombieSpawn3 || item == InventoryItem.ZombieSpawn4
                || item == InventoryItem.ZombieSpawn5 || item == InventoryItem.ZombieSpawn6 || item == InventoryItem.KingSpawn)
                return true;
            else
                return false;
        }

        #endregion

        public static int GetSpawnNumber(InventoryItem spawn)
        {
            if (spawn == InventoryItem.Spawn1 || spawn == InventoryItem.TeamASpawn1 || spawn == InventoryItem.TeamBSpawn1 || spawn == InventoryItem.ZombieSpawn1)
                return 1;
            else if (spawn == InventoryItem.Spawn2 || spawn == InventoryItem.TeamASpawn2 || spawn == InventoryItem.TeamBSpawn2 || spawn == InventoryItem.ZombieSpawn2)
                return 2;
            else if (spawn == InventoryItem.Spawn3 || spawn == InventoryItem.TeamASpawn3 || spawn == InventoryItem.TeamBSpawn3 || spawn == InventoryItem.ZombieSpawn3)
                return 3;
            else if (spawn == InventoryItem.Spawn4 || spawn == InventoryItem.ZombieSpawn4)
                return 4;
            else if (spawn == InventoryItem.Spawn5 || spawn == InventoryItem.ZombieSpawn5)
                return 5;
            else if (spawn == InventoryItem.Spawn6 || spawn == InventoryItem.ZombieSpawn6)
                return 6;
            else
                return 0;
        }

        public static Texture2D GetHoldingBlockTextureForSpawn(InventoryItem spawn)
        {
            if (spawn == InventoryItem.TeamASpawn1 || spawn == InventoryItem.TeamASpawn2 || spawn == InventoryItem.TeamASpawn3)
                return Resources.TeamASpawnBlockTexture;
            else if (spawn == InventoryItem.TeamBSpawn1 || spawn == InventoryItem.TeamBSpawn2 || spawn == InventoryItem.TeamBSpawn3)
                return Resources.TeamBSpawnBlockTexture;
            else if (spawn == InventoryItem.Spawn1 || spawn == InventoryItem.Spawn2 || spawn == InventoryItem.Spawn3 || spawn == InventoryItem.Spawn4
                || spawn == InventoryItem.Spawn5 || spawn == InventoryItem.Spawn6)
                return Resources.SpawnBlockTexture;
            else if (spawn == InventoryItem.ZombieSpawn1 || spawn == InventoryItem.ZombieSpawn2 || spawn == InventoryItem.ZombieSpawn3
                || spawn == InventoryItem.ZombieSpawn4 || spawn == InventoryItem.ZombieSpawn5 || spawn == InventoryItem.ZombieSpawn6)
                return Resources.ZombieBlockTexture;
            else if (spawn == InventoryItem.KingSpawn)
                return Resources.KingBeachBlockTexture;
            else
                return null;
        }
    }
}
