using System;
using System.Diagnostics;

namespace PetroGlyph.Games.EawFoc.Games
{
    public class GameStartedEventArgs : EventArgs
    {
        public IGame Game { get; }

        public GameCommandArguments GameArguments { get; }

        public GameBuildType BuildType { get; }

        public Process Process { get; }

        public GameStartedEventArgs(IGame game, GameCommandArguments gameArguments, GameBuildType buildType, Process process)
        {
            Game = game;
            GameArguments = gameArguments;
            BuildType = buildType;
            Process = process;
        }
    }
}