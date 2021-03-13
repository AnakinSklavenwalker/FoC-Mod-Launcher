using System.Collections.Generic;
using EawModinfo.Model;

namespace PetroGlyph.Games.EawFoc.Services
{
    public interface IInstalledLanguageFinder
    {
        ICollection<LanguageInfo> FindInstalledLanguages(IPlayableObject playableObject);
    }
}
