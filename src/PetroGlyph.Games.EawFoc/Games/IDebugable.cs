namespace PetroGlyph.Games.EawFoc.Games
{
    public interface IDebugable
    {
        bool DebugBuildExists { get; }

        /// <summary>
        /// Plays the game using the debug executable. 
        /// </summary>
        /// <param name="args">Arguments which specify more options, including the mod, that shall get used.</param>
        /// <param name="iconFile">Path to the icon file that shall get used.</param>
        /// <param name="fallbackToNormal">When no debug executable was found, the method falls back using the normal executable.</param>
        /// <exception cref="GameStartException">When the game instance could not get started due to an error.</exception>
        void DebugGame(GameCommandArguments? args = null, string? iconFile = null, bool fallbackToNormal = true);
    }
}