using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Restart;

namespace TaskBasedUpdater.New.Update
{
    public interface IUpdaterServices
    {

        IRestartNotificationService RestartNotificationService { get; }

        IFileSystem FileSystem { get; }

        ILogger? Logger { get; }

    }
}