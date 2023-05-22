using DSLManager.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Generation
{
    public struct CodeFile
    {
        public string SourceCode;
        public string FileName;
        public CodeFileType FileType;
        public GenerationMode GenerationMode;

        public static CodeFile FromTemplate(string templateName, string fileName, params object[] args)
        {
            CodeFile tmplSource = new CodeFile(StringUtils.RTemplate(templateName, args), fileName, CodeFileType.SourceCode);
            return tmplSource;
        }

        public CodeFile(string sourceCode, string fileName, CodeFileType fileType)
        {
            this.SourceCode = sourceCode;
            this.FileName = fileName;
            this.FileType = fileType;
            this.GenerationMode = Generation.GenerationMode.Override;
        }

        public void IndentCode(string indentIn, string indentOut)
        {
            string[] codeLines = SourceCode.Replace("\r\n", "\n").Split('\n');
            StringBuilder indentedCode = new StringBuilder();
            int curIndent = 0;
            foreach (string line in codeLines)
            {
                string cleanLine = line.Trim();
                if (cleanLine.StartsWith(indentOut)) curIndent--;
                indentedCode.Append(new string('\t', curIndent));
                indentedCode.Append(cleanLine);
                indentedCode.Append('\n');
                if (cleanLine.StartsWith(indentIn)) curIndent++;              
            }
            SourceCode = indentedCode.ToString();
        }

    }

    public enum CodeFileType
    {
        /// <summary>
        /// Visible from project; if the format is known, will be compiled.
        /// </summary>
        SourceCode,
        /// <summary>
        /// Visible from project, and will be packed into the executable assembly.
        /// </summary>
        EmbeddedResource,
        /// <summary>
        /// Visible from project, will be copied to the output directory.
        /// </summary>
        RuntimeResource,
        /// <summary>
        /// Generation only, this file will not be visible from project.
        /// </summary>
        ExternalFile
    }

    public enum GenerationMode
    {
        Override,
        OverrideIfNotModified,
        CreateIfNotExists, 
        Merge//Each exporter will manage this in different ways
    }

}
