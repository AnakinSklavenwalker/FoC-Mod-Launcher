using System;
using System.Collections.Generic;
using System.ComponentModel;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Dependencies;

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

        /// <inheritdoc cref="IModIdentity.Name" />
        new string Name { get; }

        /// <summary>
        /// The <see cref="IGame"/> this mod is associated with.
        /// </summary>
        IGame Game { get; }

        /// <summary>
        /// If a modinfo.json file is available its data gets stored here; otherwise this returns <see langword="null"/>
        /// </summary>
        IModinfo? ModInfo { get; }

        /// <summary>
        /// Ordered List of <see cref="IMod"/>s this instance depends on. Initially this list will be empty. To fill it 
        /// </summary>
        new IReadOnlyList<IMod> Dependencies { get; }


        /// <summary>
        /// DependencyResolveStatus flag if and at which result dependencies have been resolved for this instance.
        /// </summary>
        DependencyResolveStatus DependencyResolveStatus { get; }

        /// <summary>
        /// Searches for direct <see cref="IMod"/> dependencies.
        /// Updates <see cref="IModIdentity.Dependencies"/> property.
        /// This operation ignores whether dependencies have already been resolved or not.
        /// <param name="resolver">Resolver service to use</param>
        /// <param name="options"></param>
        /// </summary>
        /// <returns><c>true</c> if all direct dependencies could be resolved (even if this instance has no dependencies); <c>false</c> otherwise</returns>
        /// <exception cref="ModException">Throws exception if a dependency cycle was found.</exception>
        bool ResolveDependencies(IDependencyResolver resolver, DependencyResolverOptions options);
    }
}
