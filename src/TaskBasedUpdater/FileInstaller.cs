using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Elevation;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.Restart;
using Validation;

namespace TaskBasedUpdater
{
    internal class FileInstaller
    {
        private readonly IServiceProvider _serviceProvider;
        private static FileInstaller _fileInstaller;

        private readonly ILogger? _logger;

        private readonly List<string> _lockedFiles;
        private LockedFileLogger _lockedFileLogger;
        private readonly LockedFilesWatcher _lockedFilesWatcher;

        protected CancellationToken Token { get; private set; }

        protected internal IEnumerable<string> LockedFiles => _lockedFiles;

        protected ProductComponent ProductComponent { get; private set; }


        public static FileInstaller Instance => _fileInstaller ??= new FileInstaller(null);


        private FileInstaller(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
            _lockedFilesWatcher = LockedFilesWatcher.Instance;
            _lockedFiles = new List<string>();
        }

        public InstallResult Remove(ProductComponent productComponent, CancellationToken token, bool isPresent = false)
        {
            Token = token;
            var location = productComponent.Destination;
            return PlanAndApplyExecuteAction(location, productComponent, isPresent, null);
        }

        public InstallResult Install(ProductComponent productComponent, CancellationToken token, string localPath, bool isPresent)
        {
            Token = token;
            var location = productComponent.Destination;
            return PlanAndApplyExecuteAction(location, productComponent, isPresent, localPath);
        }

        protected InstallResult InstallCore(string localPath, string installDir, ProductComponent? productComponent = null)
        {
            return DoAction(() => InstallCoreInternal(localPath, installDir, productComponent));
        }

        protected InstallResult UninstallCore(string installDir, ProductComponent productComponent)
        {
            return DoAction(() => UninstallCoreInternal(installDir, productComponent));
        }

        protected InstallResult InstallCoreInternal(string localPath, string installDir, ProductComponent productComponent)
        {
            var file = productComponent.GetFilePath();
            if (localPath.Equals(file))
            {
                _logger.LogWarning("Install: Local path and destination path are equal.");
                return InstallResult.Failure;
            }

            var restartPending = false;
            var result = CopyFile(localPath, file, out var restartRequired);
            restartPending |= restartRequired;
            if (!result && !restartRequired)
                return InstallResult.Failure;

            if (restartPending)
                return InstallResult.SuccessRestartRequired;

            productComponent.CurrentState = CurrentState.Installed;
            return InstallResult.Success;
        }
        
        internal InstallResult UninstallCoreInternal(string installDir, ProductComponent productComponent)
        {
            if (!FileSystemExtensions.ContainsPath(installDir, productComponent.Destination))
            {
                _logger.LogWarning("Different paths for item and method input");
                return InstallResult.Failure;
            }

            var restartPending = false;
            var file = productComponent.GetFilePath();
            var deleteResult = DeleteFile(file, out var restartRequired);
            restartPending |= restartRequired;
            if (!deleteResult && !restartRequired)
                return InstallResult.Failure;

            if (restartPending)
                return InstallResult.SuccessRestartRequired;

            productComponent.CurrentState = CurrentState.Removed;
            return InstallResult.Success;
        }

        private bool CopyFile(string source, string destination, out bool restartRequired)
        {
            restartRequired = false;
            if (LockedFilesWatcher.Instance.LockedFiles.Contains(destination))
                restartRequired = true;
            
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            Stream output = null;
            try
            {
                output = FileSystemExtensions.CreateFileWithRetry(destination, 2, 500);
            }
            catch (IOException)
            {
                _lockedFiles.Add(destination);
                restartRequired = true;
            }

            if (output == null)
            {
                _logger.LogError($"Creation of file {destination} failed");
                return false;
            }

            using (output)
            {
                using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
                sourceStream.CopyTo(output);
            }
            return true;
        }

        protected bool DeleteFile(string file, out bool restartRequired)
        {
            if (!File.Exists(file))
            {
                _logger.LogTrace($"'{file}' file is already deleted.");
                restartRequired = false;
                return true;
            }

            var deleteSuccess = FileSystemExtensions.DeleteFileWithRetry(file, out restartRequired, true, 2, 500,
                (ex, attempt) =>
                {
                    _logger?.LogTrace(
                        $"Error occurred while deleting file '{file}'. Error details: {ex.Message}. Retrying after {0.5f} seconds...");
                    return true;
                });
            if (deleteSuccess)
                _logger.LogInformation($"{file} file deleted.");
            else
            {
                _lockedFiles.Add(file);
                _logger.LogInformation($"{file} file is scheduled for deletion after restarting.");
            }

            return deleteSuccess;
        }

        protected void PrintReturnCode(InstallResult result, ProductComponent productComponent, ComponentAction executeAction)
        {
            if (result.IsFailure())
            {
                var installResultDetails = GetInstallResultDetails(result, productComponent, executeAction.ToString().ToLowerInvariant());
                LogFailure(productComponent, executeAction, installResultDetails);
            }
            else
            {
                var text = "Result: " + result;
                _logger.LogTrace("Package executed successfully. {0}", text);
            }
        }

        protected string GetInstallResultDetails(InstallResult installResult, ProductComponent? productComponent, string action)
        {
            var stringBuilder = new StringBuilder();
            if (stringBuilder.Length > 0) stringBuilder.Append(", ");
            stringBuilder.Append("Result: ");
            stringBuilder.Append(installResult);
            if (productComponent != null)
            {
                var failureSignature = productComponent.GetFailureSignature(action, installResult.ToString());
                stringBuilder.Append(", Signature: ");
                stringBuilder.Append(failureSignature);
            }
            return stringBuilder.ToString();
        }

        private InstallResult InstallHelper(InstallData installData)
        {
            InstallResult installResult;
            var component = installData.ProductComponent;

            try
            {
                if (installData.LocalPath == null)
                    return InstallResult.Failure;
                installResult = InstallCore(installData.LocalPath, installData.InstallDir, component);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("User canceled during install.");
                return InstallResult.Cancel;
            }
            catch (UnauthorizedAccessException e)
            {
                Elevator.Instance.RequestElevation(e, ProductComponent);
                return InstallResult.Failure;
            }
            catch (Exception ex)
            {
                LogFailure(component, ComponentAction.Update, ex.Message);
                return InstallResult.FailureException;
            }
            PrintReturnCode(installResult, component, ComponentAction.Update);
            return installResult;
        }

        private InstallResult UninstallHelper(InstallData uninstallData)
        {
            InstallResult result;
            var component = uninstallData.ProductComponent;

            try
            {
                if (uninstallData.ProductComponent == null && uninstallData.LocalPath == null)
                    result = InstallResult.Failure;
                else
                    result = UninstallCore(uninstallData.InstallDir, component);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("User canceled during item uninstall.");
                return InstallResult.Cancel;
            }
            catch (Exception e)
            {
                LogFailure(component, ComponentAction.Delete, e.ToString());
                return InstallResult.FailureException;
            }
            PrintReturnCode(result, component, ComponentAction.Delete);
            return result;
        }

        private InstallResult DoAction(Func<InstallResult> action)
        {
            Reset();
            var installResult = action();
            try
            {
                RegisterLockedFiles();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to log locking processes: {ex.Message}");
            }
            return installResult;
        }

        private InstallResult PlanAndApplyExecuteAction(string location, ProductComponent productComponent, bool isPresent, string localPath)
        {
            var requestedAction = productComponent.RequiredAction;

            Func<InstallData, InstallResult> action = null;
            switch (requestedAction)
            {
                case ComponentAction.Update:
                    if (productComponent.CurrentState == CurrentState.Downloaded)
                        action = InstallHelper;
                    break;
                case ComponentAction.Delete:
                    if (isPresent)
                        action = UninstallHelper;
                    break;
            }

            if (action == null)
                return InstallResult.Success;
            try
            {
                ProductComponent = productComponent;
                return action(new InstallData(productComponent, location, localPath));
            }
            finally
            {
                ProductComponent = null;
            }
        }
        
        private void RegisterLockedFiles()
        {
            if (_lockedFiles.Count <= 0)
                return;

            _lockedFilesWatcher.LockedFiles.AddRange(_lockedFiles);

            if (_lockedFileLogger == null) 
                _lockedFileLogger = LockedFileLogger.Instance;
            _lockedFileLogger.Log(_lockedFiles);
        }

        private void Reset()
        {
            this._lockedFiles.Clear();
        }

        private void LogFailure(ProductComponent productComponent, ComponentAction executeAction, string details)
        {
            _logger?.LogError(productComponent != null
                ? $"Package '{productComponent.Name}' failed to {executeAction.ToString().ToLowerInvariant()}. {details}"
                : $"Failed to {executeAction.ToString().ToLowerInvariant()}. {details}");
        }


        private class InstallData
        {
            internal string InstallDir { get; }

            internal string? LocalPath { get; }

            internal ProductComponent ProductComponent { get; }

            internal InstallData(ProductComponent productComponent, string installDir, string localPath)
            {
                InstallDir = installDir;
                LocalPath = localPath;
                ProductComponent = productComponent;
            }
        }
    }
}