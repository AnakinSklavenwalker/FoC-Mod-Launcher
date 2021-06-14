using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public class PhysicalModLanguageFinderBase : ModLanguageFinderBase
    {
        public PhysicalModLanguageFinderBase(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected override ISet<ILanguageInfo> FindInstalledLanguagesCore(IMod mod)
        {
            if (mod is not IPhysicalMod physicalMod)
                throw new NotSupportedException("Non physical mod is not supported by this instance.");
            var text = Helper.GetTextLocalizations(physicalMod);
            var speechMeg = Helper.GetSpeechLocalizationsFromMegs(physicalMod);
            var speechFolder = Helper.GetSpeechLocalizationsFromFolder(physicalMod);
            var sfx = Helper.GetSfxMegLocalizations(physicalMod);
            return Helper.Merge(text, speechFolder, speechMeg, sfx);
        }
    }
}