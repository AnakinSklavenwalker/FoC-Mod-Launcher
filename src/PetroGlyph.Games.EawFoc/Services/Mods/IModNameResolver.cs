using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Mods
{
    public interface IModNameResolver
    {
        string ResolveName(IModReference modReference);
    }
}