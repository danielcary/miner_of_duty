using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Menus;

namespace Miner_Of_Duty.Game
{


    public class SwarmStoreMenu : IHasCash, IMenuOwner
    {

        public void SelectFirst()
        {
            main.SelectFirst();
        }

        private const int
            COSTMP5K = 1750,
            COSTUMP45 = 2400,
            COSTVECTOR = 2900,
            COSTFAL = 1700,
            COSTM16 = 2300,
            COSTAK47 = 2600,
            COST12GAUGE = 1250,
            COSTDOUBLEBARREL = 2000,
            COSTAA12 = 2500,
            COSTCOLT45 = 1000,
            COSTMAGNUM = 1750,
            COSTROCKSHOVEL = 500,
            COSTSTEELSHOVEL = 600,
            COSTDIAMONDSHOVEL = 750,
            COSTROCKPICK = 400,
            COSTSTEELPICK = 500,
            COSTDIAMONDPICK = 600,
            COSTSTONEBLOCK = 1000,
            COSTDIRTBLOCK = 100,
            COSTSANDBLOCK = 100,
            COSTGLOWBLOCK = 1200;

        public int GetCostOfAmmo(int amountOfAmmoDesired, byte gunID)
        {
            switch (gunID)
            {
                case GunType.GUNID_AA12:
                    return 11 * amountOfAmmoDesired;
                case GunType.GUNID_12GAUGE:
                case GunType.GUNID_DOUBLEBARREL:
                    return 17 * amountOfAmmoDesired;
                case GunType.GUNID_COLT45:
                case GunType.GUNID_MAGNUM:
                    return 15 * amountOfAmmoDesired;
                case GunType.GUNID_M16:
                case GunType.GUNID_FAL:
                case GunType.GUNID_AK47:
                    return 12 * amountOfAmmoDesired;
                case GunType.GUNID_VECTOR:
                case GunType.GUNID_UMP45:
                case GunType.GUNID_MP5K:
                    return 11 * amountOfAmmoDesired;
            }

            return 0;
        }

        public int GetTradeInValueOfWorkingSlot()
        {
            if (IsWorkingSlotAItemSlot())
            {
                switch (GetSelectedSlotAsItem().Item)
                {
                    case InventoryItem.StoneBlock:
                        return (int)(COSTSTONEBLOCK * .5f);
                    case InventoryItem.DirtBlock:
                        return (int)(COSTDIRTBLOCK * .5f);
                    case InventoryItem.SandBlock:
                        return (int)(COSTSANDBLOCK * .5f);
                    case InventoryItem.GlowBlock:
                        return (int)(COSTGLOWBLOCK * .5f);
                }
                return 0;
            }
            else if (IsWorkingSlotAToolSlot())
            {
                switch (GetSelectedSlotAsTool().ToolTypeID)
                {
                    case ToolType.TOOLID_ROCKPICK:
                        return (int)(COSTROCKPICK * .5f);
                    case ToolType.TOOLID_ROCKSHOVEL:
                        return (int)(COSTROCKSHOVEL * .5f);
                    case ToolType.TOOLID_STEELPICK:
                        return (int)(COSTSTEELPICK * .5f);
                    case ToolType.TOOLID_STEELSHOVEL:
                        return (int)(COSTSTEELSHOVEL * .5f);
                    case ToolType.TOOLID_DIAMONDPICK:
                        return (int)(COSTDIAMONDPICK * .5f);
                    case ToolType.TOOLID_DIAMONDSHOVEL:
                        return (int)(COSTDIAMONDSHOVEL * .5f);
                }
                return 0;
            }
            else if (IsWorkingSlotAWeaponSlot())
            {
                float gunVal = 0;
                switch (GetSelectedSlotAsWeapon().GunID)
                {
                    case GunType.GUNID_12GAUGE:
                        gunVal = COST12GAUGE;
                        break;
                    case GunType.GUNID_AA12:
                        gunVal = COSTAA12;
                        break;
                    case GunType.GUNID_AK47:
                        gunVal = COSTAK47;
                        break;
                    case GunType.GUNID_COLT45:
                        gunVal = COSTCOLT45;
                        break;
                    case GunType.GUNID_DOUBLEBARREL:
                        gunVal = COSTDOUBLEBARREL;
                        break;
                    case GunType.GUNID_FAL:
                        gunVal = COSTFAL;
                        break;
                    case GunType.GUNID_M16:
                        gunVal = COSTM16;
                        break;
                    case GunType.GUNID_MAGNUM:
                        gunVal = COSTMAGNUM;
                        break;
                    case GunType.GUNID_MP5K:
                        gunVal = COSTMP5K;
                        break;
                    case GunType.GUNID_UMP45:
                        gunVal = COSTUMP45;
                        break;
                    case GunType.GUNID_VECTOR:
                        gunVal = COSTVECTOR;
                        break;
                }
                gunVal *= .4f;
                return (int)(gunVal + (GetCostOfAmmo((workingInventory.useableItems[slotNumber - 1] as Gun).CurrentAmmo, GetSelectedSlotAsWeapon().GunID) * .6f));
            }
            else
                return 0;
        }

        public void BuyGun(byte gunID)
        {
            switch (gunID)
            {
                case GunType.GUNID_MP5K:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTMP5K;
                    break;
                case GunType.GUNID_UMP45:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTUMP45;
                    break;
                case GunType.GUNID_VECTOR:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTVECTOR;
                    break;
                case GunType.GUNID_FAL:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTFAL;
                    break;
                case GunType.GUNID_M16:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTM16;
                    break;
                case GunType.GUNID_AK47:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTAK47;
                    break;
                case GunType.GUNID_12GAUGE:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COST12GAUGE;
                    break;
                case GunType.GUNID_DOUBLEBARREL:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTDOUBLEBARREL;
                    break;
                case GunType.GUNID_AA12:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTAA12;
                    break;
                case GunType.GUNID_COLT45:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTCOLT45;
                    break;
                case GunType.GUNID_MAGNUM:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTMAGNUM;
                    break;
            }
            SetNewSlotOnWorkingSlot(new WeaponSlot(gunID, false, false, false));
            if (workingInventory.useableItems.ContainsKey(slotNumber - 1))
                workingInventory.useableItems[slotNumber - 1] = new Gun(gunID, player.BurstFire, false, false, false);
            else
                workingInventory.useableItems.Add(slotNumber - 1, new Gun(gunID, player.BurstFire, false, false, false));
            workingInventory.items[slotNumber - 1] = Inventory.GunIDToInventoryItem(gunID);
            (workingInventory.useableItems[slotNumber - 1] as Gun).MaxFillAmmo();
        }


        public void BuyToolie(byte toolID)
        {
            switch (toolID)
            {
                case ToolType.TOOLID_ROCKPICK:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTROCKPICK;
                    break;
                case ToolType.TOOLID_STEELPICK:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTSTEELPICK;
                    break;
                case ToolType.TOOLID_DIAMONDPICK:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTDIAMONDPICK;
                    break;
                case ToolType.TOOLID_ROCKSHOVEL:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTROCKSHOVEL;
                    break;
                case ToolType.TOOLID_STEELSHOVEL:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTSTEELSHOVEL;
                    break;
                case ToolType.TOOLID_DIAMONDSHOVEL:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTDIAMONDSHOVEL;
                    break;
            }
            SetNewSlotOnWorkingSlot(new ToolSlot(toolID, false, false));
            if (workingInventory.useableItems.ContainsKey(slotNumber - 1))
                workingInventory.useableItems[slotNumber - 1] = new Tool(toolID, false, false);
            else
                workingInventory.useableItems.Add(slotNumber - 1, new Tool(toolID, false, false));
            workingInventory.items[slotNumber - 1] = Inventory.ToolIDToInventoryItem(toolID);
        }


        public void BuyItemie(InventoryItem item)
        {
            switch (item)
            {
                case InventoryItem.StoneBlock:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTSTONEBLOCK;
                    break;
                case InventoryItem.DirtBlock:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTDIRTBLOCK;
                    break;
                case InventoryItem.SandBlock:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTSANDBLOCK;
                    break;
                case InventoryItem.GlowBlock:
                    cash += GetTradeInValueOfWorkingSlot();
                    cash -= COSTGLOWBLOCK;
                    break;
            }
            SetNewSlotOnWorkingSlot(new ItemSlot(item));
            workingInventory.items[slotNumber - 1] = item;
        }


        private Menu main, trade, tradeAmmo, tradeList, smgMenu, assualtRifleMenu, shotgunMenu, toolMenu, itemMenu, pistolMenu;
        private Menu workingMenu;
        private Menu.BackPressed back;
        private CharacterClass workingClass;
        private Inventory workingInventory;
        public int cash;
        public int Cash { get { return cash; } set { cash = value; } }
        public int ExtraCash { get { return GetTradeInValueOfWorkingSlot(); } }


        private Player player;

        public SwarmStoreMenu(Menu.BackPressed back)
        {
            this.back = back;
            main = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("leave", "Leave Store"),
                new MenuElement("item1", "item1"),
                new MenuElement("item2", "item2"),
                new MenuElement("item3", "item3"),
                new MenuElement("item4", "item4"),
                new CostMenuElement("grenade", "Buy a Grenade", 300, this, false), 
            }, 75);
            workingMenu = main;
            main["leave"].Position.X -= 50;
            tradeAmmo = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("hidden",""),
                new MenuElement("trade", "Trade"),
                new CostMenuElement("ammo", "Buy Ammo", 0, this, false)}, 75, 1);

            tradeAmmo["hidden"].Position.X -= 50;

            trade = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("hidden",""),
                new MenuElement("trade", "Trade")}, 75, 1);

            trade["hidden"].Position.X -= 50;

            tradeList = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("back", "back"),
                new MenuElement("smg", "SMGs"),
                new MenuElement("assualt", "Assualt Rifle"),
                new MenuElement("shot", "Shotguns"),
                new MenuElement("pistol", "Pistols"),
                new MenuElement("tools", "Tools"),
                new MenuElement("items", "Items"),
            }, 75);

            tradeList["back"].Position.X -= 50f;
            
            smgMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new CostMenuElement("MP5K", "MP5K", COSTMP5K, this, true),
                new CostMenuElement("UMP45", "UMP45", COSTUMP45, this, true),
                new CostMenuElement("vector", "VECTOR", COSTVECTOR, this, true)}, 75);

            smgMenu["back"].Position.X -= 50f;

            assualtRifleMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new CostMenuElement("FAL", "FAL", COSTFAL, this, true),
                new CostMenuElement("M16", "M16", COSTM16, this, true),
                new CostMenuElement("AK47", "AK-47", COSTAK47, this, true)}, 75);

            assualtRifleMenu["back"].Position.X -= 50f;

            shotgunMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new CostMenuElement("12guage", "12 Gauge", COST12GAUGE, this, true),
                new CostMenuElement("doublebarrel", "Double Barrel", COSTDOUBLEBARREL, this, true),
                new CostMenuElement("aa12", "aa-12", COSTAA12, this, true),}, 75);

            shotgunMenu["back"].Position.X -= 50f;

            pistolMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new CostMenuElement("colt45", "Colt .45", COSTCOLT45, this, true),
                new CostMenuElement("magnum", ".357 Magnum", COSTMAGNUM, this, true)}, 75);

            pistolMenu["back"].Position.X -= 50f;

            toolMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new CostMenuElement("bShovel", "Rock Shovel", COSTROCKSHOVEL, this, true),
                new CostMenuElement("sShovel", "Steel Shovel", COSTSTEELSHOVEL, this, true),
                new CostMenuElement("dShovel", "Diamond Shovel", COSTDIAMONDSHOVEL, this, true),
                new CostMenuElement("bPick", "Rock Pick" , COSTROCKPICK, this, true),
                new CostMenuElement("sPick", "Steel Pick", COSTSTEELPICK, this, true),
                new CostMenuElement("dPick", "Diamond Pick", COSTDIAMONDPICK, this, true)}, 75);

            toolMenu["back"].Position.X -= 50f;

            itemMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new CostMenuElement("stoneblock", "Stone Block", COSTSTONEBLOCK, this, true),
                new CostMenuElement("dirtblock", "Dirt Block", COSTDIRTBLOCK, this, true),
                new CostMenuElement("sandblock", "Sand Block", COSTSANDBLOCK, this, true),
                new CostMenuElement("glowblock", "Glow Block", COSTGLOWBLOCK, this, true),}, 75);

            itemMenu["back"].Position.X -= 50f;
        }

        #region poop
        private void SetNewSlotOnWorkingSlot(Slot slot)
        {
            if (slotNumber == 1)
                workingClass.Slot1 = slot;
            else if (slotNumber == 2)
                workingClass.Slot2 = slot;
            else if (slotNumber == 3)
                workingClass.Slot3 = slot;
            else
                workingClass.Slot4 = slot;
        }

        private bool IsWorkingSlotAWeaponSlot()
        {
            if (slotNumber == 1)
            {
                return workingClass.Slot1 is WeaponSlot;
            }
            else if (slotNumber == 2)
            {
                return workingClass.Slot2 is WeaponSlot;
            }
            else if (slotNumber == 3)
            {
                return workingClass.Slot3 is WeaponSlot;
            }
            else if (slotNumber == 4)
                return workingClass.Slot4 is WeaponSlot;//I <3 C#
            else return false;
        }

        private WeaponSlot GetSelectedSlotAsWeapon()
        {
            if (slotNumber == 1)
            {
                return workingClass.Slot1 as WeaponSlot;
            }
            else if (slotNumber == 2)
            {
                return workingClass.Slot2 as WeaponSlot;
            }
            else if (slotNumber == 3)
            {
                return workingClass.Slot3 as WeaponSlot;
            }
            else
                return workingClass.Slot4 as WeaponSlot;
        }

        private bool IsWorkingSlotAToolSlot()
        {
            if (slotNumber == 1)
            {
                return workingClass.Slot1 is ToolSlot;
            }
            else if (slotNumber == 2)
            {
                return workingClass.Slot2 is ToolSlot;
            }
            else if (slotNumber == 3)
            {
                return workingClass.Slot3 is ToolSlot;
            }
            else if (slotNumber == 4)
                return workingClass.Slot4 is ToolSlot;//I <3 C#
            else return false;
        }

        private ToolSlot GetSelectedSlotAsTool()
        {
            if (slotNumber == 1)
            {
                return workingClass.Slot1 as ToolSlot;
            }
            else if (slotNumber == 2)
            {
                return workingClass.Slot2 as ToolSlot;
            }
            else if (slotNumber == 3)
            {
                return workingClass.Slot3 as ToolSlot;
            }
            else
                return workingClass.Slot4 as ToolSlot;
        }

        private bool IsWorkingSlotAItemSlot()
        {
            if (slotNumber == 1)
            {
                return workingClass.Slot1 is ItemSlot;
            }
            else if (slotNumber == 2)
            {
                return workingClass.Slot2 is ItemSlot;
            }
            else if (slotNumber == 3)
            {
                return workingClass.Slot3 is ItemSlot;
            }
            else if (slotNumber == 4)
                return workingClass.Slot4 is ItemSlot;//I <3 C#
            else return false;
        }

        private ItemSlot GetSelectedSlotAsItem()
        {
            if (slotNumber == 1)
            {
                return workingClass.Slot1 as ItemSlot;
            }
            else if (slotNumber == 2)
            {
                return workingClass.Slot2 as ItemSlot;
            }
            else if (slotNumber == 3)
            {
                return workingClass.Slot3 as ItemSlot;
            }
            else
                return workingClass.Slot4 as ItemSlot;
        }
        #endregion

        private int slotNumber;
        public void Choose(object sender, string id)
        {
            if (sender == main)
            {
                if (id == "leave")
                {
                    back.Invoke(this);
                }
                else if (id == "item1")
                {
                    if (workingClass.Slot1 is WeaponSlot)
                    {
                        workingMenu = tradeAmmo;
                        tradeAmmo["hidden"].Text = GetNameFromSlot(workingClass.Slot1);
                        int cost = GetCostOfAmmo((workingInventory.useableItems[0] as Gun).GunType.MaxAmmo - (workingInventory.useableItems[0] as Gun).CurrentAmmo,
                            (workingClass.Slot1 as WeaponSlot).GunID);
                        if (cost == 0)
                            (tradeAmmo["ammo"] as CostMenuElement).Grayed = true;
                        else
                            (tradeAmmo["ammo"] as CostMenuElement).Grayed = false;
                        (tradeAmmo["ammo"] as CostMenuElement).SetCost(cost);
                    }
                    else
                    {
                        trade["hidden"].Text = GetNameFromSlot(workingClass.Slot1);
                        workingMenu = trade;
                    }
                    slotNumber = 1;
                }
                else if (id == "item2")
                {
                    if (workingClass.Slot2 is WeaponSlot)
                    {
                        workingMenu = tradeAmmo;
                        tradeAmmo["hidden"].Text = GetNameFromSlot(workingClass.Slot2);
                        int cost = GetCostOfAmmo((workingInventory.useableItems[1] as Gun).GunType.MaxAmmo - (workingInventory.useableItems[1] as Gun).CurrentAmmo,
                            (workingClass.Slot2 as WeaponSlot).GunID);
                        if (cost == 0)
                            (tradeAmmo["ammo"] as CostMenuElement).Grayed = true;
                        else
                            (tradeAmmo["ammo"] as CostMenuElement).Grayed = false;
                        (tradeAmmo["ammo"] as CostMenuElement).SetCost(cost);
                    }
                    else
                    {
                        trade["hidden"].Text = GetNameFromSlot(workingClass.Slot2);
                        workingMenu = trade;
                    }
                    slotNumber = 2;
                }
                else if (id == "item3")
                {
                    if (workingClass.Slot3 is WeaponSlot)
                    {
                        workingMenu = tradeAmmo;
                        tradeAmmo["hidden"].Text = GetNameFromSlot(workingClass.Slot3);
                        int cost = GetCostOfAmmo((workingInventory.useableItems[2] as Gun).GunType.MaxAmmo - (workingInventory.useableItems[2] as Gun).CurrentAmmo,
                            (workingClass.Slot3 as WeaponSlot).GunID);
                        if (cost == 0)
                            (tradeAmmo["ammo"] as CostMenuElement).Grayed = true;
                        else
                            (tradeAmmo["ammo"] as CostMenuElement).Grayed = false;
                        (tradeAmmo["ammo"] as CostMenuElement).SetCost(cost);
                    }
                    else
                    {
                        trade["hidden"].Text = GetNameFromSlot(workingClass.Slot3);
                        workingMenu = trade;
                    }
                    slotNumber = 3;
                }
                else if (id == "item4")
                {
                    if (workingClass.Slot4 is WeaponSlot)
                    {
                        workingMenu = tradeAmmo;
                        tradeAmmo["hidden"].Text = GetNameFromSlot(workingClass.Slot4);
                        int cost = GetCostOfAmmo((workingInventory.useableItems[3] as Gun).GunType.MaxAmmo - (workingInventory.useableItems[3] as Gun).CurrentAmmo,
                            (workingClass.Slot4 as WeaponSlot).GunID);
                        if (cost == 0)
                            (tradeAmmo["ammo"] as CostMenuElement).Grayed = true;
                        else
                            (tradeAmmo["ammo"] as CostMenuElement).Grayed = false;
                        (tradeAmmo["ammo"] as CostMenuElement).SetCost(cost);
                    }
                    else
                    {
                        trade["hidden"].Text = GetNameFromSlot(workingClass.Slot4);
                        workingMenu = trade;
                    }
                    slotNumber = 4;
                }
                else if (id == "grenade")
                {
                    cash -= 300;
                    player.inventory.GetLethalGrenade.AddGrenade();
                    if (player.inventory.GetLethalGrenade.AmountOfGrenades < 3)
                        (main["grenade"] as CostMenuElement).Grayed = false;
                    else
                        (main["grenade"] as CostMenuElement).Grayed = true;
                }
            }
            else if (sender == trade || sender == tradeAmmo)
            {
                if (id == "trade")
                {
                    workingMenu = tradeList;
                }
                else if (id == "ammo")
                {
                    int cost = GetCostOfAmmo((workingInventory.useableItems[slotNumber - 1] as Gun).GunType.MaxAmmo - (workingInventory.useableItems[slotNumber - 1] as Gun).CurrentAmmo,
                        GetSelectedSlotAsWeapon().GunID);
                    if (cost <= cash)
                    {
                        cash -= cost;
                        (workingInventory.useableItems[slotNumber - 1] as Gun).MaxFillAmmo();
                        workingMenu = main;
                        slotNumber = 0;
                    }
                }
            }
            else if (sender == tradeList)
            {
                #region tradelist
                if (id == "back")
                {
                    workingMenu = main;
                    slotNumber = 0;
                }
                else if (id == "smg")
                {
                    workingMenu = smgMenu;
                    (smgMenu["vector"] as CostMenuElement).Grayed = false;
                    (smgMenu["UMP45"]as CostMenuElement).Grayed = false;
                    (smgMenu["MP5K"] as CostMenuElement).Grayed = false;
                    if (IsWorkingSlotAWeaponSlot())
                    {
                        if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_MP5K)
                            (smgMenu["MP5K"] as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_UMP45)
                            (smgMenu["UMP45"] as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_VECTOR)
                            (smgMenu["vector"] as CostMenuElement).Grayed = true;
                    }
                }
                else if (id == "assualt")
                {
                    workingMenu = assualtRifleMenu;
                    (assualtRifleMenu["FAL"]as CostMenuElement).Grayed = false;
                    (assualtRifleMenu["M16"]as CostMenuElement).Grayed = false;
                    (assualtRifleMenu["AK47"]as CostMenuElement).Grayed = false;
                    if (IsWorkingSlotAWeaponSlot())
                    {
                        if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_FAL)
                            (assualtRifleMenu["FAL"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_M16)
                            (assualtRifleMenu["M16"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_AK47)
                            (assualtRifleMenu["AK47"]as CostMenuElement).Grayed = true;
                    }
                }
                else if (id == "shot")
                {
                    workingMenu = shotgunMenu;
                    (shotgunMenu["12guage"]as CostMenuElement).Grayed = false;
                    (shotgunMenu["doublebarrel"]as CostMenuElement).Grayed = false;
                    (shotgunMenu["aa12"]as CostMenuElement).Grayed = false;
                    if (IsWorkingSlotAWeaponSlot())
                    {
                        if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_12GAUGE)
                            (shotgunMenu["12guage"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_DOUBLEBARREL)
                            (shotgunMenu["doublebarrel"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_AA12)
                            (shotgunMenu["aa12"]as CostMenuElement).Grayed = true;
                    }
                }
                else if (id == "pistol")
                {
                    workingMenu = pistolMenu;
                    (pistolMenu["colt45"]as CostMenuElement).Grayed = false;
                    (pistolMenu["magnum"]as CostMenuElement).Grayed = false;
                    if (IsWorkingSlotAWeaponSlot())
                    {
                        if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_COLT45)
                            (pistolMenu["colt45"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsWeapon().GunID == GunType.GUNID_MAGNUM)
                            (pistolMenu["magnum"]as CostMenuElement).Grayed = true;
                    }
                }
                else if (id == "tools")
                {
                    workingMenu = toolMenu;
                    (toolMenu["bShovel"]as CostMenuElement).Grayed = false;
                    (toolMenu["sShovel"]as CostMenuElement).Grayed = false;
                    (toolMenu["dShovel"]as CostMenuElement).Grayed = false;
                    (toolMenu["bPick"]as CostMenuElement).Grayed = false;
                   ( toolMenu["sPick"]as CostMenuElement).Grayed = false;
                    (toolMenu["dPick"]as CostMenuElement).Grayed = false;
                    if (IsWorkingSlotAToolSlot())
                    {
                        if (GetSelectedSlotAsTool().ToolTypeID == ToolType.TOOLID_ROCKSHOVEL)
                            (toolMenu["bShovel"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsTool().ToolTypeID == ToolType.TOOLID_STEELSHOVEL)
                           ( toolMenu["sShovel"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsTool().ToolTypeID == ToolType.TOOLID_DIAMONDSHOVEL)
                           ( toolMenu["dShovel"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsTool().ToolTypeID == ToolType.TOOLID_ROCKPICK)
                            (toolMenu["bPick"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsTool().ToolTypeID == ToolType.TOOLID_STEELPICK)
                            (toolMenu["sPick"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsTool().ToolTypeID == ToolType.TOOLID_DIAMONDPICK)
                            (toolMenu["dPick"]as CostMenuElement).Grayed = true;
                    }
                }
                else if (id == "items")
                {
                    workingMenu = itemMenu;
                    (itemMenu["stoneblock"]as CostMenuElement).Grayed = false;
                    (itemMenu["dirtblock"]as CostMenuElement).Grayed = false;
                    (itemMenu["sandblock"]as CostMenuElement).Grayed = false;
                    (itemMenu["glowblock"] as CostMenuElement).Grayed = false;
                    if (IsWorkingSlotAItemSlot())
                    {
                        if (GetSelectedSlotAsItem().Item == InventoryItem.DirtBlock)
                            (itemMenu["stoneblock"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsItem().Item == InventoryItem.DirtBlock)
                            (itemMenu["dirtblock"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsItem().Item == InventoryItem.SandBlock)
                            (itemMenu["sandblock"]as CostMenuElement).Grayed = true;
                        else if (GetSelectedSlotAsItem().Item == InventoryItem.GlowBlock)
                            (itemMenu["glowblock"] as CostMenuElement).Grayed = true;
                    }
                }
                #endregion
            }
            else if (sender == smgMenu || sender == assualtRifleMenu || sender == shotgunMenu || sender == pistolMenu
                || sender == toolMenu || sender == itemMenu)
            {
                #region buy item
                if (id == "back")
                {
                    workingMenu = tradeList;
                }
                else if (id == "aa12")
                    BuyWeapon(GunType.GUNID_AA12);
                else if (id == "MP5K")
                    BuyWeapon(GunType.GUNID_MP5K);
                else if (id == "UMP45")
                    BuyWeapon(GunType.GUNID_UMP45);
                else if (id == "vector")
                    BuyWeapon(GunType.GUNID_VECTOR);
                else if (id == "FAL")
                    BuyWeapon(GunType.GUNID_FAL);
                else if (id == "M16")
                    BuyWeapon(GunType.GUNID_M16);
                else if (id == "AK47")
                    BuyWeapon(GunType.GUNID_AK47);
                else if (id == "12guage")
                    BuyWeapon(GunType.GUNID_12GAUGE);
                else if (id == "doublebarrel")
                    BuyWeapon(GunType.GUNID_DOUBLEBARREL);
                else if (id == "colt45")
                    BuyWeapon(GunType.GUNID_COLT45);
                else if (id == "magnum")
                    BuyWeapon(GunType.GUNID_MAGNUM);
                else if (id == "bShovel")
                    BuyTool(ToolType.TOOLID_ROCKSHOVEL);
                else if (id == "sShovel")
                    BuyTool(ToolType.TOOLID_STEELSHOVEL);
                else if (id == "dShovel")
                    BuyTool(ToolType.TOOLID_DIAMONDSHOVEL);
                else if (id == "bPick")
                    BuyTool(ToolType.TOOLID_ROCKPICK);
                else if (id == "sPick")
                    BuyTool(ToolType.TOOLID_STEELPICK);
                else if (id == "dPick")
                    BuyTool(ToolType.TOOLID_DIAMONDPICK);
                else if (id == "stoneblock")
                    BuyItem(InventoryItem.StoneBlock);
                else if (id == "dirtblock")
                    BuyItem(InventoryItem.DirtBlock);
                else if (id == "sandblock")
                    BuyItem(InventoryItem.SandBlock);
                else if (id == "glowblock")
                    BuyItem(InventoryItem.GlowBlock);
                #endregion
            }

        }

        public void Back(object sender)
        {
            if (sender == main)
            {
                back.Invoke(this);
            }
            else if (sender == trade || sender == tradeAmmo)
            {
                slotNumber = 0;
                workingMenu = main;
            }
            else if (sender == tradeList)
            {
                workingMenu = main;
                slotNumber = 0;
            }
            else if (sender == smgMenu || sender == assualtRifleMenu || sender == shotgunMenu ||
                sender == toolMenu || sender == itemMenu || sender == pistolMenu)
            {
                workingMenu = tradeList;
            }
        }

        #region callbacks
        private byte weaponID;
        private void BuyWeaponCallback(int selected)
        {
            if (selected == 0)
            {
                BuyGun(weaponID);
                SetClass(workingClass, workingInventory, player);//upadates
                workingMenu = main;
                slotNumber = 0;
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private byte toolID;
        private void BuyToolCallback(int selected)
        {
            if (selected == 0)
            {
                BuyToolie(toolID);
                SetClass(workingClass, workingInventory, player);
                workingMenu = main;
                slotNumber = 0;
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private InventoryItem item;
        private void BuyItemCallback(int selected)
        {
            if (selected == 0)
            {
                BuyItemie(item);
                SetClass(workingClass, workingInventory, player);
                workingMenu = main;
                slotNumber = 0;
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }
        #endregion

        #region buyies
        public void BuyWeapon(byte weaponID)
        {
            this.weaponID = weaponID;
            MessageBox.ShowMessageBox(BuyWeaponCallback, new string[] { "YES, BUY", "NO, DON'T BUY" }, 1, new string[] { "ARE YOU SURE YOU WANT", "TO BUY A " + Inventory.GetItemAsString(Inventory.GunIDToInventoryItem(weaponID)) });
        }

        public void BuyTool(byte toolID)
        {
            this.toolID = toolID;
            MessageBox.ShowMessageBox(BuyToolCallback, new string[] { "YES, BUY", "NO, DON'T BUY" }, 1, new string[] { "ARE YOU SURE YOU WANT", "TO BUY A " + Inventory.GetItemAsString(Inventory.ToolIDToInventoryItem(toolID)) });
        }

        public void BuyItem(InventoryItem item)
        {
            this.item = item;
            MessageBox.ShowMessageBox(BuyItemCallback, new string[] { "YES, BUY", "NO, DON'T BUY" }, 1, new string[] { "ARE YOU SURE YOU WANT", "TO BUY A " + Inventory.GetItemAsString(item) });
        }
        #endregion

        public string GetNameFromSlot(Slot slot)
        {
            if (slot is WeaponSlot)
            {
                return Inventory.GetItemAsString(Inventory.GunIDToInventoryItem((slot as WeaponSlot).GunID));//yay
            }
            else if (slot is ToolSlot)
            {
                return Inventory.GetItemAsString(Inventory.ToolIDToInventoryItem((slot as ToolSlot).ToolTypeID));
            }
            else if (slot is ItemSlot)
            {
                return Inventory.GetItemAsString((slot as ItemSlot).Item);
            }
            else
                return "error";
        }

        public void SetClass(CharacterClass clas, Inventory inventory, Player player)
        {
            workingClass = clas;
            workingInventory = inventory;
            this.player = player;
            main["item1"].Text = GetNameFromSlot(clas.Slot1);
            main["item2"].Text = GetNameFromSlot(clas.Slot2);
            main["item3"].Text = GetNameFromSlot(clas.Slot3);
            main["item4"].Text = GetNameFromSlot(clas.Slot4);
            if (player.inventory.GetLethalGrenade.AmountOfGrenades < 3)
                (main["grenade"] as CostMenuElement).Grayed = false;
            else
                (main["grenade"] as CostMenuElement).Grayed = true;
        }

        public void Update(GameTime gameTime)
        {
            workingMenu.Update((short)gameTime.ElapsedGameTime.Milliseconds);
        }

        public void Draw(SpriteBatch sb)
        {
            workingMenu.Draw(sb);
            sb.DrawString(Resources.NameFont, "Total Credits:", new Vector2(900, 150), Color.White, 0, Resources.NameFont.MeasureString("Total Credits:") / 2f, 1, SpriteEffects.None, 0);
            sb.DrawString(Resources.NameFont, "$" + cash, new Vector2(900, 150 + Resources.NameFont.LineSpacing), Color.White, 0, Resources.NameFont.MeasureString("$" + cash) / 2f, 1, SpriteEffects.None, 0);
            if (ExtraCash > 0)
            {
                sb.DrawString(Resources.NameFont, "Trade In Value:", new Vector2(900, 150 + 3 * Resources.NameFont.LineSpacing), Color.White, 0, Resources.NameFont.MeasureString("Trade In Value:") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.NameFont, "$" + ExtraCash, new Vector2(900, 150 + 4 * Resources.NameFont.LineSpacing), Color.White, 0, Resources.NameFont.MeasureString("$" + ExtraCash) / 2f, 1, SpriteEffects.None, 0);
            }
        }




        public void Update(short timePassedInMilliseconds)
        {
            
        }
    }
}
