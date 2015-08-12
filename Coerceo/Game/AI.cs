using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Game
{
    public struct StateTransition
    {
        public Board Board;
        public int Score;
        public Move Move;
        public byte ScoreDepth;
        public byte ScoreAlgorithm;

        public StateTransition(byte[] Source)
        {
            unsafe
            {
                Board = new Board(Source, 0);
                Score = BitConverter.ToInt32(Source, sizeof(Board));
                Move = new Move(Source, sizeof(Board) + sizeof(int));
                ScoreDepth = Source[sizeof(Board) + sizeof(int) + sizeof(Move)];
                ScoreAlgorithm = Source[sizeof(Board) + sizeof(int) + sizeof(Move) + 1];
            }
        }

        public StateTransition WithScoring(byte Algorithm, byte Depth, int Score)
        {
            return new StateTransition
            {
                Board = this.Board,
                Score = Score,
                Move = this.Move,
                ScoreDepth = Depth,
                ScoreAlgorithm = Algorithm
            };
        }
    }

    public static class AI
    {
        private static Dictionary<Board, List<StateTransition>> GameStates = new Dictionary<Board, List<StateTransition>>();
        private static List<Board> OpenConfigurations = new List<Board>();
        private static System.Threading.Mutex StateLock = new Mutex();

        private static List<Tuple<Board, List<StateTransition>>> PendingWrites = new List<Tuple<Board, List<StateTransition>>>();
        private static System.Threading.Mutex SaveLock = new Mutex();
        private static System.IO.FileStream File;
        private static int SaveRate = 100;
        private static int SaveSleepTime = 200;
        private static String Filename;

        private static int[] LastExponentials = new int[100];
        private static int NextLastExponential = 0;

        private static bool Stop = false;

        public static int Stage { get; private set; }

        public static int Exponential
        {
            get
            {
                return LastExponentials.Sum() / LastExponentials.Length;
            }
        }

        public static int MaxExponential
        {
            get
            {
                return LastExponentials.Max();
            }
        }

        public static int CountOfConfigurationsExplored
        {
            get
            {
                return GameStates.Count;
            }
        }

        public static int CountOfOpenConfigurations
        {
            get
            {
                return OpenConfigurations.Count;
            }
        }

        public static int CountOfPendingWrites
        {
            get
            {
                return PendingWrites.Count;
            }
        }

        public static void StartAI(String Filename)
        {
            Stage = 0;

            AI.Filename = Filename;

            for (int i = 0; i < LastExponentials.Length; ++i)
                LastExponentials[i] = 0;

            (new System.Threading.Thread(StartThread)).Start();
        }

        private static void StartThread()
        {
            var StatesCopy = new Dictionary<Board, List<StateTransition>>();

            /*if (System.IO.File.Exists(Filename))
            {
                unsafe
                {
                    var file = System.IO.File.OpenRead(Filename);
                    var buffer = new byte[sizeof(StateTransition)];

                    while (true)
                    {
                        var read = file.Read(buffer, 0, sizeof(Board));
                        if (read != sizeof(Board)) break;

                        var board = new Board(buffer, 0);
                        var transitions = new List<StateTransition>();

                        read = file.Read(buffer, 0, 4);

                        var transitionCount = BitConverter.ToInt32(buffer, 0);

                        for (var i = 0; i < transitionCount; ++i)
                        {
                            read = file.Read(buffer, 0, sizeof(StateTransition));
                            transitions.Add(new StateTransition(buffer));
                        }

                        GameStates.Add(board, transitions);
                        StatesCopy.Add(board, transitions);
                    }

                    file.Close();
                }
            }*/

            (new System.Threading.Thread(WorkerThread)).Start();
            (new System.Threading.Thread(WorkerThread)).Start();
            (new System.Threading.Thread(WorkerThread)).Start();
            (new System.Threading.Thread(WorkerThread)).Start();

            File = System.IO.File.Open(Filename, System.IO.FileMode.Append);

            //(new System.Threading.Thread(SaveThread)).Start();
            
            Stage = 1;

            var discoveredQueue = new List<Board>();
            foreach (var configuration in StatesCopy)
                foreach (var transition in configuration.Value)
                    if (!StatesCopy.ContainsKey(transition.Board))
                    {
                        discoveredQueue.Add(transition.Board);

                        if (discoveredQueue.Count > 1000)
                        {
                            StateLock.WaitOne();
                            OpenConfigurations.AddRange(discoveredQueue);
                            StateLock.ReleaseMutex();
                            discoveredQueue.Clear();
                        }
                    }

            Stage = 2;
        }

        public static void StopAI()
        {
            Stop = true;
        }

        private static void WorkerThread()
        {
            while (!Stop)
            {
                StateLock.WaitOne();
                Board? toConsider = null;
                if (OpenConfigurations.Count > 0)
                {
                    toConsider = OpenConfigurations[OpenConfigurations.Count - 1];
                    OpenConfigurations.RemoveAt(OpenConfigurations.Count - 1);
                }
                StateLock.ReleaseMutex();

                if (toConsider != null && toConsider.HasValue) QueryForMoves(toConsider.Value, 2);
            }
        }

        private static void SaveThread()
        {
            while (!Stop)
            {
                SaveLock.WaitOne();
                if (PendingWrites.Count > SaveRate)
                {
                    foreach (var pendingWrite in PendingWrites)
                    {
                        File.Write(pendingWrite.Item1.Bytes, 0, 20);
                        File.Write(BitConverter.GetBytes(pendingWrite.Item2.Count), 0, 4);
                        foreach (var transition in pendingWrite.Item2)
                        {
                            File.Write(transition.Move.Bytes, 0, 2);
                            File.Write(transition.Board.Bytes, 0, 20);
                            File.Write(BitConverter.GetBytes(transition.Score), 0, 4);
                        }
                    }
                    PendingWrites.Clear();
                    File.Flush();
                }
                SaveLock.ReleaseMutex();

                System.Threading.Thread.Sleep(SaveSleepTime);
            }
        }

        public static void FocusOn(Board Board)
        {
            StateLock.WaitOne();
            OpenConfigurations.Insert(0, Board);
            StateLock.ReleaseMutex();
        }

        public static List<StateTransition> QueryForMoves(Board Board, byte ScoringDepth)
        {
            List<StateTransition> result;

            StateLock.WaitOne();

            if (!GameStates.TryGetValue(Board, out result))
            {
                StateLock.ReleaseMutex();

                result = new List<StateTransition>(Coerceo.EnumerateLegalMoves(Board).Select(m => new StateTransition
                {
                    Move = m,
                    Board = Coerceo.ApplyMove(Board, m),
                    Score = 0
                }));

                StateLock.WaitOne();

                for (var i = 0; i < result.Count; ++i)
                    if (!GameStates.ContainsKey(result[i].Board))
                        OpenConfigurations.Add(result[i].Board);

                GameStates.Upsert(Board, result);

                StateLock.ReleaseMutex();

                //SaveLock.WaitOne();
                //PendingWrites.Add(Tuple.Create(Board, result));
                //SaveLock.ReleaseMutex();
            }
            else
                StateLock.ReleaseMutex();

            for (var i = 0; i < result.Count; ++i)
            {
                if (result[i].ScoreDepth < ScoringDepth)
                    result[i] = result[i].WithScoring(1, ScoringDepth, ScoreBoard(result[i].Board, ScoringDepth));
            }

            return result;
        }

        public static int ScoreBoard(Board Board, byte ScoringDepth)
        {
            var previousPlayer = (byte)~Board.Header.WhoseTurnNext;

            var r = 0;

            r -= 20 * Board.CountOfHeldTiles(Board.Header.WhoseTurnNext);
            r += 10 * Board.CountOfHeldTiles(previousPlayer);

            var subMoves = QueryForMoves(Board, (byte)(ScoringDepth - 1));
            var total = subMoves.Sum(m => m.Score);

            r = (r + r + (-total / subMoves.Count)) / 3;

            return r;
        }
    }
}
