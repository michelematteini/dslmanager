using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public class SemanticError : CompileError
    {
        public SemanticError(int line, string code, string source, string msg)
            : base(line, code, msg, source)
        {
        }

    }
}
