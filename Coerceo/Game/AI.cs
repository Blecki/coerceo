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
        public Move Move;
        public Board Board;
        public int Score;

        public override string ToString()
        {
            return Move.ToString() + Board.ToString() + String.Format("{0:X8}", Score);
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

            if (System.IO.File.Exists(Filename))
            {
                var file = System.IO.File.OpenRead(Filename);
                var buffer = new byte[20];

                while (true)
                {
                    var read = file.Read(buffer, 0, 20);
                    if (read != 20) break;

                    var board = new Board(buffer);
                    var transitions = new List<StateTransition>();

                    read = file.Read(buffer, 0, 4);

                    var transitionCount = BitConverter.ToInt32(buffer, 0);

                    for (var i = 0; i < transitionCount; ++i)
                    {
                        var transition = new StateTransition();

                        read = file.Read(buffer, 0, 2);
                        transition.Move = new Move(buffer);

                        read = file.Read(buffer, 0, 20);
                        transition.Board = new Board(buffer);

                        read = file.Read(buffer, 0, 4);
                        transition.Score = BitConverter.ToInt32(buffer, 0);

                        transitions.Add(transition);
                    }

                    GameStates.Add(board, transitions);
                    StatesCopy.Add(board, transitions);
                }

                file.Close();
            }

            (new System.Threading.Thread(WorkerThread)).Start();
            (new System.Threading.Thread(WorkerThread)).Start();
            (new System.Threading.Thread(WorkerThread)).Start();
            (new System.Threading.Thread(WorkerThread)).Start();

            File = System.IO.File.Open(Filename, System.IO.FileMode.Append);

            (new System.Threading.Thread(SaveThread)).Start();


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

                if (toConsider != null && toConsider.HasValue) QueryForMoves(toConsider.Value);
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

        public static List<StateTransition> QueryForMoves(Board Board)
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

                if (!GameStates.ContainsKey(Board))
                {
                    GameStates.Upsert(Board, result);
                    var newConfigurationCount = 0;

                    foreach (var postMove in result)
                    {
                        if (!GameStates.ContainsKey(postMove.Board))
                        {
                            newConfigurationCount += 1;
                            OpenConfigurations.Add(postMove.Board);
                        }
                    }

                    LastExponentials[NextLastExponential++] = newConfigurationCount;
                    if (NextLastExponential == LastExponentials.Length) NextLastExponential = 0;

                    SaveLock.WaitOne();
                    PendingWrites.Add(Tuple.Create(Board, result));
                    SaveLock.ReleaseMutex();
                }
            }

            StateLock.ReleaseMutex();

            return result;
        }
    }
}
