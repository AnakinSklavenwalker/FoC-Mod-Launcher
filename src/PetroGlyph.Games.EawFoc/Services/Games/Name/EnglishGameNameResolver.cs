using System;
using System.Globalization;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Name
{
    public class EnglishGameNameResolver : IGameNameResolver
    {
        public string ResolveName(IGameIdentity game)
        {
            var gameName = game.Type == GameType.EaW
                ? PetroglyphStarWarsGameConstants.EmpireAtWarEnglishNameShort
                : PetroglyphStarWarsGameConstants.ForcesOfCorruptionEnglishNameShort;
            var platform = game.Platform.ToString();
            return $"{gameName} ({platform})";
        }

        public string ResolveName(IGameIdentity game, CultureInfo culture)
        {
            throw new NotSupportedException("Not supported");
        }
    }
}