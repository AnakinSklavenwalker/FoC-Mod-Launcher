using System;

namespace PetroGlyph.Games.EawFoc.Games
{
    public interface IGame : IModContainer, IPhysicalPlayableObject, IGameIdentity, IEquatable<IGame>
    {
        /// <summary>
        /// Checks whether this game instance exists on this machine
        /// </summary>
        /// <returns><see langword="true"/> when the game exists; <see langword="false"/> otherwise.</returns>
        bool Exists();
    }
}