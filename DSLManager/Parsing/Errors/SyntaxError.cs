using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLManager.Ebnf;

namespace DSLManager.Parsing
{
    public class SyntaxError : CompileError
    {
        public SyntaxError(int line, string code, string source, string found, List<EbnfExpression> expected)
            : base(line, code, buildErrorString(found, expected.ToArray()), source)
        {

        }

        public SyntaxError(int line, string code, string source, string found, EbnfExpression expected)
            : base(line, code, buildErrorString(found, new EbnfExpression[] { expected }), source)
        {
            
        }

        public SyntaxError(int line, string code, string source, EbnfToken found, List<EbnfExpression> expected)
            : this(line, code, source, string.Format("\"{0}\" (as {1})", found.StringValue, found), expected)
        {

        }

        private static string buildErrorString(string found, EbnfExpression[] expected)
        {
            string error = String.Format("Syntax Error: {0}", found);
            if (expected.Length > 0)
            {
                error += " found, expected: " + expected[0].ToEbnf();
                for (int i = 1; i < expected.Length; i++)
                {
                    EbnfExpression t = expected[i];
                    error += " or " + t.ToEbnf();
                }
            }

            return error;
        }

        private static string buildErrorString(int position, string found)
        {
            string error = String.Format("Position {0}: \"{1}\" is not a valid expression for this language.", position, found);

            return error;
        }


    }
}
