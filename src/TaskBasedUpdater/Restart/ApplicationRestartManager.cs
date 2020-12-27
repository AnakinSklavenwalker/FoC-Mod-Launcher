using System;
using System.Diagnostics;
using TaskBasedUpdater.Elevation;

namespace TaskBasedUpdater.Restart
{
    public static class ApplicationRestartManager
    {
        public static void RestartApplication(IRestartOptions restartOptions)
        {
            RestartApplication(restartOptions, Elevator.IsProcessElevated);
        }

        public static void RestartApplication(IRestartOptions restartOptions, bool elevated)
        {
            // TODO: split-projects
            //if (!UpdateConfiguration.Instance.SupportsRestart)
            //    throw new RestartDeniedOrFailedException("Application restart is not supported.");

            if (restartOptions is null)
                throw new ArgumentNullException(nameof(restartOptions));

            // TODO: split-projects
            var updaterTool = string.Empty;
            //var updaterTool = UpdateConfiguration.Instance.ExternalUpdaterPath;
            //if (string.IsNullOrEmpty(updaterTool) || !File.Exists(updaterTool))
                throw new RestartDeniedOrFailedException("External updater tool not found");

            var startInfo = new ProcessStartInfo(updaterTool)
            {
#if !DEBUG
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
#endif
            };

            if (elevated) 
                startInfo.Verb = "runas";

            startInfo.Arguments = restartOptions.ToCommandLine();
            Process.Start(startInfo);

            Environment.Exit(0);
        }
    }
}
