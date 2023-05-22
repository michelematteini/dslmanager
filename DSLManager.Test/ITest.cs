using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Test
{
    public interface ITest
    {
        string TestName { get; }

        void Run();
    }
}
