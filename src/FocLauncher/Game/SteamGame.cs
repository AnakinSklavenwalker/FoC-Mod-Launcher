﻿using System;
using System.IO;
using FocLauncher.Mods;
using FocLauncher.Threading;
using FocLauncher.Utilities;
using FocLauncher.WaitDialog;

namespace FocLauncher.Game
{
    public sealed class SteamGame : AbstractFocGame
    {
        public const string GameconstantsUpdateHash = "b0818f73031b7150a839bb83e7aa6187";

        public const int EmpireAtWarSteamId = 32470;
        public const int ForcesOfCorruptionSteamId = 32472;


        protected override string GameExeFileName => "StarwarsG.exe";
        protected override string DebugGameExeFileName => "StarwarsI.exe";

        protected override int DefaultXmlFileCount => 1;

        public override GameType Type => GameType.SteamGold;

        public override string Name => "Forces of Corruption (Steam)";

        public SteamGame(string gameDirectory) : base(gameDirectory)
        {
        }

        public override bool IsPatched()
        {
            var gameConstantsFilePath = Path.Combine(GameDirectory, @"Data\XML\GAMECONSTANTS.XML");
            if (!File.Exists(gameConstantsFilePath))
                return false;
            var hashProvider = new HashProvider();
            return hashProvider.GetFileHash(gameConstantsFilePath) == GameconstantsUpdateHash;
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

        public override bool HasDebugBuild()
        {
            return File.Exists(Path.Combine(GameDirectory, DebugGameExeFileName));
        }
    }
}
