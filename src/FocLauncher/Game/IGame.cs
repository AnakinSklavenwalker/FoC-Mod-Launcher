﻿using System;
using System.Diagnostics;

namespace FocLauncher.Game
{
    public interface IGame : IPetroglyhGameableObject
    {
        event EventHandler<Process> GameStarted;

        event EventHandler<GameStartingEventArgs> GameStarting;

        event EventHandler GameClosed;

        GameType Type { get; }

        /// <summary>
        /// Returns the full Path of the Games Root Directory
        /// </summary>
        string GameDirectory { get; }

        ///// <summary>
        ///// Returns the name of the Game
        ///// </summary>
        //string Name { get; }

        /// <summary>
        /// Contains Data of the Process
        /// </summary>
        GameProcessWatcher GameProcessWatcher { get; }

        /// <summary>
        /// Checks whether a mod exists
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// Plays the default game
        /// </summary>
        bool PlayGame(string? iconFile = null);

        /// <summary>
        /// Plays the game with the mod
        /// </summary>
        /// <param name="args">Arguments which specify more options, including the mod, that shall get used.</param>
        /// <returns><c>true</c> when the game process was invoked; <c>false</c> otherwise. E.g. when the procedure was cancelled.</returns>
        bool PlayGame(GameCommandArguments args, string? iconFile = null);

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
    }
}