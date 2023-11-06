using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Game.Editor;
using Microsoft.Xna.Framework.GamerServices;
using System.Threading;
using Miner_Of_Duty.LobbyCode;

namespace Miner_Of_Duty.Menus
{
    public class InEditorMenu : IMenuOwner
    {
         private Menu.BackPressed back;

         private IMenuOwner workin;
        private Menu mainMenu, optionsMenu;
        private WorldEditor we;
        private bool saving = false;
        private int dot, dotdelay;
        private PlayerPermissions ps;

        public void SetPerm(byte id, bool view)
        {
            ps.SetPerm(id, view);
        }

        public InEditorMenu(Menu.BackPressed back, WorldEditor we)
        {
            this.back = back;
            this.we = we;
            ps = new PlayerPermissions();

            if (MinerOfDuty.Session.SessionType == Microsoft.Xna.Framework.Net.NetworkSessionType.Local)
            {
                mainMenu = new Menu(MenuChoose, Back, new MenuElement[]
                {
                    new MenuElement("back", "Back"),
                    new MenuElement("options", "Options"),
                    new MenuElement("edit", "Edit Map Info"),
                    new MenuElement("save", "Save"),
                    new MenuElement("exit", "Exit")
                });
            }
            else
            {
                mainMenu = new Menu(MenuChoose, Back, new MenuElement[]
                {
                    new MenuElement("back", "Back"),
                    new GamerLockedMenuElement("perm", "EDIT PERMiSSIONS", delegate()
                        {
                            if (MinerOfDuty.Session.Host.IsLocal)
                                return true;
                            return false;   
                        }),
                    new MenuElement("options", "Options"),
                    new MenuElement("edit", "Edit Map Info"),
                    new MenuElement("save", "Save"),
                    new MenuElement("exit", "Exit")
                });
            }

            optionsMenu = new Menu(MenuChoose, Back, new MenuElement[]{
                new MenuElement("back", "back"),
                new ValueMenuElement("music", "Music:", 100, "%", 0, 100, 0),
                new ValueMenuElement("sound", "Sound:", 100, "%", 0, 100, 0),
                new ValueMenuElement("sens", "sensitivity:", 5, "", 1, 10, 69),
                new BooleanValueMenuElement("invert", "Invert Y: ", false, 150),
            });

            (optionsMenu["music"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["sound"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["sens"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["invert"] as BooleanValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;

            workin = mainMenu;
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
              //  MinerOfDuty.SaveSettings();
            }
            else if (id == "sens")
            {
                MinerOfDuty.PlayerSensitivity = (int)(optionsMenu["sens"] as ValueMenuElement).Value;
              //  MinerOfDuty.SaveSettings();
            }
            else if (id == "invert")
            {
                MinerOfDuty.InvertYAxis = (optionsMenu["invert"] as BooleanValueMenuElement).Value;
               // MinerOfDuty.SaveSettings();
            }
        }

        public void SelectFirst()
        {
            workin = mainMenu;
            mainMenu.SelectFirst();
        }

        private void Exit(int selected)
        {
            if (selected == 0)
            {
                we.Dispose();
                we.GetLiquidManager.KillWaterManagerThread();
                MinerOfDuty.mainMenu.ShowCustomMenu();
                MinerOfDuty.DrawMenu(true);
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private void NameChanged(IAsyncResult result)
        {
            string newname = Guide.EndShowKeyboardInput(result);

            if (newname == null)
                return;

            if (newname.Length > 15)
                newname = newname.Substring(0, 15);

            for (int i = 0; i < newname.Length; i++)
            {
                if (char.IsLetterOrDigit(newname[i]) == false || Resources.Font.Characters.Contains(newname[i]) == false)
                    newname = newname.Replace(newname[i], ' ');
            }

            we.mapName = NameFilter.FilterName(newname);
            we.filename = ":(";
        }

        private void DumbUCantNotSaveAtThisTimeUntilYouPlaceBothTeamSpawnsMessageBoxCallBack(int selected)
        {
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private void Save(int selected)
        {
            if (selected == 0)
            {
                if (we.password == null)
                {
                    if (we.filename == ":(")
                    {
                        toSave =
                            SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag
                            + we.mapName;

                        addon = 0;

                        MinerOfDuty.SaveDevice.FileExistsCompleted += SaveDevice_FileExistsCompleted;
                        MinerOfDuty.SaveDevice.DeviceDisconnected += SaveDevice_DeviceDisconnected;
                        MinerOfDuty.SaveDevice.FileExistsAsync("Miner Of Duty Custom Maps", toSave);
                    }
                    else
                    {
                        MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Custom Maps",
                             we.filename, we.Save);
                        MinerOfDuty.SaveDevice.SaveCompleted += SaveDevice_SaveCompleted;
                        MinerOfDuty.SaveDevice.DeviceDisconnected += SaveDevice_DeviceDisconnected;
                    }
                    saving = true;
                    dot = 0;
                }
                else
                {

                    if (Guide.IsVisible)
                        return;

                    try
                    {
                        Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "Enter Password",
                            "This map is password locked, and to save you must enter the password.", "", new AsyncCallback(
                                delegate(IAsyncResult result)
                                {
                                    string password = Guide.EndShowKeyboardInput(result);

                                    if (password == null || password == "")
                                        return;

                                    if (password == we.password)
                                    {
                                        if (we.filename == ":(")
                                        {
                                            toSave =
                                                SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag
                                                + we.mapName;

                                            addon = 0;

                                            MinerOfDuty.SaveDevice.FileExistsCompleted += SaveDevice_FileExistsCompleted;
                                            MinerOfDuty.SaveDevice.DeviceDisconnected += SaveDevice_DeviceDisconnected;
                                            MinerOfDuty.SaveDevice.FileExistsAsync("Miner Of Duty Custom Maps", toSave);
                                        }
                                        else
                                        {
                                            MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Custom Maps",
                                                 we.filename, we.Save);
                                            MinerOfDuty.SaveDevice.SaveCompleted += SaveDevice_SaveCompleted;
                                            MinerOfDuty.SaveDevice.DeviceDisconnected += SaveDevice_DeviceDisconnected;
                                        }
                                        saving = true;
                                        dot = 0;
                                    }
                                    else
                                    {
                                        //wrong password
                                        while (Guide.IsVisible)
                                            Thread.Sleep(1);

                                        try
                                        {
                                            Guide.BeginShowMessageBox("Wrong Password", "Wrong password: passwords do not match.", new string[] { "Okay" }, 0, MessageBoxIcon.Alert,
                                                new AsyncCallback(delegate(IAsyncResult result2)
                                                    {
                                                        Guide.EndShowMessageBox(result2);

                                                    }), null);
                                        }
                                        catch { }
                                    }

                                }), null);
                    }
                    catch { }
                }
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        void SaveDevice_DeviceDisconnected(object sender, EasyStorage.SaveDeviceEventArgs e)
        {
            MinerOfDuty.SaveDevice.DeviceDisconnected -= SaveDevice_DeviceDisconnected;
            MinerOfDuty.SaveDevice.SaveCompleted -= SaveDevice_SaveCompleted;
            saving = false;
        }

        private string toSave;
        private int addon = 0;
        private void SaveDevice_FileExistsCompleted(object sender, EasyStorage.FileExistsCompletedEventArgs args)
        {
            MinerOfDuty.SaveDevice.FileExistsCompleted -= SaveDevice_FileExistsCompleted;

            if (!args.Result)
            {
                if (addon > 0)
                {
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Custom Maps",
                        toSave + addon, we.Save);
                    we.filename = toSave + addon;
                }
                else
                {
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Custom Maps",
                        toSave, we.Save);
                    we.filename = toSave;
                }
                MinerOfDuty.SaveDevice.SaveCompleted += SaveDevice_SaveCompleted;
                MinerOfDuty.SaveDevice.DeviceDisconnected += SaveDevice_DeviceDisconnected;
            }
            else
            {
                addon++;
                MinerOfDuty.SaveDevice.FileExistsCompleted += SaveDevice_FileExistsCompleted;
                MinerOfDuty.SaveDevice.FileExistsAsync("Miner Of Duty Custom Maps", toSave + addon);
            }
        }

        private void SaveDevice_SaveCompleted(object sender, EasyStorage.FileActionCompletedEventArgs args)
        {
            MinerOfDuty.SaveDevice.DeviceDisconnected -= SaveDevice_DeviceDisconnected;
            MinerOfDuty.SaveDevice.SaveCompleted -= SaveDevice_SaveCompleted;
            saving = false;
        }

        public void MenuChoose(IMenuOwner menu, string id)
        {
            if (id == "back")
            {
                if (menu == mainMenu)
                {
                    back.Invoke(this);
                    MinerOfDuty.SaveSettings();
                }
                else
                    workin = mainMenu; 
            }
            else if(id == "perm")
            {
                ps.Show(Back);
                workin = ps;
            }
            else if (id == "edit")
            {
                if (Guide.IsVisible == false && we.password != null)
                {
                    try
                    {
                        Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "Enter Password",
                                   "This map is password locked, and to edit map info you must enter the password.", "", new AsyncCallback(
                                       delegate(IAsyncResult result)
                                       {
                                           string password = Guide.EndShowKeyboardInput(result);

                                           if (password == null || password == "")
                                               return;

                                           if (password == we.password)
                                           {
                                               workin = new EditInfoMapMenu(delegate(object sender)
                                               {
                                                   workin = mainMenu;
                                               }, delegate(string name, string password2, string teamA, string teamB, bool weapons,bool editing)
                                               {
                                                   we.mapName = name;
                                                   we.password = password2;
                                                   we.teamAName = teamA;
                                                   we.teamBName = teamB;
                                                   we.weapons = weapons;
                                                   we.editing = editing;

                                                   workin = null;
                                                   workin = mainMenu;
                                               }, we.mapName, we.password, we.teamAName, we.teamBName, we.gameMode, we.size, we.weapons, we.editing, we.GetTerrain.WorldGen, we.trees);
                                           }
                                           else
                                           {
                                               //wrong password
                                               while (Guide.IsVisible)
                                                   Thread.Sleep(1);

                                               try
                                               {
                                                   Guide.BeginShowMessageBox("Wrong Password", "Wrong password: passwords do not match.", new string[] { "Okay" }, 0, MessageBoxIcon.Alert,
                                                       new AsyncCallback(delegate(IAsyncResult result2)
                                                       {
                                                           Guide.EndShowMessageBox(result2);

                                                       }), null);
                                               }
                                               catch { }
                                           }

                                       }), null);
                    }
                    catch { }
                }
                else
                {
                    workin = new EditInfoMapMenu(delegate(object sender)
                    {
                        workin = mainMenu;
                    }, delegate(string name, string password2, string teamA, string teamB, bool weapons, bool editing)
                    {
                        we.mapName = name;
                        we.password = password2;
                        we.teamAName = teamA;
                        we.teamBName = teamB;
                        we.weapons = weapons;
                        we.editing = editing;

                        workin = null;
                        workin = mainMenu;
                    }, we.mapName, we.password, we.teamAName, we.teamBName, we.gameMode, we.size, we.weapons, we.editing, we.GetTerrain.WorldGen, we.trees);
                }
            }
            else if (id == "name")
            {
                Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "Change Custom Map Name",
                    "Change the name of the map. Name can be at most 15 characters", we.mapName, NameChanged, null);
            }
            else if (id == "save")
            {
                string[] wordsOfLove;
                if (we.IsKingSet())
                {
                    if (we.AreSpawnsSet(out wordsOfLove))
                    {
                        if (we.AreGoldBlocksSet())
                        {
                            if (we.filename == ":(")
                                MessageBox.ShowMessageBox(Save, new string[] { "YES, SAVE", "NO" }, 1, new string[] { "ARE YOU SURE YOU WANT", "TO SAVE THE MAP?" });
                            else
                                MessageBox.ShowMessageBox(Save, new string[] { "YES, SAVE", "NO" }, 1, new string[] { "ARE YOU SURE YOU WANT", "TO SAVE OVER EXISTING MAP?" });
                        }
                        else
                            MessageBox.ShowMessageBox(DumbUCantNotSaveAtThisTimeUntilYouPlaceBothTeamSpawnsMessageBoxCallBack, new string[] { "OKAY" }, 0, new string[] { "AT LEAST " + (we.gameMode == GameModes.CustomSM ? 15 : 4).ToString() + " GOLD BLOCKS MUST", "BE PLACE BEFORE YOU CAN SAVE." });
                    }
                    else
                        MessageBox.ShowMessageBox(DumbUCantNotSaveAtThisTimeUntilYouPlaceBothTeamSpawnsMessageBoxCallBack, new string[] { "OKAY" }, 0, wordsOfLove);
                }
                else
                    MessageBox.ShowMessageBox(DumbUCantNotSaveAtThisTimeUntilYouPlaceBothTeamSpawnsMessageBoxCallBack, new string[] { "OKAY" }, 0, new string[] { "THE KING OF THE BEACH REGION", "MUST BE MARKED."});
            }
            else if (id == "exit")
            {
                MessageBox.ShowMessageBox(Exit, new string[] { "YES, EXIT", "NO, STAY" }, 1, new string[] { "ARE YOU SURE YOU WANT", "TO EXIT? ANY UNSAVED", "WORK WILL BE GONE." });
            }
            else if (id == "options")
            {
                (optionsMenu["music"] as ValueMenuElement).Value = Audio.MusicVolume * 100;
                (optionsMenu["sound"] as ValueMenuElement).Value = Audio.SoundVolume * 100;
                (optionsMenu["sens"] as ValueMenuElement).Value = MinerOfDuty.PlayerSensitivity;
                (optionsMenu["invert"] as BooleanValueMenuElement).Value = MinerOfDuty.InvertYAxis;
                workin = optionsMenu;
            }
        }

        public void Back(object sender)
        {
            if (sender == mainMenu)
            {
                back.Invoke(this);
                MinerOfDuty.SaveSettings();
            }
            else
                workin = mainMenu; 
        }

        public bool ShouldDeactiveate()
        {
            return workin is EditInfoMapMenu == false;
        }

        public void Update(short timePassedInMilliseconds)
        {
            if (saving == false)
            {
                if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Start) && workin is EditInfoMapMenu == false)
                    back.Invoke(this);
                workin.Update(timePassedInMilliseconds);
            }
            else
            {
                dotdelay += timePassedInMilliseconds;
                if (dotdelay > 1000)
                {
                    dot++;
                    dotdelay = 0;
                }
                if (dot == 4)
                    dot = 0;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(InGameMenu.SideBack, Vector2.Zero, Color.White);
            workin.Draw(sb);
            if (saving)
            {
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, "SAVING MAP" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""),
                    new Vector2(640 - (Resources.Font.MeasureString("SAVING MAP").X / 2),
                        320 - (Resources.Font.LineSpacing / 2f)), Color.White);
            }
        }
    }
}
