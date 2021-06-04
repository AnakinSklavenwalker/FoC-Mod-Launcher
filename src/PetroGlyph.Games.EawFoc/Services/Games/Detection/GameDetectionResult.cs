using System;
using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public class GameDetectionResult
    {
        public IGameIdentity GameIdentity { get; }

        public IDirectoryInfo? GameLocation { get; }

        public bool InitializationRequired { get; }

        public Exception? Error { get; }

        private GameDetectionResult(GameType type)
        {
            GameIdentity = new GameIdentity(type, GamePlatform.Undefined);
        }

        public GameDetectionResult(IGameIdentity gameIdentity, IDirectoryInfo location)
        {
            Requires.NotNull(location, nameof(location));
            GameIdentity = gameIdentity;
            GameLocation = location;
        }
        
        internal GameDetectionResult(GameType type, Exception error) : this(type)
        {
            Error = error;
        }

        private GameDetectionResult(GameType type, bool initializationRequired) : this(type)
        {
            InitializationRequired = initializationRequired;
        }

        public static GameDetectionResult NotInstalled(GameType type)
        {
            return new(type);
        }

        public static GameDetectionResult RequiresInitialization(GameType type)
        {
            return new(type, true);
        }
    }
}