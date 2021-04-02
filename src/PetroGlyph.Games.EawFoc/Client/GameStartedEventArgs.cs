using System;
using System.Diagnostics;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Arguments;

namespace PetroGlyph.Games.EawFoc.Client
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