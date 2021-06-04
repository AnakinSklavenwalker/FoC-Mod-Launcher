using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games.Registry;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public sealed class RegistryGameDetector : GameDetector, IDisposable
    {
        private readonly IGameRegistry _registry;

        public RegistryGameDetector(IGameRegistry registry, bool tryHandleInitialization, IServiceProvider serviceProvider) 
            : base(serviceProvider, tryHandleInitialization)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotSupportedException("This instance is only supported on windows systems");
            Requires.NotNull(registry, nameof(registry));
            _registry = registry;
        }

        private protected override GameLocationData FindGameLocation(GameDetectorOptions options)
        {
            Logger?.LogDebug("Attempting to fetch the game from the registry.");
            if (!_registry.Exits)
            {
                Logger?.LogDebug("The Game's Registry does not exist.");
                return new GameLocationData();
            }

            if (_registry.Version is null)
            {
                Logger?.LogDebug("Registry-Key found, but games are not initialized.");
                return new GameLocationData {InitializationRequired = true};
            }

            var installPath = _registry.InstallPath;
            if (installPath is not null) 
                return new GameLocationData {Location = installPath};


            var e = new InvalidOperationException("Could not get instal location from registry path.");
            Logger?.LogDebug(e, e.Message);
            throw e;
        }

        public void Dispose()
        {
            _registry.Dispose();
        }
    }
}