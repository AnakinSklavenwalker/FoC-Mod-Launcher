using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace TaskBasedUpdater
{
    public static class UpdaterUtilities
    {
        internal static readonly string UpdaterMutex = $"Global\\{Process.GetCurrentProcess().ProcessName}";

        // https://stackoverflow.com/a/9995303
        internal static byte[] HexToArray(string input)
        {
            if (input.Length % 2 != 0)
                throw new NotSupportedException("string input lenght is wrong");
            input = input.ToLower();
            var arr = new byte[input.Length >> 1];
            for (var i = 0; i < input.Length >> 1; ++i)
                arr[i] = (byte) ((GetHexVal(input[i << 1]) << 4) + (GetHexVal(input[(i << 1) + 1])));
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
            var val = (int) hex;
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
    }
}
