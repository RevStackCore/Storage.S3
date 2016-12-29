using System;
using RevStackCore.Storage.Model;

namespace RevStackCore.Storage.S3
{
    public class File : IFile
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string Url { get; set; }
    }
}
