using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Miner_Of_Duty.Game
{
    public struct VertexTextureLight : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureUV;
        public float Light;

        private static readonly VertexDeclaration vd = new VertexDeclaration(new VertexElement[] { 
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
            new VertexElement(4 * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(4 * 5, VertexElementFormat.Byte4, VertexElementUsage.Color, 0) });
        public VertexDeclaration VertexDeclaration
        {
            get { return vd; }
        }

    }

    public struct VertexWaterTextureLight : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureUV;
        public float Light;

        private static readonly VertexDeclaration vd = new VertexDeclaration(new VertexElement[] { 
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
            new VertexElement(4 * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(4 * 5, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1) });
        public VertexDeclaration VertexDeclaration
        {
            get { return vd; }
        }

    }

    public struct VertexPositionTextureSideLight : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureUV;
        /// <summary>
        /// RGB correspond to XYZ and A corresponds to the side of the light
        /// </summary>
        public float Lighting;
        public HalfVector2 TexCoords;


        private static readonly VertexDeclaration vd = new VertexDeclaration(new VertexElement[] { 
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), 
            new VertexElement(4 * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(4 * 5, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(4 * 6, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 2) });
        public VertexDeclaration VertexDeclaration
        {
            get { return vd; }
        }
    }

}