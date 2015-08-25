using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Connect4v2._0 {
    static class Constants {

        public const int WIDTH = 7, HEIGHT = 6, H1 = 7, H2 = 8, SIZE = 42, SIZE1 = 49;
        public const UInt64 ALL1 = 0x1FFFFFFFFFFFF, COL1 = 0x7F, BOTTOM = 0x40810204081, TOP = 0x1020408102040, ALL = ALL1 ^ TOP;
        public const int WIN = 100, DRAW = 0, GAMENOTOVER = -1;
        public const int MAX_DEPTH = 42, INF = 10000;
        public static UInt64[,] pieceZobrist = new UInt64[2, 49];
        public static Random rnd = new Random(0);
        public const int TT_SIZE = 15485867, BUCKET_SIZE = 4;
        public const int EXACT = 1, L_BOUND = 2, U_BOUND = 3;
        public const int ROOT = 1, NON_ROOT = 2;
        public static TTEntry EMPTY_ENTRY = new TTEntry(0,0,0,0);
        public const int PHASE_KILLER_0 = 0, PHASE_KILLER_1 = 1, PHASE_REST = 2;
        public const int CENTRAL_COLUMN_SCORE = 40, DISTANCE_PENALTY = 10;
        public static int KILLER_0_SCORE = 10, KILLER_1_SCORE = 8, HASH_MOVE_SCORE = 50;

        public static void DrawBoard(Position inputBoard) {
            for (int i = 0; i < 6; i++) {
                if (i == 0) {
                    Console.WriteLine("┌───┬───┬───┬───┬───┬───┬───┐");
                } else if (i >= 1) {
                    Console.WriteLine("├───┼───┼───┼───┼───┼───┼───┤");
                }
                
                for (int j = 0; j < 7; j++) {
                    int shiftNumber = 5 - i + (7*j);
                    String piece;
                    if ((0x1UL << shiftNumber & inputBoard.arrayOfBitboard[0]) != 0) {
                        piece = "O";
                    } else if ((0x1UL << shiftNumber & inputBoard.arrayOfBitboard[1]) != 0) {
                        piece = "X";
                    } else {
                        piece = " ";
                    }

                    Console.Write("│ " + piece + " ");
                }
                Console.WriteLine("│");
            }
            Console.WriteLine("└───┴───┴───┴───┴───┴───┴───┘");
            Console.WriteLine("  1   2   3   4   5   6   7");
            Console.WriteLine("Key: " + inputBoard.key);
            Console.WriteLine("");
        }

        // Extension method that generates a random ulong
        public static UInt64 NextUInt64(this Random rnd) {
            var buffer = new byte[sizeof(UInt64)];
            rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static void initSearchConstants() {
            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < 49; j++) {
                    Constants.pieceZobrist[i, j] = rnd.NextUInt64();
                }
            }
            for (int i = 0; i < Constants.MAX_DEPTH; i++) {
                for (int j = 0; j < 2; j++) {
                    Search.killerTable[i, j] = -1;
                }
            }
        }

        public static void PerftTest() {
            for (int i = 1; i < 14; i++) {
                Position test = new Position();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                long nodeCount = Constants.perft(i, test);
                Console.Write(i + "\t\t" + nodeCount + "\t\t");
                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds + "\t\t" + nodeCount/(stopwatch.ElapsedMilliseconds+1)*1000);
            }   
        }

        public static long perft(int depth, Position inputBoard) {
            long nodes = 0;
            if (inputBoard.GameStatus() != Constants.GAMENOTOVER) {
                return 1;
            } else if (depth == 1) {
                for (int i = 0; i < 7; i++) {
                    if (inputBoard.ColPlayable(i)) {
                        nodes++;
                    }
                }
            } else {
                for (int i = 0; i < 7; i++) {
                    if (inputBoard.ColPlayable(i)) {
                        inputBoard.MakeMove(i);
                        nodes += perft(depth - 1, inputBoard);
                        inputBoard.UnmakeMove();
                    }
                }
            }
            return nodes;
        }
    }
}
