using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using SimpleDownloadManager.Verification;

namespace SimpleDownloadManager
{
    public interface IDownloadManagerServices 
    {
        IFileSystem FileSystem { get; }

        ILogger? Logger { get; }

        IVerifier DownloadVerifier { get; }
    }
}