using System;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using TaskBasedUpdater.NativeMethods;
using Validation;

namespace TaskBasedUpdater.FileSystem
{
    internal static class FileSystemExtensions
    {
        public static void DeleteFileIfInTemp(this IFileSystem fileSystem, IFileInfo file)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNull(file, nameof(file));
            if (!file.Exists || !fileSystem.Path.ContainsPath(file.FullName, fileSystem.Path.GetTempPath()))
                return;
            fileSystem.File.Delete(file.FullName);
        }

        public static long GetDriveFreeSpace(this IFileSystem fileSystem, string path)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            var pathInstance = fileSystem.Path;
            var root = pathInstance.GetPathRoot(path);
            return fileSystem.DriveInfo.FromDriveName(root).AvailableFreeSpace;
        }
        
        public static Stream? CreateFileWithRetry(this IFileSystem fileSystem, string path, int retryCount = 2, int retryDelay = 500)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNullOrEmpty(path, nameof(path));
            Stream? stream = null;
            ExecuteFileActionWithRetry(retryCount, retryDelay,
                () => stream = fileSystem.FileStream.Create(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None));
            return stream;
        }

        public static void CopyFileWithRetry(this IFileSystem fileSystem, string source, string destination, int retryCount = 2, int retryDelay = 500)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNullOrEmpty(source, nameof(source));
            Requires.NotNullOrEmpty(destination, nameof(destination));
            ExecuteFileActionWithRetry(retryCount, retryDelay, () => fileSystem.File.Copy(source, destination, true));
        }

        public static bool DeleteFileWithRetry(this IFileSystem fileSystem, string path, out bool restartRequired,
            bool restartOk = false, int retryCount = 2, int retryDelay = 500, Func<Exception, int, bool>? errorAction = null)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            Requires.NotNullOrEmpty(path, nameof(path));

            
            if (!fileSystem.File.Exists(path))
            {
                restartRequired = false;
                return true;
            }

            var flag = ExecuteFileActionWithRetry(retryCount, retryDelay, () => fileSystem.File.Delete(path), !restartOk, (ex, attempt) =>
            {
                if (ex is UnauthorizedAccessException)
                {
                    if (attempt == 0)
                    {
                        if ((fileSystem.File.GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            fileSystem.RemoveFileAttributes(path, FileAttributes.ReadOnly);
                            errorAction?.Invoke(ex, attempt);
                            return true;
                        }
                    }
                    else if (!restartOk && attempt == retryCount)
                        throw ex;
                }
                errorAction?.Invoke(ex, attempt);
                return false;
            });
            if (flag || !restartOk)
            {
                restartRequired = false;
                return flag;
            }

            restartRequired = true;
            return false;
        }

        public static void RemoveFileAttributes(this IFileSystem fileSystem, string path, FileAttributes attributesToRemove)
        {
            var fileInfo = fileSystem.FileInfo.FromFileName(path);
            var currentAttributes = fileInfo.Attributes;
            var newAttributes = currentAttributes & ~attributesToRemove;
            fileSystem.File.SetAttributes(path, newAttributes);
            fileInfo.Refresh();
        }

        // Based on: https://stackoverflow.com/questions/1410127/c-sharp-test-if-user-has-write-access-to-a-folder
        public static bool UserHasDirectoryAccessRights(this IFileSystem fileSystem, string path,
            FileSystemRights accessRights, bool create = false)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            bool isInRoleWithAccess;
            var di = fileSystem.DirectoryInfo.FromDirectoryName(path);
            try
            {
                if (!di.Exists && create)
                    di.Create();
#if NET
                if (!OperatingSystem.IsWindows())
                    return TestAccessRightsOnNonWindows();
#endif
                isInRoleWithAccess = TestAccessRightsOnWindows(di, accessRights);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return isInRoleWithAccess;
        }
        
        public static bool MoveFile(this IFileSystem fileSystem, string source, string destination, bool replace = false)
        {
            Requires.NotNullOrEmpty(source, nameof(source));
            var directoryExists = fileSystem.Directory.Exists(source);
            if (!replace)
            {
                if (directoryExists)
                    fileSystem.Directory.Move(source, destination);
                else
                    fileSystem.File.Move(source, destination);
                return true;
            }
            var flags = MoveFileFlags.MoveFileWriteThrough;
            flags |= MoveFileFlags.MoveFileReplaceExisting;
            if (!directoryExists)
                flags |= MoveFileFlags.MoveFileCopyAllowed;
            return MoveFileEx(source, destination, flags);
        }
        
        private static bool ExecuteFileActionWithRetry(int retryCount, int retryDelay, Action fileAction,
            bool throwOnFailure = true, Func<Exception, int, bool>? errorAction = null)
        {
            var num = retryCount + 1;
            for (var index = 0; index < num; ++index)
            {
                try
                {
                    fileAction();
                    return true;
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    if (!throwOnFailure || index + 1 < num)
                    {
                        if (errorAction != null)
                        {
                            if (!errorAction(ex, index))
                            {
                                if (index + 1 >= num)
                                    continue;
                            }
                            else
                                continue;
                        }

                        Task.Delay(retryDelay).Wait();
                    }
                    else
                        throw;
                }
            }
            return false;
        }

        private static bool MoveFileEx(string source, string destination, MoveFileFlags flags)
        {
#if NET
            if (!OperatingSystem.IsWindows())
                throw new NotImplementedException();
#endif
            return Kernel32.MoveFileEx(source, destination, flags);
        }

#if NET
        [SupportedOSPlatform("windows")]
#endif
        private static bool TestAccessRightsOnWindows(IDirectoryInfo directoryInfo, FileSystemRights accessRights)
        {
            var acl = directoryInfo.GetAccessControl();
            var rules = acl.GetAccessRules(true, true,
                // If Windows 7
                Environment.OSVersion.VersionString.StartsWith("6.1")
                    ? typeof(SecurityIdentifier)
                    : typeof(NTAccount));

            var currentUser = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(currentUser);
            foreach (AuthorizationRule rule in rules)
            {
                var fsAccessRule = rule as FileSystemAccessRule;
                if (fsAccessRule == null)
                    continue;

                if ((fsAccessRule.FileSystemRights & accessRights) > 0)
                {
                    var ntAccount = rule.IdentityReference as NTAccount;
                    if (ntAccount == null)
                        continue;

                    if (principal.IsInRole(ntAccount.Value))
                    {
                        return fsAccessRule.AccessControlType != AccessControlType.Deny;
                    }
                }
            }

            return false;
        }

        private static bool TestAccessRightsOnNonWindows()
        {
            throw new NotImplementedException();
        }
    }
}
