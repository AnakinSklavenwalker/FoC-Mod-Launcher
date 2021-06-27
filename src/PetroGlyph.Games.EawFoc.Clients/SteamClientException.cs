using System;

namespace PetroGlyph.Games.EawFoc.Client
{
    public class SteamClientException : Exception
    {
        public SteamClientException(string message) : base(message)
        {
        }
    }
}
