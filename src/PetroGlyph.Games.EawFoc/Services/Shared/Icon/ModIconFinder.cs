using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Shared.Icon
{
    public class ModIconFinder : IIconFinder
    {
        public string? FindIcon(IPlayableObject playableObject)
        {
            if (playableObject is IGame)
                throw new NotSupportedException("Games are not supported by this instance.");
            throw new NotImplementedException();
        }
    }
}