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
        public event EventHandler<ModCollectionChangedEventArgs>? ModsCollectionModified;
        /// <inheritdoc/>
        public event EventHandler<ResolvingModinfoEventArgs>? ResolvingModinfo;
        /// <inheritdoc/>
        public event EventHandler<ModinfoResolvedEventArgs>? ModinfoResolved;
        /// <inheritdoc/>
        public event EventHandler<ModDependenciesChangedEventArgs>? DependenciesChanged;

        private string? _iconFile;
        private SemanticVersion? _modVersion;
        private IModinfo? _modInfo;
        private ICollection<ILanguageInfo>? _installedLanguages;
        private readonly List<IMod> _dependencies = new();

        protected readonly HashSet<IMod> ModsInternal = new();

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


        IList<IModReference> IModIdentity.Dependencies => Dependencies.OfType<IModReference>().ToList();

        /// <inheritdoc cref="IModIdentity.Dependencies"/>
        public IReadOnlyList<IMod> Dependencies => _dependencies.ToList();

        /// <inheritdoc/>
        public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();
        
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
            Type = type;

        }

        /// <inheritdoc/>
        public bool ResolveDependencies(IDependencyResolver resolver, bool recursive, bool addModContainer)
        {
            var modinfo = ModInfo;
            if (modinfo is null)
                return false;
            if (!Equals(modinfo))
                throw new ModException("Unable to resolve mod dependencies. Modinfo is not matching the current mod.");

            try
            {
                var options = new DependencyResolverOptions {Recursive = recursive};
                var dependencies = resolver.Resolve(this, options);
                var oldList = _dependencies.ToList();
                _dependencies.Clear();
                _dependencies.AddRange(dependencies);
                OnDependenciesChanged(new ModDependenciesChangedEventArgs(this, oldList, dependencies));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual bool AddMod(IMod mod)
        {
            if (Game != mod.Game)
                throw new InvalidOperationException("Game instances of the two mods must be equal");
            var result = ModsInternal.Add(mod);
            if (result)
                OnModsCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Add));
            return result;
        }

        /// <inheritdoc/>
        public virtual bool RemoveMod(IMod mod)
        {
            var result = ModsInternal.Remove(mod);
            if (result)
                OnModsCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Remove));
            return result;
        }

        /// <inheritdoc/>
        public virtual bool Equals(IMod other)
        {
            return ModEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc/>
        public virtual bool Equals(IModIdentity other)
        {
            return ModEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc/>
        public virtual bool Equals(IModReference other)
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

        protected virtual IModinfo? ResolveModInfoCore()
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

        private IModinfo? ResolveModInfo()
        {
            var resolvingArgs = new ResolvingModinfoEventArgs(this);
            OnResolvingModinfo(resolvingArgs);
            IModinfo? modinfo = null;
            if (!resolvingArgs.Cancel)
                modinfo =  ResolveModInfoCore();
            OnModinfoResolved(new ModinfoResolvedEventArgs(this, modinfo));
            return modinfo;
        }

        protected virtual void OnResolvingModinfo(ResolvingModinfoEventArgs e)
        {
            ResolvingModinfo?.Invoke(this, e);
        }

        protected virtual void OnModinfoResolved(ModinfoResolvedEventArgs e)
        {
            ModinfoResolved?.Invoke(this, e);
        }

        protected virtual void OnModsCollectionModified(ModCollectionChangedEventArgs e)
        {
            ModsCollectionModified?.Invoke(this, e);
        }

        protected virtual void OnDependenciesChanged(ModDependenciesChangedEventArgs e)
        {
            DependenciesChanged?.Invoke(this, e);
        }
    }
}
