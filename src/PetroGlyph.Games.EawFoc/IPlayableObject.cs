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
        /// The name of this playable object.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Set of available languages for this instance.
        /// </summary>
        ISet<ILanguageInfo> InstalledLanguages { get; }

        /// <summary>
        /// Absolute or relative path to an icon of this instance.
        /// </summary>
        string? IconFile { get; }
    }
}
