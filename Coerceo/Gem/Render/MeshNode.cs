using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gem.Geo;

namespace Gem.Render
{
    public class MeshNode : ISceneNode
    {
        public Mesh Mesh;
        public Vector3 Color = Vector3.One;
        public Texture2D Texture = null;

        public MeshNode(Mesh Mesh, Texture2D Texture, Euler Orientation = null)
        {
            this.Mesh = Mesh;
            this.Texture = Texture;
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        public override void Draw(RenderContext Context)
        {
            Context.Color = Color;
            if (Texture != null) Context.Texture = Texture;
            else Context.Texture = Context.White;
            Context.NormalMap = Context.NeutralNormals;
            Context.World = WorldTransform;
            Context.ApplyChanges();
            Context.Draw(Mesh);
        }
    }
}
