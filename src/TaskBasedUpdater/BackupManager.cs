using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using TaskBasedUpdater.FileSystem;
using TaskBasedUpdater.New.Product.Component;
using TaskBasedUpdater.Verification;

namespace TaskBasedUpdater
{
    public class BackupManager : IEnumerable<KeyValuePair<ProductComponent, string?>>
    {
        //private const string NonExistentSource = "SOURCE_ORIGINALLY_MISSING";

        private static BackupManager? _instance;

        private readonly object _syncObject = new();
        private readonly Dictionary<ProductComponent, string?> _backupLookup = new();

        public static BackupManager Instance => _instance ??= new BackupManager();

        private readonly HashingService _hasher = new();

        private BackupManager()
        {
        }

        public void CreateBackup(ProductComponent productComponent)
        {
            ValidateItem(productComponent);
            var backupPath = GetBackupPath(productComponent);
            ValidateHasAccess(backupPath);
            if (_backupLookup.ContainsKey(productComponent))
                return;
            string? backupFilePath;
            var itemFilePath = productComponent.GetFilePath();
            if (File.Exists(itemFilePath))
            {
                backupFilePath = CreateBackupFilePath(productComponent, backupPath);
                // TODO: split-projects
                //FileSystemExtensions.CopyFileWithRetry(itemFilePath, backupFilePath);
            }
            else
            {
                backupFilePath = null;
            }
            lock (_syncObject)
                _backupLookup.Add(productComponent, backupFilePath);
        }

        public void RestoreAllBackups()
        {
            var keys = _backupLookup.Keys.ToList();
            foreach (var productComponent in keys)
                RestoreBackup(productComponent);
        }
        
        public void RestoreBackup(ProductComponent productComponent)
        {
            if (!_backupLookup.ContainsKey(productComponent))
                return;
            var backupFile = _backupLookup[productComponent];
            var componentFile = productComponent.GetFilePath();

            var remove = true;

            try
            {
                if (string.IsNullOrEmpty(backupFile))
                {
                    if (!File.Exists(componentFile))
                        return;
                    // TODO: split-projects
                    //var success = FileSystemExtensions.DeleteFileWithRetry(componentFile, out _);
                    //if (success) 
                    //    return;
                    remove = false;
                    throw new IOException("Unable to restore the backup. Please restart your computer!");
                }
                else
                {
                    if (!File.Exists(backupFile))
                        return;

                    if (File.Exists(componentFile))
                    {
                        var backupHash = _hasher.GetFileHash(backupFile!, HashType.Sha256);
                        var fileHash = _hasher.GetFileHash(componentFile!, HashType.Sha256);
                        if (backupHash.SequenceEqual(fileHash))
                        {
                            remove = false;
                            return;
                        }
                    }
                    // TODO: split-projects
                    //var success = FileSystemExtensions.MoveFile(backupFile, productComponent.GetFilePath(), true);
                    //if (!success)
                    //{
                    //    remove = false;
                    //    throw new IOException($"Unable to restore the backup file '{backupFile}'. Please restart your computer!");
                    //}

                    // TODO: split-projects
                    //if (UpdateConfiguration.Instance.DownloadOnlyMode)
                    //    UpdateItemDownloadPathStorage.Instance.Remove(updateItem);

                    try
                    {
                        // TODO: split-projects
                        //FileSystemExtensions.DeleteFileWithRetry(backupFile!, out _);
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
                        _backupLookup.Remove(productComponent);
            }
        }
        
        public bool TryGetValue(ProductComponent productComponent, out string? value)
        {
            lock (_syncObject)
                return _backupLookup.TryGetValue(productComponent, out value);
        }

        public void Flush()
        {
            _backupLookup.Clear();
        }

        public IEnumerator<KeyValuePair<ProductComponent, string?>> GetEnumerator()
        {
            return _backupLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static void ValidateItem(ProductComponent productComponent)
        {
            if (productComponent == null)
                throw new ArgumentNullException(nameof(productComponent));
            // TODO: split-projects; empty is allowed - test behavior 
            if (string.IsNullOrEmpty(productComponent.Destination))
                throw new IOException("Unable to resolve the updateItem's file path");
        }

        internal static void ValidateHasAccess(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            // TODO: split-projects
            //if (!FileSystemExtensions.UserHasDirectoryAccessRights(path, FileSystemRights.Read | FileSystemRights.Write, true))
            //    throw new InvalidOperationException($"No Read/Write access to the backup directory: {path}");
        }

        private static string CreateBackupFilePath(ProductComponent productComponent, string backupPath)
        {
            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var backupFileName = $"{productComponent.Name}.{randomFileName}.bak";
            return Path.Combine(backupPath, backupFileName);
        }

        private static string GetBackupPath(ProductComponent productComponent)
        {
            // TODO: split-projects
            return string.Empty;
            //var backupPath = UpdateConfiguration.Instance.BackupPath;
            //if (string.IsNullOrEmpty(backupPath))
            //    backupPath = updateItem.Destination;
            //return backupPath!;
        }
    }
}
