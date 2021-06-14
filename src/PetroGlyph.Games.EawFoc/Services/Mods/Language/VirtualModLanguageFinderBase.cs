using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public class VirtualModLanguageFinderBase : ModLanguageFinderBase
    {
        public VirtualModLanguageFinderBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override ISet<ILanguageInfo> FindInstalledLanguagesCore(IMod mod)
        {
            if (mod.Type != ModType.Virtual)
                throw new NotSupportedException($"Mod type: {mod.Type} is not supported by this instance.");

            // Since virtual mods inherit their language from a (physical) dependency.
            // There is nothing to do here.
            // Simply returning the default collection. The base class will do the rest.
            return DefaultLanguageCollection;
        }
    }
}