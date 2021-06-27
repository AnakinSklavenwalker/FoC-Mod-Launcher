using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public interface IModReferenceFinder
    {
        ISet<IModReference> FindMods(IGame game);
    }
}