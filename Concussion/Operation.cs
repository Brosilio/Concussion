using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concussion
{
    public struct Operation
    {
        public int count;
        public bool flag;
        public EOpType opType;

        public Operation(int count, bool flag, EOpType opType)
        {
            this.count = count;
            this.flag = flag;
            this.opType = opType;
        }

        public override string ToString()
        {
            return $"{opType} x {count} / {flag}";
        }
    }

    public enum EOpType
    {
        MoveLeft, MoveRight,
        Incr, Decr,
        Print, Read,
        LoopStart, LoopEnd,
        Nop, ZeroMem,
        Copy
    }
}
