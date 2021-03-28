using System;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Icon
{
    public class CompositeIconFinder : IIconFinder
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