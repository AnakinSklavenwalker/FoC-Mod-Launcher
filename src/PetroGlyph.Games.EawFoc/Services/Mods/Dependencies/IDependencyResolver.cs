using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    /// <summary>
    /// Resolves mod dependencies.
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Resolves the dependencies of a given mod.
        /// </summary>
        /// <param name="mod">The target mod</param>
        /// <param name="options">Options how the resolver behaves.</param>
        /// <returns>A sorted list of dependencies. Result depends of the <see cref="options"/>.</returns>
        IList<IMod> Resolve(IMod mod, DependencyResolverOptions options);
    }
}
