using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
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
            var sniping = false;

            switch ((MoveType)Move.Type)
            {
                case MoveType.Trade:
                    {
                        // Verify move is legal.

                        // Reject move if attempting to snipe own piece.
                        if (Move.Triangle % 2 != Board.Header.WhoseTurnNext) throw new InvalidOperationException();

                        // Reject move if player does not hold at least two tiles.
                        var heldTileCount = Board.Tiles.Count(t => t.IsHeldBy(Board.Header.WhoseTurnNext));
                        if (heldTileCount < 2) throw new InvalidOperationException();

                        // Reject move if there is no enemy piece at the location.
                        if (Board.GetTriangle(Move.Coordinate) == 0) throw new InvalidOperationException();

                        // Turn is legal, mutate board.
                        foreach (var tile in Board.Tiles.Where(t => t.IsHeldBy(Board.Header.WhoseTurnNext)).Take(2))
                            Board = Board.WithTile(tile.ID, tile.WithStatus(0x03));
                        Board = Board.WithTile(Move.Tile, Board.GetTile(Move.Tile).WithTriangle(Move.Triangle, 0));

                        checkForEmpty.Add(Move.Tile);
                        sniping = true;
                    }
                    break;
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
                        if (neighbor != 0xFF && !Board.GetTile(neighbor).IsOutOfPlay())
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
                        runs[0] = new AdjacentRun(runs[runsCount - 1].Start, runs[0].End, runs[0].Count + runs[runsCount - 1].Count);
                        runsCount -= 1;
                    }

                    if (runsCount == 1 && runs[0].Count <= 3)
                    {
                        if (sniping)
                            Board = Board.WithTile(tileID, Board.GetTile(tileID).WithStatus(0x03));
                        else
                            Board = Board.WithTile(tileID, Board.GetTile(tileID).WithStatus((byte)(Board.Header.WhoseTurnNext + 1)));


                        // Mark adjacent triangles for surround consideration.
                        for (var x = 0; x < 6; ++x)
                        {
                            // Don't check own pieces for surround
                            if (Tables.ExposedAdjacency[x] % 2 != Board.Header.WhoseTurnNext) continue;

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
            }

            if (Board.Header.WhoseTurnNext == 0)
                Board = Board.WithHeader(new StateHeader(0x80));
            else
                Board = Board.WithHeader(new StateHeader(0x00));

            return Board;
        }

        public static IEnumerable<Move> EnumerateLegalMoves(Board Board)
        {
            for (byte t = 0; t < 19; ++t)
            {
                var tile = Board.GetTile(t);
                if (tile.IsOutOfPlay()) continue;
                for (byte x = 0; x < 6; ++x)
                    foreach (var move in EnumerateLegalPieceMoves(new Coordinate(t, x), Board))
                        yield return move;
            }
        }

        public static IEnumerable<Move> EnumerateLegalPieceMoves(Coordinate Piece, Board Board)
        {
            if (Piece.Triangle % 2 != Board.Header.WhoseTurnNext)
            {
                if (Board.GetTriangle(Piece) != 0)
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
            else
            {
                if (Board.GetTriangle(Piece) != 0)
                {
                    var canSnipe = Board.Tiles.Count(t => t.IsHeldBy(Board.Header.WhoseTurnNext)) >= 2;
                    if (canSnipe)
                        yield return new Move(Piece, 0, MoveType.Trade);
                }
            }
        }
    }
}
