using System;
using System.Diagnostics;
using PetroGlyph.Games.EawFoc.Client.Arguments;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Client
{
    public class GameStartedEventArgs : EventArgs
    {
        public IGame Game { get; }

        public GameArguments GameArguments { get; }

        public GameBuildType BuildType { get; }

        public Process Process { get; }

        public GameStartedEventArgs(IGame game, GameArguments gameArguments, GameBuildType buildType, Process process)
        {
            Game = game;
            GameArguments = gameArguments;
            BuildType = buildType;
            Process = process;
        }
    }
}