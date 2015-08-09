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
    }

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
    }

    public struct Board
    {
        byte[] Data;

        public static Board Empty() { return new Board(new byte[20]); }

        public Board(byte[] Source)
        {
            Data = new byte[20];
            Source.CopyTo(Data, 0);
        }

        public StateHeader Header { get { return new StateHeader(Data[0]); } }

        public IEnumerable<Tile> Tiles { get { for (var i = 1; i < 20; ++i) yield return new Tile((byte)(i - 1), Data[i]); } }
        public IEnumerable<byte> Triangles { get { foreach (var tile in Tiles) for (var i = 1; i < 6; ++i) yield return tile.GetTriangle((byte)i); } }

        public Tile GetTile(byte Index) { return new Tile(Index, Data[Index + 1]); }
        public byte GetTriangle(Coordinate Coordinate) { return GetTile(Coordinate.Tile).GetTriangle(Coordinate.Triangle); }

        public Board WithTile(byte Index, Tile Tile)
        {
                var r = new Board(Data);
                r.Data[Index + 1] = Tile.Data;
                return r;
        }

        public Board WithHeader(StateHeader Header)
        {
            var r = new Board(Data);
            r.Data[0] = Header.Data;
            return r;
        }
    }

    /*
    Encoding a move: [00000][000] [000][00][000] [00000000] [00000000]
     Tile -------------+      |     |   |    |     |          |
      Triangle ---------------+     |   |    |     +----------+
        Direction-------------------+   |    |     |
         Movement type -----------------+    |     |
          Reserved --------------------------+     |
           Move Score -----------------------------+
    */

    public enum MoveType
    {
        MovePiece = 0,
        Trade = 1,
        Resign = 2
    }

    public struct Move
    {
        int Data;

        public Move(Coordinate Coordinate, byte Direction, MoveType Type)
        {
            Data = (Coordinate.Data << 24) + (Direction << 21) + ((byte)Type << 19);
        }

        public Coordinate Coordinate { get { return new Coordinate((byte)(Data >> 24)); } }
        public byte Tile { get { return Coordinate.Tile; } }
        public byte Triangle { get { return Coordinate.Triangle; } }

        public byte Direction { get { return (byte)((Data >> 21) & 0x07); } }
        public byte Type { get { return (byte)((Data >> 19) & 0x03); } }
    }

    public static class Coerceo
    {
        public static Coordinate FindMoveNeighbor(Coordinate Of, byte Direction)
        {
            var relativeMove = Tables.MoveAdjacency[Of.Triangle][Direction];
            if (relativeMove.Direction == 0xFF)
                return new Coordinate(Of.Tile, relativeMove.Triangle);
            else
                return new Coordinate(Tables.TileAdjacency[Of.Tile][relativeMove.Direction], relativeMove.Triangle);
        }

        public static Coordinate FindSurroundNeighbor(Coordinate Of, byte Direction)
        {
            var relativeMove = Tables.SurroundAdjacency[Of.Triangle][Direction];
            if (relativeMove.Direction == 0xFF)
                return new Coordinate(Of.Tile, relativeMove.Triangle);
            else
                return new Coordinate(Tables.TileAdjacency[Of.Tile][relativeMove.Direction], relativeMove.Triangle);
        }
 
        struct AdjacentRun
			{
				public int Start;
				public int End;
				public int Count;

            public AdjacentRun(int S, int E, int C)
            {
                Start = S;
                End = E;
                Count = C;
            }
			}


        public static Board ApplyMove(Board Board, Move Move)
        {

            var checkForSurrounded = new List<Coordinate>();
            var checkForEmpty = new List<byte>();

            switch ((MoveType)Move.Type)
            {
                /*
                case Resign: 
                    End game in loss for Board.WhoseTurn

                case Trade in tiles to snipe piece:
                    {
                        // Verify move is legal.

                        // Reject move if attempting to snipe own piece.
                        if (Move.Triangle % 2 == Board.WhoseTurn) return Rejected;

                        // Reject move if player does not hold at least two tiles.
                        var heldTileCount = Board.Tiles.Count(t => t.IsHeldBy(Board.WhoseTurnNext));
                        if (heldTileCount < 2) return Rejected;

                        // Reject move if there is no enemy piece at the location.
                        if (Board.GetTriangle(Move.Coordinate) == 0) return Rejected;

                        // Turn is legal, mutate board.
                        foreach (var tile in Board.Tiles.Where(t => t.IsHeldBy(Board.WhoseTurnNext).Take(2)))
                            Board = Board.WithTile(tile.ID, tile.WithStatus(OutOfPlay));
                        Board = Board.WithTile(Move.Tile, Board.GetTile(Move.Tile).WithTriangle(Move.Triangle, 0));

                        checkForEmpty.Add(Move.Tile);
                    }
                */
                case MoveType.MovePiece:
                    {
                        // Verify move is legal.

                        // Reject move if attempting to move opponent piece.
                        if (Move.Triangle % 2 == Board.Header.WhoseTurnNext) throw new InvalidOperationException();

                        // Reject move if there is no piece at the location.
                        if (Board.GetTriangle(Move.Coordinate) == 0) throw new InvalidOperationException();

                        // Find destination triangle coordinate.
                        var dest = FindMoveNeighbor(Move.Coordinate, Move.Direction);

                        // Reject move if player attempted to move off board.
                        if (dest.Invalid) throw new InvalidOperationException();
                        if (Board.GetTile(dest.Tile).IsOutOfPlay()) throw new InvalidOperationException();

                        // Reject move if destination triangle is occupied.
                        if (Board.GetTriangle(dest) != 0) throw new InvalidOperationException();

                        // Move is legal, mutate board.

                        Board = Board.WithTile(Move.Tile, Board.GetTile(Move.Tile).WithTriangle(Move.Triangle, 0));
                        Board = Board.WithTile(dest.Tile, Board.GetTile(dest.Tile).WithTriangle(dest.Triangle, 1));

                        checkForEmpty.Add(Move.Tile);

                        for (byte x = 0; x < 3; ++x)
                        {
                            var neighbor = FindSurroundNeighbor(dest, x);
                            if (!neighbor.Invalid && !Board.GetTile(neighbor.Tile).IsOutOfPlay())
                                checkForSurrounded.Add(neighbor);
                        }
                    }
                    break;
            }

            if (Board.Header.WhoseTurnNext == 0)
                Board = Board.WithHeader(new StateHeader(0x80));
            else
                Board = Board.WithHeader(new StateHeader(0x00));

            while (checkForSurrounded.Count > 0 || checkForEmpty.Count > 0)
            {
                while (checkForSurrounded.Count > 0)
                {
                    var piece = checkForSurrounded[0];
                    checkForSurrounded.RemoveAt(0);

                    var sum = 0;
                    for (byte x = 0; x < 3; ++x)
                    {
                        var neighbor = FindSurroundNeighbor(piece, x);
                        if (neighbor.Invalid || Board.GetTile(neighbor.Tile).IsOutOfPlay())
                            sum += 1;
                        else
                            sum += Board.GetTriangle(neighbor);
                    }

                    if (sum == 3)
                    {
                        Board = Board.WithTile(piece.Tile, Board.GetTile(piece.Tile).WithTriangle(piece.Triangle, 0));
                        checkForEmpty.Add(piece.Tile);
                    }
                }


                while (checkForEmpty.Count > 0)
                {
                    var tileID = checkForEmpty[0];
                    checkForEmpty.RemoveAt(0);

                    if (!Board.GetTile(tileID).IsEmpty())
                        continue;

                    // Tile is empty - if it has 3 or less adjacent tiles in a row, it can be removed.

                    var runs = new AdjacentRun[3];
                    var runsCount = 0;

                    for (var x = 0; x < 6; ++x)
                    {
                        var neighbor = Tables.TileAdjacency[tileID][x];
                        if (neighbor != 0xFF && !Board.GetTile(neighbor).IsEmpty())
                        {
                            if (runsCount == 0)
                            {
                                runs[0] = new AdjacentRun(x, x, 1);
                                runsCount += 1;
                            }
                            else if (runs[runsCount - 1].End == x - 1)
                                runs[runsCount - 1] = new AdjacentRun(runs[runsCount - 1].Start, x, runs[runsCount - 1].Count + 1);
                            else
                            {
                                runs[runsCount] = new AdjacentRun(x, x, 1);
                                runsCount += 1;
                            }
                        }
                    }

                    if (runsCount > 1 && runs[0].Start == 0 && runs[runsCount - 1].End == 5)
                    {
                        runs[0] = new AdjacentRun(runs[runsCount - 1].Start, runs[0].Start, runs[0].Count + runs[runsCount - 1].Count);
                        runsCount -= 1;
                    }

                    if (runsCount == 1 && runs[0].Count <= 3)
                        Board = Board.WithTile(tileID, Board.GetTile(tileID).WithStatus((byte)(Board.Header.WhoseTurnNext + 1)));

                    // Mark adjacent triangles for surround consideration.
                    for (var x = 0; x < 6; ++x)
                    {
                        // Don't check own pieces for surround
                        if (x % 2 != Board.Header.WhoseTurnNext) continue;

                        // Don't check tiles that don't exist.
                        var neighbor = Tables.TileAdjacency[tileID][x];
                        if (neighbor == 0xFF || Board.GetTile(neighbor).IsOutOfPlay()) continue;

                        checkForSurrounded.Add(new Coordinate(neighbor, Tables.ExposedAdjacency[x]));
                    }

                    // Check neighboring tiles to see if they can be removed.
                    for (var x = 0; x < 6; ++x)
                    {
                        var neighbor = Tables.TileAdjacency[tileID][x];
                        if (neighbor == 0xFF || Board.GetTile(neighbor).IsOutOfPlay()) continue;
                        checkForEmpty.Add(neighbor);
                    }
                }
            }

            return Board;
        }

    /*


AI Game State Graph

Cycles are very possible.

class StateTransition
{
	public Move Move;
	public Board Board;
}

var GameStateTable = new Dictionary<Board, List<StateTransition>>();

List<StateTransition> QueryForMoves(Board Board)
{
	List<StateTransition> result;
	if (GameStateTable.TryGetValue(Board, out result))
		return result;
	else
	{
		result = new List<StateTransition>(EnumerateLegalMoves(Board).Select(m => new StateTransition(m, ApplyMove(Board, m))));
		GameStateTable.Add(Board, result);
		return result;
	}
}


IEnumerable<Move> EnumerateLegalMoves(Board)
{
	var canSnipe = Board.Tiles.Count(t => t.IsHeldBy(Board.WhoseTurnNext)) >= 2;

	for (var t = 0; t < 19; ++t)
	{
		var tile = Board.GetTile(t);
		if (tile.IsOutOfPlay())
			continue;
		for (var x = 0; x < 6; ++x)
		{
			if (x % 2 == Board.WhoseTurnNext)
			{
				if (tile.GetTriangle(x) == 1)
					foreach (var move in EnumerateLegalPieceMoves(new TriangleCoordinate(t, x), Board))
						yield return move;
			}
			else
			{
				if (tile.GetTriangle(x) == 1 && canSnipe)
					yield return new Move(t, x, 0, MoveType.Snipe);
			}
		}
	}
}
         * 
         */

        public static IEnumerable<Move> EnumerateLegalPieceMoves(Coordinate Piece, Board Board)
        {
            if (Piece.Triangle % 2 != Board.Header.WhoseTurnNext)
            {
                for (byte x = 0; x < 6; ++x)
                {
                    var neighbor = FindMoveNeighbor(Piece, x);
                    if (neighbor.Invalid || Board.GetTile(neighbor.Tile).IsOutOfPlay())
                        continue;
                    if (Board.GetTriangle(neighbor) == 0)
                        yield return new Move(Piece, x, MoveType.MovePiece);
                }
            }
        }
    }
}
