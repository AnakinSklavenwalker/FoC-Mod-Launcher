using System.IO;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Services.Games.Detection
{
    public class GameDetection
    {
        public static GameDetection NotSettedUp => new GameDetection(DetectionResult.NotSettedUp);
        public static GameDetection NotInstalled => new GameDetection(DetectionResult.NotInstalled);

        public GamePlatform FocPlatform { get; }

        public GamePlatform EawPlatform { get; }


        // Setter required, because EA was too dump to fix registry values when they adapted the game for their Origin client.
        public FileInfo? FocExe { get; internal set; }

        public FileInfo? EawExe { get; }

        public DetectionResult Result { get; }

        private GameDetection(DetectionResult result)
        {
            Result = result;
            FocPlatform = GamePlatform.Undefined;
            EawPlatform = GamePlatform.Undefined;
        }

        public GameDetection(FileInfo eawExe, FileInfo focExe)
        {
            Requires.NotNull(eawExe, nameof(eawExe));
            Requires.NotNull(focExe, nameof(focExe));
            Result = DetectionResult.Installed;
            EawExe = eawExe;
            FocExe = focExe;
            FocPlatform = GameTypeHelper.GetGameType(this);
            // TODO: Don't assume eaw platform is the same as foc platform
            EawPlatform = FocPlatform;
        }

        public bool IsError => Result != DetectionResult.Installed || FocExe == null || EawExe == null;

        public override string ToString()
        {
            return $"Game Detection Object: Result:{Result}; " +
                   $"FoC-Platform:{FocPlatform}; FoC:'{FocExe?.FullName}'; " +
                   $"Eaw-Platform:{EawPlatform}; EaW:'{EawExe?.FullName}'";
        }
    }
}
