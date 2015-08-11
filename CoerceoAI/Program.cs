using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoerceoAI
{
    class Program
    {
        static void Main(string[] args)
        {
            Game.Tables.Initialize();
            Game.AI.StartAI("AI.TXT");
            Game.AI.FocusOn(Game.Tables.InitialBoard);

            var previousBoardConfigurations = 0;

            System.Threading.Thread.Sleep(1000); //Let the AI thread get going.

            while (true)
            {
                switch (Game.AI.Stage)
                {
                    case 0:
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("READING FILE...");
                        Console.WriteLine("BOARD CONFIGURATIONS:           {0}             ", Game.AI.CountOfConfigurationsExplored);
                        break;
                    case 1:
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("DISCOVERING OPEN CONFIGURATIONS....");
                        Console.WriteLine("BOARD CONFIGURATIONS:           {0}             ", Game.AI.CountOfConfigurationsExplored);
                        Console.WriteLine("QUEUED CONFIGURATIONS:          {0}             ", Game.AI.CountOfOpenConfigurations);
                        break;
                    case 2:
                        var configurations = Game.AI.CountOfConfigurationsExplored;
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("AI STATS                                        ");
                        Console.WriteLine("BOARD CONFIGURATIONS:           {0}             ", configurations);
                        var queued = Game.AI.CountOfOpenConfigurations;
                        Console.WriteLine("QUEUED CONFIGURATIONS:          {0}             ", queued);
                        Console.WriteLine("EXPONENT (AVG LAST 100):        {0}             ", Game.AI.Exponential);
                        Console.WriteLine("MAX EXPONENT:                   {0}             ", Game.AI.MaxExponential);
                        Console.WriteLine("MEMORY USAGE:                   {0} KB          ", (int)(System.GC.GetTotalMemory(false) / 1024));
                        Console.WriteLine("BOARD CONFIGURATIONS / SECOND : {0}             ", (configurations - previousBoardConfigurations) * 10);
                        previousBoardConfigurations = configurations;
                        break;
                }

                System.Threading.Thread.Sleep(100);
            }

        }
    }
}
