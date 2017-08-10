using System;
using System.Diagnostics;

namespace Connect4v2._0 {
    static class Solve {

        internal static TTable TranspositionTable = new TTable();
        internal static UInt64 nodesVisited = 0;
        internal static double fh1 = 0, fh = 0;
        internal static int[,] killerTable = new int[Constants.MAX_DEPTH, 2];
        internal static int[,] historyTable = new int[2, 49];
       
        public static int solve(int nodeType, Position inputBoard, int ply, int alpha, int beta, int depth) {
            
            // return score for terminal state
            if (inputBoard.HasWon(inputBoard.arrayOfBitboard[(inputBoard.nPlies - 1) & 1])) {
                return -Constants.WIN + ply;
            } else if (inputBoard.nPlies == 42) {
                Debug.Assert(depth == 0);
                return Constants.DRAW;
            }

            // "Mate" distance pruning
            alpha = Math.Max(ply-Constants.WIN, alpha);
            beta = Math.Min(Constants.WIN - (ply + 1), beta);
            if (alpha >= beta) {
                return alpha;
            }
            
            // probe transposition table
            TTEntry entry = Solve.TranspositionTable.probeTTable(inputBoard.key);

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
            int numberOfMoves = 0, moveIndex = 0;
            bool raisedAlpha = false;
            Move[] moveList = moveGenerator(ply, hashMove, ref numberOfMoves, inputBoard);
            int bestMove = Constants.NO_MOVE;

            // loop through all moves
            while (true) {
                int move = getNextMove(numberOfMoves, ref moveIndex, moveList);

                if (move == Constants.NO_MOVE) {
                    break;
                }
                
                inputBoard.MakeMove(move);
                int score = -solve(Constants.NON_ROOT, inputBoard, ply + 1, -beta, -alpha, depth - 1);
                inputBoard.UnmakeMove();
                nodesVisited++;
                movesMade++;

                if (score >= beta) {
                    TTEntry newEntry = new TTEntry(inputBoard.key, Constants.L_BOUND, depth, score, move);
                    Solve.TranspositionTable.storeTTable(inputBoard.key, newEntry);
                    updateKillers(move, ply);
                    updateHistory(depth, ply, move);

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
                    }
                }   
            }
            // Store in transposition table
            if (raisedAlpha) {
                TTEntry newEntry = new TTEntry(inputBoard.key, Constants.EXACT, depth, alpha, bestMove);
                Solve.TranspositionTable.storeTTable(inputBoard.key, newEntry);
            } else {
                TTEntry newEntry = new TTEntry(inputBoard.key, Constants.U_BOUND, depth, bestScore, Constants.NO_MOVE); // no best move
                Solve.TranspositionTable.storeTTable(inputBoard.key, newEntry);
            }
            return bestScore;
        }

        // update the killer table
        internal static void updateKillers(int move, int ply) {
            Debug.Assert(move >= 0 && move <= 47);
            if (move != Solve.killerTable[ply, 0] && move != Constants.NO_MOVE) {
                Solve.killerTable[ply, 1] = Solve.killerTable[ply, 0];
                Solve.killerTable[ply, 0] = move;
            }
        }

        // update the history table, divide all entries by 2 when an entry exceeds 100,000,000
        internal static void updateHistory(int depth, int ply, int move) {
            Solve.historyTable[(ply & 1), move] += depth;
            if (Solve.historyTable[(ply & 1), move] >= 100000000) {
                for (int i = 0; i < 49; i++) {
                    Solve.historyTable[(ply & 1), i] /= 2;
                }
            }
        }
        
        // generates array of moves
        internal static Move[] moveGenerator(int ply, int hashMove, ref int numberOfMoves, Position inputBoard) {
            Move[] moveList = new Move[7];

            for (int i = 0; i < 7; i++) {
                int lowestFreeInCol = inputBoard.height[i];
                if (lowestFreeInCol - 7 * i <= 5) {
                    int score = (Constants.CENTRAL_COLUMN_SCORE - Constants.DISTANCE_PENALTY * Math.Abs(i - 3));
                    if (hashMove != Constants.NO_MOVE && lowestFreeInCol == hashMove) {
                        score += Constants.HASH_MOVE_SCORE;
                    }
                    if (lowestFreeInCol == Solve.killerTable[ply, 0]) {
                        score += Constants.KILLER_0_SCORE;
                    }
                    else if (lowestFreeInCol == Solve.killerTable[ply, 1]) {
                        score += Constants.KILLER_1_SCORE;
                    }
                    score += Solve.historyTable[ply & 1, lowestFreeInCol];

                    moveList[numberOfMoves] = new Move(lowestFreeInCol, score);
                    numberOfMoves++;
                }
            }
            return moveList;
        }

        internal static int getNextMove(int numberOfMoves, ref int moveIndex, Move[] moveList) {
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
                return move;
            }
            return Constants.NO_MOVE;
        }
    }
}
