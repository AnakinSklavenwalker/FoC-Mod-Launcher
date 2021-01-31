using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleDownloadManager;
using SimpleDownloadManager.Configuration;
using Validation;

namespace ProductUpdater.New
{
    public class ManifestDownloadEngine
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger? _logger;

        public ManifestDownloadEngine(IFileSystem fileSystem, ILogger? logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }
        
        public IFileInfo DownloadManifest(Uri manifestUri, CancellationToken token = default)
        {
            _logger?.LogTrace($"Downloading \"{manifestUri}\"");
            var downloadTask = DownloadManifestAsync(manifestUri, token);
            try
            {
                downloadTask.Wait(token);
            }
            catch (AggregateException e)
            {
                _logger?.LogError(e, e.Message);
            }
            return downloadTask.Result;
        }

        public async Task<IFileInfo> DownloadManifestAsync(Uri manifestUri, CancellationToken token = default)
        {
            Requires.NotNull(manifestUri, nameof(manifestUri));

            var tempFile = _fileSystem.Path.GetTempFileName();
            using var file = _fileSystem.File.Create(tempFile);
            await new DownloadManager(DownloadManagerConfiguration.Default)
                .DownloadAsync(manifestUri, file, null, token);
            
            return _fileSystem.FileInfo.FromFileName(tempFile);
        }
    }
}