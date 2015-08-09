using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class ImmediateMode2D
    {
        GraphicsDevice Device;
        Gem.Geo.Vertex[] VertexBuffer = new Gem.Geo.Vertex[8];
        Vector2[] TempVectors = new Vector2[8];

        public ImmediateMode2D(GraphicsDevice Device)
        {
            this.Device = Device;

            for (int i = 0; i < VertexBuffer.Length; ++i)
            {
                VertexBuffer[i].Normal = Vector3.UnitZ;
                VertexBuffer[i].Tangent = Vector3.UnitX;
                VertexBuffer[i].BiNormal = Vector3.UnitY;
            }
        }

        public void Quad(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float depth = 0)
        {
            VertexBuffer[0].Position = new Vector3(v0, depth);
            VertexBuffer[1].Position = new Vector3(v1, depth);
            VertexBuffer[2].Position = new Vector3(v2, depth);
            VertexBuffer[3].Position = new Vector3(v3, depth);
            VertexBuffer[0].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[1].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 1);            

            Device.DrawUserPrimitives<Gem.Geo.Vertex>(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void Glyph(float x, float y, float w, float h, double tx, double ty, double tw, double th, float depth = 0)
        {
            VertexBuffer[0].Position = new Vector3(x - 0.5f, y - 0.5f, depth);
            VertexBuffer[1].Position = new Vector3(x + w - 0.5f, y - 0.5f, depth);
            VertexBuffer[2].Position = new Vector3(x - 0.5f, y + h - 0.5f, depth);
            VertexBuffer[3].Position = new Vector3(x + w - 0.5f, y + h - 0.5f, depth);
			VertexBuffer[0].TextureCoordinate = new Vector2((float)tx, (float)ty);
			VertexBuffer[1].TextureCoordinate = new Vector2((float)(tx + tw), (float)ty);
			VertexBuffer[2].TextureCoordinate = new Vector2((float)tx, (float)(ty + th));
			VertexBuffer[3].TextureCoordinate = new Vector2((float)(tx + tw), (float)(ty + th));

            Device.DrawUserPrimitives<Gem.Geo.Vertex>(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void Quad(float x, float y, float w, float h, float depth = 0)
        {
            Quad(new Vector2(x, y),
                new Vector2(x + w, y),
                new Vector2(x, y + h),
                new Vector2(x + w, y + h), depth);
        }

        public void Quad(Rectangle rect, float depth = 0)
        {
            Quad(new Vector2(rect.X, rect.Y),
               new Vector2(rect.X + rect.Width, rect.Y),
               new Vector2(rect.X, rect.Y + rect.Height),
               new Vector2(rect.X + rect.Width, rect.Y + rect.Height), depth);
        }

        public void Quad(Vector2[] verts, Vector2[] texcoords, float z = 0.0f)
        {
            for (int i = 0; i < 4; ++i)
            {
                VertexBuffer[i].Position = new Vector3(verts[i], z);
                VertexBuffer[i].TextureCoordinate = texcoords[i];
            }

            Device.DrawUserPrimitives<Gem.Geo.Vertex>(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void OrientedSprite(Vector2 Orientation)
        {
            VertexBuffer[2].Position = new Vector3(Orientation.X - Orientation.Y, Orientation.X + Orientation.Y, 0.0f);
            VertexBuffer[3].Position = new Vector3(-(Orientation.X + Orientation.Y), Orientation.X - Orientation.Y, 0.0f);
            VertexBuffer[1].Position = new Vector3(Orientation.Y - Orientation.X, -(Orientation.X + Orientation.Y), 0.0f);
            VertexBuffer[0].Position = new Vector3(Orientation.X + Orientation.Y, Orientation.Y - Orientation.X, 0.0f);

            VertexBuffer[0].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[1].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(1, 1);

            Device.DrawUserPrimitives<Gem.Geo.Vertex>(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void SpriteAt(Vector2 Position, Vector2 Orientation, float Z)
        {
            VertexBuffer[2].Position = new Vector3(Position.X + Orientation.X - Orientation.Y, Position.Y + Orientation.X + Orientation.Y, Z);
            VertexBuffer[3].Position = new Vector3(Position.X + -(Orientation.X + Orientation.Y), Position.Y + Orientation.X - Orientation.Y, Z);
            VertexBuffer[1].Position = new Vector3(Position.X + Orientation.Y - Orientation.X, Position.Y + -(Orientation.X + Orientation.Y), Z);
            VertexBuffer[0].Position = new Vector3(Position.X + Orientation.X + Orientation.Y, Position.Y + Orientation.Y - Orientation.X, Z);

            VertexBuffer[0].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[1].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(1, 1);

            Device.DrawUserPrimitives<Gem.Geo.Vertex>(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void Sprite(bool Flip)
        {
            VertexBuffer[0].Position = new Vector3(-0.5f, -0.5f, 0);
            VertexBuffer[1].Position = new Vector3(-0.5f, 0.5f, 0);
            VertexBuffer[2].Position = new Vector3(0.5f, -0.5f, 0);
            VertexBuffer[3].Position = new Vector3(0.5f, 0.5f, 0);

            if (!Flip)
            {
                VertexBuffer[0].TextureCoordinate = new Vector2(0, 0);
                VertexBuffer[1].TextureCoordinate = new Vector2(0, 1);
                VertexBuffer[2].TextureCoordinate = new Vector2(1, 0);
                VertexBuffer[3].TextureCoordinate = new Vector2(1, 1);
            }
            else
            {
                VertexBuffer[0].TextureCoordinate = new Vector2(1, 1);
                VertexBuffer[1].TextureCoordinate = new Vector2(1, 0);
                VertexBuffer[3].TextureCoordinate = new Vector2(0, 0);
                VertexBuffer[2].TextureCoordinate = new Vector2(0, 1);
            }

            Device.DrawUserPrimitives<Gem.Geo.Vertex>(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void Box(Vector2 Position, Vector2 Scale, float Angle, float Width)
        {
            TempVectors[0] = new Vector2(-0.5f, -0.5f);
            TempVectors[1] = new Vector2(-0.5f, 0.5f);
            TempVectors[2] = new Vector2(0.5f, 0.5f);
            TempVectors[3] = new Vector2(0.5f, -0.5f);

            DrawLine(TempVectors[0], TempVectors[1], Width);
            DrawLine(TempVectors[1], TempVectors[2], Width);
            DrawLine(TempVectors[2], TempVectors[3], Width);
            DrawLine(TempVectors[3], TempVectors[0], Width);
        }        

        public void DrawLine(Vector2 Start, Vector2 End, float Width)
        {
            var LineNormal = Vector2.Normalize(End - Start);
            LineNormal = new Vector2(LineNormal.Y, -LineNormal.X);
            LineNormal *= Width * 0.5f;

            VertexBuffer[0].Position = new Vector3(Start + LineNormal, 0);
            VertexBuffer[1].Position = new Vector3(Start - LineNormal, 0);
            VertexBuffer[3].Position = new Vector3(End - LineNormal, 0);
            VertexBuffer[2].Position = new Vector3(End + LineNormal, 0);
            VertexBuffer[0].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[1].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(1, 1);

            Device.DrawUserPrimitives<Gem.Geo.Vertex>(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }

        public void DrawLine(Vector3 Start, Vector3 End, float Width)
        {
            var LineNormal = Vector3.Normalize(End - Start);
            LineNormal = new Vector3(LineNormal.Y, -LineNormal.X, LineNormal.Z);
            LineNormal *= Width * 0.5f;

            VertexBuffer[0].Position = Start + LineNormal;
            VertexBuffer[1].Position = Start - LineNormal;
            VertexBuffer[3].Position = End - LineNormal;
            VertexBuffer[2].Position = End + LineNormal;
            VertexBuffer[0].TextureCoordinate = new Vector2(0, 1);
            VertexBuffer[1].TextureCoordinate = new Vector2(0, 0);
            VertexBuffer[3].TextureCoordinate = new Vector2(1, 0);
            VertexBuffer[2].TextureCoordinate = new Vector2(1, 1);

            Device.DrawUserPrimitives<Gem.Geo.Vertex>(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        }
    }
}
