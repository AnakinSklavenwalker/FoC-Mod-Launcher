using System;

namespace ProductUpdater.Restart
{
    internal class RestartNotificationService : IRestartNotificationService
    {
        public event EventHandler<EventArgs> RebootRequired;
        public bool RestartRequired { get; }
        public void NotifyRestartRequired()
        {
            throw new NotImplementedException();
        }
    }
}