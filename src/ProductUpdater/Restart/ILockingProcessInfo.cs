using System;

namespace ProductUpdater.Restart
{
    public interface ILockingProcessInfo
    {
        int Id { get; }

        DateTime StartTime { get; }

        string Description { get; }

        string ServiceName { get; }

        ApplicationType ApplicationType { get; }

        ApplicationStatus ApplicationStatus { get; }

        bool IsRestartable { get; }
    }
}