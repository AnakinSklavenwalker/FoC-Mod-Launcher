using System;
using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    internal class GogIdentifier : SpecificPlatformIdentifier
    {
        private static readonly string[] KnownGogFiles = {
            "Language.exe",
            "goggame.sdb",
            "goggame-1421404887.hashdb",
            "goggame-1421404887.info",
            "Language.exe"
        };

        public GogIdentifier(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool IsPlatformFoc(ref IDirectoryInfo location)
        {
            if (!GameDetector.GameExeExists(location, GameType.Foc))
                return false;

            if (!location.Name.Equals("EAWX", StringComparison.InvariantCultureIgnoreCase))
                return false;

            return ParentContainsSteamFiles(location);
        }

        public override bool IsPlatformEaw(ref IDirectoryInfo location)
        {
            if (!GameDetector.GameExeExists(location, GameType.EaW))
                return false;

            if (!location.Name.Equals("GameData", StringComparison.InvariantCultureIgnoreCase))
                return false;

            const string gogGameDll = "goggame-1421404887.dll";
            return DirectoryContainsFiles(location, new[] {gogGameDll}) && ParentContainsSteamFiles(location);
        }

        private static bool ParentContainsSteamFiles(IDirectoryInfo gameLocation)
        {
            var parentDir = gameLocation.Parent;
            return parentDir is not null && DirectoryContainsFiles(parentDir, KnownGogFiles);
        }
    }
}