using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services
{
    public interface IModNameResolver
    {
        string ResolveName(IModReference modReference);
    }
}