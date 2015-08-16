using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    /* Encoding a tile in 1 byte: [00] [000000]
        Status code (A) ------------+     |
        Triangle bit mask (B) -----------+
     */

    public enum TileStatusCode
    {
        InPlay = 0,
        HeldByWhite = 1,
        HeldByBlack = 2,
        OutOfPlay = 3
    }

    public struct Tile
    {
        internal byte ID;
        internal byte Data;

        public Tile(byte ID, byte Data)
        {
            this.ID = ID;
            this.Data = Data;
        }

        public byte GetTriangle(byte Index) { return (byte)((Data & (1 << Index)) >> Index); }
        public Tile WithStatus(byte NewStatus) { return new Tile(ID, (byte)((Data & 0x3F) | (byte)(NewStatus << 6))); }
        public Tile WithTriangle(byte Index, byte NewValue) { return new Tile(ID, (byte)((Data & ~(1 << Index)) | (NewValue << Index))); }
        public bool IsHeldBy(byte Player) { return ((Data & 0xC0) >> 6) == (Player + 1); }
        public bool IsEmpty() { return (Data & 0x3F) == 0; }
        public bool IsOutOfPlay() { return (Data & 0xC0) != 0; }
        public int CountOfPieces(byte Player)
        {
            var r = 0;
            for (byte i = 0; i < 6; ++i)
                if (i % 2 != Player && GetTriangle(i) != 0) r += 1;
            return r;
        }
    }
}