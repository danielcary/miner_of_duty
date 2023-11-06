using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Net;
using Miner_Of_Duty.Menus;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Game.Editor;
using Microsoft.Xna.Framework.GamerServices;

namespace Miner_Of_Duty.LobbyCode
{
    public class SearchLobby : IGameScreen
    {
        private NetworkSessionProperties props;
        private GameModes gameMode;
        private enum State { Finding, Refreshing, Joining, Main }
        private State state;
        private Menu.BackPressed back;

        private int MaxPages
        {
            get
            {
                if (MinerOfDuty.SessionCollection.Count <= 7)
                    return 1;
                else if (MinerOfDuty.SessionCollection.Count <= 14)
                    return 2;
                else if (MinerOfDuty.SessionCollection.Count <= 21)
                    return 3;
                else
                    return 4;
            }
        }

        private int ItemsOnPage(int page)
        {
            if (MaxPages == 1)
            {
                return MinerOfDuty.SessionCollection.Count;
            }
            else if (MaxPages > page)
                return 8;
            else
            {
                return MinerOfDuty.SessionCollection.Count - (page * 7);
            }
        }

        private int page = 1;
        private int selectedIndex;
        private int[] indexs;
        private MinerOfDuty minerOfDuty;

        private string title;
        private Vector2 titlePos;

        public SearchLobby(Menu.BackPressed back, MinerOfDuty minerOfDuty)
        {
            this.minerOfDuty = minerOfDuty;
            this.back = back;
            
            state = State.Main;
            page = 1;
            dot = 0;
        }

        public void Show(GameModes gameMode)
        {
            this.gameMode = gameMode;
            props = new NetworkSessionProperties();
            props[0] = (int)gameMode;

          //  if(gameMode == GameModes.SwarmMode)
            //    props[7] = 1;

            Refresh();
            state = State.Finding;
            dot = 0;
            page = 1;


            title = gameMode == GameModes.TeamDeathMatch ? "Team Deathmatch".ToUpper() : gameMode == GameModes.FreeForAll ? "Free For All".ToUpper()
                : gameMode == GameModes.FortWars ? "FORT WARS" : gameMode == GameModes.SearchNMine ? "Search & Mine".ToUpper() : gameMode == GameModes.KingOfTheBeach ? "KING OF THE BEACH" : "Map Edit";

            if (gameMode == GameModes.SwarmMode)
                title = "SWARM MODE";

            titlePos = new Vector2(640 - (Resources.TitleFont.MeasureString(title).X / 2f), 115);
        }

        private int dot = 0;
        private int dotDelay = 0;
        private int delay = 0;
        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (delay > 0)
                delay -= gameTime.ElapsedGameTime.Milliseconds;

            if (state == State.Main)
            {
                if (Input.WasButtonPressed(Buttons.A))
                {
                    if (MinerOfDuty.SessionCollection.Count > 0)
                        Join();
                }
                else if (Input.WasButtonPressed(Buttons.Y))
                {
                    Refresh();
                }
                else if (Input.WasButtonPressed(Buttons.B))
                {
                    if (MinerOfDuty.SessionCollection != null && MinerOfDuty.SessionCollection.IsDisposed == false)
                        MinerOfDuty.SessionCollection.Dispose();

                    MinerOfDuty.SessionCollection = null;

                    state = State.Main;
                    back.Invoke(this);
                }
                else if (Input.WasButtonPressed(Buttons.LeftShoulder))
                {
                    if (--page < 1)
                        page = 1;

                    if (selectedIndex >= ItemsOnPage(page))
                        selectedIndex = ItemsOnPage(page) - 1;
                }
                else if (Input.WasButtonPressed(Buttons.RightShoulder))
                {
                    if (++page > MaxPages)
                        page = MaxPages;

                    if (selectedIndex >= ItemsOnPage(page))
                        selectedIndex = ItemsOnPage(page) - 1;
                }
                else if (Input.IsThumbstickOrDPad(Input.Direction.Up) && delay <= 0)
                {
                    if (--selectedIndex <= -1)
                        selectedIndex = 0;
                    else
                    {
                        delay = 175;
                    }
                }
                else if (Input.IsThumbstickOrDPad(Input.Direction.Down) && delay <= 0)
                {
                    if (++selectedIndex >= MinerOfDuty.SessionCollection.Count)
                    {
                        selectedIndex = MinerOfDuty.SessionCollection.Count - 1;
                    }
                    else
                    {
                        delay = 175;
                    }
                }
            }
            else
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

            
        }

        private bool needsFurtherSorting = false;
        private void Sort()
        {
            List<int> open, closed;
            open = new List<int>(); closed = new List<int>();

            for (int i = 0; i < MinerOfDuty.SessionCollection.Count; i++)
            {
                open.Add(i);
            }

            while (open.Count > 0)
            {
                int bestVal = 6969696;
                int bestIndex = 0;

                for (int i = 0; i < open.Count; i++)
                {
                    if (MinerOfDuty.SessionCollection[open[i]].QualityOfService.AverageRoundtripTime.Milliseconds < bestVal)
                    {
                        bestVal = MinerOfDuty.SessionCollection[open[i]].QualityOfService.AverageRoundtripTime.Milliseconds;
                        bestIndex = i;
                    }
                }

                closed.Add(open[bestIndex]);
                open.RemoveAt(bestIndex);
            }

            indexs = closed.ToArray();
            needsFurtherSorting = false;
            for (int i = 0; i < indexs.Length; i++)
            {
                if (MinerOfDuty.SessionCollection[indexs[i]].QualityOfService.IsAvailable == false)
                {
                    needsFurtherSorting = true;
                    break;
                }
            }
        }

        private void EndFind(IAsyncResult result)
        {
            MinerOfDuty.SessionCollection = NetworkSession.EndFind(result);
            Sort();
            state = State.Main;
            dot = 0;
            page = 1;
        }

        private void Refresh()
        {
            if (MinerOfDuty.SessionCollection != null && MinerOfDuty.SessionCollection.IsDisposed == false)
                MinerOfDuty.SessionCollection.Dispose();
            MinerOfDuty.SessionCollection = null;

            NetworkSession.BeginFind(NetworkSessionType.PlayerMatch, new SignedInGamer[] { SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer] }, props, EndFind, null);
            state = State.Refreshing;
        }

        private void BadJoin(int selected)
        {
            Refresh();
        }

        private void EndJoin(IAsyncResult result)
        {
            try
            {
                MinerOfDuty.Session = NetworkSession.EndJoin(result);
                if (gameMode != GameModes.Edit)
                {
                    if (MinerOfDuty.Session.SessionState == NetworkSessionState.Lobby)
                    {
                        MinerOfDuty.lobby = new Lobby(minerOfDuty, gameMode);
                        MinerOfDuty.DrawLobby();
                    }
                    else if (MinerOfDuty.Session.SessionState == NetworkSessionState.Playing)
                    {
                        MinerOfDuty.lobby = new Lobby(gameMode);
                    }
                    else
                        throw new NetworkSessionJoinException();
                }
                else
                {
                    if (MinerOfDuty.Session.SessionState == NetworkSessionState.Lobby)
                    {
                        MinerOfDuty.lobby = new MapEditLobby((MapEditCreateType)MinerOfDuty.Session.SessionProperties[7]);
                        MinerOfDuty.DrawLobby();
                    }
                    else
                    {
                        MinerOfDuty.editor = new WorldEditor(0);
                        MinerOfDuty.DrawEdit();
                    }
                }
                state = State.Main;
                dot = 0;
            }
            catch (Exception)
            {
                MessageBox.ShowMessageBox(BadJoin, new string[] { "OK" }, 0, new string[] { "ERROR JOINING!" });
            }
        }

        private void Join()
        {
            NetworkSession.BeginJoin(MinerOfDuty.SessionCollection[selectedIndex], EndJoin, null);
            state = State.Joining;
        }


        public void Draw(SpriteBatch sb)
        {
            

            if (state == State.Main)
            {
                sb.Draw(Resources.LobbyBackgroundTexture, Vector2.Zero, Color.White);
                if (MinerOfDuty.SessionCollection != null && indexs != null)
                {
                    AvailableNetworkSession session;


                    sb.DrawString(Resources.TitleFont, title, titlePos, Color.White);

                    sb.DrawString(Resources.Font, "HOST", new Vector2(225, 185), Color.White);
                    sb.DrawString(Resources.Font, "PLAYERS", new Vector2(575 + 100, 185), Color.White);
                    sb.DrawString(Resources.Font, "PING", new Vector2(715 + 200, 185), Color.White);

                    Vector2 startPos = new Vector2(250, 175 + Resources.Font.LineSpacing * 1.5f);

                    for (int i = (page - 1) * 7; i < ((page - 1) * 7) + ItemsOnPage(page); i++)
                    {
                        session = MinerOfDuty.SessionCollection[indexs[i]];
                        sb.DrawString(Resources.NameFont, session.HostGamertag, startPos, selectedIndex == indexs[i] ? Color.Green : Color.White);
                        sb.DrawString(Resources.NameFont, session.CurrentGamerCount + " / " + (session.CurrentGamerCount + session.OpenPrivateGamerSlots + session.OpenPublicGamerSlots).ToString(), startPos + new Vector2(575 - Resources.NameFont.MeasureString(session.CurrentGamerCount + " / 8").X, 0), selectedIndex == indexs[i] ? Color.Green : Color.White);
                        if(session.QualityOfService.IsAvailable)
                            sb.DrawString(Resources.NameFont, session.QualityOfService.AverageRoundtripTime.Milliseconds.ToString(), startPos + new Vector2(775 - Resources.NameFont.MeasureString(session.QualityOfService.AverageRoundtripTime.Milliseconds.ToString()).X, 0), selectedIndex == indexs[i] ? Color.Green : Color.White);
                        else
                            sb.DrawString(Resources.NameFont, "~", startPos + new Vector2(775 - Resources.NameFont.MeasureString("~").X, 0), selectedIndex == indexs[i] ? Color.Green : Color.White);
                        startPos.Y += Resources.NameFont.LineSpacing;
                    }


                    if (MaxPages == 1)
                    {
                        sb.DrawString(Resources.DescriptionFont, "(A) JOIN  (Y) REFRESH  (B) BACK", new Vector2(140, 595), Color.White);
                    }
                    else if (page > 1 && page != MaxPages)
                    {
                        sb.DrawString(Resources.DescriptionFont, "(A) JOIN  (Y) REFRESH  (B) BACK  (LB) PREVIOUS  (RB) NEXT", new Vector2(140, 595), Color.White);
                    }
                    else if (page == MaxPages)
                    {
                        sb.DrawString(Resources.DescriptionFont, "(A) JOIN  (Y) REFRESH  (B) BACK  (LB) PREVIOUS", new Vector2(140, 595), Color.White);
                    }
                    else
                    {
                        sb.DrawString(Resources.DescriptionFont, "(A) JOIN  (Y) REFRESH  (B) BACK  (RB) NEXT", new Vector2(140, 595), Color.White);
                    }

                    sb.DrawString(Resources.DescriptionFont, "PAGE: " + page + " / " + MaxPages, new Vector2(1150 - Resources.DescriptionFont.MeasureString("PAGE: " + page + " / " + MaxPages).X, 595), Color.White);
                }

                
            }
            else 
            {
                sb.Draw(Resources.MainMenuTexture, Vector2.Zero, Color.White);
                string text;
                if (state == State.Joining)
                    text = "JOINING SESSION";
                else if (state == State.Finding)
                    text = "FINDING SESSIONS";
                else
                    text = "REFRESHING";

                sb.Draw(Resources.MessageBoxBackTexture, new Vector2(640 - (Resources.MessageBoxBackTexture.Width / 2), 320 - (Resources.MessageBoxBackTexture.Height / 2)), Color.White);
                sb.DrawString(Resources.Font, text + (dot == 1 ? "." : dot == 2 ? ".." : dot == 3 ? "..." : ""), new Vector2(640 - (Resources.Font.MeasureString(text).X / 2f), 320 - (Resources.Font.LineSpacing / 2f)), Color.White);
            }
        }

        public void Render(Microsoft.Xna.Framework.Graphics.GraphicsDevice gd) { }


        public void Activated()
        {
        }

        public void Deactivated()
        {
        }
    }
}
