using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc
{
    /// <summary>
    /// Flag interface to identify an object which can be played in some sort.
    /// Known types are <see cref="IGame"/> and <see cref="IMod"/>
    /// </summary>
    public interface IPlayableObject
    {
        /// <summary>
        /// Collection of available languages for this instance.
        /// </summary>
        ICollection<ILanguageInfo> InstalledLanguages { get; }

        /// <summary>
        /// Path to an icon which shall get used as a taskbar item when this instance is started.
        /// </summary>
        string? IconFile { get; }
    }
}
