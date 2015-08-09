using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Gen
    {
        public static void CopyIndicies(short[] into, int at, short[] source)
        {
            for (int i = 0; i < source.Length; ++i)
                into[at + i] = source[i];
        }

        public static Mesh Copy(Mesh mesh)
        {
            var result = new Mesh();

            result.verticies = new Vertex[mesh.verticies.Length];
            for (int i = 0; i < mesh.verticies.Length; ++i) result.verticies[i] = mesh.verticies[i];
            result.indicies = new short[mesh.indicies.Length];
            CopyIndicies(result.indicies, 0, mesh.indicies);
            return result;
        }

        public static void Transform(Mesh mesh, Matrix m, int start, int count)
        {
            if (start < 0) start = mesh.verticies.Length - start;
            for (int i = start; i < start + count; ++i)
                mesh.verticies[i].Position = Vector3.Transform(mesh.verticies[i].Position, m);

            Vector3 scale;
            Vector3 trans;
            Quaternion rot;
            m.Decompose(out scale, out rot, out trans);
            for (int i = start; i < start + count; ++i)
            {
                mesh.verticies[i].Normal = Vector3.Transform(mesh.verticies[i].Normal, rot);
                mesh.verticies[i].Tangent = Vector3.Transform(mesh.verticies[i].Tangent, rot);
                mesh.verticies[i].BiNormal = Vector3.Transform(mesh.verticies[i].BiNormal, rot);
            }
        }

        public static Mesh TransformCopy(Mesh mesh, Matrix m, int start, int count)
        {
            var result = Copy(mesh);
            Transform(result, m, start, count);
            return result;
        }

        public static void Transform(Mesh mesh, Matrix m)
        {
            Transform(mesh, m, 0, mesh.verticies.Length);
        }

        public static Mesh TransformCopy(Mesh mesh, Matrix m)
        {
            return TransformCopy(mesh, m, 0, mesh.verticies.Length);
        }

        public static void Morph(Mesh mesh, Func<Vector3, Vector3> func)
        {
            for (int i = 0; i < mesh.verticies.Length; ++i)
                mesh.verticies[i].Position = func(mesh.verticies[i].Position);
        }

        public static Mesh MorphCopy(Mesh mesh, Func<Vector3, Vector3> func)
        {
            var result = Copy(mesh);
            Morph(result, func);
            return result;
        }

        public static Vector3 CalculateNormal(Mesh part, int a, int b, int c)
        {
            return Vector3.Normalize(Vector3.Cross(part.GetVertex(b).Position - part.GetVertex(a).Position,
                     part.GetVertex(c).Position - part.GetVertex(a).Position));
        }

        public static Tuple<Vector3, Vector3> CalculateTangentAndBinormal(Mesh part, int a, int b, int c)
        {
            var _a = part.GetVertex(a);
            var _b = part.GetVertex(b);
            var _c = part.GetVertex(c);

            //// Calculate the two vectors for this face.
            //vector1[0] = vertex2.x - vertex1.x;
            //vector1[1] = vertex2.y - vertex1.y;
            //vector1[2] = vertex2.z - vertex1.z;
            
            var v1 = part.GetVertex(b).Position - part.GetVertex(a).Position;

            //vector2[0] = vertex3.x - vertex1.x;
            //vector2[1] = vertex3.y - vertex1.y;
            //vector2[2] = vertex3.z - vertex1.z;

            var v2 = part.GetVertex(c).Position - part.GetVertex(a).Position;

            //// Calculate the tu and tv texture space vectors.
            //tuVector[0] = vertex2.tu - vertex1.tu;
            //tuVector[1] = vertex3.tu - vertex1.tu;

            var tu = new Vector2(
                part.GetVertex(b).TextureCoordinate.X - part.GetVertex(a).TextureCoordinate.X,
                part.GetVertex(c).TextureCoordinate.X - part.GetVertex(a).TextureCoordinate.X);

            //tvVector[0] = vertex2.tv - vertex1.tv;
            //tvVector[1] = vertex3.tv - vertex1.tv;

            var tv = new Vector2(
                part.GetVertex(b).TextureCoordinate.Y - part.GetVertex(a).TextureCoordinate.Y,
                part.GetVertex(c).TextureCoordinate.Y - part.GetVertex(a).TextureCoordinate.Y);

            //// Calculate the denominator of the tangent/binormal equation.
            //den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            var den = 1.0f / (tu.X * tv.Y - tu.Y * tv.X);

            //// Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            //tangent.x = (tvVector[1] * vector1[0] - tvVector[0] * vector2[0]) * den;
            //tangent.y = (tvVector[1] * vector1[1] - tvVector[0] * vector2[1]) * den;
            //tangent.z = (tvVector[1] * vector1[2] - tvVector[0] * vector2[2]) * den;

            var tangent = new Vector3(
                tv.Y * v1.X - tv.X * v2.X,
                tv.Y * v1.Y - tv.X * v2.Y,
                tv.Y * v1.Z - tv.X * v2.Z) * den;

            //binormal.x = (tuVector[0] * vector2[0] - tuVector[1] * vector1[0]) * den;
            //binormal.y = (tuVector[0] * vector2[1] - tuVector[1] * vector1[1]) * den;
            //binormal.z = (tuVector[0] * vector2[2] - tuVector[1] * vector1[2]) * den;

            var binormal = new Vector3(
                tu.X * v2.X - tu.Y * v1.X,
                tu.X * v2.Y - tu.Y * v1.Y,
                tu.X * v2.Z - tu.Y * v1.Z) * den;

            //// Calculate the length of this normal.
            //length = sqrt((tangent.x * tangent.x) + (tangent.y * tangent.y) + (tangent.z * tangent.z));

            //// Normalize the normal and then store it
            //tangent.x = tangent.x / length;
            //tangent.y = tangent.y / length;
            //tangent.z = tangent.z / length;

            //// Calculate the length of this normal.
            //length = sqrt((binormal.x * binormal.x) + (binormal.y * binormal.y) + (binormal.z * binormal.z));

            //// Normalize the normal and then store it
            //binormal.x = binormal.x / length;
            //binormal.y = binormal.y / length;
            //binormal.z = binormal.z / length;

            tangent.Normalize();
            binormal.Normalize();

            return Tuple.Create(tangent, binormal);
        }

        public static Vector3 CalculateNormal(Vector3 Tangent, Vector3 BiNormal)
        {
            return Vector3.Normalize(Vector3.Cross(Tangent, BiNormal));
        }
        
        public static Vertex[] GetVerticies(Mesh mesh, int startIndex, int Length)
        {
            var r = new Vertex[Length];
            for (int i = 0; i < Length; ++i) r[i] = mesh.verticies[i + startIndex];
            return r;
        }

        public static BoundingBox CalculateBoundingBox(Mesh mesh)
        {
            Vector3 minimum = mesh.verticies[0].Position;
            Vector3 maximum = mesh.verticies[0].Position;
            foreach (var vert in mesh.verticies)
            {
                if (vert.Position.X < minimum.X) minimum.X = vert.Position.X;
                if (vert.Position.Y < minimum.Y) minimum.Y = vert.Position.Y;
                if (vert.Position.Z < minimum.Z) minimum.Z = vert.Position.Z;
                if (vert.Position.X > maximum.X) maximum.X = vert.Position.X;
                if (vert.Position.Y > maximum.Y) maximum.Y = vert.Position.Y;
                if (vert.Position.Z > maximum.Z) maximum.Z = vert.Position.Z;
            }

            return new BoundingBox(minimum, maximum);
        }

        public static Mesh Merge(params Mesh[] parts)
        {
            var result = new Mesh();

            result.verticies = new Vertex[parts.Sum((p) => p.verticies.Length)];
            result.indicies = new short[parts.Sum((p) => p.indicies.Length)];

            int vCount = 0;
            int iCount = 0;
            foreach (var part in parts)
            {
                for (int i = 0; i < part.verticies.Length; ++i) result.verticies[i + vCount] = part.verticies[i];
                for (int i = 0; i < part.indicies.Length; ++i) result.indicies[i + iCount] = (short)(part.indicies[i] + vCount);
                vCount += part.verticies.Length;
                iCount += part.indicies.Length;
            }

            return result;
        }

    }
}