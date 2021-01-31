using System;
using System.IO.Abstractions;

namespace CommonUtilities
{
    internal static class PathExtensions
    {
        private const char DirectorySeparatorChar = '\\';

        public static bool ContainsPath(this IPath instance, string fullPath, string path)
        {
            return instance.ContainsPath(fullPath, path, false);
        }

        public static bool ContainsPath(this IPath instance, string fullPath, string path, bool excludeSame)
        {
            try
            {
                if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(path))
                    return false;
                fullPath = instance.GetFullPath(fullPath);
                path = instance.GetFullPath(path);
                fullPath = AddBackslashIfNotPresent(fullPath);
                path = AddBackslashIfNotPresent(path);
                var flag = fullPath.StartsWith(path, StringComparison.OrdinalIgnoreCase);
                return flag & excludeSame ? !fullPath.Equals(path, StringComparison.OrdinalIgnoreCase) : flag;
            }
            catch
            {
                return false;
            }
        }

        private static string AddBackslashIfNotPresent(string path)
        {
            if (!string.IsNullOrEmpty(path) && path[path.Length - 1] != DirectorySeparatorChar)
                path += DirectorySeparatorChar.ToString();
            return path;
        }
    }
}