using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Language;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class Mod : ModBase, IHasDirectory
    {
        internal string InternalPath { get; }

        public DirectoryInfo Directory { get; }
        

        //public Mod(IGame game, bool workshop, ModInfoFile modInfoFile) :
        //    base(game, workshop ? ModType.Workshops : ModType.Default, modInfoFile)
        //{
        //}

        public Mod(IGame game, DirectoryInfo modDirectory, bool workshop) :
            this(game, modDirectory, workshop, null)
        {
        }

        public Mod(IGame game, DirectoryInfo modDirectory, bool workshop, IModinfo? modInfoData) :
            base(game, workshop ? ModType.Workshops : ModType.Default, modInfoData)
        {
            if (modDirectory is null)
                throw new ArgumentNullException(nameof(modDirectory));
            if (!modDirectory.Exists)
                throw new ModException($"The mod's directory '{modDirectory.FullName}' does not exists.");
            Directory = modDirectory;
            InternalPath = CreateInternalPath(modDirectory);
        }

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

        public override bool Equals(IMod other)
        {
            if (other is null)
                return false;
            if (!(other is IHasDirectory directoryMod))
                return false;

            string otherPath;
            if (directoryMod is Mod mod)
                otherPath = mod.InternalPath;
            else
                otherPath = CreateInternalPath(directoryMod.Directory);

            return FileUtilities.Comparer.Equals(InternalPath, otherPath);
        }

        public override bool Equals(IModIdentity other)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(IModReference? modReference)
        {
            if (modReference is null)
                return false;
            if (modReference.Type != Type)
                return false;
            switch (Type)
            {
                case ModType.Default:
                    var realLocation = FileUtilities.NormalizeForPathComparison(modReference.GetAbsolutePath(Game), true);
                    return FileUtilities.Comparer.Equals(InternalPath, realLocation);
                case ModType.Workshops:
                    return Directory.Name.Equals(modReference.Identifier);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int GetHashCode()
        {
            var hash = FileUtilities.Comparer.GetHashCode(InternalPath);
            return hash;
        }

        public override string ToArgs(bool includeDependencies)
        {
            if (includeDependencies)
                throw new NotImplementedException();

            var folderName = Directory.Name;
            return WorkshopMod ? $"STEAMMOD={folderName}" : $"MODPATH=Mods/{folderName}"; 
        }

        protected override bool ResolveDependenciesCore()
        {
            throw new NotImplementedException();
        }


        protected override string InitializeName()
        {
            var name = base.InitializeName();
            if (string.IsNullOrEmpty(name))
                name = Directory.Name;
            return name;
        }

        protected override ICollection<ILanguageInfo> ResolveInstalledLanguages()
        {
            var languages =  base.ResolveInstalledLanguages();
            if (!languages.Any())
                languages =  new GenericModLanguageFinder(Directory).Find();
            return languages;
        }

        protected override string? InitializeIcon()
        {
            var iconFile = base.InitializeIcon();
            if (!string.IsNullOrEmpty(iconFile))
                iconFile = Path.Combine(Directory.FullName, iconFile!);
            else
            {
                var icon = Directory.EnumerateFiles("*.ico");
                iconFile = icon.FirstOrDefault()?.FullName;
            }
            return iconFile;
        }

        internal static string CreateInternalPath(DirectoryInfo directory)
        {
            return FileUtilities.NormalizeForPathComparison(directory.FullName, true);
        }
    }
}
