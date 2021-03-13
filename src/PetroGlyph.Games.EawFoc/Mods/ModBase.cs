using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EawModinfo;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using NuGet.Versioning;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods
{
    /// <summary>
    /// Base implementation for Mods
    /// </summary>
    public abstract class ModBase : IMod
    {
        /// <inheritdoc/>
        public event EventHandler<ModCollectionChangedEventArgs>? ModCollectionModified;

        private string? _iconFile;
        private SemanticVersion? _modVersion;
        private IModinfo? _modInfo;
        private ICollection<ILanguageInfo>? _installedLanguages;

        /// <inheritdoc/>
        public abstract string Identifier { get; }

        /// <inheritdoc/>
        public IGame Game { get; }

        /// <inheritdoc/>
        public ModType Type { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IModinfo? ModInfo
        {
            get
            {
                if (_modInfo != null)
                    return _modInfo;
                _modInfo = ResolveModInfo();
                return _modInfo;
            }
        }
        
        /// <inheritdoc/>
        public string? IconFile => _iconFile ??= InitializeIcon();

        /// <inheritdoc/>
        public SemanticVersion? Version => _modVersion ??= InitializeVersion();

        /// <inheritdoc/>
        public ICollection<ILanguageInfo> InstalledLanguages => _installedLanguages ??= ResolveInstalledLanguages();


        IList<IModReference> IModIdentity.Dependencies => Dependencies.ToList();

        /// <inheritdoc cref="IModIdentity.Dependencies"/>
        public IReadOnlyList<IModReference> Dependencies { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IMod> Mods { get; }
        
        /// <summary>
        /// Creates a new <see cref="IMod"/> instances with a constant name
        /// </summary>
        /// <param name="game">The game of the mod</param>
        /// <param name="type">The mod's type</param>
        /// <param name="name">The name of the mod.</param>
        protected ModBase(IGame game, ModType type, string name)
        {
            Requires.NotNull(game, nameof(game));
            Requires.NotNullOrEmpty(name, nameof(name));
            Name = name;
            Game = game;
            Type = type;
        }

        /// <summary>
        /// Creates a new <see cref="IMod"/> instances from a modinfo. The modinfo must not be <see langword="null"/>!
        /// </summary>
        /// <param name="game">The game of the mod</param>
        /// <param name="type">The mod's type</param>
        /// <param name="modinfo">The modinfo data.</param>
        /// <exception cref="ModinfoException">when <paramref name="modinfo"/> is not valid.</exception>
        protected ModBase(IGame game, ModType type, IModinfo modinfo)
        {
            Requires.NotNull(modinfo, nameof(modinfo));
            Requires.NotNull(game, nameof(game));
            modinfo.Validate();
            _modInfo = modinfo;
            Game = game;
            Name = modinfo.Name;

        }

        /// <inheritdoc/>
        public bool ResolveDependencies(IDependencyResolver resolver)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool AddMod(IMod mod)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool RemoveMod(IMod mod)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IMod? SearchMod(IModReference modReference, ModSearchOptions modSearchOptions, bool add)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Equals(IMod other)
        {
            return ModEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc/>
        public bool Equals(IModIdentity other)
        {
            return ModEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc/>
        public bool Equals(IModReference other)
        {
            return ModEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc/>
        public IEnumerator<IMod> GetEnumerator()
        {
            return Mods.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual IModinfo? ResolveModInfo()
        {
            return null;
        }

        protected virtual SemanticVersion? InitializeVersion()
        {
            return ModInfo?.Version;
        }

        protected virtual string? InitializeIcon()
        {
            return ModInfo?.Icon;
        }

        protected virtual ICollection<ILanguageInfo> ResolveInstalledLanguages()
        {
            return ModInfo?.Languages.ToList() ?? new List<ILanguageInfo>{LanguageInfo.Default};
        }
    }
}
