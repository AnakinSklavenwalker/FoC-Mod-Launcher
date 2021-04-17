using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Mods.Language
{
    public class ModLanguageFinder : IModLanguageFinder
    {
        public ICollection<ILanguageInfo> FindInstalledLanguages(IPlayableObject playableObject)
        {
            if (playableObject is IGame)
                throw new NotSupportedException("Games are not supported by this instance.");
            throw new NotImplementedException();
        }
    }
}