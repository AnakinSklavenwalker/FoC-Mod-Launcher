using System;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public class ModLanguageFinderFactory : IModLanguageFinderFactory
    {
        public IModLanguageFinder CreateLanguageFinder(IMod mod, IServiceProvider serviceProvider)
        {
            if (mod.Type == ModType.Virtual)
                return new VirtualModLanguageFinderBase(serviceProvider);
            return new PhysicalModLanguageFinderBase(serviceProvider);
        }
    }
}