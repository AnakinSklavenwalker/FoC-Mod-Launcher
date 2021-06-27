using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Client
{
    internal sealed class EawSteamGameSlim
    {
        public static string ProcessName = "StarWarsG";
        public static string Executable = "StarWarsG.exe";

        public bool Close()
        {
            var process = ProcessHelper.FindProcess(ProcessName);
            if (process == null)
                return false;
            process.Kill();
            return true;
        }

        public void StartGame()
        {
            SteamClient.Instance.StartGame(SteamGameEaw.EmpireAtWarSteamId);
        }
    }
}