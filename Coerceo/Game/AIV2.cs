using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Game
{
    public static class AIV2
    {
        public static int CountOfConfigurationsScored = 0;
        public static int DepthReached = 0;
        private static Random Random = new Random();
        private static bool WinConditionFound = false;

        public static void ExpandMax(DataEntry Entry, int Depth, int DepthLimit)
        {
            CountOfConfigurationsScored += 1;
            var recurseLimit = DepthLimit;
            if (Depth >= DepthReached) DepthReached = Depth;

            if (Entry.Moves == null)
            {
                AI.Expand(Entry);
                recurseLimit = Depth + 1;
            }

            if (Entry.Moves.Count == 0)
            {
                Entry.ScoreAdjustment = 100000;
                WinConditionFound = true;
            }
            else
            {
                var baseMax = Entry.Moves.Max(m => m.Score);
                var index = Random.Next(0, Entry.Moves.Where(m => m.Score == baseMax).Count());
                foreach (var move in Entry.Moves)
                    move.Ignored = true;
                var bestMove = Entry.Moves.Where(m => m.Score == baseMax).ElementAt(index);
                if (Depth < DepthLimit) ExpandMax(bestMove.NextBoard, Depth + 1, recurseLimit);
                bestMove.Score = bestMove.NextBoard.CombinedScore;
                bestMove.Ignored = false;

                Entry.ScoreAdjustment = -(Entry.Moves.Max(m => m.Score) * 0.8f);
            }
        }

        public static async Task<Move> PickBestMove(Board Board, int Depth)
        {
            CountOfConfigurationsScored = 0;
            DepthReached = 0;
            WinConditionFound = false;
            var entry = AI.CreateEntry(Board);
            await Task.Run(() =>
                {
                    while (DepthReached < Depth && !WinConditionFound)
                        ExpandMax(entry, 0, Depth);
                });
            if (entry.Moves.Count == 0) throw new InvalidOperationException();
            var max = entry.Moves.Where(m => !m.Ignored).Max(m => m.Score);
            var index = Random.Next(0, entry.Moves.Where(m => !m.Ignored && m.Score == max).Count());
            return entry.Moves.Where(m => !m.Ignored && m.Score == max).ElementAt(index).Move;
        }
    }
}
