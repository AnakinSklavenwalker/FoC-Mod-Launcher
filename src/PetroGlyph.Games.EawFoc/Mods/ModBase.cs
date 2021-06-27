using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EawModinfo;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Versioning;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using PetroGlyph.Games.EawFoc.Services.Language;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods
{
    /// <summary>
    /// Base implementation for Mods
    /// </summary>
    public abstract class ModBase : PlayableObject, IMod
    {
        /// <inheritdoc/>
        public event EventHandler<ModCollectionChangedEventArgs>? ModsCollectionModified;
        /// <inheritdoc/>
        public event EventHandler<ResolvingModinfoEventArgs>? ResolvingModinfo;
        /// <inheritdoc/>
        public event EventHandler<ModinfoResolvedEventArgs>? ModinfoResolved;
        /// <inheritdoc/>
        public event EventHandler<ModDependenciesChangedEventArgs>? DependenciesChanged;

        private SemanticVersion? _modVersion;
        private IModinfo? _modInfo;
        protected readonly IServiceProvider ServiceProvider;

        private bool _modinfoSearched;
        
        protected readonly List<IMod> DependenciesInternal = new();

        protected readonly HashSet<IMod> ModsInternal = new();

        /// <inheritdoc/>
        public abstract string Identifier { get; }


        /// <inheritdoc/>
        public IGame Game { get; }

        /// <inheritdoc/>
        public ModType Type { get; }

        /// <inheritdoc cref="IModIdentity" />
        public override string Name { get; }

        /// <inheritdoc/>
        string IModIdentity.Name => Name;

        /// <inheritdoc/>
        public IModinfo? ModInfo
        {
            get
            {
                if (_modinfoSearched)
                    return _modInfo;
                if (_modInfo != null)
                    return _modInfo;
                _modInfo = ResolveModInfo();
                _modinfoSearched = true;
                return _modInfo;
            }
        }
        
        /// <inheritdoc/>
        public SemanticVersion? Version => _modVersion ??= InitializeVersion();


        IList<IModReference> IModIdentity.Dependencies => Dependencies.OfType<IModReference>().ToList();

        /// <inheritdoc cref="IModIdentity.Dependencies"/>
        public IReadOnlyList<IMod> Dependencies => DependenciesInternal.ToList();

        /// <inheritdoc/>
        public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();

        /// <summary>
        /// Creates a new <see cref="IMod"/> instances with a constant name
        /// </summary>
        /// <param name="game">The game of the mod</param>
        /// <param name="type">The mod's platform</param>
        /// <param name="name">The name of the mod.</param>
        /// <param name="serviceProvider">The service provider.</param>
        protected ModBase(IGame game, ModType type, string name, IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(game, nameof(game));
            Requires.NotNullOrEmpty(name, nameof(name));
            ServiceProvider = serviceProvider;
            Name = name;
            Game = game;
            Type = type;
        }

        /// <summary>
        /// Creates a new <see cref="IMod"/> instances from a modinfo. The modinfo must not be <see langword="null"/>!
        /// </summary>
        /// <param name="game">The game of the mod</param>
        /// <param name="type">The mod's platform</param>
        /// <param name="modinfo">The modinfo data.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <exception cref="ModinfoException">when <paramref name="modinfo"/> is not valid.</exception>
        protected ModBase(IGame game, ModType type, IModinfo modinfo, IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Requires.NotNull(modinfo, nameof(modinfo));
            Requires.NotNull(game, nameof(game));
            modinfo.Validate();
            _modInfo = modinfo;
            ServiceProvider = serviceProvider;
            Game = game;
            Name = modinfo.Name;
            Type = type;
        }



        public DependencyResolveStatus DependencyResolveStatus { get; protected set; }
        

        /// <inheritdoc/>
        public virtual bool ResolveDependencies(IDependencyResolver resolver, DependencyResolverOptions options)
        {
            if (DependencyResolveStatus == DependencyResolveStatus.Resolving)
                throw new InvalidOperationException("Already resolving mod dependencies."); 
            
            try
            {
                DependencyResolveStatus = DependencyResolveStatus.Resolving;
                var dependencies = resolver.Resolve(this, options);
                var oldList = DependenciesInternal.ToList();
                DependenciesInternal.Clear();
                DependenciesInternal.AddRange(dependencies);
                OnDependenciesChanged(new ModDependenciesChangedEventArgs(this, oldList, dependencies));
                DependencyResolveStatus = DependencyResolveStatus.Resolved;
                return true;
            }
            catch
            {
                DependencyResolveStatus = DependencyResolveStatus.Faulted;
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
        public virtual bool Equals(IMod? other)
        {
            return ModEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc/>
        public virtual bool Equals(IModIdentity? other)
        {
            return ModEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc/>
        public virtual bool Equals(IModReference? other)
        {
            return ModEqualityComparer.Default.Equals(this, other);
        }

        /// <inheritdoc/>
        public IEnumerator<IMod> GetEnumerator()
        {
            return Mods.GetEnumerator();
        }

        public IModinfo? ResetModinfo()
        {
            var old = _modInfo;
            _modinfoSearched = false;
            _modInfo = null;
            return old;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual IModinfo? ResolveModInfoCore()
        {
            // Intentionally return null in base implementation,
            // since we cannot know which modinfo, if multiple found shall get used.
            return null;
        }

        protected virtual SemanticVersion? InitializeVersion()
        {
            return ModInfo?.Version;
        }

        protected override string? ResolveIconFile()
        {
            return ModInfo?.Icon;
        }

        protected override ISet<ILanguageInfo> ResolveInstalledLanguages()
        {
            var factory = ServiceProvider.GetService<IModLanguageFinderFactory>() ?? new ModLanguageFinderFactory();
            var finder = factory.CreateLanguageFinder(this, ServiceProvider);
            return finder.FindInstalledLanguages(this);
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
