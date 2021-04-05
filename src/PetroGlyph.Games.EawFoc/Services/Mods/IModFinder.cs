using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Mods
{
    public interface IModFinder
    {
        IEnumerable<IModReference> FindMods();
    }
}
