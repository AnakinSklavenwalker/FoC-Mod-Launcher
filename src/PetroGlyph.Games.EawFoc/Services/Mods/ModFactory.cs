using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using EawModinfo.File;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Name;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services
{
    public class ModFactory : IModFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ModFactory(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<IPhysicalMod> FromReference(IGame game, IModReference modReference)
        {
            var modReferenceLocation = new ModReferenceLocationResolver().ResolveLocation(modReference, game);
            var modinfoFinder = new ModinfoFileFinder(modReferenceLocation);
            var searchResult = modinfoFinder.Find(FindOptions.FindAny);

            return !searchResult.HasVariantModinfoFiles
                ? new[] {CreateModFromDirectory(game, modReference, modReferenceLocation, searchResult.MainModinfo)}
                : CreateVariants(game, modReference, modReferenceLocation, searchResult.Variants);
        }

        public IPhysicalMod FromReference(IGame game, IModReference modReference, ModinfoData? modinfo)
        {
            var modReferenceLocation = new ModReferenceLocationResolver().ResolveLocation(modReference, game);
            return CreateModFromDirectory(game, modReference, modReferenceLocation, modinfo);
        }

        public IPhysicalMod FromReference(IGame game, IModReference modReference, bool searchModinfoFile)
        {
            var modReferenceLocation = new ModReferenceLocationResolver().ResolveLocation(modReference, game);

            IModinfoFile? mainModinfoFile = null;
            if (searchModinfoFile)
            {
                var modinfoFinder = new ModinfoFileFinder(modReferenceLocation);
                mainModinfoFile = modinfoFinder.Find(FindOptions.FindMain).MainModinfo;
            }
            return CreateModFromDirectory(game, modReference, modReferenceLocation, mainModinfoFile);
        }

        public IEnumerable<IPhysicalMod> VariantsFromReference(IGame game, IList<IModReference> modReferences)
        {
            var mods = new HashSet<IPhysicalMod>();
            foreach (var modReference in modReferences)
            {
                var modReferenceLocation = new ModReferenceLocationResolver().ResolveLocation(modReference, game); 
                var variants = new ModinfoFileFinder(modReferenceLocation).Find(FindOptions.FindVariants);
                var variantMods = CreateVariants(game, modReference, modReferenceLocation, variants);
                foreach (var mod in variantMods)
                {
                    if (!mods.Add(mod))
                        throw new ModException(
                            $"Unable to create mod {mod.Name} " +
                            $"from '{modReferenceLocation.FullName}' because it already was created within this operation.");
                }
            }
            return mods;
        }

        private IEnumerable<IPhysicalMod> CreateVariants(IGame game, IModReference modReference, IDirectoryInfo modReferenceLocation, IEnumerable<IModinfoFile> variantModInfoFiles)
        {
            var variants = new HashSet<IPhysicalMod>();
            var names = new HashSet<string>();
            foreach (var variant in variantModInfoFiles)
            {
                if (variant.FileKind == ModinfoFileKind.MainFile)
                    throw new ModException("Cannot create a variant mod from a main modinfo file.");

                var mod = CreateModFromDirectory(game, modReference, modReferenceLocation, variant);
                if (!variants.Add(mod) || !names.Add(mod.Name))
                    throw new ModException($"Unable to create variant mod of name {mod.Name}, because it already exists");
            }
            return variants;
        }

        public IEnumerable<IVirtualMod> CreateVirtualVariants(IGame game, ISet<IModinfo> virtualModInfos)
        {
            var mods = new HashSet<IVirtualMod>();
            foreach (var modinfo in virtualModInfos)
                mods.Add(new VirtualMod(game, modinfo, _serviceProvider));
            return mods;
        }

        public IEnumerable<IVirtualMod> CreateVirtualVariants(IGame game, Dictionary<string, IList<IMod>> virtualModInfos)
        {
            var mods = new HashSet<IVirtualMod>();
            foreach (var virtualModData in virtualModInfos)
                mods.Add(new VirtualMod(virtualModData.Key, game, virtualModData.Value, _serviceProvider));
            return mods;
        }

        private IPhysicalMod CreateModFromDirectory(IGame game, IModReference modReference, IDirectoryInfo directory,
            IModinfoFile? modinfoFile)
        {
            return CreateModFromDirectory(game, modReference, directory, modinfoFile?.GetModinfo());
        }

        private IPhysicalMod CreateModFromDirectory(IGame game, IModReference modReference, IDirectoryInfo directory, IModinfo? modinfo)
        {
            if (modReference.Type == ModType.Virtual)
                throw new InvalidOperationException("modType cannot be a virtual mod.");
            if (!directory.Exists)
                throw new DirectoryNotFoundException($"Unable to find mod location '{directory.FullName}'");

            if (modinfo == null)
            {
                var nameResolver = _serviceProvider.GetService<IModNameResolver>() ?? new DefaultModNameResolver(_serviceProvider);
                var name = nameResolver.ResolveName(modReference);
                if (string.IsNullOrEmpty(name))
                    throw new ModException("Unable to create a mod with an empty name.");
                return new Mod(game, directory, modReference.Type == ModType.Workshops, name, _serviceProvider);
            }

            modinfo.Validate();
            return new Mod(game, directory, modReference.Type == ModType.Workshops, modinfo, _serviceProvider);
        }
    }
}
