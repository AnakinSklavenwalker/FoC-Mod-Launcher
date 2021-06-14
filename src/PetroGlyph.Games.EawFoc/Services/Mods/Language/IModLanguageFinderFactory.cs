using System;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public interface IModLanguageFinderFactory
    {
        IModLanguageFinder CreateLanguageFinder(IMod mod, IServiceProvider serviceProvider);
    }
}