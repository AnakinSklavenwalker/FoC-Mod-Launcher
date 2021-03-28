using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Shared.Language
{
    public class ModLanguageFinder : IInstalledLanguageFinder
    {
        public ICollection<ILanguageInfo> FindInstalledLanguages(IPlayableObject playableObject)
        {
            if (playableObject is IGame)
                throw new NotSupportedException("Games are not supported by this instance.");
            throw new NotImplementedException();
        }
    }
}