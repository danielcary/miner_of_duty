using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Miner_Of_Duty.Game.Networking;
using Miner_Of_Duty.Game;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.GamerServices;

namespace Miner_Of_Duty.LobbyCode
{


    public class CustomLobby : ILobby
    {
        #region vars
        private MinerOfDuty minerOfDuty;

        private EditClasses editClasses;
        private ProfileView profileViewer;

        private string title;
        private Vector2 titlePos;

        private IMenuOwner otherScreen;
        private Menu sideMenu;

        private int countDownTimeInSeconds = 0;
        private int countDownMilli = 0;

        private bool useLeftMenu = true;

        private int selectedIndex = 0;

        private bool shouldCountDown = false;

        private EventHandler<GamerLeftEventArgs> teamManagerLeft;
        private EventHandler<GamerLeftEventArgs> gameLeft;

        private byte[] Sorted;

        private TeamManager teamManager;
        #endregion

        private List<byte[]> mapPackets;
        private MapMetaInfo mapInfo;
        private string filename;
        private GameModes gameMode;
        private MemoryStream mapDataStream;

        /// <summary>
        /// use when hosting a match
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="minerOfDuty"></param>
        /// <param name="mapCompressed"></param>
        /// <param name="filename"></param>
        public CustomLobby(MinerOfDuty minerOfDuty, Stream mapCompressed, MapMetaInfo mapInfo)
        {
            this.minerOfDuty = minerOfDuty;
            this.mapInfo = mapInfo;

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                MinerOfDuty.Session.AllGamers[i].Tag = new GamePlayerStats();
            }

            mapPackets = new List<byte[]>();

            mapDataStream = new MemoryStream();

            mapCompressed.Position = 0;

            //copy to
            int num;
            byte[] buffer = new byte[4096];
            while ((num = mapCompressed.Read(buffer, 0, buffer.Length)) != 0)
            {
                mapDataStream.Write(buffer, 0, num);
            }
            mapDataStream.Position = 0;
            mapCompressed.Position = 0;
            while (true)
            {
                buffer = new byte[1000];
                int end = mapCompressed.Read(buffer, 0, 1000);

                if (end == 0)//no bytes need to be sent
                    break;
                else if (end != 1000)
                {
                    byte[] tmpBuffer = new byte[end];
                    Array.Copy(buffer, tmpBuffer, end);

                    mapPackets.Add(tmpBuffer);
                    //we could break here but I'll just let it run again to make
                    //sure we got everything
                }
                else
                {
                    mapPackets.Add(buffer);
                }
            }
            mapCompressed.Dispose();

            MinerOfDuty.Session.LocalGamers[0].IsReady = true;

            Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

            Sort();

            if (MinerOfDuty.Session.Host.IsLocal)
                countDownTimeInSeconds = 45;

            int a = MinerOfDuty.Session.SessionProperties[1].Value;
            int b = MinerOfDuty.Session.SessionProperties[2].Value;
            int c = MinerOfDuty.Session.SessionProperties[3].Value;
            int d = MinerOfDuty.Session.SessionProperties[4].Value;
            int e = MinerOfDuty.Session.SessionProperties[5].Value;


            title = Encoding.UTF8.GetString(EndianBitConverter.GetBytes(a), 0, 4) +
                Encoding.UTF8.GetString(EndianBitConverter.GetBytes(b), 0, 4) +
                Encoding.UTF8.GetString(EndianBitConverter.GetBytes(c), 0, 4) +
                Encoding.UTF8.GetString(EndianBitConverter.GetBytes(d), 0, 4) +
                Encoding.UTF8.GetString(EndianBitConverter.GetBytes(e), 0, 4);

            title = title.Trim();

            gameMode = (GameModes)MinerOfDuty.Session.SessionProperties[6].Value;

            titlePos = new Vector2(640 - (Resources.TitleFont.MeasureString(title).X / 2f), 130);

            sideMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("edit", "EDIt classes"),
                new MenuElement("view", "View Profile"),
                new MenuElement("clan", "Set Clan Tag"),
                new MenuElement("leave", "Leave Lobby")}, 25.0f, (float)(200 - 90));

            editClasses = new EditClasses(Back);
            profileViewer = new ProfileView(Back);

            if (gameMode == GameModes.CustomFFA)
            {
                teamManager = new CustomFFAManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomSNM)
            {
                teamManager = new CustomSearchNMiner(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomTDM)
            {
                teamManager = new CustomTDMManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomSM)
            {
                teamManager = new CustomSMManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomKB)
            {
                teamManager = new CustomKBManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }

            MinerOfDuty.Session.GamerJoined += session_GamerJoined;
            MinerOfDuty.Session.GamerLeft += session_GamerLeft;
            MinerOfDuty.Session.GameStarted += session_GameStarted;
            MinerOfDuty.Session.GameEnded += session_GameEnded;
            MinerOfDuty.Session.SessionEnded += session_SessionEnded;
            MinerOfDuty.Session.HostChanged += session_HostChanged;

            if (gameMode == GameModes.CustomSM)
            {
                MinerOfDuty.Session.StartGame();
            }
        }

        public CustomLobby(MinerOfDuty minerOfDuty)
        {
            this.minerOfDuty = minerOfDuty;

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                MinerOfDuty.Session.AllGamers[i].Tag = new GamePlayerStats();
            }

            Packet.PacketWriter.Write(Packet.PACKETID_FILENAMEREQUEST);
            MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);

            Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

            Sort();

            int a = MinerOfDuty.Session.SessionProperties[1].Value;
            int b = MinerOfDuty.Session.SessionProperties[2].Value;
            int c = MinerOfDuty.Session.SessionProperties[3].Value;
            int d = MinerOfDuty.Session.SessionProperties[4].Value;
            int e = MinerOfDuty.Session.SessionProperties[5].Value;

            title = Encoding.UTF8.GetString(EndianBitConverter.GetBytes(a), 0, 4) +
                Encoding.UTF8.GetString(EndianBitConverter.GetBytes(b), 0, 4) +
                Encoding.UTF8.GetString(EndianBitConverter.GetBytes(c), 0, 4) +
                Encoding.UTF8.GetString(EndianBitConverter.GetBytes(d), 0, 4) +
                Encoding.UTF8.GetString(EndianBitConverter.GetBytes(e), 0, 4);

            title = title.Trim();

            titlePos = new Vector2(640 - (Resources.TitleFont.MeasureString(title).X / 2f), 130);

            gameMode = (GameModes)MinerOfDuty.Session.SessionProperties[6].Value;

            sideMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("edit", "EDIt classes"),
                new MenuElement("view", "View Profile"),
                new MenuElement("clan", "Set Clan Tag"),
                new MenuElement("leave", "Leave Lobby")}, 25.0f, (float)(200 - 90));

            editClasses = new EditClasses(Back);
            profileViewer = new ProfileView(Back);

            if (gameMode == GameModes.CustomFFA)
            {
                teamManager = new CustomFFAManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomSNM)
            {
                teamManager = new CustomSearchNMiner(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomTDM)
            {
                teamManager = new CustomTDMManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomSM)
            {
                teamManager = new CustomSMManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomKB)
            {
                teamManager = new CustomKBManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }

            MinerOfDuty.Session.GamerJoined += session_GamerJoined;
            MinerOfDuty.Session.GamerLeft += session_GamerLeft;
            MinerOfDuty.Session.GameStarted += session_GameStarted;
            MinerOfDuty.Session.GameEnded += session_GameEnded;
            MinerOfDuty.Session.SessionEnded += session_SessionEnded;
            MinerOfDuty.Session.HostChanged += session_HostChanged;
        }

        void session_HostChanged(object sender, HostChangedEventArgs e)
        {
            if (e.NewHost.IsLocal)
            {
                mapPackets = new List<byte[]>();
                byte[] buffer;
                mapDataStream.Position = 0;

                while (true)
                {
                    buffer = new byte[1000];
                    int end = mapDataStream.Read(buffer, 0, 1000);

                    if (end == 0)//no bytes need to be sent
                        break;
                    else if (end != 1000)
                    {
                        byte[] tmpBuffer = new byte[end];
                        Array.Copy(buffer, tmpBuffer, end);

                        mapPackets.Add(tmpBuffer);
                        //we could break here but I'll just let it run again to make
                        //sure we got everything
                    }
                    else
                    {
                        mapPackets.Add(buffer);
                    }
                }
            }
        }

        private void okayItEnded(int selected)
        {
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private void session_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
        {
            MinerOfDuty.Session.GamerJoined -= session_GamerJoined;
            MinerOfDuty.Session.GamerLeft -= session_GamerLeft;
            MinerOfDuty.Session.GameStarted -= session_GameStarted;
            MinerOfDuty.Session.GameEnded -= session_GameEnded;
            MinerOfDuty.Session.SessionEnded -= session_SessionEnded;
            MinerOfDuty.Session.HostChanged -= session_HostChanged;
            if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                MinerOfDuty.Session.Dispose();

            if (mapDataStream != null)
                mapDataStream.Dispose();
            MinerOfDuty.DrawMenu(false);
            MessageBox.ShowMessageBox(okayItEnded, new string[] { "OKAY" }, 0, new string[] { "HOST LEFT SESSION!", "WHY WOULD (S)HE DO THAT?" });
        }

        private void Choose(IMenuOwner sender, string id)
        {
            if (id == "edit")
            {
                otherScreen = editClasses;
            }
            else if (id == "view")
            {
                otherScreen = profileViewer;
                profileViewer.Show();
            }
            else if (id == "clan")
            {
                try
                {
                    if (Guide.IsVisible == false)
                        Guide.BeginShowKeyboardInput((PlayerIndex)Input.ControllingPlayer, "Set Clan Tag", "Enter your clan tag. Max character limit is 4.", MinerOfDuty.CurrentPlayerProfile.Clan, delegate(IAsyncResult result)
                        {
                            string newClanName = Guide.EndShowKeyboardInput(result);

                            if (newClanName == null)
                                return;
                            if (newClanName.Length > 4)
                                newClanName = newClanName.Substring(0, 4);

                            for (int i = 0; i < newClanName.Length; i++)
                            {
                                if (Resources.Font.Characters.Contains(newClanName[i]) == false)
                                    newClanName = newClanName.Replace(newClanName[i], ' ');
                            }

                            newClanName = NameFilter.FilterName(newClanName);

                            try
                            {
                                if (newClanName.ToLower() == "dev" && SpecialGamer.IsDev(SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer]) == false)
                                    newClanName = "nope";
                            }
                            catch { }

                            MinerOfDuty.CurrentPlayerProfile.Clan = newClanName;
                            MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                            Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);
                        }, null);
                }
                catch (Exception) { }
            }
            else if (id == "leave")
            {
                MessageBox.ShowMessageBox(LeaveLobby, new string[] { "Yes, Leave".ToUpper(), "No, Stay".ToUpper() }, 1, new string[] { "Are you sure you want to".ToUpper(), "leave the lobby?".ToUpper() });
            }
        }

        private void Back(object sender)
        {
            otherScreen = null;
        }

        private int delay = 0;
        public void Update(GameTime gameTime)
        {

            if (Lobby.IsPrivateLobby() && MinerOfDuty.Session.AllGamers.Count < 8 && (shouldCountDown ? countDownTimeInSeconds > 5 : true))
            {
                if (Input.WasButtonPressed(Buttons.X) && useLeftMenu)
                    if (Guide.IsVisible == false)
                    {
                        try
                        {
                            Guide.ShowGameInvite((PlayerIndex)Input.ControllingPlayer, null);
                        }
                        catch (Exception) { MessageBox.ShowMessageBox(delegate(int selected) { Audio.PlaySound(Audio.SOUND_UICLICK); }, new string[] { "OKAY" }, 0, new string[] { "CAN'T SEND INVITE" }); }
                    }
            }

            if (askForTehMapz)
            {
                Packet.PacketWriter.Write(Packet.PACKETID_MAPDATAREQUEST);
                Packet.PacketWriter.Write((short)0);
                MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);
                askForTehMapz = false;
                mapDataStream = new MemoryStream();
            }

            #region networking
            if (MinerOfDuty.Session.LocalGamers[0].IsDataAvailable)
            {
                NetworkGamer sender;
                MinerOfDuty.Session.LocalGamers[0].ReceiveData(Packet.PacketReader, out sender);
                switch (Packet.GetPacketID())
                {
                    case Packet.PACKETID_GETMEATEAMMR:
                        TeamManager.Team teamt = teamManager.CalculateWhichTeamForPlayer(sender.Id);
                        Packet.WriteToAddToTeamManager(MinerOfDuty.Session.LocalGamers[0], sender.Id, teamt);
                        break;
                    case Packet.PACKETID_TOADDTOTEAM:
                        byte newPersonID;
                        TeamManager.Team team;
                        Packet.ReadToAddToTeamManager(out newPersonID, out team);
                        teamManager.AddPlayer(newPersonID, team);
                        if (newPersonID == MinerOfDuty.Session.LocalGamers[0].Id)
                        {
                            haveIBeenAssigned = true;
                        }
                        break;
                    case Packet.PACKETID_MUTE:
                        MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(sender, Packet.PacketReader.ReadBoolean());
                        break;
                    case Packet.PACKETID_COUNTDOWN:
                        int time = Packet.PacketReader.ReadInt16();
                        if (!MinerOfDuty.Session.LocalGamers[0].IsHost)
                        {
                            countDownTimeInSeconds = time;
                            if (time <= 4)
                                shouldCountDown = true;
                        }
                        break;
                    case Packet.PACKETID_PLAYERSCORE:
                        (MinerOfDuty.Session.FindGamerById(Packet.PacketReader.ReadByte()).Tag as GamePlayerStats).Score = Packet.PacketReader.ReadInt16();
                        break;
                    case Packet.PACKETID_PLAYERLEVEL:
                        if (MinerOfDuty.Session.FindGamerById(sender.Id) != null)
                        {
                            Packet.ReadPlayerLevel(MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats);
                        }
                        break;
                    case Packet.PACKETID_FILENAMEREQUEST:
                        if (MinerOfDuty.Session.IsHost == false)//should never be called
                            break;

                        //write file name and date
                        Packet.PacketWriter.Write(Packet.PACKETID_FILENAMERESPONSE);
                        Packet.PacketWriter.Write(mapInfo.FileName);
                        Packet.PacketWriter.Write(mapInfo.TimeEdited.Ticks);
                        MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, sender);

                        break;
                    case Packet.PACKETID_FILENAMERESPONSE:

                        string filename = Packet.PacketReader.ReadString();
                        mapInfo = new MapMetaInfo("doesntmatter", "doesntmatter", Packet.PacketReader.ReadInt64(), filename, GameModes.CustomMap);
                        //now we need to scan through files and check if we got it
                        Thread t = new Thread(poop => MinerOfDuty.mainMenu.mapSearchPlayList.FindMaps(FilesFound));
                        t.IsBackground = true;
                        t.Start();

                        break;
                    case Packet.PACKETID_MAPDATAREQUEST:
                        if (MinerOfDuty.Session.IsHost == false)//should never be called
                            break;

                        short packetWanted = Packet.PacketReader.ReadInt16();

                        Packet.PacketWriter.Write(Packet.PACKETID_MAPDATA);
                        Packet.PacketWriter.Write(packetWanted);//this is the packet u requested
                        Packet.PacketWriter.Write((packetWanted + 1) < mapPackets.Count);//if the packet u wanted equals 
                        //the amount of packets ur done
                        Packet.PacketWriter.Write((short)mapPackets[packetWanted].Length);
                        Packet.PacketWriter.Write(mapPackets[packetWanted]);

                        MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, sender);
                        break;
                    case Packet.PACKETID_MAPDATA:

                        packetWanted = Packet.PacketReader.ReadInt16();
                        bool doIWantMore = Packet.PacketReader.ReadBoolean();

                        short bytes = Packet.PacketReader.ReadInt16();
                        mapDataStream.Write(Packet.PacketReader.ReadBytes(bytes), 0, bytes);

                        if (doIWantMore)
                        {
                            Packet.PacketWriter.Write(Packet.PACKETID_MAPDATAREQUEST);
                            Packet.PacketWriter.Write(packetWanted + 1);
                            MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);
                        }
                        else
                        {
                            MinerOfDuty.Session.LocalGamers[0].IsReady = true;
                            MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Custom Maps", this.filename, delegate(Stream s)
                            {
                                try
                                {
                                    mapDataStream.Position = 0;

                                    int num;
                                    byte[] buffer = new byte[4096];
                                    while ((num = mapDataStream.Read(buffer, 0, buffer.Length)) != 0)
                                    {
                                        s.Write(buffer, 0, num);
                                    }


                                    mapDataStream.Position = 0;
                                }
                                catch (Exception e)
                                {
                                    lock (MinerOfDuty.ExceptionsLock)
                                        MinerOfDuty.Exceptions.Enqueue(e);
                                }
                            });
                        }

                        break;
                }
            }

            if (otherScreen == null)
            {
                if (MinerOfDuty.Session.LocalGamers[0].IsHost && MinerOfDuty.Session.IsEveryoneReady && shouldCountDown == false)
                {
                    if (Input.WasButtonPressed(Buttons.Y))
                    {
                        shouldCountDown = true;
                        countDownTimeInSeconds = 5;
                    }
                }
            }
            #endregion

            #region countdown
            if (shouldCountDown)
            {
                if (MinerOfDuty.Session.Host.IsLocal)
                {
                    countDownMilli += gameTime.ElapsedGameTime.Milliseconds;
                    if (countDownMilli >= 1000)
                    {
                        countDownTimeInSeconds--;
                        countDownMilli = 0;
                        if (countDownTimeInSeconds > 0)
                        {
                            Packet.PacketWriter.Write(Packet.PACKETID_COUNTDOWN);
                            Packet.PacketWriter.Write((short)countDownTimeInSeconds);
                            MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable);
                        }
                    }

                    if (countDownTimeInSeconds <= 0)
                    {
                        if (MinerOfDuty.Session.SessionState == NetworkSessionState.Lobby)
                        {
                            MinerOfDuty.Session.StartGame();
                            MinerOfDuty.Session.AllowHostMigration = true;
                        }
                    }
                }
            }
            #endregion

            #region choose
            if (useLeftMenu)
            {
                if (otherScreen != null)
                    otherScreen.Update((short)gameTime.ElapsedGameTime.Milliseconds);
                else
                    sideMenu.Update((short)gameTime.ElapsedGameTime.Milliseconds);
            }
            else
            {
                if (Input.WasButtonPressed(Buttons.A))
                {
                    //mute
                    if (MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]).Id != MinerOfDuty.Session.LocalGamers[0].Id)
                    {
                        (MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]).Tag as GamePlayerStats).IsMuted = !(MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]).Tag as GamePlayerStats).IsMuted;
                        Packet.WriteMutePacket(MinerOfDuty.Session.LocalGamers[0], MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]), (MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]).Tag as GamePlayerStats).IsMuted);
                        MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]), !(MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]).Tag as GamePlayerStats).IsMuted);
                    }
                    Audio.PlaySound(Audio.SOUND_UICLICK);
                }
                else if (Input.WasButtonPressed(Buttons.X))
                {
                    if ((MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]).Tag as GamePlayerStats).pp != null)
                    {
                        useLeftMenu = true;
                        otherScreen = new ProfileView(delegate(object sender) { otherScreen = null; useLeftMenu = false; }, MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]).Gamertag);
                        (otherScreen as ProfileView).Show((MinerOfDuty.Session.FindGamerById(Sorted[selectedIndex]).Tag as GamePlayerStats).pp);
                    }
                    Audio.PlaySound(Audio.SOUND_UICLICK);
                }
            }
            #endregion

            if (delay >= 0)
                delay -= gameTime.ElapsedGameTime.Milliseconds;

            if (otherScreen == null)
                #region nav
                if (delay < 0)
                {
                    if (Input.IsThumbstickOrDPad(Input.Direction.Left))
                    {
                        if (useLeftMenu == false)
                        {
                            useLeftMenu = true;
                            delay = 180;
                            selectedIndex = 0;
                        }
                    }
                    else if (Input.IsThumbstickOrDPad(Input.Direction.Right))
                    {
                        if (useLeftMenu == true)
                        {
                            useLeftMenu = false;
                            delay = 180;
                            selectedIndex = 0;
                        }
                    }

                    if (Input.IsThumbstickOrDPad(Input.Direction.Down) && useLeftMenu == false)
                    {
                        if (++selectedIndex >= MinerOfDuty.Session.AllGamers.Count)
                        {
                            selectedIndex = MinerOfDuty.Session.AllGamers.Count - 1;
                        }
                        else
                            delay = 180;
                    }

                    if (Input.IsThumbstickOrDPad(Input.Direction.Up))
                    {
                        if (--selectedIndex < 0)
                        {
                            selectedIndex = 0;
                        }
                        else
                            delay = 180;
                    }
                }
                #endregion

            MinerOfDuty.Session.Update();
        }


        private void FilesFound(object sender, EventArgs e)
        {
            MapMetaInfo[] maps = (sender as MapSearchPlayList).maps;

            for (int i = 0; i < maps.Length; i++)
            {
                if (maps[i].FileName == mapInfo.FileName)
                {
                    if (maps[i].TimeEdited == mapInfo.TimeEdited)
                    {
                        //okay we already have the map lets load it in memory
                        MinerOfDuty.SaveDevice.Load("Miner Of Duty Custom Maps", maps[i].FileName,
                            delegate(Stream s)
                            {
                                try
                                {
                                    mapDataStream = new MemoryStream();
                                    int num;
                                    byte[] buffer = new byte[4096];
                                    while ((num = s.Read(buffer, 0, buffer.Length)) != 0)
                                    {
                                        mapDataStream.Write(buffer, 0, num);
                                    }
                                }
                                catch (Exception ee)
                                {
                                    lock (MinerOfDuty.ExceptionsLock)
                                        MinerOfDuty.Exceptions.Enqueue(ee);
                                }
                            });
                        MinerOfDuty.Session.LocalGamers[0].IsReady = true;
                        return;
                    }
                }
            }

            filename = mapInfo.FileName;
            askForTehMapz = true;
        }

        private bool askForTehMapz = false;//i dunno if crossing threads will be bad

        public void Draw(SpriteBatch sb)
        {
            if (otherScreen != null)
            {
                if (otherScreen == editClasses)
                    sb.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                otherScreen.Draw(sb);
            }
            else
            {
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);

                if (Lobby.IsPrivateLobby() && useLeftMenu && MinerOfDuty.Session.AllGamers.Count < 8 && (shouldCountDown ? countDownTimeInSeconds > 5 : true))
                {
                    sb.DrawString(Resources.NameFont, "PRESS X TO", new Vector2(300 - (Resources.NameFont.MeasureString("PRESS X TO").X / 2f), 140 - (Resources.NameFont.LineSpacing / 2)), Color.White);
                    sb.DrawString(Resources.NameFont, "SEND AN INVITE", new Vector2(300 - (Resources.NameFont.MeasureString("SEND AN INVITE").X / 2f), 140 + (Resources.NameFont.LineSpacing / 2)), Color.White);
                }

                sb.DrawString(Resources.TitleFont, title, titlePos, Color.White);

                sideMenu.Draw(sb);

                if (useLeftMenu == false) //////////whites out the side menu
                {
                    if (sideMenu.GetSelectedItemID() == "edit")
                        sb.DrawString(Resources.Font, "EDIT CLASSES", sideMenu["edit"].Position, Color.White);
                    else if (sideMenu.GetSelectedItemID() == "view")
                        sb.DrawString(Resources.Font, "VIEW PROFILE", sideMenu["view"].Position, Color.White);
                    else if (sideMenu.GetSelectedItemID() == "clan")
                        sb.DrawString(Resources.Font, "SET CLAN TAG", sideMenu["clan"].Position, Color.White);
                    else if (sideMenu.GetSelectedItemID() == "leave")
                        sb.DrawString(Resources.Font, "LEAVE LOBBY", sideMenu["leave"].Position, Color.White);
                }

                sb.DrawString(Resources.Font, "PLAYERS", new Vector2(425, 200), Color.White);
                sb.DrawString(Resources.Font, "SCORE", new Vector2(1055 - Resources.Font.MeasureString("SCORE").X, 200), Color.White);

                Vector2 pos = new Vector2(450, 240);

                NetworkGamer gamer;
                for (int i = 0; i < Sorted.Length; i++)
                {
                    gamer = (MinerOfDuty.Session.FindGamerById(Sorted[i]));
                    if (useLeftMenu == false && selectedIndex == i)
                    {
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Level + " " +
                            ((gamer.Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (gamer.Tag as GamePlayerStats).ClanTag + "]" : "")
                            + gamer.Gamertag, pos, Color.Green);
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Score.ToString(),
                            pos + new Vector2(610 - Resources.NameFont.MeasureString((gamer.Tag as GamePlayerStats).Score.ToString()).X, 0), Color.Green);
                        pos.Y += Resources.Font.LineSpacing;
                    }
                    else
                    {
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Level + " " +
                            ((gamer.Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (gamer.Tag as GamePlayerStats).ClanTag + "]" : "")
                             + gamer.Gamertag, pos, Color.White);
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Score.ToString(),
                            pos + new Vector2(610 - Resources.NameFont.MeasureString((gamer.Tag as GamePlayerStats).Score.ToString()).X, 0), Color.White);
                        pos.Y += Resources.Font.LineSpacing;
                    }

                    if ((gamer.Tag as GamePlayerStats).IsMuted || MinerOfDuty.Session.FindGamerById(Sorted[i]).IsMutedByLocalUser)
                        sb.Draw(Resources.Muted, pos - new Vector2(45, 45), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(Sorted[i]).IsTalking)
                        sb.Draw(Resources.IsTalking, pos - new Vector2(45, 45), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(Sorted[i]).HasVoice && !MinerOfDuty.Session.FindGamerById(Sorted[i]).IsTalking)
                        sb.Draw(Resources.CanTalk, pos - new Vector2(45, 45), Color.White);
                }



                if (shouldCountDown)
                    sb.DrawString(Resources.NameFont, "0:" + (countDownTimeInSeconds < 10 ? "0" + countDownTimeInSeconds : countDownTimeInSeconds.ToString()), new Vector2(1000 - (Resources.NameFont.MeasureString((countDownTimeInSeconds < 10 ? "0" + countDownTimeInSeconds : countDownTimeInSeconds.ToString())).X / 2f), 140), Color.White);
                else if (MinerOfDuty.Session.IsEveryoneReady)
                {
                    sb.DrawString(Resources.NameFont, "WAITING FOR", new Vector2(1000 - (Resources.NameFont.MeasureString("WAITING FOR").X / 2f), 140 - (Resources.NameFont.LineSpacing / 2)), Color.White);
                    sb.DrawString(Resources.NameFont, "HOST TO START", new Vector2(1000 - (Resources.NameFont.MeasureString("HOST TO START").X / 2f), 140 + (Resources.NameFont.LineSpacing / 2)), Color.White);
                }
                else
                {
                    sb.DrawString(Resources.NameFont, "WAITING FOR", new Vector2(1000 - (Resources.NameFont.MeasureString("WAITING FOR").X / 2f), 140 - (Resources.NameFont.LineSpacing / 2)), Color.White);
                    sb.DrawString(Resources.NameFont, "MAP TO DOWNLOAD", new Vector2(1000 - (Resources.NameFont.MeasureString("HOST TO START").X / 2f), 140 + (Resources.NameFont.LineSpacing / 2)), Color.White);
                }

                if (MinerOfDuty.Session.IsHost && MinerOfDuty.Session.IsEveryoneReady && shouldCountDown == false)
                {
                    sb.DrawString(Resources.DescriptionFont, "PRESS Y TO START", new Vector2(sideMenu["edit"].Position.X, 475), Color.White);
                }

                if (useLeftMenu == false)
                {
                    sb.DrawString(Resources.DescriptionFont, "PRESS X TO VIEW", new Vector2(sideMenu["edit"].Position.X, 480 + Resources.DescriptionFont.LineSpacing + Resources.DescriptionFont.LineSpacing), Color.White);
                    sb.DrawString(Resources.DescriptionFont, "        PROFILE", new Vector2(sideMenu["edit"].Position.X, 480 + 3 * Resources.DescriptionFont.LineSpacing), Color.White);
                }
            }
        }

        private void Sort()
        {
            List<byte> open, closed;

            open = new List<byte>();
            closed = new List<byte>();

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                open.Add(MinerOfDuty.Session.AllGamers[i].Id);
            }

            while (open.Count > 0)
            {
                int indexHigh = 0;
                for (int i = 0; i < open.Count; i++)
                {
                    if ((MinerOfDuty.Session.FindGamerById(open[indexHigh]).Tag as GamePlayerStats).Score < (MinerOfDuty.Session.FindGamerById(open[i]).Tag as GamePlayerStats).Score)
                    {
                        indexHigh = i;
                    }
                }
                closed.Add(open[indexHigh]);
                open.RemoveAt(indexHigh);
            }

            Sorted = closed.ToArray();
        }

        public void LeaveLobby(int selected)
        {
            if (selected == 0)
            {
                MinerOfDuty.Session.GamerJoined -= new EventHandler<GamerJoinedEventArgs>(session_GamerJoined);
                MinerOfDuty.Session.GamerLeft -= new EventHandler<GamerLeftEventArgs>(session_GamerLeft);
                MinerOfDuty.Session.GameStarted -= new EventHandler<GameStartedEventArgs>(session_GameStarted);
                MinerOfDuty.Session.GameEnded -= new EventHandler<GameEndedEventArgs>(session_GameEnded);
                MinerOfDuty.Session.SessionEnded -= session_SessionEnded;
                MinerOfDuty.Session.HostChanged -= session_HostChanged;
                if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                    MinerOfDuty.Session.Dispose();
                if (mapDataStream != null)
                    mapDataStream.Dispose();
                MinerOfDuty.DrawMenu(false);
            }
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        private void session_GamerLeft(object sender, GamerLeftEventArgs e)
        {
            if (teamManagerLeft != null)
                teamManagerLeft.Invoke(sender, e);
            if (gameLeft != null)
                gameLeft.Invoke(sender, e);

            Sort();
        }
        private bool haveIBeenAssigned = false;
        private void session_GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            if (e.Gamer.IsLocal)
                Packet.WriteGetMeATeam(MinerOfDuty.Session.LocalGamers[0]);
            else if (haveIBeenAssigned)
                Packet.WriteToAddToTeamManager(MinerOfDuty.Session.LocalGamers[0], MinerOfDuty.Session.LocalGamers[0].Id, teamManager.WhatTeam(MinerOfDuty.Session.LocalGamers[0].Id), e.Gamer);


            e.Gamer.Tag = new GamePlayerStats();

            if (MinerOfDuty.Session.Host.IsLocal && e.Gamer.IsLocal == false)
            {
                for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                {
                    if (MinerOfDuty.Session.AllGamers[i].Id != e.Gamer.Id)
                        Packet.WritePlayerScore(MinerOfDuty.Session.LocalGamers[0], e.Gamer, (short)(MinerOfDuty.Session.AllGamers[i].Tag as GamePlayerStats).Score, MinerOfDuty.Session.AllGamers[i].Id);
                }
            }



            if (MinerOfDuty.Session.Host.IsLocal && e.Gamer.IsLocal == false)
            {
                Packet.PacketWriter.Write(Packet.PACKETID_COUNTDOWN);
                Packet.PacketWriter.Write((short)countDownTimeInSeconds);
                MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, e.Gamer);
            }

            if (e.Gamer.IsLocal == false)
                Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

            Sort();

            shouldCountDown = false;
        }

        private void session_GameStarted(object sender, GameStartedEventArgs e)
        {
            teamManager.MuteTeam();
            MemoryStream gameStream = new MemoryStream();

            gameStream.Position = 0;
            mapDataStream.Position = 0;
            int num;
            byte[] buffer = new byte[4096];
            while ((num = mapDataStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                gameStream.Write(buffer, 0, num);
            }
            gameStream.Position = 0;
            mapDataStream.Position = 0;

            if (gameMode == GameModes.CustomTDM)
            {
                MinerOfDuty.game = new CustomTDM(minerOfDuty, teamManager as CustomTDMManager, minerOfDuty.GraphicsDevice, out gameLeft,
                    gameStream);
            }
            else if (gameMode == GameModes.CustomSM)
            {
                MinerOfDuty.game = new CustomSwarmGame(minerOfDuty, teamManager as CustomSMManager, minerOfDuty.GraphicsDevice, out gameLeft,
                  gameStream);
            }
            else if (gameMode == GameModes.CustomSNM)
            {
                MinerOfDuty.game = new CustomSNM(minerOfDuty, teamManager as CustomSearchNMiner, minerOfDuty.GraphicsDevice, out gameLeft,
                   gameStream);
            }
            else if (gameMode == GameModes.CustomFFA)
            {
                MinerOfDuty.game = new CustomFFA(minerOfDuty, teamManager as CustomFFAManager, minerOfDuty.GraphicsDevice, out gameLeft,
                   gameStream);
            }
            else if (gameMode == GameModes.CustomKB)
            {
                MinerOfDuty.game = new CustomKB(minerOfDuty, teamManager as CustomKBManager, minerOfDuty.GraphicsDevice, out gameLeft,
                   gameStream);
            }
            MinerOfDuty.DrawGame();


            if (MessageBox.IsMessageBeingShown)
            {
                MessageBox.CloseMessageBox();
            }

            Audio.SetFading(Audio.Fading.Out);
        }

        private void session_GameEnded(object sender, GameEndedEventArgs e)
        {
            MinerOfDuty.CurrentPlayerProfile.Save();
            MinerOfDuty.DrawLobby();

            countDownTimeInSeconds = 45;

            Sort();

            shouldCountDown = false;

            if (MinerOfDuty.CurrentPlayerProfile.MessagesToRead.Count > 0)
                MessageBox.ShowMessageBox(UnlockMsg, new string[] { "OK" }, 0, MinerOfDuty.CurrentPlayerProfile.MessagesToRead.Dequeue());

            Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

            Audio.SetFading(Audio.Fading.In);
            Audio.SetPlaylist(new Playlist(new byte[] { Audio.SONG_STORMFRONT }));
            Audio.PlaySong();

            if (MinerOfDuty.Session.IsHost)
            {
                MinerOfDuty.Session.AllowHostMigration = false;
                MinerOfDuty.Session.LocalGamers[0].IsReady = true;
            }
            else
            {
                Packet.PacketWriter.Write(Packet.PACKETID_FILENAMEREQUEST);
                MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);
            }

            if (gameMode == GameModes.CustomFFA)
            {
                teamManager = new CustomFFAManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomSNM)
            {
                teamManager = new CustomSearchNMiner(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomTDM)
            {
                teamManager = new CustomTDMManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.CustomSM)
            {
                teamManager = new CustomSMManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }

            haveIBeenAssigned = false;
            Packet.WriteGetMeATeam(MinerOfDuty.Session.LocalGamers[0]);
        }

        private void UnlockMsg(int i)
        {
            if (MinerOfDuty.CurrentPlayerProfile.MessagesToRead.Count > 0)
                MessageBox.ShowMessageBox(UnlockMsg, new string[] { "OK" }, 0, MinerOfDuty.CurrentPlayerProfile.MessagesToRead.Dequeue());
            Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        public void Render(GraphicsDevice gd)
        {
        }


        public void Activated()
        {

        }

        public void Deactivated()
        {

        }
    }
}

