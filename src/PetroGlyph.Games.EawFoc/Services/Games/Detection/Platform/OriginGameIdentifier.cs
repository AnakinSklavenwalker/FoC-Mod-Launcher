using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    internal class OriginIdentifier : SpecificPlatformIdentifier
    {
        private static readonly string[] KnownOriginDirs = {
            "Manuals",
            "__Installer"
        };

        public OriginIdentifier(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool IsPlatformFoc(ref IDirectoryInfo location)
        {
            if (!GameDetector.GameExeExists(location, GameType.Foc))
            {
                Logger?.LogWarning("Unable to find FoC Origin at first location. Trying to fix broken registry path");
                TryFixBrokenFocLocation(ref location);
                if (!GameDetector.GameExeExists(location, GameType.Foc))
                {
                    Logger.LogWarning("Origin location fix was unsuccessful. This is not a Origin installation.");
                    return false;
                }
            }

            if (!DirectoryContainsFiles(location, new[] {"EALaunchHelper.exe"}))
                return false;
            
            return ParentContainsOriginDirectories(location);
        }

        public override bool IsPlatformEaw(ref IDirectoryInfo location)
        {
            if (!GameDetector.GameExeExists(location, GameType.EaW))
            {
                Logger?.LogWarning("Unable to find EaW Origin at first location. " +
                                   "I don't know if the EAW path might be broken as well?!");
                return false;
            }

            // TODO: Do we have EALaunchHelper.exe here too?

            return location.Name.Equals("GameData", StringComparison.InvariantCultureIgnoreCase) &&
                   ParentContainsOriginDirectories(location);
        }

        private void TryFixBrokenFocLocation(ref IDirectoryInfo location)
        {
            if (location.Name.Equals("EAWX", StringComparison.InvariantCultureIgnoreCase))
                return;
            if (!location.Name.Equals("corruption", StringComparison.InvariantCultureIgnoreCase))
            {
                Logger?.LogDebug("Unable to apply Origin fix to a directory called other than 'corruption'.");
                return;
            }
            
            Logger.LogDebug("Changing direcotry name from 'corruption' to 'EAWX'");
            var parentDir = location.Parent;
            if (parentDir is null)
                return;

            var correctedPath = location.FileSystem.Path.Combine(parentDir.FullName, "EAWX");
            if (!location.FileSystem.File.Exists(correctedPath))
            {
                Logger.LogDebug($"Corrected path '{correctedPath}' does not exists.");
                return;
            }
            location = location.FileSystem.DirectoryInfo.FromDirectoryName(correctedPath);
        }

        private static bool ParentContainsOriginDirectories(IDirectoryInfo gameLocation)
        {
            var parentDir = gameLocation.Parent;
            return parentDir is not null && DirectoryContainsFolders(parentDir, KnownOriginDirs);
        }
    }
}