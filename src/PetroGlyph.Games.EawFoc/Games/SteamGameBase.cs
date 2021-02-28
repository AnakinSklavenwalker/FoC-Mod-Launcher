using System;
using System.IO;

namespace PetroGlyph.Games.EawFoc.Games
{
    public abstract class SteamGameBase : PetroglyphGame, IDebugable
    {
        public bool DebugBuildExists => File.Exists(Path.Combine(Directory.FullName, DebugExeName));

        protected virtual string DebugExeName => "StarwarsI.exe";

        protected override string GameExeFileName => "StarWarsG.exe";

        protected string DebugExePath => Path.Combine(Directory.FullName, DebugExeName);

        
        protected SteamGameBase(DirectoryInfo gameDirectory) : base(gameDirectory)
        {
        }

        public void DebugGame(GameCommandArguments? args = null, string? iconFile = null, bool fallbackToNormal = true)
        {
            GameStartInfo startInfo;
            if (DebugBuildExists)
            {
                var exeFile = new FileInfo(DebugExePath);
                startInfo = new GameStartInfo(exeFile, GameBuildType.Debug);
            }
            else
            {
                if (!fallbackToNormal)
                    throw new GameStartException($"Unable to find debug executable {DebugExeName}");

                var exeFile = new FileInfo(Path.Combine(Directory.FullName, GameExeFileName));
                startInfo = new GameStartInfo(exeFile, GameBuildType.Release);
            }
            args ??= new GameCommandArguments();
            StartGame(args, startInfo, iconFile);
        }

        protected override void OnGameStarting(GameStartingEventArgs args)
        {
            if (!SteamClient.Instance.IsRunning)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    var data = new WaitDialogProgressData("Waiting for Steam...", isCancelable: true);
                    using var s = WaitDialogFactory.Instance.StartWaitDialog("FoC Launcher", data, TimeSpan.FromSeconds(2));
                    SteamClient.Instance.StartSteam();
                    try
                    {
                        await SteamClient.Instance.WaitSteamRunningAndLoggedInAsync(s.UserCancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        args.Cancel = true;
                    }
                });
            }
            base.OnGameStarting(args);
        }
    }
}