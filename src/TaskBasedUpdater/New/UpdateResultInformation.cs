using TaskBasedUpdater.New;

namespace TaskBasedUpdater
{
    public class UpdateResultInformation : IUpdateResultInformation
    {
        public UpdateResult Result { get; set; }

        public bool RequiresUserNotification { get; set; }

        public string? Message { get; set; }

        public override string ToString()
        {
            return $"Result: {Result}, Message: {Message}";
        }
    }
}