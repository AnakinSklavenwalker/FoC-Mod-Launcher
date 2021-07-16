using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    public record ResolveResult
    {
        public DependencyResolveLayout ResolveLayout { get; }
        public IList<IMod> Dependencies { get; }

        public ResolveResult(IList<IMod> dependencies, DependencyResolveLayout resolveLayout)
        {
            Requires.NotNull(dependencies, nameof(dependencies));
            Dependencies = dependencies;
            ResolveLayout = resolveLayout;
        }

        public ResolveResult(IList<IMod> dependencies) : this(dependencies, DependencyResolveLayout.ResolveRecursive)
        {
        }
    }
}