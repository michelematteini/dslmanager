using System;
using System.Collections.Generic;
using DSLManager.Ebnf;
using DSLManager.Parsing;
using DSLManager.Ebnf.Filters;
using DSLManager.Utils;

namespace DSLManager.Languages
{
    /// <summary>
    /// A compiler initialize with a language grammar.
    /// </summary>
    /// <typeparam name="ILType">The intermediate output type that the language compiles in.</typeparam>
    /// <typeparam name="OutType">The final output type, converted from the ILType after compilation using language-specific logic.</typeparam>
    public abstract class BasicCompiler<ILType, OutType> : Language<ILType>, ICompiler<OutType> 
    {
        // syntax and grammar related members
        private bool initalized;
        private FullNormalizingFilter normalizingFilter;

        //language cache
        private ParseTable parsingTable;
        private LexerTable lexerTable;
        private DerivationRule[] normalizedRules;
        private int[] normSDTIndices;

        //results cache
        private List<ILType> intermediateOutputs;
        private OutType finalOutput;
        private bool finalOutputComputed;

        public BasicCompiler()
        {
            normalizingFilter = new FullNormalizingFilter();
            initalized = false;
            Reset();
        }

        public BasicCompiler(string ebnfGrammar) : this()
        {
            DerivationRule[] grammmar = EbnfParser.ParseGrammar(ebnfGrammar);

            foreach (DerivationRule rule in grammmar)
                AddRule(rule);
        }


        public void Initialize()
        {
            if (initalized) return;
            UpdateLanguage();
            Reset();
            initalized = true;
        }

        #region Language

        protected override void OnRuleModified(LanguageRule rule)
        {
            initalized = false;
        }

        /// <summary>
        /// Commits all the changes to the language grammar,and recalculates the parse table.
        /// This is automatically called on check/compile if the language has been changed.
        /// </summary>
        private void UpdateLanguage()
        {
            normalizedRules = normalizingFilter.ApplyFilter(DerivationRules.ToArray(), out this.normSDTIndices);
            parsingTable = LRParser.GenerateParseTable(normalizedRules, GenerateNormalizedPriorities());
            lexerTable = LRParser.GenerateLexerTable(normalizedRules);
        }

        private RulePriority[] GenerateNormalizedPriorities()
        {
            RulePriority[] normPriorities = new RulePriority[normSDTIndices.Length];
            for (int i = 0; i < normSDTIndices.Length; i++)
                normPriorities[i] = normSDTIndices[i] < 0 ? RulePriority.Default : Rules[normSDTIndices[i]].Priority;
            return normPriorities;
        }

        #endregion

        #region Source code comments 

        protected string InlineCommentStart { get; set; }

        protected string MultilineCommentStart { get; set; }

        protected string MultilineCommentEnd { get; set; }

        private string RemoveComments(string program)
        {
            return program.RemoveComments(InlineCommentStart, MultilineCommentStart, MultilineCommentEnd);
        }

        #endregion

        #region Compiler

        /// <summary>
        /// Check whether a translation is available for each rule so that the language can be translated.
        /// </summary>
        public bool CanCompile
        {
            get
            {
                foreach (LanguageRule r in Rules)
                    if (r.Translator == null) return false;

                return true;
            }
        }

        public bool IsCompiledOutputAvailable { get; private set; }

        public virtual string DebugName
        {
            get { return "DSLManager/Languages/BasicCompiler.cs"; }
        }

        public virtual string FileExtension { get; protected set; }

        public bool IsMultipassCompiler { get; protected set; }

        protected virtual void OnBeforeCompile()
        {
            Initialize();

            if (!IsMultipassCompiler || finalOutputComputed)
                Reset();
        }

        public virtual void CheckProgram(string program)
        {
            OnBeforeCompile();
            LRParser.CheckSyntax(parsingTable, LRParser.Tokenize(RemoveComments(program), lexerTable), normalizedRules);
        }

        public virtual void CompileProgram(string program)
        {
            if (!CanCompile)
                throw new CompileError(
                    -1,
                    "BCOMP0001",
                    "The program string cannot be compiled beacuse this language is incomplete (not all the translations are available).\n" +
                    "If you just want to check your program correctness, use CheckProgram().",
                    DebugName
                );

            OnBeforeCompile();

            try
            {
                ILType result;
                LRParser.CompileCode<ILType>(parsingTable, LRParser.Tokenize(RemoveComments(program), lexerTable), this.normalizedRules/*Rules*/, Translators.ToArray(), normSDTIndices, false, out result);
                ProcessIntermediateOutput(ref result);
                IsCompiledOutputAvailable = true;
                intermediateOutputs.Add(result);
            }
            catch (CompileError e)
            {
                e.Source = DebugName;
                throw e;
            }
        }

        public virtual void Reset()
        {
            IsCompiledOutputAvailable = false;
            intermediateOutputs = new List<ILType>();
            finalOutputComputed = false;
        }

        #endregion

        /// <summary>
        /// Called after a single compile process is over.
        /// <para/>This method can be used to process the current output to prepare it for the user.
        /// </summary>
        /// <param name="rawResult"></param>
        protected abstract void ProcessIntermediateOutput(ref ILType ilOutput);

        /// <summary>
        /// Converts intermediate outputs into the final output.
        /// </summary>
        /// <param name="ilOutputs">The list of intermediate outputs. Contains only a single element if IsMultipassCompiler = false.</param>
        /// <returns>The final output of the compilation process.</returns>
        protected abstract OutType BuildFinalOutput(List<ILType> ilOutputs);

        /// <summary>
        /// Retrive the results of compilation.
        /// For compilers where IsMultipassCompiler = false, this should be called after each compile call.
        /// If this compiler is multi-pass, this call should be made after all the compilation calls.
        /// </summary>
        /// <param name="error">Error occurred during compilation. If error = null, no error occurred and the last compilation succeeded.</param>
        /// <returns></returns>
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
                    finalOutput = BuildFinalOutput(intermediateOutputs);
                    finalOutputComputed = true;
				}
				catch(CompileError e)
				{
                    e.Source = DebugName;
					error = e;
				}           
            }

            return finalOutput;
        }

    }

}
