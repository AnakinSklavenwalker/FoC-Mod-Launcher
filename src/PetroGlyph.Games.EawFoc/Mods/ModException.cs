using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Mods
{
    public class ModException : PetroglyphException
    {
        public ModException()
        {
        }

        public ModException(string message) : base(message)
        {
        }

        public ModException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}