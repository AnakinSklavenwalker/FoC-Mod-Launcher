using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public class GameLanguageFinder : IInstalledLanguageFinder
    {
        public ICollection<ILanguageInfo> FindInstalledLanguages(IPlayableObject playableObject)
        {
            if (playableObject is IMod)
                throw new NotSupportedException("Mods are not supported by this instance.");
            throw new NotImplementedException();
        }
    }
}