using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Games.Language
{
    public static class LanguageFinderUtilities
    {
        private static readonly IList<CultureInfo> AvailableCultures = CultureInfo.GetCultures(CultureTypes.AllCultures).ToList();

        public static IEnumerable<ILanguageInfo> GetTextFileLanguages(string directory)
        {
            return GetLanguages(directory, GetTextFileLanguageNames, LanguageSupportLevel.Text);
        }

        public static IEnumerable<ILanguageInfo> GetSpeechMegLanguages(string directory)
        {
            return GetLanguages(directory, GetSpeechMegsLanguageNames, LanguageSupportLevel.Speech);
        }

        public static IEnumerable<ILanguageInfo> GetSfxLanguages(string directory)
        {
            return GetLanguages(directory, GetSfxMegsLanguageNames, LanguageSupportLevel.SFX);
        }


        public static IEnumerable<ILanguageInfo> GetLanguages(string directory, Func<string, IEnumerable<string>> languageNameFinder, LanguageSupportLevel supportLevel)
        {
            var languages = languageNameFinder(directory).ToList();
            if (!languages.Any())
                yield break;

            foreach (var language in languages)
            {
                if (!TryGetLanguageInfoFromEnglishName(language, supportLevel, out var languageInfo))
                    continue;
                yield return languageInfo!;
            }
        }

        public static bool TryGetLanguageInfoFromEnglishName(string englishName, LanguageSupportLevel supportLevel, out ILanguageInfo? languageInfo)
        {
            var culture = AvailableCultures.FirstOrDefault(x =>
                x.EnglishName.Equals(englishName, StringComparison.InvariantCultureIgnoreCase));
            languageInfo = default;
            if (culture == null)
                return false;
            languageInfo = new LanguageInfo(culture.TwoLetterISOLanguageName) {Support = supportLevel};
            return true;
        }


        public static IEnumerable<string> GetLanguageNamesFromFile(string directory, string filePattern, Func<string, string> extractFunc)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory) || extractFunc is null)
                yield break;

            var files = Directory.EnumerateFiles(directory, filePattern).ToList();
            if (files.Count() < 0)
                yield break;

            foreach (var file in files)
            {
                var langName = extractFunc(Path.GetFileNameWithoutExtension(file));
                if (string.IsNullOrEmpty(langName))
                    continue;
                yield return langName;
            }
        }

        public static string ExtractLanguageNameFromMasterTextFile(string file)
        {
            var start = file.IndexOf('_');
            if (start < 0 || start + 1 == file.Length)
                return string.Empty;
            return file.Substring(start + 1);
        }

        public static string ExtractLanguageNameFromSpeechMeg(string file)
        {
            var end = file.IndexOf("speech", StringComparison.InvariantCultureIgnoreCase);
            return end < 0 ? string.Empty : file.Substring(0, end);
        }

        public static string ExtractLanguageNameFromSfxMeg(string file)
        {
            if (file.Equals("sfx2d_non_localized", StringComparison.InvariantCultureIgnoreCase))
                return string.Empty;
            var start = file.LastIndexOf('_');
            if (start < 0 || start + 1 == file.Length)
                return string.Empty;
            return file.Substring(start + 1);
        }

        internal static IEnumerable<string> GetTextFileLanguageNames(string directory)
        {
            return GetLanguageNamesFromFile(directory, "MasterTextFile_*.dat", ExtractLanguageNameFromMasterTextFile);
        }

        internal static IEnumerable<string> GetSpeechMegsLanguageNames(string directory)
        {
            return GetLanguageNamesFromFile(directory, "*speech.meg", ExtractLanguageNameFromSpeechMeg);
        }

        internal static IEnumerable<string> GetSfxMegsLanguageNames(string directory)
        {
            return GetLanguageNamesFromFile(directory, "sfx2d_*.meg", ExtractLanguageNameFromSfxMeg);
        }
    }
}