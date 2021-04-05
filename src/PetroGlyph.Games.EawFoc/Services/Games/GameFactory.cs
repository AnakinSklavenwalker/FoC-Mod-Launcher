using System;
using System.IO;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Games
{
    public class GameFactory
    {
        public static IGame CreateFocGame(DirectoryInfo directory, GamePlatform platform)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            switch (platform)
            {
                case GamePlatform.SteamGold:
                    return new SteamGameFoc(directory);
                case GamePlatform.Disk:
                case GamePlatform.Origin:
                case GamePlatform.GoG:
                case GamePlatform.DiskGold:
                    return new Foc(directory, platform);
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform));
            }
        }

        public static IGame CreateEawGame(DirectoryInfo directory, GamePlatform platform)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            switch (platform)
            {
                case GamePlatform.SteamGold:
                    return new SteamGameEaw(directory);
                case GamePlatform.Disk:
                case GamePlatform.Origin:
                case GamePlatform.GoG:
                case GamePlatform.DiskGold:
                    return new Eaw(directory, platform);
                default:
                    throw new ArgumentOutOfRangeException(nameof(platform));
            }
        }
    }
}