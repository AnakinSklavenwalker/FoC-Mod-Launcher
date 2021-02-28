using System;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class SteamClientException : Exception
    {
        public SteamClientException(string message) : base(message)
        {
        }
    }
}
