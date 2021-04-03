namespace PetroGlyph.Games.EawFoc.Games
{
    public interface IGameIdentity
    {
        public GameType Type { get; }

        public GamePlatform Platform { get; }
    }
}