using System;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class PetroglyphGameException : PetroglyphException
    {
        public PetroglyphGameException()
        {
        }

        public PetroglyphGameException(string message) : base(message)
        {
        }

        public PetroglyphGameException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}