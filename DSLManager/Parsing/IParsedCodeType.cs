using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public interface IParsedCodeType
    {    
        /// <summary>
        /// If a compiler user IL type implements this call, the parsed code preview will be created from this instead of using ToString()
        /// </summary>
        string GetSrcCode();
    }
}
