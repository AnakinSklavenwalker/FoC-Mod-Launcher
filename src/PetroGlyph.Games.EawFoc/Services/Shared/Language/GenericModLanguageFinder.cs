using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Shared.Language
{
    public class GenericModLanguageFinder : LanguageFinderBase
    {
        public GenericModLanguageFinder(DirectoryInfo baseDirectory) : base(baseDirectory)
        {
        }

        protected override IEnumerable<ILanguageInfo> FindTextLanguages()
        {
            var textPath = Path.Combine(BaseDirectory.FullName, "Data\\Text");
            return LanguageFinderUtilities.GetTextFileLanguages(textPath);
        }

        protected override IEnumerable<ILanguageInfo> FindSpeechLanguages()
        {
            return Enumerable.Empty<ILanguageInfo>();
        }

        protected override IEnumerable<ILanguageInfo> FindSfxLanguages()
        {
            return Enumerable.Empty<ILanguageInfo>();
        }
    }
}