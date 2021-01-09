namespace TaskBasedUpdater.New.Update.Service
{
    public enum UpdateResult
    {
        Failed,
        Success,
        SuccessRestartRequired,
        Cancelled,
        NoUpdate
    }
}