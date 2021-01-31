using System;
using System.Collections.Generic;

namespace ProductUpdater.Restart
{
    public interface ILockingProcessManager : IDisposable
    {
        void Register(IEnumerable<string> files = null, IEnumerable<ILockingProcessInfo> processes = null);

        void Shutdown(WindowsRestartManagerShutdown action = WindowsRestartManagerShutdown.ForceShutdown);

        void Restart();

        IEnumerable<ILockingProcessInfo> GetProcesses();
    }
}