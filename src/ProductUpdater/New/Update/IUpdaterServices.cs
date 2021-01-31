using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ProductUpdater.Restart;
using SimpleDownloadManager;

namespace ProductUpdater.New.Update
{
    public interface IUpdaterServices
    {

        IRestartNotificationService RestartNotificationService { get; }

        IDownloadManager DownloadManager { get; }

        IFileSystem FileSystem { get; }

        ILogger? Logger { get; }

    }
}