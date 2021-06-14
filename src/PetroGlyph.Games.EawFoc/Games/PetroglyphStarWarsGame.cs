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
    public class PetroglyphStarWarsGame : IGame
    {
        public event EventHandler<ModCollectionChangedEventArgs>? ModsCollectionModified;

        private readonly string _normalizedPath;

        private string? _iconFile;
        private ISet<ILanguageInfo>? _languages;


        protected IServiceProvider ServiceProvider;

        protected internal readonly HashSet<IMod> ModsInternal = new();
        

        public string Name { get; }

        public GameType Type { get; }
        public GamePlatform Platform { get; }

        public IDirectoryInfo Directory { get; }

        public ISet<ILanguageInfo> InstalledLanguages
        {
            get
            {
                // TODO: Flag if already tried
                return _languages ??= ResolveInstalledLanguages();
            }
        }

        public string? IconFile
        {
            get
            {
                // TODO: Flag if already tried
                return _iconFile ??= ResolveIconFile();
            }
        }

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

        
        public virtual bool Exists()
        {
            if (!GameDetector.GameExeExists(Directory, Type))
                return false;
            var directory = Directory;
            var result = GamePlatformIdentifierFactory.Create(Platform, ServiceProvider).IsPlatform(Type, ref directory);
            return result && directory == Directory;
        }


        public virtual void Setup(GameSetupOptions setupMode)
        {
            // TODO
        }

        public virtual bool AddMod(IMod mod)
        {
            if (this != mod.Game)
                throw new InvalidOperationException("Mod does not match to this game instance.");

            var result = ModsInternal.Add(mod);
            if (result)
                OnModsCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Add));
            return result;
        }

        public virtual bool RemoveMod(IMod mod)
        {
            var result = ModsInternal.Remove(mod);
            if (result)
                OnModsCollectionModified(new ModCollectionChangedEventArgs(mod, ModCollectionChangedAction.Remove));
            return result;
        }

        public IEnumerator<IMod> GetEnumerator()
        {
            return Mods.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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

        public override bool Equals(object? obj)
        {
            if (this == obj)
                return true;
            if (obj is null)
                return false;
            return obj is IGame otherGame && Equals(otherGame);
        }
        
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
        
        public override string ToString()
        {
            return $"{Type}:{Platform} @{Directory}";
        }


        protected virtual ISet<ILanguageInfo> ResolveInstalledLanguages()
        {
            return ServiceProvider.GetService<IGameLanguageFinder>()?
                    .FindInstalledLanguages(this) ??
                new HashSet<ILanguageInfo>();
        }

        protected virtual string? ResolveIconFile()
        {
            return ServiceProvider.GetService<IGameIconFinder>()?.FindIcon(this) ?? string.Empty;
        }

        public void ResetLanguages()
        {
            _languages = null;
        }

        public void ResetIcon()
        {
            _iconFile = null;
        }

        protected virtual void OnModsCollectionModified(ModCollectionChangedEventArgs e)
        {
            ModsCollectionModified?.Invoke(this, e);
        }
    }
}