using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.VisualStudio.Threading;
using NLog;
using TaskBasedUpdater;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.New;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Update;

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
        private bool _isInitialized;
        private IInstalledProduct? _installedProduct;

        public IInstalledProduct GetCurrentInstance()
        {
            Initialize();
            return _installedProduct!;
        }

        public IProductReference CreateProductReference(Version? newVersion = null,
            ProductReleaseType newReleaseType = ProductReleaseType.Stable)
        {
            Initialize();
            return new ProductReference(_installedProduct!.Name)
            {
                Version = newVersion,
                ReleaseType = newReleaseType
            };
        }

        public void UpdateCurrentInstance(IInstalledProduct product)
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;
            _installedProduct ??= BuildLauncherProduct();
            _isInitialized = true;
        }

        private static InstalledProduct BuildLauncherProduct()
        {
            var name = GetProductName();
            var path = GetInstallationPath();
            var manifest = GetManifest();
            return new InstalledProduct(name, path, manifest)
            {
                ReleaseType = ProductReleaseType.Stable
            };
        }

        private static IInstalledProductManifest GetManifest()
        {
            return new LauncherManifest();
        }

        private static string GetProductName()
        {
            return LauncherConstants.ProductName;
        }

        private static string GetInstallationPath()
        {
            return Directory.GetCurrentDirectory();
        }
    }

    internal class LauncherManifest : IInstalledProductManifest
    {
        public IEnumerable<ProductComponent> Items
        {
            get
            {
                yield return new(LauncherConstants.LauncherFileName, "");
                
                yield return new(LauncherConstants.UpdaterFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");
                
                yield return new(LauncherConstants.LauncherDllFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");
                
                yield return new(LauncherConstants.LauncherThemeFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");
                
                yield return new(LauncherConstants.LauncherThreadingFileName,
                    $"%{LauncherConstants.ApplicationBaseVariable}%");
            }
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
            _services = new AggregatedServiceProvider(services, InitializeServices);
            UpdateConfiguration = updateConfiguration;
        }

        public UpdateResultInformation CheckAndUpdate(IUpdateRequest updateRequest, CancellationToken token)
        {
            Requires.NotNull(updateRequest, nameof(updateRequest));
            try
            {
                if (!IsUpdateAvailable(updateRequest))
                    return UpdateResultInformation.NoUpdate;
                if (_updateCatalog is null)
                    return new UpdateResultInformation
                    {
                        Result = UpdateResult.NoUpdate,
                        Message = "Unable to find update manifest."
                    };
                token.ThrowIfCancellationRequested();
                return Update(token);
            }
            catch (OperationCanceledException)
            {
                return new UpdateResultInformation
                {
                    Result = UpdateResult.Cancelled,
                    Message = "Update cancelled by user request."
                };
            }
        }

        private UpdateResultInformation Update(CancellationToken token)
        {
            var updater =
                new NewUpdateManager(new ServiceCollection().BuildServiceProvider(), UpdateConfiguration);

            if (_updateCatalog is null)
                throw new InvalidOperationException("Catalog cannot be null");

            updater.Update(_updateCatalog, token);

            return UpdateResultInformation.Success;
        }

        private bool IsUpdateAvailable(IUpdateRequest updateRequest)
        {
            var productProviderService = _services.GetRequiredService<IProductCatalogService>();

            var currentCatalog = productProviderService.GetInstalledProductCatalog(_product);
            var availableCatalog = productProviderService.GetAvailableProductCatalog(updateRequest);

            IUpdateCatalogBuilder builder = _services.GetRequiredService<IUpdateCatalogBuilder>();
            var updateCatalog = builder.Build(currentCatalog, availableCatalog);

            if (!updateCatalog.Items.Any())
                return false;
            _updateCatalog = updateCatalog;
            return true;
        }

        private IServiceCollection InitializeServices()
        {
            var sc =  new ServiceCollection();
            sc.AddTransient<IProductCatalogService>(_ => new LauncherCatalogService(_services));
            sc.AddTransient<IUpdateCatalogBuilder>(_ => new UpdateCatalogBuilder());
            return sc;
        }

        public void Dispose()
        {
            _updateManager?.Dispose();
        }
    }

    internal class LauncherCatalogService : IProductCatalogService
    {
        private readonly IServiceProvider _serviceProvider;

        public LauncherCatalogService(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        public IInstalledProductCatalog GetInstalledProductCatalog(IInstalledProduct product)
        {
            var launcherProduct = _serviceProvider.GetRequiredService<ILauncherProductService>().GetCurrentInstance();
            if (!ProductReferenceEqualityComparer.Default.Equals(launcherProduct, product))
                throw new InvalidOperationException("Not compatible product");

            var installedComponents = FindInstalledComponents(launcherProduct);
            return new InstalledProductCatalog(product, installedComponents);
        }

        public IAvailableProductCatalog GetAvailableProductCatalog(IUpdateRequest request)
        {
            var launcherProduct = _serviceProvider.GetRequiredService<ILauncherProductService>().GetCurrentInstance();
            if (!ProductReferenceEqualityComparer.Default.Equals(launcherProduct, request.Product))
                throw new InvalidOperationException("Not compatible product");
            
            return new AvailableProductCatalog(request.Product, new ProductComponent[0]);
        }

        internal IEnumerable<ProductComponent> FindInstalledComponents(IInstalledProduct product)
        {
            if (product.ProductManifest is null)
                throw new InvalidOperationException("Product manifest cannot be null.");

            foreach (var component in product.ProductManifest.Items)
            {
                var path = component.GetFilePath();

                FileInfo fileInfo = Path.IsPathRooted(path)
                    ? new FileInfo(path)
                    : new FileInfo(Path.Combine(product.InstallationPath, path));

                var installedComponent = ComponentFileFactory.FromFile(component, new PhysicalFileInfo(fileInfo),
                    LauncherVersionUtilities.GetFileVersionSafe, HashType.Sha256);

                if (installedComponent.CurrentState == CurrentState.Installed)
                    yield return installedComponent;
            }
        }
    }

    internal static class LauncherVersionUtilities
    {
        public static Version? GetFileVersionSafe(IFileInfo file)
        {
            try
            {
                return GetFileVersion(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Version GetFileVersion(IFileInfo file)
        {
            Requires.NotNull(file, nameof(file));
            if (file.IsDirectory)
                throw new IOException("Cannot get version from directory");
            if (file.PhysicalPath is null)
                throw new IOException("Cannot get physical path from file");
            var existingVersionString = FileVersionInfo.GetVersionInfo(file.PhysicalPath).FileVersion;
            return Version.Parse(existingVersionString);
        }
    }



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
                    
                    UpdateResultInformation? updateInformation = null;
                    try
                    {
                        
                        updateInformation = await Task.Run(() =>
                        {
                            var ps = _services.GetRequiredService<ILauncherProductService>();
                            var p = ps.GetCurrentInstance();

                            var u = new FocLauncherUpdater(p, new UpdateConfiguration(), _services);

                            var r = new UpdateRequest
                            {
                                Product = ps.CreateProductReference(null, ProductReleaseType.Stable),
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