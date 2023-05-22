using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public static class LRChar
    {
        public static bool IsLetter(char c)
        {
            int ci = (int)c;
            return 96 < ci && ci < 123 || 64 < ci && ci < 91;
        }

        public static bool IsDigit(char c)
        {
            int ci = (int)c;
            return 47 < ci && ci < 58;
        }

        public static bool IsLetterOrDigit(char c)
        {
            return IsLetter(c) || IsDigit(c);
        }

    }
}
