using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Icon;
using PetroGlyph.Games.EawFoc.Services.Language;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;
using LanguageInfo = EawModinfo.Model.LanguageInfo;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class Mod : ModBase, IPhysicalMod
    {
        internal string InternalPath { get; }

        public IDirectoryInfo Directory { get; }

        public IModinfoFile? ModinfoFile { get; }

        protected IServiceProvider ServiceProvider { get; }

        public override string Identifier
        {
            get
            {
                switch (Type)
                {
                    case ModType.Default:
                        return InternalPath;
                    case ModType.Workshops:
                        return Directory.Name;
                    case ModType.Virtual:
                        throw new ModException($"Instance of {typeof(Mod)} must not be virtual.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        

        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfoFile modinfoFile, IServiceProvider serviceProvider) 
            : base(game, workshop ? ModType.Workshops : ModType.Default, modinfoFile.GetModinfo())
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNull(modinfoFile, nameof(modinfoFile));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            ModinfoFile = modinfoFile;
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
            ServiceProvider = serviceProvider;
        }

        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, string name, IServiceProvider serviceProvider) 
            : base(game, workshop ? ModType.Workshops : ModType.Default, name)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNullOrEmpty(name, nameof(name));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
            ServiceProvider = serviceProvider;
        }

        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfo modInfoData, IServiceProvider serviceProvider) :
            base(game, workshop ? ModType.Workshops : ModType.Default, modInfoData)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            if (!modDirectory.Exists)
                throw new ModException($"The mod's directory '{modDirectory.FullName}' does not exists.");
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
            ServiceProvider = serviceProvider;
        }
        

        protected override ICollection<ILanguageInfo> ResolveInstalledLanguages()
        {
            // We don't "trust" the modinfo here as the default language (EN) also gets
            // applied when nothing was specified by the mod developer.
            // Only if we have more than the default language, we trust the modinfo.
            if (ModInfo is not null && ModInfo.Languages.All(x => x.Equals(LanguageInfo.Default)))
                return ModInfo.Languages.ToList();

            return ServiceProvider.GetService<IModLanguageFinder>()?
                .FindInstalledLanguages(this) ?? new List<ILanguageInfo>();
        }

        protected override string? InitializeIcon()
        {
            var iconFile = base.InitializeIcon();
            return string.IsNullOrEmpty(iconFile)
                ? null
                : ServiceProvider.GetService<IModIconFinder>()?.FindIcon(this) ?? null;
        }

        internal static string CreateInternalPath(IDirectoryInfo directory)
        {
            return directory.FileSystem.Path.NormalizePath(directory.FullName);
        }
    }
}
