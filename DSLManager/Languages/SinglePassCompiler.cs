using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Languages
{
    public abstract class SinglePassCompiler<OutType> : BasicCompiler<OutType, OutType>
    {
        public SinglePassCompiler()
        {
            IsMultipassCompiler = false;
        }

        protected override OutType BuildFinalOutput(List<OutType> ilOutputs)
        {
            return ilOutputs[0];
        }

        protected override void ProcessIntermediateOutput(ref OutType ilOutput)
        {
            
        }

    }
}
