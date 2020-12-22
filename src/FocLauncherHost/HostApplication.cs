using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher;
using FocLauncher.Threading;
using FocLauncherHost.Dialogs;
using FocLauncherHost.Update;
using FocLauncherHost.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using NLog;
using TaskBasedUpdater;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Product.Manifest;
using TaskBasedUpdater.New.Update;
using Requires = Microsoft.Requires;

namespace FocLauncherHost.Product
{
    internal sealed class LauncherProductService : ProductServiceBase
    {
        public LauncherProductService(IProductComponentBuilder componentBuilder, IServiceProvider serviceProvider) : base(componentBuilder, serviceProvider)
        {
        }

        public override IProductReference CreateProductReference(Version? newVersion,
            ProductReleaseType newReleaseType)
        {
            return new ProductReference(LauncherConstants.ProductName)
            {
                Version = newVersion,
                ReleaseType = newReleaseType
            };
        }

        protected override IInstalledProduct BuildProduct()
        {
            var productRef = CreateProductReference(null, ProductReleaseType.Stable);
            var path = GetInstallationPath();
            var manifest = GetManifest(productRef);
            return new InstalledProduct(productRef, manifest, path);
        }

        protected override IAvailableProductManifest LoadManifest(IProductReference product, IFileInfo manifestFile)
        {
            throw new NotImplementedException();
        }


        private static IInstalledProductManifest GetManifest(IProductReference productReference)
        {
            return new LauncherManifest(productReference);
        }

        private static string GetInstallationPath()
        {
            return Directory.GetCurrentDirectory();
        }
    }
    
    internal class LauncherComponentBuilder : IProductComponentBuilder
    {
        public HashType HashType => HashType.Sha256;
        
        public Version? GetVersion(IFileInfo file)
        {
            return LauncherVersionUtilities.GetFileVersionSafe(file);
        }
    }
    
    internal class LauncherManifest : IInstalledProductManifest
    {
        public LauncherManifest(IProductReference product)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }

        public IEnumerable<ProductComponent> Items
        {
            get
            {
                yield return new ProductComponent(LauncherConstants.LauncherFileName, "");

                yield return new ProductComponent(LauncherConstants.UpdaterFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");

                yield return new ProductComponent(LauncherConstants.LauncherDllFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");

                yield return new ProductComponent(LauncherConstants.LauncherThemeFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");

                yield return new ProductComponent(LauncherConstants.LauncherThreadingFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");
            }
        }

        public IProductReference Product { get; }
    }
}


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

            _services = new UpdaterServiceFactory().CreateServiceProvider(
                new LauncherServiceFactory().CreateLauncherServices());
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
                    
                    UpdateResultInformation? updateInformation = null;
                    try
                    {
                        
                        updateInformation = await Task.Run(() =>
                        {
                            var ps = _services.GetRequiredService<IProductService>();
                            var p = ps.GetCurrentInstance();

                            var u = new FocLauncherUpdater(p, new UpdateConfiguration(), _services);

                            var r = new UpdateRequest
                            {
                                Product = ps.CreateProductReference(null, ProductReleaseType.Stable),
                                UpdateManifestPath = new Uri("file://c:/Test/text.xml", UriKind.Absolute)
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

        private void ReportUpdateResult(UpdateResultInformation? updateInformation)
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