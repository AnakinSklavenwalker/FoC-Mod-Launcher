using System;
using System.Collections.Generic;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services;

namespace PetroGlyph.Games.EawFoc.Games
{
    public interface IGame : IPlayableObject, IModContainer, IHasDirectory
    {
        event EventHandler<GameStartedEventArgs> GameStarted;

        event EventHandler<GameStartingEventArgs> GameStarting;

        event EventHandler GameClosed;

        GameType Type { get; }
        
        /// <summary>
        /// Contains Data of the Process
        /// </summary>
        GameProcessWatcher GameProcessWatcher { get; }


        IModTypeDetector ModTypeDetector { get; }

        /// <summary>
        /// Checks whether a mod exists
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// Plays the game with the mod
        /// </summary>
        /// <param name="args">Arguments which specify more options, including the mod, that shall get used.</param>
        /// <param name="iconFile">Path to the icon file that shall get used.</param>
        /// <exception cref="GameStartException">When the game instance could not get started due to an error.
        /// If the operation was cancelled by an handler of <see cref="GameStarting"/> no exception will be thrown.</exception>
        void PlayGame(GameCommandArguments? args = null, string? iconFile = null);

        /// <summary>
        /// Checks if the patch is installed
        /// </summary>
        /// <returns></returns>
        bool IsPatched();

        /// <summary>
        /// Determines whether the game AI files are cleared
        /// </summary>
        /// <returns>
        ///   <see langword="true"/> if game is clear; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsGameAiClear();

        /// <summary>
        /// Determines whether the the given language is installed
        /// </summary>
        /// <param name="language">The english name of the language</param>
        /// <returns><see langword="true"/> if game is the language is available; otherwise, <see langword="false"/>.</returns>
        bool IsLanguageInstalled(string language);

        /// <summary>
        /// Searches for mods for this game on disk and creates a new instance of an <see cref="IMod"/> when necessary
        /// </summary>
        /// <param name="add">When set to <c>true</c> the mods will get added to the <see cref="IGame.Mods"/> collection if not present already.
        /// This will raise the <see cref="IModContainer.ModsCollectionModified"/> event </param>
        /// <returns>An unsorted collection of all mods found.</returns>
        ICollection<IMod> GetPhysicalMods(bool add);

        IMod CreateMod(ModCreationDelegate modCreation, bool add);

        bool TryCreateMod(ModCreationDelegate modCreation, bool add, out IMod mod);

        void Setup(GameSetupOptions setupMode);
    }
}