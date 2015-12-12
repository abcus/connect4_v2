using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect4v2._0 {
    class Program {
        static void Main(string[] args) {
            Constants.initSearchConstants();
            //Constants.PerftTest();
            Program.InitiateSearch();
        }

        static void InitiateSearch() {
            Console.Write("Enter position to solve: ");
            String inputString = Console.ReadLine();
            Position test = new Position(inputString);
            Constants.DrawBoard(test);
            Console.Write((test.nPlies & 1) == 0 ? "White: " : "Black: ");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int evaluation = Solve.solve(Constants.ROOT, test, 0, -Constants.INF, Constants.INF, (Constants.MAX_DEPTH - test.nPlies));
            Console.Write(evaluation >= 0 ? evaluation == 0 ? "draw" : "win in " + (Constants.WIN - evaluation) : "loss in " + (Constants.WIN + evaluation));
            Console.WriteLine("\t\tBest move: Column " + (Solve.TranspositionTable.probeTTable(test.key).move/ 7 + 1));
            stopwatch.Stop();
            Console.WriteLine("Nodes: " + Solve.nodesVisited.ToString("#,##0") + "\t\tTime: " + stopwatch.ElapsedMilliseconds.ToString("#,##0") + " milliseconds\t\tNPS: " + (Solve.nodesVisited / ((ulong)stopwatch.ElapsedMilliseconds + 1) * 1000).ToString("#,##0"));
            Console.WriteLine("FH1: " + Solve.fh1 / (Solve.fh1 + Solve.fh) * 100);
        }
    }
}
