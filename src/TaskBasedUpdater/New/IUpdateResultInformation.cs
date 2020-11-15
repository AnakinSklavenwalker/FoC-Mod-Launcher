namespace TaskBasedUpdater.New
{
    public interface IUpdateResultInformation
    {
        UpdateResult Result { get; }

        public bool RequiresUserNotification { get; }

        public string? Message { get; }
    }
}