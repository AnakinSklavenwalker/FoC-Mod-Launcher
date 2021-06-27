using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Detection.Platform;
using PetroGlyph.Games.EawFoc.Services.Icon;
using PetroGlyph.Games.EawFoc.Services.Language;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class PetroglyphStarWarsGame : PlayableObject, IGame
    {
        public event EventHandler<ModCollectionChangedEventArgs>? ModsCollectionModified;

        private readonly string _normalizedPath;


        protected IServiceProvider ServiceProvider;

        protected internal readonly HashSet<IMod> ModsInternal = new();

        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public GameType Type { get; }

        /// <inheritdoc/>
        public GamePlatform Platform { get; }

        /// <inheritdoc/>
        public IDirectoryInfo Directory { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IMod> Mods => ModsInternal.ToList();

        public PetroglyphStarWarsGame(
            IGameIdentity gameIdentity, 
            IDirectoryInfo gameDirectory, 
            string name, 
            IServiceProvider serviceProvider)
        {
            Requires.NotNullAllowStructs(gameIdentity, nameof(gameIdentity));
            Requires.NotNullOrEmpty(name, nameof(name));
            Requires.NotNull(gameDirectory, nameof(gameDirectory));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Name = name;
            Type = gameIdentity.Type;
            Platform = gameIdentity.Platform;
            Directory = gameDirectory;
            ServiceProvider = serviceProvider;
            _normalizedPath = Directory.FileSystem.Path.NormalizePath(Directory.FullName);
        }

        /// <inheritdoc/>
        public virtual bool Exists()
        {
            if (!GameDetector.GameExeExists(Directory, Type))
                return false;
            var directory = Directory;
            var result = GamePlatformIdentifierFactory.Create(Platform, ServiceProvider).IsPlatform(Type, ref directory);
            return result && directory == Directory;
        }

        /// <inheritdoc/>
        public virtual void Setup(GameSetupOptions setupMode)
        {
            // TODO
        }

        /// <inheritdoc/>
        public virtual bool AddMod(IMod mod)
        {
            // To avoid programming errors due to copies of the same game instance, we only check for reference equality.
            if (!ReferenceEquals(this, mod.Game))
                throw new InvalidOperationException("Mod does not match to this game instance.");

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
        public IEnumerator<IMod> GetEnumerator()
        {
            return Mods.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Equals(IGame? other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (Type != other.Type)
                return false;
            if (Platform != other.Platform)
                return false;
            var normalizedDirectory =
                other.Directory.FileSystem.Path.NormalizePath(other.Directory.FullName);
            return _normalizedPath.Equals(normalizedDirectory, StringComparison.Ordinal);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (this == obj)
                return true;
            if (obj is null)
                return false;
            return obj is IGame otherGame && Equals(otherGame);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _normalizedPath.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Type;
                hashCode = (hashCode * 397) ^ (int) Platform;
                return hashCode;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Type}:{Platform} @{Directory}";
        }


        protected override ISet<ILanguageInfo> ResolveInstalledLanguages()
        {
            return ServiceProvider.GetService<IGameLanguageFinder>()?
                    .FindInstalledLanguages(this) ??
                new HashSet<ILanguageInfo>();
        }

        protected override string? ResolveIconFile()
        {
            return ServiceProvider.GetService<IGameIconFinder>()?.FindIcon(this) ?? string.Empty;
        }

        protected virtual void OnModsCollectionModified(ModCollectionChangedEventArgs e)
        {
            ModsCollectionModified?.Invoke(this, e);
        }
    }
}