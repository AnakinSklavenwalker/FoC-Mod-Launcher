using System;
using System.IO.Abstractions;
using System.Text;
using Validation;

namespace CommonUtilities
{
    public static class PathExtensions
    {
        private const char AltDirectorySeparatorChar = '/';
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

        public static string NormalizePath(string path, bool addBackslash = false)
        {
            Requires.NotNullOrEmpty(path, nameof(path));
            path = path.Replace(AltDirectorySeparatorChar, DirectorySeparatorChar);
            path = RemoveAdjacentChars(path, DirectorySeparatorChar, 1);
            return addBackslash ? AddBackslashIfNotPresent(path) : path;
        }

        private static string AddBackslashIfNotPresent(string path)
        {
            if (!string.IsNullOrEmpty(path) && path[path.Length - 1] != DirectorySeparatorChar)
                path += DirectorySeparatorChar.ToString();
            return path;
        }

        private static string RemoveAdjacentChars(string value, char ch, int startIndex)
        {
            if (startIndex >= value.Length)
                return value;
            StringBuilder stringBuilder = new(value);
            var lastChar = char.MinValue;
            for (var index = startIndex; index < stringBuilder.Length; ++index)
            {
                var currentChar = stringBuilder[index];
                var currentIsAdjacent = ch == currentChar && currentChar == lastChar;
                lastChar = currentChar;
                if (currentIsAdjacent)
                {
                    stringBuilder.Remove(index, 1);
                    --index;
                }
            }
            return stringBuilder.ToString();
        }
    }
}