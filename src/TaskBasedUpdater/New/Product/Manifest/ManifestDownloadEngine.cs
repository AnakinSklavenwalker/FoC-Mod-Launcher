using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Download;
using Validation;

namespace TaskBasedUpdater.New.Product.Manifest
{
    public class ManifestDownloadEngine
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;

        public ManifestDownloadEngine(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService<ILogger>();
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
            var fileSystem = _serviceProvider.GetRequiredService<IFileSystem>();

            var tempFile = fileSystem.Path.GetTempFileName();
            using var file = fileSystem.File.Create(tempFile);
            await DownloadManager.Instance.DownloadAsync(manifestUri, file, null, token);
            
            return fileSystem.FileInfo.FromFileName(tempFile);
        }
    }
}