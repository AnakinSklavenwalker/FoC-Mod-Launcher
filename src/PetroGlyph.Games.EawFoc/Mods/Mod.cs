using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services;
using PetroGlyph.Games.EawFoc.Services.Icon;
using PetroGlyph.Games.EawFoc.Services.Language;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;
using LanguageInfo = EawModinfo.Model.LanguageInfo;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class Mod : ModBase, IPhysicalPlayableObject
    {
        internal string InternalPath { get; }

        public IDirectoryInfo Directory { get; }

        public IModinfoFile? ModinfoFile { get; }

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
        

        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfoFile modinfoFile) 
            : base(game, workshop ? ModType.Workshops : ModType.Default, modinfoFile.GetModinfo())
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNull(modinfoFile, nameof(modinfoFile));
            ModinfoFile = modinfoFile;
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }

        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, string name) 
            : base(game, workshop ? ModType.Workshops : ModType.Default, name)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            Requires.NotNullOrEmpty(name, nameof(name));
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }

        public Mod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfo modInfoData) :
            base(game, workshop ? ModType.Workshops : ModType.Default, modInfoData)
        {
            Requires.NotNull(modDirectory, nameof(modDirectory));
            if (!modDirectory.Exists)
                throw new ModException($"The mod's directory '{modDirectory.FullName}' does not exists.");
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }
        

        protected override ICollection<ILanguageInfo> ResolveInstalledLanguages()
        {
            // We don't "trust" the modinfo here as the default language (EN) also gets
            // applied when nothing was specified by the mod developer.
            // Only if we have more than the default language, we trust the modinfo.
            if (ModInfo is not null && ModInfo.Languages.All(x => x.Equals(LanguageInfo.Default)))
                return ModInfo.Languages.ToList();
            return new CompositeLanguageFinder().FindInstalledLanguages(this);
        }

        protected override string? InitializeIcon()
        {
            var iconFile = base.InitializeIcon();
            return string.IsNullOrEmpty(iconFile) ? iconFile : new CompositeIconFinder().FindIcon(this);
        }

        internal static string CreateInternalPath(IDirectoryInfo directory)
        {
            return directory.FileSystem.Path.NormalizePath(directory.FullName);
        }
    }
}
