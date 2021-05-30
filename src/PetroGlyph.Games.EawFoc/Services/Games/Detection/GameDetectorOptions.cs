using System.Collections.Generic;
using System.Linq;
using PetroGlyph.Games.EawFoc.Games;
using Validation;

namespace PetroGlyph.Games.EawFoc.Services.Detection
{
    public record GameDetectorOptions(GameType Type)
    {
        private static readonly GamePlatform[] AnyPlatform = {GamePlatform.Undefined};

        private IList<GamePlatform> _targetPlatforms = AnyPlatform;

        public IList<GamePlatform> TargetPlatforms
        {
            get => _targetPlatforms;
            set
            {
                Requires.NotNull(value, nameof(value));
                _targetPlatforms = value;
            }
        }

        internal GameDetectorOptions Normalize()
        {
            if (!TargetPlatforms.Any())
                return this with { TargetPlatforms = new List<GamePlatform> { GamePlatform.Undefined } };
            if (TargetPlatforms.Contains(GamePlatform.Undefined))
                return this with {TargetPlatforms = AnyPlatform}; 
            return this with { TargetPlatforms = TargetPlatforms.Distinct().ToList() };
        }
    }
}