using System.IO.Abstractions;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;

namespace PetroGlyph.Games.EawFoc.Services
{
    public interface IGameFactory
    {
        public bool TryCreateGame(GameDetectionResult gameDetection, out IGame? game);
        public IGame CreateGame(GameDetectionResult gameDetection);

        public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists, out IGame? game);
        public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location, bool checkGameExists);

        public bool TryCreateGame(IGameIdentity identity, IDirectoryInfo location, out IGame? game);
        public IGame CreateGame(IGameIdentity identity, IDirectoryInfo location);
    }
}