using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.Game;

namespace Miner_Of_Duty.Menus
{
    public class ClassViewer
    {
        private CharacterClass workingClass;

        public ClassViewer(CharacterClass startingClass)
        {
            workingClass = startingClass;
            SetUpView();
        }

        private Vector2 mainAPicPos = new Vector2(620, 120),
            mainBPicPos = new Vector2(900, 120),
            itemAPicPos = new Vector2(620, 375),
            itemBPicPos = new Vector2(620, 495);
        private Vector2 itemANamePos = new Vector2(620 + 150, 375 + (75/2f) - (Resources.Font.LineSpacing / 2f)),
            itemBNamePos = new Vector2(620 + 150, 495 + (75 / 2f) - (Resources.Font.LineSpacing / 2f));

        private Vector2
            grenadeAPicPos = new Vector2(620, 585),
            grenadeBPicPos = new Vector2(885, 585),
            grenadeANamePos = new Vector2(670, 585 + (50 - Resources.DescriptionFont.LineSpacing)),
            grenadeBNamePos = new Vector2(935, 585 + (50 - Resources.DescriptionFont.LineSpacing));


        private Texture2D mainAPic, mainBPic, itemAPic, itemBPic, grenadeAPic, grenadeBPic;
        private string mainAName, mainBName, itemAName, itemBName, grenadeAName, grenadeBName;
        private string mainADesc, mainBDesc;

        private Vector2 mainANamePos, mainBNamePos;
        private Vector2 mainADescPos, mainBDescPos;

        private string playerUpgrade;
        private Vector2 playerUpgradePos;

        private void SetUpView()
        {
            if (workingClass.Slot1 is WeaponSlot)
            {
                mainAPic = Resources.GunPics[(workingClass.Slot1 as WeaponSlot).GunID];
                mainAName = Inventory.GetItemAsString(Inventory.GunIDToInventoryItem((workingClass.Slot1 as WeaponSlot).GunID));
                if ((workingClass.Slot1 as WeaponSlot).BallisticTip)
                {
                    mainADesc = "BALLISTIC TIP";
                }
                else if ((workingClass.Slot1 as WeaponSlot).ExtendedMags)
                {
                    mainADesc = "EXTENDED MAGS";
                }
                else if ((workingClass.Slot1 as WeaponSlot).MoreAmmo)
                {
                    mainADesc = "MORE AMMO";
                }
                else mainADesc = "";
            }
            else if (workingClass.Slot1 is ToolSlot)
            {
                mainAPic = Resources.ToolPics[(workingClass.Slot1 as ToolSlot).ToolTypeID];
                mainAName = Inventory.GetItemAsString(Inventory.ToolIDToInventoryItem((workingClass.Slot1 as ToolSlot).ToolTypeID));
                if ((workingClass.Slot1 as ToolSlot).DurableConstruction)
                {
                    mainADesc = "HARDY MATERIALS";
                }
                else if ((workingClass.Slot1 as ToolSlot).SharperEdges)
                {
                    mainADesc = "SHARPER EDGES";
                }
                else mainADesc = "";
            }

            if (workingClass.Slot2 is WeaponSlot)
            {
                mainBPic = Resources.GunPics[(workingClass.Slot2 as WeaponSlot).GunID];
                mainBName = Inventory.GetItemAsString(Inventory.GunIDToInventoryItem((workingClass.Slot2 as WeaponSlot).GunID));
                if ((workingClass.Slot2 as WeaponSlot).BallisticTip)
                {
                    mainBDesc = "BALLISTIC TIP";
                }
                else if ((workingClass.Slot2 as WeaponSlot).ExtendedMags)
                {
                    mainBDesc = "EXTENDED MAGS";
                }
                else if ((workingClass.Slot2 as WeaponSlot).MoreAmmo)
                {
                    mainBDesc = "MORE AMMO";
                }
                else mainBDesc = "";
            }
            else if (workingClass.Slot2 is ToolSlot)
            {
                mainBPic = Resources.ToolPics[(workingClass.Slot2 as ToolSlot).ToolTypeID];
                mainBName = Inventory.GetItemAsString(Inventory.ToolIDToInventoryItem((workingClass.Slot2 as ToolSlot).ToolTypeID));
                if ((workingClass.Slot2 as ToolSlot).DurableConstruction)
                {
                    mainBDesc = "HARDY MATERIALS";
                }
                else if ((workingClass.Slot2 as ToolSlot).SharperEdges)
                {
                    mainBDesc = "SHARPER EDGES";
                }
                else mainBDesc = "";
            }

            if (workingClass.MoreStamina)
                playerUpgrade = "ENDURANCE";
            else if (workingClass.QuickHands)
                playerUpgrade = "QUICK HANDS";
            else if (workingClass.ThickerSkin)
                playerUpgrade = "THICK SKIN";
            else
                playerUpgrade = "";

            grenadeAPic = Resources.ItemPics[Inventory.GrenadeIDToInventoryItem(workingClass.LethalGrenadeID)];
            grenadeAName = Inventory.GetItemAsString(Inventory.GrenadeIDToInventoryItem(workingClass.LethalGrenadeID));

            grenadeBPic = Resources.ItemPics[Inventory.GrenadeIDToInventoryItem(workingClass.SpecialGrenadeID)];
            grenadeBName = Inventory.GetItemAsString(Inventory.GrenadeIDToInventoryItem(workingClass.SpecialGrenadeID));

            itemAPic = Resources.ItemPics[(workingClass.Slot3 as ItemSlot).Item];
            itemAName = Inventory.GetItemAsString((workingClass.Slot3 as ItemSlot).Item);

            itemBPic = Resources.ItemPics[(workingClass.Slot4 as ItemSlot).Item];
            itemBName = Inventory.GetItemAsString((workingClass.Slot4 as ItemSlot).Item);


            mainANamePos = new Vector2(620 + (230 / 2f) - (Resources.Font.MeasureString(mainAName).X / 2f), 120 + 100 + 25);
            mainBNamePos = new Vector2(900 + (230 / 2f) - (Resources.Font.MeasureString(mainBName).X / 2f), 120 + 100 + 25);

            playerUpgradePos = new Vector2((Resources.BackTexture.Width / 2) + 550, 120 + 100 + 25 + (2 * Resources.Font.LineSpacing) + 20);

            mainADescPos = new Vector2(620 + (230 / 2f) - (Resources.DescriptionFont.MeasureString(mainADesc).X / 2f), 120 + 100 + 25 + Resources.Font.LineSpacing + 10);
            mainBDescPos = new Vector2(900 + (230 / 2f) - (Resources.DescriptionFont.MeasureString(mainBDesc).X / 2f), 120 + 100 + 25 + Resources.Font.LineSpacing + 10);
        }

        public void SetCharacterClass(CharacterClass cclass)
        {
            workingClass = cclass;
            SetUpView();
        }


        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.BackTexture, new Vector2(550, 0), Color.White);

            sb.Draw(mainAPic, mainAPicPos, Color.White);
            sb.Draw(mainBPic, mainBPicPos, Color.White);
            sb.Draw(itemAPic, new Rectangle((int)itemAPicPos.X, (int)itemAPicPos.Y, 75, 75), Color.White);
            sb.Draw(itemBPic, new Rectangle((int)itemBPicPos.X, (int)itemBPicPos.Y, 75, 75), Color.White);

            sb.Draw(grenadeAPic, new Rectangle((int)grenadeAPicPos.X, (int)grenadeAPicPos.Y, 50, 50), Color.White);
            sb.Draw(grenadeBPic, new Rectangle((int)grenadeBPicPos.X, (int)grenadeBPicPos.Y, 50, 50), Color.White);

            sb.DrawString(Resources.Font, itemAName, itemANamePos, Color.White);
            sb.DrawString(Resources.Font, itemBName, itemBNamePos, Color.White);
            sb.DrawString(Resources.Font, mainAName, mainANamePos, Color.White);
            sb.DrawString(Resources.Font, mainBName, mainBNamePos, Color.White);

            sb.DrawString(Resources.DescriptionFont, grenadeAName, grenadeANamePos, Color.White);
            sb.DrawString(Resources.DescriptionFont, grenadeBName, grenadeBNamePos, Color.White);

            sb.DrawString(Resources.DescriptionFont, playerUpgrade, playerUpgradePos, Color.White, 0, Resources.DescriptionFont.MeasureString(playerUpgrade) / 2f, 1, SpriteEffects.None, 0);

            sb.DrawString(Resources.DescriptionFont, mainADesc, mainADescPos, Color.White);
            sb.DrawString(Resources.DescriptionFont, mainBDesc, mainBDescPos, Color.White);

        }

    }
}
