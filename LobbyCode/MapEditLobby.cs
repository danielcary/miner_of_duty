using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using Miner_Of_Duty.Menus;
using Miner_Of_Duty.Game.Networking;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;
using System.Threading;
using Miner_Of_Duty.Game.Editor;

namespace Miner_Of_Duty.LobbyCode
{
    public enum MapEditCreateType
    {
        Random, Flat, FlatWithCaves, Island, Edit
    }

   

    public class MapEditLobby : ILobby
    {
        public static MapEditCreateType Convert(WorldEditor.WorldGeneration gen)
        {
            if (gen == WorldEditor.WorldGeneration.Random)
                return MapEditCreateType.Random;
            else if (gen == WorldEditor.WorldGeneration.Flat)
                return MapEditCreateType.Flat;
            else if (gen == WorldEditor.WorldGeneration.FlatWithCaves)
                return MapEditCreateType.FlatWithCaves;
            else if (gen == WorldEditor.WorldGeneration.Island)
                return MapEditCreateType.Island;
            else
                return MapEditCreateType.Edit;
        }
        internal interface IMapEditSubLobby
        {
            void Dispose();
            void Update(GameTime gameTime);
            void Draw(SpriteBatch sb);
        }

        private MapEditCreateType type;
        private IMapEditSubLobby Lobby;

        public MapEditLobby(MapEditCreateType createType)
        {
            type = createType;
            if (createType == MapEditCreateType.Edit)
                Lobby = new MapEditMapLobby(LeaveLobby);
            else
                Lobby = new MapCreateLobby(createType, LeaveLobby, null, null, null, null, null, null, null, null);
        }

        public MapEditLobby(MapEditCreateType createType, string password, int size, GameModes? gameMode, string teamAName, string teamBName, bool trees, bool weapons, bool editing)
        {
            type = createType;
            if (createType == MapEditCreateType.Edit)
                Lobby = new MapEditMapLobby(LeaveLobby);
            else
                Lobby = new MapCreateLobby(createType, LeaveLobby, password, size, teamAName, teamBName, gameMode, trees, weapons, editing);
        }

        /// <summary>
        /// Use this for hosting an edited map
        /// </summary>
        public MapEditLobby(Stream mapCompressed, MapMetaInfo mapInfo)
        {
            type = MapEditCreateType.Edit;
            Lobby = new MapEditMapLobby(mapCompressed, mapInfo, LeaveLobby);
        }


        public void LeaveLobby(int selected)
        {
            if (selected == 0)
            {
                Lobby.Dispose();
                if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                    MinerOfDuty.Session.Dispose();
                MinerOfDuty.DrawMenu();
            }
            
            if (selected == -1)
            {
                Lobby.Dispose();
            }
            else
                Audio.PlaySound(Audio.SOUND_UICLICK);
        }

        public void Update(GameTime gameTime)
        {
            Lobby.Update(gameTime);
        }

        public void Draw(SpriteBatch sb)
        {
            Lobby.Draw(sb);
        }

        public void Activated() { }
        public void Deactivated() { }
        public void Render(GraphicsDevice gd) { }

        internal class MapEditMapLobby : IMapEditSubLobby
        {
            private Menu sideMenu;
            private int countDownTimeInSeconds = 10;
            private int countDownMilli = 0;
            private bool useLeftMenu = true;
            private int selectedIndex = 0;
            private bool shouldCountDown = false;
            private string title;
            private Vector2 titlePos;
            private List<byte[]> mapPackets;
            private MapMetaInfo mapInfo;
            private string filename;
            private MemoryStream mapDataStream;

            public MapEditMapLobby(Stream compressedMap, MapMetaInfo mapInfo, MessageBox.MessageBoxResult LeaveLobby)
            {
                for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                {
                    MinerOfDuty.Session.AllGamers[i].Tag = new GamePlayerStats();
                }


                this.mapInfo = mapInfo;

                #region make packets and copy compressed map stream
                mapPackets = new List<byte[]>();

                mapDataStream = new MemoryStream();

                compressedMap.Position = 0;

                //copy to
                int num;
                byte[] buffer = new byte[4096];
                while ((num = compressedMap.Read(buffer, 0, buffer.Length)) != 0)
                {
                    mapDataStream.Write(buffer, 0, num);
                }
                mapDataStream.Position = 0;
                compressedMap.Position = 0;
                while (true)
                {
                    buffer = new byte[1000];
                    int end = compressedMap.Read(buffer, 0, 1000);

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
                compressedMap.Dispose();
                #endregion

                MinerOfDuty.Session.LocalGamers[0].IsReady = true;

                Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

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

                sideMenu = new Menu(delegate(IMenuOwner sender, string id)
                    {
                        if (id == "clan")
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
                    }, delegate(object sender){}, new MenuElement[]{
                    new MenuElement("clan", "Set Clan Tag"),
                    new MenuElement("leave", "Leave Lobby")}, 25.0f, (float)(200 - 90));

                MinerOfDuty.Session.GamerJoined += Session_GamerJoined;
                MinerOfDuty.Session.GameStarted += Session_GameStarted;
                MinerOfDuty.Session.SessionEnded += Session_SessionEnded;
            }

            private void Session_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
            {
                MinerOfDuty.DrawMenu();
            }

            public MapEditMapLobby(MessageBox.MessageBoxResult LeaveLobby)
            {
                for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                {
                    MinerOfDuty.Session.AllGamers[i].Tag = new GamePlayerStats();
                }


                Packet.PacketWriter.Write(Packet.PACKETID_FILENAMEREQUEST);
                MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, MinerOfDuty.Session.Host);
                Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

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

                sideMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "clan")
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
                }, delegate(object sender) { }, new MenuElement[]{
                    new MenuElement("clan", "Set Clan Tag"),
                    new MenuElement("leave", "Leave Lobby")}, 25.0f, (float)(200 - 90));

                MinerOfDuty.Session.GamerJoined += Session_GamerJoined;
                MinerOfDuty.Session.GameStarted += Session_GameStarted;
                MinerOfDuty.Session.SessionEnded += Session_SessionEnded;
            }

            public void Dispose()
            {
                MinerOfDuty.Session.GamerJoined -= Session_GamerJoined;
                MinerOfDuty.Session.GameStarted -= Session_GameStarted;
                MinerOfDuty.Session.SessionEnded -= Session_SessionEnded;
                if(mapDataStream != null)
                    mapDataStream.Dispose();
                mapDataStream = null;
            }

            private int delay = 0;
            public void Update(GameTime gameTime)
            {
                if (MinerOfDuty.Session.AllGamers.Count < 8 && (shouldCountDown ? countDownTimeInSeconds > 5 : true))
                {
                    if (Input.WasButtonPressed(Buttons.X))
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
                    try
                    {
                        if (mapDataStream != null)
                            mapDataStream.Dispose();
                    }
                    catch (Exception) { }//shouldnt crash but idk about dispose
                    mapDataStream = new MemoryStream();
                }


                #region networkingmy
                if (MinerOfDuty.Session.LocalGamers[0].IsDataAvailable)
                {
                    NetworkGamer sender;
                    MinerOfDuty.Session.LocalGamers[0].ReceiveData(Packet.PacketReader, out sender);
                    switch (Packet.GetPacketID())
                    {
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

                if (MinerOfDuty.Session.LocalGamers[0].IsHost && MinerOfDuty.Session.IsEveryoneReady && shouldCountDown == false)
                {
                    if (Input.WasButtonPressed(Buttons.Y))
                    {
                        shouldCountDown = true;
                        countDownTimeInSeconds = 5;
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
                            }
                        }
                    }
                }
                #endregion

                #region choose
                if (!useLeftMenu)
                {
                    if (Input.WasButtonPressed(Buttons.A))
                    {
                        //mute
                        if (MinerOfDuty.Session.AllGamers[selectedIndex].Id != MinerOfDuty.Session.LocalGamers[0].Id)
                        {
                            (MinerOfDuty.Session.AllGamers[selectedIndex].Tag as GamePlayerStats).IsMuted = !(MinerOfDuty.Session.AllGamers[selectedIndex].Tag as GamePlayerStats).IsMuted;
                            Packet.WriteMutePacket(MinerOfDuty.Session.LocalGamers[0], MinerOfDuty.Session.AllGamers[selectedIndex], (MinerOfDuty.Session.AllGamers[selectedIndex].Tag as GamePlayerStats).IsMuted);
                            MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(MinerOfDuty.Session.AllGamers[selectedIndex], !(MinerOfDuty.Session.AllGamers[selectedIndex].Tag as GamePlayerStats).IsMuted);
                        }
                        Audio.PlaySound(Audio.SOUND_UICLICK);
                    }
                }
                #endregion

                if (delay >= 0)
                    delay -= gameTime.ElapsedGameTime.Milliseconds;

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
                if (useLeftMenu)
                    sideMenu.Update((short)gameTime.ElapsedGameTime.Milliseconds);

                if (MinerOfDuty.Session != null & MinerOfDuty.Session.IsDisposed == false)
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
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);

                if (MinerOfDuty.Session.AllGamers.Count < 8 && (shouldCountDown ? countDownTimeInSeconds > 5 : true))
                {
                    sb.DrawString(Resources.NameFont, "PRESS X TO", new Vector2(300 - (Resources.NameFont.MeasureString("PRESS X TO").X / 2f), 140 - (Resources.NameFont.LineSpacing / 2)), Color.White);
                    sb.DrawString(Resources.NameFont, "SEND AN INVITE", new Vector2(300 - (Resources.NameFont.MeasureString("SEND AN INVITE").X / 2f), 140 + (Resources.NameFont.LineSpacing / 2)), Color.White);
                }

                sb.DrawString(Resources.TitleFont, title, titlePos, Color.White);

                sideMenu.Draw(sb);

                if (useLeftMenu == false) //////////whites out the side menu
                {
                    if (sideMenu.GetSelectedItemID() == "clan")
                        sb.DrawString(Resources.Font, "SET CLAN TAG", sideMenu["clan"].Position, Color.White);
                    else if (sideMenu.GetSelectedItemID() == "leave")
                        sb.DrawString(Resources.Font, "LEAVE LOBBY", sideMenu["leave"].Position, Color.White);
                }

                sb.DrawString(Resources.Font, "PLAYERS", new Vector2(425, 200), Color.White);

                Vector2 pos = new Vector2(450, 240);

                NetworkGamer gamer;
                for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                {
                    gamer = MinerOfDuty.Session.AllGamers[i];
                    if (useLeftMenu == false && selectedIndex == i)
                    {
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Level + " " +
                            ((gamer.Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (gamer.Tag as GamePlayerStats).ClanTag + "]" : "")
                            + gamer.Gamertag, pos, Color.Green);
                        pos.Y += Resources.Font.LineSpacing;
                    }
                    else
                    {
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Level + " " +
                            ((gamer.Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (gamer.Tag as GamePlayerStats).ClanTag + "]" : "")
                             + gamer.Gamertag, pos, Color.White);
                        pos.Y += Resources.Font.LineSpacing;
                    }

                    if ((gamer.Tag as GamePlayerStats).IsMuted || gamer.IsMutedByLocalUser)
                        sb.Draw(Resources.Muted, pos - new Vector2(45, 45), Color.White);
                    else if (gamer.IsTalking)
                        sb.Draw(Resources.IsTalking, pos - new Vector2(45, 45), Color.White);
                    else if (gamer.HasVoice && !gamer.IsTalking)
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
                    sb.DrawString(Resources.NameFont, "PRESS Y TO START", new Vector2(sideMenu["clan"].Position.X, 500), Color.White);
                }
            }

           

            private void Session_GameStarted(object sender, GameStartedEventArgs e)
            {
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

                if(MinerOfDuty.Session.LocalGamers[0].IsHost)
                    MinerOfDuty.Session.AllowHostMigration = true;

                MinerOfDuty.editor = new WorldEditor();
                
                Thread t = new Thread(poop => MinerOfDuty.editor.Initialize(MinerOfDuty.Self.GraphicsDevice, gameStream));
                t.Name = "gen";
                t.IsBackground = true;
                t.Start();

                MinerOfDuty.DrawEdit();

                if (MessageBox.IsMessageBeingShown)
                {
                    MessageBox.CloseMessageBox();
                }

                Audio.SetFading(Audio.Fading.Out);
            }

            private void Session_GamerJoined(object sender, GamerJoinedEventArgs e)
            {
                e.Gamer.Tag = new GamePlayerStats();

                if (MinerOfDuty.Session.Host.IsLocal && e.Gamer.IsLocal == false)
                {
                    Packet.PacketWriter.Write(Packet.PACKETID_COUNTDOWN);
                    Packet.PacketWriter.Write((short)countDownTimeInSeconds);
                    MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, e.Gamer);
                }

                if (e.Gamer.IsLocal == false)
                    Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

                shouldCountDown = false;
            }
        }

        internal class MapCreateLobby : IMapEditSubLobby
        {
            private Menu sideMenu;
            private int countDownTimeInSeconds = 10;
            private int countDownMilli = 0;
            private bool useLeftMenu = true;
            private int selectedIndex = 0;
            private bool shouldCountDown = false;
            private string title;
            private Vector2 titlePos;
            private MapEditCreateType type;

            private string password;
            private int? size; 
            private GameModes? gameMode;
            private string teamAName;
            private string teamBName;
            private bool? trees, weapons, editing;

            public MapCreateLobby(MapEditCreateType t, MessageBox.MessageBoxResult LeaveLobby, string password, int? size, string teamAName, string teamBName, GameModes? gameMode, 
                bool? trees, bool? weapons, bool? editing)
            {
                this.password = password;
                this.size = size;
                this.gameMode = gameMode;
                this.teamAName = teamAName;
                this.teamBName = teamBName;
                this.trees = trees;
                this.weapons = weapons;
                this.editing = editing;

                type = t;

                for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                {
                    MinerOfDuty.Session.AllGamers[i].Tag = new GamePlayerStats();
                }

                Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

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

                sideMenu = new Menu(delegate(IMenuOwner sender, string id)
                {
                    if (id == "clan")
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
                }, delegate(object sender) { }, new MenuElement[]{
                    new MenuElement("clan", "Set Clan Tag"),
                    new MenuElement("leave", "Leave Lobby")}, 25.0f, (float)(200 - 90));

                MinerOfDuty.Session.GamerJoined += Session_GamerJoined;
                MinerOfDuty.Session.GameStarted += Session_GameStarted;
                MinerOfDuty.Session.SessionEnded += Session_SessionEnded;
            }

            void Session_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
            {
                MinerOfDuty.DrawMenu(false);
            }

            public void Dispose()
            {
                MinerOfDuty.Session.GamerJoined -= Session_GamerJoined;
                MinerOfDuty.Session.GameStarted -= Session_GameStarted;
                MinerOfDuty.Session.SessionEnded -= Session_SessionEnded;
            }

            private int delay = 0;
            public void Update(GameTime gameTime)
            {
                if (MinerOfDuty.Session.AllGamers.Count < 8 && (shouldCountDown ? countDownTimeInSeconds > 5 : true))
                {
                    if (Input.WasButtonPressed(Buttons.X))
                        if (Guide.IsVisible == false)
                        {
                            try
                            {
                                Guide.ShowGameInvite((PlayerIndex)Input.ControllingPlayer, null);
                            }
                            catch (Exception) { MessageBox.ShowMessageBox(delegate(int selected) { Audio.PlaySound(Audio.SOUND_UICLICK); }, new string[] { "OKAY" }, 0, new string[] { "CAN'T SEND INVITE" }); }
                        }
                }
                #region networking
                if (MinerOfDuty.Session.LocalGamers[0].IsDataAvailable)
                {
                    NetworkGamer sender;
                    MinerOfDuty.Session.LocalGamers[0].ReceiveData(Packet.PacketReader, out sender);
                    switch (Packet.GetPacketID())
                    {
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
                        case Packet.PACKETID_PLAYERLEVEL:
                            if (MinerOfDuty.Session.FindGamerById(sender.Id) != null)
                            {
                                Packet.ReadPlayerLevel(MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats);
                            }
                            break;
                    }
                }

                if (MinerOfDuty.Session.LocalGamers[0].IsHost && shouldCountDown == false)
                {
                    if (Input.WasButtonPressed(Buttons.Y))
                    {
                        shouldCountDown = true;
                        countDownTimeInSeconds = 5;
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
                            }
                        }
                    }
                }
                #endregion

                #region choose
                if (!useLeftMenu)
                {
                    if (Input.WasButtonPressed(Buttons.A))
                    {
                        //mute
                        if (MinerOfDuty.Session.AllGamers[selectedIndex].Id != MinerOfDuty.Session.LocalGamers[0].Id)
                        {
                            (MinerOfDuty.Session.AllGamers[selectedIndex].Tag as GamePlayerStats).IsMuted = !(MinerOfDuty.Session.AllGamers[selectedIndex].Tag as GamePlayerStats).IsMuted;
                            Packet.WriteMutePacket(MinerOfDuty.Session.LocalGamers[0], MinerOfDuty.Session.AllGamers[selectedIndex], (MinerOfDuty.Session.AllGamers[selectedIndex].Tag as GamePlayerStats).IsMuted);
                            MinerOfDuty.Session.LocalGamers[0].EnableSendVoice(MinerOfDuty.Session.AllGamers[selectedIndex], !(MinerOfDuty.Session.AllGamers[selectedIndex].Tag as GamePlayerStats).IsMuted);
                        }
                        Audio.PlaySound(Audio.SOUND_UICLICK);
                    }
                }
                #endregion

                if (delay >= 0)
                    delay -= gameTime.ElapsedGameTime.Milliseconds;

              
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
                if(useLeftMenu)
                    sideMenu.Update((short)gameTime.ElapsedGameTime.Milliseconds);


                MinerOfDuty.Session.Update();
            }


            public void Draw(SpriteBatch sb)
            {
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);

                if (MinerOfDuty.Session.AllGamers.Count < 8 && (shouldCountDown ? countDownTimeInSeconds > 5 : true))
                {
                    sb.DrawString(Resources.NameFont, "PRESS X TO", new Vector2(300 - (Resources.NameFont.MeasureString("PRESS X TO").X / 2f), 140 - (Resources.NameFont.LineSpacing / 2)), Color.White);
                    sb.DrawString(Resources.NameFont, "SEND AN INVITE", new Vector2(300 - (Resources.NameFont.MeasureString("SEND AN INVITE").X / 2f), 140 + (Resources.NameFont.LineSpacing / 2)), Color.White);
                }

                sb.DrawString(Resources.TitleFont, title, titlePos, Color.White);

                sideMenu.Draw(sb);

                if (useLeftMenu == false) //////////whites out the side menu
                {
                    if (sideMenu.GetSelectedItemID() == "clan")
                        sb.DrawString(Resources.Font, "SET CLAN TAG", sideMenu["clan"].Position, Color.White);
                    else if (sideMenu.GetSelectedItemID() == "leave")
                        sb.DrawString(Resources.Font, "LEAVE LOBBY", sideMenu["leave"].Position, Color.White);
                }

                sb.DrawString(Resources.Font, "PLAYERS", new Vector2(425, 200), Color.White);

                Vector2 pos = new Vector2(450, 240);

                NetworkGamer gamer;
                for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                {
                    gamer = MinerOfDuty.Session.AllGamers[i];
                    if (useLeftMenu == false && selectedIndex == i)
                    {
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Level + " " +
                            ((gamer.Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (gamer.Tag as GamePlayerStats).ClanTag + "]" : "")
                            + gamer.Gamertag, pos, Color.Green);
                        pos.Y += Resources.Font.LineSpacing;
                    }
                    else
                    {
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Level + " " +
                            ((gamer.Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (gamer.Tag as GamePlayerStats).ClanTag + "]" : "")
                             + gamer.Gamertag, pos, Color.White);
                        pos.Y += Resources.Font.LineSpacing;
                    }

                    if ((gamer.Tag as GamePlayerStats).IsMuted || gamer.IsMutedByLocalUser)
                        sb.Draw(Resources.Muted, pos - new Vector2(45, 45), Color.White);
                    else if (gamer.IsTalking)
                        sb.Draw(Resources.IsTalking, pos - new Vector2(45, 45), Color.White);
                    else if (gamer.HasVoice && !gamer.IsTalking)
                        sb.Draw(Resources.CanTalk, pos - new Vector2(45, 45), Color.White);
                }



                if (shouldCountDown)
                    sb.DrawString(Resources.NameFont, "0:" + (countDownTimeInSeconds < 10 ? "0" + countDownTimeInSeconds : countDownTimeInSeconds.ToString()), new Vector2(1000 - (Resources.NameFont.MeasureString((countDownTimeInSeconds < 10 ? "0" + countDownTimeInSeconds : countDownTimeInSeconds.ToString())).X / 2f), 140), Color.White);
                else
                {
                    sb.DrawString(Resources.NameFont, "WAITING FOR", new Vector2(1000 - (Resources.NameFont.MeasureString("WAITING FOR").X / 2f), 140 - (Resources.NameFont.LineSpacing / 2)), Color.White);
                    sb.DrawString(Resources.NameFont, "HOST TO START", new Vector2(1000 - (Resources.NameFont.MeasureString("HOST TO START").X / 2f), 140 + (Resources.NameFont.LineSpacing / 2)), Color.White);
                }

                if (MinerOfDuty.Session.IsHost && shouldCountDown == false)
                {
                    sb.DrawString(Resources.NameFont, "PRESS Y TO START", new Vector2(sideMenu["clan"].Position.X, 500), Color.White);
                }
            }


            private void Session_GameStarted(object sender, GameStartedEventArgs e)
            {
                MinerOfDuty.editor = new Game.Editor.WorldEditor(MinerOfDuty.Self.GraphicsDevice,
                    type == MapEditCreateType.Flat ? Game.Editor.WorldEditor.WorldGeneration.Flat
                    : type == MapEditCreateType.FlatWithCaves ? Game.Editor.WorldEditor.WorldGeneration.FlatWithCaves
                    : type == MapEditCreateType.Random ? Game.Editor.WorldEditor.WorldGeneration.Random : WorldEditor.WorldGeneration.Island, password, size, gameMode, teamAName, teamBName,
                    trees, weapons, editing);
                MinerOfDuty.DrawEdit();
                MinerOfDuty.editor.mapName = title;
                if (MessageBox.IsMessageBeingShown)
                {
                    MessageBox.CloseMessageBox();
                }

                Audio.SetFading(Audio.Fading.Out);
            }

            private void Session_GamerJoined(object sender, GamerJoinedEventArgs e)
            {
                e.Gamer.Tag = new GamePlayerStats();

                if (MinerOfDuty.Session.Host.IsLocal && e.Gamer.IsLocal == false)
                {
                    Packet.PacketWriter.Write(Packet.PACKETID_COUNTDOWN);
                    Packet.PacketWriter.Write((short)countDownTimeInSeconds);
                    MinerOfDuty.Session.LocalGamers[0].SendData(Packet.PacketWriter, SendDataOptions.Reliable, e.Gamer);
                }

                if (e.Gamer.IsLocal == false)
                    Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

                shouldCountDown = false;
            }
        }
    }
}
