using System;

namespace TaskBasedUpdater.Restart
{
    public struct PendingHandledResult
    {
        public string Message { get; }
        public HandlePendingItemStatus Status { get; }

        public PendingHandledResult(HandlePendingItemStatus status, string message)
        {
            Message = message;
            Status = status;
        }

        public PendingHandledResult(HandlePendingItemStatus status) : this(status, String.Empty)
        {
        }
    }
}