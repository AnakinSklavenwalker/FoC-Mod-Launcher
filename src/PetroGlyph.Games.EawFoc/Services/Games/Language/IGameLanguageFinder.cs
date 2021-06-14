using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public interface IGameLanguageFinder
    {
        ISet<ILanguageInfo> FindInstalledLanguages(IGame game);
    }
}