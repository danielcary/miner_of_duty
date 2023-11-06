using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace Miner_Of_Duty.Menus
{
    public struct HeadVertexType : IVertexType
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Color EyeColor;
        public Color HairColor;
        public Color SkinColor;

        public HeadVertexType(Vector3 position, Vector2 texCoord, Color eyeColor, Color hairColor, Color skinColor)
        {
            Position = position;
            TexCoord = texCoord;
            EyeColor = eyeColor;
            HairColor = hairColor;
            SkinColor = skinColor;
        }

        private static VertexDeclaration vd = new VertexDeclaration(new VertexElement[]{
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 5 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(sizeof(float) * 5 + 8, VertexElementFormat.Color, VertexElementUsage.Color, 2)
        });

        public VertexDeclaration VertexDeclaration
        {
            get { return vd; }
        }

        public static short[] HeadIndices;
        public static PositionNormalTextureColor[] HeadVertices;

        public static void CreateHead(GraphicsDevice gd, out VertexBuffer vb, out IndexBuffer ib, Color eyeColor, Color hairColor, Color skinColor)
        {
            vb = new VertexBuffer(gd, vd, 24, BufferUsage.None);
            ib = new IndexBuffer(gd, IndexElementSize.SixteenBits, 36, BufferUsage.None);
            ib.SetData<short>(HeadIndices);

            HeadVertexType[] headVerts = new HeadVertexType[24];
            for (int i = 0; i < 24; i++)
            {
                headVerts[i] = new HeadVertexType(HeadVertices[i].Position, HeadVertices[i].TexCoord, eyeColor, hairColor, skinColor);
            }

            vb.SetData<HeadVertexType>(headVerts);
        }
    }

    public struct BodyVertexType : IVertexType
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Color ShirtColor;
        public Color PantsColor;
        public Color SkinColor;

        public BodyVertexType(Vector3 position, Vector2 texCoord, Color shirtColor, Color pantsColor, Color skinColor)
        {
            Position = position;
            TexCoord = texCoord;
            ShirtColor = shirtColor;
            PantsColor = pantsColor;
            SkinColor = skinColor;
        }

        private static VertexDeclaration vd = new VertexDeclaration(new VertexElement[]{
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 5 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(sizeof(float) * 5 + 8, VertexElementFormat.Color, VertexElementUsage.Color, 2)
        });

        public VertexDeclaration VertexDeclaration
        {
            get { return vd; }
        }

        public static short[] BodyIndices;
        public static PositionNormalTextureColor[] BodyVertices;

        public static void CreateBody(GraphicsDevice gd, out VertexBuffer vb, out IndexBuffer ib, Color shirtColor, Color pantsColor, Color skinColor)
        {
            vb = new VertexBuffer(gd, vd, 24, BufferUsage.None);
            ib = new IndexBuffer(gd, IndexElementSize.SixteenBits, 36, BufferUsage.None);
            ib.SetData<short>(BodyIndices);

            BodyVertexType[] verts = new BodyVertexType[24];
            for (int i = 0; i < 24; i++)
            {
                verts[i] = new BodyVertexType(BodyVertices[i].Position, BodyVertices[i].TexCoord, shirtColor, pantsColor, skinColor);
            }

            vb.SetData<BodyVertexType>(verts);
        }
    }

    public struct ArmVertexType : IVertexType
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Color ShirtColor;
        public Color SkinColor;

        public ArmVertexType(Vector3 position, Vector2 texCoord, Color shirtColor, Color skinColor)
        {
            Position = position;
            TexCoord = texCoord;
            ShirtColor = shirtColor;
            SkinColor = skinColor;
        }

        private static VertexDeclaration vd = new VertexDeclaration(new VertexElement[]{
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 5 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1)
        });

        public VertexDeclaration VertexDeclaration
        {
            get { return vd; }
        }

        public static short[] ArmIndices;
        public static PositionNormalTextureColor[] ArmVertices;

        public static void CreateArm(GraphicsDevice gd, out VertexBuffer vb, out IndexBuffer ib, Color shirtColor, Color skinColor)
        {
            vb = new VertexBuffer(gd, vd, 24, BufferUsage.None);
            ib = new IndexBuffer(gd, IndexElementSize.SixteenBits, 36, BufferUsage.None);
            ib.SetData<short>(ArmIndices);

            ArmVertexType[] verts = new ArmVertexType[24];
            for (int i = 0; i < 24; i++)
            {
                verts[i] = new ArmVertexType(ArmVertices[i].Position, ArmVertices[i].TexCoord, shirtColor, skinColor);
            }

            vb.SetData<ArmVertexType>(verts);
        }
    }

    public struct LegVertexType : IVertexType
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Color PantsColor;
        public Color SkinColor;

        public LegVertexType(Vector3 position, Vector2 texCoord, Color pantsColor, Color skinColor)
        {
            Position = position;
            TexCoord = texCoord;
            PantsColor = pantsColor;
            SkinColor = skinColor;
        }

        private static VertexDeclaration vd = new VertexDeclaration(new VertexElement[]{
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 5 + 4, VertexElementFormat.Color, VertexElementUsage.Color, 1)
        });

        public VertexDeclaration VertexDeclaration
        {
            get { return vd; }
        }

        public static short[] LegIndices;
        public static PositionNormalTextureColor[] LegVertices;

        public static void CreateLeg(GraphicsDevice gd, out VertexBuffer vb, out IndexBuffer ib, Color pantsColor, Color skinColor)
        {
            vb = new VertexBuffer(gd, vd, 24, BufferUsage.None);
            ib = new IndexBuffer(gd, IndexElementSize.SixteenBits, 36, BufferUsage.None);
            ib.SetData<short>(LegIndices);

            LegVertexType[] verts = new LegVertexType[24];
            for (int i = 0; i < 24; i++)
            {
                verts[i] = new LegVertexType(LegVertices[i].Position, LegVertices[i].TexCoord, pantsColor, skinColor);
            }

            vb.SetData<LegVertexType>(verts);
        }
    }

    public class EditCharacter : IMenuOwner
    {
        public static Texture2D HeadBaseTexture, HeadEyeTexture, HeadHairTexture, HeadSkinTexture;
        public static Texture2D BodyPantsTexture, BodyShirtTexture, BodySkinTexture;
        public static Texture2D ArmShirtTexture, ArmSkinTexture;
        public static Texture2D LegPantsTexture, LegSkinTexture;

        private VertexBuffer HeadVertexBuffer, BodyVertexBuffer, ArmVertexBuffer, LegVertexBuffer;
        private IndexBuffer HeadIndexBuffer, BodyIndexBuffer, ArmIndexBuffer, LegIndexBuffer;

        private Matrix View, Projection;
        private GraphicsDevice gd;

        public static void CreateBodyFromProfile(GraphicsDevice gd, out VertexBuffer headVB, out IndexBuffer headIB, out VertexBuffer bodyVB, out IndexBuffer bodyIB,
            out VertexBuffer armVB, out IndexBuffer armIB, out VertexBuffer legVB, out IndexBuffer legIB, PlayerProfile profile)
        {
            HeadVertexType.CreateHead(gd, out headVB, out headIB, profile.EyeColor, profile.HairColor, profile.SkinColor);
            BodyVertexType.CreateBody(gd, out bodyVB, out bodyIB, profile.ShirtColor, profile.PantsColor, profile.SkinColor);
            ArmVertexType.CreateArm(gd, out armVB, out armIB, profile.ShirtColor, profile.SkinColor);
            LegVertexType.CreateLeg(gd, out legVB, out legIB, profile.PantsColor, profile.SkinColor);
        }

        public static void CreateBodyFromColor(GraphicsDevice gd, out VertexBuffer headVB, out IndexBuffer headIB, out VertexBuffer bodyVB, out IndexBuffer bodyIB,
           out VertexBuffer armVB, out IndexBuffer armIB, out VertexBuffer legVB, out IndexBuffer legIB, Color eyeColor, Color hairColor, Color pantsColor, Color shirtColor, Color skinColor)
        {
            HeadVertexType.CreateHead(gd, out headVB, out headIB, eyeColor, hairColor, skinColor);
            BodyVertexType.CreateBody(gd, out bodyVB, out bodyIB, shirtColor, pantsColor, skinColor);
            ArmVertexType.CreateArm(gd, out armVB, out armIB, shirtColor, skinColor);
            LegVertexType.CreateLeg(gd, out legVB, out legIB, pantsColor, skinColor);
        }

        private Menu menu, color;
        private Menu.BackPressed menuBack;
        private RenderTarget2D renderTarget;

        private enum Editing { None, Hair, Eye, Skin, Shirt, Pants }
        private Editing item;

        private void Rebuild(Editing part)
        {
            switch (part)
            {
                case Editing.Eye:
                    HeadVertexBuffer.Dispose();
                    HeadIndexBuffer.Dispose();
                    HeadVertexType.CreateHead(gd, out HeadVertexBuffer, out HeadIndexBuffer, MinerOfDuty.CurrentPlayerProfile.EyeColor, MinerOfDuty.CurrentPlayerProfile.HairColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    break;
                case Editing.Skin:
                    HeadVertexBuffer.Dispose();
                    HeadIndexBuffer.Dispose();
                    HeadVertexType.CreateHead(gd, out HeadVertexBuffer, out HeadIndexBuffer, MinerOfDuty.CurrentPlayerProfile.EyeColor, MinerOfDuty.CurrentPlayerProfile.HairColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    BodyVertexBuffer.Dispose();
                    BodyIndexBuffer.Dispose();
                    BodyVertexType.CreateBody(gd, out BodyVertexBuffer, out BodyIndexBuffer, MinerOfDuty.CurrentPlayerProfile.ShirtColor, MinerOfDuty.CurrentPlayerProfile.PantsColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    LegVertexBuffer.Dispose();
                    LegIndexBuffer.Dispose();
                    LegVertexType.CreateLeg(gd, out LegVertexBuffer, out LegIndexBuffer, MinerOfDuty.CurrentPlayerProfile.PantsColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    ArmVertexBuffer.Dispose();
                    ArmIndexBuffer.Dispose();
                    ArmVertexType.CreateArm(gd, out ArmVertexBuffer, out ArmIndexBuffer, MinerOfDuty.CurrentPlayerProfile.ShirtColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    break;
                case Editing.Pants:
                    BodyVertexBuffer.Dispose();
                    BodyIndexBuffer.Dispose();
                    BodyVertexType.CreateBody(gd, out BodyVertexBuffer, out BodyIndexBuffer, MinerOfDuty.CurrentPlayerProfile.ShirtColor, MinerOfDuty.CurrentPlayerProfile.PantsColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    LegVertexBuffer.Dispose();
                    LegIndexBuffer.Dispose();
                    LegVertexType.CreateLeg(gd, out LegVertexBuffer, out LegIndexBuffer, MinerOfDuty.CurrentPlayerProfile.PantsColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    break;
                case Editing.Hair:
                    HeadVertexType.CreateHead(gd, out HeadVertexBuffer, out HeadIndexBuffer, MinerOfDuty.CurrentPlayerProfile.EyeColor, MinerOfDuty.CurrentPlayerProfile.HairColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    break;
                case Editing.Shirt:
                    ArmVertexBuffer.Dispose();
                    ArmIndexBuffer.Dispose();
                    ArmVertexType.CreateArm(gd, out ArmVertexBuffer, out ArmIndexBuffer, MinerOfDuty.CurrentPlayerProfile.ShirtColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    BodyVertexBuffer.Dispose();
                    BodyIndexBuffer.Dispose();
                    BodyVertexType.CreateBody(gd, out BodyVertexBuffer, out BodyIndexBuffer, MinerOfDuty.CurrentPlayerProfile.ShirtColor, MinerOfDuty.CurrentPlayerProfile.PantsColor, MinerOfDuty.CurrentPlayerProfile.SkinColor);
                    break;
            }
        }

        public EditCharacter(GraphicsDevice gd, Menu.BackPressed back)
        {
            this.gd = gd;

            PlayerProfile.PlayerProfileReloadedEvent += new PlayerProfile.PlayerProfileReloaded(PlayerProfile_PlayerProfileReloadedEvent);

            renderTarget = new RenderTarget2D(gd, 1280, 720, false, SurfaceFormat.Color, DepthFormat.Depth16);

            menuBack = back;
            CreateBodyFromProfile(gd, out HeadVertexBuffer, out HeadIndexBuffer, out BodyVertexBuffer, out BodyIndexBuffer, out ArmVertexBuffer, out ArmIndexBuffer, out LegVertexBuffer, out LegIndexBuffer, MinerOfDuty.CurrentPlayerProfile);

            menu = new Menu(OptionChose, BackPressed, new MenuElement[]{ 
                new MenuElement("back","Back"), 
                new MenuElement("hair", "Hair Color"), 
                new MenuElement("eye", "Eye Color"), 
                new MenuElement("skin", "Skin Color"), 
                new MenuElement("shirt", "Shirt Color"), 
                new MenuElement("pants", "Pants Color")
            });

            color = new Menu(OptionChose, BackPressed, new MenuElement[] { 
                new MenuElement("back", "Back"), 
                new ValueMenuElement("red", "Red: ", 100, "", 0, 255, 0), 
                new ValueMenuElement("green", "Green: ", 100, "", 0, 255, 0),
                new ValueMenuElement("blue", "Blue: ", 100, "", 0, 255, 0), 
            });

            (color["red"] as ValueMenuElement).ValueChangedEvent += EditCharacter_ValueChangedEvent;
            (color["blue"] as ValueMenuElement).ValueChangedEvent += EditCharacter_ValueChangedEvent;
            (color["green"] as ValueMenuElement).ValueChangedEvent += EditCharacter_ValueChangedEvent;

            View = Matrix.CreateLookAt(new Vector3(-1.75f, .5f, -3.5f) - new Vector3(-.9f, .5f, 1), -new Vector3(-.9f, .5f, 1), Vector3.UnitY);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1280f / 720f, .1f, 100f);

            item = Editing.None;
        }

        private void EditCharacter_ValueChangedEvent(string id)
        {
            if (item == Editing.Hair)
                MinerOfDuty.CurrentPlayerProfile.HairColor = new Color((int)(color["red"] as ValueMenuElement).Value, (int)(color["green"] as ValueMenuElement).Value, (int)(color["blue"] as ValueMenuElement).Value);
            else if (item == Editing.Eye)
                MinerOfDuty.CurrentPlayerProfile.EyeColor = new Color((int)(color["red"] as ValueMenuElement).Value, (int)(color["green"] as ValueMenuElement).Value, (int)(color["blue"] as ValueMenuElement).Value);
            else if (item == Editing.Pants)
                MinerOfDuty.CurrentPlayerProfile.PantsColor = new Color((int)(color["red"] as ValueMenuElement).Value, (int)(color["green"] as ValueMenuElement).Value, (int)(color["blue"] as ValueMenuElement).Value);
            else if (item == Editing.Shirt)
                MinerOfDuty.CurrentPlayerProfile.ShirtColor = new Color((int)(color["red"] as ValueMenuElement).Value, (int)(color["green"] as ValueMenuElement).Value, (int)(color["blue"] as ValueMenuElement).Value);
            else if (item == Editing.Skin)
                MinerOfDuty.CurrentPlayerProfile.SkinColor = new Color((int)(color["red"] as ValueMenuElement).Value, (int)(color["green"] as ValueMenuElement).Value, (int)(color["blue"] as ValueMenuElement).Value);

            Rebuild(item);
        }

        private void PlayerProfile_PlayerProfileReloadedEvent()
        {
            Rebuild(Editing.Skin);
        }

        public void GoHome()
        {
            item = Editing.None;
            menu.SelectFirst();
            color.SelectFirst();
        }

        private void OptionChose(object sender, string id)
        {
            if (sender == menu)
            {
                if (id == "back")
                {
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                    menu.SelectFirst();
                    menuBack.Invoke(this);
                }
                else
                {
                    switch (id)
                    {
                        case "hair":
                            item = Editing.Hair;
                            (color["red"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.HairColor.R;
                            (color["blue"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.HairColor.B;
                            (color["green"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.HairColor.G;
                            break;
                        case "eye":
                            item = Editing.Eye;
                            (color["red"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.EyeColor.R;
                            (color["blue"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.EyeColor.B;
                            (color["green"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.EyeColor.G;
                            break;
                        case "skin":
                            item = Editing.Skin;
                            (color["red"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.SkinColor.R;
                            (color["blue"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.SkinColor.B;
                            (color["green"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.SkinColor.G;
                            break;
                        case "shirt":
                            item = Editing.Shirt;
                            (color["red"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.ShirtColor.R;
                            (color["blue"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.ShirtColor.B;
                            (color["green"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.ShirtColor.G;
                            break;
                        case "pants":
                            item = Editing.Pants;
                            (color["red"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.PantsColor.R;
                            (color["blue"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.PantsColor.B;
                            (color["green"] as ValueMenuElement).Value = MinerOfDuty.CurrentPlayerProfile.PantsColor.G;
                            break;
                    }
                }
            }
            else if (sender == color)
            {
                if (id == "back")
                {
                    MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                    color.SelectFirst();
                    item = Editing.None;
                }
            }
        }

        private void BackPressed(object sender)
        {
            if (item == Editing.None)
            {
                menuBack.Invoke(this);
                MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
            }
            else
            {
                MinerOfDuty.SaveDevice.SaveAsync("Miner Of Duty Player Profiles", SignedInGamer.SignedInGamers[(PlayerIndex)Input.ControllingPlayer].Gamertag, new EasyStorage.FileAction(MinerOfDuty.CurrentPlayerProfile.Save));
                item = Editing.None;
            }
        }

        public void Update(short timeInMillseconds)
        {
            if (item != Editing.None)
                color.Update(timeInMillseconds);
            else
                menu.Update(timeInMillseconds);

        }

        public void Render()
        {

            gd.SetRenderTarget(renderTarget);
            gd.Clear(Color.Transparent);
            Vector3 a;

            try
            {
                Resources.BlockEffect.Parameters["GrayAmount"].SetValue(0f);

                //Head
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyHead"];
                Resources.BlockEffect.Parameters["View"].SetValue(View);
                Resources.BlockEffect.Parameters["Projection"].SetValue(Projection);
                Resources.BlockEffect.Parameters["CameraPosition"].SetValue(Vector3.Zero);

                Resources.BlockEffect.Parameters["Body1"].SetValue(HeadBaseTexture);
                Resources.BlockEffect.Parameters["Body2"].SetValue(HeadEyeTexture);
                Resources.BlockEffect.Parameters["Body3"].SetValue(HeadHairTexture);
                Resources.BlockEffect.Parameters["Body4"].SetValue(HeadSkinTexture);
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.Identity);
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.Indices = HeadIndexBuffer;
                gd.SetVertexBuffer(HeadVertexBuffer);
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, HeadVertexBuffer.VertexCount, 0, 12);


                //Body
                a = new Vector3(0, -.47f, 0);
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyBody"];
                Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.BodyShirtTexture);
                Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.BodyPantsTexture);
                Resources.BlockEffect.Parameters["Body3"].SetValue(EditCharacter.BodySkinTexture);
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(a));
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.Indices = BodyIndexBuffer;
                gd.SetVertexBuffer(BodyVertexBuffer);
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, BodyVertexBuffer.VertexCount, 0, 12);


                //Right Arm
                a = new Vector3(.325f, -.56f, 0);
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(a));
                Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
                Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.Indices = ArmIndexBuffer;
                gd.SetVertexBuffer(ArmVertexBuffer);
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);


                //Left Arm
                a = new Vector3(-.325f, -.56f, 0);
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(a));
                Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.ArmShirtTexture);
                Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.ArmSkinTexture);
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.Indices = ArmIndexBuffer;
                gd.SetVertexBuffer(ArmVertexBuffer);
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ArmVertexBuffer.VertexCount, 0, 12);


                //Left Leg
                a = new Vector3(-.125f, -1.1f, 0);
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(a));
                Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
                Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.Indices = LegIndexBuffer;
                gd.SetVertexBuffer(LegVertexBuffer);
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);

                //Right Leg
                a = new Vector3(.125f, -1.1f, 0);
                Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["BodyArmLeg"];
                Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(a));
                Resources.BlockEffect.Parameters["Body1"].SetValue(EditCharacter.LegPantsTexture);
                Resources.BlockEffect.Parameters["Body2"].SetValue(EditCharacter.LegSkinTexture);
                Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
                gd.Indices = LegIndexBuffer;
                gd.SetVertexBuffer(LegVertexBuffer);
                gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, LegVertexBuffer.VertexCount, 0, 12);
            }
            catch { }

            gd.SetRenderTarget(null);
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Resources.EditCharacterBackgroundTexture, Vector2.Zero, Color.White);
            sb.Draw(renderTarget, Vector2.Zero, Color.White);
            if (item != Editing.None)
            {
                sb.DrawString(Resources.Font,
                    item == Editing.Hair ? "HAIR COLOR" :
                    item == Editing.Eye ? "EYE COLOR" :
                    item == Editing.Pants ? "PANTS COLOR" :
                    item == Editing.Shirt ? "SHIRT COLOR" :
                    "SKIN COLOR",
                    new Vector2(300, 100), Color.White);
                color.Draw(sb);
            }
            else
                menu.Draw(sb);
        }
    }
}
