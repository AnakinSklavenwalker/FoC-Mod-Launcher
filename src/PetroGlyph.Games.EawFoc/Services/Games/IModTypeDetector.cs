using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Games
{
    public interface IModTypeDetector
    {
        ModType GetModType(IGame game, string path);
    }
}