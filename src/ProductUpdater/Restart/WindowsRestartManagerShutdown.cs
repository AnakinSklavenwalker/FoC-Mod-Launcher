using System;

namespace ProductUpdater.Restart
{
    [Flags]
    public enum WindowsRestartManagerShutdown
    {
        ForceShutdown = 1,
        ShutdownOnlyRegistered = 16,
    }
}