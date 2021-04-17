using System.Collections.Generic;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services
{
    public interface IModFinder
    {
        IEnumerable<IModReference> FindMods();
    }
}
