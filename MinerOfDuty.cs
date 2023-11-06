using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using EasyStorage;
using Miner_Of_Duty.Game;
using System.IO;
using System.Threading;
using Miner_Of_Duty.Menus;
using Miner_Of_Duty.LobbyCode;
using Miner_Of_Duty.Game.Editor;
using Microsoft.Xna.Framework.Net;

namespace Miner_Of_Duty
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MinerOfDuty : Microsoft.Xna.Framework.Game
    {
        public static PlayerProfile CurrentPlayerProfile;
        private static int playerSensitivity;
        public static int PlayerSensitivity { get { return playerSensitivity; } set { playerSensitivity = value; Player.SetRotSpeed(playerSensitivity); } }
        private static bool invertYAxis;
        public static bool InvertYAxis { get { return invertYAxis; } set { invertYAxis = value; } }
        public static float InvertY(float val)
        {
            if (!invertYAxis)
                return val;
            else
                return -1 * val;
        }

        public static SaveDevice SaveDevice;
        public static NetworkSession Session;
        public static AvailableNetworkSessionCollection SessionCollection;

        #region Saving
        public static void LoadSettings()
        {
            Thread t = new Thread(delegate()
                {

                    try
                    {
                        if (SaveDevice.FileExists("Miner Of Duty Settings", "Settings"))
                        {
                            SaveDevice.LoadAsync("Miner Of Duty Settings", "Settings", new EasyStorage.FileAction(loadSettings));
                        }
                        else
                        {
                            PlayerSensitivity = 5;
                            Audio.SetMusicVolume(1);
                            Audio.SetSoundVolume(1);
                            invertYAxis = false;
                            SaveDevice.SaveAsync("Miner Of Duty Settings", "Settings", new EasyStorage.FileAction(saveSettings));
                        }
                    }
                    catch (InvalidOperationException) { }
                    catch (NullReferenceException) { }
                });
            t.IsBackground = true;
            t.Name = "Load Settings poop";
            t.Start();
        }

        public static void SaveSettings()
        {
            SaveDevice.SaveAsync("Miner Of Duty Settings", "Settings", new EasyStorage.FileAction(saveSettings));
        }

        private static void saveSettings(Stream s)
        {
            try
            {
                using (BinaryWriter bw = new BinaryWriter(s))
                {
                    bw.Write(1);//version
                    bw.Write(Audio.MusicVolume);
                    bw.Write(Audio.SoundVolume);
                    bw.Write(PlayerSensitivity);
                    bw.Write(InvertYAxis);
                }
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        private static void loadSettings(Stream s)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(s))
                {
                    br.ReadInt32();
                    Audio.SetMusicVolume(br.ReadSingle());
                    Audio.SetSoundVolume(br.ReadSingle());
                    PlayerSensitivity = br.ReadInt32();
                    InvertYAxis = br.ReadBoolean();
                }
            }
            catch (Exception e)
            {
                lock (MinerOfDuty.ExceptionsLock)
                    MinerOfDuty.Exceptions.Enqueue(e);
            }
        }

        public void LoadProfile(string gamerTag)
        {
           
                Thread t = new Thread(delegate()
                    {
                        try
                        {
                            if (SaveDevice.FileExists("Miner Of Duty Player Profiles", gamerTag))
                            {
                                SaveDevice.LoadAsync("Miner Of Duty Player Profiles", gamerTag, new EasyStorage.FileAction(PlayerProfile.LoadPlayerProfile));
                            }
                            else
                            {
                                CurrentPlayerProfile = new PlayerProfile();
                                SaveDevice.SaveAsync("Miner Of Duty Player Profiles", gamerTag, new EasyStorage.FileAction(CurrentPlayerProfile.Save));
                            }

                            if (SaveDevice.FileExists("MOD UNLOCKABLES", "SWORD"))
                            {
                                SaveDevice.Load("MOD UNLOCKABLES", "SWORD", delegate(Stream s)
                                {
                                    string name = "";
                                    using (BinaryReader br = new BinaryReader(s))
                                    {
                                        while (br.BaseStream.Position < br.BaseStream.Length)
                                        {
                                            br.ReadByte();
                                            if (br.BaseStream.Position < br.BaseStream.Length)
                                                name += br.ReadChar();
                                        }
                                    }
                                    if (name == gamerTag)
                                        CurrentPlayerProfile.HasSword = true;
                                });
                            }
                        }
                        catch (InvalidOperationException) { }
                        catch (NullReferenceException) { }
                    });
                t.IsBackground = true;
                t.Name = "Load Profile poop";
                t.Start();
        }

        public void LoadGamer(SignedInGamer gamer)
        {
            if (SaveDevice == null)
                return;
            LoadProfile(gamer.Gamertag);
        }

        public void SaveDeviceSelected(object sender, EventArgs e)
        {
            try
            {
                LoadProfile(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag);
            }
            catch (Exception ee)
            {
                System.Diagnostics.Debug.WriteLine(ee.Message);//if only one controller is on and its in seconds slot this crashes
            }
            LoadSettings();
            mainMenu.GetMaps();
        }
        #endregion

        public static ContentManager ContentManager;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static MainMenu mainMenu;
        public static ILobby lobby;
        public static MultiplayerGame game;
        public static WorldEditor editor;
        public static IGameScreen searchLobby;
        public static SearchLobby _searchLobby;
        public static CustomSearchLobby _customSearchLobby;

        public static IGameScreen gameScreen;

        public static MinerOfDuty Self;

        public static void DrawLobby()
        {
            if (game != null)
            {
                game.UnSub();
                game.Dispose();
            }
            game = null;
            gameScreen = lobby;

            if (editor != null)
            {
                editor.Dispose();
            }
            editor = null;

            if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer] != null)
                SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Presence.PresenceMode = GamerPresenceMode.WaitingInLobby;
        }

        public static void DrawSearchLobby()
        {
            gameScreen = searchLobby;

            if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer] != null)
                SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Presence.PresenceMode = GamerPresenceMode.LookingForGames;
        }

        public static void DrawEdit()
        {
            gameScreen = editor;

           

            if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer] != null)
                SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Presence.PresenceMode = GamerPresenceMode.EditingLevel;

        }

        public static void DrawGame()
        {
            gameScreen = game;

            if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer] != null)
                if (game != null)
                {
                    if (game is SwarmGame)
                        SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Presence.PresenceMode = GamerPresenceMode.SinglePlayer;
                    else
                        SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Presence.PresenceMode = GamerPresenceMode.Multiplayer;
                }
        }

        public static void DrawMenu()
        {
            DrawMenu(true);
        }

        public static void DrawMenu(bool startMusic)
        {
            gameScreen = mainMenu;
            if (game != null)
            {
                game.UnSub();
                game.Dispose();
            }

            game = null;

            if (editor != null)
                editor.Dispose();

            editor = null;

            if (lobby != null)
                lobby.LeaveLobby(-1);

            lobby = null;

            if (Session != null && Session.IsDisposed == false)
                Session.Dispose();
            Session = null;

            if (SessionCollection != null && SessionCollection.IsDisposed == false)
                SessionCollection.Dispose();
            SessionCollection = null;

            if (startMusic)
            {
                Audio.Stop();
                Audio.SetPlaylist(new Playlist(new byte[] { Audio.SONG_STORMFRONT }));
                Audio.PlaySong();
                Audio.SetFading(Audio.Fading.In);
            }
            try
            {
                if (SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer] != null)
                    SignedInGamer.SignedInGamers[(PlayerIndex) Input.ControllingPlayer].Presence.PresenceMode = GamerPresenceMode.AtMenu;
            }
            catch (ArgumentOutOfRangeException) { }//probaly dont need this
        }


        private void SignedInGamer_SignedIn(object sender, SignedInEventArgs e)
        {
            if (mainMenu == null || mainMenu.state != MainMenu.State.PressStart)
                return;


            if (SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] != null)
                SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Presence.PresenceMode = GamerPresenceMode.AtMenu;

            if (SaveDevice == null)
                return;
            if (e.Gamer.IsSignedInToLive)
            {
                LoadProfile(e.Gamer.Gamertag);
            }
        }

        private bool waitingForGame;
        private bool WaitTillSignOut;
        private void SignedInGamer_SignedOut(object sender, SignedOutEventArgs e)
        {
            if (e.Gamer.PlayerIndex == (PlayerIndex)Input.ControllingPlayer)
            {
                if (game != null && game.isGenerated == false)
                {
                    waitingForGame = true;
                    WaitTillSignOut = true;
                    return;
                }

                if (editor != null && editor.isGenerated == false)
                {
                    waitingForGame = false;
                    WaitTillSignOut = true;
                    return;
                }

                DrawMenu();
                mainMenu.ShowStart();
                mainMenu.SelectMain();
            }
        }

        public void ExitGame()
        {
            this.Exit();
        }


        public MinerOfDuty()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Components.Add(new GamerServicesComponent(this));

            Self = this;
        }

        public void NetworkSession_InviteAccepted(object sender, InviteAcceptedEventArgs e)
        {
            if (e.Gamer.PlayerIndex != (PlayerIndex)Input.ControllingPlayer)
                return;

            DrawMenu();
            DrawInvite = true;
            dot = 0;

            try
            {
                NetworkSession.BeginJoinInvited(1, AcceptInvite, null);
            }
            catch (Exception) { }
        }
        private void OkayFailed(int selected)
        {
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private void AcceptInvite(IAsyncResult result)
        {
            try
            {
                DrawInvite = false;
                Session = NetworkSession.EndJoinInvited(result);

                try
                {
                    if (Session.SessionProperties[0].Value == (int)GameModes.CustomMap)
                    {
                        //join custom lobby
                        lobby = new CustomLobby(this);
                        DrawLobby();
                    }
                    else if (Session.SessionProperties[0].Value == (int)GameModes.Edit)
                    {
                        if (Session.SessionState == NetworkSessionState.Lobby)
                        {
                            lobby = new MapEditLobby((MapEditCreateType)Session.SessionProperties[7].Value);
                            DrawLobby();
                        }
                        else
                        {
                            editor = new WorldEditor(-2);
                            DrawEdit();
                        }
                    }
                    else if (Session.SessionProperties[0].Value == (int)GameModes.SwordGiver)
                    {
                        MessageBox.ShowMessageBox(delegate(int selected) { }, new string[] { "OKAY" }, 0, new string[] { "SWORD UNLOCKED." });
                        SaveDevice.Save("MOD UNLOCKABLES", "SWORD", new FileAction(delegate(Stream s) 
                            {
                                string name = SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag;
                                Random ran = new Random();
                                using (BinaryWriter bw = new BinaryWriter(s))
                                {
                                    for (int i = 0; i < name.Length; i++)
                                    {
                                        bw.Write((byte)ran.Next(0, 256));
                                        bw.Write(name[i]);
                                    }
                                }
                            
                            }));
                        Session.Dispose();
                        Session = null;
                    }
                    else if (Session.SessionState == NetworkSessionState.Playing)
                    {
                        lobby = new Lobby((GameModes)Session.SessionProperties[0].Value);
                        
                    }
                    else 
                    {
                        lobby = new Lobby(this, (GameModes)Session.SessionProperties[0].Value);
                        DrawLobby();
                    }
                }
                catch (Exception e)
                {
                    lock (MinerOfDuty.ExceptionsLock)
                        MinerOfDuty.Exceptions.Enqueue(e);
                }
            }
            catch (NetworkSessionJoinException)
            {
                DrawInvite = false;
                MessageBox.ShowMessageBox(OkayFailed, new string[] { "OKAY" }, 0, new string[] { "JOINING INVITE FAILED!" });
            }
        }

        private bool DrawInvite = false;

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            
            graphics.ApplyChanges();

           // Guide.SimulateTrialMode = true;

          //  Components.Add(new FrameRateCounter(this));

            base.Initialize();
        }


        private void Restart()
        {
            cg = null;
            System.GC.Collect();
            
            CurrentPlayerProfile = new PlayerProfile();

            SaveDevice = new EasyStorage.SharedSaveDevice();
            Components.Add(SaveDevice);
            SaveDevice.DeviceSelectorCanceled += (s, e) => e.Response = EasyStorage.SaveDeviceEventResponse.Force;
            SaveDevice.DeviceDisconnected += (s, e) => e.Response = EasyStorage.SaveDeviceEventResponse.Force;
            SaveDevice.DeviceSelected += SaveDeviceSelected;

            mainMenu = new MainMenu(this);
            gameScreen = mainMenu;

            SaveDevice.PromptForDevice();

            

            isDoneLoading = true;
        }

        private bool isDoneLoading = false;
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ContentManager = Content;

            Resources.MainMenuTexture = Content.Load<Texture2D>("Menu/background");

            Resources.Font = Content.Load<SpriteFont>("Fonts/34");
            Resources.Font.Spacing = -4;
            Resources.TitleFont = Content.Load<SpriteFont>("Fonts/46");
            Resources.TitleFont.Spacing = -4;
            Resources.DescriptionFont = Content.Load<SpriteFont>("Fonts/24");
            Resources.DescriptionFont.Spacing = -7.5f;
            Resources.NameFont = Content.Load<SpriteFont>("Fonts/32");
            Resources.NameFont.Spacing -= 4;

            Thread t = new Thread(poop => Resources.LoadContent(Content, DoneLoad, GraphicsDevice, graphics));
            t.Name = "Loading Thread";
            t.IsBackground = true;
            t.Start();


            
        }

        private void DoneLoad(IAsyncResult result)
        {
            SignedInGamer.SignedOut += new EventHandler<SignedOutEventArgs>(SignedInGamer_SignedOut);
            SignedInGamer.SignedIn += new EventHandler<SignedInEventArgs>(SignedInGamer_SignedIn);


            MinerOfDuty.CurrentPlayerProfile = new PlayerProfile();

            
            Audio.SetMusicVolume(0);

            MinerOfDuty.mainMenu = new MainMenu(this);
            MinerOfDuty.gameScreen = MinerOfDuty.mainMenu;

            MinerOfDuty.SaveDevice = new EasyStorage.SharedSaveDevice();
            Components.Add(SaveDevice);
            SaveDevice.DeviceSelectorCanceled += (s, e) => e.Response = EasyStorage.SaveDeviceEventResponse.Force;
            SaveDevice.DeviceDisconnected += (s, e) => e.Response = EasyStorage.SaveDeviceEventResponse.Force;
            SaveDevice.DeviceSelected += SaveDeviceSelected;

            SaveDevice.PromptForDevice();

            isDoneLoading = true;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public static Queue<Exception> Exceptions = new Queue<Exception>();
        public static readonly object ExceptionsLock = new object();
        private void TestThread()
        {
            try
            {
                throw new Exception("POOP");
            }
            catch (Exception e)
            {
                lock (ExceptionsLock)
                    Exceptions.Enqueue(e);
            }
        }

        private void TestForExceptions()
        {
            lock (ExceptionsLock) //do this twice
            {
                if (Exceptions.Count > 0)
                {
                    cg = new CrashedGame(Exceptions.Dequeue());

                    Exceptions.Clear();

                    try
                    {
                        DrawMenu(false);
                    }
                    catch (Exception) { }

                    try
                    {
                        Audio.Stop();
                    }
                    catch (Exception) { }

                    editor = null;
                    game = null;
                    try
                    {
                        mainMenu.ShowStart();
                    }
                    catch { }
                    mainMenu = null;
                    lobby = null;
                    searchLobby = null;
                    _searchLobby = null;
                    _customSearchLobby = null;
                    gameScreen = null;
                    if(SaveDevice != null)
                        Components.Remove(SaveDevice);
                    SaveDevice = null;
                    MessageBox.CloseMessageBox();

                    
                }
            }
        }

        private CrashedGame cg = null;
        private int timeLoading;
        private int dot;
        private int dotDelay;
        private bool wasShown;
        protected override void Update(GameTime gameTime)
        {

            TestForExceptions();

            if (cg != null)
            {
                cg.Update(gameTime);
                if (cg.Restart)
                    Restart();
                base.Update(gameTime);
                return;
            }

            if (DrawInvite)
            {
                Input.Update();
                Audio.Update(gameTime);

                if (MessageBox.IsMessageBeingShown)
                {
                    MessageBox.Update(gameTime);
                    Input.Flush();
                    wasShown = true;
                }

                timeLoading += gameTime.ElapsedGameTime.Milliseconds;
                dotDelay += gameTime.ElapsedGameTime.Milliseconds;
                if (dotDelay > 1000)
                {
                    dot++;
                    dotDelay = 0;
                }
                if (dot == 4)
                    dot = 0;

                if (wasShown)
                {
                    Input.ReFill();
                    wasShown = false;
                }
            }
            if (isDoneLoading)
            {
                Input.Update();
                Audio.Update(gameTime);

                try
                {
                    if (MessageBox.IsMessageBeingShown)
                    {
                        MessageBox.Update(gameTime);
                        Input.Flush();
                        wasShown = true;
                    }

                    if (gameScreen != null)
                        gameScreen.Update(gameTime);

                    if (WaitTillSignOut)
                    {
                        if (waitingForGame ? game.isGenerated : editor.isGenerated)
                        {
                            DrawMenu();
                            mainMenu.ShowStart();
                            mainMenu.SelectMain();
                            WaitTillSignOut = false;
                        }
                    }

                    if (wasShown)
                    {
                        Input.ReFill();
                        wasShown = false;
                    }
                }
                catch (Exception e)
                {
                    lock (ExceptionsLock)
                        Exceptions.Enqueue(e);
                }

            }
            else
            {
                timeLoading += gameTime.ElapsedGameTime.Milliseconds;
                dotDelay += gameTime.ElapsedGameTime.Milliseconds;
                if (dotDelay > 1000)
                {
                    dot++;
                    dotDelay = 0;
                }
                if (dot == 4)
                    dot = 0;
            }

            TestForExceptions();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (cg != null)
            {
                cg.Render(GraphicsDevice);

                spriteBatch.Begin();
                cg.Draw(spriteBatch);
                spriteBatch.End();
            }
            else if (DrawInvite)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                spriteBatch.DrawString(Resources.Font, "JOINING INVITE" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""), new Vector2(640, 360), Color.White, 0, Resources.Font.MeasureString("JOINING INVITE") / 2f, 1, SpriteEffects.None, 0);
                spriteBatch.End();
            }
            else if (isDoneLoading)
            {
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

                try
                {
                    gameScreen.Render(GraphicsDevice);
                }
                catch (Exception e)
                {
                    lock (ExceptionsLock)
                        Exceptions.Enqueue(e);
                }

                spriteBatch.Begin();

                try
                {
                    gameScreen.Draw(spriteBatch);


                    if (MessageBox.IsMessageBeingShown)
                        MessageBox.Draw(spriteBatch);
                }
                catch (Exception e)
                {
                    lock (ExceptionsLock)
                        Exceptions.Enqueue(e);
                }

               // spriteBatch.DrawString(Resources.Font, (System.GC.GetTotalMemory(false) / 1000f / 1000f).ToString(), new Vector2(100, 100), Color.White);

                try
                {
                    spriteBatch.End();
                }
                catch (InvalidOperationException)
                {
                    GraphicsDevice.SetRenderTarget(null);
                    spriteBatch.End();
                }
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                //  if (timeLoading < 4000)
                //  {
                //      spriteBatch.Draw(Resources.DannyTexture, new Vector2(640, 360), null, Color.White, 0, new Vector2(Resources.DannyTexture.Width, Resources.DannyTexture.Height) / 2f, 1, SpriteEffects.None, 0);
                //      spriteBatch.Draw(Resources.CreditsTexture, new Vector2(275, 600), null, Color.White, 0, new Vector2(Resources.CreditsTexture.Width, Resources.CreditsTexture.Height) / 2f, 1, SpriteEffects.None, 0);
                //  }
                //   else
                spriteBatch.DrawString(Resources.Font, "LOADING" + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""), new Vector2(640, 360), Color.White, 0, Resources.Font.MeasureString("LOADING") / 2f, 1, SpriteEffects.None, 0);
                spriteBatch.End();
            }

            

            base.Draw(gameTime);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            if (gameScreen != null)
                gameScreen.Activated();
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            if (gameScreen != null)
                gameScreen.Deactivated();
        }
    }
}
