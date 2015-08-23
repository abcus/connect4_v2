using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Connect4v2._0 {
    static class Search {

        internal static TTable hashTable = new TTable();
        internal static UInt64 nodesVisited = 0;
        internal static int bestMove = -1;
        internal static double fh1 = 0, fh = 0;
        
        public static int search(Position inputBoard, SearchInfo[] searchStack, int ssPos, int alpha, int beta, int depth) {
            
            // base case when player has won or nobody has won and board is full
            if (inputBoard.HasWon(inputBoard.arrayOfBitboard[(inputBoard.nPlies - 1) & 1])) {
                return -Utilities.WIN + ssPos;
            } else if (inputBoard.BoardFull()) {
                return Utilities.DRAW;
            }

            // probe transposition table
            TTEntry entry = Search.hashTable.probeTTable(inputBoard.key);
            if (entry.depth >= depth && entry.key == inputBoard.key
                && (entry.flag == Utilities.EXACT || (entry.flag == Utilities.L_BOUND && entry.evaluationScore >= beta) || (entry.flag == Utilities.U_BOUND && entry.evaluationScore <= alpha))) {
                return entry.evaluationScore;
            }

            int bestScore = -Utilities.INF;
            int movesMade = 0;
            bool raisedAlpha = false;

            for (int i = 0; i < 7; i++) {
                if (inputBoard.ColPlayable(i)) {
                    inputBoard.MakeMove(i);
                    int score = -search(inputBoard, searchStack, ssPos + 1, -beta, -alpha, depth - 1);
                    inputBoard.UnmakeMove();
                    nodesVisited++;
                    movesMade++;

                    if (score >= beta) {
                        TTEntry newEntry = new TTEntry(inputBoard.key, Utilities.L_BOUND, depth, score, i);
                        Search.hashTable.storeTTable(inputBoard.key, newEntry);

                        if (movesMade == 1) fh1++;
                        fh ++;
                        return score;
                    } else if (score > bestScore) {
                        bestScore = score;
                        if (score > alpha) {
                            alpha = score;
                            raisedAlpha = true;
                            if (depth == Utilities.MAX_DEPTH) {
                                bestMove = i;
                            }
                        }
                    }
                }
            }
            if (raisedAlpha) {
                TTEntry newEntry = new TTEntry(inputBoard.key, Utilities.EXACT, depth, alpha, bestMove);
                Search.hashTable.storeTTable(inputBoard.key, newEntry);
            } else {
                TTEntry newEntry = new TTEntry(inputBoard.key, Utilities.U_BOUND, depth, bestScore);
                Search.hashTable.storeTTable(inputBoard.key, newEntry);
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
