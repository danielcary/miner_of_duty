using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Miner_Of_Duty.LobbyCode;
using Microsoft.Xna.Framework.Net;
using Miner_Of_Duty.Game;
using System.IO;
using Miner_Of_Duty.Game.Editor;
using System.Threading;
using Microsoft.Xna.Framework.Input;

namespace Miner_Of_Duty.Menus
{
    public class MainMenu : IGameScreen
    {

        private Menu mainMenu, multiplayer, gameMode;
        private Menu trialMenu;
        private Menu help1Menu, help2Menu, help3Menu, help4Menu;
        private Menu optionsMenu;
        private Menu swarmMenu, multiplayerSwarmMenu;
        private Menu creditsMenu;
        private IMenuOwner workingMenu;
        public MinerOfDuty minerOfDuty;

        private Menu customMaps;
        private Menu customPlay, customeMPlay;
        //private Menu customCreate, customMCreate;

        private EditClasses editClasses;
        private EditCharacter editCharacter;

        private MapSearchList mapSearchList;
        public MapSearchPlayList mapSearchPlayList;

        public enum State { PressStart, Normal, Joining, Finding, Creating }
        public State state;

        public void ShowCustomMenu()
        {
            workingMenu = customMaps;
        }

        public void ShowStart()
        {
            state = State.PressStart;
            NetworkSession.InviteAccepted -= minerOfDuty.NetworkSession_InviteAccepted;
        }

        public void SelectMain()
        {
            workingMenu = mainMenu;
            mainMenu.SelectFirst();
            editClasses.GoHome();
            editCharacter.GoHome();
        }

        public void GetMaps()
        {
            Thread t = new Thread(mapSearchList.FindMaps);
            t.IsBackground = true;
            t.Start();
        }      

        public MainMenu(MinerOfDuty MinerOfDuty)
        {
            this.minerOfDuty = MinerOfDuty;

            creditsMenu = new Menu(Chose, Back, new MenuElement[]{
                new MenuElement("back", "BACK")});

            mainMenu = new Menu(Chose, Back, new MenuElement[]{
                    new MenuElement("swarm", "Swarm Mode"),
                    new MenuElement("multi", "Multiplayer"),
                    new MenuElement("custom", "Custom Maps"),
                    new MenuElement("edit", "Edit Character"),
                    new MenuElement("class", "Edit Classes"),
                    new MenuElement("help", "Controls"),
                    new MenuElement("option", "Options"),
                    new MenuElement("credits", "credits"),
                    new MenuElement("exit", "Exit")
                });

            swarmMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "single")
                    {
                        MinerOfDuty.Session = NetworkSession.Create(NetworkSessionType.Local, 1, 2);
                        EventHandler<GamerLeftEventArgs> neverGonnaBe;
                        MinerOfDuty.Session.LocalGamers[0].Tag = new GamePlayerStats();
                        (MinerOfDuty.Session.LocalGamers[0].Tag as GamePlayerStats).Level = MinerOfDuty.CurrentPlayerProfile.Level;
                        MinerOfDuty.Session.StartGame();
                        SwarmManager sm = new SwarmManager(out neverGonnaBe, MinerOfDuty.Session.LocalGamers[0].Id);
                        MinerOfDuty.game = new SwarmGame(minerOfDuty, sm, minerOfDuty.GraphicsDevice, out neverGonnaBe);
                        sm.AddPlayer(MinerOfDuty.Session.LocalGamers[0].Id, TeamManager.Team.None);

                        MinerOfDuty.DrawGame();
                        Audio.SetFading(Audio.Fading.Out);
                    }
                    else if (id == "multi")
                    {
#if XBOX  
                        if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Privileges.AllowOnlineSessions == false)
                        {
                            MessageBox.ShowMessageBox(delegate(int selected)
                            {
                                try
                                {
                                    Guide.ShowSignIn(1, true);
                                }
                                catch (GamerPrivilegeException)
                                {

                                }
                                catch (GuideAlreadyVisibleException)
                                {

                                }
                            }, new string[] { "OKAY" }, 0, new string[] { "A XBOX LIVE GOLD", "ACCOUNT IS REQUIRED." });
                        }
                        else
                            workingMenu = multiplayerSwarmMenu;
#else
                        workingMenu = multiplayerSwarmMenu;
#endif
                        
                    }
                    else if (id == "back")
                    {
                        workingMenu = mainMenu;
                    }
                }, delegate(object sender)
                {
                    workingMenu = mainMenu;
                }, new MenuElement[]
                {
                    new MenuElement("back", "back"),
                    new MenuElement("single", "SINGLE PLAYER"),
                   // new MenuElement("multi", "MULTIPLAYER"),
                });


            multiplayerSwarmMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "back")
                    {
                        workingMenu = swarmMenu;
                    }
                    else if (id == "host")
                    {
                        CreateSwarmMatch();
                    }
                    else if (id == "join")
                    {
                        MinerOfDuty._searchLobby.Show(GameModes.SwarmMode);
                        MinerOfDuty.searchLobby = MinerOfDuty._searchLobby;
                        MinerOfDuty.DrawSearchLobby();
                    }

                }, delegate(object sender)
                {
                    workingMenu = swarmMenu;
                }, new MenuElement[]
                {
                    new MenuElement("back", "back"),
                    new MenuElement("host", "HOST MATCH"),
                    new MenuElement("join", "JOIN MATCH"),
                });

            trialMenu = new Menu(Chose, Back, new MenuElement[]{
                    new MenuElement("swarm", "Swarm Mode"),
                    new MenuElement("multi", "Multiplayer"),
                    new MenuElement("custom", "Custom Maps"),
                    new MenuElement("edit", "Edit Character"),
                    new MenuElement("class", "Edit Classes"),
                    new MenuElement("help", "Controls"),
                    new MenuElement("option", "Options"),
                    new MenuElement("credits", "credits"),
                    new MenuElement("buy", "buy"),
                    new MenuElement("exit", "Exit")
                });

            customMaps = new Menu(Chose, Back, new MenuElement[]{
                new MenuElement("back", "back"),
                new MenuElement("play", "play"),
                new MenuElement("create", "create"),
                new MenuElement("edit", "edit"),
                new MenuElement("mcreate", "host multiplayer Create map"),
                new MenuElement("medit", "host multiplayer edit map"),
                new MenuElement("jedit", "join multiplayer edit map")});

            customPlay = new Menu(Chose, Back, new MenuElement[]{
                new MenuElement("back", "back"),
                new MenuElement("host", "host match"),
                new MenuElement("hostprivate", "host private match"),
                new MenuElement("join", "join match")});

            //customCreate = new Menu(Chose, Back, new MenuElement[]{
            //    new MenuElement("back", "back"),
            //    new MenuElement("random", "Create Random World"),
            //    new MenuElement("flat", "Create Flat World"),
            //    new MenuElement("flatcave", "Create Flat World With Caves")});

            //customMCreate = new Menu(delegate(IMenuOwner sender, string id)
            //    {
            //        if (id == "back")
            //        {
            //            workingMenu = customMaps;
            //        }
            //        else if (id == "random")
            //        {
            //            worldGen = WorldEditor.WorldGeneration.Random;
            //            workingMenu = new CreateAMapMenu(Back, MCreateMapName);
            //        }
            //        else if (id == "flat")
            //        {
            //            worldGen = WorldEditor.WorldGeneration.Flat;
            //            workingMenu = new CreateAMapMenu(Back, MCreateMapName);
            //        }
            //        else if (id == "flatcave")
            //        {
            //            worldGen = WorldEditor.WorldGeneration.FlatWithCaves;
            //            workingMenu = new CreateAMapMenu(Back, MCreateMapName);
            //        }
            //    }, delegate(object sender)
            //    {
            //        workingMenu = customMaps;
            //    }, new MenuElement[]{
            //        new MenuElement("back", "back"),
            //        new MenuElement("random", "Create Random World"),
            //        new MenuElement("flat", "Create Flat World"),
            //        new MenuElement("flatcave", "Create Flat World With Caves")});

            
            multiplayer = new Menu(Chose, Back, new MenuElement[]{
                new MenuElement("back", "back"),
                new MenuElement("join", "quick match"),
                new MenuElement("host", "host match"),
                new MenuElement("hostprivate", "host private match"),
                new MenuElement("search", "join match"),
            });

            

            gameMode = new Menu(Chose, Back,
               new MenuElement[]{
                    new MenuElement("tdm", "Team Deathmatch"),
                    new MenuElement("ffa", "Free For All"),
                    new MenuElement("fw", "Fort Wars"),
                    new MenuElement("snm", "Search & Mine"),
                    new MenuElement("ktb", "King Of The Beach"),
                });

            help1Menu = new Menu(Chose, Back, new MenuElement[]{
                    new MenuElement("next", "".ToUpper()),
                });
            help2Menu = new Menu(Chose, Back, new MenuElement[]{
                    new MenuElement("next", "".ToUpper()),
                });
            help3Menu = new Menu(Chose, Back, new MenuElement[]{
                    new MenuElement("next", "".ToUpper()),
                });
            help4Menu = new Menu(Chose, Back, new MenuElement[]{
                    new MenuElement("next", "".ToUpper()),
                });

            optionsMenu = new Menu(Chose, Back, new MenuElement[]{
                new MenuElement("back", "back"),
                new ValueMenuElement("music", "Music Volume:", 100, "%", 0, 100, 0),
                new ValueMenuElement("sound", "Sound Volume:", 100, "%", 0, 100, 0),
                new ValueMenuElement("sens", "sensitivity:", 5, "", 1, 10, 69),
                new BooleanValueMenuElement("invert", "Invert Y: ", false, 150),
            });

            (optionsMenu["music"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["sound"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["sens"] as ValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;
            (optionsMenu["invert"] as BooleanValueMenuElement).ValueChangedEvent += MainMenu_ValueChangedEvent;

            workingMenu = mainMenu;

            editClasses = new EditClasses(Back);
            editCharacter = new EditCharacter(MinerOfDuty.GraphicsDevice, Back);

            MinerOfDuty._searchLobby = new SearchLobby(Back, minerOfDuty);
            MinerOfDuty._customSearchLobby = new CustomSearchLobby(Back, minerOfDuty);

            mapSearchList = new MapSearchList(Back, EditMap);
            mapSearchPlayList = new MapSearchPlayList(Back, HostMap);

            Audio.SetPlaylist(new Playlist(new byte[] { Audio.SONG_STORMFRONT }));
            Audio.PlaySong();
            Audio.SetFading(Audio.Fading.In);
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
                //MinerOfDuty.SaveSettings();
            }
            else if (id == "sens")
            {
                MinerOfDuty.PlayerSensitivity = (int)(optionsMenu["sens"] as ValueMenuElement).Value;
                //MinerOfDuty.SaveSettings();
            }
            else if (id == "invert")
            {
                MinerOfDuty.InvertYAxis = (optionsMenu["invert"] as BooleanValueMenuElement).Value;
               // MinerOfDuty.SaveSettings();
            }
        }

        private void SureExit(int choice)
        {   
            if (choice == 0)
            {
                minerOfDuty.ExitGame();
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private void BuyGame(int selected)
        {
            if (selected == 0)
            {
                if (Guide.IsVisible == false)
                {
                    try
                    {
                        Guide.ShowMarketplace((PlayerIndex)Input.ControllingPlayer);
                    }
                    catch (GamerPrivilegeException)
                    {
                        Guide.ShowSignIn(1, true);
                    }
                    catch (GuideAlreadyVisibleException)
                    {

                    }
                }
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private MapMetaInfo mapLoading;
        private void LoadMap(Stream s)
        {
            try
            {
                using (s)
                {
                    MemoryStream file = new MemoryStream();
                    s.Position = 0;

                    int num;
                    byte[] buffer = new byte[4096];
                    while ((num = s.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        file.Write(buffer, 0, num);
                    }

                    Thread t = new Thread(poop => StartLoadMap(file));
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private void StartLoadMap(MemoryStream file)
        {
            try
            {
                file.Position = 0;

                MinerOfDuty.editor.Initialize(minerOfDuty.GraphicsDevice, file);

                if (SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] != null)
                {
                    if (mapLoading.Author == SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag)
                        MinerOfDuty.editor.filename = mapLoading.FileName;
                }

            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private void hostMap(Stream s)
        {
            try
            {
                MemoryStream file = new MemoryStream();
                s.Position = 0;

                int num;
                byte[] buffer = new byte[4096];
                while ((num = s.Read(buffer, 0, buffer.Length)) != 0)
                {
                    file.Write(buffer, 0, num);
                }

                Thread t = new Thread(poop => StartHostMap(file));
                t.IsBackground = true;
                t.Start();
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private bool isCustomEdit = false;
        private void StartHostMap(MemoryStream ms)
        {
            try
            {
                NetworkSessionProperties props = new NetworkSessionProperties();
                props[0] = (int)GameModes.CustomMap;//4 bytes

                string name = mapLoading.MapName;

                while (name.Length < 20)
                    name += " ";

                string a = name.Substring(0, 4);
                string b = name.Substring(4, 4);
                string c = name.Substring(8, 4);
                string d = name.Substring(12, 4);
                string e = name.Substring(16, 4);

                

                props[1] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(a));
                props[2] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(b));
                props[3] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(c));
                props[4] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(d));
                props[5] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(e));
                if(mapSearchPlayList.isPrivate)
                    props[7] = 1;

                if (isCustomEdit)
                {
                    props[0] = (int)GameModes.Edit;
                    props[7] = (int)MapEditCreateType.Edit;

                    isCustomEdit = false;

                    MinerOfDuty.Session = NetworkSession.Create(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, 
                        mapLoading.GameMode == GameModes.SwarmMode || mapLoading.GameMode == GameModes.CustomSM ? 4 :
                        8, 0, props);
                    MinerOfDuty.lobby = new MapEditLobby(ms, mapLoading);
                    MinerOfDuty.DrawLobby();

                    
                }
                else
                {
                    props[6] = (int)mapLoading.GameMode;

                    MinerOfDuty.Session = NetworkSession.Create(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, 
                        mapLoading.GameMode == GameModes.SwarmMode || mapLoading.GameMode == GameModes.CustomSM ? 4 :
                        8, 0, props);
                    MinerOfDuty.lobby = new CustomLobby(minerOfDuty, ms, mapLoading);
                    MinerOfDuty.DrawLobby();
                }
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private void HostMap(MapMetaInfo map)
        {
            mapLoading = map;
            MinerOfDuty.SaveDevice.LoadAsync("Miner Of Duty Custom Maps", map.FileName, hostMap);
        }

        private void EditMap(MapMetaInfo map)
        {
            mapLoading = map;

            if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                MinerOfDuty.Session.Dispose();

            MinerOfDuty.Session = NetworkSession.Create(NetworkSessionType.Local, 1, 1);

            MinerOfDuty.editor = new Game.Editor.WorldEditor();
            MinerOfDuty.SaveDevice.LoadAsync("Miner Of Duty Custom Maps",  map.FileName, LoadMap);
            MinerOfDuty.DrawEdit();
            Audio.SetFading(Audio.Fading.Out);
        }


        private void CreateMapName(string mapName, string password, string teamAName, string teamBName, GameModes gameMode, int worldSize, WorldEditor.WorldGeneration worldGen, bool trees, bool weapons, bool editing)
        {
            Thread t = new Thread(delegate()
            {

                if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                    MinerOfDuty.Session.Dispose();

                MinerOfDuty.Session = NetworkSession.Create(NetworkSessionType.Local, 1, 1);
                MinerOfDuty.editor = new Game.Editor.WorldEditor(minerOfDuty.GraphicsDevice, worldGen, password, worldSize, gameMode, teamAName, teamBName, trees, weapons, editing);
                MinerOfDuty.editor.mapName = mapName;
                MinerOfDuty.DrawEdit();
                Audio.SetFading(Audio.Fading.Out);
            });
            t.IsBackground = true;
            t.Start();
        }

        private void MCreateMapName(string mapName, string password, string teamAName, string teamBName, GameModes gameMode, int worldSize, WorldEditor.WorldGeneration worldGen, bool trees, bool weapons, bool editing)
        {
            while (mapName.Length < 20)
                mapName += " ";

            NetworkSessionProperties nsp = new NetworkSessionProperties();

            string a = mapName.Substring(0, 4);
            string b = mapName.Substring(4, 4);
            string c = mapName.Substring(8, 4);
            string d = mapName.Substring(12, 4);
            string e = mapName.Substring(16, 4);

            nsp[0] = (int)GameModes.Edit;

            nsp[1] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(a));
            nsp[2] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(b));
            nsp[3] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(c));
            nsp[4] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(d));
            nsp[5] = EndianBitConverter.ToInt32(Encoding.UTF8.GetBytes(e));

            nsp[7] = (int)(MapEditLobby.Convert(worldGen));

            hostPasswordyeah = password;
            hostSize = worldSize;
            hostTeamAName = teamAName;
            hostTeamBName = teamBName;
            hostGameMode = gameMode;
            hostWorldGen = worldGen;
            hostTrees = trees;
            hostWeapons = weapons;
            hostEditing = editing;

            state = State.Creating;
            NetworkSession.BeginCreate(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, 8, 0, nsp,
                HostCreateMap, null);
        }

        private string hostPasswordyeah;
        private int hostSize;
        private GameModes hostGameMode;
        private string hostTeamAName;
        private string hostTeamBName;
        private WorldEditor.WorldGeneration hostWorldGen;
        private bool hostTrees, hostWeapons, hostEditing;
        private void HostCreateMap(IAsyncResult result)
        {
            MinerOfDuty.Session = NetworkSession.EndCreate(result);

            MinerOfDuty.lobby = new MapEditLobby(MapEditLobby.Convert(hostWorldGen), hostPasswordyeah, hostSize, hostGameMode, hostTeamAName, hostTeamBName, hostTrees, hostWeapons, hostEditing);
            MinerOfDuty.DrawLobby();
            state = State.Normal;
        }

        private string choosing;
        private void Chose(IMenuOwner sender, string id)
        {
            if (sender == mainMenu || sender == trialMenu)
            {
                if (id == "swarm")
                {
                    workingMenu = swarmMenu;
                }
                else if (id == "multi")
                {
                    if (Guide.IsTrialMode == false)
                    {
#if XBOX
                        
                        if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Privileges.AllowOnlineSessions == false)
                        {
                            MessageBox.ShowMessageBox(delegate(int selected)
                            {
                                try
                                {
                                    Guide.ShowSignIn(1, true);
                                }
                                catch (GamerPrivilegeException)
                                {

                                }
                                catch (GuideAlreadyVisibleException)
                                {

                                }
                            }, new string[] { "OKAY" }, 0, new string[] { "A XBOX LIVE GOLD", "ACCOUNT IS REQUIRED." });
                        }
                        else
                            workingMenu = multiplayer;
#else
                        workingMenu = multiplayer;
#endif
                    }
                    else
                    {
                        MessageBox.ShowMessageBox(BuyGame, new string[] { "Buy Game".ToUpper(), "Back".ToUpper() }, 1, new string[] { "MULTIPLAYER ISN'T AVAILABLE", "IN TRIAL MODE. WANT TO BUY?" });
                    }
                }
                else if (id == "custom")
                {
                    workingMenu = customMaps;
                }
                else if (id == "edit")
                {
                    workingMenu = editCharacter;
                }
                else if (id == "class")
                    workingMenu = editClasses;
                else if (id == "help")
                {
                    workingMenu = help1Menu;
                }
                else if (id == "option")
                {
                    workingMenu = optionsMenu;
                    optionsMenu.SelectFirst();
                    (optionsMenu["music"] as ValueMenuElement).Value = Audio.MusicVolume * 100;
                    (optionsMenu["sound"] as ValueMenuElement).Value = Audio.SoundVolume * 100;
                    (optionsMenu["sens"] as ValueMenuElement).Value = MinerOfDuty.PlayerSensitivity;
                    (optionsMenu["invert"] as BooleanValueMenuElement).Value = MinerOfDuty.InvertYAxis;
                }
                else if (id == "credits")
                {
                    creditOffset = -300;
                    workingMenu = creditsMenu;
                }
                else if (id == "exit")
                {
                    MessageBox.ShowMessageBox(SureExit, new string[] { "Yes, Exit".ToUpper(), "No, Stay".ToUpper() }, 1, new string[] { "Are you sure you want to".ToUpper(), "exit the game?".ToUpper() });
                }
                else if (id == "buy")
                {
                    MessageBox.ShowMessageBox(BuyGame, new string[] { "Buy Game".ToUpper(), "Back".ToUpper() }, 1, new string[] { "WANT TO MAKE ME HAPPY", "AND/OR BUY THE FULL GAME?" });
                }
            }
            else if (sender == creditsMenu)
            {
                workingMenu = mainMenu;
            }
            else if (sender == customMaps)
            {
                if (id == "back")
                    workingMenu = mainMenu;
                else if (id == "play")
                {
                    if (Guide.IsTrialMode == false)
                    {
#if XBOX
                        
                        if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Privileges.AllowOnlineSessions == false)
                        {
                            MessageBox.ShowMessageBox(delegate(int selected)
                            {
                            try
                            {
                                Guide.ShowSignIn(1, true);
                            }
                            catch (GamerPrivilegeException)
                            {

                            }
                            catch (GuideAlreadyVisibleException)
                            {

                            }
                            }, new string[] { "OKAY" }, 0, new string[] { "A XBOX LIVE GOLD", "ACCOUNT IS REQUIRED." });
                        }
                        else
                            workingMenu = customPlay;
#else
                        workingMenu = customPlay;
#endif
                    }
                    else
                    {
                        MessageBox.ShowMessageBox(BuyGame, new string[] { "Buy Game".ToUpper(), "Back".ToUpper() }, 1, new string[] { "MULTIPLAYER ISN'T AVAILABLE", "IN TRIAL MODE. WANT TO BUY?" });
                    }
                }
                else if (id == "create")
                    workingMenu = new CreateAMapMenu(Back, CreateMapName);
                else if (id == "edit")
                {
                    mapSearchList.Show();
                    workingMenu = mapSearchList;
                }
                else if (id == "mcreate")
                {
                    if (Guide.IsTrialMode == false)
                    {
#if XBOX
                        
                        if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Privileges.AllowOnlineSessions == false)
                        {
                            MessageBox.ShowMessageBox(delegate(int selected)
                            {
                                try
                                {
                                    Guide.ShowSignIn(1, true);
                                }
                                catch (GamerPrivilegeException)
                                {

                                }
                                catch (GuideAlreadyVisibleException)
                                {

                                }
                            }, new string[] { "OKAY" }, 0, new string[] { "A XBOX LIVE GOLD", "ACCOUNT IS REQUIRED." });
                        }
                        else
                            
                     workingMenu = new CreateAMapMenu(Back, MCreateMapName);
#else

                        workingMenu = new CreateAMapMenu(Back, MCreateMapName);
#endif
                    }
                    else
                    {
                        MessageBox.ShowMessageBox(BuyGame, new string[] { "Buy Game".ToUpper(), "Back".ToUpper() }, 1, new string[] { "MULTIPLAYER ISN'T AVAILABLE", "IN TRIAL MODE. WANT TO BUY?" });
                    }
                }
                else if (id == "medit")
                {
                    if (Guide.IsTrialMode == false)
                    {
#if XBOX
                        
                        if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Privileges.AllowOnlineSessions == false)
                        {
                            MessageBox.ShowMessageBox(delegate(int selected)
                            {
                                try
                                {
                                    Guide.ShowSignIn(1, true);
                                }
                                catch (GamerPrivilegeException)
                                {

                                }
                                catch (GuideAlreadyVisibleException)
                                {

                                }
                            }, new string[] { "OKAY" }, 0, new string[] { "A XBOX LIVE GOLD", "ACCOUNT IS REQUIRED." });
                        }
                        else
                    {                            
                        mapSearchPlayList.tag = true;
                        mapSearchPlayList.Show();
                        workingMenu = mapSearchPlayList;
                        mapSearchPlayList.isPrivate = false;
                        isCustomEdit = true;
                        }
                    
#else

                        mapSearchPlayList.tag = true;
                        mapSearchPlayList.Show();
                        workingMenu = mapSearchPlayList;
                        mapSearchPlayList.isPrivate = false;
                        isCustomEdit = true;
#endif
                    }
                    else
                    {
                        MessageBox.ShowMessageBox(BuyGame, new string[] { "Buy Game".ToUpper(), "Back".ToUpper() }, 1, new string[] { "MULTIPLAYER ISN'T AVAILABLE", "IN TRIAL MODE. WANT TO BUY?" });
                    }
                }
                else if (id == "jedit")
                {
                    if (Guide.IsTrialMode == false)
                    {
#if XBOX
                        
                        if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Privileges.AllowOnlineSessions == false)
                        {
                            MessageBox.ShowMessageBox(delegate(int selected)
                            {
                                try
                                {
                                    Guide.ShowSignIn(1, true);
                                }
                                catch (GamerPrivilegeException)
                                {

                                }
                                catch (GuideAlreadyVisibleException)
                                {

                                }
                            }, new string[] { "OKAY" }, 0, new string[] { "A XBOX LIVE GOLD", "ACCOUNT IS REQUIRED." });
                        }
                        else
                    {                            
                        
                    MinerOfDuty._searchLobby.Show(GameModes.Edit);
                    MinerOfDuty.searchLobby = MinerOfDuty._searchLobby;
                    MinerOfDuty.DrawSearchLobby();
                        }
                    
#else
                        MinerOfDuty._searchLobby.Show(GameModes.Edit);
                        MinerOfDuty.searchLobby = MinerOfDuty._searchLobby;
                        MinerOfDuty.DrawSearchLobby();
#endif
                    }
                    else
                    {
                        MessageBox.ShowMessageBox(BuyGame, new string[] { "Buy Game".ToUpper(), "Back".ToUpper() }, 1, new string[] { "MULTIPLAYER ISN'T AVAILABLE", "IN TRIAL MODE. WANT TO BUY?" });
                    }
                }
            }
            //else if (sender == customCreate)
            //{
            //    if (id == "back")
            //        workingMenu = customMaps;
            //    else if (id == "random")
            //    {
            //        worldGen = WorldEditor.WorldGeneration.Random;
            //        workingMenu = new CreateAMapMenu(Back, CreateMapName);
            //    }
            //    else if (id == "flat")
            //    {
            //        worldGen = WorldEditor.WorldGeneration.Flat;
            //        workingMenu = new CreateAMapMenu(Back, CreateMapName);
            //    }
            //    else if (id == "flatcave")
            //    {
            //        worldGen = WorldEditor.WorldGeneration.FlatWithCaves;
            //        workingMenu = new CreateAMapMenu(Back, CreateMapName);
            //    }
            //}
            else if (sender == customPlay)
            {
                if (id == "back")
                    workingMenu = customMaps;
                else if (id == "host")
                {
                    mapSearchPlayList.tag = false;
                    mapSearchPlayList.Show();
                    workingMenu = mapSearchPlayList;
                    mapSearchPlayList.isPrivate = false;
                }
                else if (id == "hostprivate")
                {
                    mapSearchPlayList.tag = false;
                    mapSearchPlayList.Show();
                    workingMenu = mapSearchPlayList;
                    mapSearchPlayList.isPrivate = true;
                }
                else if (id == "join")
                {
                    MinerOfDuty._customSearchLobby.Show();
                    MinerOfDuty.searchLobby = MinerOfDuty._customSearchLobby;
                    MinerOfDuty.DrawSearchLobby();
                }
            }
            else if (sender == help1Menu)
            {
                workingMenu = help2Menu;
            }
            else if (sender == help2Menu)
            {
                workingMenu = help3Menu;
            }
            else if (sender == help3Menu)
                workingMenu = help4Menu;
            else if (sender == optionsMenu)
            {
                if (id == "back")
                {
                    workingMenu = mainMenu;
                    MinerOfDuty.SaveSettings();
                }
            }
            else if (sender == gameMode)
            {
                if (id == "back")
                    workingMenu = mainMenu;
                else
                {
                    if (choosing == "join")
                    {
                        FindMatch(id == "tdm" ? GameModes.TeamDeathMatch : id == "ffa" ? GameModes.FreeForAll : id == "fw" ? GameModes.FortWars : id == "snm" ? GameModes.SearchNMine : GameModes.KingOfTheBeach);
                    }
                    else if (choosing == "host")
                    {
                        CreateMatch(id == "tdm" ? GameModes.TeamDeathMatch : id == "ffa" ? GameModes.FreeForAll : id == "fw" ? GameModes.FortWars : id == "snm" ? GameModes.SearchNMine : GameModes.KingOfTheBeach);
                    }
                    else if (choosing == "hostprivate")
                    {
                        CreatePrivateMatch(id == "tdm" ? GameModes.TeamDeathMatch : id == "ffa" ? GameModes.FreeForAll : id == "fw" ? GameModes.FortWars : id == "snm" ? GameModes.SearchNMine : GameModes.KingOfTheBeach);
                    }
                    else if (choosing == "search")
                    {
                        MinerOfDuty._searchLobby.Show(id == "tdm" ? GameModes.TeamDeathMatch : id == "ffa" ? GameModes.FreeForAll : id == "fw" ? GameModes.FortWars : id == "snm" ? GameModes.SearchNMine : GameModes.KingOfTheBeach);
                        MinerOfDuty.searchLobby = MinerOfDuty._searchLobby;
                        MinerOfDuty.DrawSearchLobby();
                    }
                }
            }
            else if (sender == multiplayer)
            {
                if (id == "back")
                    workingMenu = mainMenu;
                else
                {
                    choosing = id;
                    workingMenu = gameMode;
                }
            }
        }

        private void Back(object sender)
        {
            if (sender == multiplayer)
            {
                workingMenu = mainMenu;
            }
            else if (sender == gameMode)
            {
                workingMenu = multiplayer;
            }
            else if (sender == editClasses)
            {
                workingMenu = mainMenu;
            }
            else if (sender == editCharacter)
                workingMenu = mainMenu;
            else if (sender == help1Menu)
            {
                workingMenu = mainMenu;
            }
            else if (sender == help2Menu)
            {
                workingMenu = help1Menu;
            }
            else if (sender == help3Menu)
            {
                workingMenu = help2Menu;
            }
            else if (sender == help4Menu)
                workingMenu = help3Menu;
            else if (sender == optionsMenu)
            {
                workingMenu = mainMenu;
                MinerOfDuty.SaveSettings();
            }
            else if (sender == creditsMenu)
            {
                workingMenu = mainMenu;
            }
            else if (sender == MinerOfDuty._searchLobby)
            {
                if (workingMenu == multiplayerSwarmMenu)
                {
                    workingMenu = multiplayerSwarmMenu;
                }
                else if (workingMenu != customMaps)
                {
                    workingMenu = multiplayer;
                }

                MinerOfDuty.DrawMenu(false);
            }
            else if (sender == customMaps)
                workingMenu = mainMenu;
            else if (sender == customPlay)
                workingMenu = customMaps;
            //else if (sender == customCreate)
            //    workingMenu = customMaps;
            else if (sender == mapSearchList)
                workingMenu = customMaps;
            else if (sender == mapSearchPlayList)
            {
                if (mapSearchPlayList.tag == false)
                {
                    workingMenu = customPlay;
                }
                else
                    workingMenu = customMaps;
            }
            else if (sender == MinerOfDuty._customSearchLobby)
            {
                workingMenu = customPlay;
                MinerOfDuty.DrawMenu(false);
            }
            else if (sender is CreateAMapMenu)
            {
                workingMenu = null;
                workingMenu = customMaps;
            }
        }
        private Queue<Buttons> comboButtons = new Queue<Buttons>();
        private float creditOffset = -300;
        private bool goUp;
        private Color pressStartColor = Color.White;
        public void Update(GameTime gameTime)
        {
            if (state == State.PressStart)
            {
                pressStartColor.A -= (byte)(goUp == false ? 3 : -3);
                pressStartColor.B = pressStartColor.G = pressStartColor.R = pressStartColor.A;
                if (pressStartColor.A == 30)
                    goUp = true;
                else if (pressStartColor.A == 255)
                    goUp = false;

                if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Start, 0))
                {
                    if (SignedInGamer.SignedInGamers[PlayerIndex.One] == null)
                    {
                        if (Guide.IsVisible == false)
                            try
                            {
                                Guide.ShowSignIn(1, false);
                            }
                            catch (GuideAlreadyVisibleException) { }
                    }
                    else
                    {
                        Input.ControllingPlayer = 0;
                        minerOfDuty.LoadGamer(SignedInGamer.SignedInGamers[PlayerIndex.One]);
                       state = State.Normal;
                       NetworkSession.InviteAccepted += minerOfDuty.NetworkSession_InviteAccepted;
                    }
                }
                else if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Start, 1))
                {
                    if (SignedInGamer.SignedInGamers[PlayerIndex.Two] == null)
                    {
                        if (Guide.IsVisible == false)
                            try
                            {
                                Guide.ShowSignIn(1, false);
                            }
                            catch (GuideAlreadyVisibleException) { }
                    }
                    else
                    {
                        Input.ControllingPlayer = 1;
                        minerOfDuty.LoadGamer(SignedInGamer.SignedInGamers[PlayerIndex.Two]);
                        state = State.Normal;
                        NetworkSession.InviteAccepted += minerOfDuty.NetworkSession_InviteAccepted;
                    }
                }

                else if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Start, 2))
                {
                    if (SignedInGamer.SignedInGamers[PlayerIndex.Three] == null)
                    {
                        if (Guide.IsVisible == false)
                            try
                            {
                                Guide.ShowSignIn(1, false);
                            }
                            catch (GuideAlreadyVisibleException) { }
                    }
                    else
                    {
                        Input.ControllingPlayer = 2;
                        minerOfDuty.LoadGamer(SignedInGamer.SignedInGamers[PlayerIndex.Three]);
                        state = State.Normal;
                        NetworkSession.InviteAccepted += minerOfDuty.NetworkSession_InviteAccepted;
                    }
                }

                else if (Input.WasButtonPressed(Microsoft.Xna.Framework.Input.Buttons.Start, 3))
                {
                    if (SignedInGamer.SignedInGamers[PlayerIndex.Four] == null)
                    {
                        if (Guide.IsVisible == false)
                            try
                            {
                                Guide.ShowSignIn(1, false);
                            }
                            catch (GuideAlreadyVisibleException) { }
                    }
                    else
                    {
                        Input.ControllingPlayer = 3;
                        minerOfDuty.LoadGamer(SignedInGamer.SignedInGamers[PlayerIndex.Four]);
                        state = State.Normal;
                        NetworkSession.InviteAccepted += minerOfDuty.NetworkSession_InviteAccepted;
                    }
                }
            }
            else if(state == State.Normal)
            {
              

                if (SpecialGamer.IsDev(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer]))
                {
                    if (Input.WasButtonPressed(Buttons.X))
                        comboButtons.Enqueue(Buttons.X);

                    if (Input.WasButtonPressed(Buttons.Y))
                        comboButtons.Enqueue(Buttons.Y);

                    if (comboButtons.Count == 5)
                    {
                        if (comboButtons.Dequeue() == Buttons.X
                            && comboButtons.Dequeue() == Buttons.X
                            && comboButtons.Dequeue() == Buttons.X
                            && comboButtons.Dequeue() == Buttons.Y
                            && comboButtons.Dequeue() == Buttons.X)
                        {
                            NetworkSessionProperties nps = new NetworkSessionProperties();
                            nps[0] = (int)GameModes.SwordGiver;

                            MinerOfDuty.Session = NetworkSession.Create(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, 2, 1,
                                nps);
                            MinerOfDuty.lobby = new Lobby(minerOfDuty, GameModes.SwordGiver);
                            MinerOfDuty.DrawLobby();

                            

                           
                        }
                    }

                }
                workingMenu.Update((short)gameTime.ElapsedGameTime.Milliseconds);
            }

            if (state != State.PressStart && state != State.Normal)
            {
                dotDelay += gameTime.ElapsedGameTime.Milliseconds;
                if (dotDelay > 1000)
                {
                    dot++;
                    dotDelay = 0;
                }
                if (dot == 4)
                    dot = 0;
            }

            if (Guide.IsTrialMode)
            {
                if (workingMenu == mainMenu)
                    workingMenu = trialMenu;
            }
            else if (workingMenu == trialMenu)
                workingMenu = mainMenu;

            if (workingMenu == creditsMenu)
            {
                creditOffset += 2200 * (float)gameTime.ElapsedGameTime.TotalMinutes;

                if (Input.IsThumbstickOrDPad(Input.Direction.Down))
                    creditOffset += 150 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                else if (Input.IsThumbstickOrDPad(Input.Direction.Up))
                    creditOffset -= 175 * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (creditOffset < -150)
                    creditOffset = -150;

                if (creditOffset > 700)
                    creditOffset = 700;
            }
        }

        public void Render(GraphicsDevice gd)
        {
            if (workingMenu == editCharacter)
                editCharacter.Render();
        }

        private int dot;
        private int dotDelay;
        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);

            if (state == State.PressStart)
            {
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640, 360), null, Color.White, 0, new Vector2(Resources.MessageBoxBackTexture.Width / 2, Resources.MessageBoxBackTexture.Height / 2), 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.TitleFont, "PRESS START TO BEGIN", new Vector2(640, 360), pressStartColor, 0, Resources.TitleFont.MeasureString("PRESS START TO BEGIN") / 2f, 1, SpriteEffects.None, 0);
            }
            else if(state == State.Normal)
            {
                workingMenu.Draw(sb);
            }
            else if (state == State.Creating)
            {
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, "CREATING SESSION" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""), new Vector2(640 - (Resources.Font.MeasureString("Creating Session").X / 2f), 320 - (Resources.Font.LineSpacing / 2f)), Color.White);
            }
            else if (state == State.Finding)
            {
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, "FINDING SESSIONS" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""), new Vector2(640 - (Resources.Font.MeasureString("Finding Session").X / 2f), 320 - (Resources.Font.LineSpacing / 2f)), Color.White);
            }
            else if (state == State.Joining)
            {
                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, "JOINING SESSION" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""), new Vector2(640 - (Resources.Font.MeasureString("Joining Session").X / 2f), 320 - (Resources.Font.LineSpacing / 2f)), Color.White);
            }

            if (workingMenu == creditsMenu)
            {
                sb.Draw(Resources.WhiteScreen, new Rectangle(0, 0, 1280, 720), new Color(Color.Black.ToVector4() /3f));

                sb.DrawString(Resources.Font, "LEAD PROGRAMMER / DEVELOPER, ART", new Vector2(280, 185 - creditOffset), Color.White);
                sb.DrawString(Resources.Font, "- DANNY C.",
                    new Vector2(275 + Resources.Font.MeasureString("LEAD PROGRAMMER / DEVELOPER, ART").X - Resources.Font.MeasureString("- DANNY C.").X, 185 - creditOffset + Resources.Font.LineSpacing * 1.25f)
                , Color.White);
                sb.DrawString(Resources.Font, "MARKETING, PR, MEDIA, WEB DEV", new Vector2(315, 340 - creditOffset), Color.White);
                sb.DrawString(Resources.Font, "- NICK V.",
                    new Vector2(315 + Resources.Font.MeasureString("MARKETING, PR, MEDIA, WEB DEV").X - Resources.Font.MeasureString("- NICK V.").X, 340 - creditOffset + Resources.Font.LineSpacing * 1.25f)
                    , Color.White);
                //(Nickev Dev)
                sb.DrawString(Resources.DescriptionFont, "(Nickev Dev)",
                    new Vector2(315 + Resources.Font.MeasureString("MARKETING, PR, MEDIA, WEB DEV").X - Resources.DescriptionFont.MeasureString("(Nickev Dev)").X, 340 - creditOffset + Resources.Font.LineSpacing * 2.25f)
                    , Color.White);

                sb.DrawString(Resources.NameFont, "MUSIC - KEVIN MACLEOD", new Vector2(640, 540 - creditOffset), Color.White, 0, Resources.NameFont.MeasureString("MUSIC - KEVIN MACLEOD") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.DescriptionFont, "(CC BY 3.0)", new Vector2(600 + (Resources.NameFont.MeasureString("MUSIC - KEVIN MACLEOD").X / 2), 540 - creditOffset + Resources.NameFont.LineSpacing), 
                    Color.White, 0,
                    Resources.NameFont.MeasureString("(CC BY 3.0)") / 2f, 1, SpriteEffects.None, 0);


                sb.DrawString(Resources.Font, "TESTERS", new Vector2(550, 700 - creditOffset), Color.White);
                sb.DrawString(Resources.Font, "- LJ x John x LJ",
                    new Vector2(475 + Resources.Font.MeasureString("TESTERS").X, 700 - creditOffset + Resources.Font.LineSpacing * 1.25f)
                , Color.White);
                sb.DrawString(Resources.Font, "- almantux11",
                    new Vector2(475 + Resources.Font.MeasureString("TESTERS").X, 700 - creditOffset + Resources.Font.LineSpacing * 2.25f)
                , Color.White);

                sb.DrawString(Resources.NameFont, "WEBSITE: WWW.MINEROFDUTY.COM", new Vector2(640, 950 - creditOffset), Color.White, 0,
                    Resources.NameFont.MeasureString("WEBSITE: WWW.MINEROFDUTY.COM") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.NameFont, "FORUMS: WWW.MINEROFDUTYFORUMS.COM", new Vector2(640, 950 - creditOffset + (Resources.NameFont.LineSpacing * 1)), Color.White, 0,
                    Resources.NameFont.MeasureString("FORUMS: WWW.MINEROFDUTYFORUMS.COM") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.NameFont, "TWITTER: WWW.TWITTER.COM/MINEROFDUTY", new Vector2(640, 950 - creditOffset + (Resources.NameFont.LineSpacing * 2)), Color.White, 0,
                    Resources.NameFont.MeasureString("TWITTER: WWW.TWITTER.COM/MINEROFDUTY") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.NameFont, "FACEBOOK: WWW.FACEBOOK.COM/MINEROFDUTY", new Vector2(640, 950 - creditOffset + (Resources.NameFont.LineSpacing * 3)), Color.White, 0,
                    Resources.NameFont.MeasureString("FACEBOOK: WWW.FACEBOOK.COM/MINEROFDUTY") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.NameFont, "YOUTUBE: WWW.YOUTUBE.COM/MINEROFDUTY", new Vector2(640, 950 - creditOffset + (Resources.NameFont.LineSpacing * 4)), Color.White, 0,
                    Resources.NameFont.MeasureString("YOUTUBE: WWW.YOUTUBE.COM/MINEROFDUTY") / 2f, 1, SpriteEffects.None, 0);

                sb.DrawString(Resources.DescriptionFont, "v1.3", new Vector2(1000, 1250 - creditOffset), Color.White);

                workingMenu.Draw(sb);

            }
            else if (workingMenu == help1Menu || workingMenu == help2Menu || workingMenu == help3Menu || workingMenu == help4Menu)
            {
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);
                if (workingMenu == help1Menu)
                {
                    sb.Draw(Resources.Help1Texture, new Vector2(160, 90), Color.White);
                }
                else if (workingMenu == help2Menu)
                {
                    sb.Draw(Resources.Help2Texture, new Vector2(160, 90), Color.White);
                }
                else if (workingMenu == help3Menu)
                {
                    sb.Draw(Resources.Help3Texture, new Vector2(160, 90), Color.White);
                }
                else
                {
                    sb.DrawString(Resources.Font, "FOR MORE INFO ON GAME MODES", new Vector2(640, 250), Color.White, 0, Resources.Font.MeasureString("FOR MORE INFO ON GAME MODES") / 2f, 1, SpriteEffects.None, 0);
                    sb.DrawString(Resources.Font, "AND GUN STATS/UNLOCKS CHECK", new Vector2(640, 300), Color.White, 0, Resources.Font.MeasureString("AND GUN STATS/UNLOCKS CHECK") / 2f, 1, SpriteEffects.None, 0);
                    sb.DrawString(Resources.Font, "OUT MINER OF DUTY'S WEBSITE:", new Vector2(640, 350), Color.White, 0, Resources.Font.MeasureString("OUT MINER OF DUTY'S WEBSITE:") / 2f, 1, SpriteEffects.None, 0);
                    sb.DrawString(Resources.Font, "minerofduty.com", new Vector2(640, 400), Color.White, 0, Resources.Font.MeasureString("minerofduty.com") / 2f, 1, SpriteEffects.None, 0);
                }
                if (workingMenu != help4Menu)
                    sb.DrawString(Resources.Font, "(B) BACK  (A) NEXT", new Vector2(140, 590), Color.White);
                else
                    sb.DrawString(Resources.Font, "(B) BACK", new Vector2(140, 590), Color.White);
            }
        }

        #region networking
        private void DoneCreateMatch(IAsyncResult result)
        {
            MinerOfDuty.Session = NetworkSession.EndCreate(result);
            MinerOfDuty.Session.AllowHostMigration = true;
            MinerOfDuty.lobby = new Lobby(minerOfDuty, (GameModes)MinerOfDuty.Session.SessionProperties[0].Value);
            MinerOfDuty.DrawLobby();
            state = State.Normal;
            dot = 0;
        }

        private void CreateMatch(GameModes gameMode)
        {
            NetworkSessionProperties n = new NetworkSessionProperties();
            n[0] = (int)gameMode;
            NetworkSession.BeginCreate(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, 8, 0, n, DoneCreateMatch, null);
            state = State.Creating;
            dot = 0;
        }

        private void CreateSwarmMatch()
        {
            NetworkSessionProperties n = new NetworkSessionProperties();
            n[0] = (int)GameModes.SwarmMode;
            NetworkSession.BeginCreate(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, 4, 0, n, DoneCreateMatch, null);
            state = State.Creating;
            dot = 0;
        }

        private void CreatePrivateMatch(GameModes gameMode)
        {
            NetworkSessionProperties n = new NetworkSessionProperties();
            n[0] = (int)gameMode;
            n[7] = 1;
            NetworkSession.BeginCreate(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, 8, 7, n, DoneCreateMatch, null);
            state = State.Creating;
            dot = 0;
        }

        private GameModes findingGameMode;
        private void DoneFindMatch(IAsyncResult result)
        {
            if (MinerOfDuty.SessionCollection != null && MinerOfDuty.SessionCollection.IsDisposed == false)
                MinerOfDuty.SessionCollection.Dispose();



            MinerOfDuty.SessionCollection = NetworkSession.EndFind(result);

            if (MinerOfDuty.SessionCollection.Count == 0)
            {
                MinerOfDuty.SessionCollection.Dispose();

                //lets make one
                NetworkSessionProperties n = new NetworkSessionProperties();
                n[0] = (int)findingGameMode;
                NetworkSession.BeginCreate(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, 8, 0, n, DoneCreateMatch, null);
                state = State.Creating;
                dot = 0;

                
            }
            else
            {
                int bestSes = 0;
                for (int i = 0; i < MinerOfDuty.SessionCollection.Count; i++)
                {
                    if (MinerOfDuty.SessionCollection[i].QualityOfService.AverageRoundtripTime < MinerOfDuty.SessionCollection[bestSes].QualityOfService.AverageRoundtripTime)
                        bestSes = i;
                }

                NetworkSession.BeginJoin(MinerOfDuty.SessionCollection[bestSes], DoneJoinMatch, null);
                state = State.Joining;
                dot = 0;
            }
        }

        private void DoneJoinMatch(IAsyncResult result)
        {
            try
            {
                MinerOfDuty.Session = NetworkSession.EndJoin(result);
                if (MinerOfDuty.Session.SessionState == NetworkSessionState.Lobby)
                {
                    MinerOfDuty.lobby = new Lobby(minerOfDuty, (GameModes)MinerOfDuty.Session.SessionProperties[0].Value);
                    MinerOfDuty.DrawLobby();
                }
                else if (MinerOfDuty.Session.SessionState == NetworkSessionState.Playing)
                {
                    MinerOfDuty.lobby = new Lobby((GameModes)MinerOfDuty.Session.SessionProperties[0].Value);
                }
                else
                    throw new NetworkSessionJoinException();
                

                state = State.Normal;
                dot = 0;
            }
            catch (NetworkSessionJoinException)
            {
                workingMenu = gameMode;
                state = State.Normal;
            }
        }

        private void FindMatch(GameModes gameMode)
        {
            findingGameMode = gameMode;
            NetworkSessionProperties n = new NetworkSessionProperties();
            n[0] = (int)gameMode;
            NetworkSession.BeginFind(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, n, DoneFindMatch, null);
            state = State.Finding;
            dot = 0;
        }
        #endregion






        public void Activated()
        {
        }

        public void Deactivated()
        {
        }

    }
}
