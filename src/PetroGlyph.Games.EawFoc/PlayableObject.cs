using System;
using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc
{
    public abstract class PlayableObject : IPlayableObject
    {
        private ISet<ILanguageInfo>? _installedLanguages;
        private string? _iconFile;

        private bool _languageSearched;
        private bool _iconSearched;

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public ISet<ILanguageInfo> InstalledLanguages
        {
            get
            {
                if (_languageSearched)
                    return _installedLanguages!;
                if (_installedLanguages is not null)
                    return _installedLanguages;
                _installedLanguages = ResolveInstalledLanguages();
                if (_installedLanguages is null)
                    throw new NullReferenceException("Resolved languages must not be null.");
                _languageSearched = true;
                return _installedLanguages;
            }
        }

        /// <inheritdoc/>
        public string? IconFile
        {
            get
            {
                if (_iconSearched)
                    return _iconFile;
                if (_iconFile is not null)
                    return _iconFile;
                _iconFile = ResolveIconFile();
                _iconSearched = true;
                return _iconFile;
            }
        }

        protected virtual string? ResolveIconFile()
        {
            return null;
        }

        protected virtual ISet<ILanguageInfo> ResolveInstalledLanguages()
        {
            return new HashSet<ILanguageInfo>();
        }

        /// <summary>
        /// Resets the state of <see cref="IconFile"/> so it can be resolved again.
        /// </summary>
        /// <returns>Returns the old value.</returns>
        public virtual string? ResetIcon()
        {
            var oldIcon = _iconFile;
            _iconFile = null;
            _iconSearched = false;
            return oldIcon;
        }

        /// <summary>
        /// Resets the state of <see cref="InstalledLanguages"/> so it can be resolved again.
        /// </summary>
        /// <returns>Returns the old value.</returns>
        public virtual string? ResetLanguages()
        {
            var oldIcon = _iconFile;
            _installedLanguages = null;
            _languageSearched = false;
            return oldIcon;
        }
    }
}