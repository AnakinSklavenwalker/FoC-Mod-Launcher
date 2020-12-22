using System;
using System.IO;
using System.IO.Abstractions;
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



        private static readonly char DirectorySeparatorChar = '\\';

        public static bool FileExists(FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));
            return File.Exists(fileInfo.FullName);
        }

        public static long GetDriveFreeSpace(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            return new DriveInfo(Path.GetPathRoot(path)).AvailableFreeSpace;
        }

        public static void DeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            File.Delete(path);
        }

        public static bool ContainsPath(string fullPath, string path)
        {
            return ContainsPath(fullPath, path, false);
        }

        public static bool ContainsPath(string fullPath, string path, bool excludeSame)
        {
            try
            {
                if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(path))
                    return false;
                fullPath = Path.GetFullPath(fullPath);
                path = Path.GetFullPath(path);
                fullPath = AddBackslashIfNotPresent(fullPath);
                path = AddBackslashIfNotPresent(path);
                var flag = fullPath.StartsWith(path, StringComparison.OrdinalIgnoreCase);
                return flag & excludeSame ? !fullPath.Equals(path, StringComparison.OrdinalIgnoreCase) : flag;
            }
            catch
            {
                return false;
            }
        }

        public static string AddBackslashIfNotPresent(string path)
        {
            if (!string.IsNullOrEmpty(path) && path[path.Length - 1] != DirectorySeparatorChar)
                path += DirectorySeparatorChar.ToString();
            return path;
        }

        public static Stream CreateFileWithRetry(string path, int retryCount = 2, int retryDelay = 500)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            Stream stream = null;
            ExecuteFileActionWithRetry(retryCount, retryDelay, () => stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None));
            return stream;
        }

        public static void CopyFileWithRetry(string source, string destination, int retryCount = 2, int retryDelay = 500)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentNullException(nameof(destination));
            ExecuteFileActionWithRetry(retryCount, retryDelay, () => File.Copy(source, destination, true));
        }

        public static bool DeleteFileWithRetry(FileInfo file, out bool rebootRequired, 
            bool rebootOk = false, int retryCount = 2, int retryDelay = 500, Func<Exception, int, bool> errorAction = null)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            return DeleteFileWithRetry(file.FullName, out rebootRequired, rebootOk, retryCount, retryDelay, errorAction);
        }

        public static FileAttributes GetFileAttributes(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            return (FileAttributes) File.GetAttributes(path);
        }

        public static void SetFileAttributes(string path, FileAttributes attributeValues, FileAttributes attributeMask)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            var fileInfo = new FileInfo(path);
            var attributes = (int)fileInfo.Attributes;
            var fileAttributes = attributeValues & attributeMask;
            var num = (int)~attributeMask;
            fileInfo.Attributes = (System.IO.FileAttributes)((FileAttributes)(attributes & num) | fileAttributes);
            fileInfo.Refresh();
        }

        public static bool DeleteFileWithRetry(string path, out bool restartRequired,
            bool restartOk = false, int retryCount = 2, int retryDelay = 500, Func<Exception, int, bool>? errorAction = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            restartRequired = false;
            if (!File.Exists(path))
                return true;

            var flag = ExecuteFileActionWithRetry(retryCount, retryDelay, () => DeleteFile(path), !restartOk, (ex, attempt) =>
            {
                if (ex is UnauthorizedAccessException)
                {
                    if (attempt == 0)
                    {
                        if ((GetFileAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            SetFileAttributes(path, FileAttributes.None, FileAttributes.ReadOnly);
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
                return flag;

            restartRequired = true;
            return false;
        }
        
        // Based on: https://stackoverflow.com/questions/1410127/c-sharp-test-if-user-has-write-access-to-a-folder
        public static bool UserHasDirectoryAccessRights(string path, FileSystemRights accessRights, bool create = false)
        {
            var isInRoleWithAccess = false;
            try
            {
                var di = new DirectoryInfo(path);

                if (!di.Exists && create)
                    di.Create();

                var acl = di.GetAccessControl();

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
                            if (fsAccessRule.AccessControlType == AccessControlType.Deny)
                                return false;
                            isInRoleWithAccess = true;
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            return isInRoleWithAccess;
        }

        public static bool MoveFile(string source, string destination, bool replace = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var isDirectory = Directory.Exists(source);
            if (!replace)
            {
                if (isDirectory)
                    Directory.Move(source, destination);
                else
                    File.Move(source, destination);
                return true;
            }
            var flags = MoveFileFlags.MoveFileWriteThrough;
            flags |= MoveFileFlags.MoveFileReplaceExisting;
            if (!isDirectory)
                flags |= MoveFileFlags.MoveFileCopyAllowed;
            return MoveFileEx(source, destination, flags);
        }

        internal static string? GetPathRoot(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var pathRoot = Path.GetPathRoot(Environment.ExpandEnvironmentVariables(filePath));
                if (!string.IsNullOrEmpty(pathRoot))
                    return pathRoot;
            }
            return null;
        }


        private static bool ExecuteFileActionWithRetry(int retryCount, int retryDelay, Action fileAction,
            bool throwOnFailure = true, Func<Exception, int, bool> errorAction = null)
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
            return Kernel32.MoveFileEx(source, destination, flags);
        }
    }

    public enum FileAttributes
    {
        None = 0,
        ReadOnly = 1,
        Hidden = 2,
        Directory = 16
    }
}
