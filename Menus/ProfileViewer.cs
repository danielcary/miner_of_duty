using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Game;

namespace Miner_Of_Duty.Menus
{
    public class ProfileView : IMenuOwner
    {

        public void SelectFirst()
        {
            menu.SelectFirst();
            stats.SelectFirst();
        }
        private Menu.BackPressed back;

        private Menu menu;
        private Menu stats;

        private Menu mainMenu;
        private Menu mainStats;

        private Menu kNdMenu;
        private Menu kNdStats;

        private Menu winsMenu;
        private Menu winsStats;

        private Menu defeatsMenu;
        private Menu defeatStats;

        private Menu weaponMenu;
        private Menu weaponStats;

        private Menu pistolMenu, pistolStats;
        private Menu smgMenu, smgStats;
        private Menu assualtMenu, assualtStats;
        private Menu shotMenu, shotStats;

        private Menu showweaponMenu, showweaponStats;

        private void ShowWeapon(byte id,  Menu callbackMenu, Menu callbackStats)
        {
            showweaponMenu = null;
            showweaponStats = null;

            string name = Inventory.GetItemAsString(Inventory.GunIDToInventoryItem((byte)id));
            showweaponMenu = new Menu(
                delegate(IMenuOwner sender, string tid)
                {
                    if (tid == "back")
                    {
                        menu = callbackMenu;
                        stats = callbackStats;
                    }
                }, 
                delegate(object sender)
                {
                    menu = callbackMenu;
                    stats = callbackStats;
                }, new MenuElement[]
                    {
                        new MenuElement("back", "back"),
                        new MenuElement(name + "kills", name + " " + "Kills"),
                        new MenuElement(name + "fired", name + " " + "Shots Fired"),
                        new MenuElement(name + "hits", name + " " + "Hits"),
                        new MenuElement(name + "accuracy", name + " " + "Accuracy"),
                    }, 50);

            showweaponMenu["back"].Position.X -= 25;

            showweaponStats = new Menu(delegate(IMenuOwner sender, string tid) { }, DummyBack, new MenuElement[]
            {
                        new MenuElement("back", ""),
                        new MenuElement(name + "kills", pp.gunStats[id].Kills.ToString()),
                        new MenuElement(name + "fired", pp.gunStats[id].Fired.ToString()),
                        new MenuElement(name + "hits", pp.gunStats[id].Hits.ToString()),
                        new MenuElement(name + "accuracy", ((int)(pp.gunStats[id].Accuracy * 100)).ToString() + "%"),
            }, 725);

            menu = showweaponMenu;
            stats = showweaponStats;
        }

        private Vector2 NamePos = new Vector2(-100, -100);
        private string name;
        public ProfileView(Menu.BackPressed back, string name = "")
        {
            this.back = back;

            List<MenuElement> menuElements = new List<MenuElement>();

            /*
             *level 
             * xp
             * Kills & Death
             * Wins
             * Defeats
             * Weapon Stats
             */

            /*
             * Kills
             * 
             */

            menuElements.AddRange(new MenuElement[]
            {
                new MenuElement("back", "Back"),
                new MenuElement("level", "  Level"),
                new MenuElement("xp", "  XP"),
                new MenuElement("kills", "Kills"),
                new MenuElement("headshots", "Headshots"),
                new MenuElement("revengekills", "Revenges"),
                new MenuElement("deaths", "Deaths"),
                new MenuElement("gravity", "Gravity"),
                new MenuElement("lava", "Lava"),
                new MenuElement("wins", "Wins"),
                new MenuElement("tdmwins", "Team Deathmatch"),
                new MenuElement("snmwins", "Search & Mine"),
                new MenuElement("ffawins", "Free For All"),
                new MenuElement("fwwins", "Fort Wars"),
                new MenuElement("defeats", "Defeats"),
                new MenuElement("tdmdefeats", "Team Deathmatch"),
                new MenuElement("snmdefeats", "Search & Mine"),
                new MenuElement("ffadefeats", "Free For All"),
                new MenuElement("fwdefeats", "Fort Wars"),
            });


            #region main
            mainMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "back")
                        back.Invoke(this);
                    else if (id == "knd")
                    {
                        menu = kNdMenu;
                        stats = kNdStats;
                        kNdMenu.SelectFirst();
                        kNdStats.SelectFirst();
                    }
                    else if (id == "wins")
                    {
                        menu = winsMenu;
                        stats = winsStats;
                        winsMenu.SelectFirst();
                        winsStats.SelectFirst();
                    }
                    else if (id == "defeats")
                    {
                        menu = defeatsMenu;
                        stats = defeatStats;
                        defeatsMenu.SelectFirst();
                        defeatStats.SelectFirst();
                    }
                    else if (id == "weapon")
                    {
                        menu = weaponMenu;
                        stats = weaponStats;
                        weaponMenu.SelectFirst();
                        weaponStats.SelectFirst();
                    }
                }, back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("level", "Level"),
                new MenuElement("xp", "XP"),
                new MenuElement("knd", "Kills & Deaths"),
                new MenuElement("wins", "Victories"),
                new MenuElement("defeats", "Defeats"),
                new MenuElement("weapon", "Weapon Stats"),
            }, 50);

            mainMenu["back"].Position.X -= 25;

            mainStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("level", ""),
                new MenuElement("xp", ""),
                new MenuElement("knd", "+"),
                new MenuElement("wins", "+"),
                new MenuElement("defeats", "+"),
                new MenuElement("weapon", "+"),
            }, 725);

            mainStats["back"].Position.X = 640;

            NamePos = mainStats["back"].Position;
            NamePos.X -= (Resources.TitleFont.MeasureString(name).X / 2f);
            this.name = name;
            #endregion

            #region KnD
            kNdMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "back")
                    {
                        menu = mainMenu; stats = mainStats;
                    }
                }, delegate(object sender) { menu = mainMenu; stats = mainStats;}, new MenuElement[]{
                    new MenuElement("back", "back"),
                    new MenuElement("kills", "Kills"),
                    new MenuElement("headshots", "-Headshots"),
                    new MenuElement("revengekills", "-Revenges"),
                    new MenuElement("grenadekills", "-Grenade"),
                    new MenuElement("knifekills", "-Knife"),
                    new MenuElement("deaths", "Deaths"),
                    new MenuElement("gravity", "-Gravity"),
                    new MenuElement("lava", "-Lava"),
                }, 50);

            kNdMenu["back"].Position.X -= 25;
            kNdMenu["headshots"].Position.X += 25;
            kNdMenu["revengekills"].Position.X += 25;
            kNdMenu["grenadekills"].Position.X += 25;
            kNdMenu["knifekills"].Position.X += 25;
            kNdMenu["gravity"].Position.X += 25;
            kNdMenu["lava"].Position.X += 25;

            //kNdMenu["headshots"].NormalTextColor = Color.Gray;
            //kNdMenu["revengekills"].NormalTextColor = Color.Gray;
            //kNdMenu["grenadekills"].NormalTextColor = Color.Gray;
            //kNdMenu["knifekills"].NormalTextColor = Color.Gray;
            //kNdMenu["gravity"].NormalTextColor = Color.Gray;
            //kNdMenu["lava"].NormalTextColor = Color.Gray;

            kNdStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("kills", "0"),
                new MenuElement("headshots", "0"),
                new MenuElement("revengekills", "0"),
                new MenuElement("grenadekills", "0"),
                new MenuElement("knifekills", "0"),
                new MenuElement("deaths", "0"),
                new MenuElement("gravity", "0"),
                new MenuElement("lava", "0"),
            }, 725);
            #endregion

            #region winds
            winsMenu = new Menu(delegate(IMenuOwner sender, string id)
            {
                if (id == "back")
                {
                    menu = mainMenu; stats = mainStats;
                }
            }, delegate(object sender) { menu = mainMenu; stats = mainStats; }, new MenuElement[]{
                    new MenuElement("back", "back"),
                    new MenuElement("wins", "Victories"),
                    new MenuElement("tdmwins", "-Team Deathmatch"),
                    new MenuElement("snmwins", "-Search & Mine"),
                    new MenuElement("ffawins", "-Free For All"),
                    new MenuElement("fwwins", "-Fort Wars"),
                    new MenuElement("kbwins", "-King of the Beach"),
                }, 50);

            winsMenu["back"].Position.X -= 25;
            winsMenu["tdmwins"].Position.X += 25;
            winsMenu["snmwins"].Position.X += 25;
            winsMenu["ffawins"].Position.X += 25;
            winsMenu["fwwins"].Position.X += 25;
            winsMenu["kbwins"].Position.X += 25;

            winsStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("wins", "0"),
                new MenuElement("tdmwins", "0"),
                new MenuElement("snmwins", "0"),
                new MenuElement("ffawins", "0"),
                new MenuElement("fwwins", "0"),
                new MenuElement("kbwins", "0"),
            }, 725);
            #endregion

            #region defeays
            defeatsMenu = new Menu(delegate(IMenuOwner sender, string id)
            {
                if (id == "back")
                {
                    menu = mainMenu; stats = mainStats;
                }
            }, delegate(object sender) { menu = mainMenu; stats = mainStats; }, new MenuElement[]{
                    new MenuElement("back", "back"),
                    new MenuElement("defeats", "Defeats"),
                    new MenuElement("tdmdefeats", "-Team Deathmatch"),
                    new MenuElement("snmdefeats", "-Search & Mine"),
                    new MenuElement("ffadefeats", "-Free For All"),
                    new MenuElement("fwdefeats", "-Fort Wars"),
                    new MenuElement("kbdefeats", "-King of the Beach"),
                }, 50);

            defeatsMenu["back"].Position.X -= 25;
            defeatsMenu["tdmdefeats"].Position.X += 25;
            defeatsMenu["snmdefeats"].Position.X += 25;
            defeatsMenu["ffadefeats"].Position.X += 25;
            defeatsMenu["fwdefeats"].Position.X += 25;
            defeatsMenu["kbdefeats"].Position.X += 25;

            defeatStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("defeats", "0"),
                new MenuElement("tdmdefeats", "0"),
                new MenuElement("snmdefeats", "0"),
                new MenuElement("ffadefeats", "0"),
                new MenuElement("fwdefeats", "0"),
                new MenuElement("kbdefeats", "0"),
            }, 725);
            #endregion

            #region weaponmain
            weaponMenu = new Menu(delegate(IMenuOwner sender, string id)
            {
                if (id == "back")
                {
                    menu = mainMenu; stats = mainStats;
                }
                else if (id == "pistol")
                {
                    menu = pistolMenu;
                    stats = pistolStats;
                    pistolMenu.SelectFirst();
                    pistolStats.SelectFirst();
                }
                else if (id == "smg")
                {
                    menu = smgMenu;
                    stats = smgStats;
                    smgMenu.SelectFirst();
                    smgStats.SelectFirst();
                }
                else if (id == "assualt")
                {
                    menu = assualtMenu;
                    stats = assualtStats;
                    assualtMenu.SelectFirst();
                    assualtStats.SelectFirst();
                }
                else if (id == "shot")
                {
                    menu = shotMenu;
                    stats = shotStats;
                    shotMenu.SelectFirst();
                    shotStats.SelectFirst();
                }
            }, delegate(object sender) { menu = mainMenu; stats = mainStats; }, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("pistol", "Pistols"),
                new MenuElement("smg", "SMGs"),
                new MenuElement("assualt", "Assault Rifles"),
                new MenuElement("shot", "Shotguns"),
            }, 50);

            weaponMenu["back"].Position.X -= 25;

            weaponStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("pistol", "+"),
                new MenuElement("smg", "+"),
                new MenuElement("assualt", "+"),
                new MenuElement("shot", "+"),
            }, 725);
            #endregion

            #region pistol
            pistolMenu = new Menu(delegate(IMenuOwner sender, string id)
            {
                if (id == "back")
                {
                    menu = weaponMenu; stats = weaponStats;
                }
                else if (id == "colt45")
                {
                    ShowWeapon(GunType.GUNID_COLT45, pistolMenu, pistolStats);
                }
                else if (id == "magnum")
                {
                    ShowWeapon(GunType.GUNID_MAGNUM, pistolMenu, pistolStats);
                }
            }, delegate(object sender) { menu = weaponMenu; stats = weaponStats; }, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("colt45", "-Colt .45"),
                new MenuElement("magnum", "-.357 Magnum")
            }, 50);

            pistolMenu["back"].Position.X -= 25;

            pistolStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("colt45", "+"),
                new MenuElement("magnum", "+")
            }, 725);
            #endregion

            #region smg
            smgMenu = new Menu(delegate(IMenuOwner sender, string id)
            {
                if (id == "back")
                {
                    menu = weaponMenu; stats = weaponStats;
                }
                else if (id == "MP5K")
                {
                    ShowWeapon(GunType.GUNID_MP5K, smgMenu, smgStats);
                }
                else if (id == "UMP45")
                {
                    ShowWeapon(GunType.GUNID_UMP45, smgMenu, smgStats);
                }
                else if (id == "vector")
                {
                    ShowWeapon(GunType.GUNID_VECTOR, smgMenu, smgStats);
                }
            }, delegate(object sender) { menu = weaponMenu; stats = weaponStats; }, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("MP5K", "-MP5K"),
                new MenuElement("UMP45", "-UMP45"),
                new MenuElement("vector", "-VECTOR")
            }, 50);

            smgMenu["back"].Position.X -= 25;

            smgStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("MP5K", "+"),
                new MenuElement("UMP45", "+"),
                new MenuElement("vector", "+")
            }, 725);
            #endregion

            #region shot
            shotMenu = new Menu(delegate(IMenuOwner sender, string id)
            {
                if (id == "back")
                {
                    menu = weaponMenu; stats = weaponStats;
                }
                else if (id == "12guage")
                {
                    ShowWeapon(GunType.GUNID_12GAUGE, shotMenu, shotStats);
                }
                else if (id == "doublebarrel")
                {
                    ShowWeapon(GunType.GUNID_DOUBLEBARREL, shotMenu, shotStats);
                }
                else if (id == "aa12")
                {
                    ShowWeapon(GunType.GUNID_AA12, shotMenu, shotStats);
                }
            }, delegate(object sender) { menu = weaponMenu; stats = weaponStats; }, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("12guage", "-12 Gauge"),
                new MenuElement("doublebarrel", "-Double Barrel"),
                new MenuElement("aa12", "-aa-12")
            }, 50);

            shotMenu["back"].Position.X -= 25;

            shotStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("12guage", "+"),
                new MenuElement("doublebarrel", "+"),
                new MenuElement("aa12", "+")
            }, 725);
            #endregion

            #region assault
            assualtMenu = new Menu(delegate(IMenuOwner sender, string id)
            {
                if (id == "back")
                {
                    menu = weaponMenu; stats = weaponStats;
                }
                else if (id == "FAL")
                {
                    ShowWeapon(GunType.GUNID_FAL, assualtMenu, assualtStats);
                }
                else if (id == "M16")
                {
                    ShowWeapon(GunType.GUNID_M16, assualtMenu, assualtStats);
                }
                else if (id == "AK47")
                {
                    ShowWeapon(GunType.GUNID_AK47, assualtMenu, assualtStats);
                }
            }, delegate(object sender) { menu = weaponMenu; stats = weaponStats; }, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("FAL", "-FAL"),
                new MenuElement("M16", "-M16"),
                new MenuElement("AK47", "-AK-47")
            }, 50);

            assualtMenu["back"].Position.X -= 25;

            assualtStats = new Menu(delegate(IMenuOwner sender, string id) { }, DummyBack, new MenuElement[]{
                new MenuElement("back", ""),
                new MenuElement("FAL", "+"),
                new MenuElement("M16", "+"),
                new MenuElement("AK47", "+")
            }, 725);
            #endregion

            menu = mainMenu;
            stats = mainStats;
        }

        private void Choose(object sender, string id)
        {
            if (id == "back")
                back.Invoke(this);
        }

        private void DummyBack(object sender) { }

        public void Show()
        {
            Show(MinerOfDuty.CurrentPlayerProfile);
        }

        private PlayerProfile pp;
        public void Show(PlayerProfile pp)
        {
            this.pp = pp;
            mainStats["level"].Text = pp.Level.ToString();
            if (pp.Level != 50)
                mainStats["xp"].Text = pp.XP + " / " + pp.XPTillLevel;
            else
                mainStats["xp"].Text = pp.XP.ToString();

            kNdStats["kills"].Text = pp.Kills.ToString();
            kNdStats["headshots"].Text = pp.Headshots.ToString();
            kNdStats["revengekills"].Text = pp.RevengeKills.ToString();

            kNdStats["grenadekills"].Text = pp.GrenadeKills.ToString();
            kNdStats["knifekills"].Text = pp.KnifeKills.ToString();

            kNdStats["deaths"].Text = pp.Deaths.ToString();
            kNdStats["gravity"].Text = pp.GravityDeaths.ToString();
            kNdStats["lava"].Text = pp.LavaDeaths.ToString();

            winsStats["wins"].Text = pp.Wins.ToString();
            winsStats["tdmwins"].Text = pp.TDMWins.ToString();
            winsStats["snmwins"].Text = pp.SNMWins.ToString();
            winsStats["ffawins"].Text = pp.FFAWins.ToString();
            winsStats["fwwins"].Text = pp.FWWins.ToString();
            winsStats["kbwins"].Text = pp.KBWins.ToString();

            defeatStats["defeats"].Text = pp.Defeats.ToString();
            defeatStats["tdmdefeats"].Text = pp.TDMDefeats.ToString();
            defeatStats["snmdefeats"].Text = pp.SNMDefeats.ToString();
            defeatStats["ffadefeats"].Text = pp.FFADefeats.ToString();
            defeatStats["kbdefeats"].Text = pp.KBDefeats.ToString();

        }

        public void Update(short timePassedInMilliseconds)
        {
            menu.Update(timePassedInMilliseconds);
            stats.Update(timePassedInMilliseconds);
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);
            menu.Draw(sb);
            stats.Draw(sb);
            sb.DrawString(Resources.TitleFont, name, NamePos, Color.Yellow);
        }
    }
}
