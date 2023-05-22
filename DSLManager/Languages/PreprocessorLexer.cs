using DSLManager.Parsing;
using DSLManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Languages
{
    public class PreprocessorLexer
    {
        private string startToken, endToken;
        private bool checkOnly;

        /// <summary>
        /// Event called when the end of code is reached while compiling.
        /// This event isn't raised when calling CheckProgram()
        /// </summary>
        public event Action EndOfCode;

        /// <summary>
        /// Event called when CompileProgram() is called.
        /// </summary>
        public event Action NewCompile;

        /// <summary>
        /// Action performed to check preprocessor code.
        /// </summary>
        public Action<string> CheckCall { get; set; }

        /// <summary>
        /// Action performed to compile preprocessor code.
        /// </summary>
        public Action<string> CompileCall { get; set; }

        /// <summary>
        /// Source code between the previous and the current preprocessor code.
        /// </summary>
        public string CodeBlock
        {
            get;
            private set;
        }

        public string StartCodeToken
        {
            get
            {
                return startToken;
            }
        }

        public string EndCodeToken
        {
            get
            {
                return endToken;
            }
        }

        public PreprocessorLexer(string startToken, string endToken)
        {
            this.startToken = startToken;
            this.endToken = endToken;

            // initialize events
            EndOfCode += () => { };
            NewCompile += () => { };
        }

        public void CheckProgram(string program)
        {
            checkOnly = true;
            CompileProgram(program);
            checkOnly = false;
        }

        public void CompileProgram(string program)
        {
            if (!checkOnly) NewCompile.Invoke();

            int endCodeIndex = 0, startCodeIndex = program.IndexOf(startToken);

            while (startCodeIndex >= 0)
            {
                if (!checkOnly) CodeBlock = program.Substring(endCodeIndex, startCodeIndex - endCodeIndex);
                startCodeIndex += startToken.Length;
                endCodeIndex = program.IndexOf(endToken, startCodeIndex);

                if (endCodeIndex < 0)
                // open code section
                {
                    Reset();
                    throw new CompileError(-1, "PPLEX0001", "End of file reached while parsing code, are you missing a \"" + endToken + "\"?", "");
                }

                string ppCode = program.Substring(startCodeIndex, endCodeIndex - startCodeIndex);

                try
                {
                    if (checkOnly)
                    {
                        if (CheckCall != null) CheckCall.Invoke(ppCode);
                    }
                    else
                    {
                        if (CompileCall != null) CompileCall.Invoke(ppCode);
                    }
                }
                catch (CompileError e)
                // interrupt if an error occurred during compile
                {
                    Reset();
                    //add to the error line (relative to pp code only), the code line from which the pp code starts.
                    e.Line += program.LineIndexOf(startCodeIndex);
                    throw e;
                }

                //update pp code indices
                endCodeIndex += endToken.Length;
                startCodeIndex = program.IndexOf(startToken, endCodeIndex);
            }

            if (!checkOnly)
            {
                CodeBlock = program.Substring(endCodeIndex);
                EndOfCode.Invoke();
            }
        }

        public void Reset()
        {
            CodeBlock = string.Empty;
        }

    }
}
