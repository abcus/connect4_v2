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
            internal int[] moveList;
            internal int moveIndex = 0;
            internal int numberOfMoves = 0;

            public movePicker(Position inputBoard, int ply) {
                board = inputBoard;
                this.ply = ply;
                moveList = this.moveGenerator();
            }
            // generates array of moves
            internal int[] moveGenerator() {
                int[] moveList = { -1, -1, -1, -1, -1, -1, -1 };
                
                for (int i = 0; i < 7; i++) {
                    if (board.ColPlayable(i)) {
                        int score = (Constants.CENTRAL_COLUMN_SCORE - Constants.DISTANCE_PENALTY*Math.Abs(i - 3));
                        if (i == Search.killerTable[ply, 0]) {
                            score += Constants.KILLER_0_SCORE;
                        } else if (i == Search.killerTable[ply, 1]) {
                            score += Constants.KILLER_1_SCORE;
                        }

                        moveList[numberOfMoves] = i | score << Constants.SCORE_SHIFT;
                        numberOfMoves++;
                    }
                }
                return moveList;
            }

            internal int getNextMove() {
                while (moveIndex < numberOfMoves) {

                    int maxScore = -999;
                    int maxPosition = -1;
                    for (int i = moveIndex; i < numberOfMoves; i++) {
                        
                        if (((moveList[i] & Constants.SCORE_MASK) >> Constants.SCORE_SHIFT) > maxScore) {
                            maxScore = ((moveList[i] & Constants.SCORE_MASK) >> Constants.SCORE_SHIFT);
                            maxPosition = i;
                        }
                    }
                    int moveTemp = moveList[moveIndex];
                    moveList[moveIndex] = moveList[maxPosition];
                    moveList[maxPosition] = moveTemp;

                    int move = (moveList[moveIndex] & Constants.MOVE_MASK);
                    
                    
                    moveIndex++;
                    return move;   
                }
                return -1;
            }
        }
    }
}
