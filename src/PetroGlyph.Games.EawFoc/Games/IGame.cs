using System;

namespace PetroGlyph.Games.EawFoc.Games
{
    public interface IGame : IModContainer, IPhysicalPlayableObject, IGameIdentity, IEquatable<IGame>
    {
        string Name { get; }

        bool Exists();

        void Setup(GameSetupOptions setupMode);
    }
}