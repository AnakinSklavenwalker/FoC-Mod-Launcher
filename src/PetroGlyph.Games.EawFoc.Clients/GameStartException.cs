using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Client
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