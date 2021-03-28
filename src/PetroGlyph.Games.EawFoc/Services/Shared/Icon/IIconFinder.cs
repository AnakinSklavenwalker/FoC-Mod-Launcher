namespace PetroGlyph.Games.EawFoc.Services.Shared.Icon
{
    public interface IIconFinder
    {
        string? FindIcon(IPlayableObject playableObject);
    }
}