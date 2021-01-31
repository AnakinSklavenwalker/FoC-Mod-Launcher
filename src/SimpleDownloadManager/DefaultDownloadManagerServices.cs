using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using SimpleDownloadManager.Verification;

namespace SimpleDownloadManager
{
    public class DefaultDownloadManagerServices : IDownloadManagerServices
    {
        private IVerifier? _verifier;

        public IFileSystem FileSystem { get; } = new FileSystem();

        public ILogger? Logger => null;

        public IVerifier DownloadVerifier
        {
            get
            {
                _verifier ??= new HashVerifier(FileSystem, Logger);
                return _verifier;
            }
        }
    }
}