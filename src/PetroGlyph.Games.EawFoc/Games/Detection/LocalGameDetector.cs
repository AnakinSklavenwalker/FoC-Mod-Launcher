using System.IO;

namespace PetroGlyph.Games.EawFoc.Games.Detection
{
    public class LocalGameDetector : GameDetector
    {
        protected override GameDetection DetectGamesCore()
        {
            var localResult = FindGamesFromExecutingPath();
            Logger.Trace("Local path game detection result:");
            Logger.Trace("\t" + localResult);
            return localResult;
        }

        private static GameDetection FindGamesFromExecutingPath()
        {
            var currentPath = Directory.GetCurrentDirectory();

            var focExe = new FileInfo(Path.Combine(currentPath, "swfoc.exe"));
            if (!focExe.Exists)
                return GameDetection.NotInstalled;

            var regPath = FocRegistryHelper.Instance.ExePath;
            if (!string.IsNullOrEmpty(regPath) && Path.GetFullPath(currentPath) == Path.GetFullPath(regPath!))
            {
                var eawRegPath = EaWRegistryHelper.Instance.ExePath;
                return new GameDetection(new FileInfo(eawRegPath!), focExe);
            }

            var gameType = GameTypeHelper.GetGameType(focExe);
            if (!Eaw.FindInstallationRelativeToFoc(focExe, gameType, out var eawPath))
            {
                var eawRegPath = EaWRegistryHelper.Instance.ExePath;
                return new GameDetection(new FileInfo(eawRegPath!), focExe);
            }

            return new GameDetection(eawPath!, focExe);
        }
    }
}