using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.Game.Networking;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Menus
{
    public class PlayerPermissions : IMenuOwner
    {
        /// <summary>
        /// true if in view only mode
        /// </summary>
        private Dictionary<byte, bool> permissions;

        public void SetPerm(byte id, bool view)
        {
            if (permissions.ContainsKey(id))
            {
                permissions[id] = view;
            }
            else
                permissions.Add(id, view); 
        }

        public PlayerPermissions()
        {
            permissions = new Dictionary<byte, bool>();
        }
        private Menu menu, trueFalseMenu;

        public void Show(Menu.BackPressed back)
        {
            for(int i = 0; i < permissions.Keys.Count; i++)
            {
                if (MinerOfDuty.Session.FindGamerById(permissions.Keys.ElementAt(i)) == null)
                {
                    permissions.Remove(permissions.Keys.ElementAt(i));
                    i = 0;
                }
            }

            for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
            {
                if (permissions.ContainsKey(MinerOfDuty.Session.AllGamers[i].Id) == false)
                {
                    permissions.Add(MinerOfDuty.Session.AllGamers[i].Id, false);
                }
                
            }


            List<MenuElement> menuElements = new List<MenuElement>(), tfMenuElements = new List<MenuElement>();
            menuElements.Add(new MenuElement("TITback", "back"));
            tfMenuElements.Add(new MenuElement("",""));

            for (int i = 0; i < permissions.Keys.Count; i++)
            {

                menuElements.Add(new MenuElement(permissions.Keys.ElementAt(i).ToString(), MinerOfDuty.Session.FindGamerById(permissions.Keys.ElementAt(i)).Gamertag));
                tfMenuElements.Add(new MenuElement(permissions.Keys.ElementAt(i).ToString(), permissions[permissions.Keys.ElementAt(i)] ? "VIEW ONLY" : "FULL ACCESS"));

            }

            menu = new Menu(delegate(IMenuOwner ms, string id)
                {
                    if (id == "TITback")
                    {
                        back.Invoke(this);
                    }
                    else
                    {
                        byte parsedID = byte.Parse(id);

                        permissions[parsedID] = !permissions[parsedID];
                        trueFalseMenu[id].Text = permissions[parsedID] ? "VIEW ONLY" : "FULL ACCESS";
                        //send packet
                        Packet.WritePermissionPacket(MinerOfDuty.Session.LocalGamers[0], permissions[parsedID], parsedID);

                    }
                }, delegate(object sender)
                {
                    back.Invoke(this);
                }, menuElements.ToArray(),100);

            menu["TITback"].Position.X -= 25;

            trueFalseMenu = new Menu(delegate(IMenuOwner ms, string id){}, delegate(object sender){}, tfMenuElements.ToArray(),700);

        }

        public void Update(short timePassedInMilliseconds)
        {

            try
            {
                for (int i = 0; i < permissions.Keys.Count; i++)
                {
                    if (MinerOfDuty.Session.FindGamerById(permissions.Keys.ElementAt(i)) == null)
                        menu[permissions.Keys.ElementAt(i).ToString()].CanSelectMe = false;
                }
            }
            catch (Exception) { }

            menu.Update(timePassedInMilliseconds);
            trueFalseMenu.Update(timePassedInMilliseconds);

        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.ScoreboardBack, Vector2.Zero, Color.White);
            menu.Draw(sb);
            trueFalseMenu.Draw(sb);
        }
    }
}
