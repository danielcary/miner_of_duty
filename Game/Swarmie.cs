using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Miner_Of_Duty.Menus;
using Microsoft.Xna.Framework.Graphics;
using Miner_Of_Duty.Game.Networking;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Miner_Of_Duty.Game
{

    //fooswarmie needs to be feed:
    //position Vector3 12bytes
    //bodyrot 4bytes
    //dont move 1bytes
    //movementspeed 4bytes

    public abstract class ISwarmie
    {
        public short ID;
        public abstract bool DeadAndDeathAnimaitionDone { get; }
        public abstract bool Dead { get; }
        public Vector3 Position;
        public BoundingBox bb;

        public abstract void Update(GameTime gameTime);

        public PlayerBody.Hit CheckForCollision(ref Ray ray, float blockHit, out float bestDis)
        {
            Ray toUse = ray;
            Matrix test;
            float? result;
            bestDis = 69696969;
            PlayerBody.Hit hit = PlayerBody.Hit.None;

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
                        hit = PlayerBody.Hit.Head;
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
                        hit = PlayerBody.Hit.Body;
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
                        hit = PlayerBody.Hit.Arm;
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
                        hit = PlayerBody.Hit.Arm;
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
                    hit = PlayerBody.Hit.Leg;
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
                        hit = PlayerBody.Hit.Leg;
                    }
            }

            return hit;
        }

        public float bodyRot;
        public float movementSpeed;
        protected int millisecondsInSwing;
        protected bool dontMove;
        protected float fallRot = 0;
        protected Matrix HeadWorld, BodyWorld, ArmLeftWorld, ArmRightWorld, LegLeftWorld, LegRightWorld;

        public static VertexBuffer HeadVertexBuffer, BodyVertexBuffer, ArmVertexBuffer, LegVertexBuffer;
        public static IndexBuffer HeadIndexBuffer, BodyIndexBuffer, ArmIndexBuffer, LegIndexBuffer;
        private static GraphicsDevice gd;
        public void Render(Camera camera)
        {
            if (Dead == false)
            {
                render(camera);
            }
            else
                deadRender(camera);
        }

        private static Matrix mat, mat2;
        private static Vector3 a;
        private void deadRender(Camera camera)
        {
            Position.Y += .9f;

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyHead"];
            Resources.BlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);

            //head
            a.X = 0;
            a.Y = 1.4f;
            a.Z = 0;
            Matrix.CreateRotationZ(fallRot, out mat);
            Vector3.Transform(ref a, ref mat, out a);
            a.Y -= 1.4f;

            //HeadWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateTranslation(a)
            //    * Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2) * Matrix.CreateTranslation(position);

            Matrix.CreateRotationX(-fallRot, out mat);
            Matrix.CreateRotationY(MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationZ(MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out HeadWorld);

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
            a.X = 0;
            a.Y = 1.4f - .47f;
            a.Z = 0;
            Matrix.CreateRotationZ(fallRot, out mat);
            Vector3.Transform(ref a, ref mat, out a);
            a.Y -= 1.4f;

            ////  BodyWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)
            ////       * Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2) * Matrix.CreateTranslation(position);

            Matrix.CreateRotationX(-fallRot, out mat);
            Matrix.CreateRotationY(MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out BodyWorld);

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
            a.X = 0;
            a.Y = 1.4f - .5f;
            a.Z = .325f;
            Matrix.CreateRotationZ(fallRot, out mat);
            Vector3.Transform(ref a, ref mat, out a);
            a.Y -= 1.4f;

            //  ArmRightWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
            //    * Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2) * Matrix.CreateTranslation(position);

            Matrix.CreateRotationX(-fallRot, out mat);
            Matrix.CreateRotationY(MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out ArmRightWorld);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);

            //Left Arm
            a.X = 0;
            a.Y = 1.4f - .5f;
            a.Z = -.325f;
            Matrix.CreateRotationZ(fallRot, out mat);
            Vector3.Transform(ref a, ref mat, out a);
            a.Y -= 1.4f;

            //ArmLeftWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
            //    * Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2) * Matrix.CreateTranslation(position);

            Matrix.CreateRotationX(-fallRot, out mat);
            Matrix.CreateRotationY(MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out ArmLeftWorld);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);


            //Left Leg
            a.X = 0;
            a.Y = 1.4f - 1.1f;
            a.Z = -.125f;
            Matrix.CreateRotationZ(fallRot, out mat);
            Vector3.Transform(ref a, ref mat, out a);
            a.Y -= 1.4f;

            //  LegLeftWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
            //     * Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2) * Matrix.CreateTranslation(position);

            Matrix.CreateRotationX(-fallRot, out mat);
            Matrix.CreateRotationY(MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out LegLeftWorld);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);

            //Right Leg
            a.X = 0;
            a.Y = 1.4f - 1.1f;
            a.Z = .125f;
            Matrix.CreateRotationZ(fallRot, out mat);
            Vector3.Transform(ref a, ref mat, out a);
            a.Y -= 1.4f;

            //  LegRightWorld = Matrix.CreateRotationX(-fallRot) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateTranslation(a)//.325f
            //      * Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2) * Matrix.CreateTranslation(position);

            Matrix.CreateRotationX(-fallRot, out mat);
            Matrix.CreateRotationY(MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot + MathHelper.PiOver2, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out LegRightWorld);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);

            Position.Y -= .9f;
        }

        private void render(Camera camera)
        {

            if (gd == null)
            {
                gd = HeadVertexBuffer.GraphicsDevice;
            }
            Position.Y += .9f;

            float rot, outt;

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyHead"];
            Resources.BlockEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            Resources.BlockEffect.Parameters["View"].SetValue(camera.ViewMatrix);
            Resources.BlockEffect.Parameters["Projection"].SetValue(camera.ProjMatrix);

            //Head
            // HeadWorld = Matrix.CreateRotationX(MathHelper.Clamp(headRot, -.8f, 1)) * Matrix.CreateRotationY(bodyRot) * Matrix.CreateTranslation(position);

            Matrix.CreateRotationX(MathHelper.Clamp(0, -.8f, 1), out mat);
            Matrix.CreateRotationY(bodyRot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out HeadWorld);

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
            a.X = 0;
            a.Y = -.47f;
            a.Z = 0;
            //BodyWorld = Matrix.CreateRotationY(bodyRot) * Matrix.CreateTranslation(position + a);

            Matrix.CreateRotationY(bodyRot, out mat);
            Position.Y += a.Y;
            Matrix.CreateTranslation(ref Position, out mat2);
            Position.Y -= a.Y;
            Matrix.Multiply(ref mat, ref mat2, out BodyWorld);

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
            a.X = .325f;
            a.Y = -.56f;
            a.Z = 0;
            rot = (dontMove ? .7f : movementSpeed) * (float)Math.Sin((Math.PI * 2 / 400f) * millisecondsInSwing);
            if (!dontMove)
                rot /= 5;
            outt = (dontMove ? .7f : movementSpeed) * 3;

            //  ArmRightWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(MathHelper.PiOver2 + rot) * Matrix.CreateRotationZ(MathHelper.ToRadians(outt)) * Matrix.CreateTranslation(0, .25f, 0)
            //  * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRot) * Matrix.CreateTranslation(position);

            Matrix.CreateTranslation(0, -.25f, 0, out mat);
            Matrix.CreateRotationX(MathHelper.PiOver2 + rot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationZ(MathHelper.ToRadians(outt), out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(0, .25f, 0, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out ArmRightWorld);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);




            //Left Arm
            a.X = -.325f;
            a.Y = -.56f;
            a.Z = 0;
            rot = (dontMove ? .7f : movementSpeed) * (float)Math.Cos((Math.PI * 2 / 400) * millisecondsInSwing);
            if (!dontMove)
                rot /= 5;
            outt = (dontMove ? .7f : movementSpeed) * -3;

            // ArmLeftWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(MathHelper.PiOver2 + rot) * Matrix.CreateRotationZ(MathHelper.ToRadians(outt)) * Matrix.CreateTranslation(0, .25f, 0)
            // * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRot) * Matrix.CreateTranslation(position);

            Matrix.CreateTranslation(0, -.25f, 0, out mat);
            Matrix.CreateRotationX(MathHelper.PiOver2 + rot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationZ(MathHelper.ToRadians(outt), out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(0, .25f, 0, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out ArmLeftWorld);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(ArmLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = ArmIndexBuffer;
            gd.SetVertexBuffer(ArmVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);


            //Left Leg
            a.X = -.125f;
            a.Y = -1.1f;
            a.Z = 0;
            rot = movementSpeed * (float)Math.Sin((Math.PI * 2 / 400) * millisecondsInSwing);
            outt = movementSpeed * -2.5f;
            // LegLeftWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(rot) * Matrix.CreateRotationZ(MathHelper.ToRadians(outt)) * Matrix.CreateTranslation(0, .25f, 0)
            //     * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRot) * Matrix.CreateTranslation(position);

            Matrix.CreateTranslation(0, -.25f, 0, out mat);
            Matrix.CreateRotationX(rot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationZ(MathHelper.ToRadians(outt), out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(0, .25f, 0, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out LegLeftWorld);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegLeftWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);

            //right leg
            a.X = .125f;
            a.Y = -1.1f;
            a.Z = 0;
            rot = movementSpeed * (float)Math.Cos((Math.PI * 2 / 400) * millisecondsInSwing);
            outt = movementSpeed * 2.5f;
            //  LegRightWorld = Matrix.CreateTranslation(0, -.25f, 0) * Matrix.CreateRotationX(rot) * Matrix.CreateRotationZ(MathHelper.ToRadians(outt)) * Matrix.CreateTranslation(0, .25f, 0)
            //    * Matrix.CreateTranslation(a) * Matrix.CreateRotationY(bodyRot) * Matrix.CreateTranslation(position);

            Matrix.CreateTranslation(0, -.25f, 0, out mat);
            Matrix.CreateRotationX(rot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationZ(MathHelper.ToRadians(outt), out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(0, .25f, 0, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref a, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateRotationY(bodyRot, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out mat);
            Matrix.CreateTranslation(ref Position, out mat2);
            Matrix.Multiply(ref mat, ref mat2, out LegRightWorld);

            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
            Resources.BlockEffect.Parameters["World"].SetValue(LegRightWorld);
            Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
            Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.Indices = LegIndexBuffer;
            gd.SetVertexBuffer(LegVertexBuffer);
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);

            Position.Y -= .9f;
        }
    }

    /// <summary>
    /// used for networked swarmies
    /// </summary>
    public class FooSwarmie : ISwarmie
    {
        private MovementPacketState a, b;
        /// <summary>
        /// Used for updating bb
        /// </summary>
        private Vector3 oldPosition;

        public FooSwarmie(short id, ref Vector3 position)
        {
            ID = id;
            Position = position;
            oldPosition = position;
            bb = new BoundingBox(Position - new Vector3(.27f, .45f, .27f), Position + new Vector3(.27f, 1.3f, .27f));
            a = new MovementPacketState(Position, 0, 0, 0, 0);
            b = new MovementPacketState(Position, 0, 0, 0, 0);
        }


        public override bool DeadAndDeathAnimaitionDone { get { return timeInDeath > 3000; } }
        public override bool Dead { get { return dead; } }

        private int timeInDeath;
        private bool dead = false;
        public void Kill()
        {
            dead = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (dead)
            {
                timeInDeath += gameTime.ElapsedGameTime.Milliseconds;
                if (fallRot < MathHelper.PiOver2)
                    fallRot += (float)(MathHelper.Pi * gameTime.ElapsedGameTime.TotalSeconds);
            }

            if(dead == false)
                MovementPacketState.InterpolateFooSwarmie(gameTime, ref a, ref b, this);

            millisecondsInSwing += gameTime.ElapsedGameTime.Milliseconds;

          //  bb.Max += Position - oldPosition;
            //bb.Min += Position - oldPosition;


            bb = new BoundingBox(Position - new Vector3(.27f, .45f, .27f), Position + new Vector3(.27f, 1.3f, .27f));

            oldPosition = Position;
        }

        public void ReadToPacketReaderToSwarmie(float timeSent)
        {

            Vector4 data = new HalfVector4() { PackedValue = Packet.PacketReader.ReadUInt64() }.ToVector4();

            Vector3 position = new Vector3(data.X, data.Y, data.Z);
            float newBodyRoy = new HalfSingle() { PackedValue = Packet.PacketReader.ReadUInt16() }.ToSingle();
            float newMovementSpeed = data.W > 8 && data.W < 10 ? 0 : data.W; //use the old movspped
            dontMove = data.W > 8 && data.W < 10;

            //Vector3 position = Packet.PacketReader.ReadVector3();
            //float newBodyRoy = Packet.PacketReader.ReadSingle();
            //float newMovementSpeed = Packet.PacketReader.ReadSingle();
            //dontMove = Packet.PacketReader.ReadBoolean();

            a = b;
            b = new MovementPacketState(ref position, newBodyRoy, timeSent, newMovementSpeed, 0);

        }

    }

    public class Swarmie : ISwarmie
    {

        public void WriteToPacketWriter()
        {
            Packet.PacketWriter.Write(ID);

            Packet.PacketWriter.Write(new HalfVector4(Position.X, Position.Y, Position.Z, dontMove ? 9 : movementSpeed).PackedValue);
            Packet.PacketWriter.Write(new HalfSingle(bodyRot).PackedValue);


            //Packet.PacketWriter.Write(Position);
            //Packet.PacketWriter.Write(bodyRot);
            //Packet.PacketWriter.Write(movementSpeed);
            //Packet.PacketWriter.Write(dontMove);
        }

        public float health;
        private Player attacker;
        private bool followAttacker;

        private int timeInDeath;
        public override bool DeadAndDeathAnimaitionDone { get { return timeInDeath > 3000; } }
        public override bool Dead { get { return health < 0; } }

        private SwarmieManager swarmManager;
        public int pathToFollow;
        private int nodeAt = 0;

        private float speed { get { return constspeed + (.2f * swarmManager.currentRound - .2f); } }
        private const int constspeed = 4;

        public Swarmie()
        {
            health = -100;
            timeInDeath = 5000;
        }

        public void RebuildSwarmie(float health, SwarmieManager sm, int pathToFollow, short id)
        {
            this.ID = id;
            this.health = health;
            this.swarmManager = sm;
            this.pathToFollow = pathToFollow;
            followAttacker = false;
            attacker = null;
            Position = sm.paths[pathToFollow][0];
            bb = new BoundingBox(Position - new Vector3(.27f, .45f, .27f), Position + new Vector3(.27f, 1.3f, .27f));
            nodeAt = 0;
            timeInDeath = 0;
            movementSpeed = 0;
            millisecondsInSwing = 0;
            bodyRot = 0;
            yVelocity = 0;
            timeInAir = 0;
            jumping = false;
            attackCoolDown = 0;
            dontMove = false;
            followAttackerCooldown = 0;
            onGround = false;
            fallRot = 0;
        }

        public void Damage(float dmg)
        {
            health -= dmg;
        }

        public bool Attack(Player attacker, float dmg)
        {
            this.attacker = attacker;
            followAttacker = true;
            Damage(dmg);
            followAttackerCooldown = 15000;

            if (swarmManager.GoldBlocks[pathToFollow].IsTakenBy == this)
            {
                swarmManager.GoldBlocks[pathToFollow].IsTakenBy = null;
            }
            if (health < 0)
                if (swarmManager.GoldBlocks[pathToFollow].IsTakenBy == this)
                    swarmManager.GoldBlocks[pathToFollow].IsTakenBy = null;
            return health < 0;
        }



        private float yVelocity;
        private static Vector3 movement = Vector3.Zero;

        float timeInAir = 0;
        bool jumping = false;
        private static Vector2 amountMoved;
        private static Vector3 dir = Vector3.Zero;
        private bool onGround;
        private static BoundingBox test;
        private static Vector3 x, z;
        private static Vector3 target;
        private static float distance;
        private int attackCoolDown;
        private int followAttackerCooldown;
        public bool DidUpdate = false;
        private short BeenSlowMoving = 0;

        public override void Update(GameTime gameTime)
        {
            if (swarmManager.GoldBlocks[pathToFollow].IsTakenBy == null)
            {
                swarmManager.GoldBlocks[pathToFollow].IsTakenBy = this;
            }


            if (followAttacker == false)
            {
                if (swarmManager.GoldBlocks[pathToFollow].IsTakenBy != null && swarmManager.GoldBlocks[pathToFollow].IsTakenBy != this)
                {
                    int num = new Random().Next(0, MinerOfDuty.Session.AllGamers.Count);
                    Attack(swarmManager.game.players[MinerOfDuty.Session.AllGamers[num].Id], 0);
                }

            }
            else
            {
                if (swarmManager.GoldBlocks[pathToFollow].IsTakenBy == this)
                {
                    swarmManager.GoldBlocks[pathToFollow].IsTakenBy = null;
                }
            }

            if (followAttackerCooldown >= 0)
            {
                followAttackerCooldown -= gameTime.ElapsedGameTime.Milliseconds;
                if (followAttackerCooldown < 0)
                {
                    if (swarmManager.paths[pathToFollow] != null)
                    {
                        followAttacker = false;

                        int best = 0;
                        float bestDis = 69699696969696969;
                        float dist;

                        for (int i = 0; i < swarmManager.paths[pathToFollow].Length; i++)
                        {
                            Vector3.DistanceSquared(ref Position, ref swarmManager.paths[pathToFollow][i], out dist);

                            if (dist < 2.5f * 2.5f)
                            {
                                best = i;
                                break;
                            }
                            else
                            {
                                if (bestDis > dist)
                                {
                                    best = i;
                                    bestDis = dist;
                                }
                            }
                        }

                        nodeAt = best;
                    }
                    else
                    {
                        pathToFollow = -1;
                        for (int i = 0; i < swarmManager.paths.Count; i++)
                        {
                            if (swarmManager.paths[i] != null)
                            {
                                pathToFollow = i;
                                followAttacker = false;
                                break;
                            }
                        }
                        if (pathToFollow == -1)
                        {
                            pathToFollow = 0;
                            followAttacker = true;
                        }
                        else
                        {
                            int best = 0;
                            float bestDis = 69699696969696969;
                            float dist;

                            for (int i = 0; i < swarmManager.paths[pathToFollow].Length; i++)
                            {
                                Vector3.DistanceSquared(ref Position, ref swarmManager.paths[pathToFollow][i], out dist);

                                if (dist < 2.5f * 2.5f)
                                {
                                    best = i;
                                    break;
                                }
                                else
                                {
                                    if (bestDis > dist)
                                    {
                                        best = i;
                                        bestDis = dist;
                                    }
                                }
                            }

                            nodeAt = best;
                        }
                    }
                }
            }

            if (health > 0)
            {
                millisecondsInSwing += gameTime.ElapsedGameTime.Milliseconds;

                if (followAttacker)
                {
                    if (attacker.dead)
                    {
                        if (swarmManager.paths[pathToFollow] != null)
                            followAttacker = false;//go back to path////////////////find other players to follow
                    }
                    else
                    {
                        target = attacker.position;
                    }
                }
                else
                {




                    if (nodeAt < swarmManager.paths[pathToFollow].Length)
                    {
                        target = swarmManager.paths[pathToFollow][nodeAt];
                        Vector3.DistanceSquared(ref target, ref Position, out distance);
                        if (distance < .1f * .1f)//distance squraed
                        {
                            nodeAt++;
                            if (nodeAt < swarmManager.paths[pathToFollow].Length)
                                target = swarmManager.paths[pathToFollow][nodeAt];
                            else
                                nodeAt--;
                        }
                    }
                    else
                        target = swarmManager.paths[pathToFollow][swarmManager.paths[pathToFollow].Length - 1];
                }

                dontMove = false;
                if ((followAttacker && attacker.dead == false) || (swarmManager.paths[pathToFollow] != null && nodeAt == swarmManager.paths[pathToFollow].Length - 1))
                {
                    Vector3.DistanceSquared(ref Position, ref target, out distance);
                    if (distance < 2f * 2f)//distance squarde
                    {
                        //attack //we shouldnt have to send packets of there attacks as the exact thing should be happing on the othre
                        if (attackCoolDown <= 0)
                        {
                            if (followAttacker && distance < 1.75f * 1.75f)
                            {
                                float damage = (float)MathHelper.Lerp(10, 18, swarmManager.currentRound / 15f);

                                Packet.PacketWriter.Write(Packet.PACKETID_SWARMIEATTACKEDPLAYER);
                                Packet.PacketWriter.Write(damage);

                                try
                                {
                                    if(MinerOfDuty.Session.FindGamerById(attacker.PlayerID) != null)
                                        swarmManager.game.Me.SendData(Packet.PacketWriter, Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                                            MinerOfDuty.Session.FindGamerById(attacker.PlayerID));
                                }
                                catch (Exception) { }


                                attackCoolDown = 300;
                                BeenSlowMoving = 0;
                            }
                            else if (followAttacker == false)
                            {
                                swarmManager.AttackBlock(1.5f, pathToFollow);
                                attackCoolDown = 300;
                                BeenSlowMoving = 0;
                            }
                        }
                        else
                        {
                            attackCoolDown -= gameTime.ElapsedGameTime.Milliseconds;
                            BeenSlowMoving = 0;
                        }
                        dontMove = true;
                        //   return;
                    }
                }


                #region movement code
                x.X = target.X;
                x.Z = target.Z;

                z.X = Position.X;
                z.Z = Position.Z;

                movement = x - z;
                movement.Normalize();

                bodyRot = (float)Math.Atan2(movement.X, movement.Z);
                bodyRot += MathHelper.Pi;


                movement.X = movement.X * (float)(5 * gameTime.ElapsedGameTime.TotalSeconds);
                movement.Z = movement.Z * (float)(5 * gameTime.ElapsedGameTime.TotalSeconds);

                amountMoved.X = 0;
                amountMoved.Y = 0;



                if (target.Y - Position.Y > .25f)
                {
                    if (jumping == false)
                        jumping = true;
                }



                if (jumping)
                    yVelocity += 1.75f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity
                else
                    yVelocity += -1f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity

                movement.Y = MathHelper.Clamp(yVelocity, -4 * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, 4 * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                yVelocity -= movement.Y;

                dir.X = 0;
                dir.Y = movement.Y;
                dir.Z = 0;
                Vector3.Add(ref bb.Min, ref dir, out test.Min);
                Vector3.Add(ref bb.Max, ref dir, out test.Max);
                BoundingBox.CreateMerged(ref bb, ref test, out test);
                if (!swarmManager.game.Terrain.CheckForCollisionZombie(ref test, ref Position) && !swarmManager.CheckForCollision(ref test, this))
                {
                    bb.Min.X += dir.X;
                    bb.Min.Y += dir.Y;
                    bb.Min.Z += dir.Z;

                    bb.Max.X += dir.X;
                    bb.Max.Y += dir.Y;
                    bb.Max.Z += dir.Z;

                    Position.X += dir.X;
                    Position.Y += dir.Y;
                    Position.Z += dir.Z;
                    onGround = false;//we could move down were not on ground
                }
                else
                {

                    onGround = true;
                }

                if (onGround == false)
                {
                    timeInAir += .5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (jumping)
                        yVelocity += -.06f * speed * 3 * timeInAir; //gravity
                    else
                        yVelocity += -.06f * speed * timeInAir; //gravity
                }
                else
                {
                    jumping = false;
                    timeInAir = 0;
                }

                if (dontMove == false)
                {
                    if (movement.X != 0)
                    {
                        dir.X = movement.X;
                        dir.Y = 0;
                        dir.Z = 0;
                        Vector3.Add(ref bb.Min, ref dir, out test.Min);
                        Vector3.Add(ref bb.Max, ref dir, out test.Max);
                        BoundingBox.CreateMerged(ref bb, ref test, out test);
                        if ((swarmManager.GoldBlocks[pathToFollow].IsTakenBy == this ? true : !swarmManager.CheckForCollision(ref test, this)) && !swarmManager.game.Terrain.CheckForCollisionZombie(ref test, ref Position))
                        {

                            bb.Min.X += dir.X;
                            bb.Min.Y += dir.Y;
                            bb.Min.Z += dir.Z;

                            bb.Max.X += dir.X;
                            bb.Max.Y += dir.Y;
                            bb.Max.Z += dir.Z;

                            Position.X += dir.X;
                            Position.Y += dir.Y;
                            Position.Z += dir.Z;

                            amountMoved.X += dir.X;
                            amountMoved.Y += dir.Z;
                        }
                    }

                    if (movement.Z != 0)
                    {
                        dir.X = 0;
                        dir.Y = 0;
                        dir.Z = movement.Z;
                        Vector3.Add(ref bb.Min, ref dir, out test.Min);
                        Vector3.Add(ref bb.Max, ref dir, out test.Max);
                        BoundingBox.CreateMerged(ref bb, ref test, out test);
                        if ((swarmManager.GoldBlocks[pathToFollow].IsTakenBy == this ? true : !swarmManager.CheckForCollision(ref test, this)) && !swarmManager.game.Terrain.CheckForCollisionZombie(ref test, ref Position))
                        {

                            bb.Min.X += dir.X;
                            bb.Min.Y += dir.Y;
                            bb.Min.Z += dir.Z;

                            bb.Max.X += dir.X;
                            bb.Max.Y += dir.Y;
                            bb.Max.Z += dir.Z;

                            Position.X += dir.X;
                            Position.Y += dir.Y;
                            Position.Z += dir.Z;
                            amountMoved.X += dir.X;
                            amountMoved.Y += dir.Z;
                        }
                    }
                }


                movementSpeed = amountMoved.LengthSquared() / lengthSqed;

                if (movementSpeed < .1f)
                {
                    BeenSlowMoving++;
                    if (BeenSlowMoving >= 370)
                    {
                        BeenSlowMoving = 0;
                        dir.X = movement.X;
                        dir.Y = 0;
                        dir.Z = movement.Z;
                        Vector3.Add(ref bb.Min, ref dir, out test.Min);
                        Vector3.Add(ref bb.Max, ref dir, out test.Max);
                        BoundingBox.CreateMerged(ref bb, ref test, out test);
                        Vector3 blockTarget = new Vector3();
                        if (swarmManager.game.Terrain.CheckForCollisionZombie(ref test, ref Position, ref blockTarget))
                        {
                            if (swarmManager.game.Terrain.blocks[(int)blockTarget.X, (int)blockTarget.Y, (int)blockTarget.Z] == Block.BLOCKID_SAND ||
                                swarmManager.game.Terrain.blocks[(int)blockTarget.X, (int)blockTarget.Y, (int)blockTarget.Z] == Block.BLOCKID_DIRT)
                                swarmManager.game.Terrain.RemoveBlock((int)blockTarget.X, (int)blockTarget.Y, (int)blockTarget.Z);
                        }
                    }
                    else if (BeenSlowMoving >= 30)
                    {
                        //Damage(1000);
                        //get block
                        dir.X = movement.X;
                        dir.Y = 0;
                        dir.Z = movement.Z;
                        Vector3.Add(ref bb.Min, ref dir, out test.Min);
                        Vector3.Add(ref bb.Max, ref dir, out test.Max);
                        BoundingBox.CreateMerged(ref bb, ref test, out test);
                        Vector3 blockTarget = new Vector3();
                        if (swarmManager.game.Terrain.CheckForCollisionZombie(ref test, ref Position, ref blockTarget))
                        {
                            if (swarmManager.game.Terrain.blocks[(int)blockTarget.X, (int)blockTarget.Y, (int)blockTarget.Z] == Block.BLOCKID_SAND ||
                                swarmManager.game.Terrain.blocks[(int)blockTarget.X, (int)blockTarget.Y, (int)blockTarget.Z] == Block.BLOCKID_DIRT)
                                dontMove = true;//swarmManager.game.Terrain.RemoveBlock((int)blockTarget.X, (int)blockTarget.Y, (int)blockTarget.Z);
                        }
                    }
                }
                else
                    BeenSlowMoving = 0;


                #endregion
            }
            else
            {
                #region for falling when dead
                if (onGround == false)
                {
                    yVelocity += -1f * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; //gravity

                    movement.Y = MathHelper.Clamp(yVelocity, -4 * speed * (float)gameTime.ElapsedGameTime.TotalSeconds, 4 * speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                    yVelocity -= movement.Y;

                    dir.X = 0;
                    dir.Y = movement.Y;
                    dir.Z = 0;
                    Vector3.Add(ref bb.Min, ref dir, out test.Min);
                    Vector3.Add(ref bb.Max, ref dir, out test.Max);
                    BoundingBox.CreateMerged(ref bb, ref test, out test);
                    if (!swarmManager.game.Terrain.CheckForCollision(ref test, ref Position))
                    {

                        bb.Min.X += dir.X;
                        bb.Min.Y += dir.Y;
                        bb.Min.Z += dir.Z;

                        bb.Max.X += dir.X;
                        bb.Max.Y += dir.Y;
                        bb.Max.Z += dir.Z;

                        Position.X += dir.X;
                        Position.Y += dir.Y;
                        Position.Z += dir.Z;

                        onGround = false;//we could move down were not on ground
                    }
                    else
                        onGround = true;

                    if (onGround == false)
                    {
                        timeInAir += .5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (jumping)
                            yVelocity += -.06f * speed * 3 * timeInAir; //gravity
                        else
                            yVelocity += -.06f * speed * timeInAir; //gravity
                    }
                    else
                    {
                        jumping = false;
                        timeInAir = 0;
                    }
                }
                #endregion
                timeInDeath += gameTime.ElapsedGameTime.Milliseconds;
                if (fallRot < MathHelper.PiOver2)
                    fallRot += (float)(MathHelper.Pi * gameTime.ElapsedGameTime.TotalSeconds);
            }

        }
        private static float lengthSqed = new Vector2(4.5f * 0.0155f, 4.5f * 0.0155f).LengthSquared();
    }
}
