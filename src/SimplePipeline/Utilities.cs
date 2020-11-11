using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace SimplePipeline
{
    internal static class Utilities
    {
        internal static readonly string GlobalPipelineMutex = $"Global\\{Process.GetCurrentProcess().ProcessName}";

        internal static Mutex CheckAndSetGlobalMutex(string? name = null)
        {
            var mutex = EnsureMutex(name);

            if (mutex == null)
                throw new InvalidOperationException("Unable to acquire Mutex");
            return mutex;
        }

        internal static Mutex? EnsureMutex(string? name = null)
        {
            return EnsureMutex(name, TimeSpan.Zero);
        }

        internal static Mutex? EnsureMutex(string? name, TimeSpan timeout)
        {
            name ??= GlobalPipelineMutex;
            Mutex mutex;
            try
            {
                mutex = Mutex.OpenExisting(name);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
                    mutex = CreateSecurityMutexWindows(name);
                else
                    throw new NotImplementedException("No mutex for non-Windows Systems.");
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

        private static Mutex CreateSecurityMutexWindows(string name)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new InvalidOperationException("Creating a secure mutex is only allowed on Windows Systems.");

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
    }
}