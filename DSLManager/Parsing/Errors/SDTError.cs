using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public class SDTError : CompileError
    {
        public SDTError(int line, string code, string source, string msg)
            : base(line, code, "Translation error: " + msg, source)
        {
        }
    }
}
