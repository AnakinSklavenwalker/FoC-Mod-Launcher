using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public class CompositeLanguageFinder : IInstalledLanguageFinder
    {
        public static readonly IInstalledLanguageFinder GameLanguageFinder = new GameLanguageFinder();
        public static readonly IInstalledLanguageFinder ModLanguageFinder = new ModLanguageFinder();

        public ICollection<ILanguageInfo> FindInstalledLanguages(IPlayableObject playableObject)
        {
            return playableObject switch
            {
                IGame => GameLanguageFinder.FindInstalledLanguages(playableObject),
                IMod => ModLanguageFinder.FindInstalledLanguages(playableObject),
                _ => throw new NotSupportedException($"{playableObject.GetType().Name} is not supported.")
            };
        }
    }
}