using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DSLManager.Generation
{
    public struct BinFile
    {
        public byte[] Content;
        public string FileName;
        public CodeFileType FileType;

        public BinFile(byte[] content, string fileName, CodeFileType fileType)
        {
            this.Content = content;
            this.FileName = fileName;
            this.FileType = fileType;
        }

        public BinFile(string path, CodeFileType fileType)
        {
            this.Content = File.ReadAllBytes(path);
            this.FileName = Path.GetFileName(path);
            this.FileType = fileType;
        }


    }
}
