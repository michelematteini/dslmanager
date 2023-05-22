using DSLManager.Ebnf;
using DSLManager.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Languages
{
    public abstract class BasicPreprocessor<OutType> : ICompiler<OutType>
    { 
        private class PreprocCompiler : SinglePassCompiler<string> 
        {
            private BasicPreprocessor<OutType> parent;

            public override string DebugName
            {
                get { return parent.DebugName; }
            }

            public override string FileExtension
            {
                get { return parent.FileExtension; }
            }

            public PreprocCompiler(BasicPreprocessor<OutType> parent)
            {
                this.parent = parent;
                IsMultipassCompiler = false;
            }

            protected override void ProcessIntermediateOutput(ref string ilOutput)
            {
                parent.OnNewProcessedDirective(ilOutput);
            }
        }

        private PreprocessorLexer ppLexer;
        private PreprocCompiler ppCompiler;

        // compilation cache
        StringBuilder compiledProgram;
        OutType finalOutput;
        private bool finalOutputComputed;

        public BasicPreprocessor(string directiveStartToken, string directiveEndToken)
        {
            ppCompiler = new PreprocCompiler(this);
            ppLexer = new PreprocessorLexer(directiveStartToken, directiveEndToken);
            ppLexer.CheckCall = (string program) => ppCompiler.CheckProgram(program);
            ppLexer.CompileCall = (string program) => ppCompiler.CompileProgram(program);
            ppLexer.EndOfCode += OnEndOfFile;
            Reset();
        }

        public BasicCompiler<string, string> DirectiveCompiler
        {
            get { return ppCompiler; }
        }

        public bool IsCompiledOutputAvailable { get; private set; }

        #region Compiler

        public bool IsMultipassCompiler { get; protected set; }

        public abstract string FileExtension { get; set; }

        public virtual string DebugName
        {
            get { return "DSLManager/Languages/BasicPreprocessor.cs"; }
        }

        public void Initialize()
        {
            ppCompiler.Initialize();
        }

        public void CheckProgram(string program)
        {
            Reset();
            ppLexer.CheckProgram(program);
        }

        public void CompileProgram(string program)
        {
            if(!IsMultipassCompiler)
                Reset();
            ppLexer.CompileProgram(program);
            IsCompiledOutputAvailable = true;
        }

        private void OnNewProcessedDirective(string processedDirective)
        {
            string previousCode = ppLexer.CodeBlock;
            OnNewProcessedDirective(ref previousCode, ref processedDirective);
            compiledProgram.Append(previousCode);
            compiledProgram.Append(processedDirective);
        }

        private void OnEndOfFile()
        {
            OnNewProcessedDirective(string.Empty);
        }

        /// <summary>
        /// Called for every preprocessor directive after it has been compiled. Both the source code before this directive and the compiled directive can be modified by this call before they get appended to the output.
        /// </summary>
        /// <param name="previousSourceCode">Source code before this directive, starting after the previous directive if available.</param>
        /// <param name="compiledDirective">The compiled proeprocessor directive.</param>
        protected abstract void OnNewProcessedDirective(ref string previousSourceCode, ref string compiledDirective);

        /// <summary>
        /// Converts the processed input code into the final output.
        /// </summary>
        /// <param name="processedCode">The processed code with all the preprocessing directive resolved.</param>
        /// <returns>The final output of the compilation process.</returns>
        protected abstract OutType BuildFinalOutput(string processedCode);

        public OutType GetCompiledResults(out Exception error)
        {
            error = null;

            if (!IsCompiledOutputAvailable)
                return default(OutType);

            if (!finalOutputComputed)
            {
                finalOutput = default(OutType);

                try
                {
                    finalOutput = BuildFinalOutput(compiledProgram.ToString());
                    finalOutputComputed = true;
                }
                catch (CompileError e)
                {
                    e.Source = DebugName;
                    error = e;
                }
            }

            return finalOutput;
        }

        public void Reset()
        {
            compiledProgram = new StringBuilder();
            finalOutputComputed = false;
            IsCompiledOutputAvailable = false;
        }

        #endregion
    }
}
