using System;
using System.Collections.Generic;
using System.IO;

namespace DSLManager.Utils
{
    /// <summary>
    /// DSL utilities to browse for project files and sources.
    /// This static class can be used to share directory informations to languages at compile time.
    /// </summary>
    public class DSLDir
    {
        private static readonly string ORDER_FILE_EXT = ".order";

        #region Static Utilities

        /// <summary>
        /// Get the current thread exe directory.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Return the first file found with the given extension, tracing back from the current thread directory.
        /// </summary>
        /// <param name="ext">The file extension to look for ( e.g. "exe")</param>
        /// <returns></returns>
        public static string GetFirstFileMatch(string startDir, string ext)
        {
            DirectoryInfo curPath = new DirectoryInfo(startDir);
            while (curPath != null)
            {
                string[] csprojPath = Directory.GetFiles(curPath.FullName, "*." + ext, SearchOption.TopDirectoryOnly);
                if (csprojPath.Length > 0) return csprojPath[0];
                curPath = curPath.Parent;
            }

            return string.Empty;
        }

        #endregion

        private static DSLDir instance;
        public static DSLDir Instance
        {
            get
            {
                if (instance == null) throw new Exception("Call an inialization method first!");
                return instance;
            }
        }

        #region Singleton members

        private string solutionFilePath;
        private List<string> projectFiles;
        int currentProjectIndex;

        #region Constructors

        private DSLDir()
        {
            currentProjectIndex = 0;
            projectFiles = new List<string>();
        }

        /// <summary>
        /// Initialize directory informations for a single project.
        /// </summary>
        /// <param name="projectExtension"></param>
        public static void Initialize(string projectExtension)
        {
            instance = new DSLDir();
            string curDir = GetCurrentDir();
            instance.solutionFilePath = GetFirstFileMatch(curDir, projectExtension);
            instance.projectFiles.Add(instance.solutionFilePath);
        }

        /// <summary>
        /// Initialize directory informations for a solution of projects.
        /// The order of projects can be specified by adding a "&lt;project_file_name&gt;.&lt;priority&gt;.order" file in the same folder of any project file:
        /// Default priority is 0, higher priorities compile before lower ones.
        /// </summary>
        /// <param name="projectExtension"></param>
        /// <param name="solutionExtension"></param>
        public static void Initialize(string projectExtension, string solutionExtension)
        {
            Initialize(projectExtension, solutionExtension, GetCurrentDir());
        }

        /// <summary>
        /// Initialize directory informations for a solution of projects.
        /// The order of projects can be specified by adding a "&lt;project_file_name&gt;.&lt;priority&gt;.order" file in the same folder of any project file:
        /// Default priority is 0, higher priorities compile before lower ones.
        /// </summary>
        /// <param name="projectExtension"></param>
        /// <param name="solutionExtension"></param>
        /// <param name="basePath">A directory that will be used to search for project files. 
        /// This directiory should locate the solution file or any of the solution subdirectories. </param>
        public static void Initialize(string projectExtension, string solutionExtension, string baseDirectiory)
        {
            instance = new DSLDir();

            // search for projects
            instance.solutionFilePath = GetFirstFileMatch(baseDirectiory, solutionExtension);
            string[] files = Directory.GetFiles(Path.GetDirectoryName(instance.solutionFilePath), "*." + projectExtension, SearchOption.AllDirectories);

            // search for priority files
            int[] priorities = new int[files.Length];
            for(int i = 0; i < files.Length; i++)
            {
                string projDir = Path.GetDirectoryName(files[i]);
                string projName = Path.GetFileNameWithoutExtension(files[i]);
                string[] prioFileList = Directory.GetFiles(projDir, projName + ".*" + ORDER_FILE_EXT, SearchOption.TopDirectoryOnly);
                if (prioFileList.Length > 0)
                {
                    // priority file exists, assign priority to this project
                    string prioFileName = Path.GetFileName(prioFileList[0]);
                    priorities[i] = -int.Parse(prioFileName.Substring(projName.Length + 1, prioFileName.Length - ORDER_FILE_EXT.Length - projName.Length - 1));
                }

            }

            // order project by priority
            Array.Sort<int, string>(priorities, files);

            // add projects to the current dir state
            instance.projectFiles.AddRange(files);
        }

        #endregion

        public int ProjectCount
        {
            get
            {
                return this.projectFiles.Count;
            }
        }

        public string ProjectRoot
        {
            get
            {
                return Path.GetDirectoryName(this.projectFiles[currentProjectIndex]);
            }
        }

        public string ProjectFile
        {
            get
            {
                return this.projectFiles[currentProjectIndex];
            }
        }

        public string SolutionRoot
        {
            get
            {
                return Path.GetDirectoryName(this.solutionFilePath);
            }
        }

        public string SolutionFile
        {
            get
            {
                return this.solutionFilePath;
            }
        }

        public string GetRootFromProjectName(string name)
        {
            for (int i = 0; i < ProjectCount; i++)
            {
                if (Path.GetFileNameWithoutExtension(this.projectFiles[i]) == name)
                {
                    return Path.GetDirectoryName(this.projectFiles[i]);
                }
            }

            return null;
        }

        public void SetProjectIndex(int index)
        {         
            string projectPath = this.projectFiles[index];// useless, but will throw an exception here if index is invalid.
            this.currentProjectIndex = index;
        }

        #endregion
    }
}
