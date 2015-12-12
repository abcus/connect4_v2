using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Connect4v2._0 {
    class Position {

        internal int nPlies; // keeps track of the number of plies that have been played to get to the current position
        internal UInt64[] arrayOfBitboard; // stores the bitboards for the white and black players and the combined bitboard
        internal int[] moveHistory; // stores the history of the moves (represented by what column 0-7 the move was played in)
        internal int[] height; // stores the index of the lowest unoccupied square in each column
        internal UInt64 key = 0x1UL;

        // Constructor
        public Position(String inputString) {
            nPlies = 0;
            arrayOfBitboard = new UInt64[2];
            moveHistory = new int[42];
            height = new int[7];
            for (int i = 0; i < 7; i++) {
                height[i] = 7*i;
            }
            if (inputString != "") { // if something other than "ENTER" was typed in
                foreach (char c in inputString) {
                    this.MakeMove(this.height[c - '1']);
                }
            }
        }
        
        // make move (input is square index that piece ends up in)
        public void MakeMove(int move) {
            moveHistory[nPlies] = move;
            arrayOfBitboard[nPlies & 1] ^= 0x1UL << move;
            key ^= Constants.pieceZobrist[nPlies & 1, move];
            height[move/7]++;
            nPlies++;
        }

        // undo move
        public void UnmakeMove() {
            nPlies--;
            int lastMove = moveHistory[nPlies];
            height[lastMove/7]--;
            key ^= Constants.pieceZobrist[nPlies & 1, lastMove];
            arrayOfBitboard[nPlies & 1] ^= 0x1UL << lastMove;
            moveHistory[nPlies] = 0;
        }

        // returns whether there is a 4-in-a-row (copied shamelessly from John Tromp's Fhourstones program)
        public bool HasWon(UInt64 inputBoard) {
            // checks diagonal \
            UInt64 temp = inputBoard & (inputBoard >> Constants.HEIGHT);
            if ((temp & (temp >> 2 * Constants.HEIGHT)) != 0) {
                return true;
            }
            // checks horizontal -
            temp = inputBoard & (inputBoard >> Constants.H1);
            if ((temp & (temp >> 2 * Constants.H1)) != 0) {
                return true;
            }
            //checks diagonal /
            temp = inputBoard & (inputBoard >> Constants.H2);
            if ((temp & (temp >> 2 * Constants.H2)) != 0) {
                return true;
            }
            // checks vertical |
            temp = inputBoard & (inputBoard >> 1);
            return (temp & (temp >> 2)) != 0;
        }
    }

    internal struct Move {
        internal int move;
        internal int score;

        public Move(int move, int score) {
            this.move = move;
            this.score = score;
        }
    }
}
