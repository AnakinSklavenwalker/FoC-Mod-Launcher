using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace TaskBasedUpdater
{
    public static class UpdaterUtilities
    {
        internal static readonly string UpdaterMutex = $"Global\\{Process.GetCurrentProcess().ProcessName}";

        public static Version GetAssemblyVersion(string file)
        {
            var assembly = AssemblyName.GetAssemblyName(file);
            return assembly.Version;
        }

        public static Version GetAssemblyFileVersion(string file)
        {
            var version = FileVersionInfo.GetVersionInfo(file).FileVersion;
            return Version.Parse(version);
        }
        
        // https://stackoverflow.com/a/9995303
        internal static byte[] HexToArray(string input)
        {
            if (input.Length % 2 != 0)
                throw new NotSupportedException("string input lenght is wrong");
            input = input.ToLower();
            var arr = new byte[input.Length >> 1];
            for (var i = 0; i < input.Length >> 1; ++i)
                arr[i] = (byte)((GetHexVal(input[i << 1]) << 4) + (GetHexVal(input[(i << 1) + 1])));
            return arr;
        }

        internal static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                extensions = new[] {".*"};
            var files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        private static int GetHexVal(char hex)
        {
            var val = (int)hex;
            return val - (val < 58 ? 48 : 87);
        }

        internal static Mutex CheckAndSetGlobalMutex(string name = null)
        {
            var mutex = EnsureMutex(name);

            if (mutex == null)
                throw new InvalidOperationException("Setup can not run");
            return mutex;
        }

        internal static Mutex? EnsureMutex(string name = null)
        {
            return EnsureMutex(name, TimeSpan.Zero);
        }

        internal static Mutex? EnsureMutex(string name, TimeSpan timeout)
        {
#if NET
            if (!OperatingSystem.IsWindows())
                throw new NotSupportedException("Creating a mutex is only supported on windows.");
#endif
            name ??= UpdaterMutex;
            Mutex mutex;
            try
            {
                mutex = Mutex.OpenExisting(name);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                var mutexSecurity = new MutexSecurity();
                var rule = new MutexAccessRule(securityIdentifier, MutexRights.FullControl, AccessControlType.Allow);
                mutexSecurity.AddAccessRule(rule);
#if NET
                return MutexAcl.Create(false, name, out _, mutexSecurity);
#else
                return new Mutex(false, name, out _, mutexSecurity);
#endif
            }

            bool mutexAbandoned;
            try
            {
                mutexAbandoned = mutex.WaitOne(timeout);
            }
            catch (AbandonedMutexException)
            {
                mutexAbandoned = true;
            }
            return mutexAbandoned ? mutex : null;
        }

        public static long CopyFileToStream(string filePath, Stream outStream, ProgressUpdateCallback progress, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            var downloadSize = 0L;
            var array = new byte[32768];
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var readSize = fileStream.Read(array, 0, array.Length);
                cancellationToken.ThrowIfCancellationRequested();
                if (readSize <= 0)
                    break;
                outStream.Write(array, 0, readSize);
                downloadSize += readSize;
                progress?.Invoke(new ProgressUpdateStatus(downloadSize, fileStream.Length, 0.0));
            }
            if (downloadSize != fileStream.Length)
                throw new IOException("Internal error copying streams. Total read bytes does not match stream Length.");
            return downloadSize;
        }
    }
}
