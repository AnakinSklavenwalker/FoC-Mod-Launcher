using System;

namespace PetroGlyph.Games.EawFoc.Games
{
    public interface IGame : IModContainer, IPhysicalPlayableObject, IGameIdentity, IEquatable<IGame>
    {
        string Name { get; }

        void Setup(GameSetupOptions setupMode);
    }
}