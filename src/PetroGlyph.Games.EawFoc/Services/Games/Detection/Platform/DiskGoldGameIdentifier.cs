using System;
using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    internal class DiskGoldIdentifier : SpecificPlatformIdentifier
    {
        private static readonly string[] FocKnownGoldFiles = {
            "fpupdate.exe",
            "LaunchEAWX.exe",
            "main.wav"
        };

        private static readonly string[] FocKnownGoldFolders = {
            "Install",
            "Manuals"
        };


        private static readonly string[] EawKnownGoldFiles = {
            "fpupdate.exe",
            "MCELaunch.exe",
            "StubUpdate.exe"
        };
        private static readonly string[] EawKnownGoldParentFiles = {
            "LaunchEAW.exe",
            "main.wav"
        };

        public DiskGoldIdentifier(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        public override bool IsPlatformFoc(ref IDirectoryInfo location)
        {
            if (!GameDetector.GameExeExists(location, GameType.Foc))
                return false;

            return DirectoryContainsFiles(location, FocKnownGoldFiles) &&
                   DirectoryContainsFolders(location, FocKnownGoldFolders);
        }

        public override bool IsPlatformEaw(ref IDirectoryInfo location)
        {
            if (!GameDetector.GameExeExists(location, GameType.EaW))
                return false;

            if (!location.Name.Equals("GameData", StringComparison.InvariantCultureIgnoreCase))
                return false;

            var parent = location.Parent;
            if (parent is null)
                return false;

            return DirectoryContainsFiles(location, EawKnownGoldFiles) &&
                   DirectoryContainsFolders(parent, EawKnownGoldParentFiles);
        }
    }
}