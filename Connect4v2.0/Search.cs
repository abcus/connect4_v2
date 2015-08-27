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
        internal static int PVMove = Constants.NO_MOVE;
        internal static double fh1 = 0, fh = 0;
        internal static int[,] killerTable = new int[Constants.MAX_DEPTH, 2];
        //internal static int[,] historyTable = new int[2, 7];
       
        public static int search(int nodeType, Position inputBoard, int ply, int alpha, int beta, int depth) {
            
            // base case when player has won or nobody has won and board is full
            if (inputBoard.HasWon(inputBoard.arrayOfBitboard[(inputBoard.nPlies - 1) & 1])) {
                return -Constants.WIN + ply;
            } else if (inputBoard.BoardFull()) {
                Debug.Assert(depth == 0);
                return Constants.DRAW;
            }

            // probe transposition table
            TTEntry entry = Search.TranspositionTable.probeTTable(inputBoard.key);

            // If entry has a flag type, then key will match (probe function performs check), only empty entries will have a key that doesn't match
            if (entry.flag == Constants.EXACT
                || entry.flag == Constants.L_BOUND && entry.evaluationScore >= beta
                || entry.flag == Constants.U_BOUND && entry.evaluationScore <= alpha) {
                Debug.Assert(entry.key == inputBoard.key && entry.depth == depth);

                // Only exact and lower bound entries can satisfy this condition and they all have valid moves stored, so don't have to check if move == -1 (only the case with upper bound entries)
                if (entry.evaluationScore >= beta) { 
                    Debug.Assert(entry.move != Constants.NO_MOVE);
                    updateKillers(entry.move, ply);
                }
                return entry.evaluationScore;
            }
            
            // hash move, if entry is exact then code will not be reached and if entry is an upper bound then it will have no move
            int hashMove = (entry.flag == Constants.L_BOUND && entry.evaluationScore < beta) ? entry.move : Constants.NO_MOVE;
            int bestScore = -Constants.INF;
            int movesMade = 0;
            bool raisedAlpha = false;
            //int[] moveHistory = {-1, -1, -1, -1, -1, -1, -1};
            movePicker mPicker = new movePicker(inputBoard, ply, hashMove);
            int bestMove = Constants.NO_MOVE;

            // loop through all moves
            while (true) {
                int move = mPicker.getNextMove();
                
                if (move == Constants.NO_MOVE) {
                    break;
                }
                
                inputBoard.MakeMove(move);
                int score = -search(Constants.NON_ROOT, inputBoard, ply + 1, -beta, -alpha, depth - 1);
                inputBoard.UnmakeMove();
                //moveHistory[movesMade] = move;
                nodesVisited++;
                movesMade++;

                if (score >= beta) {
                    TTEntry newEntry = new TTEntry(inputBoard.key, Constants.L_BOUND, depth, score, move);
                    Search.TranspositionTable.storeTTable(inputBoard.key, newEntry);
                    updateKillers(move, ply);
                    //updateHistory(moveHistory, ply, movesMade);

                    if (movesMade == 1) {
                        fh1++;
                    } else {
                        fh++;
                    }
                    return score;
                } else if (score > bestScore) {
                    bestScore = score;
                    bestMove = move;
                    if (score > alpha) {
                        alpha = score;
                        raisedAlpha = true;
                        if (nodeType == Constants.ROOT) {
                            PVMove = move;
                        }
                    }
                }   
            }
            // Store in transposition table
            if (raisedAlpha) {
                TTEntry newEntry = new TTEntry(inputBoard.key, Constants.EXACT, depth, alpha, bestMove);
                Search.TranspositionTable.storeTTable(inputBoard.key, newEntry);
            } else {
                TTEntry newEntry = new TTEntry(inputBoard.key, Constants.U_BOUND, depth, bestScore, Constants.NO_MOVE); // no best move
                Search.TranspositionTable.storeTTable(inputBoard.key, newEntry);
            }
            return bestScore;
        }

        internal static void updateKillers(int move, int ply) {
            Debug.Assert(move >= 0 && move <= 6);
            if (move != Search.killerTable[ply, 0] && move != Constants.NO_MOVE) {
                Search.killerTable[ply, 1] = Search.killerTable[ply, 0];
                Search.killerTable[ply, 0] = move;
            }
        }

        /*internal static void updateHistory(int[] moveHistory , int ply, int movesMade) {
            int movesMadeBeforeCutoff = movesMade - 1;
            Search.historyTable[(ply & 1), moveHistory[movesMadeBeforeCutoff]] += movesMadeBeforeCutoff;
            for (int i = 0; i < movesMadeBeforeCutoff; i++) {
                Search.historyTable[(ply & 1), moveHistory[i]] --;
            }
        }*/

        internal class movePicker {
            internal Position board;
            internal int ply;
            internal Move[] moveList;
            internal int moveIndex = 0;
            internal int numberOfMoves = 0;
            internal int hashMove = Constants.NO_MOVE;

            public movePicker(Position inputBoard, int ply, int hashMove) {
                board = inputBoard;
                this.ply = ply;
                this.hashMove = hashMove;
                moveList = this.moveGenerator();
            }
            // generates array of moves
            internal Move[] moveGenerator() {
                Move[] moveList = new Move[7];
                
                for (int i = 0; i < 7; i++) {
                    if (board.height[i] - 7 * i <= 5) {
                        int score = (Constants.CENTRAL_COLUMN_SCORE - Constants.DISTANCE_PENALTY*Math.Abs(i - 3));
                        if (hashMove != Constants.NO_MOVE && i == hashMove) {
                            score += Constants.HASH_MOVE_SCORE;
                        } 
                        if (i == Search.killerTable[ply, 0]) {
                            score += Constants.KILLER_0_SCORE;
                        } else if (i == Search.killerTable[ply, 1]) {
                            score += Constants.KILLER_1_SCORE;
                        }
                        //score += Search.historyTable[ply & 1, i];

                        moveList[numberOfMoves] = new Move(board.height[i], score);
                        numberOfMoves++;
                    }
                }
                return moveList;
            }

            internal int getNextMove() {
                while (moveIndex < numberOfMoves) {

                    int maxScore = -Constants.INF;
                    int maxPosition = -1;
                    for (int i = moveIndex; i < numberOfMoves; i++) {
                        
                        if (moveList[i].score > maxScore) {
                            maxScore = (moveList[i].score);
                            maxPosition = i;
                        }
                    }
                    Move moveTemp = moveList[moveIndex];
                    moveList[moveIndex] = moveList[maxPosition];
                    moveList[maxPosition] = moveTemp;

                    int move = (moveList[moveIndex].move);
                    
                    moveIndex++;
                    return move/7;   
                }
                return Constants.NO_MOVE;
            }
        }
    }
}
