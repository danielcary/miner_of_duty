using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Miner_Of_Duty.Game.Networking;
using Microsoft.Xna.Framework.Net;
using Miner_Of_Duty.LobbyCode;

namespace Miner_Of_Duty.Game
{
    public class Player 
    {
        public enum Stance { Standing = 0, Crouching = 1, Prone = 2 }
        private Stance dontusethistance = Stance.Standing;
        public Stance stance
        {
            get { return dontusethistance; }
            set
            {
                if (dontusethistance == Stance.Prone)
                    bb.Max = bb.Max + new Vector3(0, 1.2f, 0);
                dontusethistance = value;
                if (value == Stance.Prone)
                    bb.Max = bb.Max - new Vector3(0, 1.2f, 0);
            }
        }

        public byte PlayerID;
        private MultiplayerGame game;
        public FPSCamera Camera { get; set; }
        public Vector3 position;
        public BoundingBox bb;
        private BoundingBox HitDetection;
        public Inventory inventory;
        public ArmAnimation armAnimation;
        public Terrain.UnderLiquid underLiquid;
        private BoundingSphere boundingSphere;
        public bool dead = false;

        public float health = 100;
        private float waterHealth = 100;

        private const float speed = 5.89f;

        private float CurrentBlockDmgDealt = -15;

        public void SetPosition()
        {
           // stance = Stance.Standing;
            Vector3 pos = position;
            switch (stance)
            {
                case Stance.Standing:
                    pos -= new Vector3(0, 1.4f, 0);
                    break;
                case Stance.Crouching:
                    pos -= new Vector3(0, .475f, 0);
                    break;
                case Stance.Prone:
                    pos -= new Vector3(0, .8f, 0);
                    break;
            }

            HitDetection = new BoundingBox(pos - new Vector3(.27f, 0f, .27f), pos + new Vector3(.27f, 1.8f, .27f));
            HitDetection.Max += new Vector3(1, 1, 1);
            HitDetection.Min -= new Vector3(1, 1, 1);
            boundingSphere.Center = position;
        }

        public void Respawn(ref Vector3 pos)
        {
            stance = Stance.Standing;
            position = pos + new Vector3(0, 1.4f, 0);
            distanceFromGround = new Vector3(0, 1.4f, 0);
            bb = new BoundingBox(pos - new Vector3(.27f, 0f, .27f), pos + new Vector3(.27f, 1.8f, .27f));
            health = 100;
            waterHealth = 100;
            leftRightRot = 0;
            upDownRot = 0;
            leftTriggerDown = false;
            HoldingAGrenadeDown = false;
            launchAnotherGrenade = 20;
            armAnimation.LauchGrenade();
            if(classIUse != null)
                SetWeapons(classIUse);
            healSpan = new TimeSpan(0, 0, 5);
            ClearGamePad();
        }

        public bool thickSkin = false;
        private bool moreStamina = false;

        private CharacterClass classIUse;
        public void SetWeapons(CharacterClass clas)
        {
            inventory = new Inventory();

            classIUse = clas;

            if (clas.MoreStamina)
            {
                thickSkin = false;
                moreStamina = true;
                Gun.ReloadFaster = false;
            }
            else if (clas.QuickHands)
            {
                Gun.ReloadFaster = true;
                thickSkin = false;
                moreStamina = false;
            }
            else if (clas.ThickerSkin)
            {
                thickSkin = true;
                moreStamina = false;
                Gun.ReloadFaster = false;
            }

            if (clas.Slot1 is WeaponSlot)
            {
                WeaponSlot s = clas.Slot1 as WeaponSlot;
                inventory.AddGun(new Gun(s.GunID, BurstFire, s.MoreAmmo, s.ExtendedMags, s.BallisticTip)); 
            }
            else if (clas.Slot1 is ToolSlot)
            {
                ToolSlot s = clas.Slot1 as ToolSlot;
                inventory.AddTool(new Tool(s.ToolTypeID, s.SharperEdges, s.DurableConstruction));
            }

            if (clas.Slot2 is WeaponSlot)
            {
                WeaponSlot s = clas.Slot2 as WeaponSlot;
                inventory.AddGun(new Gun(s.GunID, BurstFire, s.MoreAmmo, s.ExtendedMags, s.BallisticTip));
            }
            else if (clas.Slot2 is ToolSlot)
            {
                ToolSlot s = clas.Slot2 as ToolSlot;
                inventory.AddTool(new Tool(s.ToolTypeID, s.SharperEdges, s.DurableConstruction));
            }

            ItemSlot ss = clas.Slot3 as ItemSlot;
            if (ss.Item == InventoryItem.Goggles)
                inventory.AddGoggle();
            else
                inventory.AddItem(ss.Item);

            ss = clas.Slot4 as ItemSlot;
            if (ss.Item == InventoryItem.Goggles)
                inventory.AddGoggle();
            else
                inventory.AddItem(ss.Item);

            inventory.SetLethalGrenade(new Grenade(clas.LethalGrenadeID, GrenadeCooked));
            if (game is SwarmGame)
            {
                inventory.SetSpecialGrenade(new Grenade(clas.SpecialGrenadeID, GrenadeCooked));
                inventory.GetSpecialGrenade.Throw();
                inventory.GetSpecialGrenade.Throw();
                inventory.GetSpecialGrenade.Throw();
            }
            else
                inventory.SetSpecialGrenade(new Grenade(clas.SpecialGrenadeID, GrenadeCooked));

        }

        private bool isSwarmMode = false;
        private TimeSpan healSpan;
        public Player(MultiplayerGame game, Vector3 pos, GraphicsDevice gd, byte playerId)
        {
            this.game = game;
            this.PlayerID = playerId;
            position = pos + new Vector3(0,1.4f,0);
            bb = new BoundingBox(pos - new Vector3(.27f, 0f, .27f), pos + new Vector3(.27f, 1.8f, .27f));
            //if above is changed change packet
            distanceFromGround = new Vector3(0, 1.4f, 0);
            boundingSphere = new BoundingSphere();

            armAnimation = new ArmAnimation(gd);

            inventory = new Inventory();

            isSwarmMode = game is SwarmGame;
        }

        public float leftRightRot = 0, upDownRot = 0;
        public GamePadState oldState, newState;
        public bool leftTriggerDown = false;
        public float movingSpeed;

        public const float maxUpDownRot = 1.2f, minUpDownRot = -1.4f;
        public static float rotationSpeed = 1.4f;
        public static void SetRotSpeed(int sens)
        {
            if(sens == 10)
            {
                rotationSpeed = 8;
            }
            else if (sens == 9)
            {
                rotationSpeed = 7f;
            }
            else if (sens == 8)
            {
                rotationSpeed = 6f;
            }
            else if (sens == 7)
            {
                rotationSpeed = 4.5f;
            }
            else if (sens == 6)
            {
                rotationSpeed = 3.4f;
            }
            else if (sens == 5)
            {
                rotationSpeed = 3f;
            }
            else if (sens == 4)
            {
                rotationSpeed = 2.4f;
            }
            else if (sens == 3)
            {
                rotationSpeed = 1.8f;
            }
            else if (sens == 2)
            {
                rotationSpeed = 1.4f;
            }
            else if (sens == 1)
            {
                rotationSpeed = 1.1f;
            }
        }

        private Vector3i selected;
        private bool onGround = true;
        private Vector3 translation, dir, dir2;
        private BoundingBox test;
        private Ray blockSelectRay;
        private Vector3 block = Vector3.Zero;

        private static readonly Vector3 MaxVelocity = new Vector3(2, 4f, 2);
        private static readonly Vector3 MinVelocity = new Vector3(-2, -4f, -2);
        private Vector3 velocity = Vector3.Zero;
        private float timeInAir;
        private bool jumping = false;
        private int millisecondsInAir;
        private Vector2 amountMoved = Vector2.Zero;
        private bool wasRightTrigDown = false;
        private int timeBeenRunning;
        private int timeBWasDown = 0;

        public bool HoldingAGrenadeDown;
        public byte GrenadeID;

        private void GrenadeCooked(Grenade sender)
        {
            Packet.WriteGrenadeThrown(game.Me, position - (distanceFromGround * .25f), sender.GrenadeID, leftRightRot, upDownRot, (int)sender.time);
            game.grenadeManager.AddGrenade(position - (distanceFromGround * .25f), sender.GrenadeID, leftRightRot, upDownRot, (int)sender.time, game.Me);
            armAnimation.LauchGrenade();
            launchAnotherGrenade = 650;
            HoldingAGrenadeDown = false;
        }

        public void GrenadeHurt(float dmg, NetworkGamer gamer)
        {
            if (useInvincibleityYeahBitchSpeellingISForLosers)
                return;

            health -= dmg;
            if (health <= 0)
            {
                dead = true;
                game.timeTillRespawn = MultiplayerGame.respawntime + game.TeamManager.GetSpawnDelay();
                Packet.WriteIDiedPacket(game.Me, gamer.Gamertag, gamer.Id, 69, KillText.DeathType.Grenade);
                if (game is SwarmGame)
                    (game.TeamManager as SwarmManager).AddDeath(PlayerID);
                game.SetDeathCamOnMe();
            }
            else
                Hurt();
        }

        int timeInWhite;
        public void DrawWhiteScreen()
        {
            timeInWhite = 2200;
        }

        public bool useInvincibleityYeahBitchSpeellingISForLosers = false;
        public void Attack(float dmg)
        {
            if (useInvincibleityYeahBitchSpeellingISForLosers)
                return;

            health -= dmg;
            Hurt();
            if (health < 0)
            {
                dead = true;
                game.timeTillRespawn = MultiplayerGame.respawntime + game.TeamManager.GetSpawnDelay();
                Packet.WriteIDiedPacket(game.Me, "Zombie", 0, 69, KillText.DeathType.Zombie);
                (game.TeamManager as SwarmManager).AddDeath(PlayerID);
                game.SetDeathCamOnMe();
            }
        }

        public void Hurt()
        {
            healSpan = new TimeSpan(0, 0, 0, 0, 0);
        }

        private int knifeCooldown;
        private Vector3 distanceFromGround;
        public void DropWeapon()
        {
            if (Inventory.IsGun(inventory.GetSelectedItem))
            {
                if (inventory.GetSelectedGun.CurrentAmmo > 0)
                    Packet.WriteWeaponDropPacket(game.Me, position - distanceFromGround + new Vector3(0, .2f, 0), inventory.GetSelectedGun.GunTypeID, (short)inventory.GetSelectedGun.CurrentAmmo, inventory.GetSelectedGun.ExtendedMags, game.WeaponDropManager.GenerateID());
            }
        }

        private float GetSpeed()
        {
            if (stance == Stance.Crouching)
            {
                return speed * .55f * (underLiquid == Terrain.UnderLiquid.Water ? .5f : 1f);
            }
            else if (stance == Stance.Prone)
            {
                return speed * .35f * (underLiquid == Terrain.UnderLiquid.Water ? .5f : 1f);
            }
            else
            {
                if (newState.Buttons.LeftStick == ButtonState.Pressed && onGround && OutOfBreath() == false)
                    return speed * 1.6f * (underLiquid == Terrain.UnderLiquid.Water ? .65f : 1f);
                else
                    return speed * (underLiquid == Terrain.UnderLiquid.Water ? .5f : 1f);
            }
        }

        private int recoverTime = 0;
        private bool OutOfBreath()
        {
            if (moreStamina)
            {
                if (timeBeenRunning > 2000)
                    return true;
                else
                    return false;
            }
            else
            {
                if (timeBeenRunning > 1250)
                    return true;
                else
                    return false;
            }
        }

        public bool Knifing { get { return armAnimation.knifing; } }

        private void KnifeTime()
        {
            if (game is SwarmGame)
            {
                KnifeZombieTime();
                return;
            }

            Ray bulletRay = new Ray(Camera.Position, Vector3.Normalize(Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot))));
            float? result;

            byte[] shootables = game.TeamManager.GetKillablePlayers();
            Dictionary<int, float> results = new Dictionary<int, float>();

            MultiplayerGame g = game;
            for (int i = 0; i < shootables.Length; i++)
            {
                if (shootables[i] == game.Me.Id)
                    continue;

                if (g.players[shootables[i]].dead)
                    continue;

                g.players[shootables[i]].HitDetection.Intersects(ref bulletRay, out result);
                if (result.HasValue)
                {
                    if (result.Value < 1.25f)
                        results.Add(i, result.Value);
                }
            }


            if (results.Count > 0)
            {
                //there was possibly a hit
                results = (from entry in results
                           orderby entry.Value ascending
                           select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

                //we do this because our testhit box is a bit bigger than it should be
                int[] keys = results.Keys.ToArray();
                PlayerBody.Hit hit;
                for (int i = 0; i < keys.Length; i++)
                {

                    float range;
                    hit = g.playerBodies[shootables[keys[i]]].CheckForCollision(ref bulletRay, 69696969, out range);
                    if (hit != PlayerBody.Hit.None)
                    {
                        Packet.WriteKnife(game.Me, MinerOfDuty.Session.FindGamerById(shootables[keys[i]]));
                        break;
                    }

                }
            }
        }

        private void KnifeZombieTime()
        {
            Ray bulletRay = new Ray(Camera.Position, Vector3.Normalize(Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot))));
            float? result;

            //byte[] shootables = game.TeamManager.GetKillablePlayers();
            Dictionary<int, float> results = new Dictionary<int, float>();

            BoundingSphere bs = new BoundingSphere(Vector3.Zero, 1.45f);
            ISwarmie[] swarmies = (game as SwarmGame).SwarmManager.GetSwarmies();
            for (int i = 0; i < swarmies.Length; i++)
            {
                if (swarmies[i] == null)
                    continue;
                if (swarmies[i].Dead)
                    continue;

                bs.Center = swarmies[i].Position;
                bs.Intersects(ref bulletRay, out result);
                if (result.HasValue)
                {
                    if (result.Value < 1.6f)
                        results.Add(i, result.Value);
                }
            }


            if (results.Count > 0)
            {
                //there was possibly a hit
                results = (from entry in results
                           orderby entry.Value ascending
                           select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

                //we do this because our testhit box is a bit bigger than it should be
                int[] keys = results.Keys.ToArray();
                PlayerBody.Hit hit;
                for (int i = 0; i < keys.Length; i++)
                {

                    float range;
                    hit = swarmies[keys[i]].CheckForCollision(ref bulletRay, 69696969, out range);
                    if (hit != PlayerBody.Hit.None)
                    {
                        Packet.WriteSwarmieShotPacket(game.Me, 40, swarmies[keys[i]].ID, KillText.DeathType.Knife, hit);
                        break;
                    }

                }
            }
        }

        public void ClearGamePad()
        {
            oldState = Input.Empty;
            newState = Input.Empty;
        }

        private float autoAimTime = 0;
        private int timeI;
        public void DontUseInput()
        {
            timeI = 250;
        }

        public void Paused()
        {
            HoldingAGrenadeDown = false;
            launchAnotherGrenade = 0;
            armAnimation.LauchGrenade();
        }

        public bool IsUsingGoggles { get { return inventory.EquipedGoggles == null ? false : true; } }

        private int launchAnotherGrenade = 0;
        private int timeToMakeFoot = 0;
        public void Update(GameTime gameTime, GamePadState gamepadState)
        {

            if (updateMe == false)
                return;


            oldState = newState;
            newState = gamepadState;

            if (Input.WasButtonPressed(Buttons.DPadRight, ref oldState, ref newState) && SpecialGamer.IsDev(MinerOfDuty.Session.LocalGamers[0]))
                useInvincibleityYeahBitchSpeellingISForLosers = !useInvincibleityYeahBitchSpeellingISForLosers;


            timeInWhite -= gameTime.ElapsedGameTime.Milliseconds;

            if(onGround)
                if (timeToMakeFoot <= 0)
                {
                    Audio.PlaySound(Audio.SOUND_FOOTSTEP, MathHelper.Lerp(0, .8f, movingSpeed));
                    timeToMakeFoot += (int)MathHelper.Lerp(600, 300, movingSpeed);
                }
                else
                    timeToMakeFoot -= gameTime.ElapsedGameTime.Milliseconds;

            if (showHitMarker)
            {
                timeShowhitMarker += gameTime.ElapsedGameTime.Milliseconds;
                if (timeShowhitMarker > 175)
                    showHitMarker = false;
            }

            if (timeI > 0)
                timeI -= gameTime.ElapsedGameTime.Milliseconds;

            if (timeI <= 0 && Input.WasButtonPressed(Buttons.A, ref oldState, ref newState))
            {
                if (stance == Stance.Standing)
                    jumping = true;
                else
                {
                    if (stance == Stance.Crouching)
                    {
                        position.Y += .475f;
                    }
                    else if (stance == Stance.Prone)
                    {
                        position.Y += .8f;
                    }
                    Stance backUp = stance;
                    stance = Stance.Standing;
                    if (game.Terrain.CheckForCollision(ref bb, ref position))
                    {
                        stance = backUp;
                        if (stance == Stance.Crouching)
                        {
                            position.Y -= .475f;
                        }
                        else if (stance == Stance.Prone)
                        {
                            position.Y -= .8f;
                        }
                    }
                    else
                    {
                        distanceFromGround.Y = 1.4f;
                    }
                }
            }

            if (newState.Buttons.LeftStick == ButtonState.Pressed && onGround)
            {
                timeBeenRunning += gameTime.ElapsedGameTime.Milliseconds;
            }
            else if(newState.Buttons.LeftStick == ButtonState.Released)
            {
                if(timeBeenRunning > 0)
                    timeBeenRunning -= gameTime.ElapsedGameTime.Milliseconds;
            }

            if(OutOfBreath())
            {
                recoverTime += gameTime.ElapsedGameTime.Milliseconds;
                if (recoverTime > 1250)
                {
                    timeBeenRunning = 0;
                    recoverTime = 0;
                }
            }

            

            velocity.X += gamepadState.ThumbSticks.Left.X * GetSpeed() * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (jumping)
                velocity.Y += 1.5f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity
            else
                velocity.Y += -1f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity
            velocity.Z += -gamepadState.ThumbSticks.Left.Y * GetSpeed() * (float)gameTime.ElapsedGameTime.TotalSeconds;

            

            translation.X = MathHelper.Clamp(velocity.X, MinVelocity.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.X * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            velocity.X -= translation.X;

            translation.Y = MathHelper.Clamp(velocity.Y, MinVelocity.Y * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.Y * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            velocity.Y -= translation.Y;

            translation.Z = MathHelper.Clamp(velocity.Z, MinVelocity.Z * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaxVelocity.Z * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            velocity.Z -= translation.Z;


            //angle rotation
            leftRightRot += -gamepadState.ThumbSticks.Right.X * rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; ;
            upDownRot += MinerOfDuty.InvertY(gamepadState.ThumbSticks.Right.Y) * rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds; ;
            if (upDownRot > maxUpDownRot)
                upDownRot = maxUpDownRot;
            else if (upDownRot < minUpDownRot)
                upDownRot = minUpDownRot;

            Ray r = new Ray(position,-Vector3.UnitY);
            Vector3i outt;
            game.Terrain.GetSelectedBlock2(ref r, out outt);

            //Y check
            
            dir.X = 0;
            dir.Y = translation.Y;
            dir.Z = 0;
            test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
            float distanceSquared;
            Vector3.DistanceSquared(ref bb.Max, ref test.Max, out distanceSquared);
            if ( distanceSquared < 8 && 
                (!(game is SwarmGame) && !game.Terrain.CheckForCollision(ref test, ref position))
                ||
                (game is SwarmGame && !game.Terrain.CheckForCollision(ref test, ref position) && !((game as SwarmGame).SwarmManager.CheckForPlayerCollision(ref test, ref position)))
                )
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

                millisecondsInAir += gameTime.ElapsedGameTime.Milliseconds;
            }
            else
            {
                jumping = false;


                timeInAir = 0;

                if (millisecondsInAir > 600)
                {
                    if(useInvincibleityYeahBitchSpeellingISForLosers == false)
                    health -= (.000603f * (millisecondsInAir * millisecondsInAir)) + (-.7576184f * millisecondsInAir) + 252.864f;
                    if (health < 0)
                    {
                        dead = true;
                        game.timeTillRespawn = MultiplayerGame.respawntime + game.TeamManager.GetSpawnDelay();
                        DropWeapon();
                        Packet.WriteIDiedPacket(game.Me, "Gravity", 0, 69, KillText.DeathType.Fall);
                        game.SetDeathCamOnMe();
                    }
                }
                millisecondsInAir = 0;
            }


            translation = Vector3.Transform(translation, Matrix.CreateRotationY(leftRightRot));
            
            if (translation.X != 0)
            {
                dir.X = translation.X;
                dir.Y = 0;
                dir.Z = 0;
                test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
                Vector3.DistanceSquared(ref bb.Max, ref test.Max, out distanceSquared);
                if (distanceSquared < 8 && 
                    (!(game is SwarmGame) && !game.Terrain.CheckForCollision(ref test, ref position))
                    ||
                    (game is SwarmGame && !game.Terrain.CheckForCollision(ref test, ref position) && !((game as SwarmGame).SwarmManager.CheckForPlayerCollision(ref test, ref position)))
                    )
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
                dir.Z =  translation.Z;
                test = BoundingBox.CreateMerged(bb, new BoundingBox(bb.Min + dir, bb.Max + dir));
                Vector3.DistanceSquared(ref bb.Max, ref test.Max, out distanceSquared);
                if (distanceSquared < 8 && 
                    (!(game is SwarmGame) && !game.Terrain.CheckForCollision(ref test, ref position))
                    ||
                    (game is SwarmGame && !game.Terrain.CheckForCollision(ref test, ref position) && !((game as SwarmGame).SwarmManager.CheckForPlayerCollision(ref test, ref position)))
                    )
                {
                    bb.Min += dir;
                    bb.Max += dir;
                    amountMoved.X += dir.X;
                    amountMoved.Y += dir.Z;
                    position += dir;
                }
            }

            game.Terrain.CheckForPitFall(ref bb, ref position, ref distanceFromGround, gameTime.ElapsedGameTime.Milliseconds);
            
            movingSpeed = amountMoved.Length() / new Vector2(speed * (float)gameTime.ElapsedGameTime.TotalSeconds, speed * (float)gameTime.ElapsedGameTime.TotalSeconds).Length();

            amountMoved = Vector2.Zero;

            if (timeI <= 0 && Input.WasButtonPressed(Buttons.B, ref oldState, ref newState))
            {
                if (stance == Stance.Standing)
                {
                    position.Y -= .475f;
                    distanceFromGround.Y = 1.4f - .475f;
                    stance = Stance.Crouching;
                }
                else if(stance == Stance.Crouching)
                {
                    position.Y += .475f;
                    distanceFromGround.Y = 1.4f;
                    stance = Stance.Standing;

                    
                }
                else if (stance == Stance.Prone)
                {
                    position.Y += .8f;
                    position.Y -= .475f;
                    distanceFromGround.Y = 1.4f - .475f;
                    stance = Stance.Crouching;
                    if (game.Terrain.CheckForCollision(ref bb, ref position))
                    {
                        stance = Stance.Prone;
                        position.Y += .475f;
                        position.Y -= .8f;
                        distanceFromGround.Y = 1.4f - .8f;
                    }
                }
            }

            if(autoAimTime > 0)
                autoAimTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeI <= 0 && newState.Buttons.B == ButtonState.Pressed)
            {
                timeBWasDown += gameTime.ElapsedGameTime.Milliseconds;
                if (stance != Stance.Prone)
                    if (timeBWasDown >= 200)
                    {
                        timeBWasDown = 0;
                        if (stance == Stance.Crouching)
                        {
                            position.Y += .475f;
                        }
                        stance = Stance.Prone;
                        position.Y -= .8f;
                        distanceFromGround.Y = 1.4f - .8f;
                    }
            }
            else
                timeBWasDown = 0;

            boundingSphere.Center = position;

            if (Camera != null) //all from here shouldnt be movemnet
            {
                Camera.Update(leftRightRot, upDownRot, ref position);

                if (dead)
                    return;

                healSpan = healSpan.Add(gameTime.ElapsedGameTime);

                if (healSpan.TotalSeconds > 3)
                {
                    if (health < 100)
                    {
                        health += 35f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (waterHealth < 100)
                        {
                            health = MathHelper.Clamp(health + 35f * (float)gameTime.ElapsedGameTime.TotalSeconds, 0, 100);
                            waterHealth = MathHelper.Clamp(waterHealth + 35f * (float)gameTime.ElapsedGameTime.TotalSeconds, 0, 100);
                        }
                    }
                }


                //section
                inventory.Update(gameTime, ref gamepadState);

                Vector3i oldSelected = selected;

                if (Inventory.IsGun(inventory.GetSelectedItem) == false)
                {
                    blockSelectRay = new Ray(Camera.Position, Vector3.Normalize(Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot))));
                    game.Terrain.GetSelectedBlock(ref blockSelectRay, out selected, inventory.GetSelectedItem == InventoryItem.EmptyBucket);
                }
                else
                    selected = Vector3i.NULL;

                if (armAnimation.IsUsingAGrenade)
                    selected = Vector3i.NULL;

                if (oldSelected != selected)
                    CurrentBlockDmgDealt = -15;

                if (inventory.GetSelectedItem == InventoryItem.LavaBucket || inventory.GetSelectedItem == InventoryItem.WaterBucket || inventory.GetSelectedItem == InventoryItem.EmptyBucket)
                    CurrentBlockDmgDealt = -15;

                if (knifeCooldown <= 0)
                {
                    if (Input.WasButtonPressed(Buttons.RightStick, ref oldState, ref newState))
                    {
                        if (armAnimation.knifing == false && game.WeaponsEnabled && armAnimation.IsUsingAGrenade == false)
                        {
                            if (Inventory.IsGun(inventory.GetSelectedItem) && inventory.GetSelectedGun.CanReload)
                            {
                                armAnimation.Knife(KnifeTime);
                                knifeCooldown = 1100;
                            }
                            else if (Inventory.IsGun(inventory.GetSelectedItem) == false)
                            {
                                armAnimation.Knife(KnifeTime);
                                knifeCooldown = 1100;
                            }
                        }
                    }
                }
                else
                    knifeCooldown -= gameTime.ElapsedGameTime.Milliseconds;


                if (selected != Vector3i.NULL)
                {
                    if (Input.WasButtonPressed(Buttons.RightTrigger, ref oldState, ref gamepadState) && game.TeamManager.CanPlaceBlocks() && (inventory.GetSelectedItem == InventoryItem.LavaBucket || inventory.GetSelectedItem == InventoryItem.WaterBucket || inventory.GetSelectedItem == InventoryItem.EmptyBucket))
                    {
                        armAnimation.AttackSwing();

                        if (inventory.GetSelectedItem == InventoryItem.LavaBucket || inventory.GetSelectedItem == InventoryItem.WaterBucket)
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
                            if (block.Y < 62)
                            {
                                (game as MultiplayerGame).BlockChanged(ref block, inventory.GetSelectedItem == InventoryItem.LavaBucket ? Block.BLOCKID_WATER : Block.BLOCKID_LAVA, true);
                                inventory.ChangeSelectedTo(InventoryItem.EmptyBucket);
                                Audio.PlaySound(Audio.SOUND_PLACESOFT);
                            }
                        }
                        else
                        {
                            block.X = selected.X;
                            block.Y = selected.Y;
                            block.Z = selected.Z;
                            byte blockID = game.Terrain.blocks[(int)block.X, (int)block.Y, (int)block.Z];
                            if ((blockID == Block.BLOCKID_LAVA || blockID == Block.BLOCKID_WATER) && game.LiquidManager.Liquids[(int)block.X, (int)block.Y, (int)block.Z].liquidLevel == 10)
                            {
                                inventory.ChangeSelectedTo(blockID == Block.BLOCKID_LAVA ? InventoryItem.LavaBucket : InventoryItem.WaterBucket);
                                (game as MultiplayerGame).BlockChanged(ref block, Block.BLOCKID_AIR, true);
                            }
                            else
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
                                 blockID = game.Terrain.blocks[(int)block.X, (int)block.Y, (int)block.Z];
                                if ((blockID == Block.BLOCKID_LAVA || blockID == Block.BLOCKID_WATER) && game.LiquidManager.Liquids[(int)block.X, (int)block.Y, (int)block.Z].liquidLevel == 10)
                                {
                                    inventory.ChangeSelectedTo(blockID == Block.BLOCKID_LAVA ? InventoryItem.LavaBucket : InventoryItem.WaterBucket);
                                    (game as MultiplayerGame).BlockChanged(ref block, Block.BLOCKID_AIR, true);
                                    Audio.PlaySound(Audio.SOUND_PLACESOFT);
                                }
                            }
                        }
                    }
                    else if (gamepadState.Triggers.Right > .2f && Inventory.IsTool(inventory.GetSelectedItem))
                    {
                        bool goAhead = false;
                        if (game.Terrain.blocks[selected.X, selected.Y, selected.Z] == Block.BLOCKID_GOLD)
                            goAhead = game.TeamManager.CanMineGoldBlocks();
                        else
                            goAhead = true;

                        if (game.TeamManager.CanPlaceBlocks() && goAhead&& (game.TeamManager is FortWarsManager ? (game.TeamManager as FortWarsManager).IsBlockPlaceAbleHere((int)selected.X, (int)selected.Y, (int)selected.Z) : true))
                            if (Block.IsDestructible(game.Terrain.blocks[selected.X, selected.Y, selected.Z]))
                            {
                                if (Block.IsBlockHard(game.Terrain.blocks[selected.X, selected.Y, selected.Z]))
                                {
                                    CurrentBlockDmgDealt += inventory.GetSelectedTool.GetDamageTowardsHard() * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                }
                                else
                                {
                                    CurrentBlockDmgDealt += inventory.GetSelectedTool.GetDamageTowardsSoft() * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                }

                                if (armAnimation.GetInSwing == false)
                                {
                                    armAnimation.AttackSwing();
                                    if (Block.IsBlockHard(game.Terrain.blocks[selected.X, selected.Y, selected.Z]))
                                        Audio.PlaySound(Audio.SOUND_MINEHARD);
                                    else
                                        Audio.PlaySound(Audio.SOUND_MINESOFT);
                                }

                                if (CurrentBlockDmgDealt >= Block.BlockHardness(game.Terrain.blocks[selected.X, selected.Y, selected.Z]))
                                {
                                    inventory.GetSelectedTool.Use();
                                    CurrentBlockDmgDealt = -15;
                                    block.X = selected.X;
                                    block.Y = selected.Y;
                                    block.Z = selected.Z;
                                    (game as MultiplayerGame).BlockChanged(ref block, game.Terrain.blocks[selected.X, selected.Y, selected.Z], false);
                                    selected = Vector3i.NULL;
                                }
                            }
                    }
                    else if (Input.WasButtonPressed(Buttons.RightTrigger, ref oldState, ref gamepadState))
                    {
                        armAnimation.AttackSwing();

                        if (game.TeamManager.CanPlaceBlocks())
                        {
                            if (Inventory.IsItemBlock(inventory.GetSelectedItem) || inventory.GetSelectedItem == InventoryItem.LavaBucket || inventory.GetSelectedItem == InventoryItem.WaterBucket)
                            {
                                int val = Block.GetIntersectionSide(ref blockSelectRay, ref selected);
                                InventoryItem item = inventory.GetSelectedItem;
                                #region BlockPlacement=
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

                                if (block.Y < 62)
                                {

                                    game.Terrain.blockBox.Max = block + Block.halfVector;
                                    game.Terrain.blockBox.Min = block - Block.halfVector;

                                    bool hit = false;
                                    for (int i = 0; i < MinerOfDuty.Session.AllGamers.Count; i++)
                                    {
                                        if (game.playerBodies[MinerOfDuty.Session.AllGamers[i].Id].CheckForIntersection(ref game.Terrain.blockBox))
                                        {
                                            hit = true;
                                            break;
                                        }
                                    }

                                    if (!hit && (game.TeamManager is FortWarsManager ? (game.TeamManager as FortWarsManager).IsBlockPlaceAbleHere((int)block.X, (int)block.Y, (int)block.Z) : true))
                                    {
                                        if (game.TeamManager is SwarmManager)
                                        {
                                            if (Block.IsBlockHard(blockID))
                                            {
                                                if ((game as SwarmGame).SwarmManager.StoreMenu.Cash > 20)
                                                {
                                                    //buy block
                                                    (game as SwarmGame).SwarmManager.StoreMenu.Cash -= 20;
                                                    (game as SwarmGame).showPurchase = 500;
                                                    (game as SwarmGame).showError = 0;
                                                    if (!game.BlockChanged(ref block, blockID, true))
                                                    {
                                                        (game as SwarmGame).SwarmManager.StoreMenu.Cash += 20;
                                                        (game as SwarmGame).showPurchase = 0;
                                                    }

                                                    Audio.PlaySound(Audio.SOUND_PLACEHARD);
                                                }
                                                else
                                                {
                                                    //error
                                                    Audio.PlaySound(Audio.SOUND_UIERROR);

                                                    (game as SwarmGame).showPurchase = 0000;
                                                    (game as SwarmGame).showError = 500;
                                                }
                                            }
                                            else
                                            {
                                                game.BlockChanged(ref block, blockID, true);
                                                Audio.PlaySound(Audio.SOUND_PLACESOFT);
                                            }
                                        }
                                        else
                                        {
                                            

                                            game.BlockChanged(ref block, blockID, true);
                                            if (Block.IsBlockHard(blockID))
                                                Audio.PlaySound(Audio.SOUND_PLACEHARD);
                                            else
                                                Audio.PlaySound(Audio.SOUND_PLACESOFT);
                                        }
                                    }

                                    game.Terrain.blockBox.Max = Block.halfVector;
                                    game.Terrain.blockBox.Min = -Block.halfVector;
                                }
                            }
                                #endregion
                            game.CantPlaceBlockWarning = false;
                        }
                        else
                        {
                            game.CantPlaceBlockWarning = true;
                        }
                    }
                    else if (gamepadState.Triggers.Left > .2f && !Inventory.IsGun(inventory.GetSelectedItem) && (inventory.GetSelectedItem == InventoryItem.LavaBucket || inventory.GetSelectedItem == InventoryItem.WaterBucket || inventory.GetSelectedItem == InventoryItem.EmptyBucket) == false)
                    {
                        bool goAhead = false;
                        if (game.Terrain.blocks[selected.X, selected.Y, selected.Z] == Block.BLOCKID_GOLD)
                            goAhead = game.TeamManager.CanMineGoldBlocks();
                        else
                            goAhead = true;

                        if (goAhead)
                        {
                            if (game.TeamManager.CanPlaceBlocks())
                            {

                                if (Block.IsDestructible(game.Terrain.blocks[selected.X, selected.Y, selected.Z]) && (game.TeamManager is FortWarsManager ? (game.TeamManager as FortWarsManager).IsBlockPlaceAbleHere((int)selected.X, (int)selected.Y, (int)selected.Z) : true))
                                {
                                    if (Block.IsBlockHard(game.Terrain.blocks[selected.X, selected.Y, selected.Z]))
                                    {
                                        if (Inventory.IsItemBlock(inventory.GetSelectedItem) || inventory.GetSelectedItem == InventoryItem.Goggles)
                                            CurrentBlockDmgDealt += 35 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                        else
                                            CurrentBlockDmgDealt += inventory.GetSelectedTool.GetDamageTowardsHard() * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                    }
                                    else
                                    {
                                        if (Inventory.IsItemBlock(inventory.GetSelectedItem) || inventory.GetSelectedItem == InventoryItem.Goggles)
                                            CurrentBlockDmgDealt += 50 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                        else
                                            CurrentBlockDmgDealt += inventory.GetSelectedTool.GetDamageTowardsSoft() * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                    }

                                    if (armAnimation.GetInSwing == false)
                                    {
                                        armAnimation.AttackSwing();
                                        if (Block.IsBlockHard(game.Terrain.blocks[selected.X, selected.Y, selected.Z]))
                                            Audio.PlaySound(Audio.SOUND_MINEHARD);
                                        else
                                            Audio.PlaySound(Audio.SOUND_MINESOFT);
                                    }

                                    if (CurrentBlockDmgDealt >= Block.BlockHardness(game.Terrain.blocks[selected.X, selected.Y, selected.Z]))
                                    {
                                        if (Inventory.IsItemBlock(inventory.GetSelectedItem) == false && inventory.GetSelectedItem != InventoryItem.Goggles)
                                            inventory.GetSelectedTool.Use();
                                        CurrentBlockDmgDealt = -15;
                                        block.X = selected.X;
                                        block.Y = selected.Y;
                                        block.Z = selected.Z;
                                        (game as MultiplayerGame).BlockChanged(ref block, game.Terrain.blocks[selected.X, selected.Y, selected.Z], false);
                                        selected = Vector3i.NULL;
                                    }
                                }
                                game.CantPlaceBlockWarning = false;
                            }
                            else
                            {
                                game.CantPlaceBlockWarning = true;
                            }
                        }
                    }
                    else if (Inventory.IsTool(inventory.GetSelectedItem) && (gamepadState.Triggers.Left < .2f && gamepadState.Triggers.Right < .2f))
                        CurrentBlockDmgDealt = -15;
                    else if (Inventory.IsGun(inventory.GetSelectedItem) == false && Inventory.IsTool(inventory.GetSelectedItem) == false && gamepadState.Triggers.Left < .2f)
                        CurrentBlockDmgDealt = -15;

                }
                else if (Inventory.IsGun(inventory.GetSelectedItem) && armAnimation.IsUsingAGrenade == false)
                {
                    if((gamepadState.Triggers.Right > .2f && inventory.GetSelectedGun.GunType.FireType == GunType.FireTypeEnum.Auto)
                        || (gamepadState.Triggers.Right > .2f && !wasRightTrigDown))
                        if (inventory.GetSelectedGun.IsTimeToFire() && !inventory.GetSelectedGun.NeedsToReload() && armAnimation.knifing == false)
                        {
                            if (isSwarmMode == false)
                                FireWeapon(true);
                            else
                                FireWeaponSwarmMode(true);
                        }

                    if (Input.WasButtonPressed(Buttons.X, ref oldState, ref newState))
                    {
                        if (inventory.GetSelectedGun.CurrentAmmoInClip < inventory.GetSelectedGun.ClipSize && inventory.GetSelectedGun.CurrentAmmo > 0 && inventory.GetSelectedGun.CurrentAmmo - inventory.GetSelectedGun.CurrentAmmoInClip > 0)
                        {
                            if (inventory.GetSelectedGun.CanReload)
                            {
                                inventory.GetSelectedGun.Reload();
                                armAnimation.Reload(inventory.GetSelectedGun.GunType.ReloadLoaded);
                                Audio.PlaySound(Audio.SOUND_RELOAD);
                            }
                        }
                    }

                    if (inventory.GetSelectedGun.CurrentAmmoInClip == 0 && inventory.GetSelectedGun.CurrentAmmo > 0 && inventory.GetSelectedGun.CanReload)
                    {
                        inventory.GetSelectedGun.Reload();
                        armAnimation.Reload(inventory.GetSelectedGun.GunType.ReloadLoaded);
                        Audio.PlaySound(Audio.SOUND_RELOAD);
                    }
                }


                if(launchAnotherGrenade > 0)
                    launchAnotherGrenade -= gameTime.ElapsedGameTime.Milliseconds;

                if (inventory.GetLethalGrenade != null && inventory.GetLethalGrenade.AmountOfGrenades > 0 )
                {
                    if (newState.Buttons.Y == ButtonState.Pressed && launchAnotherGrenade <= 0)
                    {
                        HoldingAGrenadeDown = true;
                        GrenadeID = inventory.GetLethalGrenade.GrenadeID;
                        inventory.GetLethalGrenade.RBIsDown(gameTime);
                        armAnimation.GrenadeHoldDown(inventory.GetLethalGrenade);
                    }

                    if (Input.WasButtonReleased(Buttons.Y, ref oldState, ref newState) && launchAnotherGrenade <= 0)
                    {
                        Packet.WriteGrenadeThrown(game.Me, position - (distanceFromGround * .25f), inventory.GetLethalGrenade.GrenadeID, leftRightRot, upDownRot, (int)inventory.GetLethalGrenade.time);
                        game.grenadeManager.AddGrenade(position - (distanceFromGround * .25f), inventory.GetLethalGrenade.GrenadeID, leftRightRot, upDownRot, (int)inventory.GetLethalGrenade.time, game.Me);
                        inventory.GetLethalGrenade.Throw();
                        armAnimation.LauchGrenade();
                        launchAnotherGrenade = 650;
                        HoldingAGrenadeDown = false;
                    }
                }
                if (inventory.GetSpecialGrenade != null && inventory.GetSpecialGrenade.AmountOfGrenades > 0)
                {
                    if (newState.DPad.Left == ButtonState.Pressed && launchAnotherGrenade <= 0)
                    {
                        HoldingAGrenadeDown = true;
                        GrenadeID = inventory.GetSpecialGrenade.GrenadeID;
                        inventory.GetSpecialGrenade.RBIsDown(gameTime);
                        armAnimation.GrenadeHoldDown(inventory.GetSpecialGrenade);
                    }

                    if (Input.WasButtonReleased(Buttons.DPadLeft, ref oldState, ref newState) && launchAnotherGrenade <= 0)
                    {
                        Packet.WriteGrenadeThrown(game.Me, position - (distanceFromGround * .25f), inventory.GetSpecialGrenade.GrenadeID, leftRightRot, upDownRot, (int)inventory.GetSpecialGrenade.time);
                        game.grenadeManager.AddGrenade(position - (distanceFromGround * .25f), inventory.GetSpecialGrenade.GrenadeID, leftRightRot, upDownRot, (int)inventory.GetSpecialGrenade.time, game.Me);
                        inventory.GetSpecialGrenade.Throw();
                        armAnimation.LauchGrenade();
                        launchAnotherGrenade = 650;
                        HoldingAGrenadeDown = false;
                    }
                }

                if (gamepadState.Triggers.Right < .1f)
                    wasRightTrigDown = false;


              //  if (timeInLava <= 0)
              //  {
                if (health > 0)
                    if (game.Terrain.IsInLiquid(ref bb) == Terrain.UnderLiquid.Lava && useInvincibleityYeahBitchSpeellingISForLosers == false)
                    {
                        
                        health -= 100 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        Hurt();
                      //  Packet.WritePlayerShotPacket((game as MultiplayerGame).Me, 13, (game as MultiplayerGame).Me.Id, KillText.DeathType.Lava);
                       // timeInLava = 75;
                        if (health < 0)
                        {
                            dead = true;
                            game.timeTillRespawn = MultiplayerGame.respawntime + game.TeamManager.GetSpawnDelay();
                            DropWeapon();
                            Packet.WriteIDiedPacket(game.Me, "Lava", 0, 69, KillText.DeathType.Lava);
                            game.SetDeathCamOnMe();
                        }
                    }

                if (underLiquid == Terrain.UnderLiquid.Water && useInvincibleityYeahBitchSpeellingISForLosers == false)
                {
                    health -= 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    waterHealth -= 10 * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Hurt();
                    //  Packet.WritePlayerShotPacket((game as MultiplayerGame).Me, 13, (game as MultiplayerGame).Me.Id, KillText.DeathType.Lava);
                    // timeInLava = 75;
                    if (health < 0)
                    {
                        dead = true;
                        game.timeTillRespawn = MultiplayerGame.respawntime + game.TeamManager.GetSpawnDelay();
                        DropWeapon();
                        Packet.WriteIDiedPacket(game.Me, "Water", 0, 69, KillText.DeathType.Water);
                        game.SetDeathCamOnMe();
                    }
                }
                else
                {
                    if(waterHealth < 70)
                        healSpan = healSpan.Add(new TimeSpan(0, 0, 3));
                }

             //   }
              //  else
               //     timeInLava -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                
                if (autoAimTime <= 0 && Inventory.IsGun(inventory.GetSelectedItem) && isLeftDown == false && gamepadState.Triggers.Left > .2f)
                {
                    AutoAim();
                    autoAimTime = 200;
                }


                canChangeWeapon = false;
                if (Inventory.IsGun(inventory.GetSelectedItem))
                {
                    
                    drop = game.WeaponDropManager.CheckForPickup(ref position);
                    if (drop != null)
                    {
                        if (drop.WeaponID == inventory.GetSelectedGun.GunTypeID && drop is WeaponDropManager.WeaponDrop)
                        {
                            if (Vector3.Distance(drop.Position, position - new Vector3(0, 1.4f, 0)) < .9f)
                                if (inventory.GetSelectedGun.CurrentAmmo + (inventory.GetSelectedGun.ClipSize - inventory.GetSelectedGun.CurrentAmmoInClip) < inventory.GetSelectedGun.GunType.MaxAmmo && (drop as WeaponDropManager.WeaponDrop).AmmoLeft > 0)
                                {
                                    inventory.GetSelectedGun.AddAmmo((drop as WeaponDropManager.WeaponDrop).TakeAmmo(inventory.GetSelectedGun.CurrentAmmo + (inventory.GetSelectedGun.ClipSize - inventory.GetSelectedGun.CurrentAmmoInClip)));
                                    Packet.WriteWeaponDropAmmoChange(game.Me, drop.ID, (short)(drop as WeaponDropManager.WeaponDrop).AmmoLeft);
                                }
                        }
                        else
                        {
                            if (newState.Buttons.X == ButtonState.Pressed)
                                holdTime += gameTime.ElapsedGameTime.Milliseconds;
                            else
                                holdTime = 0;

                            if (holdTime > 350)
                            {
                                Gun gun = drop.CreateWeapon(BurstFire);
                                if (drop is WeaponDropManager.WeaponDrop)
                                {
                                    (drop as WeaponDropManager.WeaponDrop).SwitchOut(inventory.GetSelectedGun.GunTypeID, inventory.GetSelectedGun.CurrentAmmo, false, drop.ID);
                                    Packet.WriteWeaponDropSwitch(game.Me, drop.ID, inventory.GetSelectedGun.GunTypeID, (short)inventory.GetSelectedGun.CurrentAmmo, false);
                                }
                                else
                                {
                                    (drop as WeaponDropManager.Spawner).WeaponTaken();
                                    Packet.WriteWeaponSpawnerTaken(game.Me, drop as WeaponDropManager.Spawner);
                                }
                                inventory.ChangeSelectedGun(gun);
                                holdTime = 0;

                            }
                            else
                            {
                                canChangeWeapon = true;
                                gunName = Inventory.GetItemAsString(Inventory.GunIDToInventoryItem(drop.WeaponID));
                            }
                        }
                    }
                }

                if (inventory.GetSelectedItem == InventoryItem.Goggles)
                {
                    if (Input.WasButtonPressed(Buttons.X, ref oldState, ref newState))
                    {
                        if (inventory.GetSelectedGoggle.IsEquiped == false && inventory.GetSelectedGoggle.IsEmpty == false)
                            inventory.EquipGoggles();
                        else if (inventory.GetSelectedGoggle.IsEmpty == false)
                            inventory.UnEquipGoggles();
                    }
                }
                
                isLeftDown = leftTriggerDown = gamepadState.Triggers.Left > .2f;
                if (Inventory.IsGun(inventory.GetSelectedItem) && inventory.GetSelectedGun.CanReload == false)
                    isLeftDown = leftTriggerDown = false;
                armAnimation.Update(gameTime, upDownRot, leftRightRot, inventory.GetSelectedItem, ref position, isLeftDown);
                underLiquid = game.Terrain.IsUnderLiquid(ref position);
            }
        }
        private WeaponDropManager.WeaponPickupable drop;

        private void AutoAim()
        {
            if (game is SwarmGame)
            {
                AutoAimZombie();
                return;
            }

            //auto aim
            Ray testRay = new Ray(Camera.Position, Vector3.Normalize(Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot))));
            MultiplayerGame g = game as MultiplayerGame;
            float distance;

            byte[] shootables = game.TeamManager.GetKillablePlayers();
            int bestIndex = -1; float bestVal = 69696969; float? val;

            float? blockCollision = game.Terrain.BulletIntersection(ref testRay);

            for (int i = 0; i < shootables.Length; i++)
            {
                if (shootables[i] == game.Me.Id)
                    continue;

                if (g.players[shootables[i]].dead)
                    continue;

                Vector3.Distance(ref g.players[shootables[i]].position, ref position, out distance);

                distance = .06f * distance + 1; //new size

                g.players[shootables[i]].boundingSphere.Radius = distance;
                g.players[shootables[i]].boundingSphere.Intersects(ref testRay, out val);

                if (val.HasValue)
                    if (val.Value < bestVal)
                    {
                        bestVal = val.Value;
                        bestIndex = i;
                    }
            }

            bool goAhead = true;

            if (blockCollision.HasValue)
                if (blockCollision.Value < bestVal)
                    goAhead = false;

            if (goAhead && bestIndex != -1)
            {
                //now roatate us to them 

                //left right
                Vector2 us = new Vector2(position.X, position.Z), them = new Vector2(g.players[shootables[bestIndex]].position.X, g.players[shootables[bestIndex]].position.Z);
                Vector2 dir;
                Vector2.Subtract(ref them, ref us, out dir);
                dir.Normalize();
                float xRot = (float)Math.Acos(dir.X);
                float yRot = (float)Math.Asin(dir.Y);
                if (yRot < 0)
                    xRot *= -1;
                leftRightRot = -(xRot - MathHelper.Pi - MathHelper.PiOver2);
                //  Console.WriteLine(MathHelper.ToDegrees(xRot));

                //upDpwn
                dir.X = Vector2.Distance(us, them);
                dir.Y = (g.players[shootables[bestIndex]].position.Y - .5f) - position.Y;
                dir.Normalize();
                xRot = (float)Math.Acos(dir.X);
                yRot = (float)Math.Asin(dir.Y);
                if (yRot < 0)
                    xRot *= -1;
                upDownRot = MathHelper.Clamp(xRot, minUpDownRot, maxUpDownRot);
            }
        }

        private void AutoAimZombie()
        {
            //auto aim
            Ray testRay = new Ray(Camera.Position, Vector3.Normalize(Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot))));
            ISwarmie[] swarmies = (game as SwarmGame).SwarmManager.GetSwarmies();
            float distance;

            int bestIndex = -1; float bestVal = 69696969; float? val;

            float? blockCollision = game.Terrain.BulletIntersection(ref testRay);
            BoundingSphere bs = new BoundingSphere(Vector3.Zero, 1);
            for (int i = 0; i < swarmies.Length; i++)
            {
                if (swarmies[i] == null)
                    continue;
                if (swarmies[i].Dead)
                    continue;

                Vector3.Distance(ref swarmies[i].Position, ref position, out distance);

                distance = .06f * distance + 1; //new size

                bs.Center = swarmies[i].Position + new Vector3(0, .4f, 0);
                bs.Radius = distance;
                bs.Intersects(ref testRay, out val);

                if (val.HasValue)
                    if (val.Value < bestVal)
                    {
                        bestVal = val.Value;
                        bestIndex = i;
                    }
            }

            bool goAhead = true;

            if (blockCollision.HasValue)
                if (blockCollision.Value < bestVal)
                    goAhead = false;

            if (goAhead && bestIndex != -1)
            {
                //now roatate us to them 

                //left right
                Vector2 us = new Vector2(position.X, position.Z), them = new Vector2(swarmies[bestIndex].Position.X, swarmies[bestIndex].Position.Z);
                Vector2 dir;
                Vector2.Subtract(ref them, ref us, out dir);
                dir.Normalize();
                float xRot = (float)Math.Acos(dir.X);
                float yRot = (float)Math.Asin(dir.Y);
                if (yRot < 0)
                    xRot *= -1;
                leftRightRot = -(xRot - MathHelper.Pi - MathHelper.PiOver2);
                //  Console.WriteLine(MathHelper.ToDegrees(xRot));

                //upDpwn
                dir.X = Vector2.Distance(us, them);
                dir.Y = (swarmies[bestIndex].Position.Y + .5f) - position.Y;
                dir.Normalize();
                xRot = (float)Math.Acos(dir.X);
                yRot = (float)Math.Asin(dir.Y);
                if (yRot < 0)
                    xRot *= -1;
                upDownRot = MathHelper.Clamp(xRot, minUpDownRot, maxUpDownRot);
            }
        }
        
        private bool showHitMarker;
        private int timeShowhitMarker;
        private static Vector2 offDir;
        private void FireWeapon(bool fireGun)
        {
            wasRightTrigDown = true;

            armAnimation.AddRecoil(inventory.GetSelectedGun.GunType.Recoil, inventory.GetSelectedGun.GunTypeID);
            if(fireGun)
                inventory.GetSelectedGun.Fire();

            //add bullet fired
            MinerOfDuty.CurrentPlayerProfile.AddBulletFired(game.type, inventory.GetSelectedGun.GunTypeID, Lobby.IsPrivateLobby());


            float accuracy = 0;
            if(inventory.GetSelectedGun.GunType.Accuracy == GunType.Accuarcy.Low)
                accuracy = 1.75f * (2f/ 3f);
            else if(inventory.GetSelectedGun.GunType.Accuracy == GunType.Accuarcy.Medium)
                accuracy = 1.25f * (2f / 3f);
            else
                accuracy = .8f * (2f / 3f);

            offDir = new Vector2(game.PersonalRandom.Next(-10, 11), game.PersonalRandom.Next(-10, 11));
            offDir.Normalize();

            if (isLeftDown)
            {
                offDir *= accuracy / 3f;
            }
            else
            {
                offDir *= accuracy;
            }

            Ray bulletRay = new Ray(Camera.Position, Vector3.Normalize(Vector3.Transform(Vector3.Forward, 
                Matrix.CreateRotationX(upDownRot + MathHelper.ToRadians(armAnimation.RecoilDegrees * .4f) + MathHelper.ToRadians(offDir.X))
                * Matrix.CreateRotationY(leftRightRot + MathHelper.ToRadians(offDir.Y)))));
            float? result;

            byte[] shootables = game.TeamManager.GetKillablePlayers();
            Dictionary<int, float> results = new Dictionary<int, float>();

            MultiplayerGame g = game;
            for (int i = 0; i < shootables.Length; i++)
            {
                if (shootables[i] == game.Me.Id)
                    continue;

                if (g.players[shootables[i]].dead)
                    continue;

                g.players[shootables[i]].HitDetection.Intersects(ref bulletRay, out result);
                if (result.HasValue)
                {
                    results.Add(i, result.Value);
                }
            }



            if (results.Count > 0)
            {
                //check blocks for collision
                float? blockCollision = game.Terrain.BulletIntersection(ref bulletRay);

                //there was possibly a hit
                results = (from entry in results
                           orderby entry.Value ascending
                           select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

                //we do this because our testhit box is a bit bigger than it should be

                bool hitSomeone = false;

                int[] keys = results.Keys.ToArray();
                PlayerBody.Hit hit;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (blockCollision.HasValue)
                        if (blockCollision.Value < results[keys[i]])
                            break;
                    float range;
                    hit = g.playerBodies[shootables[keys[i]]].CheckForCollision(ref bulletRay, blockCollision.HasValue ? blockCollision.Value : 69696969, out range);
                    if (hit != PlayerBody.Hit.None)
                    {
                        GunType gt = inventory.GetSelectedGun.GunType;
                        if (inventory.GetSelectedGun.GunTypeID != GunType.GUNID_SWORD)
                        {
                            Packet.WriteBulletFired(g.Me,
                                ref bulletRay,
                                inventory.GetSelectedGun.GunTypeID, range);
                            game.EffectsManager.AddBulletStreak(ref bulletRay, range);
                        }

                        MinerOfDuty.CurrentPlayerProfile.AddGunHit(game.type, inventory.GetSelectedGun.GunTypeID, Lobby.IsPrivateLobby());

                        Packet.WritePlayerShotPacket(g.Me,
                            inventory.GetSelectedGun.GetDamage(range) * (hit == PlayerBody.Hit.Arm || hit == PlayerBody.Hit.Leg ? gt.LimbsDmgMultiplier : hit == PlayerBody.Hit.Body ? gt.BodyDmgMultiplier : gt.HeadDmgMultiplier),
                            shootables[keys[i]],
                            inventory.GetSelectedGun.GunTypeID,
                            ref position,
                            (hit == PlayerBody.Hit.Head ? KillText.DeathType.HeadShot : KillText.DeathType.Normal));
                        showHitMarker = true;
                        timeShowhitMarker = 0;
                        hitSomeone = true;
                        break;
                    }
                }

                if (hitSomeone == false)
                {
                    if (blockCollision.HasValue)
                    {
                        if (inventory.GetSelectedGun.GunTypeID != GunType.GUNID_SWORD)
                        {
                            Packet.WriteBulletFired(g.Me,
                                ref bulletRay,
                                inventory.GetSelectedGun.GunTypeID, blockCollision.Value);
                            game.EffectsManager.AddBulletStreak(ref bulletRay, blockCollision.Value);
                        }
                    }
                    else
                    {
                        if (inventory.GetSelectedGun.GunTypeID != GunType.GUNID_SWORD)
                        {
                            Packet.WriteBulletFired(g.Me,
                                ref bulletRay,
                                inventory.GetSelectedGun.GunTypeID, 256);
                            game.EffectsManager.AddBulletStreak(ref bulletRay, 256);
                        }
                    }
                }
            }
            else
            {
                //it didnt hit no one, not even the larger test boxes but we still need a bullet visual
                float? blockCollision = game.Terrain.BulletIntersection(ref bulletRay);
                if (blockCollision.HasValue)
                {
                    if (inventory.GetSelectedGun.GunTypeID != GunType.GUNID_SWORD)
                    {
                        Packet.WriteBulletFired(g.Me,
                            ref bulletRay,
                            inventory.GetSelectedGun.GunTypeID, blockCollision.Value);
                        game.EffectsManager.AddBulletStreak(ref bulletRay, blockCollision.Value);
                    }
                }
                else
                {
                    if (inventory.GetSelectedGun.GunTypeID != GunType.GUNID_SWORD)
                    {
                        Packet.WriteBulletFired(g.Me,
                            ref bulletRay,
                            inventory.GetSelectedGun.GunTypeID, 256);
                        game.EffectsManager.AddBulletStreak(ref bulletRay, 256);
                    }
                }

            }
        }

        private void FireWeaponSwarmMode(bool fireGun)
        {
            wasRightTrigDown = true;

            armAnimation.AddRecoil(inventory.GetSelectedGun.GunType.Recoil, inventory.GetSelectedGun.GunTypeID);
            if(fireGun)
                inventory.GetSelectedGun.Fire();

            float accuracy = 0;
            if (inventory.GetSelectedGun.GunType.Accuracy == GunType.Accuarcy.Low)
                accuracy = 1.75f * (2f / 3f);
            else if (inventory.GetSelectedGun.GunType.Accuracy == GunType.Accuarcy.Medium)
                accuracy = 1.25f * (2f / 3f);
            else
                accuracy = .8f * (2f / 3f);

            offDir = new Vector2(game.PersonalRandom.Next(-10, 11), game.PersonalRandom.Next(-10, 11));
            offDir.Normalize();

            if (isLeftDown)
            {
                offDir *= accuracy / 3f;
            }
            else
            {
                offDir *= accuracy;
            }

            Ray bulletRay = new Ray(Camera.Position, Vector3.Normalize(Vector3.Transform(Vector3.Forward,
                Matrix.CreateRotationX(upDownRot + MathHelper.ToRadians(armAnimation.RecoilDegrees * .4f) + MathHelper.ToRadians(offDir.X))
                * Matrix.CreateRotationY(leftRightRot + MathHelper.ToRadians(offDir.Y)))));
            float? result;

            //byte[] shootables = game.TeamManager.GetKillablePlayers();
            Dictionary<int, float> results = new Dictionary<int, float>();

            BoundingSphere bs = new BoundingSphere(Vector3.Zero, 1.25f);
            ISwarmie[] swarmies = (game as SwarmGame).SwarmManager.GetSwarmies();
            for (int i = 0; i < swarmies.Length; i++)
            {
                if (swarmies[i] == null)
                    continue;
                if (swarmies[i].Dead)
                    continue;

                bs.Center = swarmies[i].Position;
                bs.Intersects(ref bulletRay, out result);
                if (result.HasValue)
                {
                    results.Add(i, result.Value);
                }
            }



            if (results.Count > 0)
            {
                //check blocks for collision
                float? blockCollision = game.Terrain.BulletIntersection(ref bulletRay);

                //there was possibly a hit
                results = (from entry in results
                           orderby entry.Value ascending
                           select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

                //we do this because our testhit box is a bit bigger than it should be

                bool hitSomeone = false;

                int[] keys = results.Keys.ToArray();
                PlayerBody.Hit hit;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (blockCollision.HasValue)
                        if (blockCollision.Value < results[keys[i]])
                            break;
                    float range;
                    hit = swarmies[keys[i]].CheckForCollision(ref bulletRay, blockCollision.HasValue ? blockCollision.Value : 69696969, out range);
                    if (hit != PlayerBody.Hit.None)
                    {
                        GunType gt = inventory.GetSelectedGun.GunType;
                        Packet.WriteBulletFired(game.Me,
                            ref bulletRay,
                            inventory.GetSelectedGun.GunTypeID, range);
                        game.EffectsManager.AddBulletStreak(ref bulletRay, range);
                        Packet.WriteSwarmieShotPacket(game.Me,
                            inventory.GetSelectedGun.GetDamage(range) * (hit == PlayerBody.Hit.Arm || hit == PlayerBody.Hit.Leg ? gt.LimbsDmgMultiplier : hit == PlayerBody.Hit.Body ? gt.BodyDmgMultiplier : gt.HeadDmgMultiplier),
                            swarmies[keys[i]].ID, hit == PlayerBody.Hit.Head ? KillText.DeathType.HeadShot : KillText.DeathType.Normal, hit);

                        showHitMarker = true;
                        timeShowhitMarker = 0;
                        hitSomeone = true;
                        break;
                    }
                }

                if (hitSomeone == false)
                {
                    if (blockCollision.HasValue)
                    {
                        Packet.WriteBulletFired(game.Me,
                            ref bulletRay,
                            inventory.GetSelectedGun.GunTypeID, blockCollision.Value);
                        game.EffectsManager.AddBulletStreak(ref bulletRay, blockCollision.Value);
                    }
                    else
                    {
                        Packet.WriteBulletFired(game.Me,
                            ref bulletRay,
                            inventory.GetSelectedGun.GunTypeID, 256);
                        game.EffectsManager.AddBulletStreak(ref bulletRay, 256);
                    }
                }
            }
            else
            {
                //it didnt hit no one, not even the larger test boxes but we still need a bullet visual
                float? blockCollision = game.Terrain.BulletIntersection(ref bulletRay);
                if (blockCollision.HasValue)
                {
                    Packet.WriteBulletFired(game.Me,
                        ref bulletRay,
                            inventory.GetSelectedGun.GunTypeID, blockCollision.Value);
                    game.EffectsManager.AddBulletStreak(ref bulletRay, blockCollision.Value);
                }
                else
                {
                    Packet.WriteBulletFired(game.Me,
                        ref bulletRay,
                            inventory.GetSelectedGun.GunTypeID, 256);
                    game.EffectsManager.AddBulletStreak(ref bulletRay, 256);
                }

            }
        }

        private void BurstFireSwarmMode(Gun sender)
        {
            FireWeaponSwarmMode(false);
        }

        public void BurstFire(Gun sender)
        {
            if (isSwarmMode)
            {
                BurstFireSwarmMode(sender);
                return;
            }
            
            FireWeapon(false);
        }

        private int holdTime;
        private VertexTextureLight[] vertices = new VertexTextureLight[Block.VERTEXLENGTH];
        public void Render(GraphicsDevice gd)
        {
            if (selected != Vector3i.NULL)
            {
                gd.SetVertexBuffer(Resources.SelectionBuffer);
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["NoLight"];
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(selected.X, selected.Y, selected.Z));
                Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.SELECTIONTEXTURE);
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);

                if (CurrentBlockDmgDealt > 0)
                {
                    switch ((int)(CurrentBlockDmgDealt * 10 / Block.BlockHardness(game.Terrain.blocks[selected.X, selected.Y, selected.Z])))
                    {
                        case 1:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[0]);
                            break;
                        case 2:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[1]);
                            break;
                        case 3:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[2]);
                            break;
                        case 4:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[3]);
                            break;
                        case 5:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[4]);
                            break;
                        case 6:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[5]);
                            break;
                        case 7:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[6]);
                            break;
                        case 8:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[7]);
                            break;
                        case 9:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[8]);
                            break;
                        case 10:
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BlockDestroyTextures[9]);
                            break;
                    }

                    Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                    gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
                }

                gd.SetVertexBuffer(null);
            }
           
        }

        public static double speeda;
        private bool isLeftDown = false;

        public void RenderArm()
        {
            armAnimation.Render(Camera);
        }

        public bool updateMe = true;
        private bool canChangeWeapon = false;
        private string gunName;
        private readonly Rectangle fullscreen = new Rectangle(0, 0, 1280, 720);
        public void Draw(SpriteBatch sb)
        {

            if (showHitMarker && inventory.GetSelectedItem != InventoryItem.Sword)
                sb.Draw(Resources.HitMarker, new Vector2(640, 360), null, Color.White, 0, new Vector2(15, 15), 1, SpriteEffects.None, 0);

            sb.Draw(armAnimation.RenderTarget, Vector2.Zero, Color.White);

            if (underLiquid == Terrain.UnderLiquid.Lava)
            {
                sb.Draw(Resources.UnderLava, fullscreen, Color.White);
            }
            else if (underLiquid == Terrain.UnderLiquid.Water)
            {
                sb.Draw(Resources.UnderWater, fullscreen, Color.White);
            }

            if (timeInWhite > 0)
            {
                float amount = 1;
                if (timeInWhite > 300)
                    amount = (float)(.05f * Math.Cos((Math.PI * 2 / 800) * timeInWhite) + .95f);
                else
                    amount = MathHelper.Lerp(1, 0, 1 - (timeInWhite / 300f));

                sb.Draw(Resources.WhiteScreen, new Rectangle(0, 0, 1280, 720), new Color(amount, amount, amount, amount));
            }

            float num = MathHelper.Clamp(1f - (health / 80f), 0, 1);
            sb.Draw(Resources.HurtTexture, Vector2.Zero, new Color(num, num, num, num));

            if (HoldingAGrenadeDown && GrenadeID == GrenadeType.GRENADE_FRAG)
            {
                float amount = (float)(2.5f * Math.Cos((Math.PI * 2 / 800) * inventory.GetLethalGrenade.time) + 2.5f);
                DrawLines(sb, 10 + (int)amount);
            }
            else if (Inventory.IsGun(inventory.GetSelectedItem))
            {
                if (!isLeftDown)
                    DrawLines(sb, inventory.GetSelectedGun.GunType.Accuracy == GunType.Accuarcy.Medium ? 30 : inventory.GetSelectedGun.GunType.Accuracy == GunType.Accuarcy.High ? 22 : 40);
            }
            else
                DrawLines(sb, 8);

            if (MinerOfDuty.game.DrawHud)
            {
                inventory.Draw(sb);

                if (canChangeWeapon)
                {
                    sb.DrawString(Resources.NameFont, "Press X to Swap for ".ToUpper() + gunName, new Vector2(640, 450), Color.White, 0, Resources.NameFont.MeasureString("Press X to Swap for ".ToUpper() + gunName) / 2f, 1, SpriteEffects.None, 0);
                }
            }
        }

        public void DrawLines(SpriteBatch sb, int distanceFromCenter)
        {
            sb.Draw(Resources.HorizontalLineTexture, new Vector2(640 - Resources.HorizontalLineTexture.Width - distanceFromCenter, 360 - (Resources.HorizontalLineTexture.Height / 2)), Color.White);
            sb.Draw(Resources.HorizontalLineTexture, new Vector2(640 + distanceFromCenter, 360 - (Resources.HorizontalLineTexture.Height / 2)), Color.White);

            sb.Draw(Resources.VerticalLineTexture, new Vector2(640 - (Resources.VerticalLineTexture.Width / 2f), 360 - Resources.VerticalLineTexture.Height - distanceFromCenter), Color.White);
            sb.Draw(Resources.VerticalLineTexture, new Vector2(640 - (Resources.VerticalLineTexture.Width / 2f), 360 + distanceFromCenter), Color.White);
        }
    }

    public class GhostPlayer
    {

    }
}


