using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Gem
{
    /// <summary>
    /// Represent all the transformations that can be applied to an object.
    /// </summary>
    public class Euler
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Scale = Vector3.One;
        public Vector3 Orientation = Vector3.Zero;

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateScale(Scale)
                    * Matrix.CreateFromYawPitchRoll(Orientation.X, Orientation.Y, Orientation.Z)
                    * Matrix.CreateTranslation(Position);
            }
        }
    }
}
