using System;

namespace RevStackCore.Storage.S3
{
    internal static class Util
    {
        internal static string GetFilePath(string path, bool isFolder)
        {
            if (isFolder)
                path = OptimizePath(path);

            path = path.Replace("//", "/");

            if (path.StartsWith("/"))
                path = path.Substring(1, path.Length - 1);

            return path;
        }

        internal static string OptimizePath(string path)
        {
            if (!path.StartsWith("\"") && !path.StartsWith("/"))
                path = "/" + path;
            if (!path.EndsWith("\"") && !path.EndsWith("/"))
                path = path + "/";

            return path;
        }
    }
}
