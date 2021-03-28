using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public interface IInstalledLanguageFinder
    {
        ICollection<ILanguageInfo> FindInstalledLanguages(IPlayableObject playableObject);
    }
}
