using System.ComponentModel;
using PetroGlyph.Games.EawFoc.Client.Arguments;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Client
{
    public class GameStartingEventArgs : CancelEventArgs
    {
        public IGame Game { get; }

        public GameArguments GameArguments { get; }

        public GameBuildType BuildType { get; }

        public GameStartingEventArgs(IGame game, GameArguments arguments, GameBuildType buildType)
        {
            Game = game;
            GameArguments = arguments;
            BuildType = buildType;
        }
    }
}