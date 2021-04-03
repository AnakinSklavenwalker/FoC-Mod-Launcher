namespace PetroGlyph.Games.EawFoc.Games
{
    public sealed record GameIdentity(GameType Type, GamePlatform Platform) : IGameIdentity;
}