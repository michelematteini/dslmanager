using DSLManager.Generation;
using DSLManager.Languages;
using DSLManager.Parsing;
using System;
using System.Collections.Generic;
using System.IO;

namespace DSLManager.Utils
{
    public class ConsoleCompiler
    {
        private ICompiler<CodeProject>[] compilers;
        private IProjectExporter exporter;
        private List<SourceFileInfo>[/*compilerIndex*/] sources;

        public event Action<ConsoleCompiler> LoadSources;

        public ConsoleCompiler(ICompiler<CodeProject>[] compilers, IProjectExporter exporter)
        {
            this.compilers = compilers;
            this.exporter = exporter;
            ClearSources();
        }

        public ConsoleCompiler(ICompiler<CodeProject> compiler, IProjectExporter exporter)
            : this(new ICompiler<CodeProject>[] { compiler }, exporter)
        {

        }

        public void AddSources(string[] filePaths)
        {
            foreach(string file in filePaths)
            {
                if (!File.Exists(file)) continue;
                
                for(int i = 0; i < compilers.Length; i++)
                {
                    if (compilers[i].FileExtension.ToLower() == Path.GetExtension(file).Substring(1).ToLower())
                        sources[i].Add(new SourceFileInfo(file));
                }               
            }
        }

        public void AddSourcesFromFolder(string rootFolder, bool includeSubDirs)
        {
            for (int i = 0; i < compilers.Length; i++)
            {
                string[] files = Directory.GetFiles(
                    rootFolder, 
                    "*." + compilers[i].FileExtension, 
                    includeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                );

                AddSources(files);
            }
            
        }

        public void ClearSources()
        {
            sources = new List<SourceFileInfo>[compilers.Length];
            for (int i = 0; i < compilers.Length; i++)
            {
                sources[i] = new List<SourceFileInfo>();
            }
        }

        public bool Compile()
        {
            if (!initExporter()) return false;
            if (!initCompilers()) return false;

            bool errorOccurred = false;

            int compilerIndex = 0;
            foreach (ICompiler<CodeProject> compiler in compilers) //for each compiler       
            {
                // compile and export source files for this compiler
                for (int i = 0; i < sources[compilerIndex].Count; i++)
                {
                    SourceFileInfo sourceFile = sources[compilerIndex][i];
                    DSLDebug.Log("Compiling " + sourceFile.FileName + "...", DSLDebug.MsgType.InfoProgress);

                    try
                    {
                        compiler.CompileProgram(sourceFile.Code);
                    }
                    catch (CompileError compError)
                    {
                        errorOccurred = true;
                        DSLDebug.Log("Compilation failed in " + sourceFile.FileName + ":", DSLDebug.MsgType.Failure);
                        if (compError.Source == string.Empty) compError.Source = sourceFile.FileName;
                        DSLDebug.Log(compError.Message, DSLDebug.MsgType.Error);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        DSLDebug.Log(CompileError.FormatError(ex.Source, "DSLUC0001", ex.Message), DSLDebug.MsgType.Error);
                        DSLDebug.Log(ex.StackTrace, DSLDebug.MsgType.Error);
                        continue;
                    }

                    DSLDebug.Log("Compiled.", DSLDebug.MsgType.Success);

                    if (!compiler.IsMultipassCompiler) errorOccurred = errorOccurred || exportOrLogErrors(compiler, sourceFile.FileName);
                }

                // multipass compilers export everything at the end
                if (compiler.IsMultipassCompiler)
                {
                    errorOccurred = errorOccurred || exportOrLogErrors(compiler, compiler.DebugName);
                }
            }

            DSLDebug.Log("Build completed" + (errorOccurred ? ", some errors occurred." : " successfully."), errorOccurred ? DSLDebug.MsgType.Failure : DSLDebug.MsgType.Success); 
            exporter.EndExport();
            return !errorOccurred;
        }

        public void LoadFilesAndCompile()
        {
            ClearSources();
            LoadSources?.Invoke(this);
            Compile();
        }

        public void LoadAndCompileLoop()
        {
            ConsoleKeyInfo key = new ConsoleKeyInfo('n', ConsoleKey.N, false, false, false);

            while (true)
            {
                DSLDebug.Log("");
                DSLDebug.Log("Compile project? (Y/N) ", DSLDebug.MsgType.Question);
                key = waitUser();
                if (key.Key != ConsoleKey.Y) break;

                LoadFilesAndCompile();
            }
        }

        private bool initExporter()
        {
            DSLDebug.Log("Initializing exporter...", DSLDebug.MsgType.InfoProgress);
            try
            {
                exporter.BeginExport(); //initialize exporter
                DSLDebug.Log("Exporter initialized!", DSLDebug.MsgType.Success);
            }
            catch(Exception e)
            {
                DSLDebug.Log("Exporter loading error.", DSLDebug.MsgType.Error);
                DSLDebug.Log(e.ToString(), DSLDebug.MsgType.Error);
                return false;
            }
            return true;
        }

        private bool initCompilers()
        {
            //initialize compilers
            try
            {
                foreach (ICompiler<CodeProject> compiler in compilers)
                {
                    DSLDebug.Log(string.Format("Initializing {0} ({1}) compiler...", compiler.GetType().Name, compiler.FileExtension));
                    compiler.Initialize();
                    DSLDebug.Log("Done.", DSLDebug.MsgType.Success);
                }
            }
            catch (Exception langExc)
            {
                DSLDebug.Log("Language loading error.", DSLDebug.MsgType.Error);
                DSLDebug.Log(langExc is CompileError ? langExc.Message : langExc.ToString(), DSLDebug.MsgType.Error);
                return false;//some compiler cannot be initialized, abort compilation?
            }

            return true;
        }

        private bool exportOrLogErrors(ICompiler<CodeProject> compiler, string contextName)
        {
            DSLDebug.Log("Exporting results...");
            Exception error = null;
            CodeProject res = compiler.GetCompiledResults(out error);
            if (error != null)
            {
                if (error.Source == string.Empty) error.Source = contextName;
                DSLDebug.Log(error is CompileError ? error.Message : error.ToString(), DSLDebug.MsgType.Error);
            }
            else
            {
                if (res != null) exporter.ExportProject(res);
                DSLDebug.Log("Export completed.", DSLDebug.MsgType.Success);
            }
            return error != null;
        }

        private ConsoleKeyInfo waitUser()
        {
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            try
            {
                key = Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                //Code running in pre/post build.
                //@see http://stackoverflow.com/questions/22554047/visual-studio-error-in-post-build-with-exe
            }
            return key;
        }
    }

    public struct SourceFileInfo
    {
        public string FilePath;
        public string FileName;
        public string Code;

        public SourceFileInfo(string path)
        {
            FilePath = path;
            FileName = Path.GetFileName(path);
            Code = File.ReadAllText(path);
        }
    }

}
