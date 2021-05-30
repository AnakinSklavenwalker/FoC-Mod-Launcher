using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    public sealed class GamePlatformIdentifier
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;

        private static readonly GamePlatform[] GamePlatformsOrdered = {
            GamePlatform.SteamGold
        };

        public GamePlatformIdentifier(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(GamePlatformIdentifier));
        }

        public GamePlatform GetGamePlatform(GameType type, ref IDirectoryInfo location)
        {
            _logger?.LogDebug("Validating game platform:");
            
            foreach (var platform in GamePlatformsOrdered)
            {
                var validator = GamePlatformIdentifierFactory.Create(platform, _serviceProvider);
                _logger?.LogDebug($"Validating location for {platform}...");
                if (!validator.IsPlatform(type, ref location)) 
                    continue;

                _logger?.LogDebug($"Game location was identified as {platform}");
                return platform;
            }

            _logger?.LogDebug("Unable to determine which which platform the game has.");
            return GamePlatform.Undefined;
        }
    }
}