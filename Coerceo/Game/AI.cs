using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Game
{
    public class DataEntry
    {
        public Board Board;
        public int BaseScore;
        public int ScoreAdjustment;
        
        public int CombinedScore { get { return BaseScore + ScoreAdjustment; } }

        public List<MoveTransition> Moves;
    }
    
    public class MoveTransition
    {
        public DataEntry NextBoard;
        public Move Move;
        public int Score;
        public bool Ignored = false;
    }

    public static class AI
    {
        public static int CountOfConfigurationsScored = 0;
        private static Random Random = new Random();

        public static void Expand(DataEntry Entry)
        {

            if (Entry.Moves == null)
            {
                Entry.Moves = new List<MoveTransition>(Coerceo.EnumerateLegalMoves(Entry.Board).Select(move =>
                    {
                        var nextBoard = new DataEntry
                        {
                            ScoreAdjustment = 0,
                            Moves = null,
                            Board = Coerceo.ApplyMove(Entry.Board, move)
                        };

                        nextBoard.BaseScore = ScoreBoard(Entry.Board, nextBoard.Board);

                        return new MoveTransition
                            {
                                NextBoard = nextBoard,
                                Move = move,
                                Ignored = false,
                                Score = nextBoard.BaseScore
                            };
                                
                    }));
            }
        }

        public static void CalculateAdjustedScore(DataEntry Entry, int Depth)
        {
            CountOfConfigurationsScored += 1;
            if (CountOfConfigurationsScored > 10000)
                return;

            if (Entry.Moves == null)
                Expand(Entry);

            if (Entry.Moves.Count == 0)
            {
                // This board is a dead end - the player lost. It gets a strong negative penalty, but not too strong
                // or it will wrap and the other player will think this move is really fantastic.
                Entry.ScoreAdjustment = -100000;
            }
            else
            {
                var max = int.MinValue;
                var baseAverage = Entry.Moves.Sum(m => m.Score) / Entry.Moves.Count;
                var baseMax = Entry.Moves.Max(m => m.Score);

                foreach (var move in Entry.Moves)
                {
                    if (move.Score >= baseMax)
                    {
                        if (Depth > 0) CalculateAdjustedScore(move.NextBoard, Depth - 1);
                        move.Score = move.NextBoard.CombinedScore;
                        if (move.Score > max) max = move.Score;
                        move.Ignored = false;
                    }
                    else
                        move.Ignored = true;
                }

                if (max < 0) // Oh shit?
                {
                    foreach (var move in Entry.Moves.Where(m => m.Ignored))
                    {
                        if (move.Score >= baseAverage)
                        {
                            if (Depth > 0) CalculateAdjustedScore(move.NextBoard, Depth - 2); // We can't afford to look very deeply, though.
                            move.Score = move.NextBoard.CombinedScore;
                            if (move.Score > max) max = move.Score;
                            move.Ignored = false;
                        }
                        else
                            move.Ignored = true;
                    }
                }

                if (max < 0) // Dieing here
                {
                    foreach (var move in Entry.Moves.Where(m => m.Ignored))
                    {
                        if (Depth > 0) CalculateAdjustedScore(move.NextBoard, 0); // We can't afford to look very deeply, though.
                        move.Score = move.NextBoard.CombinedScore;
                        if (move.Score > max) max = move.Score;
                        move.Ignored = false;
                    }
                }

                Entry.ScoreAdjustment = -(int)(max * 0.8f);
            }
        }

        public static async Task<Move> PickBestMove(Board Board, int Depth)
        {
            CountOfConfigurationsScored = 0;
            var entry = CreateEntry(Board);
            await Task.Run(() => CalculateAdjustedScore(entry, Depth));
            if (entry.Moves.Count == 0) throw new InvalidOperationException();
            var max = entry.Moves.Where(m => !m.Ignored).Max(m => m.Score);
            var index = Random.Next(0, entry.Moves.Where(m => !m.Ignored && m.Score == max).Count());
            return entry.Moves.Where(m => !m.Ignored && m.Score == max).ElementAt(index).Move;
        }

        public static DataEntry CreateEntry(Board Board)
        {
            return new DataEntry
            {
                BaseScore = 0,
                Board = Board,
                Moves = null
            };
        }

        private static Coordinate[][] ControlTiers = new Coordinate[][]
        {
            new Coordinate[] { new Coordinate(0x09, 0), new Coordinate(0x09, 1), new Coordinate(0x09, 2), new Coordinate(0x09, 3), new Coordinate(0x09, 4), new Coordinate(0x09, 5) },
            new Coordinate[] { new Coordinate(0x04, 3), new Coordinate(0x04, 2), new Coordinate(0x06, 05), new Coordinate(0x06, 04), new Coordinate(0x06, 03), new Coordinate(0x0B, 00), new Coordinate(0x0B, 05), new Coordinate(0x0B, 04), new Coordinate(0x0E, 01), new Coordinate(0x0E, 00), new Coordinate(0x0E, 05), new Coordinate(0x0C, 02), new Coordinate(0x0C, 01), new Coordinate(0x0C, 00), new Coordinate(0x07, 03), new Coordinate(0x07, 02), new Coordinate(0x07, 01), new Coordinate(0x04, 04) },
            new Coordinate[] { new Coordinate(0x04, 00), new Coordinate(0x04, 01), new Coordinate(0x01, 04), new Coordinate(0x01, 03), new Coordinate(0x06, 00), new Coordinate(0x06, 01), new Coordinate(0x06, 02), new Coordinate(0x08, 05), new Coordinate(0x08, 04), new Coordinate(0x0B, 01), new Coordinate(0x0B, 02), new Coordinate(0x0B, 03), new Coordinate(0x10, 00), new Coordinate(0x10, 05), new Coordinate(0x0E, 02), new Coordinate(0x0E, 03), new Coordinate(0x0E, 04), new Coordinate(0x11, 01), new Coordinate(0x11, 00), new Coordinate(0x0C, 03), new Coordinate(0x0C, 04), new Coordinate(0x0C, 05), new Coordinate(0x0A, 02), new Coordinate(0x0A, 01), new Coordinate(0x07, 04), new Coordinate(0x07, 05), new Coordinate(0x07, 00), new Coordinate(0x02, 03), new Coordinate(0x02, 02), new Coordinate(0x04, 05) },
            new Coordinate[] { new Coordinate(0x00, 03), new Coordinate(0x00, 02), new Coordinate(0x01, 05), new Coordinate(0x01, 00), new Coordinate(0x01, 01), new Coordinate(0x01, 02), new Coordinate(0x03, 05), new Coordinate(0x03, 04), new Coordinate(0x03, 03), new Coordinate(0x08, 00), new Coordinate(0x08, 01), new Coordinate(0x08, 02), new Coordinate(0x08, 03), new Coordinate(0x0D, 00), new Coordinate(0x0D, 05), new Coordinate(0x0D, 04), new Coordinate(0x10, 01), new Coordinate(0x10, 02), new Coordinate(0x10, 03), new Coordinate(0x10, 04), new Coordinate(0x12, 01), new Coordinate(0x12, 00), new Coordinate(0x12, 05), new Coordinate(0x11, 02), new Coordinate(0x11, 03), new Coordinate(0x11, 04), new Coordinate(0x11, 05), new Coordinate(0x0F, 02), new Coordinate(0x0F, 01), new Coordinate(0x0F, 00), new Coordinate(0x0A, 03), new Coordinate(0x0A, 04), new Coordinate(0x0A, 05), new Coordinate(0x0A, 00), new Coordinate(0x05, 03), new Coordinate(0x05, 02), new Coordinate(0x05, 01), new Coordinate(0x02, 04), new Coordinate(0x02, 05), new Coordinate(0x02, 00), new Coordinate(0x02, 01), new Coordinate(0x00, 04) },
            new Coordinate[] { new Coordinate(0x00, 00), new Coordinate(0x00, 01), new Coordinate(0x03, 00), new Coordinate(0x03, 01), new Coordinate(0x03, 02), new Coordinate(0x0D, 01), new Coordinate(0x0D, 02), new Coordinate(0x0D, 03), new Coordinate(0x12, 02), new Coordinate(0x12, 03), new Coordinate(0x12, 04), new Coordinate(0x0F, 03), new Coordinate(0x0F, 04), new Coordinate(0x0F, 05), new Coordinate(0x05, 04), new Coordinate(0x05, 05), new Coordinate(0x05, 00), new Coordinate(0x00, 05) }
        };

        private static int[] TierValue = new int[] { 1000, 500, 300, 100, 0 };

        public static int ScoreBoard(Board PreviousBoard, Board Board)
        {
            var previousPlayer = (byte)~Board.Header.WhoseTurnNext;

            var r = 0;

            // Taking tiles is good.
            r += 50000 * (Board.CountOfHeldTiles(previousPlayer) - PreviousBoard.CountOfHeldTiles(previousPlayer));

            // Taking pieces is also good.
            r += 50000 * (PreviousBoard.CountOfPieces(Board.Header.WhoseTurnNext) - Board.CountOfPieces(Board.Header.WhoseTurnNext));

            var tierPieceCount = new int[2, ControlTiers.Length];

            // Encourage board development.
            for (int i = 0; i < ControlTiers.Length; ++i)
            {
                tierPieceCount[0, i] = 0;
                tierPieceCount[1, i] = 0;
                foreach (var triangle in ControlTiers[i])
                {
                    if (triangle.Triangle % 2 != previousPlayer)
                    {
                        tierPieceCount[0, i] += PreviousBoard.GetTriangle(triangle);
                        tierPieceCount[1, i] += Board.GetTriangle(triangle);
                    }
                }
            }

            for (int i = 0; i < ControlTiers.Length - 1; ++i)
            {
                var tierDifference = tierPieceCount[1, i] - tierPieceCount[0, i];
                r += TierValue[i] * tierDifference;
            }

            return r;
        }
    }
}
