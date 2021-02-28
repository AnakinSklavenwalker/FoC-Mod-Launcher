using System;
using System.Threading;
using System.Threading.Tasks;

namespace PetroGlyph.Games.EawFoc.Games
{
    internal static class LauncherSteamHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string WaitMessage = "Please wait while setting up the games for steam";

        public static async Task SetupSteamGamesAsync(CancellationToken cancellationToken = default)
        {
            var data = new WaitDialogProgressData(WaitMessage, null, null, true);
            using var s = WaitDialogFactory.Instance.StartWaitDialog("FoC Launcher", data, TimeSpan.FromSeconds(2));
            var linkedToken =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, s.UserCancellationToken);
            await SetupSteamGameCoreAsync(linkedToken.Token, s.Progress);
        }

        private static async Task SetupSteamGameCoreAsync(CancellationToken token, IProgress<WaitDialogProgressData> progress)
        {
            Logger.Trace("Atempting to configure the Steam Version");
            var eawSlim = new EawSteamGameSlim();
            try
            {
                progress.Report(new WaitDialogProgressData(WaitMessage, "Waiting until Steam is started...", isCancelable:true));
                if (!SteamClient.Instance.IsRunning)
                    SteamClient.Instance.StartSteam();
                await SteamClient.Instance.WaitSteamRunningAndLoggedInAsync(token).ConfigureAwait(false);
                eawSlim.Close();
                eawSlim.StartGame();
                using var processWaitTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                processWaitTokenSource.CancelAfter(120_000);

                Logger.Trace("Waiting max. two minutes for the game to be started");
                progress.Report(new WaitDialogProgressData(WaitMessage, "Waiting until EaW is started...", isCancelable: true));
                await ProcessCreationListener.WaitProcessCreatedAsync(EawSteamGameSlim.Executable, processWaitTokenSource.Token).ConfigureAwait(false);
                progress.Report(new WaitDialogProgressData(WaitMessage, "Waiting until EaW is closed..."));
                if (!processWaitTokenSource.IsCancellationRequested)
                {
                    Logger.Trace($"{EawSteamGameSlim.Executable} was started. Waiting another second. Just to be sure");
                    await Task.Delay(1000, token).ConfigureAwait(false);
                }
                processWaitTokenSource.Dispose();
            }
            catch (OperationCanceledException)
            {
                Logger.Trace("The procedure was cancelled or reached the timeout");
            }
            finally
            {
                Logger.Trace("Close the application again if it was started.");
                eawSlim.Close();
            }
        }
    }
}