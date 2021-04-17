using System;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Games.Icon;
using PetroGlyph.Games.EawFoc.Services.Mods.Icon;

namespace PetroGlyph.Games.EawFoc.Services.Shared.Icon
{
    public class CompositeIconFinder : IGameIconFinder, IModIconFinder
    {
        public static readonly IIconFinder GameIconFinder = new GameIconFinder();
        public static readonly IIconFinder ModIconFinder = new ModIconFinder();

        public string? FindIcon(IPlayableObject playableObject)
        {
            return playableObject switch
            {
                IGame => GameIconFinder.FindIcon(playableObject),
                IMod => ModIconFinder.FindIcon(playableObject),
                _ => throw new NotSupportedException($"{playableObject.GetType().Name} is not supported.")
            };
        }
    }
}