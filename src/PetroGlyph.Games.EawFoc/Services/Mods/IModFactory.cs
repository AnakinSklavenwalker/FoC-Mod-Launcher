using System;
using System.Collections.Generic;
using System.IO;
using EawModinfo.Model;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Mods
{
    /// <summary>
    /// Factory to create one or more <see cref="IMod"/>
    /// </summary>
    public interface IModFactory
    {
        /// <summary>
        /// Creates one or many <see cref="IPhysicalMod"/>s according to
        /// <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#file-position"/>
        /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
        /// If no modinfo file is present in that location, the directory itself will define the mod.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <returns>One mod or multiple variant mods. </returns>
        /// <exception cref="DirectoryNotFoundException">when <see cref="IModReference.Identifier"/> could not be located as an existing location</exception>
        /// <exception cref="ModException">when no instance could be created due to missing information (such as the mod's name)</exception>
        IEnumerable<IPhysicalMod> FromReference(IGame game, IModReference modReference);

        /// <summary>
        /// Creates a new <see cref="IPhysicalMod"/> instance for a game from a file system path.
        /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <param name="modinfo">Optional <see cref="ModinfoData"/> from which the mod will get initialized.</param>
        /// <returns>the Mod instance</returns>
        /// <exception cref="DirectoryNotFoundException">when <see cref="IModReference.Identifier"/> could not be located as an existing location</exception>
        /// <exception cref="ModException">when no instance could be created due to missing information (such as the mod's name)</exception>
        IPhysicalMod FromReference(IGame game, IModReference modReference, ModinfoData? modinfo);

        /// <summary>
        /// Creates a new <see cref="IPhysicalMod"/> instance for a game from a file system path.
        /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <param name="searchModinfoFile">When <see langword="true"/> a modinfo.json file, if present, will be used to initialize;
        /// otherwise a modinfo.json will be ignored</param>
        /// <returns>the Mod instance</returns>
        /// <exception cref="DirectoryNotFoundException">when <see cref="IModReference.Identifier"/> could not be located as an existing location</exception>
        /// <exception cref="ModException">when no instance could be created due to missing information (such as the mod's name)</exception>
        /// <exception cref="ModException">if <see cref="searchModinfoFile"/> is true and the directory contains any variant files.</exception>
        IPhysicalMod FromReference(IGame game, IModReference modReference, bool searchModinfoFile);

        /// <summary>
        /// Searches for variant modinfo files and returns new instances for each variant
        /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="modReferences">Mod references of the new mods.</param>
        /// <returns>All mod variants which are found.</returns>
        /// <exception cref="DirectoryNotFoundException">when <see cref="IModReference.Identifier"/> could not be located as an existing location</exception>
        /// <exception cref="ModException">when no instance could be created due to missing information (such as the mod's name)</exception>
        IEnumerable<IPhysicalMod> VariantsFromReference(IGame game, IList<IModReference> modReferences);


        /// <summary>
        /// Creates virtual mods for a game
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="virtualModInfos">Set of <see cref="IModinfo"/> where each element defines its own virtual mod.</param>
        /// <returns>One or many virtual mods</returns>
        /// <exception cref="InvalidOperationException">any <see cref="IModinfo"/> of <paramref name="virtualModInfos"/> is <see langword="null"/></exception>
        /// <exception cref="System.ArgumentNullException">if <see cref="virtualModInfos"/> is <see langword="null"/>.</exception>
        IEnumerable<IVirtualMod> CreateVirtualVariants(IGame game, ISet<IModinfo> virtualModInfos);

        /// <summary>
        /// Creates virtual mods for a game
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="virtualModInfos">Dictionary where key is the name of the virtual mod.
        /// The value are the sorted dependencies of the virtual mod</param>
        /// <returns>One or many virtual mods</returns>
        /// <exception cref="InvalidOperationException">any key or value or element of <paramref name="virtualModInfos"/> is <see langword="null"/></exception>
        /// <exception cref="System.ArgumentNullException">if <see cref="virtualModInfos"/> is <see langword="null"/>.</exception>
        IEnumerable<IVirtualMod> CreateVirtualVariants(IGame game, Dictionary<string, IList<IMod>> virtualModInfos);
    }
}