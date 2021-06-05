using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    internal static class GamePlatformIdentifierFactory
    {
        public static ISpecificPlatformIdentifier Create(GamePlatform platform, IServiceProvider serviceProvider)
        {
            return platform switch
            {
                GamePlatform.Undefined => throw new NotSupportedException($"{GamePlatform.Undefined} is not supported."),
                GamePlatform.Disk => new DiskIdentifier(serviceProvider),
                GamePlatform.DiskGold => new DiskGoldIdentifier(serviceProvider),
                GamePlatform.SteamGold => new SteamIdentifier(serviceProvider),
                GamePlatform.GoG => new GogIdentifier(serviceProvider),
                GamePlatform.Origin => new OriginIdentifier(serviceProvider),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }
    }
}