﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher;
using FocLauncher.Threading;
using FocLauncher.WaitDialog;
using FocLauncherHost.Controls;
using FocLauncherHost.Updater;
using FocLauncherHost.Utilities;
using Microsoft.VisualStudio.Threading;
using NLog;

namespace FocLauncherHost
{
    public class HostApplication : Application
    {
        public const string ServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/";

        private readonly AsyncManualResetEvent _canCloseApplicationEvent = new AsyncManualResetEvent(false, true);
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private bool _waitWindowShown;
        private TimeSpan _waitWindowDelay = TimeSpan.FromSeconds(2);


        static HostApplication()
        {
            // Since FocLauncher.Threading.dll and Microsoft.VisualStudio.Utilities.dll are used by the WaitWindow AppDomain we need to have them on disk
            // Make sure not to use async file writing as we need to block the app until necessary assembly are written to disk
            AssemblyExtractor.WriteNecessaryAssembliesToDisk(LauncherConstants.ApplicationBasePath, 
                "FocLauncher.Threading.dll", "Microsoft.VisualStudio.Utilities.dll");
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = new SplashScreen();
            WaitAndShutdownAsync().Forget();
            PrepareAndUpdateLauncherAsync().Forget();
        }
        
        internal async Task PrepareAndUpdateLauncherAsync()
        {
            var extractTask = AssemblyExtractor.WriteNecessaryAssembliesToDiskAsync(LauncherConstants.ApplicationBasePath, "FocLauncher.dll", "FocLauncher.Theming.dll");
            
            await ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                var data = new WaitDialogProgressData("Please wait while the launcher is downloading an update.", isCancelable: true);

                var session = WaitDialogFactory.Instance.StartWaitDialog("FoC Launcher", data, _waitWindowDelay);
                SetWhenWaitDialogIsShownAsync(_waitWindowDelay, session.UserCancellationToken).Forget();

                session.UserCancellationToken.Register(OnUserCancelled);

                var cts = CancellationTokenSource.CreateLinkedTokenSource(session.UserCancellationToken);

                UpdateInformation updateInformation = null;
                Exception updateException = null;
                try
                {
                    Task.WhenAll(extractTask, Task.Delay(200)).ContinueWith(async task => await ShowMainWindowAsync(),
                        cts.Token, TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.FromCurrentSynchronizationContext()).Forget();

                    var updateManager = new FocLauncherUpdaterManager(@"C:\Users\Anakin\OneDrive\launcherUpdate.xml");
                    updateInformation = await updateManager.CheckAndPerformUpdateAsync(cts.Token);
                    Logger.Info($"Finished automatic update with result {updateInformation}");

                    //await Task.Delay(5000, session.UserCancellationToken);
                }
                catch (OperationCanceledException)
                {
                    Logger.Info("Download Operation cancelled");
                }
                // Save the exception for later use. Reason: We want to be sure that the wait dialog is closed when showing possible error messages.
                catch (Exception exception)
                {
                    updateException = exception;
                }
                finally
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cts.Token);
                    session.Dispose();
                    cts.Dispose();
                }

                
                if (updateException != null) 
                    new ExceptionWindow(updateException).ShowDialog(); // Only unexpected exceptions should have been caught
                else
                    ReportUpdateResult(updateInformation); // Else show any other (safe) reports

                _canCloseApplicationEvent.Set();
            });
        }

        private async Task SetWhenWaitDialogIsShownAsync(TimeSpan delay, CancellationToken token)
        {
            await Task.Delay(delay, token);
            if (token.IsCancellationRequested)
                return;
            _waitWindowShown = true;
        }

        private void ReportUpdateResult(UpdateInformation updateInformation)
        {

            if (updateInformation != null)
            {
                if (updateInformation.RequiresUserNotification && _waitWindowShown ||
#if DEBUG
                    true
#endif
                    )
                {
                    MessageBox.Show($"Updating finished with result: {updateInformation.Result}\r\n" +
                                    $"Message: {updateInformation.Message}", "FoC Launcher");
                }
                
            }
        }

        private void OnUserCancelled()
        {
            // This runs before the exception is getting caught.
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
            MainWindow?.Show();
        }

        private async Task HideSplashScreenAnimatedAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (MainWindow is SplashScreen splashScreen && splashScreen.IsVisible)
            {
                await Task.Delay(500);
                await splashScreen.HideAnimationAsync();
            }
        }
    }
}