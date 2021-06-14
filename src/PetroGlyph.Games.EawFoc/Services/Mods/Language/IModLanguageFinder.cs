using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public interface IModLanguageFinder
    {
        ISet<ILanguageInfo> FindInstalledLanguages(IMod mod);
    }
}