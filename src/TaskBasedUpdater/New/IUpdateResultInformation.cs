namespace TaskBasedUpdater.New
{
    public interface IUpdateResultInformation
    {
        UpdateResult Result { get; }

        public bool RequiresUserNotification { get; set; }

        public string? Message { get; set; }
    }
}