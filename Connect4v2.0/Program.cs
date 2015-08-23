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
            Utilities.initSearchConstants();

            while (true) {
                Position test = new Position();
                SearchInfo[] searchStack = new SearchInfo[Utilities.MAX_DEPTH];

                Utilities.DrawBoard(test);
                Console.Write((test.nPlies & 1) == 0 ? "white: " : "black: ");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine(Search.search(test, searchStack, 0, -Utilities.INF, Utilities.INF, Utilities.MAX_DEPTH) + "\tbestmove: " + (Search.bestMove + 1));
                stopwatch.Stop();
                Console.WriteLine("nodes: " + Search.nodesVisited + "\ttime: " + stopwatch.ElapsedMilliseconds + "\tnps: " + Search.nodesVisited / ((ulong)stopwatch.ElapsedMilliseconds + 1) * 1000);
                Console.WriteLine("fh1: " + Search.fh1/(Search.fh1 + Search.fh) * 100);
            }      
        }
    }
}
