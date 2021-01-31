using System;

namespace ProductUpdater.Restart
{
    public interface IRestartNotificationService
    {
        event EventHandler<EventArgs> RebootRequired;

        bool RestartRequired { get; }

        void NotifyRestartRequired();
    }
}