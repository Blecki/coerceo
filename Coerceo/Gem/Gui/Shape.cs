using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Gem.Gui
{
    public abstract class Shape
    {
        public virtual bool PointInside(Vector2 Point) { return false; }
        public virtual void Render(Gem.Render.RenderContext Context) { }
        public virtual Shape Transform(Matrix M) { return this; }
    }

    public class CompositeShape : Shape
    {
        private Shape[] Children;

        public CompositeShape(params Shape[] Children)
        {
            this.Children = Children;
        }

        public CompositeShape(IEnumerable<Shape> Children)
        {
            this.Children = Children.ToArray();
        }

        public override bool PointInside(Vector2 Point)
        {
            foreach (var child in Children)
                if (child.PointInside(Point)) return true;
            return false;
        }

        public override void Render(Render.RenderContext Context)
        {
            foreach (var child in Children)
                child.Render(Context);
        }

        public override Shape Transform(Matrix M)
        {
            return new CompositeShape(Children.Select(c => c.Transform(M)));
        }
    }

    public class QuadShape : Shape
    {
        private Vector2[] Points;
        private Vector2[] TwizledPoints;

        public QuadShape(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
        {
            this.Points = new Vector2[] { A, B, C, D };
            this.TwizledPoints = new Vector2[] { A, B, D, C };
        }

        public QuadShape(IEnumerable<Vector2> points)
        {
            this.Points = points.ToArray();
            this.TwizledPoints = new Vector2[] { Points[0], Points[1], Points[3], Points[2] };
        }

        public QuadShape(float X, float Y, float W, float H)
        {
            this.Points = new Vector2[] {
                new Vector2(X,Y),
                new Vector2(X + W, Y),
                new Vector2(X + W, Y + H),
                new Vector2(X, Y + H)
            };

            this.TwizledPoints = new Vector2[] { Points[0], Points[1], Points[3], Points[2] };
        }

        public override bool PointInside(Vector2 Point)
        {
            return Gem.Math.Intersection.PointInPolygonAngle(Points, Point);
        }

        public override void Render(Render.RenderContext Context)
        {
            Context.ImmediateMode.Quad(TwizledPoints, TwizledPoints);
        }

        public override Shape Transform(Matrix M)
        {
            return new QuadShape(Points.Select(p => Vector2.Transform(p, M)));
        }
    }
}
