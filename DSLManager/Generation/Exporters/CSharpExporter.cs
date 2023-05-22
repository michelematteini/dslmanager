using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLManager.Utils;

namespace DSLManager.Generation.Exporters
{
    public class CSharpExporter : IProjectExporter
    {
        private const string EXPORT_SETUP_START = "CSharp Exporter (Do not edit)";
        private const string EXPORT_SETUP_END = "End Group"; // used to detect any exporte grp ending.
        private const string OVERRIDE_FLAG = "/* TODO: Generated file, remove this comment to keep your changes! */";
        private const string ITEM_GROUP = "<ItemGroup>";
        private const string ITEM_GROUP_END = "</ItemGroup>";
		private const char CS_OPEN_BLOCK = '{';
		private const char CS_CLOSE_BLOCK = '}';
		private const string USER_CODE_START = "#region User Code";
        private const string USER_CODE_HINT = "\t\t//TODO insert your custom code here! Will be preserved after each generation.";
		private const string USER_CODE_END = "#endregion";

        private CsExportMode exportMode;
        
        // export instance variables
        private string targetProject;
        private string projectRootPath, projectFilePath;
        private List<string> projectLines;

        /// <summary>
        /// Get or set the path to the reference project for the next export. 
        /// If left empty, the first cs project found tracing back from the current path will be used.
        /// </summary>
        public string Target 
        {
            get
            {
                return targetProject;
            }
            set
            {
                if (!value.EndsWith(".csproj") || !File.Exists(value))
                {
                    throw new ArgumentException("Target project must be a valid path to a *.csproj file!");
                }
                this.targetProject = value;
            }
        }
        

        public CSharpExporter()
            : this(CsExportMode.ExportOnly)
        {

        }

        public CSharpExporter(CsExportMode exportMode)
        {
            this.exportMode = exportMode;
            this.targetProject = string.Empty;
        }

        /// <summary>
        /// Initialize this object to export for a given C# project (*.csproj).
        /// </summary>
        /// <param name="projectFilePath">The project file path.</param>
        public void BeginExport()
        {
            //setup path
            this.projectFilePath = Target;
            this.projectRootPath = new FileInfo(this.projectFilePath).Directory.FullName;

            //load project file
            projectLines = new List<string>(File.ReadAllLines(this.projectFilePath));

            //reset exporter setup informations
            int setupGrpIndex = -1;
            prepareExporterGroup(EXPORT_SETUP_START, out setupGrpIndex);

            if (exportMode != CsExportMode.ExportOnly)
            {
                projectLines.InsertRange(setupGrpIndex, new string[]{
                    //post build events (copy compiler exe)
                    "<PropertyGroup>",
                    "   <PostBuildEvent>",
                    "       if '$(OutputType)' == 'Exe' (",
                    "           echo d | xcopy \"$(TargetDir).\" \"$(ProjectDir)compiler\" /Y /e",
                    "       )",
                    "   </PostBuildEvent>",
                    "</PropertyGroup>"
                });

                if (exportMode == CsExportMode.CompilerProject)
                {
                    projectLines.InsertRange(setupGrpIndex, new string[]{
                        // pre build events (start compiler)
                        "<PropertyGroup>",
                        "   <PreBuildEvent>",
                        "       if '$(OutputType)' == 'Library' \"$(ProjectDir)compiler\\$(ProjectName).exe\"",
                        "   </PreBuildEvent>",
                        "</PropertyGroup>",
                    });
                }
            }       
        }

        public void ExportProject(CodeProject cp)
        {
            //create directories
            string ouputPath = projectRootPath + Path.DirectorySeparatorChar + cp.ProjectName + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(ouputPath);

            //write source files
            if (cp.ContainsSources)
            {
                foreach (CodeFile codeFile in cp.Sources)
                {
					codeFile.IndentCode("{", "}");
					string sourceCode = codeFile.SourceCode;
                    bool writeFile = true; 
                    bool fileExists = File.Exists(ouputPath + codeFile.FileName);

					switch(codeFile.GenerationMode)
					{
						case GenerationMode.Override:
							//default setting
							break;
						case GenerationMode.Merge:
                            if (fileExists)
                            {
                                string oldSource = File.ReadAllText(ouputPath + codeFile.FileName);
                                sourceCode = mergeSources(oldSource, sourceCode);
                            }
							break;
						case GenerationMode.OverrideIfNotModified:
							//add-verify flag comment
                            if (fileExists)
							//file exists, check modified flag
							{
								StreamReader cfReader = File.OpenText(ouputPath + codeFile.FileName);
								string header = cfReader.ReadLine();
								cfReader.Close();
								writeFile = header == OVERRIDE_FLAG;
								sourceCode = OVERRIDE_FLAG + "\n" + sourceCode;
							}
							break;
						case GenerationMode.CreateIfNotExists:
                            writeFile = !fileExists;
							break;
					}
					
					if(writeFile)
					{
						(new FileInfo(ouputPath + codeFile.FileName)).Directory.Create();
						File.WriteAllText(ouputPath + codeFile.FileName, sourceCode);
					}
                }
            }

            //write bin files
            if (cp.ContainsBinaries)
            {
                foreach (BinFile binFile in cp.Binaries)
                {
                    (new FileInfo(ouputPath + binFile.FileName)).Directory.Create();
                    File.WriteAllBytes(ouputPath + binFile.FileName, binFile.Content);
                }
            }
            
            //add file references to the project
            int projectGrpIndex;
            prepareItemGroup(cp.ProjectName, out projectGrpIndex);
            if (cp.ContainsSources)
            {
                foreach (CodeFile codeFile in cp.Sources)
                {
                    if (codeFile.FileType == CodeFileType.ExternalFile) continue;
                    projectLines.Insert(projectGrpIndex, createIncludeString(codeFile.FileType, cp.ProjectName, codeFile.FileName));
                }
            }
            if (cp.ContainsBinaries)
            {
                foreach (BinFile binFile in cp.Binaries)
                {
                    if (binFile.FileType == CodeFileType.ExternalFile) continue;
                    projectLines.Insert(projectGrpIndex, createIncludeString(binFile.FileType, cp.ProjectName, binFile.FileName));
                }
            }
        }

        public void EndExport()
        {
            File.WriteAllLines(projectFilePath, projectLines.ToArray());
            this.targetProject = string.Empty;
        }

        private string createIncludeString(CodeFileType type, string projectName, string fileName)
        {
            switch (type)
            {
                case CodeFileType.SourceCode:
                    return string.Format("<Compile Include=\"{0}{2}{1}\" />", projectName, fileName, Path.DirectorySeparatorChar);
                case CodeFileType.RuntimeResource:
                    return string.Format("<Content Include=\"{0}{2}{1}\"><CopyToOutputDirectory>Always</CopyToOutputDirectory></Content>", projectName, fileName, Path.DirectorySeparatorChar);
                default:
                case CodeFileType.EmbeddedResource:
                    return string.Format("<EmbeddedResource Include=\"{0}{2}{1}\" />", projectName, fileName, Path.DirectorySeparatorChar);
            }

        }

        private int getItemGroupIndex(string name)
        {
            return getItemGroupIndex(name, 0);
        }

        private int getItemGroupIndex(string name, int startIndex)
        {
            string itemGrpSign = toSign(name);
            for (int i = startIndex; i < projectLines.Count; i++)
            {
                if (projectLines[i].Trim() == itemGrpSign) { return i; }
            }
            return -1;
        }

        /// <summary>
        /// Prepare this project item group, containing the files to be added to the project.
        /// If the item group is already available, all includes are removed from it.
        /// </summary>
        /// <param name="name">Name of the item group.</param>
        /// <param name="insertIndex">Index to be used to insert new include statements.</param>
        private void prepareItemGroup(string name, out int insertIndex)
        {
            // prepare a basic group
            prepareExporterGroup(name, out insertIndex);

            // add itemGroup enclosure
            projectLines.Insert(insertIndex, ITEM_GROUP_END);
            projectLines.Insert(insertIndex, ITEM_GROUP);
            insertIndex++;
        }

        private void prepareExporterGroup(string name, out int insertIndex)
        {
            //check if this item group already exists
            int exGrpIndex = getItemGroupIndex(name);
            string exGrpSign = toSign(name);
            string exGrpEnd = toSign(EXPORT_SETUP_END);

            if (exGrpIndex > 0)
            //clear the old item group
            {
                int exGrpEndIndex = getItemGroupIndex(EXPORT_SETUP_END, exGrpIndex);
                if (exGrpEndIndex < 0)
                // invalid group (no ending line found)
                {
                    throw new Exception(String.Format("Exporter ({0}) : invalid group ({1}) found in project. No termination string found", projectFilePath, name));
                }

                // remove old lines
                projectLines.RemoveRange(exGrpIndex + 1, exGrpEndIndex - exGrpIndex - 1);
                insertIndex = exGrpIndex + 1;
            }
            else
            {
                // create new group
                insertIndex = projectLines.Count;
                projectLines.Insert(projectLines.Count - 1, exGrpSign);    
                projectLines.Insert(projectLines.Count - 1, exGrpEnd);
            }
        }

        private string toSign(string name)
        {
            return "<!--" + name + "-->";
        }

        #region Source Merge

        private string mergeSources(string oldSource,string newSource)
		{
			StringBuilder merge = new StringBuilder();
            string[] sl_old = oldSource.ToLines();
            string[] sl_new = newSource.ToLines();
			
			//preprocess old code
			Dictionary<String, ulong> codeBlocks = new Dictionary<String, ulong>();
			string sign = string.Empty;
			ulong cbIndex = 0;
			bool userCode = false, emptyMethod = true;
            int cbSkipCount = 0;//number of CS_CLOSE_BLOCK to be skipped (because of blocks used inside methods)
			for(int i = 0; i < sl_old.Length; i++)
			{
				string cleanLine = sl_old[i].Trim();
				if(cbIndex > 0)
				{
                    emptyMethod = emptyMethod && cleanLine == string.Empty;

                    if ((!userCode && cleanLine == CS_CLOSE_BLOCK.ToString()) || (userCode && cleanLine == USER_CODE_END))
                    //possible end of a block
                    {
                        if (!userCode && cbSkipCount > 0)
                        //the closed block is part of this method
                        {
                            cbSkipCount--;
                        }
                        else
                        //actual end of block
                        {
                            if (i > (int)cbIndex && !emptyMethod)
                            {
                                codeBlocks.Add(sign, cbIndex + ((ulong)i << 32));
                            }
                            cbIndex = 0;
                            userCode = false;
                            emptyMethod = true;
                        }
                    }
                    else if (!userCode)
                    {
                        //increment opened block count to skip next close sequence
                        updateBlockCounts(cleanLine, ref cbSkipCount);
                    }

				}
				else
				{
                    if (cleanLine == CS_OPEN_BLOCK.ToString())
					{
						sign = sl_old[i - 1].Trim();
                        if (!sign.StartsWith("namespace") && !sign.Contains(" class "))
                        {
                            cbIndex = (ulong)i + 1;
                        }
					}
					else if(cleanLine == USER_CODE_START)
					{
                        cbIndex = (ulong)i + 1;
						sign = cleanLine;
						userCode = true;
					}
				}
			}
			
			//add old codeblocks to the new stub
			bool discardWhiteLine = false;
			for (int li_new = 0; li_new < sl_new.Length; li_new++)
			{
				//initialize new line and check if empty
				string lnew = sl_new[li_new].Trim();			
				if(discardWhiteLine)
				{
					if(lnew == string.Empty) continue;
					discardWhiteLine = false;
				}
				
				//append to merge
				merge.AppendLine(sl_new[li_new]);

                if (lnew == CS_OPEN_BLOCK.ToString())
				//search for a filled method body
				{
					sign = sl_new[li_new - 1].Trim();
					addCodeBlock(sl_old, merge, codeBlocks, sign);
					discardWhiteLine = true;
				}
				else if(lnew == USER_CODE_START)
				//add user code together with all other unmatched methods
				{
                    addCodeBlock(sl_old, merge, codeBlocks, USER_CODE_START);
					discardWhiteLine = true;
				}
			}

			return merge.ToString();
		}
		
		private void addCodeBlock(string[] sourceLines, StringBuilder code, Dictionary<String, ulong> cbIndex, string name)
		{
			//if its a usercode block, add all left code (or an hint if nothing available)
			if(name == USER_CODE_START)
			{
                if (cbIndex.ContainsKey(USER_CODE_START))
                {
                    //add previous user code
                    addCodeBlock(sourceLines, code, cbIndex, USER_CODE_START, 0, 0, false);
                }
                else if (cbIndex.Count == 0)
                {
                    //add a comment to the user
                    code.AppendLine(USER_CODE_HINT);
                }

                //add all left user code / methods
				string[] unmatchedBlocks = cbIndex.Keys.ToArray();
				for(int i = 0; i < unmatchedBlocks.Length; i++)
				{
					addCodeBlock(sourceLines, code, cbIndex, unmatchedBlocks[i], -2, 1, true);
                    code.AppendLine();
				}

                return;
			}

            addCodeBlock(sourceLines, code, cbIndex, name, 0, 0, false);
		}
		
		private void addCodeBlock(string[] sourceLines, StringBuilder code, Dictionary<String, ulong> cbIndex, string name, int startOffset, int endOffset, bool cleanOverrides)
		{
			if(cbIndex.ContainsKey(name))
			//copy body
			{
                int startBody = (int)(cbIndex[name] & 0xffffffff) + startOffset;
                int endBody = (int)(cbIndex[name] >> 32) + endOffset;
				if(cleanOverrides) sourceLines[startBody] = sourceLines[startBody].Replace(" override ", " ");
				
				for(int bi = startBody; bi < endBody; bi++)
				{
                    code.AppendLine(sourceLines[bi]);
				}
				cbIndex.Remove(name);		
            }
		}

        private void updateBlockCounts(string source, ref int openBlocks)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == CS_OPEN_BLOCK) openBlocks++;
                else if (source[i] == CS_CLOSE_BLOCK) openBlocks--;
            }
        }

        #endregion

    }

    /// <summary>
    /// Defines how the exporter make project behave.
    /// </summary>
    public enum CsExportMode
    {
        /// <summary>
        /// Just update the project, adding new files.
        /// </summary>
        ExportOnly,
        /// <summary>
        /// Update the project, adding new files. 
        /// If the project is compiled as exe it will works like a compiler; a post-build event is added to copy this compiler to te project root.
        /// </summary>
        CreateCompiler,
        /// <summary>
        /// Update the project, adding new files. If compiled as exe, the bin folder is copied in the project root to be used as a compiler. 
        /// When compiled as Library, the previously ceated compiler is used to compile the project before building it.
        /// </summary>
        CompilerProject
    }

}
