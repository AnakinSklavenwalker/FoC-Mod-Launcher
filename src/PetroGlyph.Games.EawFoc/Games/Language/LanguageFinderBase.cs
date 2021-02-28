using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Games.Language
{
    public abstract class LanguageFinderBase
    {
        private readonly IDictionary<string, LanguageInfo> _store = new Dictionary<string, LanguageInfo>();
        private bool _searched;
        private readonly object _syncObject = new object();

        public DirectoryInfo BaseDirectory { get; }

        public LanguageFinderBase(DirectoryInfo baseDirectory)
        {
            Requires.NotNull(baseDirectory, nameof(baseDirectory));
            if (!baseDirectory.Exists)
                throw new ArgumentException(nameof(baseDirectory), new DirectoryNotFoundException());
            BaseDirectory = baseDirectory;
        }

        public ICollection<ILanguageInfo> Find()
        {
            lock (_syncObject)
            {
                if (!_searched)
                {
                    FindLanguagesCore();
                    _searched = true;
                }
            }
            return _store.Values.OfType<ILanguageInfo>().ToList();
        }

        protected abstract IEnumerable<ILanguageInfo> FindTextLanguages();

        protected abstract IEnumerable<ILanguageInfo> FindSpeechLanguages();

        protected abstract IEnumerable<ILanguageInfo> FindSfxLanguages();

        protected virtual void FindLanguagesCore()
        {
            var textLangs = FindTextLanguages();
            AddToStore(textLangs);
            var speechLangs = FindSpeechLanguages();
            AddToStore(speechLangs);
            var sfxLangs = FindSfxLanguages();
            AddToStore(sfxLangs);
        }
        
        protected void AddToStore(IEnumerable<ILanguageInfo> newItems)
        {
            foreach (var languageInfo in newItems)
            {
                if (_store.ContainsKey(languageInfo.Code))
                    _store[languageInfo.Code].Support |= languageInfo.Support;
                else
                    _store.Add(languageInfo.Code, new LanguageInfo(languageInfo.Code) { Support = languageInfo.Support });
            }
        }
    }
}
