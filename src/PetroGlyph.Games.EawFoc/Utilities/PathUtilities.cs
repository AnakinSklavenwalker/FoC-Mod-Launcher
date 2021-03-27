using System;
using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace PetroGlyph.Games.EawFoc.Utilities
{
    // Based https://github.com/dotnet/roslyn/blob/main/src/Compilers/Core/Portable/FileSystem/PathUtilities.cs
    internal static class PathUtilities
    {
        internal static readonly char[] Slashes = { '/', '\\' };
        internal static bool IsUnixLikePlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        internal static string NormalizePath(this IPath instance, string path, bool trimTrailingSeparator = true)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            var fullPath = instance.GetFullPath(path);
            var slashNormalized = NormalizeWithForwardSlash(fullPath);
            return trimTrailingSeparator ? TrimTrailingSeparators(slashNormalized) : slashNormalized;
        }

        public static string TrimTrailingSeparators(string s)
        {
            var lastSeparator = s.Length;
            while (lastSeparator > 0 && IsDirectorySeparator(s[lastSeparator - 1])) 
                lastSeparator -= 1;
            if (lastSeparator != s.Length) 
                s = s.Substring(0, lastSeparator);
            return s;
        }

        public static string NormalizeWithForwardSlash(string p)
        {
            return IsUnixLikePlatform ? p : p.Replace('\\', '/');
        }

        private static bool IsDirectorySeparator(char c)
        {
            return Array.IndexOf(Slashes, c) >= 0;
        }

        private static bool PathsEqual(string path1, string path2)
        {
            return PathsEqual(path1, path2, Math.Max(path1.Length, path2.Length));
        }

        private static bool PathsEqual(string path1, string path2, int length)
        {
            if (path1.Length < length || path2.Length < length)
            {
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                if (!PathCharEqual(path1[i], path2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool PathCharEqual(char x, char y)
        {
            if (IsDirectorySeparator(x) && IsDirectorySeparator(y))
            {
                return true;
            }

            return IsUnixLikePlatform
                ? x == y
                : char.ToUpperInvariant(x) == char.ToUpperInvariant(y);
        }
    }
}
