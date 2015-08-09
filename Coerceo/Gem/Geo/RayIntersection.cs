using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Mesh
    {
        static private float AngleBetweenVectors(Vector3 A, Vector3 B)
        {
            A.Normalize();
            B.Normalize();
            float DotProduct = Vector3.Dot(A, B);
            DotProduct = MathHelper.Clamp(DotProduct, -1.0f, 1.0f);
            float Angle = (float)System.Math.Acos(DotProduct);
            return Angle;
        }

        public static bool IsPointOnFace(Vector3 p, Vector3[] Verticies)
        {
            var accumulatedAngle = 0.0f;

            for (int first = 0; first < 3; ++first)
            {
                var second = (first == 2 ? 0 : first + 1);
                var angle = AngleBetweenVectors(Verticies[first] - p, Verticies[second] - p);
                if (angle < 0) angle *= -1;
                accumulatedAngle += angle;
            }

            if (System.Math.Abs((System.Math.PI * 2) - accumulatedAngle) < 0.1f) 
                return true;
            return false;
        }

        public class RayIntersectionResult
        {
            public bool Intersects;
            public float Distance;
            public Object Tag;
            public Vector2 UV;
        }

        public RayIntersectionResult RayIntersection(Ray ray)
        {
            var closestIntersection = new RayIntersectionResult { Distance = float.PositiveInfinity, Intersects = false };

            for (int vindex = 0; vindex < indicies.Length; vindex += 3)
            {
                var p = indicies.Skip(vindex).Take(3).Select(i => verticies[i].Position).ToArray();
                var plane = new Plane(p[0], p[1], p[2]);
                var intersectionDistance = ray.Intersects(plane);
                if (intersectionDistance.HasValue && intersectionDistance.Value < closestIntersection.Distance)
                {
                    var intersectionPoint = ray.Position + (ray.Direction * intersectionDistance.Value);
                    if (IsPointOnFace(intersectionPoint, p))
                    {
                        closestIntersection.Distance = intersectionDistance.Value;
                        closestIntersection.Intersects = true;

                        var tc = indicies.Skip(vindex).Take(3).Select(i => verticies[i].TextureCoordinate).ToArray();

                        var bv = p.Select(v => v - intersectionPoint).ToArray();
                        var area = Vector3.Cross(p[0] - p[1], p[0] - p[2]).Length();
                        var baryArea = new float[] {
                            Vector3.Cross(bv[1], bv[2]).Length() / area,
                            Vector3.Cross(bv[2], bv[0]).Length() / area,
                            Vector3.Cross(bv[0], bv[1]).Length() / area
                        };

                        var uv = (tc[0] * baryArea[0]) + (tc[1] * baryArea[1]) + (tc[2] * baryArea[2]);

                        closestIntersection.UV = uv;
                    }
                }
            }

            return closestIntersection;
        }
    }
}
