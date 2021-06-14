using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public interface ILanguageFinderHelper
    {
        IEnumerable<ILanguageInfo> GetTextLocalizations(IPhysicalPlayableObject playableObject);

        IEnumerable<ILanguageInfo> GetSfxMegLocalizations(IPhysicalPlayableObject playableObject);

        IEnumerable<ILanguageInfo> GetSpeechLocalizationsFromFolder(IPhysicalPlayableObject playableObject);

        IEnumerable<ILanguageInfo> GetSpeechLocalizationsFromMegs(IPhysicalPlayableObject playableObject);

        ISet<ILanguageInfo> Merge(params IEnumerable<ILanguageInfo>[] setsToMerge);
    }
}