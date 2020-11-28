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

        protected IUpdateItem UpdateItem { get; private set; }


        public static FileInstaller Instance => _fileInstaller ??= new FileInstaller(null);


        private FileInstaller(IServiceProvider serviceProvider)
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
            _lockedFilesWatcher = LockedFilesWatcher.Instance;
            _lockedFiles = new List<string>();
        }

        public InstallResult Remove(IUpdateItem updateItem, CancellationToken token, bool isPresent = false)
        {
            Token = token;
            var location = updateItem.Destination;
            return PlanAndApplyExecuteAction(location, updateItem, isPresent, null);
        }

        public InstallResult Install(IUpdateItem updateItem, CancellationToken token, string localPath, bool isPresent)
        {
            Token = token;
            var location = updateItem.Destination;
            return PlanAndApplyExecuteAction(location, updateItem, isPresent, localPath);
        }

        protected InstallResult InstallCore(string localPath, string installDir, IUpdateItem updateItem = null)
        {
            return DoAction(() => InstallCoreInternal(localPath, installDir, updateItem));
        }

        protected InstallResult UninstallCore(string installDir, IUpdateItem updateItem)
        {
            return DoAction(() => UninstallCoreInternal(installDir, updateItem));
        }

        protected InstallResult InstallCoreInternal(string localPath, string installDir, IUpdateItem updateItem)
        {
            var file = updateItem.GetFilePath();
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

            updateItem.CurrentState = CurrentState.Installed;
            return InstallResult.Success;
        }
        
        internal InstallResult UninstallCoreInternal(string installDir, IUpdateItem updateItem)
        {
            if (!FileSystemExtensions.ContainsPath(installDir, updateItem.Destination))
            {
                _logger.LogWarning("Different paths for item and method input");
                return InstallResult.Failure;
            }

            var restartPending = false;
            var file = updateItem.GetFilePath();
            var deleteResult = DeleteFile(file, out var restartRequired);
            restartPending |= restartRequired;
            if (!deleteResult && !restartRequired)
                return InstallResult.Failure;

            if (restartPending)
                return InstallResult.SuccessRestartRequired;

            updateItem.CurrentState = CurrentState.Removed;
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

        protected void PrintReturnCode(InstallResult result, IUpdateItem updateItem, UpdateAction executeAction)
        {
            if (result.IsFailure())
            {
                var installResultDetails = GetInstallResultDetails(result, updateItem, executeAction.ToString().ToLowerInvariant());
                LogFailure(updateItem, executeAction, installResultDetails);
            }
            else
            {
                var text = "Result: " + result;
                _logger.LogTrace("Package executed successfully. {0}", text);
            }
        }

        protected string GetInstallResultDetails(InstallResult installResult, IUpdateItem? updateItem, string action)
        {
            var stringBuilder = new StringBuilder();
            if (stringBuilder.Length > 0) stringBuilder.Append(", ");
            stringBuilder.Append("Result: ");
            stringBuilder.Append(installResult);
            if (updateItem != null)
            {
                var failureSignature = updateItem.GetFailureSignature(action, installResult.ToString());
                stringBuilder.Append(", Signature: ");
                stringBuilder.Append(failureSignature);
            }
            return stringBuilder.ToString();
        }

        private InstallResult InstallHelper(InstallData installData)
        {
            InstallResult installResult;
            var updateItem = installData.UpdateItem;

            try
            {
                if (installData.LocalPath == null)
                    return InstallResult.Failure;
                installResult = InstallCore(installData.LocalPath, installData.InstallDir, updateItem);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("User canceled during install.");
                return InstallResult.Cancel;
            }
            catch (UnauthorizedAccessException e)
            {
                Elevator.Instance.RequestElevation(e, UpdateItem);
                return InstallResult.Failure;
            }
            catch (Exception ex)
            {
                LogFailure(updateItem, UpdateAction.Update, ex.Message);
                return InstallResult.FailureException;
            }
            PrintReturnCode(installResult, updateItem, UpdateAction.Update);
            return installResult;
        }

        private InstallResult UninstallHelper(InstallData uninstallData)
        {
            InstallResult result;
            var updateItem = uninstallData.UpdateItem;

            try
            {
                if (uninstallData.UpdateItem == null && uninstallData.LocalPath == null)
                    result = InstallResult.Failure;
                else
                    result = UninstallCore(uninstallData.InstallDir, updateItem);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("User canceled during item uninstall.");
                return InstallResult.Cancel;
            }
            catch (Exception e)
            {
                LogFailure(updateItem, UpdateAction.Delete, e.ToString());
                return InstallResult.FailureException;
            }
            PrintReturnCode(result, updateItem, UpdateAction.Delete);
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

        private InstallResult PlanAndApplyExecuteAction(string location, IUpdateItem updateItem, bool isPresent, string localPath)
        {
            var requestedAction = updateItem.RequiredAction;

            Func<InstallData, InstallResult> action = null;
            switch (requestedAction)
            {
                case UpdateAction.Update:
                    if (updateItem.CurrentState == CurrentState.Downloaded)
                        action = InstallHelper;
                    break;
                case UpdateAction.Delete:
                    if (isPresent)
                        action = UninstallHelper;
                    break;
            }

            if (action == null)
                return InstallResult.Success;
            try
            {
                UpdateItem = updateItem;
                return action(new InstallData(updateItem, location, localPath));
            }
            finally
            {
                UpdateItem = null;
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

        private void LogFailure(IUpdateItem updateItem, UpdateAction executeAction, string details)
        {
            _logger?.LogError(updateItem != null
                ? $"Package '{updateItem.Name}' failed to {executeAction.ToString().ToLowerInvariant()}. {details}"
                : $"Failed to {executeAction.ToString().ToLowerInvariant()}. {details}");
        }


        private class InstallData
        {
            internal string InstallDir { get; }

            internal string? LocalPath { get; }

            internal IUpdateItem UpdateItem { get; }

            internal InstallData(IUpdateItem updateItem, string installDir, string localPath)
            {
                InstallDir = installDir;
                LocalPath = localPath;
                UpdateItem = updateItem;
            }
        }
    }
}