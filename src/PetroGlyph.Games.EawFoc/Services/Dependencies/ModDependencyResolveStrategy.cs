using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Dependencies
{
    /// <summary>
    /// Defines the strategy how to resolve mod dependencies.
    /// By default dependencies get resolved from <see cref="IGame.Mods"/>
    /// Additional behavior will be specified with this enum.
    /// </summary>
    [Flags]
    public enum ModDependencyResolveStrategy
    {
        /// <summary>
        /// Recursively resolves dependencies for this instance as well as for its dependencies.
        /// </summary>
        Recursive = 1,
        /// <summary>
        /// Additionally searches for dependencies on the file System. 
        /// </summary>
        FromFileSystem = 2,
    }
}