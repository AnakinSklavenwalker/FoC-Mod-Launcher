using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Games
{
    public sealed class GameCommandArguments
    {
        public bool IgnoreAsserts { get; set; }

        public bool NoArtProcess { get; set; }

        public bool Windowed { get; set; }

        /// <summary>
        /// An ordered list of Mod that shall get executed.
        /// The first mod in the list resembles the mod you actually want to launch, and the last mod in the list resembles the most base mod.
        /// <remarks>The order of this list is equal to the order of CLI arguments.</remarks>
        /// </summary>
        public IList<IMod>? Mods { get; set; }

        public string? Language { get; set; }
    }
}