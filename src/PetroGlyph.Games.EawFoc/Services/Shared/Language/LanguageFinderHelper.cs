using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Language
{
    public sealed class LanguageFinderHelper : ILanguageFinderHelper
    {
        private static readonly Lazy<IDictionary<string, CultureInfo>> CultureFactory = new(
            () =>
            {
                return CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures)
                    .ToDictionary(info => info.EnglishName.ToLowerInvariant(), info => info);
            });
        private static readonly IDictionary<string, CultureInfo> AllCultures = CultureFactory.Value;

        public IEnumerable<ILanguageInfo> GetTextLocalizations(IPhysicalPlayableObject playableObject)
        {
            return GetLanguageFromFiles(
                () => playableObject.DataFiles("MasterTextFile_*.dat", "Text"), 
                GetTextLangName, LanguageSupportLevel.Text);

            string GetTextLangName(string textFileName)
            {
                textFileName = playableObject.Directory.FileSystem.Path.GetFileNameWithoutExtension(textFileName);
                const string cutOffPattern = "MasterTextFile_";
                return textFileName.Substring(cutOffPattern.Length);
            }
        }

        public IEnumerable<ILanguageInfo> GetSfxMegLocalizations(IPhysicalPlayableObject playableObject)
        {
            return GetLanguageFromFiles(
                () => playableObject.DataFiles("sfx2d_*.meg", "Audio/SFX"),
                GetSfxLangName, LanguageSupportLevel.SFX);

            static string? GetSfxLangName(string fileName)
            {
                if (fileName.Equals("sfx2d_non_localized", StringComparison.OrdinalIgnoreCase))
                    return null;
                var cutOffIndex = fileName.LastIndexOf('_') + 1;
                if (cutOffIndex <= 0 || cutOffIndex  == fileName.Length)
                    return null;
                return fileName.Substring(cutOffIndex);
            }
        }

        public IEnumerable<ILanguageInfo> GetSpeechLocalizationsFromMegs(IPhysicalPlayableObject playableObject)
        {
            // TODO: When merged into PG repo, try to get real path from megafiles.xml
            return GetLanguageFromFiles(
                () => playableObject.DataFiles("*speech.meg"), 
                GetSpeechLangName, LanguageSupportLevel.Speech);
            
            static string GetSpeechLangName(string megFileName)
            {
                var cutOffIndex = megFileName.IndexOf("speech.meg", StringComparison.OrdinalIgnoreCase);
                if (cutOffIndex < 0)
                    throw new InvalidOperationException($"unable to get language name from {megFileName}");
                return megFileName.Substring(0, cutOffIndex);
            }
        }
        
        public IEnumerable<ILanguageInfo> GetSpeechLocalizationsFromFolder(IPhysicalPlayableObject playableObject)
        {
            var speechDir = playableObject.DataDirectory("Audio/Speech");
            if (!speechDir.Exists)
                return new HashSet<ILanguageInfo>();

            var langFolders = speechDir.EnumerateDirectories();

            var result = new HashSet<ILanguageInfo>();
            foreach (var folder in langFolders)
            {
                try
                {
                    result.Add(LanguageNameToLanguageInfo(folder.Name, LanguageSupportLevel.Speech));
                }
                catch (CultureNotFoundException)
                {
                }
            }
            return result;
        }

        public static ILanguageInfo LanguageNameToLanguageInfo(string languageName, LanguageSupportLevel supportLevel)
        {
            languageName = languageName.ToLowerInvariant();
            if (!AllCultures.TryGetValue(languageName, out var culture))
                throw new CultureNotFoundException($"Unable to get culture for language {languageName}");
            return new LanguageInfo(culture.TwoLetterISOLanguageName, supportLevel);
        }

        public ISet<ILanguageInfo> Merge(params IEnumerable<ILanguageInfo>[] setsToMerge)
        {
            var store = new Dictionary<string, LanguageSupportLevel>();
            
            foreach (var languageInfo in setsToMerge.SelectMany(x => x))
            {
                if (store.ContainsKey(languageInfo.Code))
                    store[languageInfo.Code] |= languageInfo.Support;
                else
                    store.Add(languageInfo.Code, languageInfo.Support);
            }
            
            return new HashSet<ILanguageInfo>(store.Select(pair =>
                (ILanguageInfo) new LanguageInfo(pair.Key, pair.Value)));

        }

        private static IEnumerable<ILanguageInfo> GetLanguageFromFiles(
            Func<IEnumerable<IFileInfo>> fileEnumerator, 
            Func<string, string?> languageNameFactory, LanguageSupportLevel supportLevel)
        {
            var files = fileEnumerator().ToList();
            var result = new HashSet<ILanguageInfo>();
            foreach (var file in files)
            {
                var languageName = languageNameFactory(file.Name);
                if (languageName == null)
                    continue;
                result.Add(LanguageNameToLanguageInfo(languageName, supportLevel));
            }
            return result;
        }
    }


}