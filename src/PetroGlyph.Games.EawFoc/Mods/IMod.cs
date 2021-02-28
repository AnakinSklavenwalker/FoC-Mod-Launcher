using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

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
        /// Collection of available languages for this mod.
        /// </summary>
        ICollection<ILanguageInfo> InstalledLanguages { get; }

        /// <summary>
        /// Path to an icon which shall get used as a taskbar item when the mod is started.
        /// </summary>
        string? IconFile { get; }

        /// <summary>
        /// Searches for direct <see cref="IMod"/> dependencies. It does not resolve recursively.
        /// Updates <see cref="Dependencies"/> property.
        /// <param name="resolveStrategy">Option how resolving should be handled.</param>
        /// </summary>
        /// <returns><c>true</c> if all direct dependencies could be resolved; <c>false</c> otherwise</returns>
        /// <exception cref="PetroglyphModException">Throws exception if a dependency cycle was found.</exception>
        bool ResolveDependencies(ModDependencyResolveStrategy resolveStrategy);

        /// <summary>
        /// Converts this mod into a command line argument that can be used for starting the mod.
        /// </summary>
        /// <param name="traverseDependencies">When <c>true</c> this methods returns a valid argument that contains the whole dependency chain.</param>
        /// <remarks>This method does not re-resolve dependencies but takes whatever there is in <see cref="Dependencies"/></remarks>
        /// <returns>A valid command line argument.</returns>
        string ToArgs(bool traverseDependencies);
    }
}
