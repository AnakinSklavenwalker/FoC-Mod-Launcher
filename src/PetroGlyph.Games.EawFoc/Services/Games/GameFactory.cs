using System;
using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Name;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services
{
    public class GameFactory : IGameFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GameFactory(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        public IGame CreateGame(GameDetectionResult gameDetection)
        {
            Requires.NotNull(gameDetection, nameof(gameDetection));
            if (gameDetection.GameLocation is null)
                throw new ArgumentException("Location must not be null");
            return CreateGame(gameDetection.GameIdentity, gameDetection.GameLocation, false);
        }


        public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists)
        {
            Requires.NotNull(location, nameof(location));
            if (identity.Platform == GamePlatform.Undefined)
                throw new ArgumentException("Cannot create a game with undefined platform");

            var name = new EnglishGameNameResolver().ResolveName(identity);
            var game = new PetroglyphStarWarsGame(identity, location, name, _serviceProvider);
            if (checkGameExists && !game.Exists())
                throw new PetroglyphGameException($"Game does not exists at location: {location}");

            return game;
        }
        
        public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location)
        {
            var game = CreateGame(identity, location, true);
            return game;
        }

        public bool TryCreateGame(GameDetectionResult gameDetection, out IGame? game)
        {
            game = null;
            try
            {
                game = CreateGame(gameDetection);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, out IGame? game)
        {
            game = null;
            try
            {
                game = CreateGame(identity, location, checkGameExists);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, out IGame? game)
        {
            game = null;
            try
            {
                game = CreateGame(identity, location);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}