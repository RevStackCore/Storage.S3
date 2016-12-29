using System;
using RevStackCore.Storage.Model;

namespace RevStackCore.Storage.S3
{
    public class Folder : IFolder
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public string Url { get; set; }
    }
}
