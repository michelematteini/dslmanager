using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Parsing
{
    public class CompileError : Exception
    {
        public int Line;
        private string Code;

        public CompileError(int line, string code, string msg, string source)
            : base(msg)
        {
            this.Line = line;
            this.Code = code;
            this.Source = source;
        }

        public static string FormatWarning(string sourcePath, int line, string errorCode, string msg)
        {
            return formatError(sourcePath, line, true, errorCode, msg);
        }

        public static string FormatWarning(string sourcePath, string errorCode, string msg)
        {
            return formatError(sourcePath, -1, true, errorCode, msg);
        }

        public static string FormatError(string sourcePath, int line, string errorCode, string msg)
        {
            return formatError(sourcePath, line, false, errorCode, msg);
        }

        public static string FormatError(string sourcePath, string errorCode, string msg)
        {
            return formatError(sourcePath, -1, false, errorCode, msg);
        }

        private static string formatError(string sourcePath, int line, bool isWarning, string errorCode, string msg)
        {
            if (line < 0)
            {
                return string.Format("{0}: {1} {2}: {3}", sourcePath, isWarning ? "warning" : "error", errorCode, msg);
            }
            else
            {
                return string.Format("{0}({1}): {2} {3}: {4}", sourcePath, line, isWarning ? "warning" : "error", errorCode, msg);
            }
        }

        public override string Message
        {
            get
            {
                return FormatError(Source, Line, Code, base.Message);
            }
        }
    }
}
