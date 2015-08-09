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

    public static class Tables
    {
        public static Vector2[] HexWorldPositions;

        public static byte[][] TileAdjacency = 
        {
            /*00*/ new byte[] { 0xFF, 0xFF, 0x01, 0x04, 0x02, 0xFF },
            /*01*/ new byte[] { 0xFF, 0xFF, 0x03, 0x06, 0x04, 0x00 },
            /*02*/ new byte[] { 0xFF, 0x00, 0x04, 0x07, 0x05, 0xFF },
            /*03*/ new byte[] { 0xFF, 0xFF, 0xFF, 0x08, 0x06, 0x01 },
            /*04*/ new byte[] { 0x00, 0x01, 0x06, 0x09, 0x07, 0x02 },
            /*05*/ new byte[] { 0xFF, 0x02, 0x07, 0x0A, 0xFF, 0xFF },
            /*06*/ new byte[] { 0x01, 0x03, 0x08, 0x0B, 0x09, 0x04 },
            /*07*/ new byte[] { 0x02, 0x04, 0x09, 0x0C, 0x0A, 0x05 }, 
            /*08*/ new byte[] { 0x03, 0xFF, 0xFF, 0x0D, 0x0B, 0x06 },
            /*09*/ new byte[] { 0x04, 0x06, 0x0B, 0x0E, 0x0C, 0x07 },
            /*0A*/ new byte[] { 0x05, 0x07, 0x0C, 0x0F, 0xFF, 0xFF },
            /*0B*/ new byte[] { 0x06, 0x08, 0x0D, 0x10, 0x0E, 0x09 },
            /*0C*/ new byte[] { 0x07, 0x09, 0x0E, 0x11, 0x0F, 0x0A },
            /*0D*/ new byte[] { 0x08, 0xFF, 0xFF, 0xFF, 0x10, 0x0B },
            /*0E*/ new byte[] { 0x09, 0x0B, 0x10, 0x12, 0x11, 0x0C },
            /*0F*/ new byte[] { 0x0A, 0x0C, 0x11, 0xFF, 0xFF, 0xFF },
            /*10*/ new byte[] { 0x0B, 0x0D, 0xFF, 0xFF, 0x12, 0x0E },
            /*11*/ new byte[] { 0x0C, 0x0E, 0x12, 0xFF, 0xFF, 0x0F },
            /*12*/ new byte[] { 0x0E, 0x10, 0xFF, 0xFF, 0xFF, 0x11 }
        };

        public static Rel[][] MoveAdjacency = {
            new Rel[] { new Rel(0,2), new Rel(1,4), new Rel(255,2), new Rel(255,4), new Rel(5,2), new Rel(0,4) },
            new Rel[] { new Rel(1,5), new Rel(1,3), new Rel(2,5), new Rel(255,3), new Rel(255,5), new Rel(0,3) },
            new Rel[] { new Rel(1,4), new Rel(2,0), new Rel(2,4), new Rel(3,0), new Rel(255,4), new Rel(255,0) },
            new Rel[] { new Rel(255,1), new Rel(2,5), new Rel(3,1), new Rel(3,5), new Rel(4,1), new Rel(255,5) },
            new Rel[] { new Rel(255,0), new Rel(255,2), new Rel(3,0), new Rel(4,2), new Rel(4,0), new Rel(5,2) },
            new Rel[] { new Rel(0,3), new Rel(255,1), new Rel(255,3), new Rel(4,1), new Rel(5,3), new Rel(5,1) }
        };

        public static Rel[][] SurroundAdjacency = {
            new Rel[] { new Rel(0,3), new Rel(255,1), new Rel(255,5) },
            new Rel[] { new Rel(1,4), new Rel(255,2), new Rel(255,0) },
            new Rel[] { new Rel(2,5), new Rel(255,3), new Rel(255,1) },
            new Rel[] { new Rel(3,0), new Rel(255,4), new Rel(255,2) },
            new Rel[] { new Rel(4,1), new Rel(255,5), new Rel(255,3) },
            new Rel[] { new Rel(5,2), new Rel(255,0), new Rel(255,4) }
         };

        public static byte[] ExposedAdjacency = { 3, 4, 5, 0, 1, 2};

        public static byte[][] InitialPiecePlacements = {
            /*00*/ new byte[] {1,5},
            /*01*/ new byte[] {2,5},
            /*02*/ new byte[] {1,4},
            /*03*/ new byte[] {0,2},
            /*04*/ new byte[] {1,5},
            /*05*/ new byte[] {0,4},
            /*06*/ new byte[] {0,2},
            /*07*/ new byte[] {0,4},
            /*08*/ new byte[] {0,3},
            /*09*/ new byte[] {},
            /*0A*/ new byte[] {0,3},
            /*0B*/ new byte[] {1,3},
            /*0C*/ new byte[] {3,5},
            /*0D*/ new byte[] {1,3},
            /*0E*/ new byte[] {2,4},
            /*0F*/ new byte[] {3,5},
            /*10*/ new byte[] {1,4},
            /*11*/ new byte[] {2,5},
            /*12*/ new byte[] {2,4}
                                                        };

        public static Board InitialBoard;

        public static void Initialize()
        {
            var hexSystem = new HexDemo.Hex(1.0f);

            //           The starting board - 19 tiles
            //       __               __
            //    __/00\__         __/00\__    Neighbor 
            // __/01\__/02\__     /01\__/05\    Directions   
            ///03\__/04\__/05\    \__/  \__/       
            //\__/06\__/07\__/    /02\__/04\       
            ///08\__/09\__/0A\    \__/03\__/      
            //\__/0B\__/0C\__/       \__/  
            ///0D\__/0E\__/0F\          
            //\__/10\__/11\__/          
            //   \__/12\__/            
            //      \__/


            HexWorldPositions = new Vector2[] 
            {
              hexSystem.TileOrigin(0, 2),  
              hexSystem.TileOrigin(-1, 1),  
              hexSystem.TileOrigin(1, 1),  
              hexSystem.TileOrigin(-2, 1),  
              hexSystem.TileOrigin(0, 1),  
              hexSystem.TileOrigin(2, 1),  
              hexSystem.TileOrigin(-1, 0),  
              hexSystem.TileOrigin(1, 0),  
              hexSystem.TileOrigin(-2, 0),
              hexSystem.TileOrigin(0, 0),  
              hexSystem.TileOrigin(2, 0),  
              hexSystem.TileOrigin(-1, -1),  
              hexSystem.TileOrigin(1, -1),  
              hexSystem.TileOrigin(-2, -1),  
              hexSystem.TileOrigin(0, -1),  
              hexSystem.TileOrigin(2, -1),  
              hexSystem.TileOrigin(-1, -2),  
              hexSystem.TileOrigin(1, -2),  
              hexSystem.TileOrigin(0, -2)  
            };

            InitialBoard = Board.Empty().WithHeader(new StateHeader(0));
            for (var x = 0; x < 19; ++x)
            {
                var tile = new Tile((byte)x, 0x00);
                foreach (var item in InitialPiecePlacements[x])
                    tile = tile.WithTriangle(item, 0x01);
                InitialBoard = InitialBoard.WithTile((byte)x, tile);
            }
        }
    }
}
