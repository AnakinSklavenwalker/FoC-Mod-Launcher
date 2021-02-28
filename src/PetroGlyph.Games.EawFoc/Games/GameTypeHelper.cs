using System;
using System.IO;
using System.Linq;
using PetroGlyph.Games.EawFoc.Games.Detection;

namespace PetroGlyph.Games.EawFoc.Games
{
    public static class GameTypeHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // TODO: Test all checks again 
        internal static GameType GetGameType(GameDetection result)
        {
            if (result.IsError)
                return GameType.Undefined;
            if (CheckSteam(result.FocExe))
                return GameType.SteamGold;
            if (CheckGoG(result.FocExe))
                return GameType.GoG;
            Logger.Info("Checking CheckOrigin...");
            if (CheckOrigin(result, out var fixedFileInfo))
            {
                Logger.Info("End Checking CheckOrigin: Result TRUE");
                if (fixedFileInfo != null)
                {
                    Logger.Info($"Replacing result data to: {fixedFileInfo.FullName}");
                    result.FocExe = fixedFileInfo;
                }

                return GameType.Origin;
            }
            Logger.Info("End Checking CheckOrigin: Result FALSE");

            // TODO: Check DiskGold (just to have them all)
            return GameType.Disk;
        }


        public static GameType GetGameType(FileInfo focExe)
        {
            if (CheckSteam(focExe))
                return GameType.SteamGold;
            if (CheckGoG(focExe))
                return GameType.GoG;
            if (CheckOrigin(focExe)) 
                return GameType.Origin;
            // TODO: Check DiskGold (just to have them all)
            return GameType.Disk;
        }



        private static bool CheckSteam(FileInfo? exeFile)
        {
            if (exeFile is null || !exeFile.Exists)
                return false;

            var directory = exeFile.Directory;
            if (directory == null || !directory.Name.Equals("corruption", StringComparison.InvariantCultureIgnoreCase))
                return false;

            var gameData = directory.Parent?.GetDirectories().FirstOrDefault(x => x.Name.Equals("GameData"));
            if (gameData == null)
                return false;

            if (!gameData.GetFiles().Any(x => x.Name.Equals("sweaw.exe")))
                return false;

            var parentFiles = directory.Parent?.GetFiles() ?? new FileInfo[0];
            return parentFiles.Any(x => x.Name.Equals("runm2.dat")) && parentFiles.Any(x => x.Name.Equals("runme.dat"));
        }

        private static bool CheckGoG(FileInfo? exeFile)
        {
            if (exeFile is null || !exeFile.Exists)
                return false;
            if (exeFile.Directory?.Name != "EAWX")
                return false;
            var eawPath = exeFile.Directory.Parent?.EnumerateDirectories().FirstOrDefault(x => x.Name.Equals("GameData"));
            if (eawPath == null)
                return false;
            var fileNames = eawPath.EnumerateFiles().Select(x => x.Name).ToList();
            return fileNames.Any(x => x.Equals("sweaw.exe")) && fileNames.Any(x => x.Equals("goggame-1421404887.dll"));
        }

        private static bool CheckOrigin(GameDetection result, out FileInfo? fixedFileInfo)
        {
            fixedFileInfo = default;

            var focExe = result.FocExe;
            var exists = CheckOrigin(focExe);
            if (exists)
                return true;

            Logger.Info("Try to check again with with applied");
            var fixedFocExe = CreateOriginFileInfo(focExe);
            if (fixedFocExe is null)
                return false;

            var fixWorked = CheckOrigin(fixedFocExe);
            if (fixWorked)
            {
                fixedFileInfo = fixedFocExe;
                return true;
            }

            return false;
        }

        private static bool CheckOrigin(FileInfo? exeFile)
        {
            Logger.Info($"CheckOrigin on file: {exeFile}");
            var directory = exeFile?.Directory;
            if (directory is null)
            {
                Logger.Info("Directory is null");
                return false;
            }

            if (!exeFile.Exists || !directory.Exists)
            {
                Logger.Info("Exe file OR directory does not exists");
                return false;
            }

            if (!directory.EnumerateFiles().Any(x => x.Name.Equals("EALaunchHelper.exe")))
            {
                Logger.Info("Unable to find EALaunchHelper.exe");
                return false;
            }

            var parent = directory.Parent;
            if (parent is null)
            {
                Logger.Info("Parent is null");
                return false;
            }

            if (!parent.EnumerateDirectories().Any(x => x.Name.Equals("Manuals")) ||
                !parent.EnumerateDirectories().Any(x => x.Name.Equals("__Installer")))
            {
                Logger.Info("Unable to find directory 'Manual' OR '__Installer'");
                return false;
            }

            Logger.Info("Origin found");
            return true;
        }


        private static FileInfo? CreateOriginFileInfo(FileInfo? focExeFile)
        {
            if (focExeFile is null)
                throw new ArgumentNullException(nameof(focExeFile));

            var exeDir = focExeFile.Directory;
            if (exeDir is null)
            {
                Logger.Info("exeDir is null");
                return null;
            }

            if (exeDir.Name.Equals("EAWX"))
            {
                Logger.Info("Fix not required because directory already called 'EAWX'");
                return focExeFile;
            }

            if (exeDir.Name.Equals("corruption"))
            {
                Logger.Info("Changing direcotry name from 'corruption' to 'EAWX'");
                var parentPath = exeDir.Parent?.FullName;
                if (parentPath is null)
                {
                    Logger.Info("parentPath is null");
                    return null;
                }
                var correctedPath = Path.Combine(parentPath, "EAWX", focExeFile.Name);
                Logger.Info($"returning corrected new FileInfo({correctedPath})");
                return new FileInfo(correctedPath);
            }

            Logger.Info($"Returning null because no fix could be applied");
            return null;
        }
    }
}