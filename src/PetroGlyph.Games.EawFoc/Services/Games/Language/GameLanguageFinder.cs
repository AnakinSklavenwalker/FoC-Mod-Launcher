using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public class GameLanguageFinder : IGameLanguageFinder
    {
        private readonly ILanguageFinderHelper _helper;

        public GameLanguageFinder(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _helper = serviceProvider.GetService<ILanguageFinderHelper>() ?? new LanguageFinderHelper();
        }

        public ISet<ILanguageInfo> FindInstalledLanguages(IGame game)
        {
            var text = _helper.GetTextLocalizations(game);
            var speech = _helper.GetSpeechLocalizationsFromMegs(game);
            var sfx = _helper.GetSfxMegLocalizations(game);
            return _helper.Merge(text, speech, sfx);
        }
    }
}