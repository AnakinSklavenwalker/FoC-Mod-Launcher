using System.Collections.Generic;
using EawModinfo.Model;
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
        /// <param name="path">The path on the file system of the mod.</param>
        /// <returns>One mod or multiple variant mods. </returns>
        IEnumerable<IMod> FromDirectory(IGame game, string path);

        /// <summary>
        /// Creates a new <see cref="IMod"/> instance for a game from a file system path.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="path">The path on the file system of the mod.</param>
        /// <param name="modinfo">Optional <see cref="ModinfoData"/> from which the mod will get initialized.</param>
        /// <returns>the Mod instance</returns>
        IMod FromDirectory(IGame game, string path, ModinfoData? modinfo);

        /// <summary>
        /// Creates a new <see cref="IMod"/> instance for a game from a file system path.
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="path">The path on the file system of the mod.</param>
        /// <param name="searchModinfoFile">When <see langword="true"/> a modinfo.json file, if present, will be used to initialize;
        /// otherwise a modinfo.json will be ignored</param>
        /// <returns>the Mod instance</returns>
        /// <exception cref="ModException">if <see cref="searchModinfoFile"/> is true and the directory contains any variant files.</exception>
        IMod FromDirectory(IGame game, string path, bool searchModinfoFile);

        /// <summary>
        /// Searches for variant modinfo files and returns new instances for each variant
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="path">The path on the file system of the mod.</param>
        /// <returns></returns>
        /// <exception cref="ModException">if no modinfo variant files are found</exception>
        IEnumerable<IMod> VariantsFromDirectory(IGame game, string path);


        /// <summary>
        /// Creates virtual mods for a game
        /// </summary>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="variants">The variant modinfo data of the virtual mods.</param>
        /// <returns>One or many virtual mods</returns>
        /// <exception cref="System.ArgumentException">if <see cref="variants"/> is empty.</exception>
        IEnumerable<IMod> VirtualVariantsFrom(IGame game, List<ModinfoData> variants);
    }
}