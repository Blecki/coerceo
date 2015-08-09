using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Gem.Render
{
    public class RenderContext
    {
		private Effect Effect;
        public GraphicsDevice Device { get; private set; }
        public Gem.Render.ICamera Camera;
        public bool LightingEnabled = true;

		private Gem.Geo.Vertex[] VertexBuffer = new Gem.Geo.Vertex[8];

        private Vector3[] LightPosition = new Vector3[16];
        private float[] LightFalloff = new float[16];
        private Vector3[] LightColor = new Vector3[16];

        public Texture2D White { get; private set; }
        public Texture2D Black { get; private set; }
        public Texture2D NeutralNormals { get; private set; }

        private ImmediateMode2D _immediateMode;
        public ImmediateMode2D ImmediateMode
        {
            get
            {
                ApplyChanges();
                return _immediateMode;
            }
        }

        public RenderContext(Effect Effect, GraphicsDevice Device)
        {
            this.Effect = Effect;
            this.Device = Device;

            for (var i = 0; i < 8; ++i) SetLight(i, Vector3.Zero, 0.0f, Vector3.Zero);

            White = new Texture2D(Device, 1, 1, false, SurfaceFormat.Color);
            White.SetData(new Color[] { new Color(255, 255, 255, 255) }); 
            
            Black = new Texture2D(Device, 1, 1, false, SurfaceFormat.Color);
            Black.SetData(new Color[] { new Color(0, 0, 0, 255) });

            NeutralNormals = new Texture2D(Device, 1, 1, false, SurfaceFormat.Color);
            NeutralNormals.SetData(new Color[] { new Color(128, 128, 255, 255) });

            _immediateMode = new ImmediateMode2D(Device);
        }

        public void SetLight(int Index, Vector3 Position, float Falloff, Vector3 Color)
        {
            if (Index < 0 || Index >= 8) throw new IndexOutOfRangeException();
            LightPosition[Index] = Position;
            LightFalloff[Index] = Falloff;
            LightColor[Index] = Color;
        }

        public int ActiveLightCount { set { Effect.Parameters["ActiveLights"].SetValue((float)value); } }

        public void ApplyChanges()
        {
            Effect.CurrentTechnique = Effect.Techniques[LightingEnabled ? 0 : 1];

            Effect.Parameters["LightPosition"].SetValue(LightPosition);
            Effect.Parameters["LightFalloff"].SetValue(LightFalloff);
            Effect.Parameters["LightColor"].SetValue(LightColor);

            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);

            Effect.CurrentTechnique.Passes[0].Apply();
        }

        public Matrix World
        {
            set
            {
				Effect.Parameters["World"].SetValue(value);
				Effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(value)));
            }
        }

        public virtual Texture2D Texture
        {
            set
            {
			    Effect.Parameters["Texture"].SetValue(value);
            }
        }

        public virtual Texture2D NormalMap { set { if (Effect.Parameters["NormalMap"] != null) Effect.Parameters["NormalMap"].SetValue(value); } }

        public Matrix UVTransform
        {
            set
            {
                Effect.Parameters["UVTransform"].SetValue(value);
            }
        }

        public virtual Vector3 Color
        {
            set
            {
				Effect.Parameters["DiffuseColor"].SetValue(new Vector4(value, 1.0f));
            }
        }

        public float Alpha
        {
            set
            {
                Effect.Parameters["Alpha"].SetValue(value);
            }
        }

        public void Draw(Geo.IMesh Mesh)
        {
            ApplyChanges();
            Mesh.Render(Device);
        }

        //public void Draw(Gem.Geo.CompiledModel model)
        //{
        //    ApplyChanges();

        //    Device.SetVertexBuffer(model.verticies);
        //    Device.Indices = model.indicies;
        //    Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, model.verticies.VertexCount,
        //        0, System.Math.Min(model.primitiveCount, 65535));
        //}

        //public void Draw(Gem.Geo.Mesh mesh)
        //{
        //    if (mesh.verticies.Length > 0)
        //    {
        //        ApplyChanges();

        //        Device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, mesh.verticies, 0, mesh.verticies.Length,
        //            mesh.indicies, 0, mesh.indicies.Length / 3);
        //    }
        //}

        //public void DrawLine(Vector3 v0, Vector3 v1, float width, Vector3 n)
        //{
        //    var tangent = v1 - v0;
        //    tangent.Normalize();
        //    var offset = Vector3.Transform(tangent, Matrix.CreateFromAxisAngle(n, (float)(System.Math.PI / 2)));
        //    offset.Normalize();
        //    offset *= width / 2;

        //    VertexBuffer[0].Position = v0 + offset;
        //    VertexBuffer[1].Position = v1 + offset;
        //    VertexBuffer[2].Position = v0 - offset;
        //    VertexBuffer[3].Position = v1 + offset;
        //    VertexBuffer[4].Position = v1 - offset;
        //    VertexBuffer[5].Position = v0 - offset;

        //    for (int i = 0; i < 6; ++i) VertexBuffer[i].Normal = n;

        //    device.DrawUserPrimitives(PrimitiveType.TriangleList, VertexBuffer, 0, 2);
        //}

        //public void DrawPoint()
        //{
        //    VertexBuffer[0].Position = new Vector3(-0.5f, -0.5f, 0);
        //    VertexBuffer[1].Position = new Vector3(-0.5f, 0.5f, 0);
        //    VertexBuffer[2].Position = new Vector3(0.5f, -0.5f, 0);
        //    VertexBuffer[3].Position = new Vector3(0.5f, 0.5f, 0);
        //    for (int i = 0; i < 6; ++i) VertexBuffer[i].Normal = Vector3.UnitZ;

        //    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        //}

        //public void DrawSprite(Geo.Mesh mesh)
        //{
        //    if (mesh.verticies.Length > 0)
        //    {
        //        //spriteEffect.CurrentTechnique.Passes[0].Apply();
        //        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, mesh.verticies, 0, mesh.verticies.Length,
        //            mesh.indicies, 0, mesh.indicies.Length / 3);
        //    }
        //}

        public void DrawLineIM(Vector3 v0, Vector3 v1)
        {
            VertexBuffer[0].Position = v0;
            VertexBuffer[1].Position = v1;
            Device.DrawUserPrimitives(PrimitiveType.LineList, VertexBuffer, 0, 1);
        }

        public void DrawLines(Gem.Geo.Mesh mesh)
        {
            ApplyChanges();

            if (mesh.lineIndicies == null) mesh.PrepareLineIndicies();

            if (mesh.lineIndicies != null && mesh.verticies.Length > 0)
                Device.DrawUserIndexedPrimitives(PrimitiveType.LineList, mesh.verticies, 0, mesh.verticies.Length,
                    mesh.lineIndicies, 0, mesh.lineIndicies.Length / 2);
        }

        //public void Draw(Gem.Geo.WireframeMesh Mesh)
        //{
        //    ApplyChanges();
        //    Device.DrawUserIndexedPrimitives(PrimitiveType.LineList, Mesh.verticies, 0, Mesh.verticies.Length, Mesh.indicies, 0, Mesh.indicies.Length / 2);
        //}


        //public void DrawHitbox(float X, float Y, float W, float H)
        //{

        //    VertexBuffer[0].Position = new Vector3(X, Y, 0);
        //    VertexBuffer[1].Position = new Vector3(X, Y + H, 0);
        //    VertexBuffer[2].Position = new Vector3(X + W, Y, 0);
        //    VertexBuffer[3].Position = new Vector3(X + W, Y + H, 0);
        //    for (int i = 0; i < 4; ++i) VertexBuffer[i].Normal = Vector3.UnitZ;

        //    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, VertexBuffer, 0, 2);
        //}
	}

}
