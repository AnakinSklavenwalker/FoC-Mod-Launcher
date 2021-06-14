using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
#if NET
using System.Linq;
#else
using System;
#endif

namespace PetroGlyph.Games.EawFoc
{
    public static class PlayableObjectExtensions
    {
        public static IDirectoryInfo DataDirectory(this IPhysicalPlayableObject playableObject)
        {
            return playableObject.DataDirectory(string.Empty, true);
        }

        public static IDirectoryInfo DataDirectory(this IPhysicalPlayableObject playableObject, string? subPath, bool checkExists = false)
        {
            var objectPath = playableObject.Directory;
            var fs = playableObject.Directory.FileSystem;
            IDirectoryInfo requestedDirectory;
            subPath ??= string.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var dataPath = fs.Path.Combine(objectPath.FullName, "Data", subPath);
                requestedDirectory = fs.DirectoryInfo.FromDirectoryName(dataPath);
            }
            else
            {
#if NET
                var searchPattern = fs.Path.Combine("data", subPath);
                requestedDirectory = objectPath.EnumerateDirectories(searchPattern,
                    new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive }).First();
#else
                throw new NotImplementedException();
#endif
            }
            if (checkExists && !requestedDirectory.Exists)
                throw new DirectoryNotFoundException($"Unable to find 'Data' directory of {playableObject}");
            return requestedDirectory;
        }

        public static IEnumerable<IFileInfo> DataFiles(this IPhysicalPlayableObject playableObject, string fileSearchPattern, string? subPath = null, bool checkExists = false)
        {
            var searchLocation = playableObject.DataDirectory(subPath, checkExists);
            if (!searchLocation.Exists)
                return new List<IFileInfo>();
#if NET
            return searchLocation.EnumerateFiles(fileSearchPattern, new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive });
#else
            return searchLocation.EnumerateFiles(fileSearchPattern);
#endif
        }
    }
}