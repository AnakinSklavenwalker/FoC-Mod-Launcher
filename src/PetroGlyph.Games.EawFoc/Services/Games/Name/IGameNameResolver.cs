using System.Globalization;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Name
{
    public interface IGameNameResolver
    {
        string ResolveName(IGameIdentity game);

        string ResolveName(IGameIdentity game, CultureInfo culture);
    }
}