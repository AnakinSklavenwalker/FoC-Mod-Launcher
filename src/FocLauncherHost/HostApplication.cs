using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher;
using FocLauncher.Threading;
using FocLauncherHost.Dialogs;
using Microsoft;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using NLog;
using TaskBasedUpdater;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New;

namespace FocLauncherHost
{

    internal class UpdaterServiceFactory
    {
        public IServiceProvider CreateServiceProvider(IServiceProvider services)
        {
            Requires.NotNull(services, nameof(services));
            var serviceCollection = new ServiceCollection();

            return serviceCollection.BuildServiceProvider();
        }
    }


    internal class FocLauncherUpdater : IDisposable
    {
        private readonly IServiceProvider _services;
        private IUpdateCatalog? _updateCatalog;
        private IUpdateManager? _updateManager;

        public FocLauncherUpdater(IServiceProvider services)
        {
            Requires.NotNull(services, nameof(services));
            _services = services;
        }

        public IUpdateResultInformation CheckAndUpdate(CancellationToken token)
        {
            if (!IsUpdateAvailable(token))
                return UpdateResultInformation.NoUpdate;
            if (_updateCatalog is null)
                return new UpdateResultInformation
                {
                    Result = UpdateResult.NoUpdate,
                    Message = "Unable to find update manifest."
                };
            return Update(token);
        }

        private IUpdateResultInformation Update(CancellationToken token)
        {
            var updater =
                new NewUpdateManager(new ServiceCollection().BuildServiceProvider(), new UpdateConfiguration());

            if (_updateCatalog is null)
                throw new InvalidOperationException("Catalog cannot be null");

            updater.Update(_updateCatalog, token);

            return UpdateResultInformation.Success;
        }

        private bool IsUpdateAvailable(CancellationToken token)
        {
            return false;
        }

        public void Dispose()
        {
            _updateManager.Dispose();
        }
    }



    public class HostApplication : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan WaitSplashDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan WaitProgressDelay = TimeSpan.FromSeconds(WaitSplashDelay.Seconds + 2);

        private int _shouldShowSplashScreen = 1;

        private readonly AsyncManualResetEvent _canCloseApplicationEvent = new AsyncManualResetEvent(false, true);

        internal static ManualResetEvent SplashVisibleResetEvent { get; } = new ManualResetEvent(false);

        internal SplashScreen SplashScreen { get; }

        internal HostApplication()
        {
            MainWindow = SplashScreen = new SplashScreen();
            SplashScreen.Launcher = FocLauncherInformation.Instance;
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
                    
                    IUpdateResultInformation? updateInformation = null;
                    try
                    {
                        var emptyServices = new ServiceCollection().BuildServiceProvider();

                        var updateManager = new NewUpdateManager(emptyServices,
                            new UpdateConfiguration());


                        var t = await Task.Run(() =>
                        {
                            var s = new UpdaterServiceFactory();
                            var u = new FocLauncherUpdater(emptyServices);
                            return u.CheckAndUpdate(cts.Token);

                        }, CancellationToken.None);

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

        private void ReportUpdateResult(IUpdateResultInformation updateInformation)
        {
            if (updateInformation != null)
            {
                if (updateInformation.RequiresUserNotification && SplashScreen.IsProgressVisible || FocLauncherInformation.Instance.BuildType == BuildType.Debug)
                {
                    Interlocked.Exchange(ref _shouldShowSplashScreen, 0);
                    SplashScreen.Cancelable = false;
                    switch (updateInformation.Result)
                    {
                        case UpdateResult.Failed:
                            SplashScreen.ProgressText = "Update Failed";
                            new UpdateResultDialog("Update Failed", updateInformation.Message).ShowDialog();
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