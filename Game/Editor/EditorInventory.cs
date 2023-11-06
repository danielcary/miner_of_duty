using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Miner_Of_Duty.Game.Editor
{

    public class InventoryItemBox
    {
        public bool IsMultiple { get; private set; }
        private InventoryItem[] items;
        private int selectedIndex = 0;

        public InventoryItemBox(InventoryItem item)
        {
            IsMultiple = false;
            items = new InventoryItem[] { item };
        }

        public InventoryItemBox(InventoryItem[] items)
        {
            IsMultiple = true;
            this.items = items;
        }

        public void Up()
        {
            selectedIndex--;
            if (selectedIndex <= -1)
            {
                selectedIndex = items.Length - 1;
            }
        }

        public void Down()
        {
            selectedIndex++;
            if (selectedIndex >= items.Length)
                selectedIndex = 0;
        }

        public InventoryItem GetItem { get { return items[selectedIndex]; } }



    }


    public class EditorInventory
    {
        public InventoryItemBox[] items;
        private int selectedIndex;
        private int delay;

        public InventoryItem GetSelectedItem { get { return items[selectedIndex].GetItem; } }

        /// <summary>
        /// Used for changing the selected item of a Networked Player
        /// </summary>
        /// <param name="item">The item to chage to</param>
        public void SetSelectedTo(InventoryItem item)
        {
            selectedIndex = 0;
            items[0] = new InventoryItemBox(item);
        }

        public EditorInventory()
        {
            items = new InventoryItemBox[9];
            items[0] = new InventoryItemBox(new[] { InventoryItem.GrassBlock, InventoryItem.Pitfall, InventoryItem.DirtBlock });
            items[1] = new InventoryItemBox(InventoryItem.SandBlock);
            items[2] = new InventoryItemBox(new [] {InventoryItem.StoneBlock, InventoryItem.Cobblestone, InventoryItem.MossyCobblestone, InventoryItem.Stonebricks, InventoryItem.Firebricks});
            items[3] = new InventoryItemBox(new[] { InventoryItem.WoodBlock, InventoryItem.LeafBlock, InventoryItem.WoodPlanks });
            items[4] = new InventoryItemBox(InventoryItem.GlassBlock);
            items[5] = new InventoryItemBox(new[] { 
                InventoryItem.OrangeBlock, 
                InventoryItem.RedBlock,
                InventoryItem.YellowBlock,
                InventoryItem.BlueBlock,
                InventoryItem.TealBlock,
                InventoryItem.GreenBlock,
                InventoryItem.WhiteBlock,
                InventoryItem.GreyBlock,
                InventoryItem.BlackBlock
            });
            items[6] = new InventoryItemBox(new [] {InventoryItem.LavaBucket, InventoryItem.GlowBlock, InventoryItem.WaterBucket});
            items[7] = new InventoryItemBox(new[]{InventoryItem.AA12Spawn, InventoryItem.AK47Spawn, InventoryItem.Colt45Spawn, InventoryItem.DoubleBarrelSpawn,
                InventoryItem.FALSpawn, InventoryItem.M16Spawn, InventoryItem.MagnumSpawn, InventoryItem.MP5KSpawn, InventoryItem.SingleBarrelSpawn, InventoryItem.UMP45Spawn,
                InventoryItem.VectorSpawn});

            if(MinerOfDuty.editor.gameMode == LobbyCode.GameModes.CustomTDM)
                items[8] = new InventoryItemBox(new[] { InventoryItem.TeamASpawn1, InventoryItem.TeamBSpawn1, InventoryItem.TeamASpawn2, InventoryItem.TeamBSpawn2, InventoryItem.TeamASpawn3, InventoryItem.TeamBSpawn3});
            else if (MinerOfDuty.editor.gameMode == LobbyCode.GameModes.CustomFFA)
            {
                items[8] = new InventoryItemBox(new[] { InventoryItem.Spawn1, InventoryItem.Spawn2, InventoryItem.Spawn3, InventoryItem.Spawn4, InventoryItem.Spawn5, InventoryItem.Spawn6 });
            }
            else if (MinerOfDuty.editor.gameMode == LobbyCode.GameModes.CustomSNM)
            {
                items[8] = new InventoryItemBox(new[] { InventoryItem.TeamASpawn1, InventoryItem.TeamBSpawn1, InventoryItem.TeamASpawn2, InventoryItem.TeamBSpawn2, InventoryItem.TeamASpawn3, InventoryItem.TeamBSpawn3, InventoryItem.GoldBlock });
            }
            else if (MinerOfDuty.editor.gameMode == LobbyCode.GameModes.CustomSM)
            {
                items[8] = new InventoryItemBox(new[] { InventoryItem.Spawn1, InventoryItem.ZombieSpawn1, InventoryItem.ZombieSpawn2, InventoryItem.ZombieSpawn3, InventoryItem.ZombieSpawn4, InventoryItem.ZombieSpawn5, InventoryItem.ZombieSpawn6, InventoryItem.GoldBlock });
            }
            else if(MinerOfDuty.editor.gameMode == LobbyCode.GameModes.CustomKB)
            {
                items[8] = new InventoryItemBox(new[] { InventoryItem.KingSpawn, InventoryItem.Spawn1, InventoryItem.Spawn2, InventoryItem.Spawn3, InventoryItem.Spawn4, InventoryItem.Spawn5, InventoryItem.Spawn6, });
            }
            selectedIndex = 0;
        }

        private int flash;
        private GamePadState oldState;
        public void Update(GameTime gameTime, ref GamePadState gamePad)
        {
            flash += gameTime.ElapsedGameTime.Milliseconds;
            if (flash >= 550)
                flash = 0;


            if (delay > 0)
                delay -= gameTime.ElapsedGameTime.Milliseconds;

            if (Input.WasButtonPressed(Buttons.LeftShoulder, ref oldState, ref gamePad))
            {
                if (--selectedIndex < 0)
                    selectedIndex = items.Length - 1;

                delay = 0;
            }
            else if (Input.WasButtonPressed(Buttons.RightShoulder, ref oldState, ref gamePad))
            {
                if (++selectedIndex >= items.Length)
                    selectedIndex = 0;

                delay = 0;
            }

            if (delay <= 0)
            {
                if (Input.IsDPad(Input.Direction.Up, ref gamePad))
                {
                    delay = 150;
                    flash = 0;
                    items[selectedIndex].Up();
                }
                else if (Input.IsDPad(Input.Direction.Down, ref gamePad))
                {
                    delay = 150;
                    flash = 0;
                    items[selectedIndex].Down();
                }
            }

            oldState = gamePad;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.SelectionMenu9, new Vector2(174, 570), Color.White);
            for (int i = 0; i < items.Length; i++)
                if (items[i].GetItem != InventoryItem.Empty)
                    sb.Draw(Resources.ItemTextures[items[i].GetItem], new Vector2(174 + i * (75), 570), Color.White);
            sb.Draw(Resources.SelectedBox, new Vector2(174 + 3f + (selectedIndex * (74.75f)), 573f), Color.White);

            if(flash < 250)
            if (items[selectedIndex].IsMultiple)
            {
                sb.DrawString(Resources.NameFont, "^", new Vector2(174 + 3f + (selectedIndex * (75)) + 25, 545), Color.White);

                sb.DrawString(Resources.NameFont, "^", new Vector2(174 + 3f + (selectedIndex * (75)) + 25, 628), Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
            }

            sb.DrawString(Resources.Font, Inventory.GetItemAsString(GetSelectedItem), 
                new Vector2(174, 570 - Resources.Font.LineSpacing * 1.5f), Color.White);

        }
    }
}
