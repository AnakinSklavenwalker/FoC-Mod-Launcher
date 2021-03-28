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

        internal static string TrimTrailingSeparators(string s)
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
    }
}
