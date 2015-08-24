using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Connect4v2._0 {
    static class Search {

        internal static TTable TranspositionTable = new TTable();
        internal static UInt64 nodesVisited = 0;
        internal static int bestMove = -1;
        internal static double fh1 = 0, fh = 0;
        internal static int[,] killerTable = new int[Constants.MAX_DEPTH, 2];
        //internal static int[,] historyTable = new int[2, 7];


        public static int search(int nodeType, Position inputBoard, int ply, int alpha, int beta, int depth) {
            
            // base case when player has won or nobody has won and board is full
            if (inputBoard.HasWon(inputBoard.arrayOfBitboard[(inputBoard.nPlies - 1) & 1])) {
                return -Constants.WIN + ply;
            } else if (inputBoard.BoardFull()) {
                return Constants.DRAW;
            }

            // probe transposition table
            TTEntry entry = Search.TranspositionTable.probeTTable(inputBoard.key);
            if (entry.flag == Constants.EXACT
                || entry.flag == Constants.L_BOUND && entry.evaluationScore >= beta
                || entry.flag == Constants.U_BOUND && entry.evaluationScore <= alpha) {
                //Debug.Assert(entry.key == inputBoard.key && entry.depth == depth);
                if (entry.move!= -1 && entry.evaluationScore >= beta) {
                    updateKillers(entry.move, ply);
                }
                return entry.evaluationScore;
            } 

            int bestScore = -Constants.INF;
            int movesMade = 0;
            bool raisedAlpha = false;
            movePicker mPicker = new movePicker(inputBoard, ply);

            while (true) {
                int move = mPicker.getNextMove();
                
                if (move == -1) {
                    break;
                }

                inputBoard.MakeMove(move);
                int score = -search(Constants.NON_ROOT, inputBoard, ply + 1, -beta, -alpha, depth - 1);
                inputBoard.UnmakeMove();
                nodesVisited++;
                movesMade++;

                if (score >= beta) {
                    TTEntry newEntry = new TTEntry(inputBoard.key, Constants.L_BOUND, depth, score, move);
                    Search.TranspositionTable.storeTTable(inputBoard.key, newEntry);
                    updateKillers(move, ply);

                    if (movesMade == 1) {
                        fh1++;
                    } else {
                        fh++;
                    }
                    return score;
                } else if (score > bestScore) {
                    bestScore = score;
                    if (score > alpha) {
                        alpha = score;
                        raisedAlpha = true;
                        if (nodeType == Constants.ROOT) {
                            bestMove = move;
                        }
                    }
                }   
            }
            // Store in transposition table
            if (raisedAlpha) {
                TTEntry newEntry = new TTEntry(inputBoard.key, Constants.EXACT, depth, alpha, bestMove);
                Search.TranspositionTable.storeTTable(inputBoard.key, newEntry);
            } else {
                TTEntry newEntry = new TTEntry(inputBoard.key, Constants.U_BOUND, depth, bestScore);
                Search.TranspositionTable.storeTTable(inputBoard.key, newEntry);
            }
            return bestScore;
        }

        internal static void updateKillers(int move, int ply) {
            Debug.Assert(move >= 0 && move <= 6);
            if (move != Search.killerTable[ply, 0] && move != -1) {
                Search.killerTable[ply, 1] = Search.killerTable[ply, 0];
                Search.killerTable[ply, 0] = move;
            }
        }

        internal class movePicker {
            internal Position board;
            internal int ply;
            internal int[,] moveListScore = {{0, 1, 2, 3, 4, 5, 6}, {10, 20, 30, 40, 30, 20, 10}};
            internal int moveIndex = 0;


            public movePicker(Position inputBoard, int ply) {
                board = inputBoard;
                this.ply = ply;

                if (Search.killerTable[ply, 0] != -1) {
                    moveListScore[1, Search.killerTable[ply, 0]] += 5;
                }
                if (Search.killerTable[ply, 1] != -1) {
                    moveListScore[1, Search.killerTable[ply, 1]] += 4;
                }   
            }
            

            internal int getNextMove() {
                while (moveIndex < 7) {

                    int maxScore = -999;
                    int maxPosition = -1;
                    for (int i = moveIndex; i < 7; i++) {
                        if (moveListScore[1, i] > maxScore) {
                            maxScore = moveListScore[1, i];
                            maxPosition = i;
                        }
                    }
                    int moveTemp = moveListScore[0, moveIndex];
                    moveListScore[0, moveIndex] = moveListScore[0, maxPosition];
                    moveListScore[0, maxPosition] = moveTemp;

                    int scoreTemp = moveListScore[1, moveIndex];
                    moveListScore[1, moveIndex] = moveListScore[1, maxPosition];
                    moveListScore[1, maxPosition] = scoreTemp;

                    int move = moveListScore[0, moveIndex];
                    
                    if (board.ColPlayable(move)) {
                        moveIndex++;
                        return move;   
                    }
                    moveIndex++;
                }
                return -1;
            }
        }
    }
}
