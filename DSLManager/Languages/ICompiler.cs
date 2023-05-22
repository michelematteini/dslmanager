using DSLManager.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Languages
{
    /// <summary>
    /// Generic interface of a compiler.
    /// </summary>
    /// <typeparam name="OutType">Output type of this compiler.</typeparam>
    public interface ICompiler<OutType>
    {
        /// <summary>
        /// Initialize the compiler: can be called to preemptively process the language, that would be otherwise processed on the first check /compile.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Check whether the program syntax is correct.
        /// </summary>
        /// <param name="program">The Program to be checked.</param>
        void CheckProgram(string program);

        /// <summary>
        /// Compile the given program
        /// </summary>
        /// <param name="program">The program to be compiled.</param>
        void CompileProgram(string program);

        /// <summary>
        /// Retrive the current compilation result.
        /// </summary>
        OutType GetCompiledResults(out Exception error);

        /// <summary>
        /// If true, compilation results will be managed by the user and indicates that many compile calls can be made before retriving the overall result.
        /// If false, any compile will override the previous result.
        /// </summary>
        bool IsMultipassCompiler { get; }

        /// <summary>
        /// Preferred source file extension
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// A name used to identify the compiler when errors are thrown.
        /// </summary>
        string DebugName { get; }

        /// <summary>
        /// Clear any cached compilation result or processed source file.
        /// </summary>
        void Reset();
    }
}
