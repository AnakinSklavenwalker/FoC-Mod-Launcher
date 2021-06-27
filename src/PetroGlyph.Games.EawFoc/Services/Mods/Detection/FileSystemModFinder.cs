using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Utilities;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public class FileSystemModFinder : IModReferenceFinder
    {
        public ISet<IModReference> FindMods(IGame game)
        {
            Requires.NotNull(game, nameof(game));
            if (!game.Exists())
                throw new PetroglyphGameException("The game does not exist");

            var mods = new HashSet<IModReference>();
            foreach (var modReference in GetNormalMods(game).Union(GetWorkshopsMods(game))) 
                mods.Add(modReference);
            return mods;
        }

        private static IEnumerable<ModReference> GetNormalMods(IGame game)
        {
            return GetAllModsFromPath(game.GetModsLocation(), false);
        }

        private static IEnumerable<ModReference> GetWorkshopsMods(IGame game)
        {
            return game.Platform != GamePlatform.SteamGold
                ? Enumerable.Empty<ModReference>()
                : GetAllModsFromPath(SteamGameHelpers.GetWorkshopsLocation(game), false);
        }

        private static IEnumerable<ModReference> GetAllModsFromPath(IDirectoryInfo lookupDirectory, bool isWorkshopsPath)
        {
            if (!lookupDirectory.Exists)
                yield break;

            var type = isWorkshopsPath ? ModType.Workshops : ModType.Default;
            foreach (var modDirectory in lookupDirectory.EnumerateDirectories())
            {
                var normalizedPath = lookupDirectory.FileSystem.Path.NormalizePath(modDirectory.FullName);
                yield return new ModReference(normalizedPath, type);
            }
        }

        private readonly struct ModReference : IModReference
        {
            public string Identifier { get; }
            public ModType Type { get; }

            public ModReference(string id, ModType type)
            {
                Identifier = id;
                Type = type;
            }

            public bool Equals(IModReference? other)
            {
                return Identifier == other?.Identifier && Type == other.Type;
            }

            public override bool Equals(object? obj)
            {
                return obj is ModReference other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Identifier.GetHashCode() * 397) ^ (int)Type;
                }
            }
        }
    }
}