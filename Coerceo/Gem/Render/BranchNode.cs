using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gem.Render
{
    public class BranchNode : ISceneNode
    {
        public BranchNode(Euler Orientation = null)
        {
            this.Orientation = Orientation;
            if (this.Orientation == null) this.Orientation = new Euler();
        }

        private List<ISceneNode> children = new List<ISceneNode>();

        public void Add(ISceneNode child) { children.Add(child); }
        public void Remove(ISceneNode child) { children.Remove(child); }
        public IEnumerator<ISceneNode> GetEnumerator() { return children.GetEnumerator(); }

        public override void UpdateWorldTransform(Matrix M)
        {
            base.UpdateWorldTransform(M);
            foreach (var child in this) child.UpdateWorldTransform(WorldTransform);
        }

        public override void PreDraw(float ElapsedSeconds, RenderContext Context)
        {
            foreach (var child in this) child.PreDraw(ElapsedSeconds, Context);
        }

        public override void Draw(RenderContext Context)
        {
            foreach (var child in this) child.Draw(Context);
        }

        public override void CalculateLocalMouse(Ray MouseRay, Action<ISceneNode, float> HoverCallback)
        {
            foreach (var child in this) child.CalculateLocalMouse(MouseRay, HoverCallback);
        }
    }
}
