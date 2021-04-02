using System;
using System.ComponentModel;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Mods.Dependencies;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public interface IMod : IModIdentity, IModReference, IPlayableObject, IModContainer, IEquatable<IMod>
    {
        /// <summary>
        /// Gets fired when this instance is about to lazy-load <see cref="ModInfo"/>.
        /// <br></br>
        /// This operation can be cancelled by settings <see cref="CancelEventArgs.Cancel"/> to <see langword="true"/>. 
        /// In this case <see cref="ModInfo"/> will be set to <see langword="null"/>
        /// </summary>
        event EventHandler<ResolvingModinfoEventArgs> ResolvingModinfo;

        /// <summary>
        /// Gets fired after the lazy-load of <see cref="ModInfo"/> was completed.
        /// </summary>
        event EventHandler<ModinfoResolvedEventArgs> ModinfoResolved;

        /// <summary>
        /// Gets fired when the <see cref="IModIdentity.Dependencies"/> list was altered.
        /// </summary>
        event EventHandler<ModDependenciesChangedEventArgs> DependenciesChanged;

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
        /// <param name="recursive">When set to <see langword="true"/> the mod dependency chain gets resolved recursively.</param>
        /// <param name="addModContainer">When set to <see langword="true"/>
        /// the <see cref="IModContainer.Mods"/> collection get updated.
        /// Only first level dependencies shall get added with this option.</param>
        /// </summary>
        /// <returns><c>true</c> if all direct dependencies could be resolved; <c>false</c> otherwise</returns>
        /// <exception cref="ModException">Throws exception if a dependency cycle was found.</exception>
        bool ResolveDependencies(IDependencyResolver resolver, bool recursive, bool addModContainer);
    }
}
