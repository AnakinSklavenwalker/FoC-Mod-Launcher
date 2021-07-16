using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    public class ModDependencyResolver : IDependencyResolver
    {
        private static readonly IList<IMod> EmptyDependencyList = new List<IMod>();

        private readonly HashSet<IMod> _visitedMods = new(ModEqualityComparer.ExcludeDependencies);

        public ResolveResult Resolve(IMod mod, DependencyResolverOptions options)
        {
            Requires.NotNull(mod, nameof(mod));
            Requires.NotNull(options, nameof(options));

            
            var dependencies = ResolveNextNeighborsDependencies(mod, out var resolveLayout);
            var workingQueue = new Queue<IList<IMod>>();
            
            workingQueue.Enqueue(dependencies);

            while (options.ResolveCompleteChain && workingQueue.Count > 0)
            {
                var nextMods = workingQueue.Dequeue();

                var modsForNextIteration = new List<IMod>();
                foreach (var nextMod in nextMods)
                {
                    if (_visitedMods.Contains(nextMod))
                        continue;
                    // We should not pass options with ResolveCompleteChain set, which would be truly recursive. 
                    // This however would mean that in the event of a cycle
                    // the whole chain would be in a undefined state,
                    // where we could never trust the IMod.Dependencies property.
                    // This way we make sure the property always yields the correct 1st-level dependencies.  
                    nextMod.ResolveDependencies(this, new DependencyResolverOptions());
                    modsForNextIteration.AddRange(nextMod.Dependencies);
                }
                workingQueue.Enqueue(modsForNextIteration);
            }

            if (options.CheckForCycle)
            {
                var traverser = new ModDependencyTraverser(mod);
                if (traverser.HasDependencyCycles())
                    throw new ModException($"The mod {mod} has a dependency cycle");
            }

            return new ResolveResult(dependencies, resolveLayout);
        }
        
        private IList<IMod> ResolveNextNeighborsDependencies(IMod rootMod, out DependencyResolveLayout layout)
        {
            layout = DependencyResolveLayout.ResolveRecursive;
            _visitedMods.Add(rootMod);

            if (rootMod.DependencyResolveStatus == DependencyResolveStatus.Resolved)
                return rootMod.Dependencies.ToList();

            if (rootMod.ModInfo is null)
                return EmptyDependencyList;

            layout = rootMod.ModInfo.Dependencies.ResolveLayout;

            var dependencies = rootMod.ModInfo.Dependencies
                .Select(modReference => FindMod(rootMod.Game, modReference))
                .ToList();

            // This is the only cycle we can find, without traversing the whole Chain down.
            if (dependencies.Contains(rootMod))
                throw new ModException("Dependency Cycle: A Mod must not refer to itself.");

            return dependencies;
        }

        private static IMod FindMod(IGame game, IModReference modReference)
        {
            var foundMod = game.Mods.FirstOrDefault(m => ModEqualityComparer.Default.Equals(m, modReference));
            if (foundMod is null)
                throw new ModException($"Unable to dependency '{modReference.Identifier}' from game {game}");
            return foundMod;
        }
    }
}