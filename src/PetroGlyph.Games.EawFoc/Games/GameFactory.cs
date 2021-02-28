using System;
using System.IO;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class GameFactory
    {
        public static IGame CreateFocGame(DirectoryInfo directory, GameType type)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            switch (type)
            {
                case GameType.SteamGold:
                    return new SteamGameFoc(directory);
                case GameType.Disk:
                case GameType.Origin:
                case GameType.GoG:
                case GameType.DiskGold:
                    return new Foc(directory, type);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public static IGame CreateEawGame(DirectoryInfo directory, GameType type)
        {
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));
            switch (type)
            {
                case GameType.SteamGold:
                    return new SteamGameEaw(directory);
                case GameType.Disk:
                case GameType.Origin:
                case GameType.GoG:
                case GameType.DiskGold:
                    return new Eaw(directory, type);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}