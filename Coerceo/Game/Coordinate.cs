using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game
{
    public struct Coordinate
    {
        internal byte Data;

        public Coordinate(byte Data)
        {
            this.Data = Data;
        }

        public Coordinate(byte Tile, byte Triangle)
        {
            this.Data = (byte)((Tile << 3) + Triangle);
        }

        public byte Tile { get { return (byte)((Data & 0xF8) >> 3); } }
        public byte Triangle { get { return (byte)(Data & 0x07); } }

        public bool Invalid { get { return Tile > 19; } }

        public static bool operator ==(Coordinate x, Coordinate y)
        {
            return x.Data == y.Data;
        }

        public static bool operator !=(Coordinate x, Coordinate y)
        {
            return x.Data != y.Data;
        }

    }

    public struct Rel
    {
        public byte Direction;
        public byte Triangle;

        public Rel(byte D, byte T)
        {
            this.Direction = D;
            this.Triangle = T;
        }
    }
}
