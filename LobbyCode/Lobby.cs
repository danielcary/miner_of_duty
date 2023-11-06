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
using Microsoft.Xna.Framework.GamerServices;

namespace Miner_Of_Duty.LobbyCode
{
    public class Lobby : ILobby
    {
        #region vars
        private MinerOfDuty minerOfDuty;
        private GameModes gameMode;

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

        /// <summary>
        /// Use for an already started match, CANNOT BE SWARM MODE ARG
        /// </summary>
        public Lobby(GameModes gameMode)
        {
            this.minerOfDuty = MinerOfDuty.Self;
            this.gameMode = gameMode;
            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                MinerOfDuty.Session.AllGamers[i].Tag = new GamePlayerStats();
            }
            Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

            Sort();

            title = gameMode == GameModes.TeamDeathMatch ? "Team Deathmatch".ToUpper() : gameMode == GameModes.FreeForAll ? "Free For All".ToUpper()
                 : gameMode == GameModes.FortWars ? "FORT WARS" : gameMode == GameModes.SearchNMine ? "Search & Mine".ToUpper() : gameMode == GameModes.SwordGiver ? "SWORD GIVER"
                 : "KING OF THE BEACH";

            if (gameMode == GameModes.SwarmMode)
                title = "SWARM MODE";

            titlePos = new Vector2(640 - (Resources.TitleFont.MeasureString(title).X / 2f), 130);

            sideMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("edit", "EDIt classes"),
                new MenuElement("view", "View Profile"),
                new MenuElement("clan", "SET CLAN TAG"),
                new MenuElement("leave", "Leave Lobby")}, 25.0f, (float)(200 - 90));

            editClasses = new EditClasses(Back);
            profileViewer = new ProfileView(Back);

            

            //game has already started so we need to create the game

            if (gameMode == GameModes.FortWars)
            {
                teamManager = new FortWarsManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.FreeForAll)
            {
                teamManager = new FFAManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.SearchNMine)
            {
                teamManager = new SearchNMineManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.TeamDeathMatch)
            {
                teamManager = new TeamDeathmatchManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.KingOfTheBeach)
            {
                teamManager = new KTBManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }

            MinerOfDuty.Session.GamerJoined += session_GamerJoined;
            MinerOfDuty.Session.GamerLeft += session_GamerLeft;
            ignore = true;
            MinerOfDuty.Session.GameStarted += session_GameStarted;
            MinerOfDuty.Session.GameEnded += session_GameEnded;

            MinerOfDuty.game = new MultiplayerGame(gameMode, teamManager, minerOfDuty.GraphicsDevice, out gameLeft);
            MinerOfDuty.DrawGame();

            Audio.SetFading(Audio.Fading.Out);
        }
        private bool ignore = false;

        public Lobby(MinerOfDuty minerOfDuty, GameModes gameMode)
        {
            this.minerOfDuty = minerOfDuty;
            this.gameMode = gameMode;

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                MinerOfDuty.Session.AllGamers[i].Tag = new GamePlayerStats();
            }

            if(gameMode == GameModes.FortWars)
            {
                teamManager = new FortWarsManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if(gameMode == GameModes.FreeForAll)
            {
                teamManager = new FFAManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if(gameMode == GameModes.SearchNMine)
            {
                teamManager = new SearchNMineManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if(gameMode == GameModes.TeamDeathMatch)
            {
                teamManager = new TeamDeathmatchManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.SwarmMode)
            {
                teamManager = new SwarmManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.SwordGiver)
            {
                teamManager = new SwarmManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
                Guide.ShowGameInvite((PlayerIndex)Input.ControllingPlayer, null);
            }
            else if (gameMode == GameModes.KingOfTheBeach)
            {
                teamManager = new KTBManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }

            Packet.WritePlayerLevel(MinerOfDuty.Session.LocalGamers[0], (byte)MinerOfDuty.CurrentPlayerProfile.Level, MinerOfDuty.CurrentPlayerProfile.Clan);

            Sort();

            if (MinerOfDuty.Session.Host.IsLocal)
                countDownTimeInSeconds = 45;

            title = gameMode == GameModes.TeamDeathMatch ? "Team Deathmatch".ToUpper() : gameMode == GameModes.FreeForAll ? "Free For All".ToUpper()
                : gameMode == GameModes.FortWars ? "FORT WARS" : gameMode == GameModes.SearchNMine ? "Search & Mine".ToUpper() : gameMode == GameModes.SwordGiver ? "SWORD GIVER" 
                : "KING OF THE BEACH";

            if (gameMode == GameModes.SwarmMode)
                title = "SWARM MODE";

            titlePos = new Vector2(640 - (Resources.TitleFont.MeasureString(title).X / 2f), 130);

            sideMenu = new Menu(Choose, Back, new MenuElement[]{
                new MenuElement("edit", "EDIt classes"),
                new MenuElement("view", "View Profile"),
                new MenuElement("clan", "SET CLAN TAG"),
                new MenuElement("leave", "Leave Lobby")}, 25.0f, (float)(200 - 90));

            editClasses = new EditClasses(Back);
            profileViewer = new ProfileView(Back);

            MinerOfDuty.Session.GamerJoined += session_GamerJoined;
            MinerOfDuty.Session.GamerLeft += session_GamerLeft;
            MinerOfDuty.Session.GameStarted += session_GameStarted;
            MinerOfDuty.Session.GameEnded += session_GameEnded;
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

        public static bool IsPrivateLobby()
        {
            return MinerOfDuty.Session.SessionProperties[7].HasValue;
        }

        private int delay = 0;
        public void Update(GameTime gameTime)
        {
            MinerOfDuty.Session.Update();

            if (MinerOfDuty.Session.SessionProperties[7].HasValue && useLeftMenu && MinerOfDuty.Session.AllGamers.Count < 8 && (shouldCountDown ? countDownTimeInSeconds > 5 : true))
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
                    case Packet.PACKETID_PLAYERSCORE:
                        (MinerOfDuty.Session.FindGamerById(Packet.PacketReader.ReadByte()).Tag as GamePlayerStats).Score = Packet.PacketReader.ReadInt16();
                        break;
                    case Packet.PACKETID_PLAYERLEVEL:
                        if (MinerOfDuty.Session.FindGamerById(sender.Id) != null)
                        {
                            Packet.ReadPlayerLevel(MinerOfDuty.Session.FindGamerById(sender.Id).Tag as GamePlayerStats);
                        }
                        break;
                    case Packet.PACKETID_GETMEATEAMMR:
                        TeamManager.Team t = teamManager.CalculateWhichTeamForPlayer(sender.Id);
                        Packet.WriteToAddToTeamManager(MinerOfDuty.Session.LocalGamers[0], sender.Id, t);
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
                }
            }

            if (otherScreen == null)
            {
                if (MinerOfDuty.Session.AllGamers.Count >= (IsPrivateLobby() ? 1 : gameMode == GameModes.SwarmMode ? 1 : 2))
                {
                    if (Input.WasButtonPressed(Buttons.Y))
                    {
                        MinerOfDuty.Session.LocalGamers[0].IsReady = true;
                    }
                    if (MinerOfDuty.Session.LocalGamers[0].IsHost)
                    {
                        if (shouldCountDown == false)
                            if (MinerOfDuty.Session.IsEveryoneReady)
                            {
                                countDownTimeInSeconds = 4;
                                shouldCountDown = true;
                            }
                    }
                }
            }
            #endregion

            #region countdown
            if (IsPrivateLobby() ? shouldCountDown : shouldCountDown || MinerOfDuty.Session.AllGamers.Count >= 4)
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


                    if (countDownTimeInSeconds <= 1)
                    {
                        if (MessageBox.IsMessageBeingShown)
                        {
                            MessageBox.CloseMessageBox();
                        }
                    }

                    if (countDownTimeInSeconds <= 0)
                    {
                        if (MinerOfDuty.Session.SessionState == NetworkSessionState.Lobby)
                            MinerOfDuty.Session.StartGame();
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

            
        }

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

                if (MinerOfDuty.Session.SessionProperties[7].HasValue && useLeftMenu && MinerOfDuty.Session.AllGamers.Count < 8 && (shouldCountDown ? countDownTimeInSeconds > 5 : true))
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
                sb.DrawString(Resources.Font, "SCORE", new Vector2(1100 - Resources.Font.MeasureString("SCORE").X, 200), Color.White);

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
                            pos + new Vector2(635 - Resources.NameFont.MeasureString((gamer.Tag as GamePlayerStats).Score.ToString()).X, 0), Color.Green);
                        pos.Y += Resources.Font.LineSpacing;
                    }
                    else
                    {
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Level + " " +
                            ((gamer.Tag as GamePlayerStats).ClanTag.Length > 0 ? "[" + (gamer.Tag as GamePlayerStats).ClanTag + "]" : "")
                            + gamer.Gamertag, pos, Color.White);
                        sb.DrawString(Resources.NameFont, (gamer.Tag as GamePlayerStats).Score.ToString(),
                            pos + new Vector2(635 - Resources.NameFont.MeasureString((gamer.Tag as GamePlayerStats).Score.ToString()).X, 0), Color.White);
                        pos.Y += Resources.Font.LineSpacing;
                    }

                    if ((gamer.Tag as GamePlayerStats).IsMuted || MinerOfDuty.Session.FindGamerById(Sorted[i]).IsMutedByLocalUser)
                        sb.Draw(Resources.Muted, pos - new Vector2(45, 45), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(Sorted[i]).IsTalking)
                        sb.Draw(Resources.IsTalking, pos - new Vector2(45, 45), Color.White);
                    else if (MinerOfDuty.Session.FindGamerById(Sorted[i]).HasVoice && !MinerOfDuty.Session.FindGamerById(Sorted[i]).IsTalking)
                        sb.Draw(Resources.CanTalk, pos - new Vector2(45, 45), Color.White);
                }

                if (shouldCountDown || MinerOfDuty.Session.AllGamers.Count >= 4)
                    sb.DrawString(Resources.NameFont, "0:" + (countDownTimeInSeconds < 10 ? "0" + countDownTimeInSeconds : countDownTimeInSeconds.ToString()), new Vector2(1000 - (Resources.NameFont.MeasureString((countDownTimeInSeconds < 10 ? "0" + countDownTimeInSeconds : countDownTimeInSeconds.ToString())).X / 2f), 140), Color.White);
                else if (IsPrivateLobby() == false && gameMode != GameModes.SwarmMode)
                {
                    sb.DrawString(Resources.NameFont, "NEED " + (4 - MinerOfDuty.Session.AllGamers.Count), new Vector2(1000 - (Resources.NameFont.MeasureString("NEED" + (4 - MinerOfDuty.Session.AllGamers.Count)).X / 2f), 140 - (Resources.NameFont.LineSpacing / 2)), Color.White);
                    sb.DrawString(Resources.NameFont, "MORE PLAYERS", new Vector2(1000 - (Resources.NameFont.MeasureString("MORE PLAYERS").X / 2f), 140 + (Resources.NameFont.LineSpacing / 2)), Color.White);
                }



                if (MinerOfDuty.Session.AllGamers.Count >= (IsPrivateLobby() ? 1 : gameMode == GameModes.SwarmMode ? 1 : 2))
                {
                    if (MinerOfDuty.Session.LocalGamers[0].IsReady == false)
                    {
                        sb.DrawString(Resources.DescriptionFont, "PRESS Y TO VOTE", new Vector2(sideMenu["edit"].Position.X, 475), Color.White);
                        sb.DrawString(Resources.DescriptionFont, "      TO START", new Vector2(sideMenu["edit"].Position.X, 475 + Resources.DescriptionFont.LineSpacing), Color.White);
                    }
                    else
                    {
                        sb.DrawString(Resources.DescriptionFont, "WAITING", new Vector2(sideMenu["edit"].Position.X + 140, 475 + Resources.DescriptionFont.LineSpacing), Color.White, 0, Resources.DescriptionFont.MeasureString("WAITING") / 2f, 1, SpriteEffects.None, 0);
                    }
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
                if (MinerOfDuty.Session != null && MinerOfDuty.Session.IsDisposed == false)
                    MinerOfDuty.Session.Dispose();
                MinerOfDuty.DrawMenu(false);
            }

            if (selected == -1)
            {
                MinerOfDuty.Session.GamerJoined -= new EventHandler<GamerJoinedEventArgs>(session_GamerJoined);
                MinerOfDuty.Session.GamerLeft -= new EventHandler<GamerLeftEventArgs>(session_GamerLeft);
                MinerOfDuty.Session.GameStarted -= new EventHandler<GameStartedEventArgs>(session_GameStarted);
                MinerOfDuty.Session.GameEnded -= new EventHandler<GameEndedEventArgs>(session_GameEnded);
            }
            else
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
            else if(haveIBeenAssigned)
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

            if (MinerOfDuty.Session.Host.IsLocal)
            {
                if (countDownTimeInSeconds <= 3)
                {
                    countDownTimeInSeconds = 6;
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
        }

        private void session_GameStarted(object sender, GameStartedEventArgs e)
        {
            if (ignore)
            {
                ignore = false;
                return;
            }
            
            teamManager.MuteTeam();


            if (gameMode != GameModes.SwarmMode)
                MinerOfDuty.game = new MultiplayerGame(minerOfDuty, gameMode, teamManager, minerOfDuty.GraphicsDevice, out gameLeft);
            else
                MinerOfDuty.game = new SwarmGame(minerOfDuty, teamManager, minerOfDuty.GraphicsDevice, out gameLeft);
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


            if (gameMode == GameModes.FortWars)
            {
                teamManager = new FortWarsManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.FreeForAll)
            {
                teamManager = new FFAManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.SearchNMine)
            {
                teamManager = new SearchNMineManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.TeamDeathMatch)
            {
                teamManager = new TeamDeathmatchManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if (gameMode == GameModes.SwarmMode)
            {
                teamManager = new SwarmManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);
            }
            else if(gameMode == GameModes.KingOfTheBeach)
                teamManager = new KTBManager(out teamManagerLeft, MinerOfDuty.Session.LocalGamers[0].Id);

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
