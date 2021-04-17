using System;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Games.Icon
{
    public class GameIconFinder : IGameIconFinder
    {
        public string? FindIcon(IPlayableObject playableObject)
        {
            if (playableObject is IMod)
                throw new NotSupportedException("Mods are not supported by this instance.");
            throw new NotImplementedException();
        }
    }
}