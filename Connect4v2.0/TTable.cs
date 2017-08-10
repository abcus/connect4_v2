using System;
using System.Diagnostics;

namespace Connect4v2._0 {
    class TTable {

        internal TTEntry[] transpositionTable;

        // Constructor
        public TTable() {
            this.transpositionTable = new TTEntry[Constants.TT_SIZE + Constants.BUCKET_SIZE];
        }

        // Store entry into TTable
        public void storeTTable(UInt64 key, TTEntry entry) {
            int index = (int) (key % Constants.TT_SIZE);

            // If entry in bucket has same hash key, then replace
            for (int i = index; i < index + Constants.BUCKET_SIZE; i++) {
                if (this.transpositionTable[i].key == key) {
                    this.transpositionTable[i] = entry;
                    return;
                }
            }
            // If there is an empty spot in the bucket, then store it there
            for (int i = index; i < index + Constants.BUCKET_SIZE; i++) {
                if (this.transpositionTable[i].key == 0) {
                    this.transpositionTable[i] = entry;
                    return;
                }
            }
            // If all spots full, then replace entry with lowest depth
            int shallowestDepth = Constants.INF;
            int indexOfShallowestEntry = -1;
            for (int i = index; i < index + Constants.BUCKET_SIZE; i++) {
                if (this.transpositionTable[i].depth < shallowestDepth) {
                    shallowestDepth = this.transpositionTable[i].depth;
                    indexOfShallowestEntry = i;
                }
            }
            this.transpositionTable[indexOfShallowestEntry] = entry;
        }

        // Retrieve entry from TTable
        public TTEntry probeTTable(UInt64 key) {
            Debug.Assert(key != 0);
            int index = (int) (key % Constants.TT_SIZE);

            for (int i = index; i < index + Constants.BUCKET_SIZE; i++) {
                if (this.transpositionTable[i].key == key) {
                    return this.transpositionTable[i];
                }
            }
            return Constants.EMPTY_ENTRY;
        }
    }

    internal struct TTEntry {
        internal UInt64 key;
        internal int flag;
        internal int depth;
        internal int evaluationScore;
        internal int move;

        public TTEntry(UInt64 key, int flag, int depth, int evaluationScore, int move = Constants.NO_MOVE) {
            this.key = key;
            this.flag = flag;
            this.depth = depth;
            this.evaluationScore = evaluationScore;
            this.move = move;
        }
    }
}
