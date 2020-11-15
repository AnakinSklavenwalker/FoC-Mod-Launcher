using TaskBasedUpdater.New;

namespace TaskBasedUpdater
{
    public class UpdateResultInformation : IUpdateResultInformation
    {
        public static IUpdateResultInformation NoUpdate = new UpdateResultInformation
        {
            Result = UpdateResult.NoUpdate,
            Message = "No update required."
        };

        public static IUpdateResultInformation Success = new UpdateResultInformation
        {
            Result = UpdateResult.Success,
            Message = "Update successful."
        };

        public UpdateResult Result { get; set; }

        public bool RequiresUserNotification { get; set; }

        public string? Message { get; set; }

        public override string ToString()
        {
            return $"Result: {Result}, Message: {Message}";
        }
    }
}