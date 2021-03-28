using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Shared.Language
{
    public interface IInstalledLanguageFinder
    {
        ICollection<ILanguageInfo> FindInstalledLanguages(IPlayableObject playableObject);
    }
}
