using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using DSLManager.Parsing;
using DSLManager.Ebnf;
using System.Collections;
using DSLManager.Utils;

namespace DSLManager.Generation
{
    public class CodeProject
    {
		//Value-state fields
		private object value;
		
		//Project-state fields
        private List<CodeFile> sourceFiles;
        private List<BinFile> binFiles;
        private Dictionary<string, object> env;
		
		//Public fields
		public string ProjectName;

		#region Constructors

        public CodeProject()
        {
        }

		public static CodeProject Empty
        {
			get
			{
				return new CodeProject();
			}
        }
		
		public static CodeProject FromValue(object value)
		{
			CodeProject cp = new CodeProject();
            cp.value = value;
			return cp;
		}

        /// <summary>
        /// Combine all the values from a previous SDT.
        /// </summary>
        /// <param name="sdtValues"></param>
        /// <param name="sdtName"></param>
        /// <returns></returns>
        public static CodeProject FromSDT(ISymbolMapper<CodeProject> sdtValues, string sdtName)
        {
            return FromValue(MergeValues(sdtValues, sdtName));
        }

        public CodeProject(string projectPath)
            : this()
        {
            this.ProjectName = projectPath;
        }
		
		#endregion

		#region Properties
		
		public object this[string varName]
		{
			get
			{
                if (varName == string.Empty) return this.value;
				if (env == null) return null;
				return env[varName];
			}
			set
			{
                if (varName == string.Empty) this.value = value;
				if (env == null) env = new Dictionary<string, object>();
				env[varName] = value;
			}
		}
			
        public object Value
        {
            get
            {
                return this.value;
            }
			set
			{
				this.value = value;
			}
        }
		
		public bool ContainsValue
		{
            get { return this.value != null; }
		}
		
		public List<CodeFile> Sources
		{
			get
			{
				return this.sourceFiles;
			}
		}
		
		public bool ContainsSources
		{
            get { return this.sourceFiles != null; }
		}
		
		public List<BinFile> Binaries
		{
			get
			{
				return this.binFiles;
			}
		}
		
		public bool ContainsBinaries
		{
            get { return this.binFiles != null; }
		}
		
        #endregion

		public void AddSource(CodeFile file)
		{
			if (!ContainsSources) this.sourceFiles = new List<CodeFile>();
			this.sourceFiles.Add(file);
		}

        public void AddTemplate(string templateName, string fileName, params object[] args)
        {
            AddSource(CodeFile.FromTemplate(templateName, fileName, args));
        }
		
		public void AddBinary(BinFile file)
		{
			if (!ContainsBinaries) this.binFiles = new List<BinFile>();
			this.binFiles.Add(file);
		}

        public void AddFilesFrom(CodeProject other)
        {
			//sources
			if (!ContainsSources && other.ContainsSources) 
			{
				this.sourceFiles = new List<CodeFile>();
			}
			if(other.ContainsSources) sourceFiles.AddRange(other.sourceFiles);
			
			//binaries
			if (!ContainsBinaries && other.ContainsBinaries) 
			{
				this.binFiles = new List<BinFile>();
			}
			if(other.ContainsBinaries) binFiles.AddRange(other.binFiles);
		}
		
		public void AddFilesFrom(ISymbolMapper<CodeProject> values, string sdtName)
		{
			int cpCount = values.GetInstanceCount(sdtName);
			for(int i = 0; i < cpCount; i++)
            {
				AddFilesFrom(values[sdtName, i]);
			}
		}

        public static SDTranslator<CodeProject> EmptySDT()
        {
            return delegate(ISDTArgs<CodeProject> args) { return CodeProject.Empty; };
        }
		
		public override string ToString()
        {
            if (ContainsValue) return value.ToString();
            return ProjectName;
        }

        public bool ContainsKey(string name)
        {
            if (env == null) return false;
            return env.ContainsKey(name);
        }

        public static IList MergeValueSet(ISymbolMapper<CodeProject> sdtValues, string sdtName, string valueName, object skipThis)
        {
            int valueCount = sdtValues.GetInstanceCount(sdtName);
            HashSet<object> valueList = new HashSet<object>();
            for (int i = 0; i < valueCount; i++)
            {
                object value = sdtValues[sdtName, i][valueName];
                if (value == skipThis) continue;

                valueList.Add(value);
            }
            return valueList.ToArray();
        }

        public static IList MergeValues(ISymbolMapper<CodeProject> sdtValues, string sdtName, string valueName)
        {
            int valueCount = sdtValues.GetInstanceCount(sdtName);
            object[] valueList = new object[valueCount];
            for (int i = 0; i < valueCount; i++)
            {
                valueList[i] = sdtValues[sdtName, i][valueName];
            }
            return valueList;
        }

        /// <summary>
        /// Returns a list of values, specified through a list of CodeProject initialized with FromValue().
        /// </summary>
        /// <param name="sdtValues">The symbol mapper from which the codeproject list is taken from.</param>
        /// <param name="sdtName">The name of the symbol to search for in the symbol mapper.</param>
        /// <returns></returns>
        public static IList MergeValues(ISymbolMapper<CodeProject> sdtValues, string sdtName)
        {
            return MergeValues(sdtValues, sdtName, "");
        }

        public static IList MergeLists(ISymbolMapper<CodeProject> sdtValues, string sdtName, string valueName)
        {
            int lstCount = sdtValues.GetInstanceCount(sdtName);
            IList[] sdtLists = new IList[lstCount];
            for (int i = 0; i < lstCount; i++)
            {
                sdtLists[i] = (IList)sdtValues[sdtName, i][valueName];
            }
            return RepeatList.Concatenate(sdtLists);
        }

        public static IList MergeLists(ISymbolMapper<CodeProject> sdtValues, string sdtName)
        {
            return MergeLists(sdtValues, sdtName, "");
        }

    }
}
