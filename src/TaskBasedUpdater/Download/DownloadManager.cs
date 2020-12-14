using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.FileSystem;

namespace TaskBasedUpdater.Download
{
    internal class DownloadManager : IDownloadManager
    {
        private readonly ILogger? _logger;
        private static DownloadManager? _instance;
        private readonly List<IDownloadEngine> _allEngines = new List<IDownloadEngine>();
        private readonly List<IDownloadEngine> _defaultEngines = new List<IDownloadEngine>();

        public static IDownloadManager Instance => _instance ??= new DownloadManager();

        public IEnumerable<string> DefaultEngines
        {
            get
            {
                return _defaultEngines.Select(e => e.Name);
            }
            set
            {
                var preferredEngines = GetPreferredEngines(_allEngines, value);
                _defaultEngines.Clear();
                _defaultEngines.AddRange(preferredEngines);
            }
        }

        public IEnumerable<string> AllEngines
        {
            get
            {
                return _allEngines.Select(e => e.Name);
            }
        }

        public IEnumerable<IDownloadEngine> Engines => _allEngines;

        internal int SleepDurationBetweenRetries { get; set; }


        private DownloadManager()
        {
            AddDownloadEngine(new WebClientDownloader(null));
            AddDownloadEngine(new FileDownloader());
            DefaultEngines = _allEngines.Select(e => e.Name);
            // TODO: split-projects
            //SleepDurationBetweenRetries = UpdateConfiguration.Instance.DownloadRetryDelay;
        }

        public Task<DownloadSummary> DownloadAsync(Uri uri, Stream outputStream, ProgressUpdateCallback progress, CancellationToken cancellationToken,
            ProductComponent? productComponent = default, bool verify = false)
        {
            _logger?.LogTrace($"Download requested: {uri.AbsoluteUri}");
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));
            if (!outputStream.CanWrite)
                throw new InvalidOperationException("Input stream must be writable.");
            if (!uri.IsFile && !uri.IsUnc)
            {
                if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase) && !string.Equals(uri.Scheme, "ftp", StringComparison.OrdinalIgnoreCase))
                {
                    var argumentException = new ArgumentException($"Uri scheme '{uri.Scheme}' is not supported.");
                    _logger?.LogTrace($"Uri scheme '{uri.Scheme}' is not supported. {argumentException.Message}");
                    throw argumentException;
                }
                if (uri.AbsoluteUri.Length < 7)
                {
                    var argumentException = new ArgumentException($"Invalid Uri: {uri.AbsoluteUri}.");
                    _logger?.LogTrace($"The Uri is too short: {uri.AbsoluteUri}; {argumentException.Message}");
                    throw argumentException;
                }
            }

            try
            {
                var engines = GetSuitableEngines(_defaultEngines, uri);
                return Task.Factory.StartNew(() => DownloadWithRetry(engines, uri, outputStream, progress,
                        cancellationToken, productComponent, verify), cancellationToken,
                    TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"Unable to get download engine: {ex.Message}");
                throw;
            }
        }

        private DownloadSummary DownloadWithRetry(IDownloadEngine[] engines, Uri uri, Stream outputStream,
            ProgressUpdateCallback progress, CancellationToken cancellationToken, ProductComponent? productComponent = null,
            bool verify = false)
        {
            var failureList = new List<DownloadFailureInformation>();
            foreach (var engine in engines)
            {
                var position = outputStream.Position;
                var length = outputStream.Length;
                try
                {
                    _logger.LogTrace($"Attempting download '{uri.AbsoluteUri}' using engine '{engine.Name}'");
                    var engineSummary = engine.Download(uri, outputStream,
                        status =>
                        {
                            progress?.Invoke(new ProgressUpdateStatus(engine.Name, status.BytesRead, status.TotalBytes,
                                status.BitRate));
                        }, cancellationToken,
                        productComponent);
                    // TODO: split-projects
                    if (outputStream.Length == 0 /*&& !UpdateConfiguration.Instance.AllowEmptyFileDownload*/)
                    {
                        var exception = new UpdaterException($"Empty file downloaded on '{uri}'.");
                        _logger?.LogError(exception, exception.Message);
                        throw exception;
                    }

                    if (verify && outputStream.Length != 0)
                    {
                        if (productComponent is null)
                        {
                            // TODO: split-projects
                            //if (UpdateConfiguration.Instance.ValidationPolicy == ValidationPolicy.Enforce)
                            //    throw new ValidationFailedException(DownloadResult.MissingOrInvalidValidationContext,
                            //        "Unable to get necessary validation data because download context is null.");
                        }
                        else
                        {
                            var validationContext = productComponent.OriginInfo?.ValidationContext;
                            var valid = validationContext?.Verify();

                            // TODO: split-projects
                            //if ((!valid.HasValue || !valid.Value) && UpdateConfiguration.Instance.ValidationPolicy ==
                            //    ValidationPolicy.Enforce)
                            //    throw new ValidationFailedException(DownloadResult.MissingOrInvalidValidationContext,
                            //        $"Update Item'{updateItem.Name}' is missing or has an invalid ValidationInfo");

                            if (valid.HasValue && valid.Value)
                            {
                                var validationResult = HashVerifier.Verify(outputStream, validationContext);
                                engineSummary.ValidationResult = validationResult;
                                if (validationResult == ValidationResult.HashMismatch)
                                {
                                    var exception = new ValidationFailedException(DownloadResult.HashMismatch,
                                        $"Hash on downloaded file '{uri.AbsoluteUri}' does not match expected value.");
                                    _logger?.LogError(exception, exception.Message);
                                    throw exception;
                                }
                            }
                            else
                                _logger.LogTrace(
                                    $"Skipping validation because validation context of Update Item {productComponent.Name} is not valid.");
                        }
                    }

                    _logger?.LogInformation($"Download of '{uri.AbsoluteUri}' succeeded using engine '{engine.Name}'");
                    PreferredDownloadEngines.Instance.LastSuccessfulEngineName = engine.Name;
                    engineSummary.DownloadEngine = engine.Name;
                    return engineSummary;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    failureList.Add(new DownloadFailureInformation(ex, engine.Name));
                    _logger.LogTrace($"Download failed using {engine.Name} engine. {ex}");

                    if (engine.Equals(engines.LastOrDefault()))
                        throw new DownloadFailureException(failureList);

                    cancellationToken.ThrowIfCancellationRequested();
                    outputStream.SetLength(length);
                    outputStream.Seek(position, SeekOrigin.Begin);
                    var millisecondsTimeout = SleepDurationBetweenRetries;
                    if (millisecondsTimeout < 0)
                        millisecondsTimeout = 0;
                    _logger.LogTrace($"Sleeping {millisecondsTimeout} before retrying download.");
                    Thread.Sleep(millisecondsTimeout);
                }
            }

            return null;
        }

        private void AddDownloadEngine(IDownloadEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (_allEngines.Any(e => string.Equals(e.Name, engine.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Engine " + engine.Name + " already exists.");
            _allEngines.Add(engine);
            _defaultEngines.Add(engine);
        }

        private IDownloadEngine[] GetSuitableEngines(IEnumerable<IDownloadEngine> downloadEngines, Uri uri)
        {
            var source = uri.IsFile || uri.IsUnc ? DownloadSource.File : DownloadSource.Internet;
            var array = downloadEngines.Where(e => e.IsSupported(source)).ToArray();
            if (array.Length == 0)
            {
                _logger?.LogTrace("Unable to select suitable download engine.");
                throw new NoSuitableEngineException("Can not download. No suitable download engine found.");
            }
            return PreferredDownloadEngines.Instance.GetEnginesInPriorityOrder(array).ToArray();
        }

        private static IEnumerable<IDownloadEngine> GetPreferredEngines(IReadOnlyCollection<IDownloadEngine> engines, IEnumerable<string> engineOrder)
        {
            if (engineOrder == null)
                throw new ArgumentNullException("Invalid engine prefernece.");
            if (engineOrder.Count() > engines.Count())
                throw new ArgumentException("Default engines can't be more than all available engines.");
            var list = new List<IDownloadEngine>();
            foreach (var name in engineOrder)
            {
                var downloadEngine = engines.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
                if (downloadEngine == null)
                    throw new InvalidOperationException("Engine " + name + " not found among registered engines.");
                list.Add(downloadEngine);
            }
            return list;
        }
    }
}