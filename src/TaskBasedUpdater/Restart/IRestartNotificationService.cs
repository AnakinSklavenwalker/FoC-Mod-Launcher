using System;

namespace TaskBasedUpdater.Restart
{
    public interface IRestartNotificationService
    {
        event EventHandler<EventArgs> RebootRequired;

        bool RestartRequired { get; }

        void NotifyRestartRequired();
    }
}