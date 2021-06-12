using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Validation;

namespace PetroGlyph.Games.EawFoc.Utilities
{
    // Based https://github.com/dotnet/roslyn/blob/main/src/Compilers/Core/Portable/FileSystem/PathUtilities.cs
    internal static class PathUtilities
    {
        internal const char VolumeSeparatorChar = ':';
        internal static readonly char[] Slashes = { '/', '\\' };
        internal static bool IsUnixLikePlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        internal static string NormalizePath(this IPath instance, string path, bool resolveFullPath = true, bool trimTrailingSeparator = true)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            
            if (resolveFullPath)
                path = instance.GetFullPath(path);

            return NormalizePath(path, trimTrailingSeparator);
        }

        private static string NormalizePath(string path, bool trimTrailingSeparator = true)
        {
            var slashNormalized = NormalizeSlashes(path);
            return trimTrailingSeparator ? TrimTrailingSeparators(slashNormalized) : slashNormalized;
        }

        internal static bool IsChildOf(string basePath, string candidate)
        {
            Requires.NotNull(basePath, nameof(basePath));
            Requires.NotNull(candidate, nameof(candidate));

            basePath = NormalizePath(basePath);
            candidate = NormalizePath(candidate);

            var comparison = IsUnixLikePlatform
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            return candidate.StartsWith(basePath, comparison);


        }
        
        internal static bool IsAbsolute(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            
            if (IsUnixLikePlatform)
                return path[0] == '/';
            
            if (IsDriveRootedAbsolutePath(path))
                return true;
            return path.Length >= 2 &&
                   IsDirectorySeparator(path[0]) &&
                   IsDirectorySeparator(path[1]);
        }

        internal static bool IsValidDirectoryPath(this IFileSystem fs, string path)
        {
            Requires.NotNull(fs, nameof(fs));
            if (string.IsNullOrEmpty(path))
                return false;
            try
            {
                var name = fs.DirectoryInfo.FromDirectoryName(path).Name;
                return !string.IsNullOrEmpty(name);
            }
            catch (Exception e) when(e is ArgumentException or PathTooLongException or NotSupportedException)
            {
                return false;
            }
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

        public static string NormalizeSlashes(string p)
        {
            return IsUnixLikePlatform ? p : p.Replace('\\', '/');
        }

        private static bool IsDirectorySeparator(char c)
        {
            return Array.IndexOf(Slashes, c) >= 0;
        }

        private static bool IsDriveRootedAbsolutePath(string path)
        {
            return path.Length >= 3 && path[1] == VolumeSeparatorChar && IsDirectorySeparator(path[2]);
        }

    }
}
