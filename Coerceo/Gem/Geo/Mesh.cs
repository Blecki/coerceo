using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public struct Vertex : IVertexType
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;
        public Vector3 BiNormal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(44, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 2)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    public interface IMesh
    {
        void Render(GraphicsDevice Device);
    }

    public partial class Mesh : IMesh
    {
        public Vertex[] verticies;
        public short[] indicies;
        public short[] lineIndicies;
        public Object Tag;

        public int VertexCount { get { return verticies.Length; } }

        public Vertex GetVertex(int i)
        {
            return verticies[i];
        }

        public void PrepareLineIndicies()
        {
            lineIndicies = new short[indicies.Length * 2];
            for (int i = 0; i < indicies.Length; i += 3)
            {
                lineIndicies[i * 2] = indicies[i];
                lineIndicies[i * 2 + 1] = indicies[i + 1];
                lineIndicies[i * 2 + 2] = indicies[i + 1];
                lineIndicies[i * 2 + 3] = indicies[i + 2];
                lineIndicies[i * 2 + 4] = indicies[i + 2];
                lineIndicies[i * 2 + 5] = indicies[i];
            }
        }

        public void Render(GraphicsDevice Device)
        {
            Device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verticies, 0, verticies.Length,
                                indicies, 0, indicies.Length / 3);
        }
    }
}