using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.Game.Networking;

namespace Miner_Of_Duty.Game.Editor
{
    public class PlayerEditor
    {
        private Player.Stance dontusethistance = Player.Stance.Standing;
        public Player.Stance stance
        {
            get { return dontusethistance; }
            set
            {
                if (dontusethistance == Player.Stance.Prone)
                    bb.Max = bb.Max + new Vector3(0, 1.2f, 0);
                dontusethistance = value;
                if (value == Player.Stance.Prone)
                    bb.Max = bb.Max - new Vector3(0, 1.2f, 0);
            }
        }
        public FPSCamera Camera { get; set; }
        public Vector3 position;
        private BoundingBox bb;
        public float leftRightRot = 0, upDownRot = 0;
        public GamePadState oldState, newState;
        public bool isSneaking = false;
        public bool leftTriggerDown = false;
        public float movingSpeed;
        private static readonly Vector3 MaxVelocity = new Vector3(2, 4f, 2);
        private static readonly Vector3 MinVelocity = new Vector3(-2, -4f, -2);
        private Vector3 velocity = Vector3.Zero;
        private float timeInAir;
        private bool jumping = false;
        private Vector2 amountMoved = Vector2.Zero;
        private int timeBWasDown = 0;
        private int timeToMakeFoot = 0;
        private Vector3i selected;
        private bool onGround = true;
        private Vector3 translation, dir, dir2;
        private BoundingBox test;
        private Ray blockSelectRay;
        private Vector3 block = Vector3.Zero;
        private const float speed = 7;
        private ITerrainOwner game;
        public EditorInventory inventory;
        public ArmAnimation armAnimation;
        public Terrain.UnderLiquid underLiquid;
        private GraphicsDevice gd;
        private int showError;
        private int showError2;

        public void SetPosition()
        {
            // stance = Stance.Standing;
            Vector3 pos = position;
            switch (stance)
            {
                case Player.Stance.Standing:
                    pos -= new Vector3(0, 1.4f, 0);
                    break;
                case Player.Stance.Crouching:
                    pos -= new Vector3(0, .475f, 0);
                    break;
                case Player.Stance.Prone:
                    pos -= new Vector3(0, .8f, 0);
                    break;
            }

        }

        private float GetSpeed()
        {
            if (stance == Player.Stance.Crouching)
            {
                return speed * .55f;
            }
            else if (stance == Player.Stance.Prone)
            {
                return speed * .35f;
            }
            else
            {
                if (newState.Buttons.LeftStick == ButtonState.Pressed && (onGround == true || gravity == false))
                    return speed * 1.75f;
                else
                    return speed;
            }
        }

        public PlayerEditor(ITerrainOwner game, Vector3 pos, GraphicsDevice gd, int size)
        {
            this.game = game;
            this.gd = gd;
            position = pos + new Vector3(0, 1.4f, 0);
            bb = new BoundingBox(pos - new Vector3(.27f, 0f, .27f), pos + new Vector3(.27f, 1.8f, .27f));
            inventory = new EditorInventory();
            armAnimation = new ArmAnimation(gd);

            int half = (128 - size) / 2;
            int other = half + size;
            world = new BoundingBox(new Vector3(half, 0, half), new Vector3(other, 1000, other));
        }

        public BoundingBox world;

        private int timeI;
        public void DontUseInput()
        {
            timeI = 250;
            oldState = Input.Empty;
            newState = Input.Empty;
        }

        private bool gravity = true;

        public bool ViewOnly = false;

        public void Update(GameTime gameTime, GamePadState gamepadState)
        {
            oldState = newState;
            newState = gamepadState;

            if(showError > 0)
                showError -= gameTime.ElapsedGameTime.Milliseconds;

            if (showError2 > 0)
                showError2 -= gameTime.ElapsedGameTime.Milliseconds;

            if (Input.WasButtonPressed(Buttons.Y, ref oldState, ref newState))
                gravity = !gravity;

            if (onGround)
                if (timeToMakeFoot <= 0)
                {
                    Audio.PlaySound(Audio.SOUND_FOOTSTEP, MathHelper.Lerp(0, .8f, movingSpeed));
                    timeToMakeFoot += (int)MathHelper.Lerp(600, 300, movingSpeed);
                }
                else
                    timeToMakeFoot -= gameTime.ElapsedGameTime.Milliseconds;


            if (timeI > 0)
                timeI -= gameTime.ElapsedGameTime.Milliseconds;

            if (timeI <= 0 && Input.WasButtonPressed(Buttons.A, ref oldState, ref newState))
            {
                if (stance == Player.Stance.Standing)
                    jumping = true;
                else
                {
                    if (stance == Player.Stance.Crouching)
                    {
                        position.Y += .475f;
                    }
                    else if (stance == Player.Stance.Prone)
                    {
                        position.Y += .8f;
                    }
                    Player.Stance backUp = stance;
                    stance = Player.Stance.Standing;
                    if (game.GetTerrain.CheckForCollision(ref bb, ref position))
                    {
                        stance = backUp;
                        if (stance == Player.Stance.Crouching)
                        {
                            position.Y -= .475f;
                        }
                        else if (stance == Player.Stance.Prone)
                        {
                            position.Y -= .8f;
                        }
                    }
                }
            }

            velocity.X += gamepadState.ThumbSticks.Left.X * GetSpeed() * (float)gameTime.ElapsedGameTime.TotalSeconds;
           
            if (jumping)
                velocity.Y += 1.5f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity
            else 
                velocity.Y += -1f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity

            if (gravity == false)
            {
                onGround = true;
                velocity.Y += 1f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (velocity.Y < 0)
                {
                    timeInAir = 0;
                    jumping = false;
                    velocity.Y = 0;
                }
            }

            velocity.Z += -gamepadState.ThumbSticks.Left.Y * GetSpeed() * (float)gameTime.ElapsedGameTime.TotalSeconds;



            translation.X = MathHelper.Clamp(velocity.X, MinVelocity.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            velocity.X -= translation.X;


            translation.Y = MathHelper.Clamp(velocity.Y, MinVelocity.Y * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.Y * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            velocity.Y -= translation.Y;

            

            translation.Z = MathHelper.Clamp(velocity.Z, MinVelocity.Z * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.Z * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            velocity.Z -= translation.Z;


            //angle rotation
            leftRightRot += -gamepadState.ThumbSticks.Right.X * Player.rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; ;
            upDownRot += MinerOfDuty.InvertY(gamepadState.ThumbSticks.Right.Y) * Player.rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; ;
            if (upDownRot > Player.maxUpDownRot)
                upDownRot = Player.maxUpDownRot;
            else if (upDownRot < Player.minUpDownRot)
                upDownRot = Player.minUpDownRot;

            Ray r = new Ray(position, -Vector3.UnitY);
            Vector3i outt;
            game.GetTerrain.GetSelectedBlock2(ref r, out outt);

            //Y check

            
                dir.X = 0;
                dir.Y = translation.Y;
                dir.Z = 0;
                test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
                if (!game.GetTerrain.CheckForCollision(ref test, ref position))
                {
                    bb.Min += dir;
                    bb.Max += dir;
                    position += dir;
                    onGround = false;
                }
                else
                    onGround = true;
            

            if (onGround == false)
            {
                timeInAir += .5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (jumping)
                    velocity.Y += -.06f * speed * 3 * timeInAir; //gravity
                else
                    velocity.Y += -.06f * speed * timeInAir; //gravity


            }
            else
            {
                jumping = false;
                timeInAir = 0;
            }


            translation = Vector3.Transform(translation, Matrix.CreateRotationY(leftRightRot));

            if (translation.X != 0)
            {
                dir.X = translation.X;
                dir.Y = 0;
                dir.Z = 0;
                test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
                if (!game.GetTerrain.CheckForCollision(ref test, ref position) && world.Contains(test) == ContainmentType.Contains)
                {
                    bb.Min += dir;
                    bb.Max += dir;
                    amountMoved.X += dir.X;
                    amountMoved.Y += dir.Z;
                    position += dir;
                }
            }

            if (translation.Z != 0)
            {
                dir.X = 0;
                dir.Y = 0;
                dir.Z = translation.Z;
                test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
                if (!game.GetTerrain.CheckForCollision(ref test, ref position) && world.Contains(test) == ContainmentType.Contains)
                {
                    bb.Min += dir;
                    bb.Max += dir;
                    amountMoved.X += dir.X;
                    amountMoved.Y += dir.Z;
                    position += dir;
                }
            }

            movingSpeed = amountMoved.Length() / new Vector2(speed * (float)gameTime.ElapsedGameTime.TotalSeconds, speed * (float)gameTime.ElapsedGameTime.TotalSeconds).Length();

            amountMoved = Vector2.Zero;

            if (timeI <= 0 && Input.WasButtonPressed(Buttons.B, ref oldState, ref newState))
            {
                if (stance == Player.Stance.Standing)
                {
                    position.Y -= .475f;
                    stance = Player.Stance.Crouching;
                }
                else if (stance == Player.Stance.Crouching)
                {
                    position.Y += .475f;
                    stance = Player.Stance.Standing;
                }
                else if (stance == Player.Stance.Prone)
                {
                    position.Y += .8f;
                    position.Y -= .475f;
                    stance = Player.Stance.Crouching;
                    if (game.GetTerrain.CheckForCollision(ref bb, ref position))
                    {
                        stance = Player.Stance.Prone;
                        position.Y += .475f;
                        position.Y -= .8f;
                    }
                }
            }

            if (timeI <= 0 && newState.Buttons.B == ButtonState.Pressed)
            {
                timeBWasDown += gameTime.ElapsedGameTime.Milliseconds;
                if (stance != Player.Stance.Prone)
                    if (timeBWasDown >= 200)
                    {
                        timeBWasDown = 0;
                        if (stance == Player.Stance.Crouching)
                        {
                            position.Y += .475f;
                        }
                        stance = Player.Stance.Prone;
                        position.Y -= .8f;
                    }
            }
            else
                timeBWasDown = 0;

            Camera.Update(leftRightRot, upDownRot, ref position);

            if (ViewOnly)
            {
                selected = Vector3i.NULL;
                underLiquid = game.GetTerrain.IsUnderLiquid(ref position);
                return;
            }

            inventory.Update(gameTime, ref newState);

            

            blockSelectRay = new Ray(Camera.Position, Vector3.Normalize(Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot))));
            game.GetTerrain.GetSelectedBlock(ref blockSelectRay, out selected, false);


            if (selected != Vector3i.NULL)
            {
                if (Input.WasButtonPressed(Buttons.RightTrigger, ref oldState, ref newState) && (Inventory.IsItemBlock(inventory.GetSelectedItem) || inventory.GetSelectedItem == InventoryItem.LavaBucket || inventory.GetSelectedItem == InventoryItem.WaterBucket))
                {
                    armAnimation.AttackSwing();
                    #region BlockPlacement
                    int val = Block.GetIntersectionSide(ref blockSelectRay, ref selected);
                    InventoryItem item = inventory.GetSelectedItem;
                    byte blockID = Inventory.InventoryItemToID(item);
                    if (val == 4)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y + 1;
                        block.Z = selected.Z;
                    }
                    else if (val == 3)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y - 1;
                        block.Z = selected.Z;
                    }
                    else if (val == 1)
                    {
                        block.X = selected.X - 1;
                        block.Y = selected.Y;
                        block.Z = selected.Z;
                    }
                    else if (val == 2)
                    {
                        block.X = selected.X + 1;
                        block.Y = selected.Y;
                        block.Z = selected.Z;
                    }
                    else if (val == 5)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y;
                        block.Z = selected.Z - 1;
                    }
                    else
                    {
                        block.X = selected.X;
                        block.Y = selected.Y;
                        block.Z = selected.Z + 1;
                    }

                    if (block.Y < 62 && block.X >= 0 && block.X < Terrain.MAXWIDTH && block.Z >= 0 && block.Z < Terrain.MAXWIDTH)
                    {
                        game.GetTerrain.blockBox.Max = block + Block.halfVector;
                        game.GetTerrain.blockBox.Min = block - Block.halfVector;

                        bool result;

                        bb.Intersects(ref game.GetTerrain.blockBox, out result);

                        if (result == false && inventory.GetSelectedItem != InventoryItem.LavaBucket && inventory.GetSelectedItem != InventoryItem.WaterBucket)
                        {
                            result = !(game as WorldEditor).CanPlaceBlock(new Vector3i((int)block.X, (int)block.Y, (int)block.Z));
                            if (result == true)
                            {
                                Audio.PlaySound(Audio.SOUND_UIERROR);
                                showError2 = 2500;
                            }
                        }

                        if (!result)
                        {
                            if (inventory.GetSelectedItem != InventoryItem.LavaBucket && inventory.GetSelectedItem != InventoryItem.WaterBucket)
                            {
                                (game as WorldEditor).BlockChanged(ref block, blockID, true);
                                //   game.GetTerrain.AddBlocks((int)block.X, (int)block.Y, (int)block.Z, blockID);
                            }
                            else
                            {
                                if(inventory.GetSelectedItem == InventoryItem.LavaBucket)
                                    (game as WorldEditor).BlockChanged(ref block, Block.BLOCKID_LAVA, true);
                                else
                                    (game as WorldEditor).BlockChanged(ref block, Block.BLOCKID_WATER, true);
                                //game.GetLiquidManager.AddSourceLavaBlock((int)block.X, (int)block.Y, (int)block.Z);
                            }
                            if (Block.IsBlockHard(blockID))
                                Audio.PlaySound(Audio.SOUND_PLACEHARD);
                            else
                                Audio.PlaySound(Audio.SOUND_PLACESOFT);

                            showError2 = 0;
                        }

                        game.GetTerrain.blockBox.Max = Block.halfVector;
                        game.GetTerrain.blockBox.Min = -Block.halfVector;
                    }
                    #endregion
                }
                else if (Input.WasButtonPressed(Buttons.RightTrigger, ref oldState, ref newState) && Inventory.IsSpawn(inventory.GetSelectedItem))
                {
                    armAnimation.AttackSwing();
                    #region placement
                    int val = Block.GetIntersectionSide(ref blockSelectRay, ref selected);
                    InventoryItem item = inventory.GetSelectedItem;
                    byte blockID = Inventory.InventoryItemToID(item);
                    if (val == 4)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y + 1;
                        block.Z = selected.Z;
                    }
                    else if (val == 3)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y - 1;
                        block.Z = selected.Z;
                    }
                    else if (val == 1)
                    {
                        block.X = selected.X - 1;
                        block.Y = selected.Y;
                        block.Z = selected.Z;
                    }
                    else if (val == 2)
                    {
                        block.X = selected.X + 1;
                        block.Y = selected.Y;
                        block.Z = selected.Z;
                    }
                    else if (val == 5)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y;
                        block.Z = selected.Z - 1;
                    }
                    else
                    {
                        block.X = selected.X;
                        block.Y = selected.Y;
                        block.Z = selected.Z + 1;
                    }

                    if (block.Y < 64 && block.X >= 0 && block.X < Terrain.MAXWIDTH && block.Z >= 0 && block.Z < Terrain.MAXWIDTH)
                    {
                        InventoryItem itesm = inventory.GetSelectedItem;
                        if (itesm == InventoryItem.KingSpawn)
                            (game as WorldEditor).gameInfo.SetKingPoint(RangeMenu.GetSize(game.GetTerrain.Size), block);
                        else if (SpawnPoint.CalculateIfIsGoodSpawn(game.GetTerrain, new Vector3i((int)block.X, (int)block.Y, (int)block.Z)))
                        {
                           
                            if (itesm == InventoryItem.TeamASpawn1
                                || itesm == InventoryItem.TeamASpawn2
                                || itesm == InventoryItem.TeamASpawn3)
                            {
                                (game as WorldEditor).gameInfo.SetTeamASpawnPoint(Inventory.GetSpawnNumber(itesm) - 1, block);  //.SetTeamASpawnPoint(itesm == InventoryItem.TeamASpawn1 ? 0 : itesm == InventoryItem.TeamASpawn2 ? 1 : 2, block);
                            }
                            else if (itesm == InventoryItem.TeamBSpawn1
                                || itesm == InventoryItem.TeamBSpawn2
                                || itesm == InventoryItem.TeamBSpawn3)
                            {
                                (game as WorldEditor).gameInfo.SetTeamBSpawnPoint(Inventory.GetSpawnNumber(itesm) - 1, block); //.SetTeamBSpawnPoint(itesm == InventoryItem.TeamBSpawn1 ? 0 : itesm == InventoryItem.TeamBSpawn2 ? 1 : 2, block);
                            }
                            else if (itesm == InventoryItem.ZombieSpawn1
                                || itesm == InventoryItem.ZombieSpawn2
                                || itesm == InventoryItem.ZombieSpawn3
                                || itesm == InventoryItem.ZombieSpawn4
                                || itesm == InventoryItem.ZombieSpawn5
                                || itesm == InventoryItem.ZombieSpawn6)
                            {
                                (game as WorldEditor).gameInfo.SetZombieSpawn(Inventory.GetSpawnNumber(itesm) - 1, block);
                            }
                            else if (itesm == InventoryItem.Spawn1
                                || itesm == InventoryItem.Spawn2
                                || itesm == InventoryItem.Spawn3
                                || itesm == InventoryItem.Spawn4
                                || itesm == InventoryItem.Spawn5
                                || itesm == InventoryItem.Spawn6)
                            {
                                (game as WorldEditor).gameInfo.SetSpawnPoint(Inventory.GetSpawnNumber(itesm) - 1, block);
                            }

                            showError = 0;
                        }
                        else
                        {
                            Audio.PlaySound(Audio.SOUND_UIERROR);
                            showError = 2500;
                        }
                    }
                    #endregion

                }
                else if (Input.WasButtonPressed(Buttons.LeftTrigger, ref oldState, ref newState))
                {
                    armAnimation.AttackSwing();
                    block.X = selected.X;
                    block.Y = selected.Y;
                    block.Z = selected.Z;
                    if (Block.IsDestructible(game.GetTerrain.blocks[(int)block.X, (int)block.Y, (int)block.Z]))
                    {
                        if (Block.IsBlockHard(game.GetTerrain.blocks[selected.X, selected.Y, selected.Z]))
                            Audio.PlaySound(Audio.SOUND_MINEHARD);
                        else
                            Audio.PlaySound(Audio.SOUND_MINESOFT);

                        (game as WorldEditor).BlockChanged(ref block, Block.BLOCKID_AIR, false);
                        //game.GetTerrain.RemoveBlocks((int)block.X, (int)block.Y, (int)block.Z);
                    }
                }
                else if (Inventory.IsGunSpawner(inventory.GetSelectedItem) )
                {
                    int val = Block.GetIntersectionSide(ref blockSelectRay, ref selected);
                    if (val == 4)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y + 1;
                        block.Z = selected.Z;
                    }
                    else if (val == 3)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y - 1;
                        block.Z = selected.Z;
                    }
                    else if (val == 1)
                    {
                        block.X = selected.X - 1;
                        block.Y = selected.Y;
                        block.Z = selected.Z;
                    }
                    else if (val == 2)
                    {
                        block.X = selected.X + 1;
                        block.Y = selected.Y;
                        block.Z = selected.Z;
                    }
                    else if (val == 5)
                    {
                        block.X = selected.X;
                        block.Y = selected.Y;
                        block.Z = selected.Z - 1;
                    }
                    else
                    {
                        block.X = selected.X;
                        block.Y = selected.Y;
                        block.Z = selected.Z + 1;
                    }

                    if (block.Y < 64 && block.X >= 0 && block.X < Terrain.MAXWIDTH && block.Z >= 0 && block.Z < Terrain.MAXWIDTH)
                    {
                        if (Block.IsBlockSolid(game.GetTerrain.blocks[(int)block.X, (int)block.Y - 1, (int)block.Z]))
                        {
                            if ((wd = (game as WorldEditor).editorWeaponDrop.CheckForPickup(ref block)) != null)
                            {
                                if (Input.WasButtonPressed(Buttons.RightTrigger, ref oldState, ref newState))
                                {
                                    Packet.WriteSpawnerChanged((game as WorldEditor).Me, block, inventory.GetSelectedItem, false);
                                   // (game as WorldEditor).editorWeaponDrop.RemoveWeaponDrop(wd);
                                    Audio.PlaySound(Audio.SOUND_PLACESOFT);
                                }
                            }
                            else
                            {
                                if (Input.WasButtonPressed(Buttons.RightTrigger, ref oldState, ref newState))
                                {
                                    Packet.WriteSpawnerChanged((game as WorldEditor).Me, block, inventory.GetSelectedItem, true);
                                   // (game as WorldEditor).editorWeaponDrop.AddSpawner(block, Inventory.InventoryItemToID(inventory.GetSelectedItem));
                                    Audio.PlaySound(Audio.SOUND_PLACEHARD);
                                }
                            }
                        }
                    }
                }
                else
                    wd = null;
            }
            else
                wd = null;
           
            
            armAnimation.Update(gameTime, upDownRot, leftRightRot, inventory.GetSelectedItem, ref position, gamepadState.Triggers.Left > .2f);
            underLiquid = game.GetTerrain.IsUnderLiquid(ref position);
        }
        private WeaponDropManager.WeaponPickupable wd;

        public void Render()
        {
            if (selected != Vector3i.NULL)
            {
                gd.SetVertexBuffer(Resources.SelectionBuffer);
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["NoLight"];
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(selected.X, selected.Y, selected.Z));
                Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.SELECTIONTEXTURE);
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
                gd.SetVertexBuffer(null);
            }
        }

        public void RenderArm()
        {
            armAnimation.Render(Camera);
        }


        private readonly Rectangle fullscreen = new Rectangle(0, 0, 1280, 720);
        public void Draw(SpriteBatch sb)
        {
            if(ViewOnly == false)
                sb.Draw(armAnimation.RenderTarget, Vector2.Zero, Color.White);

            if (underLiquid == Terrain.UnderLiquid.Lava)
            {
                sb.Draw(Resources.UnderLava, fullscreen, Color.White);
            }
            else if (underLiquid == Terrain.UnderLiquid.Water)
            {
                sb.Draw(Resources.UnderWater, fullscreen, Color.White);
            }

            DrawLines(sb, 8);

            if(ViewOnly == false)
                inventory.Draw(sb);

            sb.DrawString(Resources.Font, "GRAVITY: " + (gravity ? "ON" : "OFF"), new Vector2(175, 125), Color.White);
            sb.DrawString(Resources.DescriptionFont, "     (Y) TO SWITCH", new Vector2(175, 125 + Resources.Font.LineSpacing), Color.White);

            if (ViewOnly)
                sb.DrawString(Resources.Font, "VIEW ONLY MODE", new Vector2(640, 120), Color.Red, 0, Resources.Font.MeasureString("VIEW ONLY MODE") / 2f, 1, SpriteEffects.None, 0);

            if (ViewOnly)
                return;

            if (showError > 0)
            {
                sb.DrawString(Resources.Font, "CAN'T PLACE HERE", new Vector2(640, 320), Color.Red, 0, Resources.Font.MeasureString("CAN'T PLACE HERE") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.DescriptionFont, "BLOCKS ARE IN THE WAY", new Vector2(640, 320 + Resources.Font.LineSpacing * 1.2f), Color.Red, 0, Resources.DescriptionFont.MeasureString("BLOCKS ARE IN THE WAY") / 2f, 1, SpriteEffects.None, 0);
            }
            else if (showError2 > 0)
            {
                sb.DrawString(Resources.Font, "CAN'T PLACE HERE", new Vector2(640, 320), Color.Red, 0, Resources.Font.MeasureString("CAN'T PLACE HERE") / 2f, 1, SpriteEffects.None, 0);
                sb.DrawString(Resources.DescriptionFont, "SPAWN POINT IS IN THE WAY", new Vector2(640, 320 + Resources.Font.LineSpacing * 1.2f), Color.Red, 0, Resources.DescriptionFont.MeasureString("SPAWN POINT IS IN THE WAY") / 2f, 1, SpriteEffects.None, 0);
            }

            if(wd != null)
                sb.DrawString(Resources.NameFont, "PRESS RT TO REMOVE " + Inventory.GetItemAsString(Inventory.GunIDToInventoryItem(wd.WeaponID)) + " SPAWNER", new Vector2(640, 450), Color.White, 0,
                    Resources.NameFont.MeasureString("PRESS RT TO REMOVE " + Inventory.GetItemAsString(Inventory.GunIDToInventoryItem(wd.WeaponID)) + " SPAWNER") / 2f, 1, SpriteEffects.None, 0);
        }

        public void DrawLines(SpriteBatch sb, int distanceFromCenter)
        {
            sb.Draw(Resources.HorizontalLineTexture, new Vector2(640 - Resources.HorizontalLineTexture.Width - distanceFromCenter, 360 - (Resources.HorizontalLineTexture.Height / 2)), Color.White);
            sb.Draw(Resources.HorizontalLineTexture, new Vector2(640 + distanceFromCenter, 360 - (Resources.HorizontalLineTexture.Height / 2)), Color.White);

            sb.Draw(Resources.VerticalLineTexture, new Vector2(640 - (Resources.VerticalLineTexture.Width / 2f), 360 - Resources.VerticalLineTexture.Height - distanceFromCenter), Color.White);
            sb.Draw(Resources.VerticalLineTexture, new Vector2(640 - (Resources.VerticalLineTexture.Width / 2f), 360 + distanceFromCenter), Color.White);


        }

    }
}
