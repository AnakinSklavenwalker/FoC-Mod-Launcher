using System.Collections.Generic;
using System.IO.Abstractions;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Games.Arguments
{
    public record GameArguments
    {
        public uint? LocalPort { get; set; }

        public uint? Monitor { get; set; }

        public bool? Windowed { get; set; }

        public object? MCE { get; set; }

        public uint? ScreenWidth { get; set; }
        public uint? ScreenHeight { get; set; }

        public bool? LowRAM { get; set; }
        public uint? FpsCap { get; set; }
        public bool? SafeMode { get; set; }



        public IDirectoryInfo? FallbackPath { get; set; }

        /// <summary>
        /// An ordered list of Mod that shall get executed.
        /// The first mod in the list resembles the mod you actually want to launch, and the last mod in the list resembles the most base mod.
        /// <remarks>The order of this list is equal to the order of CLI arguments.</remarks>
        /// </summary>
        public IList<IMod>? Mods { get; set; }

        public ILanguageInfo? Language { get; set; }
    }
}