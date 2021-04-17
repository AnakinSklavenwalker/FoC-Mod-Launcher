using System.Collections.Generic;
using System.Linq;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    public class ModDependencyTraverser
    {
        public enum TraverseDirection
        {
            /// <summary>
            /// The first item in the list is the most base dependency.
            /// </summary>
            BaseToSub,
            /// <summary>
            /// The first item in the list is the closest dependency.
            /// </summary>
            SubToBase
        }

        private readonly IMod _mod;

        public ModDependencyTraverser(IMod mod)
        {
            _mod = mod;
        }

        public bool HasDependencyCycles()
        {
            try
            {
                CheckForCycles();
            }
            catch (ModException)
            {
                return true;
            }
            return false;
        }

        public IList<IMod> Traverse(TraverseDirection direction = TraverseDirection.SubToBase, bool throwOnCycle = true)
        {
            if (throwOnCycle)
                CheckForCycles();

            var result = TraverseCore().Distinct();
            if (direction == TraverseDirection.BaseToSub)
                result = result.Reverse();
            return result.ToList();
        }

        private void CheckForCycles()
        {
            CheckForCycles(_mod, new HashSet<IMod>());
        }

        private static void CheckForCycles(IMod current, ISet<IMod> seenNodes)
        {
            if (!seenNodes.Add(current))
                throw new ModException();

            foreach (var before in current.Dependencies.OfType<IMod>()) 
                CheckForCycles(before, seenNodes);
            seenNodes.Remove(current);
        }

        private IEnumerable<IMod> TraverseCore()
        {
            var stack = new Queue<IMod>();
            var visited = new HashSet<IMod>();
            stack.Enqueue(_mod);
            while (stack.Count > 0)
            {
                var current = stack.Dequeue();
                if (!visited.Add(current))
                    continue;
                yield return current;

                var neighbors = current.Dependencies.Where(n => !visited.Contains(n)).OfType<IMod>();

                foreach (var child in neighbors)
                    stack.Enqueue(child);
            }
        }
    }
}