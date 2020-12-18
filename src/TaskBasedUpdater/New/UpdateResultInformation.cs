namespace TaskBasedUpdater.New
{
    public record UpdateResultInformation
    {
        public static UpdateResultInformation NoUpdate = new()
        {
            Result = UpdateResult.NoUpdate,
            Message = "No update required."
        };

        public static UpdateResultInformation Success = new()
        {
            Result = UpdateResult.Success,
            Message = "Update successful."
        };

        public UpdateResult Result { get; init; }

        public bool RequiresUserNotification { get; init; }

        public string? Message { get; init; }

        public override string ToString()
        {
            return $"Result: {Result}, Message: {Message}";
        }
    }
}