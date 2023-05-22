using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public struct LRAction
    {
        public LRActionType Type;
        public int Value;

        public LRAction(LRActionType type, int value)
        {
            this.Type = type;
            this.Value = value;
        }
    }

    public enum LRActionType
    {
        Nop,
        Shift,
        Reduce,
        Goto,
        Accept,
    }

}
