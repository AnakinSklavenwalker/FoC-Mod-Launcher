using System;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Detection.Platform
{
    internal static class GamePlatformIdentifierFactory
    {
        public static IGamePlatformValidator Create(GamePlatform platform, IServiceProvider serviceProvider)
        {
            return platform switch
            {
                GamePlatform.Undefined => throw new NotSupportedException($"{GamePlatform.Undefined} is not supported."),
                GamePlatform.Disk => new DiskGameValidator(serviceProvider),
                GamePlatform.DiskGold => new DiskGoldGameValidator(serviceProvider),
                GamePlatform.SteamGold => new SteamGameValidator(serviceProvider),
                GamePlatform.GoG => new GogGameValidator(serviceProvider),
                GamePlatform.Origin => new OriginGameValidator(serviceProvider),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
            };
        }
    }
}