using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;

namespace PetroGlyph.Games.EawFoc.Games
{
    internal static class GameStartHelper
    {
        public static Process StartGameProcess(ProcessStartInfo startInfo, string? iconPath)
        {
            if (startInfo == null)
                throw new ArgumentNullException(nameof(startInfo), "Game startup info must not be null");

            var fileName = startInfo.FileName;
            var a = startInfo.Arguments;

            var linkPath = Path.Combine(LauncherConstants.ApplicationBasePath, "tmp.lnk");

            CreateShortcut(fileName, linkPath, a, startInfo.WorkingDirectory, iconPath);

            var startingProcess = new Process
            {
                StartInfo = { FileName = linkPath }
            };
            startingProcess.Start();
            return startingProcess;
        }

        private static void CreateShortcut(string filePath, string linkPath, string arguments, string wd, string? iconPath)
        {
            var link = (NativeMethods.NativeMethods.IShellLink)new NativeMethods.NativeMethods.ShellLink();
            link.SetPath(filePath);
            link.SetWorkingDirectory(wd);
            link.SetArguments(arguments);

            if (iconPath == null || !File.Exists(iconPath))
                iconPath = LauncherApp.FocIconPath;

            link.SetIconLocation(iconPath, 0);
            var file = (IPersistFile)link;
            file.Save(linkPath, false);
        }
    }
}
