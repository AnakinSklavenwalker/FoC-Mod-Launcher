using System;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class GameStartException : PetroglyphGameException
    {
        public GameStartException(string message) : base(message)
        {
        }

        public GameStartException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}