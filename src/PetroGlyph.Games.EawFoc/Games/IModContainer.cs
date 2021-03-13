using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Games
{

    /// <summary>
    /// This instance can be parent for one or many mods. 
    /// </summary>
    public interface IModContainer : IEnumerable<IMod>
    {
        /// <summary>
        /// Notifies any handler when <see cref="Mods"/> was altered.
        /// </summary>
        event EventHandler<ModCollectionChangedEventArgs> ModCollectionModified;

        /// <summary>
        /// Set of all mods this instance is associated with.
        /// For an <see cref="IGame"/> this is a flattened set of all Mods and their respective submods.
        /// </summary>
        IReadOnlyCollection<IMod> Mods { get; }

        /// <summary>
        /// Associates an <see cref="IMod"/> to the this <see cref="IModContainer"/> 
        /// </summary>
        /// <param name="mod">The mod instance</param>
        /// <returns><see langword="true"/> if the mod was added; otherwise <see langword="false"/> if the mod already existed.</returns>
        /// <exception cref="ArgumentNullException"> when <paramref name="mod"/> is null</exception>
        bool AddMod(IMod mod);

        /// <summary>
        /// Removed an <see cref="IMod"/> from the this <see cref="IModContainer"/> 
        /// </summary>
        /// <param name="mod">The mod instance</param>
        /// <returns><see langword="true"/> if the mod was removed; otherwise <see langword="false"/> if the mod did not exists.</returns>
        /// <exception cref="ArgumentNullException"> when <paramref name="mod"/> is null</exception>
        bool RemoveMod(IMod mod);

        /// <summary>
        /// Searches a specific mod and optionally creates a new instance of an <see cref="IMod"/> when necessary
        /// </summary>
        /// <param name="modReference">The data that identifies the mod.</param>
        /// <param name="modSearchOptions">Option to specify how to look for the mod</param>
        /// <param name="add">When set to <c>true</c> the mod will get added to the <see cref="Mods"/> collection if not present already.
        /// This will raise the <see cref="IGame.ModCollectionModified"/> event </param>
        /// <returns>The mod found or <see langword="null"/></returns>
        IMod? SearchMod(IModReference modReference, ModSearchOptions modSearchOptions, bool add);
    }
}