using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Game
{
    public class PitfallBlock
    {
        private float timeStanding = 0;
        public byte stage = 0;
        public byte X, Y, Z;


        public void Reset()
        {
            timeStanding = 0;
            stage = 0;
        }

        /// <summary>
        /// Call this when the player is standing on the block
        /// </summary>
        /// <param name="timeInMilliseconds"></param>
        /// <returns>true if it should break</returns>
        public bool AddStandingTime(float timeInMilliseconds)
        {
            timeStanding += timeInMilliseconds;

            float percetage = timeStanding / 100f;

            if (percetage < .1f)
                stage = 1;
            else if (percetage < .2f)
                stage = 2;
            else if (percetage < .3f)
                stage = 3;
            else if (percetage < .4f)
                stage = 4;
            else if (percetage < .5f)
                stage = 5;
            else if (percetage < .6f)
                stage = 6;
            else if (percetage < .7f)
                stage = 7;
            else if (percetage < .8f)
                stage = 8;
            else if (percetage < .9f)
                stage = 9;
            else
                stage = 10;

            if (timeStanding >= 100)
                return true;
            else
                return false;
        }

        private static BoundingBox terrainBlock = new BoundingBox(new Vector3(-.5f, -.5f, -.5f), new Vector3(.5f, .5f, .5f));
        private static Vector3 position;
        public static bool IsPlayerOnBlock(ref Vector3i blockPos, ref Vector3 playerPos, ref Vector3 distanceFromGround)
        {
            terrainBlock.Max.X += blockPos.X;
            terrainBlock.Max.Y += blockPos.Y;
            terrainBlock.Max.Z += blockPos.Z;
            terrainBlock.Min.X += blockPos.X;
            terrainBlock.Min.Y += blockPos.Y;
            terrainBlock.Min.Z += blockPos.Z;

            position = playerPos;
            position -= distanceFromGround;
            position.Y -= .1f;

            ContainmentType type;
            terrainBlock.Contains(ref position, out type);

            terrainBlock.Max.X -= blockPos.X;
            terrainBlock.Max.Y -= blockPos.Y;
            terrainBlock.Max.Z -= blockPos.Z;
            terrainBlock.Min.X -= blockPos.X;
            terrainBlock.Min.Y -= blockPos.Y;
            terrainBlock.Min.Z -= blockPos.Z;

            if (type == ContainmentType.Disjoint)
                return false;
            else
                return true;
        }


        public static void BeginRender(GraphicsDevice gd)
        {
            gd.SetVertexBuffer(Resources.SelectionBuffer);
            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["NoLight"];
            Resources.BlockEffect.Parameters["Texture0"].SetValue(Resources.SELECTIONTEXTURE);
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
            gd.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);

        }

        public void Render(GraphicsDevice gd, Camera camera)
        {
            if (stage <= 0)
                return;

            Resources.BlockEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(X, Y, Z));
            Resources.BlockEffect.CurrentTechnique.Passes[0].Apply();
           

            switch (stage)
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

        public static void EndRender(GraphicsDevice gd)
        {
            Resources.BlockEffect.CurrentTechnique = Resources.BlockEffect.Techniques["TexturedLighting"];
            Resources.BlockEffect.Parameters["World"].SetValue(Matrix.Identity);
            gd.SetVertexBuffer(null);
        }

    }
}
