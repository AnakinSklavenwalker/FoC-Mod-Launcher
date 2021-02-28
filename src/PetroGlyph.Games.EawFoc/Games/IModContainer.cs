using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Games
{
    public interface IModContainer
    {
        event EventHandler<ModCollectionChangedEventArgs> ModCollectionModified;

        IReadOnlyCollection<IMod> Mods { get; }

        bool AddMod(IMod mod);

        bool RemoveMod(IMod mod);

        /// <summary>
        /// Searches a specific mod and optionally creates a new instance of an <see cref="IMod"/> when necessary
        /// </summary>
        /// <param name="modReference">The data that identifies the mod.</param>
        /// <param name="modSearchOptions">Option to specify how to look for the mod</param>
        /// <param name="add">When set to <c>true</c> the mod will get added to the <see cref="IGame.Mods"/> collection if not present already.
        /// This will raise the <see cref="IGame.ModCollectionModified"/> event </param>
        /// <returns>The mod found</returns>
        IMod? SearchMod(IModReference modReference, ModSearchOptions modSearchOptions, bool add);
    }
}