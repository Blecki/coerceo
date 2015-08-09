using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gem.Geo;
using Gem;

namespace Gem.Render
{
    public class NormalMapMeshNode : ISceneNode
    {
        public bool Hidden = false;
        public Mesh Mesh;
        public Vector3 Color = Vector3.One;
        public Texture2D Texture = null;
        public Texture2D NormalMap = null;
        public Mesh HiliteMesh = null;
        public Vector3 HiliteColor = Vector3.One;
        public Matrix UVTransform = Matrix.Identity;
        public bool Hilite = false;
        public Action ClickAction;
        public float Alpha = 1.0f;

        internal bool MouseHover = false;
        internal Vector2 LocalMouse = Vector2.Zero;

        public Object Tag;

        public NormalMapMeshNode(Euler Orientation = null) 
        { 
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        public override void Draw(Gem.Render.RenderContext context)
        {
            if (Hidden) return;

            context.Color = Color;
            if (Texture != null) context.Texture = Texture;
            if (NormalMap != null) context.NormalMap = NormalMap;
            else context.NormalMap = context.NeutralNormals;
            context.World = WorldTransform;
            context.UVTransform = UVTransform;
            context.LightingEnabled = true;
            context.Alpha = Alpha;
            context.ApplyChanges();
            context.Draw(Mesh);
            context.NormalMap = context.NeutralNormals;

            if (Hilite && HiliteMesh != null)
            {
                context.Texture = context.White;
                context.Color = HiliteColor;
                context.LightingEnabled = false;
                context.ApplyChanges();
                context.Draw(HiliteMesh);
            }

            context.UVTransform = Matrix.Identity;
        }

        public override void CalculateLocalMouse(Ray MouseRay, Action<Gem.Render.ISceneNode, float> HoverCallback)
        {
            MouseHover = false;
            Hilite = false;

            if (Hidden) return;

            MouseRay.Direction = Vector3.Normalize(MouseRay.Direction);

            var inverseTransform = Matrix.Invert(Orientation.Transform);
            var localMouseSource = Vector3.Transform(MouseRay.Position, inverseTransform);

            var forwardPoint = MouseRay.Position + MouseRay.Direction;
            forwardPoint = Vector3.Transform(forwardPoint, inverseTransform);
            
            var localMouse = new Ray(localMouseSource, forwardPoint - localMouseSource);

            var intersection = Mesh.RayIntersection(localMouse);
            if (intersection.Intersects)
            {
                HoverCallback(this, intersection.Distance * Orientation.Scale.Length());
            }
        }

        public override Action GetClickAction()
        {
            MouseHover = true;
            return ClickAction;
        }

    }
}
