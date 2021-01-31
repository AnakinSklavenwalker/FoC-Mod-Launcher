using System;

namespace ProductUpdater.Restart
{
    [Flags]
    public enum ApplicationStatus
    {
        Unknown = 0,
        Running = 1,
        Stopped = 2,
        StoppedOther = 4,
        Restarted = 8,
        ErrorOnStop = 16,
        ErrorOnRestart = 32,
        ShutdownMasked = 64,
        RestartMasked = 128,
    }
}