using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /*
    Encoding a move: [00000][000] [000][00][000]
     Tile -------------+      |     |   |    |     
      Triangle ---------------+     |   |    |     
        Direction-------------------+   |    |     
         Movement type -----------------+    |     
          Reserved --------------------------+     
    */

    public enum MoveType
    {
        MovePiece = 0,
        Trade = 1,
        Resign = 2
    }

    public struct Move
    {
        ushort Data;

        public Move(Coordinate Coordinate, byte Direction, MoveType Type)
        {
            Data = (ushort)((Coordinate.Data << 8) + (Direction << 5) + ((byte)Type << 3));
        }

        public Move(byte[] bytes, int offset)
        {
            Data = (ushort)(((int)bytes[0 + offset] << 8) + bytes[1 + offset]);
        }

        public Coordinate Coordinate { get { return new Coordinate((byte)(Data >> 8)); } }
        public byte Tile { get { return Coordinate.Tile; } }
        public byte Triangle { get { return Coordinate.Triangle; } }

        public byte Direction { get { return (byte)((Data >> 5) & 0x07); } }
        public byte Type { get { return (byte)((Data >> 3) & 0x03); } }

        public byte[] Bytes { get { return BitConverter.GetBytes(Data); } }

        public static bool operator ==(Move x, Move y)
        {
            return x.Data == y.Data;
        }

        public static bool operator !=(Move x, Move y)
        {
            return x.Data != y.Data;
        }
    }
}
