using System;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Shared.Icon
{
    public class GameIconFinder : IIconFinder
    {
        public string? FindIcon(IPlayableObject playableObject)
        {
            if (playableObject is IMod)
                throw new NotSupportedException("Mods are not supported by this instance.");
            throw new NotImplementedException();
        }
    }
}