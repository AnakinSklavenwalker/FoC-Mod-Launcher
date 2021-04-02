using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EawModinfo.File;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Mods
{
    public static class ModFactory
    {
        private static readonly ILogger? Logger;

        public static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, ModinfoData modinfo)
        {
            return CreateMod(game, type, directory, modinfo, false);
        }
        
        public static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, bool searchModFileOnDisk)
        {
            return CreateMod(game, type, directory, null, searchModFileOnDisk);
        }

        public static IMod CreateMod(IGame game, ModType type, string modPath, ModinfoData modInfo)
        {
            return CreateMod(game, type, new DirectoryInfo(modPath), modInfo);
        }

        public static IMod CreateMod(IGame game, ModType type, string modPath, bool searchModFileOnDisk)
        {
            return CreateMod(game, type, new DirectoryInfo(modPath), searchModFileOnDisk);
        }

        //public static IEnumerable<IMod> CreateModAndVariants(IGame game, ModType platform, DirectoryInfo directory, bool onlyVariantsIfPresent)
        //{
        //    if (game is null)
        //        throw new ArgumentNullException(nameof(game));
        //    if (directory is null)
        //        throw new ArgumentNullException(nameof(directory));

        //    if (!ModInfoFileFinder.TryFind(directory, ModInfoFileFinder.FindOptions.FindAny, out var modInfoCollection) || !modInfoCollection!.Variants.Any())
        //        yield return CreateModInstance(game, platform, directory, modInfoCollection?.MainModInfo?.GetModInfo());

        //    if (modInfoCollection is null)
        //        yield break;

        //    if (!onlyVariantsIfPresent) 
        //        yield return CreateModInstance(game, platform, directory, modInfoCollection.MainModInfo?.GetModInfo());

        //    foreach (var variant in modInfoCollection!.Variants)
        //        yield return CreateModInstance(game, platform, directory, variant.GetModInfo());
        //}

        public static IEnumerable<IMod> CreateModAndVariants(IGame game, ModType type, DirectoryInfo directory,
            bool onlyVariantsIfPresent)
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            ModinfoFinderCollection? modInfoCollection = default;
            try
            {
                var finder = new ModinfoFileFinder(directory);
                modInfoCollection = finder.Find(FindOptions.FindAny);

                if (!modInfoCollection.HasMainModinfoFile && !modInfoCollection.HasVariantModinfoFiles)
                {
                    var mod = CreateModInstanceOrNull(game, type, directory, null);
                    if (mod != null)
                        return new[] { mod };
                }
            }
            catch (Exception e)
            {
                Logger?.LogError(e, e.Message);
            }

            if (modInfoCollection is null)
                return Enumerable.Empty<IMod>();

            if (!modInfoCollection.Variants.Any())
            {
                var mod = CreateModInstanceOrNull(game, type, directory, modInfoCollection.MainModinfo?.TryGetModinfo());
                if (mod != null)
                    return new[] { mod };
            } 



            var result = new List<IMod>();

            if (!onlyVariantsIfPresent && TryCreateModInstance(game, type, directory, modInfoCollection.MainModinfo?.TryGetModinfo(), out var baseMod))
                result.Add(baseMod!);

            foreach (var variant in modInfoCollection!.Variants)
            {
                if (TryCreateModInstance(game, type, directory, variant.TryGetModinfo(), out var variantMod))
                    result.Add(variantMod!);
            }

            return result;
        }

        private static IMod CreateMod(IGame game, ModType type, DirectoryInfo directory, IModinfo? modInfo, bool searchModFileOnDisk) 
        {
            if (game is null)
                throw new ArgumentNullException(nameof(game));
            if (directory is null)
                throw new ArgumentNullException(nameof(directory));

            if (searchModFileOnDisk && modInfo is null)
            {
                var mainModinfoFile = ModinfoFileFinder.FindMain(directory);
                if (mainModinfoFile != null)
                    modInfo = mainModinfoFile!.GetModinfo();
            }


            return CreateModInstance(game, type, directory, modInfo);
        }

        private static bool TryCreateModInstance(IGame game, ModType type, DirectoryInfo directory, IModinfo? modInfo, out IMod? mod)
        {
            mod = default;
            try
            {
                mod = CreateModInstance(game, type, directory, modInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static IMod? CreateModInstanceOrNull(IGame game, ModType type, DirectoryInfo directory, IModinfo? modInfo)
        {
            TryCreateModInstance(game, type, directory, modInfo, out var mod);
            return mod;
        }

        private static IMod CreateModInstance(IGame game, ModType type, DirectoryInfo directory, IModinfo? modInfo = null)
        {
            switch (type)
            {
                case ModType.Default:
                case ModType.Workshops:
                    return new Mod(game, directory, type == ModType.Workshops, modInfo);
                case ModType.Virtual:
                    return new VirtualMod(game, modInfo);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
