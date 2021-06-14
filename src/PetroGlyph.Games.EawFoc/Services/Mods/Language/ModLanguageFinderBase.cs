using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public abstract class ModLanguageFinderBase : IModLanguageFinder
    {
        protected readonly IServiceProvider ServiceProvider;
        protected readonly ILanguageFinderHelper Helper;

        protected static ISet<ILanguageInfo> DefaultLanguageCollection =
            new HashSet<ILanguageInfo> {LanguageInfo.Default};

        protected ModLanguageFinderBase(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            ServiceProvider = serviceProvider;
            Helper = serviceProvider.GetService<ILanguageFinderHelper>() ?? new LanguageFinderHelper(serviceProvider);
        }

        public virtual ISet<ILanguageInfo> FindInstalledLanguages(IMod mod)
        {
            // We don't "trust" the modinfo here as the default language (EN) also gets
            // applied when nothing was specified by the mod developer.
            // Only if we have more than the default language, we trust the modinfo.
            var modinfo = mod.ModInfo;
            if (modinfo is not null && !IsEmptyOrDefault(modinfo.Languages))
                return modinfo.Languages.ToHashSet();


            var foundLanguages = FindInstalledLanguagesCore(mod);
            if (!IsEmptyOrDefault(foundLanguages))
                return foundLanguages;

            var inheritedLanguages = GetInheritedLanguages(mod);
            return !inheritedLanguages.Any() ? DefaultLanguageCollection : inheritedLanguages;
        }

        protected static bool IsEmptyOrDefault(IEnumerable<ILanguageInfo> languages)
        {
            var languageInfos = languages.ToList();
            return !languageInfos.Any() || languageInfos.All(x => x.Equals(LanguageInfo.Default));
        }

        // This implementation is not greedy.
        protected virtual ISet<ILanguageInfo> GetInheritedLanguages(IMod mod)
        {
            foreach (var dependency in mod.Dependencies.OfType<IMod>())
            {
                var dependencyLanguages = dependency.InstalledLanguages;
                if (!IsEmptyOrDefault(dependencyLanguages))
                    return dependencyLanguages;
            }
            return new HashSet<ILanguageInfo>();
        }
        
        protected abstract ISet<ILanguageInfo> FindInstalledLanguagesCore(IMod mod);
    }
}