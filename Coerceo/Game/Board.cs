using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /*
    Encoding the entire game board: [STATE HEADER] [TILE X 19]
    State Header:    [0] [0000000]
     Whos turn next --+    |
      Reserved ------------+
    */

    public struct StateHeader
    {
        public byte Data;

        public StateHeader(byte Data)
        {
            this.Data = Data;
        }

        public byte WhoseTurnNext { get { return (byte)((Data & 0x80) >> 7); } }
        public byte TurnDepth { get { return (byte)(Data & 0x7F); } }

    }

    public unsafe struct Board
    {
        fixed byte Data[20];

        public static Board Empty() { return new Board(new byte[20], 0); }

        public Board(byte[] Source, int offset)
        {
            fixed (byte* x = Data)
                for (int i = 0; i < 20; ++i)
                    x[i] = Source[i + offset];
        }

        public Board(Board Source)
        {
            fixed (byte* x = Data)
                for (var i = 0; i < 20; ++i)
                    x[i] = Source.Data[i];
        }

        public StateHeader Header { get { fixed (byte* x = Data) return new StateHeader(x[0]); } }

        public IEnumerable<Tile> Tiles { get { for (byte i = 0; i < 19; ++i) yield return GetTile(i); } }
        public IEnumerable<byte> Triangles { get { foreach (var tile in Tiles) for (byte i = 1; i < 6; ++i) yield return tile.GetTriangle(i); } }

        public Tile GetTile(byte Index) { fixed (byte* x = Data) return new Tile(Index, x[Index + 1]); }
        public byte GetTriangle(Coordinate Coordinate) { return GetTile(Coordinate.Tile).GetTriangle(Coordinate.Triangle); }

        public Board WithTile(byte Index, Tile Tile)
        {
            var r = new Board(this);
            r.Data[Index + 1] = Tile.Data;
            return r;
        }

        public Board WithHeader(StateHeader Header)
        {
            var r = new Board(this);
            r.Data[0] = Header.Data;
            return r;
        }

        public int CountOfHeldTiles(byte Player)
        {
            return Tiles.Count(t => t.IsHeldBy(Player));
        }

        public int CountOfPieces(byte Player)
        {
            return Tiles.Sum(t => t.CountOfPieces(Player));
        }

        public override int GetHashCode()
        {
            fixed (byte* x = Data)
                return (x[0x07] << 24) + (x[0x08] << 16) + (x[0x0C] << 8) + x[0x0D];
        }

        public override bool Equals(object obj)
        {
            var other = obj as Board?;
            if (other == null || !other.HasValue) return false;

            fixed (byte* x = Data)
                for (byte i = 0; i < 20; ++i)
                    if (x[i] != other.Value._data(i)) return false;
            return true;
        }

        private byte _data(byte i)
        {
            fixed (byte* x = Data) return x[i];
        }

        public bool Equals(Board Other)
        {
            fixed (byte* x = Data)
                for (byte i = 0; i < 20; ++i)
                    if (x[i] != Other._data(i)) return false;
            return true;
        }

        public override string ToString()
        {
            var r = new StringBuilder();
            fixed (byte* x = Data)
                for (byte i = 0; i < 20; ++i)
                    r.AppendFormat("{0:X2}", x[i]);
            return r.ToString();
        }

        public byte[] Bytes
        {
            get
            {
                var r = new byte[20];
                fixed (byte* x = Data)
                    for (var i = 0; i < 20; ++i)
                        r[i] = x[i];
                return r;
            }
        }
    }

}