using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSLManager.Generation
{
    public interface IProjectExporter
    {
        /// <summary>
        /// Get or set the target file for this exporter.
        /// Must be set to a project file for this exporter or to a dummy file that indicates the source directory.
        /// </summary>
        string Target { get; set; }

        void BeginExport();

        void ExportProject(CodeProject compiledCode);

        void EndExport();
    }
}
