using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    internal interface IGamePlatformValidator
    {
        bool IsPlatform(GameType type, ref IDirectoryInfo location);
    }
}