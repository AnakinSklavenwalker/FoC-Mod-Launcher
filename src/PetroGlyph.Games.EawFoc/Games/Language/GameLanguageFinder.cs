using System.Collections.Generic;
using System.IO;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Games.Language
{
    public class GameLanguageFinder : LanguageFinderBase
    {
        public GameLanguageFinder(DirectoryInfo baseDirectory) : base(baseDirectory)
        {
        }

        protected override IEnumerable<ILanguageInfo> FindTextLanguages()
        {
            var textPath = Path.Combine(BaseDirectory.FullName, "Data\\Text");
            return LanguageFinderUtilities.GetTextFileLanguages(textPath);
        }

        protected override IEnumerable<ILanguageInfo> FindSpeechLanguages()
        {
            var speechPath = Path.Combine(BaseDirectory.FullName, "Data");
            return LanguageFinderUtilities.GetSpeechMegLanguages(speechPath);

        }

        protected override IEnumerable<ILanguageInfo> FindSfxLanguages()
        {
            var sfxPath = Path.Combine(BaseDirectory.FullName, "Data\\Audio\\SFX");
            return LanguageFinderUtilities.GetSfxLanguages(sfxPath);
        }
    }
}