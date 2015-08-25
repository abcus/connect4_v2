﻿using System;
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

            while (true) {
                Position test = new Position();
                Constants.DrawBoard(test);
                Console.Write((test.nPlies & 1) == 0 ? "white: " : "black: ");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                int evaluation = Search.search(Constants.ROOT, test, 0, -Constants.INF, Constants.INF, (Constants.MAX_DEPTH - test.nPlies));
                Console.Write(evaluation >= 0 ? evaluation == 0 ? "draw" : "win in " + (Constants.WIN - evaluation): "loss in " + (Constants.WIN + evaluation));
                Console.WriteLine("\tbestmove: " + (Search.PVMove + 1));
                stopwatch.Stop();
                Console.WriteLine("nodes: " + Search.nodesVisited + "\ttime: " + stopwatch.ElapsedMilliseconds + "\tnps: " + Search.nodesVisited / ((ulong)stopwatch.ElapsedMilliseconds + 1) * 1000);
                Console.WriteLine("fh1: " + Search.fh1 / (Search.fh1 + Search.fh) * 100);
            } 
        }
    }
}
