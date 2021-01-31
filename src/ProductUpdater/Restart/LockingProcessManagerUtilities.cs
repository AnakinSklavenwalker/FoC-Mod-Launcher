using System;
using System.Diagnostics;
using System.Linq;

namespace ProductUpdater.Restart
{
    internal static class LockingProcessManagerUtilities
    {
        public static bool ProcessesContainsSelf(this ILockingProcessManager processManager)
        {
            if (processManager is null)
                throw new ArgumentNullException(nameof(processManager));
            var currentProcess = Process.GetCurrentProcess();
            return processManager.GetProcesses().Any(x => x.Id.Equals(currentProcess.Id));
        }

        public static bool ProcessesContainsOnlySelf(this ILockingProcessManager processManager)
        {
            if (processManager is null)
                throw new ArgumentNullException(nameof(processManager));
            return processManager.GetProcesses().Count() <= 1 && processManager.ProcessesContainsSelf();
        }
    }
}