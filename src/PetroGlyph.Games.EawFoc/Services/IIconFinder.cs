namespace PetroGlyph.Games.EawFoc.Services
{
    public interface IIconFinder
    {
        string? FindIcon(IPlayableObject playableObject);
    }

    public class DefaultIconFinder : IIconFinder
    {
        public string? FindIcon(IPlayableObject playableObject)
        {
            throw new System.NotImplementedException();
        }
    }
}