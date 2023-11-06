using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Miner_Of_Duty.Game.Editor
{
    public class SpawnPoint
    {
        public Vector3 Position;
        public string Name;
        public Color color;

        public SpawnPoint(GraphicsDevice gd, string name)
        {
            nameOrigin = Resources.Font.MeasureString(name) / 2;
            baseEffect = new BasicEffect(gd);
            baseEffect.TextureEnabled = true;
            baseEffect.VertexColorEnabled = true;
            Name = name;
            Position = Vector3.Zero;
        }

        private BasicEffect baseEffect;
        private Vector2 nameOrigin;
        private float dist;
        public void Draw(Camera cam, SpriteBatch sb)
        {

            baseEffect.World = Matrix.CreateConstrainedBillboard(Position + (Vector3.Up * .5f), cam.Position, Vector3.Down, null, null);
            baseEffect.View = cam.ViewMatrix;
            baseEffect.Projection = cam.ProjMatrix;

            sb.Begin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, baseEffect);
            sb.DrawString(Resources.Font, Name, Vector2.Zero, color, 0, nameOrigin, .0075f, 0, 0);
            sb.End();
        }

        /// <summary>
        /// Calculates if the desired spawn position is valid
        /// </summary
        /// <param name="pos">This block right above the ground</param>
        /// <returns>True if valid</returns>
        public static bool CalculateIfIsGoodSpawn(Terrain terrain, Vector3i pos)
        {
            for (int x = pos.X - 3; x <= pos.X + 3; x++)
            {
                for (int z = pos.Z - 3; z <= pos.Z + 3; z++)
                {
                    if (x < 1 || x >= 127
                        || z < 1 || z >= 127)
                        return false;

                    if (terrain.blocks[x, pos.Y, z] != Block.BLOCKID_AIR && terrain.blocks[x, pos.Y, z] != Block.BLOCKID_LAVA)
                        return false;
                }
            }

            if (pos.Y + 1 >= 64)
                return true;

            for (int x = pos.X - 2; x <= pos.X + 2; x++)
            {
                for (int z = pos.Z - 2; z <= pos.Z + 2; z++)
                {
                    if (terrain.blocks[x, pos.Y + 1, z] != Block.BLOCKID_AIR && terrain.blocks[x, pos.Y, z] != Block.BLOCKID_LAVA)
                        return false;
                }
            }

            return true;
        }

    }
}
