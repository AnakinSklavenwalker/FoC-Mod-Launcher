using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher;
using FocLauncher.Threading;
using FocLauncherHost.Dialogs;
using FocLauncherHost.Product;
using FocLauncherHost.Update;
using Microsoft.VisualStudio.Threading;
using NLog;
using ProductMetadata;
using ProductMetadata.Manifest;
using ProductUpdater;
using ProductUpdater.Configuration;
using ProductUpdater.New.Update;
using ProductUpdater.New.Update.Service;


namespace FocLauncherHost
{
    public class HostApplication : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan WaitSplashDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan WaitProgressDelay = TimeSpan.FromSeconds(WaitSplashDelay.Seconds + 2);

        private int _shouldShowSplashScreen = 1;

        private readonly AsyncManualResetEvent _canCloseApplicationEvent = new(false, true);

        internal static ManualResetEvent SplashVisibleResetEvent { get; } = new(false);

        internal SplashScreen SplashScreen { get; }

        private readonly IServiceProvider _services;

        internal HostApplication()
        {
            MainWindow = SplashScreen = new SplashScreen();
            SplashScreen.Launcher = FocLauncherInformation.Instance;

            _services = new LauncherServiceFactory().CreateLauncherServices();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            WaitAndShutdownAsync().Forget();
            UpdateLauncherAsync().Forget();
        }
        
        internal async Task UpdateLauncherAsync()
        {
            try
            {
                Task.Delay(WaitSplashDelay).ContinueWith(async _ => await ShowMainWindowAsync(), default,
                    TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext()).Forget();

                if (! await ConnectionManager.Instance.CheckConnectionAsync())
                    return;

                await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    SplashScreen.ProgressText = "Please wait while the launcher is downloading an update.";
                    SetWhenWaitDialogIsShownAsync(WaitProgressDelay, SplashScreen.CancellationToken).Forget();
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(SplashScreen.CancellationToken);

                    UpdateOperationResult? updateInformation = null;
                    try
                    {
                        var i = FocLauncherInformation.Instance;

                        var us = new LauncherUpdateSearchSettings
                        {
                            UpdateMode = UpdateMode.Explicit,
                            UpdateBranch = ProductReleaseType.Stable
                        };

                        var ps = new LauncherProductService(new LauncherComponentBuilder(),
                            new LauncherUpdateManifestBuilder(new LauncherManifestFinder(us),
                                new LauncherToProductCatalogConverter(new FileSystem()), _services), new FileSystem());

                        var cs = new UpdateCheckService(ps, _services);

                        var r = new UpdateRequest(
                            new ProductManifestLocation(
                                ps.CreateProductReference(null, ProductReleaseType.Stable),
                                    new Uri("file://c:/Test/text.xml", UriKind.Absolute)));

                        var updateCheckInformation = await cs.CheckForUpdates(r, cts.Token);

                        if (updateCheckInformation.IsUpdateAvailable)
                        {
                            using var updater = new UpdateService(ps.GetCurrentInstance(), new UpdateConfiguration());
                            updateInformation = await updater.UpdateAsync(updateCheckInformation.UpdateCatalog!, cts.Token);
                        }

                        Logger.Info($"Finished automatic update with result {updateInformation}");
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.Info("Download Operation cancelled");
                    }
                    finally
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        cts.Dispose();
                    }

                    ReportUpdateResult(updateInformation);
                });
            }
            catch (Exception e)
            {
                LogAndShowException(e);
            }
            finally
            {
                _canCloseApplicationEvent.Set();
            }
        }
        
        private async Task SetWhenWaitDialogIsShownAsync(TimeSpan delay, CancellationToken token)
        {
            await Task.Delay(delay, token);
            if (token.IsCancellationRequested)
                return;
            SplashScreen.IsProgressVisible = true;
        }

        private void ReportUpdateResult(UpdateOperationResult? updateInformation)
        {
            if (updateInformation != null)
            {
                if (updateInformation.Result == UpdateResult.Failed && SplashScreen.IsProgressVisible || FocLauncherInformation.Instance.BuildType == BuildType.Debug)
                {
                    Interlocked.Exchange(ref _shouldShowSplashScreen, 0);
                    SplashScreen.Cancelable = false;
                    switch (updateInformation.Result)
                    {
                        case UpdateResult.Failed:
                            SplashScreen.ProgressText = "Update Failed";
                            new UpdateResultDialog("Update Failed", updateInformation.Error?.Message ?? "Unknown reason").ShowDialog();
                            break;
                        case UpdateResult.Success:
                            SplashScreen.ProgressText = "Update finished";
                            new UpdateSuccessDialog().ShowDialog();
                            break;
                        case UpdateResult.SuccessRestartRequired:
                            SplashScreen.ProgressText = "Update requires restart";
                            break;
                        case UpdateResult.Cancelled:
                            SplashScreen.ProgressText = "Update cancelled";
                            break;
                    }
                }
                
            }
        }

        private async Task WaitAndShutdownAsync()
        {
            await _canCloseApplicationEvent.WaitAsync();
            await HideSplashScreenAnimatedAsync();
            Shutdown();
        }

        private async Task ShowMainWindowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (Interlocked.CompareExchange(ref _shouldShowSplashScreen, 0, 1) == 1)
            {
                MainWindow?.Show();
                SplashVisibleResetEvent.Set();
            }
        }

        private async Task HideSplashScreenAnimatedAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (SplashScreen.IsVisible)
            {
                SplashScreen.IsProgressVisible = false;
                await Task.Delay(500);
                await SplashScreen.HideAnimationAsync();
            }
        }

        private static void LogAndShowException(Exception e)
        {
            Logger.Error(e, e.Message);
            var realException = e.TryGetWrappedException() ?? e;
            new ExceptionWindow(realException).ShowDialog();
        }
    }
}