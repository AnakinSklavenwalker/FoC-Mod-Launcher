using System;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Dependencies;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public interface IMod : IModIdentity, IModReference, IPlayableObject, IModContainer, IEquatable<IMod>
    {
        /// <summary>
        /// The <see cref="IGame"/> this mod is associated with.
        /// </summary>
        IGame Game { get; }

        /// <summary>
        /// If a modinfo.json file is available its data gets stored here; otherwise this returns <see langword="null"/>
        /// </summary>
        IModinfo? ModInfo { get; }


        /// <summary>
        /// Searches for direct <see cref="IMod"/> dependencies. It does not resolve recursively.
        /// Updates <see cref="IModIdentity.Dependencies"/> property.
        /// <param name="resolver">Resolver service to use</param>
        /// </summary>
        /// <returns><c>true</c> if all direct dependencies could be resolved; <c>false</c> otherwise</returns>
        /// <exception cref="ModException">Throws exception if a dependency cycle was found.</exception>
        bool ResolveDependencies(IDependencyResolver resolver);
    }
}
