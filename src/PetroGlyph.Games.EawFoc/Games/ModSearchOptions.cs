using System;

namespace PetroGlyph.Games.EawFoc.Games
{
    /// <summary>
    /// Flag to specify mod search behavior
    /// </summary>
    [Flags]
    public enum ModSearchOptions
    {
        /// <summary>
        /// Searches for mods which are already added to <see cref="IModContainer.Mods"/>
        /// </summary>
        Registered = 1,
        /// <summary>
        /// Searches for mods on the file system. More details are implementation specific. 
        /// </summary>
        FileSystem = 2,
    }
}