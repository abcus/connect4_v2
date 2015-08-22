using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Connect4v2._0 {
    static class Search {

        internal static TTable hashTable = new TTable();
        internal static UInt64 nodesVisited = 0;
        internal static int bestMove = -1;
        

        public static int search(Board inputBoard, SearchInfo[] searchStack, int ssPos, int alpha, int beta, int depth) {
            
            if (depth == 0) {
                return Utilities.DRAW;
            }

            

            int bestScore = -Utilities.INF;
            for (int i = 0; i < 7; i++) {
                if (inputBoard.ColPlayable(i)) {
                    inputBoard.MakeMove(i);
                    nodesVisited++;
                    if (inputBoard.HasWon(inputBoard.arrayOfBitboard[(inputBoard.nPlies - 1) & 1])) {
                        inputBoard.UnmakeMove(); 
                        return Utilities.WIN;
                    }
                    int score = -search(inputBoard, searchStack, ssPos + 1, -beta, -alpha, depth - 1);
                    inputBoard.UnmakeMove();

                    if (score >= beta) {
                        return score;
                    } else if (score > bestScore) {
                        bestScore = score;
                        if (score > alpha) {
                            alpha = score;
                            if (depth == Utilities.MAX_DEPTH) {
                                bestMove = i;
                            }
                        }
                    }
                }
            }
            return bestScore;
        }
    }

    internal struct SearchInfo {
        internal int ply;
        internal int killer0;
        internal int killer1;
    }
}
