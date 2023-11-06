using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Miner_Of_Duty.Game
{
    public class ArmAnimation
    {
        public RenderTarget2D RenderTarget { get; private set; }
        private GraphicsDevice gd;
        private float speed;
        private long timeInMiliseconds;
        private InventoryItem selectedItem;
        private float upDownRot, leftRightRot;
        private Vector3 position;

        public ArmAnimation(GraphicsDevice gd)
        {
            this.gd = gd;
            RenderTarget = new RenderTarget2D(gd, gd.PresentationParameters.BackBufferWidth, gd.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);
        }

        public void Dispose()
        {
            if (RenderTarget != null && RenderTarget.IsDisposed == false)
                RenderTarget.Dispose();
        }


        /// <summary>
        /// Sets the moving speed of the character
        /// </summary>
        /// <param name="speed">Speed is percent of the how fast the character is moving to how fast it can move</param>
        public void SetSpeed(float speed)
        {
            this.speed = speed;
        }


        private Grenade holdingGrenade;
        public void Update(GameTime gameTime, float upDownRot, float leftRightRot, InventoryItem holdingItem, ref Vector3 pos, bool aiming)
        {
            this.upDownRot = upDownRot;
            this.leftRightRot = leftRightRot;

            if (drawFlareTime < 80)
                drawFlareTime += gameTime.ElapsedGameTime.Milliseconds;
            else
                drawFlare = false;

            if (selectedItem != holdingItem)
            {
                if (hitHalfway == false)
                {
                    changeToitem = holdingItem;
                    if (InSwing == false)
                        millisecondSinceSwing = 0;
                    InSwing = true;
                }
                else if(InSwing)
                {
                    selectedItem = holdingItem;
                    millisecondSinceSwing = 126;
                }
                recoilTime.Clear();
            }
            position = pos;
            timeInMiliseconds += gameTime.ElapsedGameTime.Milliseconds;

            for (int i = 0; i < recoilTime.Count; i++)
            {
                if ((recoilTime[i] += gameTime.ElapsedGameTime.Milliseconds) > 250)
                {
                    recoilTime.RemoveAt(i);
                    i--;
                }
            }
            RecoilDegrees = 0;

            for (int i = 0; i < recoilTime.Count; i++)
            {
                RecoilDegrees += amountOfRecoil * (float)((.00000019259259 * Math.Pow(recoilTime[i], 3)) + (-.0001265076 * Math.Pow(recoilTime[i], 2)) + (.0196534392 * recoilTime[i]) - .0436507937);
            }

            if (InSwing)
            {
                millisecondSinceSwing += gameTime.ElapsedGameTime.Milliseconds;

                if (millisecondSinceSwing > 250)
                {
                    InSwing = false;
                    hitHalfway = false;
                }
                else if (millisecondSinceSwing > 125 && !hitHalfway)
                {
                    selectedItem = changeToitem;
                    hitHalfway = true;
                }

            }

            if (attackSwing)
            {
                millisecondSinceAttackSwing += (int)(gameTime.ElapsedGameTime.Milliseconds * .8f);

                if (millisecondSinceAttackSwing > 175)
                {
                    attackSwing = false;
                }
            }

            this.aiming = aiming;

            if (aiming)
                if (millisecondInAim < 100)
                {
                    millisecondInAim += gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                    millisecondInAim = 100;
            else
                if (millisecondInAim > 0)
                {
                    millisecondInAim -= gameTime.ElapsedGameTime.Milliseconds;
                }
                else
                    millisecondInAim = 0;

            if (reloading)
            {
                reloadSwingTime += gameTime.ElapsedGameTime.Milliseconds;
                if (reloadSwingTime >= toReachReloadSwingTime)
                    reloading = false;
            }

            if (knifing)
            {
                timeInKnife += gameTime.ElapsedGameTime.Milliseconds;
                if (timeInKnife > 450)
                {
                    knifing = false;
                }
                if (timeInKnife > 125)
                {
                    if (killTime != null)
                    {
                        killTime.Invoke();
                        killTime = null;
                    }
                }
            }

            if (grenading)
            {


                if (holdingGrenade == null)
                {
                    timeInGrenadeHoldDown -= gameTime.ElapsedGameTime.Milliseconds;
                    if (timeInGrenadeHoldDown <= 0)
                        grenading = false;
                }
                else
                {
                    if (timeInGrenadeHoldDown < 400)
                        timeInGrenadeHoldDown += gameTime.ElapsedGameTime.Milliseconds;

                }
            }
        }


        private Matrix gunMatrix;
        private byte gunFired;
        private bool drawFlare = false;
        private int drawFlareTime = 110;

        //amount and time
        private List<int> recoilTime = new List<int>();
        private float amountOfRecoil = 0;
        public float RecoilDegrees { get; private set; }
        public void AddRecoil(float amount, byte gunFired)
        {
            amountOfRecoil = amount;
            recoilTime.Add(0);

            drawFlare = true;
            drawFlareTime = 0;
            this.gunFired = gunFired;
        }

        public void AttackSwing()
        {
            millisecondSinceAttackSwing = 0;
            attackSwing = true;
        }

        private int reloadSwingTime;
        private int toReachReloadSwingTime;
        private bool reloading = false;
        public void Reload(int reloadTime)
        {
            toReachReloadSwingTime = reloadTime;
            reloadSwingTime = 0;
            reloading = true;
        }

        private bool aiming = false;
        private int millisecondInAim = 0; // 1000 == all the way

        private bool attackSwing = false;
        private int millisecondSinceAttackSwing = 0;

        public delegate void KillTime();
        private int timeInKnife;
        public bool knifing;
        private KillTime killTime;        
        public void Knife(KillTime killTime)
        {
            timeInKnife = 0;
            knifing = true;
            this.killTime = killTime;
        }

        private int timeInGrenadeHoldDown;
        private bool grenading;
        public void GrenadeHoldDown(Grenade nade)
        {
            if (holdingGrenade == null)
            {
                holdingGrenade = nade;
                grenading = true;
                timeInGrenadeHoldDown = 0;
            }
        }

        public bool IsUsingAGrenade { get { return grenading; } }
        public void LauchGrenade()
        {
            holdingGrenade = null;
        }

        private InventoryItem changeToitem;
        private int millisecondSinceSwing = 0;
        private bool InSwing, hitHalfway;
        public bool GetInSwing { get { return attackSwing; } }

        public void Render(Camera camera)
        {
            gd.SetRenderTarget(RenderTarget);
            gd.Clear(Color.Transparent);

            if (knifing)
            {
                foreach (ModelMesh mesh in Resources.KnifeModel.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        Vector3 a = new Vector3(
                             .15f,
                             .0f,
                             -.4f);


                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["Texture0"].SetValue(Resources.KnifeTexture);
                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.3f) * Matrix.CreateRotationZ(MathHelper.PiOver4 * (2f/3f))
                            * Matrix.CreateTranslation(new Vector3(a.X, a.Y + -.075f * (float)Math.Sin((MathHelper.Pi / 450) * timeInKnife), a.Z - .2f * (float)Math.Sin((MathHelper.Pi / 450) * timeInKnife)))
                            * Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot) * Matrix.CreateTranslation(position));
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }
            else if (Inventory.IsSpawn(selectedItem))
            {
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["Light"];
                Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
                Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                Resources.BlockEffect.Parameters["Texture0"].SetValue(Inventory.GetHoldingBlockTextureForSpawn(selectedItem));

                Vector3 a = new Vector3(.25f, -.25f, -.55f);
                if (InSwing)
                {
                    a.Z -= (float)(.1f * Math.Cos((Math.PI * 2 / 250) * millisecondSinceSwing) - .1f);
                }
                if (attackSwing)
                {
                    a.Z += (float)(.15f * Math.Cos((Math.PI * 2 / 175) * millisecondSinceAttackSwing) - .15f);
                    a.X += (float)(.05f * Math.Cos((Math.PI * 2 / 175) * millisecondSinceAttackSwing) - .05f);
                }
                a.Normalize();
                a = a * .25f;
                double move = .01 * (float)Math.Sin((Math.PI * 2 / 1750) * timeInMiliseconds);

                a.X += (float)(move * .15);
                a.Y += (float)(move * .24);
                a.Z += (float)move;
                //first scale down the block, next rotate
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateScale(.0625f) * Matrix.CreateRotationX(MathHelper.ToRadians(90)) * Matrix.CreateRotationY(MathHelper.ToRadians(65)) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                    * Matrix.CreateTranslation(a)
                    * Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot) * Matrix.CreateTranslation(position));

                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.SetVertexBuffer(Resources.SelectionBuffer);
                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
            }
            else if (holdingGrenade != null)
            {
                foreach (ModelMesh mesh in (holdingGrenade.GrenadeID == GrenadeType.GRENADE_FRAG ? Resources.FragModel : Resources.SmokeFlashModel).Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        Vector3 a = new Vector3(
                             .25f,
                             -.3f,
                             -.8f);

                        if (InSwing)
                        {
                            a.Z -= (float)(.1f * Math.Cos((Math.PI * 2 / 250) * millisecondSinceSwing) - .1f);
                        }



                        float move = .025f * (float)Math.Sin((Math.PI * 2f / 1750f) * (float)timeInMiliseconds);
                        a.X += move * .15f;
                        a.Y += move * .24f;
                        a.Z += move;
                        float amount = MathHelper.Lerp(0, MathHelper.PiOver4, (float)timeInGrenadeHoldDown / 400f);

                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["Texture0"].SetValue(holdingGrenade.GrenadeID == GrenadeType.GRENADE_FRAG ? Resources.FragModelTexture : holdingGrenade.GrenadeID == GrenadeType.GRENADE_SMOKE
                            ? Resources.SmokeModelTexture : Resources.FlashModelTexture);
                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.8f) * Matrix.CreateRotationX(-MathHelper.PiOver2)
                            * Matrix.CreateTranslation(a + new Vector3(0, amount * .5f, amount)) 
                            * Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot) * Matrix.CreateTranslation(position));
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }
            else if (Inventory.IsItemBlock(selectedItem))
            {
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["Light"];
                Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
                Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BLOCKTEXTURES[Inventory.InventoryItemToID(selectedItem)]);

                Vector3 a = new Vector3(.25f, -.25f, -.55f);
                if (InSwing)
                {
                    a.Z -= (float)(.1f * Math.Cos((Math.PI * 2 / 250) * millisecondSinceSwing) - .1f);
                }
                if (attackSwing)
                {
                    a.Z += (float)(.15f * Math.Cos((Math.PI * 2 / 175) * millisecondSinceAttackSwing) - .15f);
                    a.X += (float)(.05f * Math.Cos((Math.PI * 2 / 175) * millisecondSinceAttackSwing) - .05f);
                }
                a.Normalize();
                a = a * .25f;
                double move = .01 * (float)Math.Sin((Math.PI * 2 / 1750) * timeInMiliseconds);

                a.X += (float)(move * .15);
                a.Y += (float)(move * .24);
                a.Z += (float)move;
                //first scale down the block, next rotate
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateScale(.0625f) * Matrix.CreateRotationX(MathHelper.ToRadians(90)) * Matrix.CreateRotationY(MathHelper.ToRadians(65)) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                    * Matrix.CreateTranslation(a)
                    * Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot) * Matrix.CreateTranslation(position));

                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.SetVertexBuffer(Resources.SelectionBuffer);
                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
            }
            else if (selectedItem == InventoryItem.LavaBucket || selectedItem == InventoryItem.EmptyBucket || selectedItem == InventoryItem.WaterBucket)
            {
                foreach (ModelMesh mesh in Resources.Bucket.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        Vector3 a = new Vector3(
                             .25f,
                             -.3f,
                             -.8f);

                        if (InSwing)
                        {
                            a.Z -= (float)(.1f * Math.Cos((Math.PI * 2 / 250) * millisecondSinceSwing) - .1f);
                        }

                        if (attackSwing)
                        {
                            a.Z += (float)(.15f * Math.Cos((Math.PI * 2 / 175) * millisecondSinceAttackSwing) - .15f);
                            a.X += (float)(.05f * Math.Cos((Math.PI * 2 / 175) * millisecondSinceAttackSwing) - .05f);
                        }

                        float move = .025f * (float)Math.Sin((Math.PI * 2f / 1750f) * (float)timeInMiliseconds);
                        a.X += move * .15f;
                        a.Y += move * .24f;
                        a.Z += move;

                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["Texture0"].SetValue(selectedItem == InventoryItem.EmptyBucket ? Resources.EmptyBucketTexture : selectedItem == InventoryItem.LavaBucket ? Resources.LavaBucketTexture : Resources.WaterBucketTexture);
                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.4f)
                            * Matrix.CreateTranslation(a)
                            * Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot) * Matrix.CreateTranslation(position));
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }
            else if (Inventory.IsGun(selectedItem))
            {
                foreach (ModelMesh mesh in Resources.GunModels[Inventory.InventoryItemToID(selectedItem)].Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        Vector3 a = new Vector3(
                            (-.00335f * millisecondInAim) + .25f,
                            (.0018f * millisecondInAim) - .25f,
                            (.0025f * millisecondInAim) - .75f);

                        if (InSwing)
                        {
                            a.Z -= 2 * (float)(.1f * Math.Cos((Math.PI * 2 / 250) * millisecondSinceSwing) - .1f);
                            if (aiming)
                                a.Y += 1.5f * (float)(.1f * Math.Cos((Math.PI * 2 / 250) * millisecondSinceSwing) - .1f);
                        }


                        if (aiming == false || reloading == true)
                        {
                            float move = .025f * (float)Math.Sin((Math.PI * 2f / 1750f) * (float)timeInMiliseconds);
                            a.X += move * .15f;
                            a.Y += move * .24f;
                            a.Z += move;
                        }

                        if (reloading)
                        {
                            float move = MathHelper.Clamp(.5f * (float)(Math.Sin((Math.PI / (float)toReachReloadSwingTime) * reloadSwingTime + Math.PI)), -.15f, 0);
                            if (move == -.15f)
                                move += .005f * (float)(Math.Sin((Math.PI / 200f) * reloadSwingTime));
                            a.Y += move;
                        }

                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["Texture0"].SetValue(Resources.GunModelTextures[Inventory.InventoryItemToID(selectedItem)]);
                        gunMatrix = Matrix.CreateScale(-.002f * millisecondInAim + .6f) * Matrix.CreateRotationY(MathHelper.ToRadians(87.5f)) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                            * Matrix.CreateRotationX(MathHelper.ToRadians(RecoilDegrees +
                            (reloading ? (MathHelper.Clamp(-40 * (float)(Math.Sin((Math.PI / (float)toReachReloadSwingTime) * reloadSwingTime)), -30, 0)) : 0)))
                            * Matrix.CreateTranslation(a)
                            * Matrix.CreateRotationX(upDownRot + MathHelper.ToRadians(RecoilDegrees)) * Matrix.CreateRotationY(leftRightRot) * Matrix.CreateTranslation(position);
                        effect.Parameters["World"].SetValue(gunMatrix);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }
            else if (Inventory.IsTool(selectedItem))
            {
                foreach (ModelMesh mesh in (Inventory.IsPick(selectedItem) ? Resources.Pick : Resources.Shovel).Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        Vector3 a = new Vector3(.25f, -.25f, -.75f);

                        if (InSwing)
                        {
                            a.Z -= 2 * (float)(.1f * Math.Cos((Math.PI * 2 / 250) * millisecondSinceSwing) - .1f);
                        }

                        if (attackSwing)
                        {
                            a.Z += (float)(.15f * Math.Cos((Math.PI * 2 / 175) * millisecondSinceAttackSwing) - .15f);
                            a.X += (float)(.05f * Math.Cos((Math.PI * 2 / 175) * millisecondSinceAttackSwing) - .05f);
                        }


                        float move = .025f * (float)Math.Sin((Math.PI * 2f / 1750f) * (float)timeInMiliseconds);
                        a.X += move * .15f;
                        a.Y += move * .24f;
                        a.Z += move;

                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        switch (selectedItem)
                        {
                            case InventoryItem.PickRock:
                                effect.Parameters["Texture0"].SetValue(Resources.PickRock);
                                break;
                            case InventoryItem.PickSteel:
                                effect.Parameters["Texture0"].SetValue(Resources.PickSteel);
                                break;
                            case InventoryItem.PickDiamond:
                                effect.Parameters["Texture0"].SetValue(Resources.PickDiamond);
                                break;
                            case InventoryItem.ShovelRock:
                                effect.Parameters["Texture0"].SetValue(Resources.ShovelRock);
                                break;
                            case InventoryItem.ShovelSteel:
                                effect.Parameters["Texture0"].SetValue(Resources.ShovelSteel);
                                break;
                            case InventoryItem.ShovelDiamond:
                                effect.Parameters["Texture0"].SetValue(Resources.ShovelDiamond);
                                break;
                        }
                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.6f) * Matrix.CreateRotationY(MathHelper.ToRadians(87.5f)) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                            * Matrix.CreateRotationX(MathHelper.ToRadians(RecoilDegrees))
                            * Matrix.CreateTranslation(a)
                            * Matrix.CreateRotationX(upDownRot + MathHelper.ToRadians(RecoilDegrees)) * Matrix.CreateRotationY(leftRightRot) * Matrix.CreateTranslation(position));
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }

            if (drawFlare)
            {
                Resources.BlockEffect.Parameters["Brightness"].SetValue(Vector3.Zero);

                foreach (ModelMesh mesh in Resources.MuzzleFlare[gunFired].Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["World"].SetValue(gunMatrix);
                        effect.Parameters["Texture0"].SetValue(Resources.FlareTexture);
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }

                if (MinerOfDuty.game.player.IsUsingGoggles)
                    Resources.BlockEffect.Parameters["Brightness"].SetValue(MultiplayerGame.goggles);
                else
                    Resources.BlockEffect.Parameters["Brightness"].SetValue(Vector3.Zero);
            }


            RenderTarget.GraphicsDevice.SetRenderTarget(null);
            RenderTarget.GraphicsDevice.Clear(Color.CornflowerBlue);
        }

    }

    
}