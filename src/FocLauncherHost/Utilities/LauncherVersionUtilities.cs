using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using Validation;

namespace FocLauncherHost.Utilities
{
    internal static class LauncherVersionUtilities
    {
        public static Version? GetFileVersionSafe(IFileInfo file)
        {
            try
            {
                return GetFileVersion(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Version GetFileVersion(IFileInfo file)
        {
            Requires.NotNull(file, nameof(file));
            if (file.FullName is null)
                throw new IOException("Cannot get path from file");
            var existingVersionString = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;
            return Version.Parse(existingVersionString);
        }
    }
}