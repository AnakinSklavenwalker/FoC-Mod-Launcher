using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
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

            serviceCollection.AddSingleton<ILauncherProductService>(new LauncherProductService());

            return serviceCollection.BuildServiceProvider();
        }
    }

    internal interface ILauncherProductService
    {
        IInstalledProduct GetCurrentInstance();

        void UpdateCurrentInstance(IInstalledProduct product);

        IProductReference CreateProductReference(Version? newVersion, ProductReleaseType newReleaseType);
    }

    internal class LauncherProductService : ILauncherProductService
    {
        private IInstalledProduct? _installedProduct;


        public IInstalledProduct GetCurrentInstance()
        {
            return _installedProduct ??= BuildLauncherProduct();
        }

        public IProductReference CreateProductReference(Version? newVersion = null,
            ProductReleaseType newReleaseType = ProductReleaseType.Stable)
        {
            throw new NotImplementedException();
        }

        public void UpdateCurrentInstance(IInstalledProduct product)
        {
            throw new NotImplementedException();
        }

        private InstalledProduct BuildLauncherProduct()
        {
            return new InstalledProduct
            {
                Name = "FocLauncher",
                ReleaseType = ProductReleaseType.Stable,
                InstallationPath = Directory.GetCurrentDirectory(),
            };
        }
    }


    internal class FocLauncherUpdater : IDisposable
    {
        private readonly IInstalledProduct _product;
        private readonly IServiceProvider _services;
        private IUpdateCatalog? _updateCatalog;
        private IUpdateManager? _updateManager;

        public IUpdateConfiguration UpdateConfiguration { get; }

        public FocLauncherUpdater(IInstalledProduct product, IUpdateConfiguration updateConfiguration, IServiceProvider services)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(services, nameof(services));
            Requires.NotNull(updateConfiguration, nameof(updateConfiguration));
            _product = product;
            _services = services;
            UpdateConfiguration = updateConfiguration;
        }

        public IUpdateResultInformation CheckAndUpdate(IUpdateRequest updateRequest, CancellationToken token)
        {
            Requires.NotNull(updateRequest, nameof(updateRequest));
            if (!IsUpdateAvailable(updateRequest, token))
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
                new NewUpdateManager(new ServiceCollection().BuildServiceProvider(), UpdateConfiguration);

            if (_updateCatalog is null)
                throw new InvalidOperationException("Catalog cannot be null");

            updater.Update(_updateCatalog, token);

            return UpdateResultInformation.Success;
        }

        private bool IsUpdateAvailable(IUpdateRequest updateRequest, CancellationToken token)
        {
            IProductProviderService productProviderService = null;

            var i = productProviderService.GetInstalledProductCatalog(_product);
            var a = productProviderService.GetAvailableProductCatalog(updateRequest);

            IUpdateCatalogBuilder b = null;

            var u = b.Build(i, a);

            if (!u.Items.Any())
                return false;
            _updateCatalog = u;
            return true;
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

        private readonly IServiceProvider _services;

        internal HostApplication()
        {
            MainWindow = SplashScreen = new SplashScreen();
            SplashScreen.Launcher = FocLauncherInformation.Instance;

            _services = new UpdaterServiceFactory().CreateServiceProvider(
                new ServiceCollection().BuildServiceProvider());
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
                        
                        updateInformation = await Task.Run(() =>
                        {
                            var p = _services.GetRequiredService<ILauncherProductService>().GetCurrentInstance();

                            var u = new FocLauncherUpdater(p, new UpdateConfiguration(), _services);

                            var r = new UpdateRequest
                            {
                                Product = new LauncherProductService().CreateProductReference(),
                                RequestedAction = UpdateRequestAction.Repair | UpdateRequestAction.Update,
                                UpdateManifestPath = string.Empty
                            };

                            return u.CheckAndUpdate(r, cts.Token);

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