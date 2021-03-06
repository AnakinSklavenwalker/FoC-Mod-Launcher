using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services
{
    public interface IModTypeDetector
    {
        ModType GetModType(IGame game, string path);
    }
}