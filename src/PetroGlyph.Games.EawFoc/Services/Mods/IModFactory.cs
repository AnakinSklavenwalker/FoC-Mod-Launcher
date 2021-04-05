using System.Collections.Generic;
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
        /// Searches mods according to https://github.com/AlamoEngine-Tools/eaw.modinfo#file-position
        /// If no modinfo file is present, the directory itself will define the mod.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <returns>One mod or multiple variant mods. </returns>
        IEnumerable<IMod> FromReference(IGame game, IModReference modReference);

        /// <summary>
        /// Creates a new <see cref="IMod"/> instance for a game from a file system path.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <param name="modinfo">Optional <see cref="ModinfoData"/> from which the mod will get initialized.</param>
        /// <returns>the Mod instance</returns>
        IMod FromReference(IGame game, IModReference modReference, ModinfoData? modinfo);

        /// <summary>
        /// Creates a new <see cref="IMod"/> instance for a game from a file system path.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <param name="searchModinfoFile">When <see langword="true"/> a modinfo.json file, if present, will be used to initialize;
        /// otherwise a modinfo.json will be ignored</param>
        /// <returns>the Mod instance</returns>
        /// <exception cref="ModException">if <see cref="searchModinfoFile"/> is true and the directory contains any variant files.</exception>
        IMod FromReference(IGame game, IModReference modReference, bool searchModinfoFile);

        /// <summary>
        /// Searches for variant modinfo files and returns new instances for each variant
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="modReferences">Mod references of the new mods.</param>
        /// <returns></returns>
        /// <exception cref="ModException">if no modinfo variant files are found</exception>
        IEnumerable<IMod> VariantsFromReference(IGame game, IList<IModReference> modReferences);


        /// <summary>
        /// Creates virtual mods for a game
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="variantInfos">The variant references and modinfos of the virtual mods.</param>
        /// <returns>One or many virtual mods</returns>
        /// <exception cref="System.ArgumentNullException">if <see cref="variantInfos"/> is <see langword="null"/>.</exception>
        IEnumerable<IMod> CreateVirtualVariants(IGame game, IDictionary<IModReference, IModinfo> variantInfos);
    }
}