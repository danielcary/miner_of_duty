using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Menus;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.Game.Editor;

namespace Miner_Of_Duty.Game
{
    public class PlayerBody
    {
        public int PlayerID;

        private VertexBuffer HeadVertexBuffer, BodyVertexBuffer, ArmVertexBuffer, LegVertexBuffer;
        private IndexBuffer HeadIndexBuffer, BodyIndexBuffer, ArmIndexBuffer, LegIndexBuffer;

        public void CreateParts(Color hair, Color eye, Color skin, Color shirt, Color pants)
        {
            EditCharacter.CreateBodyFromColor(gd, out HeadVertexBuffer, out HeadIndexBuffer, out BodyVertexBuffer, out BodyIndexBuffer,
                out ArmVertexBuffer, out ArmIndexBuffer, out LegVertexBuffer, out LegIndexBuffer, eye, hair, pants, shirt, skin);
        }

        public static Model Head, Leg, Body, Arm;
        public Vector3 playerPosition;
        private float upDownRotation, leftRightRotation, bodyRotation;
        private float leftBodyAngle, rightBodyAngle;
        private float movementSpeed;
        private bool leftTriggerDown;
        private bool dead;
        public Player.Stance stance;
        private InventoryItem holdingItem;
        private GraphicsDevice gd;
        private bool kNIFDING;
        private int timeInKnife;
        public bool knifing { get { return kNIFDING; } set { if (kNIFDING != value) { timeInKnife = 0; } kNIFDING = value; } }
        public bool HoldingGrenade = false;
        public bool ThrowingGrenade = false;
        public byte GrenadeID;

        public void ThrowGrenade(byte id)
        {
            HoldingGrenade = false;
            ThrowingGrenade = true;
            GrenadeID = id;
            timeInKnife = 0;
            knifing = false;
        }

        private Matrix HeadWorld, BodyWorld, LegLeftWorld, LegRightWorld, ArmLeftWorld, ArmRightWorld;
        public enum Hit { Head, Body, Leg, Arm, None };
        public Hit CheckForCollision(ref Ray ray, float blockHit, out float bestDis)
        {
            Ray toUse = ray;
            Matrix test;
            float? result;
            bestDis = 69696969;
            Hit hit = Hit.None;

            ///////////////////////////////////////////HEAD
            Matrix.Invert(ref HeadWorld, out test);
            Vector3.Transform(ref ray.Position, ref test, out toUse.Position);
            Vector3.TransformNormal(ref ray.Direction, ref test, out toUse.Direction);
            Resources.HeadBox.Intersects(ref toUse, out result);
            if (result.HasValue)
            {
                if (blockHit > result.Value)
                    if (bestDis > result.Value)
                    {
                        bestDis = result.Value;
                        hit = Hit.Head;
                    }
            }
            ///////////////////////////////////////////Body
            Matrix.Invert(ref BodyWorld, out test);
            Vector3.Transform(ref ray.Position, ref test, out toUse.Position);
            Vector3.TransformNormal(ref ray.Direction, ref test, out toUse.Direction);
            Resources.BodyBox.Intersects(ref toUse, out result);
            if (result.HasValue)
            {
                if (blockHit > result.Value)
                    if (bestDis > result.Value)
                    {
                        bestDis = result.Value;
                        hit = Hit.Body;
                    }
            }
            ///////////////////////////////////////////LeftArm
            Matrix.Invert(ref ArmLeftWorld, out test);
            Vector3.Transform(ref ray.Position, ref test, out toUse.Position);
            Vector3.TransformNormal(ref ray.Direction, ref test, out toUse.Direction);
            Resources.ArmBox.Intersects(ref toUse, out result);
            if (result.HasValue)
            {
                if (blockHit > result.Value)
                    if (bestDis > result.Value)
                    {
                        bestDis = result.Value;
                        hit = Hit.Arm;
                    }
            }
            ///////////////////////////////////////////RightArm
            Matrix.Invert(ref ArmRightWorld, out test);
            Vector3.Transform(ref ray.Position, ref test, out toUse.Position);
            Vector3.TransformNormal(ref ray.Direction, ref test, out toUse.Direction);
            Resources.ArmBox.Intersects(ref toUse, out result);
            if (result.HasValue)
            {
                if (blockHit > result.Value)
                    if (bestDis > result.Value)
                    {
                        bestDis = result.Value;
                        hit = Hit.Arm;
                    }
            }
            ///////////////////////////////////////////LeftLeg
            Matrix.Invert(ref LegLeftWorld, out test);
            Vector3.Transform(ref ray.Position, ref test, out toUse.Position);
            Vector3.TransformNormal(ref ray.Direction, ref test, out toUse.Direction);
            Resources.LegBox.Intersects(ref toUse, out result);
            if (result.HasValue)
            {
                if (bestDis > result.Value)
                {
                    bestDis = result.Value;
                    hit = Hit.Leg;
                }
            }
            ///////////////////////////////////////////RightLeg
            Matrix.Invert(ref LegRightWorld, out test);
            Vector3.Transform(ref ray.Position, ref test, out toUse.Position);
            Vector3.TransformNormal(ref ray.Direction, ref test, out toUse.Direction);
            Resources.LegBox.Intersects(ref toUse, out result);
            if (result.HasValue)
            {
                if (blockHit > result.Value)
                    if (bestDis > result.Value)
                    {
                        bestDis = result.Value;
                        hit = Hit.Leg;
                    }
            }

            return hit;
        }

        private void Dispose(GraphicsResource i)
        {
            if (i != null && i.IsDisposed == false)
                i.Dispose();
        }

        public void Dispose()
        {
            Dispose(HeadVertexBuffer);
            Dispose(BodyVertexBuffer);
            Dispose(ArmVertexBuffer);
            Dispose(LegVertexBuffer);
            Dispose(HeadIndexBuffer);
            Dispose(BodyIndexBuffer);
            Dispose(ArmIndexBuffer);
            Dispose(LegIndexBuffer);
        }

        private BoundingBox box = new BoundingBox(-new Vector3(.27f, 0f, .27f) * 1.1f, new Vector3(.27f, 1.8f, .27f) * 1.1f);
        private static Vector3 adder;
        public bool CheckForIntersection(ref BoundingBox box)
        {
            if (float.IsNaN(playerPosition.X))
                return false;

            adder = playerPosition - new Vector3(0, 1.4f, 0);
            this.box.Max += adder;
            this.box.Min += adder;

            bool ans;
            this.box.Intersects(ref box, out ans);

            this.box.Max -= adder;
            this.box.Min -= adder;

            return ans;
        }
        private bool isTester;
        private string testerName;
        private string name;
        public Color color;
        private Vector2 nameOrigin;
        public PlayerBody(GraphicsDevice gd,  int playerID, string name, Color color)
        {
            this.gd = gd;
            this.PlayerID = playerID;
            leftBodyAngle = MathHelper.ToRadians(20);
            rightBodyAngle = MathHelper.ToRadians(-20);
            this.name = name;

            if (SpecialGamer.IsTest(name))
            {
                testerName = "";
                while (Resources.Font.MeasureString(testerName).X <= Resources.Font.MeasureString(name + " ").X)
                {
                    testerName += " ";
                }

                testerName += " (TESTER)";
                isTester = true;
            }

            this.color = color;
            nameOrigin = Resources.Font.MeasureString(name + (isTester ? " (TESTER)" : "")) / 2;



            baseEffect = new BasicEffect(gd);
            baseEffect.TextureEnabled = true;
            baseEffect.VertexColorEnabled = true;
        }

        private int millisecondsInSwing = 0;
        private float fallRot;

        private float diff;
        public void Update(GameTime gameTime, Player player)
        {
            playerPosition = player.position;
            upDownRotation = player.upDownRot;
            

            stance = player.stance;

            diff = player.leftRightRot - leftRightRotation;
            leftRightRotation = player.leftRightRot;
            bodyRotation = leftRightRotation;

            //if (leftRightRotation > leftBodyAngle)
            //{
            //    leftBodyAngle += diff;
            //    rightBodyAngle += diff;
            //    bodyRotation += diff;
            //}
            //else if (leftRightRotation < rightBodyAngle)
            //{
            //    leftBodyAngle += diff;
            //    rightBodyAngle += diff;
            //    bodyRotation += diff;
            //}

            holdingItem = player.inventory.GetSelectedItem;
            movementSpeed = player.movingSpeed;
            millisecondsInSwing += gameTime.ElapsedGameTime.Milliseconds;
            leftTriggerDown = player.leftTriggerDown;
            dead = player.dead;

            if (dead)
            {
                if (fallRot < MathHelper.PiOver2)
                    fallRot += (float)(MathHelper.Pi * gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
                fallRot = 0;

            if (knifing)
                timeInKnife += gameTime.ElapsedGameTime.Milliseconds;

            if (ThrowingGrenade)
            {
                if (timeInKnife < 400)
                    timeInKnife += gameTime.ElapsedGameTime.Milliseconds;
                else
                    ThrowingGrenade = false;
            }

            //if (float.IsNaN(bodyRotation))
            //{
            //    //bodyRotation = leftRightRotation;
            //}
        }

        public void Update(GameTime gameTime, PlayerEditor player)
        {
            playerPosition = player.position;
            upDownRotation = player.upDownRot;


            stance = player.stance;

            diff = player.leftRightRot - leftRightRotation;
            leftRightRotation = player.leftRightRot;
            bodyRotation = leftRightRotation;

            //if (leftRightRotation > leftBodyAngle)
            //{
            //    leftBodyAngle += diff;
            //    rightBodyAngle += diff;
            //    bodyRotation += diff;
            //}
            //else if (leftRightRotation < rightBodyAngle)
            //{
            //    leftBodyAngle += diff;
            //    rightBodyAngle += diff;
            //    bodyRotation += diff;
            //}

            holdingItem = player.inventory.GetSelectedItem;
            movementSpeed = player.movingSpeed;
            millisecondsInSwing += gameTime.ElapsedGameTime.Milliseconds;
            leftTriggerDown = player.leftTriggerDown;

            if (dead)
            {
                if (fallRot < MathHelper.PiOver2)
                    fallRot += (float)(MathHelper.Pi * gameTime.ElapsedGameTime.TotalSeconds);
            }
            else
                fallRot = 0;

            if (knifing)
                timeInKnife += gameTime.ElapsedGameTime.Milliseconds;

            if (ThrowingGrenade)
            {
                if (timeInKnife < 400)
                    timeInKnife += gameTime.ElapsedGameTime.Milliseconds;
                else
                    ThrowingGrenade = false;
            }

            //if (float.IsNaN(bodyRotation))
            //{
            //    //bodyRotation = leftRightRotation;
            //}
        }

        public void Render(Camera camera)
        {
            if (!dead)
                render(camera);
            else if (stance == Player.Stance.Standing)
                deadRender(camera);
            else
                deadRenderProne(camera);
        }

        private void deadRender(Camera camera)
        {
            Vector3 a;

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyHead"];
            Resources.BlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);

            //head
            a = new Vector3(0, 1.4f, 0);
            Matrix mat = Matrix.CreateRotationZ(fallRot);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, 1.4f, 0);

            HeadWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(a)
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.HeadBaseTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.HeadEyeTexture);
            Resources.BlockEffect.Parameters["Body3"].SetValue(EditCharacter.HeadHairTexture);
            Resources.BlockEffect.Parameters["Body4"].SetValue(EditCharacter.HeadSkinTexture);
            Resources.BlockEffect.Parameters["World"].SetValue(HeadWorld);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = HeadIndexBuffer;
            gd.SetVertexBuffer(HeadVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, HeadVertexBuffer.VertexCount, 0, 12);

            //body
            a = new Vector3(0, 1.4f - .47f, 0);
            mat = Matrix.CreateRotationZ(fallRot);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, 1.4f, 0);

            BodyWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)
                   * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyBody"];
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.BodyShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.BodyPantsTexture);
            Resources.BlockEffect.Parameters["Body3"].SetValue(EditCharacter.BodySkinTexture);
            Resources.BlockEffect.Parameters["World"].SetValue(BodyWorld);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = BodyIndexBuffer;
            gd.SetVertexBuffer(BodyVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, BodyVertexBuffer.VertexCount, 0, 12);


            //Right Arm
            a = new Vector3(0, 1.4f - .5f, .325f);
            mat = Matrix.CreateRotationZ(fallRot);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, 1.4f, 0);

            ArmRightWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);

            //Left Arm
            a = new Vector3(0, 1.4f - .5f, -.325f);
            mat = Matrix.CreateRotationZ(fallRot);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, 1.4f, 0);

            ArmLeftWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);


            //Left Leg
            a = new Vector3(0, 1.4f - 1.1f, -.125f);
            mat = Matrix.CreateRotationZ(fallRot);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, 1.4f, 0);

            LegLeftWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);

            //Right Leg
            a = new Vector3(0, 1.4f - 1.1f, .125f);
            mat = Matrix.CreateRotationZ(fallRot);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, 1.4f, 0);

            LegRightWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);
        }

        private void deadRenderProne(Camera camera)
        {
            Vector3 a;
            Matrix mat;

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyHead"];
            Resources.BlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);

            //Head

            a = new Vector3(0, .55f, 0);
            mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, .55f, 0);

            a.X = 0;
            a.Z = -.5f;

            HeadWorld = Matrix.CreateRotationX(MathHelper.Clamp(upDownRotation + .1f, -.7f, 1)) * Matrix.CreateTranslation(a)
                * Matrix.CreateRotationY(leftRightRotation) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.HeadBaseTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.HeadEyeTexture);
            Resources.BlockEffect.Parameters["Body3"].SetValue(EditCharacter.HeadHairTexture);
            Resources.BlockEffect.Parameters["Body4"].SetValue(EditCharacter.HeadSkinTexture);
            Resources.BlockEffect.Parameters["World"].SetValue(HeadWorld);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = HeadIndexBuffer;
            gd.SetVertexBuffer(HeadVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, HeadVertexBuffer.VertexCount, 0, 12);


            //Body
            a = new Vector3(0, .6f - .47f, 0);
            mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, .6f, 0);

            BodyWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateTranslation(a)
                   * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyBody"];
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.BodyShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.BodyPantsTexture);
            Resources.BlockEffect.Parameters["Body3"].SetValue(EditCharacter.BodySkinTexture);
            Resources.BlockEffect.Parameters["World"].SetValue(BodyWorld);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = BodyIndexBuffer;
            gd.SetVertexBuffer(BodyVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, BodyVertexBuffer.VertexCount, 0, 12);


            //Right Arm
            a = new Vector3(0, .6f - .5f, .325f);
            mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, .6f, 0);

            ArmRightWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);



            a = new Vector3(0, .6f - .5f, -.325f);
            mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, .6f, 0);

            ArmLeftWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);


            //Left Leg

            a = new Vector3(0, .6f - 1.1f, -.125f);
            mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, .6f, 0);

            LegLeftWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);


            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);

            //Right Leg

            a = new Vector3(0, .6f - 1.1f, .125f);
            mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            Vector3.Transform(ref a, ref mat, out a);
            a -= new Vector3(0, .6f, 0);

            LegRightWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);
        }

        public Matrix GetBarrelMat()
        {
            Vector3 a;
            switch (stance)
            {
                case Player.Stance.Standing:
                case Player.Stance.Crouching:
                    a = new Vector3(0, 0, -.8f);
                    return Matrix.CreateScale(.65f) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                        * Matrix.CreateTranslation(a) * Matrix.CreateRotationX(MathHelper.Clamp(upDownRotation, -.8f, 1)) * Matrix.CreateTranslation(0, -.375f, 0)
                        * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                case Player.Stance.Prone:
                    a = new Vector3(0, .6f - .5f, .325f);
                    Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                    Vector3.Transform(ref a, ref mat, out a);
                    a -= new Vector3(0, .6f, 0);

                    a.X += .09f;
                    a.Z -= 1.3f;
                    a.Y += .475f;

                    return Matrix.CreateScale(.65f) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                        * Matrix.CreateTranslation(a) * Matrix.CreateTranslation(0, -.375f, 0)
                        * Matrix.CreateRotationY(leftRightRotation) * Matrix.CreateTranslation(playerPosition);
                default:
                    return Matrix.Identity;
            }
        }

        private void render(Camera camera)
        {
            Vector3 a;
            float rot, outt;

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyHead"];
            Resources.BlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);

            //Head
            switch (stance)
            {
                case Player.Stance.Crouching:
                case Player.Stance.Standing:
                    HeadWorld = Matrix.CreateRotationX(MathHelper.Clamp(upDownRotation, -.8f, 1)) * Matrix.CreateRotationY(leftRightRotation) * Matrix.CreateTranslation(playerPosition);
                    break;
                case Player.Stance.Prone:
                    a = new Vector3(0, .55f, 0);
                    Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                    Vector3.Transform(ref a, ref mat, out a);
                    a -= new Vector3(0, .55f, 0);

                    a.X = 0;
                    a.Z = -.5f;

                    HeadWorld = Matrix.CreateRotationX(MathHelper.Clamp(upDownRotation + .1f, -.7f, 1)) * Matrix.CreateTranslation(a)
                        * Matrix.CreateRotationY(leftRightRotation) * Matrix.CreateTranslation(playerPosition);
                    break;
            }
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.HeadBaseTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.HeadEyeTexture);
            Resources.BlockEffect.Parameters["Body3"].SetValue(EditCharacter.HeadHairTexture);
            Resources.BlockEffect.Parameters["Body4"].SetValue(EditCharacter.HeadSkinTexture);
            Resources.BlockEffect.Parameters["World"].SetValue(HeadWorld);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = HeadIndexBuffer;
            gd.SetVertexBuffer(HeadVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, HeadVertexBuffer.VertexCount, 0, 12);


            //Body
            switch (stance)
            {
                case Player.Stance.Crouching:
                case Player.Stance.Standing:
                    a = new Vector3(0, -.47f, 0);
                    BodyWorld = Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition + a);
                    break;
                case Player.Stance.Prone:
                    a = new Vector3(0, .6f - .47f, 0);
                    Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                    Vector3.Transform(ref a, ref mat, out a);
                    a -= new Vector3(0, .6f, 0);

                    BodyWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateTranslation(a)
                           * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);
                    break;
            }
            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyBody"];
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.BodyShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.BodyPantsTexture);
            Resources.BlockEffect.Parameters["Body3"].SetValue(EditCharacter.BodySkinTexture);
            Resources.BlockEffect.Parameters["World"].SetValue(BodyWorld);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = BodyIndexBuffer;
            gd.SetVertexBuffer(BodyVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, BodyVertexBuffer.VertexCount, 0, 12);


            //Right Arm
            if (ThrowingGrenade)
            {
                a = new Vector3(.325f, -.56f, 0);


                ArmRightWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX((MathHelper.PiOver4 * (float)Math.Sin((MathHelper.Pi / 400f) * timeInKnife)) + MathHelper.Pi) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(0, .25f, 0)
                  * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
            }
            else if (HoldingGrenade)
            {
                a = new Vector3(.325f, -.56f, 0);


                ArmRightWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX((MathHelper.PiOver4 * (float)Math.Sin((MathHelper.Pi / 400f) * 0)) + MathHelper.Pi) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(0, .25f, 0)
                  * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
            }
            else if (knifing == false)
            {
                switch (stance)
                {
                    case Player.Stance.Crouching:
                    case Player.Stance.Standing:
                        if (!Inventory.IsGun(holdingItem))
                        {
                            a = new Vector3(.325f, -.56f, 0);

                            rot = movementSpeed * (float)Math.Sin((Math.PI * 2 / 400) * millisecondsInSwing);
                            outt = movementSpeed * 5;

                            ArmRightWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(rot) * Matrix.CreateRotationZ(MathHelper.ToRadians(outt)) * Matrix.CreateTranslation(0, .25f, 0)
                              * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                        }
                        else
                        {
                            a = new Vector3(.325f, -.56f, 0);
                            ArmRightWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(MathHelper.ToRadians(80) + MathHelper.Clamp(upDownRotation, -.8f, 1)) * Matrix.CreateRotationY(MathHelper.ToRadians(7.5f)) * Matrix.CreateTranslation(0, .25f, 0)
                               * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                        }
                        break;
                    case Player.Stance.Prone:
                        a = new Vector3(0, .6f - .5f, .325f);
                        Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                        Vector3.Transform(ref a, ref mat, out a);
                        a -= new Vector3(0, .6f, 0);

                        a.X += .2f;

                        ArmRightWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                            * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);
                        break;
                }
            }
            else
            {
                a = new Vector3(.325f, -.56f, 0);


                ArmRightWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX((MathHelper.PiOver4 * (float)Math.Sin((MathHelper.Pi / 400f) * timeInKnife)) + MathHelper.Pi) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateTranslation(0, .25f, 0)
                  * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
            }

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);

            if (knifing)
            {
                foreach (ModelMesh mesh in Resources.KnifeModel.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        a = new Vector3(.325f, .3f, 0);

                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.3f)
                            * Matrix.CreateTranslation(a) * Matrix.CreateRotationX((MathHelper.PiOver4 * (float)Math.Sin((MathHelper.Pi / 400f) * (timeInKnife + 400))) - MathHelper.ToRadians(40))
                            * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition));

                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["Texture0"].SetValue(Resources.KnifeTexture);
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }
            else if (ThrowingGrenade)
            {
                foreach (ModelMesh mesh in (GrenadeID == GrenadeType.GRENADE_FRAG ? Resources.FragModel : Resources.SmokeFlashModel).Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        a = new Vector3(.325f, .3f, 0);

                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.3f)
                            * Matrix.CreateTranslation(a) * Matrix.CreateRotationX((MathHelper.PiOver4 * (float)Math.Sin((MathHelper.Pi / 400f) * (timeInKnife + 400))) - MathHelper.ToRadians(40))
                            * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition));

                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["Texture0"].SetValue(GrenadeID == GrenadeType.GRENADE_FRAG ? Resources.FragModelTexture : GrenadeID == GrenadeType.GRENADE_SMOKE ? Resources.SmokeModelTexture : Resources.FlashModelTexture);
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }
            else if (HoldingGrenade)
            {
                foreach (ModelMesh mesh in (GrenadeID == GrenadeType.GRENADE_FRAG ? Resources.FragModel : Resources.SmokeFlashModel).Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                        a = new Vector3(.325f, .3f, 0);

                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.3f)
                            * Matrix.CreateTranslation(a) * Matrix.CreateRotationX((MathHelper.PiOver4 * (float)Math.Sin((MathHelper.Pi / 400f) * (0 + 400))) - MathHelper.ToRadians(40))
                            * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition));

                        effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                        effect.Parameters["Texture0"].SetValue(GrenadeID == GrenadeType.GRENADE_FRAG ? Resources.FragModelTexture : GrenadeID == GrenadeType.GRENADE_SMOKE ? Resources.SmokeModelTexture : Resources.FlashModelTexture);
                        effect.Parameters["CameraPosition"].SetValue(camera.Position);
                        effect.Parameters["View"].SetValue(camera.ViewMatrix);
                        effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                    }
                    mesh.Draw();
                }
            }
            else
            {
                #region holdingItem
                switch (holdingItem)
                {
                    case InventoryItem.DirtBlock:
                    case InventoryItem.GrassBlock:
                    case InventoryItem.StoneBlock:
                    case InventoryItem.SandBlock:
                    case InventoryItem.GlassBlock:
                    case InventoryItem.GlowBlock:
                    case InventoryItem.LeafBlock:
                    case InventoryItem.WoodBlock:
                    case InventoryItem.YellowBlock:
                    case InventoryItem.TealBlock:
                    case InventoryItem.RedBlock:
                    case InventoryItem.OrangeBlock:
                    case InventoryItem.GreyBlock:
                    case InventoryItem.GreenBlock:
                    case InventoryItem.BlueBlock:
                    case InventoryItem.BlackBlock:
                    case InventoryItem.WhiteBlock:

                    case InventoryItem.TeamASpawn1:
                    case InventoryItem.TeamBSpawn1:
                    case InventoryItem.TeamASpawn2:
                    case InventoryItem.TeamBSpawn2:
                    case InventoryItem.TeamASpawn3:
                    case InventoryItem.TeamBSpawn3:
                        #region block
                        Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["Light2"];
                        Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
                        Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                        if (Inventory.IsSpawn(holdingItem))
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Inventory.GetHoldingBlockTextureForSpawn(holdingItem));
                        else
                            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.BLOCKTEXTURES[Inventory.InventoryItemToID(holdingItem)]);

                        switch (stance)
                        {
                            case Player.Stance.Crouching:
                            case Player.Stance.Standing:
                                a = new Vector3(.335f, -.8f, -.2f);
                                rot = movementSpeed * .75f * (float)Math.Sin((Math.PI * 2 / 400) * millisecondsInSwing);

                                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateRotationZ(-MathHelper.PiOver4 / 2) * Matrix.CreateScale(.25f)
                                   * Matrix.CreateTranslation(a) * Matrix.CreateRotationX(rot) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition));
                                break;
                            case Player.Stance.Prone:
                                a = new Vector3(0, .6f - .5f, .325f);
                                Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                                Vector3.Transform(ref a, ref mat, out a);
                                a -= new Vector3(0, .6f, 0);

                                a.X += .2f;
                                a.Z -= 1.1f;

                                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateRotationZ(-MathHelper.PiOver4 / 2) * Matrix.CreateScale(.25f)
                                   * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(leftRightRotation) * Matrix.CreateTranslation(playerPosition));
                                break;
                        }

                        Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
                        Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);

                        Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                        gd.SetVertexBuffer(Resources.SelectionBuffer);
                        gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);
                        #endregion
                        break;


                    case InventoryItem.EmptyBucket:
                    case InventoryItem.LavaBucket:
                    case InventoryItem.WaterBucket:
                        #region Lava

                        foreach (ModelMesh mesh in Resources.Bucket.Meshes)
                        {
                            foreach (Effect effect in mesh.Effects)
                            {
                                switch (stance)
                                {
                                    case Player.Stance.Standing:
                                    case Player.Stance.Crouching:
                                        a = new Vector3(.35f, -.725f, -.18f);
                                        rot = movementSpeed * .75f * (float)Math.Sin((Math.PI * 2 / 400) * millisecondsInSwing);
                                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.3f) * Matrix.CreateRotationX(-MathHelper.PiOver4 / 1.5f)
                                            * Matrix.CreateTranslation(a) * Matrix.CreateRotationX(rot)
                                            * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition));
                                        break;
                                    case Player.Stance.Prone:
                                        a = new Vector3(0, .6f - .5f, .325f);
                                        Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                                        Vector3.Transform(ref a, ref mat, out a);
                                        a -= new Vector3(0, .6f, 0);

                                        a.X += .2f;
                                        a.Z -= 1.1f;

                                        effect.Parameters["World"].SetValue(Matrix.CreateScale(.3f) * Matrix.CreateRotationX(-MathHelper.PiOver4 / 1.5f)
                                            * Matrix.CreateTranslation(a)
                                            * Matrix.CreateRotationY(leftRightRotation) * Matrix.CreateTranslation(playerPosition));


                                        break;
                                }
                                effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                                effect.Parameters["Texture0"].SetValue(holdingItem == InventoryItem.EmptyBucket ? Resources.EmptyBucketTexture : holdingItem == InventoryItem.LavaBucket ? Resources.LavaBucketTexture : Resources.WaterBucketTexture);
                                effect.Parameters["CameraPosition"].SetValue(camera.Position);
                                effect.Parameters["View"].SetValue(camera.ViewMatrix);
                                effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                            }
                            mesh.Draw();
                        }
                        #endregion
                        break;
                    case InventoryItem.Goggles:
                    case InventoryItem.Empty:
                        break;
                    default:

                        #region guncode
                        if (Inventory.IsGun(holdingItem))
                        {
                            foreach (ModelMesh mesh in Resources.GunModels[Inventory.InventoryItemToID(holdingItem)].Meshes)
                            {
                                foreach (Effect effect in mesh.Effects)
                                {
                                    switch (stance)
                                    {
                                        case Player.Stance.Standing:
                                        case Player.Stance.Crouching:
                                            a = new Vector3(0, 0, -.8f);
                                            effect.Parameters["World"].SetValue(Matrix.CreateScale(.65f) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                                                * Matrix.CreateTranslation(a) * Matrix.CreateRotationX(MathHelper.Clamp(upDownRotation, -.8f, 1)) * Matrix.CreateTranslation(0, -.375f, 0)
                                                * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition));
                                            break;
                                        case Player.Stance.Prone:
                                            a = new Vector3(0, .6f - .5f, .325f);
                                            Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                                            Vector3.Transform(ref a, ref mat, out a);
                                            a -= new Vector3(0, .6f, 0);

                                            a.X += .09f;
                                            a.Z -= 1.3f;
                                            a.Y += .475f;

                                            effect.Parameters["World"].SetValue(Matrix.CreateScale(.65f) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                                                * Matrix.CreateTranslation(a) * Matrix.CreateTranslation(0, -.375f, 0)
                                                * Matrix.CreateRotationY(leftRightRotation) * Matrix.CreateTranslation(playerPosition));
                                            break;
                                    }

                                    effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                                    effect.Parameters["CameraPosition"].SetValue(camera.Position);
                                    effect.Parameters["Texture0"].SetValue(Resources.GunModelTextures[Inventory.InventoryItemToID(holdingItem)]);
                                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                                    effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                                }
                                mesh.Draw();
                            }
                        }
                        #endregion
                        else if (Inventory.IsTool(holdingItem))
                        {
                            foreach (ModelMesh mesh in (Inventory.IsPick(holdingItem) ? Resources.Pick : Resources.Shovel).Meshes)
                            {
                                foreach (Effect effect in mesh.Effects)
                                {
                                    switch (stance)
                                    {
                                        case Player.Stance.Crouching:
                                        case Player.Stance.Standing:
                                            a = new Vector3(0, 0, -.8f);

                                            rot = movementSpeed * .75f * (float)Math.Sin((Math.PI * 2 / 400) * millisecondsInSwing);
                                            effect.Parameters["World"].SetValue(Matrix.CreateScale(.65f) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                                                * Matrix.CreateTranslation(a) * Matrix.CreateRotationX(rot) * Matrix.CreateTranslation(0, -.375f, 0)
                                                * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition));
                                            break;
                                        case Player.Stance.Prone:
                                            a = new Vector3(0, .6f - .5f, .325f);
                                            Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                                            Vector3.Transform(ref a, ref mat, out a);
                                            a -= new Vector3(0, .6f, 0);

                                            a.X += .09f;
                                            a.Z -= 1.2f;
                                            a.Y += .475f;

                                            effect.Parameters["World"].SetValue(Matrix.CreateScale(.65f) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2)
                                                * Matrix.CreateTranslation(a) * Matrix.CreateTranslation(0, -.375f, 0)
                                                * Matrix.CreateRotationY(leftRightRotation) * Matrix.CreateTranslation(playerPosition));
                                            break;
                                    }

                                    effect.CurrentTechnique = effect.Techniques["ModelLightFog"];
                                    effect.Parameters["CameraPosition"].SetValue(camera.Position);
                                    switch (holdingItem)
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

                                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                                    effect.Parameters["Projection"].SetValue(camera.ProjMatrix);
                                }
                                mesh.Draw();
                            }
                        }
                        break;

                }
                #endregion
            }

            //Left Arm
            switch (stance)
            {
                case Player.Stance.Standing:
                case Player.Stance.Crouching:
                    if (!Inventory.IsGun(holdingItem))
                    {
                        a = new Vector3(-.325f, -.56f, 0);
                        rot = movementSpeed * (float)Math.Cos((Math.PI * 2 / 400) * millisecondsInSwing);
                        outt = movementSpeed * -5;

                        ArmLeftWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(rot) * Matrix.CreateRotationZ(MathHelper.ToRadians(outt)) * Matrix.CreateTranslation(0, .25f, 0)
                          * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                    }
                    else
                    {

                        a = new Vector3(-.25f, -.5f, .1f);
                        ArmLeftWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(MathHelper.ToRadians(70) + MathHelper.Clamp(upDownRotation, -.8f, 1)) * Matrix.CreateRotationY(MathHelper.ToRadians(-35)) * Matrix.CreateTranslation(0, .25f, -.25f)
                         * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                    }
                    break;
                case Player.Stance.Prone:
                    a = new Vector3(0, .6f - .5f, -.325f);
                    Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                    Vector3.Transform(ref a, ref mat, out a);
                    a -= new Vector3(0, .6f, 0);

                    ArmLeftWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                        * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);
                    break;
            }

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);


            //Left Leg

            switch (stance)
            {
                case Player.Stance.Standing:
                    a = new Vector3(-.125f, -1.1f, 0);
                    rot = movementSpeed * (float)Math.Sin((Math.PI * 2 / 400) * millisecondsInSwing);
                    outt = movementSpeed * -2.5f;
                    LegLeftWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(rot) * Matrix.CreateRotationZ(MathHelper.ToRadians(outt)) * Matrix.CreateTranslation(0, .25f, 0)
                        * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                    break;
                case Player.Stance.Crouching:
                    a = new Vector3(-.125f, -1.1f, 0);
                    LegLeftWorld = Matrix.CreateTranslation(0, -.2f, 0) * Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90)) * Matrix.CreateTranslation(0, .2f, 0)
                        * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                    break;
                case Player.Stance.Prone:
                    a = new Vector3(0, .6f - 1.1f, -.125f);
                    Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                    Vector3.Transform(ref a, ref mat, out a);
                    a -= new Vector3(0, .6f, 0);

                    LegLeftWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                        * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);
                    break;
            }


            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);

            //Right Leg

            switch (stance)
            {
                case Player.Stance.Standing:
                    a = new Vector3(.125f, -1.1f, 0);
                    rot = movementSpeed * (float)Math.Cos((Math.PI * 2 / 400) * millisecondsInSwing);
                    outt = movementSpeed * 2.5f;
                    LegRightWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(rot) * Matrix.CreateRotationZ(MathHelper.ToRadians(outt)) * Matrix.CreateTranslation(0, .25f, 0)
                      * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                    break;
                case Player.Stance.Crouching:
                    a = new Vector3(.125f, -1.1f, 0);
                    LegRightWorld = Matrix.CreateTranslation(0, -.2f, 0) * Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90)) * Matrix.CreateTranslation(0, .2f, 0)
                       * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRotation) * Matrix.CreateTranslation(playerPosition);
                    break;
                case Player.Stance.Prone:
                    a = new Vector3(0, .6f - 1.1f, .125f);
                    Matrix mat = Matrix.CreateRotationZ(-MathHelper.PiOver2);
                    Vector3.Transform(ref a, ref mat, out a);
                    a -= new Vector3(0, .6f, 0);

                    LegRightWorld = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
                        * Matrix.CreateRotationY(leftRightRotation + MathHelper.PiOver2) * Matrix.CreateTranslation(playerPosition);
                    break;
            }
            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);
        }

        private BasicEffect baseEffect;
        private float dist;
        public void RenderName(Camera cam, SpriteBatch sb)
        {
            if (dead == false)
            {
                Vector3.Distance(ref cam.Position, ref playerPosition, out dist);

                if (dist < 35 && stance != Player.Stance.Prone)
                {
                    baseEffect.World = Matrix.CreateConstrainedBillboard(playerPosition + (Vector3.Up * .25f), cam.Position, Vector3.Down, null, null);
                    baseEffect.View = cam.ViewMatrix;
                    baseEffect.Projection = cam.ProjMatrix;

                    sb.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, baseEffect);
                    sb.DrawString(Resources.Font, name, Vector2.Zero, color, 0, nameOrigin, .0075f, 0, 0);
                    if (isTester)
                    {
                        sb.DrawString(Resources.Font, testerName, Vector2.Zero, Color.Blue, 0, nameOrigin, .0075f, 0, 0);
                    }
                    sb.End();
                }
            }
        }

    }
}

