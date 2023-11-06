using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Game;
using Microsoft.Xna.Framework.Net;

namespace Miner_Of_Duty.Menus
{
    public class InGameMenu : IMenuOwner
    {
        public static Texture2D SideBack;

        public delegate void SetClass(CharacterClass classToSet);
        public event SetClass SetClassEvent;

        private Menu.BackPressed back;

        private Menu mainMenu;
        private Menu classMenu, optionsMenu;

        private Menu workingMenu;

        private ClassViewer viewer;
        private NetworkSession session;
        public InGameMenu(Menu.BackPressed back, NetworkSession sess, bool isSwarm)
        {
            this.back = back;
            session = sess;

            if (isSwarm)
            {
                mainMenu = new Menu(MenuChoose, Back, new MenuElement[]
                {
                    new MenuElement("back", "Back"),
                    new MenuElement("options", "Options"),
                    new MenuElement("leave", "Leave Match"),
                });

            }
            else
            {
                mainMenu = new Menu(MenuChoose, Back, new MenuElement[]
                {
                    new MenuElement("back", "Back"),
                    new MenuElement("cc", "Choose Class"),
                    new MenuElement("options", "Options"),
                    new MenuElement("respawn", "respawn"),
                    new MenuElement("leave", "Leave Match"),
                });
            }

            optionsMenu = new Menu(MenuChoose, Back, new MenuElement[]{
                    new MenuElement("back", "back"),
                    new ValueMenuElement("music", "Music:", 100, "%", 0, 100, 0),
                    new ValueMenuElement("sound", "Sound:", 100, "%", 0, 100, 0),
                    new ValueMenuElement("sens", "sensitivity:", 5, "", 1, 10, 69),
                    new BooleanValueMenuElement("invert", "Invert Y: ", false, 150),
                    new BooleanValueMenuElement("hud", "Draw HUD: ", true, 150),
            });

            (optionsMenu["music"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["sound"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["sens"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["invert"] as BooleanValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["hud"] as BooleanValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;

            classMenu = new Menu(MenuChoose, Back, new MenuElement[]{
                new MenuElement("back", "Back"),
                new MenuElement("smg", "SMG"),
                new MenuElement("assualtrifle", "Assault Rifle"),
                new MenuElement("shot", "Shotgun"),
                new MenuElement("builder", "Builder"),
                new MenuElement("custom1", "Custom Class 1"),
                new MenuElement("custom2", "Custom Class 2"),
                new MenuElement("custom3", "Custom Class 3"),
                new MenuElement("custom4", "Custom Class 4"),
                new MenuElement("custom5", "Custom Class 5")});

            classMenu.SelectedIndexChangedEvent += new Menu.SelectedIndexChanged(classMenu_SelectedIndexChangedEvent);

            CharacterClass_CharacterClassRenamedEvent();
            CharacterClass.CharacterClassRenamedEvent += new CharacterClass.CharacterClassRenamed(CharacterClass_CharacterClassRenamedEvent);
            PlayerProfile.PlayerProfileReloadedEvent += CharacterClass_CharacterClassRenamedEvent;



            workingMenu = mainMenu;

            viewer = new ClassViewer(PlayerProfile.SMG);
        }

        private void MainMenu_ValueChangedEvent(string id)
        {
            if (id == "music")
            {
                Audio.SetMusicVolume((optionsMenu["music"] as ValueMenuElement).Value / 100f);
               // MinerOfDuty.SaveSettings();
            }
            else if (id == "sound")
            {
                Audio.SetSoundVolume((optionsMenu["sound"] as ValueMenuElement).Value / 100f);
             //   MinerOfDuty.SaveSettings();
            }
            else if (id == "sens")
            {
                MinerOfDuty.PlayerSensitivity = (int)(optionsMenu["sens"] as ValueMenuElement).Value;
               // MinerOfDuty.SaveSettings();
            }
            else if (id == "invert")
            {
                MinerOfDuty.InvertYAxis = (optionsMenu["invert"] as BooleanValueMenuElement).Value;
              //  MinerOfDuty.SaveSettings();
            }
            else if (id == "hud")
            {
                MinerOfDuty.game.DrawHud = (optionsMenu["hud"] as BooleanValueMenuElement).Value;
            }
        }

        private bool dontDrawViewer = true;
        void classMenu_SelectedIndexChangedEvent(Menu sender)
        {
            dontDrawViewer = false;
            try
            {
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
            catch (KeyNotFoundException)
            {
                dontDrawViewer = true;
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

        private void MsgBox(int id)
        {
            if (id == 0)
            {

                MinerOfDuty.CurrentPlayerProfile.AddDefeat(MinerOfDuty.game.type, 0, LobbyCode.Lobby.IsPrivateLobby());


                MinerOfDuty.game.UnSub();
                MinerOfDuty.game.Dispose();
                if (MinerOfDuty.game is SwarmGame)
                {
                    MinerOfDuty.DrawMenu();
                }
                else
                {
                    MinerOfDuty.lobby.LeaveLobby(0);
                    
                }

                if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                    MinerOfDuty.Session.Dispose();

            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private void MenuChoose(object sender, string id)
        {
            if (sender == mainMenu)
            {
                if (id == "back")
                {
                    back.Invoke(this);
                    MinerOfDuty.SaveSettings();
                }
                else if (id == "cc")
                    workingMenu = classMenu;
                else if (id == "leave")
                {
                    MessageBox.ShowMessageBox(MsgBox, new string[] { "YES, LEAVE", "NO, STAY" }, 1, new string[] { "ARE YOU SURE YOU WANT TO", "LEAVE THE MATCH?" });
                }
                else if (id == "respawn")
                {
                    MinerOfDuty.game.player.GrenadeHurt(125, MinerOfDuty.game.Me);
                    back.Invoke(this);
                }
                else if (id == "options")
                {
                    (optionsMenu["music"] as ValueMenuElement).Value = Audio.MusicVolume * 100;
                    (optionsMenu["sound"] as ValueMenuElement).Value = Audio.SoundVolume * 100;
                    (optionsMenu["sens"] as ValueMenuElement).Value = MinerOfDuty.PlayerSensitivity;
                    (optionsMenu["invert"] as BooleanValueMenuElement).Value = MinerOfDuty.InvertYAxis;
                    workingMenu = optionsMenu;
                }
            }
            else if (optionsMenu == sender)
            {
                if (id == "back")
                {
                    workingMenu = mainMenu;
                }
            }
            else if (sender == classMenu)
            {
                if (id == "smg")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(PlayerProfile.SMG);
                }
                else if (id == "assualtrifle")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(PlayerProfile.AssaultRifle);
                }
                else if (id == "shot")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(PlayerProfile.Shotgun);
                }
                else if (id == "builder")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(PlayerProfile.Builder);
                }
                else if (id == "custom1")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot1);
                }
                else if (id == "custom2")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot2);
                }
                else if (id == "custom3")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot3);
                }
                else if (id == "custom4")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot4);
                }
                else if (id == "custom5")
                {
                    if (SetClassEvent != null)
                        SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot5);
                }
                workingMenu = mainMenu;
                classMenu.SelectFirst();
            }
        }

        private void Back(object sender)
        {
            if (sender == mainMenu)
            {
                back.Invoke(this);
                MinerOfDuty.SaveSettings();
            }
            else if (sender == classMenu)
                workingMenu = mainMenu;

            else if (sender == optionsMenu)
            {
                workingMenu = mainMenu;
            }
        }

        public void SelectFirst()
        {
            mainMenu.SelectFirst();
        }

        public void Update(short timePassedInMilliseconds)
        {
            if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Start))
                back.Invoke(this);

            workingMenu.Update(timePassedInMilliseconds);

        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(SideBack, Vector2.Zero, Color.White);
            workingMenu.Draw(sb);
            if (!dontDrawViewer)
                viewer.Draw(sb);
        }
    }

    public class ChooseClass : IMenuOwner
    {

        public void SelectFirst()
        {
            classMenu.SelectFirst();
        }

        public delegate void SetClass(CharacterClass classToSet);
        public event SetClass SetClassEvent;

        private Menu classMenu;
        private ClassViewer viewer;

        public ChooseClass()
        {


            classMenu = new Menu(MenuChoose, Back, new MenuElement[]{
                new MenuElement("smg", "SMG"),
                new MenuElement("assualtrifle", "Assault Rifle"),
                new MenuElement("shot", "Shotgun"),
                new MenuElement("builder", "Builder"),
                new MenuElement("custom1", "Custom Class 1"),
                new MenuElement("custom2", "Custom Class 2"),
                new MenuElement("custom3", "Custom Class 3"),
                new MenuElement("custom4", "Custom Class 4"),
                new MenuElement("custom5", "Custom Class 5")});

            classMenu.SelectedIndexChangedEvent += new Menu.SelectedIndexChanged(classMenu_SelectedIndexChangedEvent);

            CharacterClass_CharacterClassRenamedEvent();
            CharacterClass.CharacterClassRenamedEvent += new CharacterClass.CharacterClassRenamed(CharacterClass_CharacterClassRenamedEvent);
            PlayerProfile.PlayerProfileReloadedEvent += CharacterClass_CharacterClassRenamedEvent;

            viewer = new ClassViewer(PlayerProfile.SMG);
        }

        private void classMenu_SelectedIndexChangedEvent(Menu sender)
        {
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

        private void MenuChoose(object sender, string id)
        {
            if (id == "smg")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(PlayerProfile.SMG);
            }
            else if (id == "assualtrifle")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(PlayerProfile.AssaultRifle);
            }
            else if (id == "shot")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(PlayerProfile.Shotgun);
            }
            else if (id == "builder")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(PlayerProfile.Builder);
            }
            else if (id == "custom1")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot1);
            }
            else if (id == "custom2")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot2);
            }
            else if (id == "custom3")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot3);
            }
            else if (id == "custom4")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot4);
            }
            else if (id == "custom5")
            {
                if (SetClassEvent != null)
                    SetClassEvent.Invoke(MinerOfDuty.CurrentPlayerProfile.Slot5);
            }
        }

        private void Back(object sender)
        {

        }

        public void Update(short timePassedInMilliseconds)
        {
            classMenu.Update(timePassedInMilliseconds);
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(InGameMenu.SideBack, Vector2.Zero, Color.White);
            classMenu.Draw(sb);
            viewer.Draw(sb);
        }
    }
}
