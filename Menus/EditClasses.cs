using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Game;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Menus
{
    public class EditClasses : IMenuOwner
    {
        public void SelectFirst()
        {
            updating.SelectFirst();
        }

        public void GoHome()
        {
            updating = classMenu;
            updating.SelectFirst();
        }

        private Menu.BackPressed back;

        private Menu classMenu, classEditMenu, slotChangeItemsMenu, slotChangeAnyMenu, slotChangeToolPistolMenu;
        private Menu smgMenu, pistolMenu, toolMenu, assualtMenu, shotgunMenu;
        private Menu falMenu, ak47Menu, m16Menu, aa12Menu, _12guageMenu, doubleBarrelMenu;
        private Menu ump45Menu, mp5kMenu, vectorMenu, colt45Menu, magnumMenu;
        private Menu specialWeapons;
        private Menu bronzeShovelMenu, steelShovelMenu, diamondShovelMenu, bronzePickMenu, steelPickMenu, diamondPickMenu;
        private Menu playerUpgradesMenu;
        private Menu lethalGrenadeMenu, specialGrenadeMenu;

        private Menu updating;
        private CharacterClass workingClass;
        private Slot workingSlot;
        private int slotNumber;
        private Menu whereICameFrom, whereICameFrom2;
        private ClassViewer viewer;

        public EditClasses(Menu.BackPressed back)
        {
            this.back = back;

            classMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new LockedMenuElement("smg", "SMG"),
                new LockedMenuElement("assualtrifle", "Assault Rifle"),
                new LockedMenuElement("shot", "Shotgun"),
                new LockedMenuElement("builder", "Builder"),
                new MenuElement("custom1", "Custom Class 1"),
                new MenuElement("custom2", "Custom Class 2"),
                new MenuElement("custom3", "Custom Class 3"),
                new MenuElement("custom4", "Custom Class 4"),
                new MenuElement("custom5", "Custom Class 5")});

            classMenu.SelectedIndexChangedEvent += new Menu.SelectedIndexChanged(classMenu_SelectedIndexChangedEvent);

            CharacterClass_CharacterClassRenamedEvent();
            CharacterClass.CharacterClassRenamedEvent += new CharacterClass.CharacterClassRenamed(CharacterClass_CharacterClassRenamedEvent);
            PlayerProfile.PlayerProfileReloadedEvent += CharacterClass_CharacterClassRenamedEvent;

            classEditMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("slot1", "Slot 1"),
                new MenuElement("slot2", "Slot 2"),
                new MenuElement("slot3", "Slot 3"),//For Name use weapon name
                new MenuElement("slot4", "Slot 4"),
                new MenuElement("playerupgrades", "Player Upgrades"),
                new MenuElement("lethal","lethal grenades"),
                new MenuElement("special", "special grenades"),
                new MenuElement("rename", "Rename")});

            playerUpgradesMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("morestamina", "Endurance", PlayerProfile.ENDURANCE),
				new UnlockableMenuElement("thickerskin", "Thick Skin", PlayerProfile.THICKSKIN),
				new UnlockableMenuElement("quickhands", "Quick Hands", PlayerProfile.QUICKHANDS)});

            lethalGrenadeMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "back")
                    {
                        updating = classEditMenu;
                    }
                    else if (id == "frag")
                    {
                        workingClass.LethalGrenadeID = GrenadeType.GRENADE_FRAG;
                        MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                        viewer.SetCharacterClass(workingClass);
                    }
                }, delegate(object sender)
                {
                    updating = classEditMenu;
                }, new MenuElement[]{
                    new MenuElement("back", "BACK"),
                    new MenuElement("frag", "FRAG GRENADE"),
                });

            specialGrenadeMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "back")
                        updating = classEditMenu;
                    else if (id == "smoke")
                    {
                        workingClass.SpecialGrenadeID = GrenadeType.GRENADE_SMOKE;
                        MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                        viewer.SetCharacterClass(workingClass);
                    }
                    else if (id == "flash")
                    {
                        workingClass.SpecialGrenadeID = GrenadeType.GRENADE_FLASH;
                        MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                        viewer.SetCharacterClass(workingClass);
                    }
                }, delegate(object sender)
                {
                    updating = classEditMenu;
                }, new MenuElement[]{
                    new MenuElement("back", "back"),
                    new MenuElement("smoke", "Smoke grenade"),
                    new MenuElement("flash", "Flash bang"),
                });

            slotChangeItemsMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("emptyBucket", "Empty Bucket"),
                new UnlockableMenuElement("lavaBucket", "Lava Bucket", PlayerProfile.LAVABUCKET),
                new MenuElement("stoneblock", "Stone Block"),
                new MenuElement("dirtblock", "Dirt Block"),
                new MenuElement("grassblock", "Grass Block"),
                new MenuElement("sandblock", "Sand Block"),
                new UnlockableMenuElement("pitfall", "PITFALL BLOCK", PlayerProfile.PITFALL),
                new MenuElement("glowblock", "Glow Block"),
                new MenuElement("goggles", "Night Vision Goggles")});

            slotChangeAnyMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("smg", "SMGs"),
                new MenuElement("assualt", "Assault Rifles"),
                new MenuElement("shot", "Shotguns"),
                new MenuElement("tools", "Tools"),
                new MenuElement("special", "Special Weapons")});

            specialWeapons = new Menu(OptionChose, Back, new MenuElement[]
            {
                new MenuElement("back", "Back"),
                new GamerLockedMenuElement("minigun", "Minigun", IsA),
                new GamerLockedMenuElement("sword", "Sword", IsA2),
            });

            slotChangeToolPistolMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("pistol", "Pistols"),
                new MenuElement("tools", "Tools")});

            smgMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new UnlockableMenuElement("MP5K", "MP5K", PlayerProfile.MP5K),
                new UnlockableMenuElement("UMP45", "UMP45", PlayerProfile.UMP45),
                new UnlockableMenuElement("vector", "VECTOR", PlayerProfile.VECTOR)});

            assualtMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new UnlockableMenuElement("FAL", "FAL", PlayerProfile.FAL),
                new UnlockableMenuElement("M16", "M16", PlayerProfile.M16),
                new UnlockableMenuElement("AK47", "AK-47", PlayerProfile.AK47)});

            shotgunMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new UnlockableMenuElement("12guage", "12 Gauge", PlayerProfile._12GUAGE),
                new UnlockableMenuElement("doublebarrel", "Double Barrel", PlayerProfile.DOUBLEBARREL),
                new UnlockableMenuElement("aa12", "aa-12", PlayerProfile.AA12)});

            pistolMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new UnlockableMenuElement("colt45", "Colt .45", PlayerProfile.COLT45),
                new UnlockableMenuElement("magnum", ".357 Magnum", PlayerProfile.MAGNUM)});

            toolMenu = new Menu(OptionChose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new UnlockableMenuElement("bShovel", "Rock Shovel", PlayerProfile.SHOVELBRONZE),
                new UnlockableMenuElement("sShovel", "Steel Shovel", PlayerProfile.SHOVELSTEEL),
                new UnlockableMenuElement("dShovel", "Diamond Shovel", PlayerProfile.SHOVELDIAMOND),
                new UnlockableMenuElement("bPick", "Rock Pick", PlayerProfile.PICKBRONZE),
                new UnlockableMenuElement("sPick", "Steel Pick", PlayerProfile.PICKSTEEL),
                new UnlockableMenuElement("dPick", "Diamond Pick", PlayerProfile.PICKDIAMOND)});

            m16Menu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.M16BALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.M16MOREAMMO),
				new UnlockableMenuElement("extendedmags", "Extended Mags", PlayerProfile.M16EXTENDEDMAGS)});

            ak47Menu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.AK47BALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.AK47MOREAMMO),
				new UnlockableMenuElement("extendedmags", "Extended Mags", PlayerProfile.AK47EXTENDEDMAGS)});

            falMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.FALBALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.FALMOREAMMO),
				new UnlockableMenuElement("extendedmags", "Extended Mags", PlayerProfile.FALEXTENDEDMAGS)});

            aa12Menu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.AA12BALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.AA12MOREAMMO)});

            _12guageMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile._12GUAGEBALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile._12GUAGEMOREAMMO)});

            doubleBarrelMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.DOUBLEBARRELBALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.DOUBLEBARRELMOREAMMO)});


            ump45Menu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.UMP45BALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.UMP45MOREAMMO),
				new UnlockableMenuElement("extendedmags", "Extended Mags", PlayerProfile.UMP45EXTENDEDMAGS)});

            vectorMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.VECTORBALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.VECTORMOREAMMO),
				new UnlockableMenuElement("extendedmags", "Extended Mags", PlayerProfile.VECTOREXTENDEDMAGS)});

            mp5kMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.MP5KBALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.MP5KMOREAMMO),
				new UnlockableMenuElement("extendedmags", "Extended Mags", PlayerProfile.MP5KEXTENDEDMAGS)});

            colt45Menu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.COLT45BALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.COLT45MOREAMMO)});

            magnumMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "Back"),
				new UnlockableMenuElement("ballistictip", "Ballisitic Tip", PlayerProfile.MAGNUMBALLISTICTIP),
				new UnlockableMenuElement("moreammo", "More Ammo", PlayerProfile.MAGNUMMOREAMMO)});

            bronzeShovelMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "bacK"),
				new UnlockableMenuElement("sharperEdges", "Sharper Edges", PlayerProfile.SHARPEREDGES),
				new UnlockableMenuElement("durableConstruction", "Hardy Materials", PlayerProfile.DURABLECONSTRUCTION)});

            steelShovelMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "bacK"),
				new UnlockableMenuElement("sharperEdges", "Sharper Edges", PlayerProfile.SHARPEREDGES),
				new UnlockableMenuElement("durableConstruction", "Hardy Materials", PlayerProfile.DURABLECONSTRUCTION)});

            diamondShovelMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "bacK"),
				new UnlockableMenuElement("sharperEdges", "Sharper Edges", PlayerProfile.SHARPEREDGES),
				new UnlockableMenuElement("durableConstruction", "Hardy Materials", PlayerProfile.DURABLECONSTRUCTION)});

            bronzePickMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "bacK"),
				new UnlockableMenuElement("sharperEdges", "Sharper Edges", PlayerProfile.SHARPEREDGES),
				new UnlockableMenuElement("durableConstruction", "Hardy Materials", PlayerProfile.DURABLECONSTRUCTION)});

            steelPickMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "bacK"),
				new UnlockableMenuElement("sharperEdges", "Sharper Edges", PlayerProfile.SHARPEREDGES),
				new UnlockableMenuElement("durableConstruction", "Hardy Materials", PlayerProfile.DURABLECONSTRUCTION)});

            diamondPickMenu = new Menu(OptionChose, Back, new MenuElement[]{
				new MenuElement("back", "bacK"),
				new UnlockableMenuElement("sharperEdges", "Sharper Edges", PlayerProfile.SHARPEREDGES),
				new UnlockableMenuElement("durableConstruction", "Hardy Materials", PlayerProfile.DURABLECONSTRUCTION)});

            updating = classMenu;
            viewer = new ClassViewer(PlayerProfile.SMG);
        }

        private bool IsA()
        {
            return SpecialGamer.IsDev(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer]);
        }

        private bool IsA2()
        {
            return SpecialGamer.IsSwordOwner(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer]);
        }

        private bool dontDrawViewer = true;
        void classMenu_SelectedIndexChangedEvent(Menu sender)
        {
            dontDrawViewer = false;
            switch (sender.GetSelectedItemID())
            {
                case "smg":
                    viewer.SetCharacterClass(PlayerProfile.SMG);
                    break;
                case "assualtrifle":
                    viewer.SetCharacterClass(PlayerProfile.AssaultRifle);
                    break;
                case "shot":
                    viewer.SetCharacterClass(PlayerProfile.Shotgun);
                    break;
                case "builder":
                    viewer.SetCharacterClass(PlayerProfile.Builder);
                    break;
                case "custom1":
                    try
                    {
                        viewer.SetCharacterClass(MinerOfDuty.CurrentPlayerProfile.Slot1);
                    }
                    catch (Exception)
                    {
                        MinerOfDuty.CurrentPlayerProfile.Slot1 = new CharacterClass(
                            PlayerProfile.SMG.Slot1,
                            PlayerProfile.SMG.Slot2,
                            PlayerProfile.SMG.Slot3 as ItemSlot,
                            PlayerProfile.SMG.Slot4 as ItemSlot);
                        classMenu_SelectedIndexChangedEvent(sender);
                    }
                    break;
                case "custom2":
                    try
                    {
                        viewer.SetCharacterClass(MinerOfDuty.CurrentPlayerProfile.Slot2);
                    }
                    catch (Exception)
                    {
                        MinerOfDuty.CurrentPlayerProfile.Slot2 = new CharacterClass(
                            PlayerProfile.SMG.Slot1,
                            PlayerProfile.SMG.Slot2,
                            PlayerProfile.SMG.Slot3 as ItemSlot,
                            PlayerProfile.SMG.Slot4 as ItemSlot);
                        classMenu_SelectedIndexChangedEvent(sender);
                    }
                    break;
                case "custom3":
                    try
                    {
                        viewer.SetCharacterClass(MinerOfDuty.CurrentPlayerProfile.Slot3);
                    }
                    catch (Exception)
                    {
                        MinerOfDuty.CurrentPlayerProfile.Slot3 = new CharacterClass(
                            PlayerProfile.SMG.Slot1,
                            PlayerProfile.SMG.Slot2,
                            PlayerProfile.SMG.Slot3 as ItemSlot,
                            PlayerProfile.SMG.Slot4 as ItemSlot);
                        classMenu_SelectedIndexChangedEvent(sender);
                    }
                    break;
                case "custom4":
                    try
                    {
                        viewer.SetCharacterClass(MinerOfDuty.CurrentPlayerProfile.Slot4);
                    }
                    catch (Exception)
                    {
                        MinerOfDuty.CurrentPlayerProfile.Slot4 = new CharacterClass(
                            PlayerProfile.SMG.Slot1,
                            PlayerProfile.SMG.Slot2,
                            PlayerProfile.SMG.Slot3 as ItemSlot,
                            PlayerProfile.SMG.Slot4 as ItemSlot);
                        classMenu_SelectedIndexChangedEvent(sender);
                    }
                    break;
                case "custom5":
                    try
                    {
                        viewer.SetCharacterClass(MinerOfDuty.CurrentPlayerProfile.Slot5);
                    }
                    catch (Exception)
                    {
                        MinerOfDuty.CurrentPlayerProfile.Slot5 = new CharacterClass(
                            PlayerProfile.SMG.Slot1,
                            PlayerProfile.SMG.Slot2,
                            PlayerProfile.SMG.Slot3 as ItemSlot,
                            PlayerProfile.SMG.Slot4 as ItemSlot);
                        classMenu_SelectedIndexChangedEvent(sender);
                    }
                    break;
                case "back":
                    dontDrawViewer = true;
                    break;
            }

        }

        private void CharacterClass_CharacterClassRenamedEvent()
        {
            classMenu["custom1"].Text = MinerOfDuty.CurrentPlayerProfile.Slot1.Name;
            classMenu["custom2"].Text = MinerOfDuty.CurrentPlayerProfile.Slot2.Name;
            classMenu["custom3"].Text = MinerOfDuty.CurrentPlayerProfile.Slot3.Name;
            classMenu["custom4"].Text = MinerOfDuty.CurrentPlayerProfile.Slot4.Name;
            classMenu["custom5"].Text = MinerOfDuty.CurrentPlayerProfile.Slot5.Name;
        }

        private void KeyboardOver(IAsyncResult result)
        {
            string newName = Guide.EndShowKeyboardInput(result);
            if (newName == null)
                return;
            if (newName.Length > 14)
                newName = newName.Substring(0, 14);

            for (int i = 0; i < newName.Length; i++)
            {
                if (Resources.Font.Characters.Contains(newName[i]) == false)
                    newName = newName.Replace(newName[i], ' ');
            }

            workingClass.Name = newName;
            MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
        }

        private void OptionChose(IMenuOwner sender, string id)
        {
            if (sender == classMenu)
            {
                if (id == "back")
                    back.Invoke(this);
                //cant edit furst four classes 
                else
                {
                    updating = classEditMenu;
                    if (id == "custom1")
                    {
                        workingClass = MinerOfDuty.CurrentPlayerProfile.Slot1;
                    }
                    else if (id == "custom2")
                    {
                        workingClass = MinerOfDuty.CurrentPlayerProfile.Slot2;
                    }
                    else if (id == "custom3")
                    {
                        workingClass = MinerOfDuty.CurrentPlayerProfile.Slot3;
                    }
                    else if (id == "custom4")
                    {
                        workingClass = MinerOfDuty.CurrentPlayerProfile.Slot4;
                    }
                    else if (id == "custom5")
                    {
                        workingClass = MinerOfDuty.CurrentPlayerProfile.Slot5;
                    }
                    else
                        updating = classMenu;

                }
            }
            else if (sender == classEditMenu)
            {
                if (id == "back")
                {
                    updating = classMenu;
                }
                else
                {
                    if (id == "playerupgrades")
                    {
                        updating = playerUpgradesMenu;
                    }
                    else if (id == "rename")
                    {
                        if (Guide.IsVisible == false)
                        {
                            Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "Rename Class", "Rename the class name. Must not be longer than 14 characters",
                                workingClass.Name, new AsyncCallback(KeyboardOver), null);
                        }
                    }
                    else if (id == "lethal")
                    {
                        updating = lethalGrenadeMenu;
                    }
                    else if (id == "special")
                    {
                        updating = specialGrenadeMenu;
                    }
                    else
                    {
                        if (id == "slot1")
                        {
                            slotNumber = 1;
                            workingSlot = workingClass.Slot1;
                        }
                        else if (id == "slot2")
                        {
                            slotNumber = 2;
                            workingSlot = workingClass.Slot2;
                        }
                        else if (id == "slot3")
                        {
                            slotNumber = 3;
                            workingSlot = workingClass.Slot3;
                        }
                        else if (id == "slot4")
                        {
                            slotNumber = 4;
                            workingSlot = workingClass.Slot4;
                        }

                        if (slotNumber == 1)
                        {
                            updating = slotChangeAnyMenu;
                        }
                        else if (slotNumber == 2)
                        {
                            updating = slotChangeToolPistolMenu;
                        }
                        else if (slotNumber == 3)
                        {
                            updating = slotChangeItemsMenu;
                        }
                        else if (slotNumber == 4)
                        {
                            updating = slotChangeItemsMenu;
                        }
                    }
                }
            }
            else if (sender == playerUpgradesMenu)
            {
                if (id == "back")
                {
                    updating = classEditMenu;
                }
                else if (id == "morestamina")
                {
                    workingClass.MoreStamina = !workingClass.MoreStamina;
                    workingClass.ThickerSkin = false;
                    workingClass.QuickHands = false;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "thickerskin")
                {
                    workingClass.MoreStamina = false;
                    workingClass.ThickerSkin = !workingClass.ThickerSkin;
                    workingClass.QuickHands = false;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "quickhands")
                {
                    workingClass.MoreStamina = false;
                    workingClass.ThickerSkin = false;
                    workingClass.QuickHands = !workingClass.QuickHands;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
            }
            else if (sender == slotChangeAnyMenu || sender == slotChangeToolPistolMenu
                || sender == slotChangeItemsMenu)
            {
                if (id == "back")
                {
                    updating = classEditMenu;
                }
                else
                    whereICameFrom = sender as Menu;
                if (id == "tools")
                {
                    updating = toolMenu;
                }
                else if (id == "pistol")
                {
                    updating = pistolMenu;
                }
                else if (id == "smg")
                {
                    updating = smgMenu;
                }
                else if (id == "assualt")
                {
                    updating = assualtMenu;
                }
                else if (id == "shot")
                {
                    updating = shotgunMenu;
                }
                else if (id == "special")
                {
                    updating = specialWeapons;
                }
                else if (id == "emptyBucket")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.EmptyBucket;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "lavaBucket")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.LavaBucket;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "stoneblock")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.StoneBlock;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "dirtblock")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.DirtBlock;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "grassblock")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.GrassBlock;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "pitfall")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.Pitfall;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "sandblock")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.SandBlock;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "glowblock")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.GlowBlock;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "goggles")
                {
                    (workingSlot as ItemSlot).Item = InventoryItem.Goggles;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
            }
            else if (sender == toolMenu || sender == pistolMenu || sender == smgMenu
                || sender == assualtMenu || sender == shotgunMenu)
            {
                if (id == "back")
                {
                    updating = whereICameFrom;
                }
                else
                {
                    whereICameFrom2 = sender as Menu;
                    if (id == "MP5K")
                    {
                        updating = mp5kMenu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_MP5K)
                                workingSlot = new WeaponSlot(GunType.GUNID_MP5K, false, false, false);
                        }
                        else //it was a tool
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_MP5K, false, false, false);
                        }
                        //we keep the old so the ups dont get messed up
                    }
                    else if (id == "UMP45")
                    {
                        updating = ump45Menu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_UMP45)
                                workingSlot = new WeaponSlot(GunType.GUNID_UMP45, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_UMP45, false, false, false);
                        }
                    }
                    else if (id == "vector")
                    {
                        updating = vectorMenu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_VECTOR)
                                workingSlot = new WeaponSlot(GunType.GUNID_VECTOR, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_VECTOR, false, false, false);
                        }
                    }
                    else if (id == "12guage")
                    {
                        updating = _12guageMenu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_12GAUGE)
                                workingSlot = new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_12GAUGE, false, false, false);
                        }
                    }
                    else if (id == "doublebarrel")
                    {
                        updating = doubleBarrelMenu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_DOUBLEBARREL)
                                workingSlot = new WeaponSlot(GunType.GUNID_DOUBLEBARREL, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_DOUBLEBARREL, false, false, false);
                        }
                    }
                    else if (id == "aa12")
                    {
                        updating = aa12Menu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_AA12)
                                workingSlot = new WeaponSlot(GunType.GUNID_AA12, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_AA12, false, false, false);
                        }
                    }
                    else if (id == "FAL")
                    {
                        updating = falMenu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_FAL)
                                workingSlot = new WeaponSlot(GunType.GUNID_FAL, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_FAL, false, false, false);
                        }
                    }
                    else if (id == "M16")
                    {
                        updating = m16Menu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_M16)
                                workingSlot = new WeaponSlot(GunType.GUNID_M16, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_M16, false, false, false);
                        }
                    }
                    else if (id == "AK47")
                    {
                        updating = ak47Menu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_AK47)
                                workingSlot = new WeaponSlot(GunType.GUNID_AK47, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_AK47, false, false, false);
                        }
                    }
                    else if (id == "colt45")
                    {
                        updating = colt45Menu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_COLT45)
                                workingSlot = new WeaponSlot(GunType.GUNID_COLT45, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_COLT45, false, false, false);
                        }
                    }
                    else if (id == "magnum")
                    {
                        updating = magnumMenu;
                        if ((workingSlot is WeaponSlot))
                        {
                            if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_MAGNUM)
                                workingSlot = new WeaponSlot(GunType.GUNID_MAGNUM, false, false, false);
                        }
                        else
                        {
                            workingSlot = new WeaponSlot(GunType.GUNID_MAGNUM, false, false, false);
                        }
                    }
                    else if (id == "bShovel")
                    {
                        updating = bronzeShovelMenu;
                        if ((workingSlot is ToolSlot))
                        {
                            if ((workingSlot as ToolSlot).ToolTypeID != ToolType.TOOLID_ROCKSHOVEL)
                                workingSlot = new ToolSlot(ToolType.TOOLID_ROCKSHOVEL, false, false);
                        }
                        else
                        {
                            workingSlot = new ToolSlot(ToolType.TOOLID_ROCKSHOVEL, false, false);
                        }
                    }
                    else if (id == "sShovel")
                    {
                        updating = steelShovelMenu;
                        if ((workingSlot is ToolSlot))
                        {
                            if ((workingSlot as ToolSlot).ToolTypeID != ToolType.TOOLID_STEELSHOVEL)
                                workingSlot = new ToolSlot(ToolType.TOOLID_STEELSHOVEL, false, false);
                        }
                        else
                        {
                            workingSlot = new ToolSlot(ToolType.TOOLID_STEELSHOVEL, false, false);
                        }
                    }
                    else if (id == "dShovel")
                    {
                        updating = diamondShovelMenu;
                        if ((workingSlot is ToolSlot))
                        {
                            if ((workingSlot as ToolSlot).ToolTypeID != ToolType.TOOLID_DIAMONDSHOVEL)
                                workingSlot = new ToolSlot(ToolType.TOOLID_DIAMONDSHOVEL, false, false);
                        }
                        else
                        {
                            workingSlot = new ToolSlot(ToolType.TOOLID_DIAMONDSHOVEL, false, false);
                        }
                    }
                    else if (id == "bPick")
                    {
                        updating = bronzePickMenu;
                        if ((workingSlot is ToolSlot))
                        {
                            if ((workingSlot as ToolSlot).ToolTypeID != ToolType.TOOLID_ROCKPICK)
                                workingSlot = new ToolSlot(ToolType.TOOLID_ROCKPICK, false, false);
                        }
                        else
                        {
                            workingSlot = new ToolSlot(ToolType.TOOLID_ROCKPICK, false, false);
                        }
                    }
                    else if (id == "sPick")
                    {
                        updating = steelPickMenu;
                        if ((workingSlot is ToolSlot))
                        {
                            if ((workingSlot as ToolSlot).ToolTypeID != ToolType.TOOLID_STEELPICK)
                                workingSlot = new ToolSlot(ToolType.TOOLID_STEELPICK, false, false);
                        }
                        else
                        {
                            workingSlot = new ToolSlot(ToolType.TOOLID_STEELPICK, false, false);
                        }
                    }
                    else if (id == "dPick")
                    {
                        updating = diamondPickMenu;
                        if ((workingSlot is ToolSlot))
                        {
                            if ((workingSlot as ToolSlot).ToolTypeID != ToolType.TOOLID_DIAMONDPICK)
                                workingSlot = new ToolSlot(ToolType.TOOLID_DIAMONDPICK, false, false);
                        }
                        else
                        {
                            workingSlot = new ToolSlot(ToolType.TOOLID_DIAMONDPICK, false, false);
                        }
                    }

                    if (slotNumber == 1)
                        workingClass.Slot1 = workingSlot;
                    else if (slotNumber == 2)
                        workingClass.Slot2 = workingSlot;
                    else if (slotNumber == 3)
                        workingClass.Slot3 = workingSlot;
                    else if (slotNumber == 4)
                        workingClass.Slot4 = workingSlot;

                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
            }
            else if (sender == falMenu || sender == ak47Menu || sender == m16Menu
                || sender == aa12Menu || sender == _12guageMenu || sender == doubleBarrelMenu
                || sender == ump45Menu || sender == mp5kMenu || sender == vectorMenu
                || sender == colt45Menu || sender == magnumMenu || sender == bronzeShovelMenu
                || sender == steelShovelMenu || sender == diamondShovelMenu || sender == bronzePickMenu
                || sender == steelPickMenu || sender == diamondPickMenu)
            {
                if (id == "back")
                {
                    updating = whereICameFrom2;
                }
                else if (id == "ballistictip")
                {
                    (workingSlot as WeaponSlot).MoreAmmo = false;
                    (workingSlot as WeaponSlot).ExtendedMags = false;
                    (workingSlot as WeaponSlot).BallisticTip = !(workingSlot as WeaponSlot).BallisticTip;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "moreammo")
                {
                    (workingSlot as WeaponSlot).ExtendedMags = false;
                    (workingSlot as WeaponSlot).BallisticTip = false;
                    (workingSlot as WeaponSlot).MoreAmmo = !(workingSlot as WeaponSlot).MoreAmmo;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "extendedmags")
                {
                    (workingSlot as WeaponSlot).MoreAmmo = false;
                    (workingSlot as WeaponSlot).BallisticTip = false;
                    (workingSlot as WeaponSlot).ExtendedMags = !(workingSlot as WeaponSlot).ExtendedMags;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "sharperEdges")
                {
                    (workingSlot as ToolSlot).DurableConstruction = false;
                    (workingSlot as ToolSlot).SharperEdges = !(workingSlot as ToolSlot).SharperEdges;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "durableConstruction")
                {
                    (workingSlot as ToolSlot).SharperEdges = false;
                    (workingSlot as ToolSlot).DurableConstruction = !(workingSlot as ToolSlot).DurableConstruction;
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
            }
            else if (specialWeapons == sender)
            {
                if (id == "back")
                {
                    updating = slotChangeAnyMenu;
                }
                else if (id == "minigun")
                {
                    if ((workingSlot is WeaponSlot))
                    {
                        if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_MINIGUN)
                            workingSlot = new WeaponSlot(GunType.GUNID_MINIGUN, false, false, false);
                    }
                    else
                    {
                        workingSlot = new WeaponSlot(GunType.GUNID_MINIGUN, false, false, false);
                    }

                    if (slotNumber == 1)
                        workingClass.Slot1 = workingSlot;
                    else if (slotNumber == 2)
                        workingClass.Slot2 = workingSlot;
                    else if (slotNumber == 3)
                        workingClass.Slot3 = workingSlot;
                    else if (slotNumber == 4)
                        workingClass.Slot4 = workingSlot;

                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
                else if (id == "sword")
                {
                    if ((workingSlot is WeaponSlot))
                    {
                        if ((workingSlot as WeaponSlot).GunID != GunType.GUNID_SWORD)
                            workingSlot = new WeaponSlot(GunType.GUNID_SWORD, false, false, false);
                    }
                    else
                    {
                        workingSlot = new WeaponSlot(GunType.GUNID_SWORD, false, false, false);
                    }

                    if (slotNumber == 1)
                        workingClass.Slot1 = workingSlot;
                    else if (slotNumber == 2)
                        workingClass.Slot2 = workingSlot;
                    else if (slotNumber == 3)
                        workingClass.Slot3 = workingSlot;
                    else if (slotNumber == 4)
                        workingClass.Slot4 = workingSlot;

                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                }
            }
            if (workingClass != null)
                viewer.SetCharacterClass(workingClass);
        }

        private void Back(object sender)
        {
            if (sender == classMenu)
                back.Invoke(this);
            else
            {
                if (sender == classEditMenu)
                    updating = classMenu;
                else if (sender == slotChangeAnyMenu || sender == slotChangeItemsMenu || sender == slotChangeToolPistolMenu)
                    updating = classEditMenu;
                else if (sender == smgMenu || sender == pistolMenu || sender == toolMenu || assualtMenu == sender || sender == shotgunMenu)
                    updating = whereICameFrom;
                else if (sender == falMenu || sender == ak47Menu || sender == m16Menu)
                    updating = assualtMenu;
                else if (sender == bronzePickMenu || sender == bronzeShovelMenu || sender == steelPickMenu || sender == steelShovelMenu
                    || sender == diamondPickMenu || sender == diamondShovelMenu)
                    updating = toolMenu;
                else if (sender == playerUpgradesMenu)
                    updating = classEditMenu;
                else if (sender == aa12Menu || sender == _12guageMenu || sender == doubleBarrelMenu)
                    updating = shotgunMenu;
                else if (sender == ump45Menu || sender == mp5kMenu || sender == vectorMenu)
                    updating = smgMenu;
                else if (sender == colt45Menu || sender == magnumMenu)
                    updating = pistolMenu;
                else if (sender == specialWeapons)
                    updating = slotChangeAnyMenu;
            }
        }

        public void Update(short timeInMilli)
        {
            updating.Update(timeInMilli);
        }

        public void Draw(SpriteBatch sb)
        {
            updating.Draw(sb);
            if (!dontDrawViewer)
                viewer.Draw(sb);
        }

    }
}
