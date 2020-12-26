using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
#if NET
using System.Buffers;
#endif

namespace TaskBasedUpdater.Download
{
    internal class FileDownloader : DownloadEngineBase
    {
        private readonly IServiceProvider _serviceProvider;

        public FileDownloader(IServiceProvider serviceProvider) : base(DownloadEngineNames.FileEngine, new DownloadSource[1])
        {
            _serviceProvider = serviceProvider;
        }

        protected override DownloadSummary DownloadCore(Uri uri, Stream outputStream, ProgressUpdateCallback? progress,
            CancellationToken cancellationToken)
        {
            if (!uri.IsFile && !uri.IsUnc)
                throw new ArgumentException("Expected file or UNC path", nameof(uri));
            return new DownloadSummary
            {
                DownloadedSize = CopyFileToStream(uri.LocalPath, outputStream, progress, cancellationToken)
            };
        }

        private long CopyFileToStream(string filePath, Stream outStream, ProgressUpdateCallback? progress,
            CancellationToken cancellationToken)
        {
            var fileSystem = _serviceProvider.GetRequiredService<IFileSystem>();
            if (!fileSystem.File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));


            var downloadSize = 0L;
            using var fileStream = fileSystem.FileStream.Create(filePath, FileMode.Open, FileAccess.Read);
            if (progress is null)
            {
#if NET
                fileStream.CopyToAsync(outStream, cancellationToken).Wait(cancellationToken);
#else
                fileStream.CopyToAsync(outStream, 32768, cancellationToken).Wait(cancellationToken);
#endif
                downloadSize = outStream.Length;
            }
            else
            {
#if NET
                var array = ArrayPool<byte>.Shared.Rent(32768);
                try
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var readSize = fileStream.Read(array, 0, array.Length);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (readSize <= 0)
                            break;
                        outStream.Write(array, 0, readSize);
                        downloadSize += readSize;
                        progress.Invoke(new ProgressUpdateStatus(downloadSize, fileStream.Length, 0.0));
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(array);
                }
#else
                byte[] array = new byte[32768];
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var readSize = fileStream.Read(array, 0, array.Length);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (readSize <= 0)
                        break;
                    outStream.Write(array, 0, readSize);
                    downloadSize += readSize;
                    progress?.Invoke(new ProgressUpdateStatus(downloadSize, fileStream.Length, 0.0));
                }
#endif
            }

            if (downloadSize != fileStream.Length)
                throw new IOException("Internal error copying streams. Total read bytes does not match stream Length.");
            return downloadSize;
        }
    }
}