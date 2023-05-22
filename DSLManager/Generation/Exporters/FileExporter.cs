using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Generation.Exporters
{
    public class FileExporter : IProjectExporter
    {
        public string Target
        {
            get; set;
        }
        
        public string OutputDir {get; set; }

        public void BeginExport() {  }

        public void EndExport() { }

        public void ExportProject(CodeProject cp)
        {
            string outputDir = OutputDir.TrimEnd('\\', '/');
            Directory.CreateDirectory(outputDir);
            outputDir = outputDir + Path.DirectorySeparatorChar;
            
            //write source files
            if (cp.ContainsSources)
            {
                foreach (CodeFile codeFile in cp.Sources)
                {
                    (new FileInfo(outputDir + codeFile.FileName)).Directory.Create();
                    File.WriteAllText(outputDir + codeFile.FileName, codeFile.SourceCode);
                }
            }
            
            //write binaries
            if (cp.ContainsBinaries)
            {
                foreach (BinFile binFile in cp.Binaries)
                {
                    (new FileInfo(outputDir + binFile.FileName)).Directory.Create();
                    File.WriteAllBytes(outputDir + binFile.FileName, binFile.Content);
                }
            }
        }
    }
}
