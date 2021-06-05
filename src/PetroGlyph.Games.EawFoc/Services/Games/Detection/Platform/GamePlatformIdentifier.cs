using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    public sealed class GamePlatformIdentifier
    {
        private readonly IList<GamePlatform> _orderedPlatforms;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;

        public static readonly GamePlatform[] DefaultGamePlatformOrdering = {
            // Sorted by (guessed) number of user installations
            GamePlatform.SteamGold, 
            GamePlatform.GoG,
            GamePlatform.Origin,

            // Disk Platforms are lowest priority, because their heuristics are less precise/specific than the above platforms. 
            // This could lead to false positives. (E.g.: Game is "Steam", but detection was "DiskGold")
            GamePlatform.DiskGold,
            GamePlatform.Disk
        };


        public GamePlatformIdentifier(IServiceProvider serviceProvider) : this(DefaultGamePlatformOrdering, serviceProvider)
        {
        }

        public GamePlatformIdentifier(IList<GamePlatform> searchOrder, IServiceProvider serviceProvider)
        {
            Requires.NotNull(searchOrder, nameof(serviceProvider));
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _orderedPlatforms = searchOrder;
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(GamePlatformIdentifier));
        }

        public GamePlatform GetGamePlatform(GameType type, ref IDirectoryInfo location)
        {
            _logger?.LogDebug("Validating game platform:");
            
            foreach (var platform in _orderedPlatforms)
            {
                if (platform == GamePlatform.Undefined)
                    continue;
                
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