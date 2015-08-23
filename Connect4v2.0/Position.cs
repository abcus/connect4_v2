using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        internal UInt64 key = 0x0UL;

        // Constructor
        public Position() {
            nPlies = 0;
            arrayOfBitboard = new UInt64[3];
            moveHistory = new int[42];
            height = new int[7];
            for (int i = 0; i < 7; i++) {
                height[i] = 7*i;
            }
            String inputString = Console.ReadLine();
            if (inputString != "") {
                foreach (char c in inputString) {
                    this.MakeMove(c - '1');
                }
            }
        }

        // make move (input is the column that the stone is being dropped into)
        public void MakeMove(int moveColumn) {
            moveHistory[nPlies] = moveColumn;
            arrayOfBitboard[nPlies & 1] ^= 0x1UL << height[moveColumn];
            arrayOfBitboard[2] ^= 0x1UL << height[moveColumn];
            key ^= Utilities.pieceZobrist[nPlies&1, height[moveColumn]];
            height[moveColumn]++;
            nPlies++;
        }

        // undo move
        public void UnmakeMove() {
            nPlies--;
            int lastMove = moveHistory[nPlies];
            height[lastMove]--;
            key ^= Utilities.pieceZobrist[nPlies & 1, height[lastMove]];
            arrayOfBitboard[nPlies & 1] ^= 0x1UL << height[lastMove];
            arrayOfBitboard[2] ^= 0x1UL << height[lastMove];
            moveHistory[nPlies] = 0;
        }

        // return whether a stone can be dropped into a column
        public bool ColPlayable(int col) {
            return height[col] - 7*col <= 5;
        }

        // returns whether the game board's 42 squares are full
        public bool BoardFull() {
            return arrayOfBitboard[2] == Utilities.ALL; // array of bitboard[2] is the bitboard of white | black
        }

        // returns whether there is a 4-in-a-row
        public bool HasWon(UInt64 inputBoard) {
            // checks diagonal \
            UInt64 temp = inputBoard & (inputBoard >> Utilities.HEIGHT);
            if ((temp & (temp >> 2*Utilities.HEIGHT)) != 0) {
                return true;
            }
            // checks horizontal -
            temp = inputBoard & (inputBoard >> Utilities.H1);
            if ((temp & (temp >> 2*Utilities.H1)) != 0) {
                return true;
            }
            //checks diagonal /
            temp = inputBoard & (inputBoard >> Utilities.H2);
            if ((temp & (temp >> 2*Utilities.H2)) != 0) {
                return true;
            }
            // checks vertical |
            temp = inputBoard & (inputBoard >> 1);
            return (temp & (temp >> 2)) != 0;
        }

        // returns whether game is won, drawn, or not over
        public int GameStatus() {
            if (this.HasWon(this.arrayOfBitboard[(nPlies - 1) & 1])) {
                return Utilities.WIN;
            } else if (this.BoardFull()) {
                return Utilities.DRAW;
            } 
            return Utilities.GAMENOTOVER;
        }
    }
}
