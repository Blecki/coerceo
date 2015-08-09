using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class ISceneNode
    {
        protected Matrix WorldTransform;
        public Euler Orientation { get; set; }

        public virtual void UpdateWorldTransform(Matrix M)
        {
            WorldTransform = M * Orientation.Transform;
        }

        public virtual void PreDraw(float ElapsedSeconds, RenderContext Context) { }
        public virtual void Draw(RenderContext Context) { }
        public virtual void CalculateLocalMouse(Ray MouseRay, Action<ISceneNode, float> HoverCallback) { }
        public virtual Action GetClickAction() { return null; }
    }
}
