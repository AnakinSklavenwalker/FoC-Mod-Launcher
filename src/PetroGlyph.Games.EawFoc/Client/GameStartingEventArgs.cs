using System.ComponentModel;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Games.Arguments;

namespace PetroGlyph.Games.EawFoc.Client
{
    public class GameStartingEventArgs : CancelEventArgs
    {
        public IGame Game { get; }

        public GameCommandArguments GameArguments { get; }

        public GameBuildType BuildType { get; }

        public GameStartingEventArgs(IGame game, GameCommandArguments arguments, GameBuildType buildType)
        {
            Game = game;
            GameArguments = arguments;
            BuildType = buildType;
        }
    }
}