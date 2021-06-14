using System;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Icon;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class Mod : ModBase, IPhysicalMod
    {
        internal string InternalPath { get; }

        public IDirectoryInfo Directory { get; }

        public IModinfoFile? ModinfoFile { get; }

        public override string Identifier
        {
            get
            {
                return Type switch
                {
                    ModType.Default => InternalPath,
                    ModType.Workshops => Directory.Name,
                    ModType.Virtual => throw new ModException($"Instance of {typeof(Mod)} must not be virtual."),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
        
        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfoFile modinfoFile, IServiceProvider serviceProvider) 
            : base(game, workshop ? ModType.Workshops : ModType.Default, modinfoFile.GetModinfo(), serviceProvider)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNull(modinfoFile, nameof(modinfoFile));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            ModinfoFile = modinfoFile;
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }

        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, string name, IServiceProvider serviceProvider) 
            : base(game, workshop ? ModType.Workshops : ModType.Default, name, serviceProvider)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNullOrEmpty(name, nameof(name));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }

        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfo modInfoData, IServiceProvider serviceProvider) :
            base(game, workshop ? ModType.Workshops : ModType.Default, modInfoData, serviceProvider)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            if (!modDirectory.Exists)
                throw new ModException($"The mod's directory '{modDirectory.FullName}' does not exists.");
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
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
