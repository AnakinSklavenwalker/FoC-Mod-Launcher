﻿using System;
using System.Collections.Generic;

namespace FocLauncherHost.Updater.Restart
{
    public interface ILockingProcessManager : IDisposable
    {
        void Register(IEnumerable<string> files = null);

        void Shutdown(WindowsRestartManagerShutdown action = WindowsRestartManagerShutdown.ForceShutdown);

        void Restart();

        IEnumerable<ILockingProcessInfo> GetProcesses();
    }
}