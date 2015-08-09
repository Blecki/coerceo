using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace HexDemo
{
    class Hex
    {
        private float radius;
        private float width;
        private float halfHeight;
        private float height;
        private float columnWidth;

        public Hex(float radius)
        {
            this.radius = radius;
            this.width = 2 * radius;
            this.columnWidth = 1.5f * radius;
            this.halfHeight = (float)Math.Sqrt((radius * radius) - ((radius / 2) * (radius / 2)));
            this.height = 2 * this.halfHeight;
        }

        public Vector2 TileOrigin(int x, int y)
        {
            return new Vector2(
                x * columnWidth,
                (y * height) + ((x % 2 != 0) ? halfHeight : 0));
        }
    }
}