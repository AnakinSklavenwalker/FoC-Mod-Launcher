using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.FileSystem;

namespace TaskBasedUpdater
{
    public class BackupManager : IEnumerable<KeyValuePair<IUpdateItem, string?>>
    {
        //private const string NonExistentSource = "SOURCE_ORIGINALLY_MISSING";

        private static BackupManager? _instance;

        private readonly object _syncObject = new object();
        private readonly Dictionary<IUpdateItem, string?> _backupLookup = new Dictionary<IUpdateItem, string?>();

        public static BackupManager Instance => _instance ??= new BackupManager();

        private BackupManager()
        {
        }

        public void CreateBackup(IUpdateItem updateItem)
        {
            ValidateComponent(updateItem);
            var backupPath = GetBackupPath(updateItem);
            ValidateHasAccess(backupPath);
            if (_backupLookup.ContainsKey(updateItem))
                return;
            string? backupFilePath;
            var componentFilePath = updateItem.GetFilePath();
            if (File.Exists(componentFilePath))
            {
                backupFilePath = CreateBackupFilePath(updateItem, backupPath);
                FileSystemExtensions.CopyFileWithRetry(componentFilePath, backupFilePath);
            }
            else
            {
                backupFilePath = null;
            }
            lock (_syncObject)
                _backupLookup.Add(updateItem, backupFilePath);
        }

        public void RestoreAllBackups()
        {
            var keys = _backupLookup.Keys.ToList();
            foreach (var component in keys)
                RestoreBackup(component);
        }
        
        public void RestoreBackup(IUpdateItem updateItem)
        {
            if (!_backupLookup.ContainsKey(updateItem))
                return;
            var backupFile = _backupLookup[updateItem];
            var updateItemFile = updateItem.GetFilePath();

            var remove = true;

            try
            {
                if (string.IsNullOrEmpty(backupFile))
                {
                    if (!File.Exists(updateItemFile))
                        return;
                    var success = FileSystemExtensions.DeleteFileWithRetry(updateItemFile, out _);
                    if (success) 
                        return;
                    remove = false;
                    throw new IOException("Unable to restore the backup. Please restart your computer!");
                }
                else
                {
                    if (!File.Exists(backupFile))
                        return;

                    if (File.Exists(updateItemFile))
                    {
                        var backupHash = UpdaterUtilities.GetFileHash(backupFile!, HashType.Sha256);
                        var fileHash = UpdaterUtilities.GetFileHash(backupFile!, HashType.Sha256);
                        if (backupHash.SequenceEqual(fileHash))
                        {
                            remove = false;
                            return;
                        }
                    }
                    var success = FileSystemExtensions.MoveFile(backupFile, updateItem.GetFilePath(), true);
                    if (!success)
                    {
                        remove = false;
                        throw new IOException($"Unable to restore the backup file '{backupFile}'. Please restart your computer!");
                    }

                    if (UpdateConfiguration.Instance.DownloadOnlyMode)
                        UpdateItemDownloadPathStorage.Instance.Remove(updateItem);

                    try
                    {
                        FileSystemExtensions.DeleteFileWithRetry(backupFile!, out _);
                    }
                    catch
                    {
                        remove = false;
                    }
                }
            }
            finally
            {
                if (remove)
                    lock (_syncObject)
                        _backupLookup.Remove(updateItem);
            }
        }
        
        public bool TryGetValue(IUpdateItem component, out string? value)
        {
            lock (_syncObject)
                return _backupLookup.TryGetValue(component, out value);
        }

        public void Flush()
        {
            _backupLookup.Clear();
        }

        public IEnumerator<KeyValuePair<IUpdateItem, string?>> GetEnumerator()
        {
            return _backupLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static void ValidateComponent(IUpdateItem updateItem)
        {
            if (updateItem == null)
                throw new ArgumentNullException(nameof(updateItem));
            if (string.IsNullOrEmpty(updateItem.Destination))
                throw new IOException("Unable to resolve the component's file path");
        }

        internal static void ValidateHasAccess(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (!FileSystemExtensions.UserHasDirectoryAccessRights(path, FileSystemRights.Read | FileSystemRights.Write, true))
                throw new InvalidOperationException($"No Read/Write access to the backup directory: {path}");
        }

        private static string CreateBackupFilePath(IUpdateItem updateItem, string backupPath)
        {
            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var backupFileName = $"{updateItem.Name}.{randomFileName}.bak";
            return Path.Combine(backupPath, backupFileName);
        }

        private static string GetBackupPath(IUpdateItem updateItem)
        {
            var backupPath = UpdateConfiguration.Instance.BackupPath;
            if (string.IsNullOrEmpty(backupPath))
                backupPath = updateItem.Destination;
            return backupPath!;
        }
    }
}
