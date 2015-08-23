using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Connect4v2._0 {
    class TTable {

        internal TTEntry[] hashTable;

        public TTable() {
            this.hashTable = new TTEntry[Utilities.TT_SIZE + Utilities.BUCKET_SIZE];
        }

        public void storeTTable(UInt64 key, TTEntry entry) {
            int index = (int) (key%Utilities.TT_SIZE);
            this.hashTable[index] = entry;
        }

        public TTEntry probeTTable(UInt64 key) {
            Debug.Assert(key != 0);
            int index = (int) (key % Utilities.TT_SIZE);
            if (this.hashTable[index].key == key) {
                return this.hashTable[index];
            }
            return Utilities.EMPTY_ENTRY;
        }
    }

    internal struct TTEntry {
        internal UInt64 key;
        internal int flag;
        internal int depth;
        internal int evaluationScore;
        internal int move;

        public TTEntry(UInt64 key, int flag, int depth, int evaluationScore, int move = -1) {
            this.key = key;
            this.flag = flag;
            this.depth = depth;
            this.evaluationScore = evaluationScore;
            this.move = move;
        }
    }
}
