using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public class ShiftReduceConflict : Exception
    {
        public ILRNode SourceNode { get; private set; }

        public ShiftReduceConflict(ILRNode onState)
            : base("On State: " + onState.ToString())
        {
            this.SourceNode = onState;
        }
    }
}
