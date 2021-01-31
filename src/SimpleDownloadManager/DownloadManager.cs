using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SimpleDownloadManager.Configuration;
using SimpleDownloadManager.Engines;
using SimpleDownloadManager.Verification;
using Validation;

namespace SimpleDownloadManager
{
    public class DownloadManager : IDownloadManager
    {
        private readonly IDownloadManagerServices _services;

        private readonly ILogger? _logger;
        private readonly List<IDownloadEngine> _allEngines = new();
        private readonly List<IDownloadEngine> _defaultEngines = new();
        private readonly PreferredDownloadEngines _preferredDownloadEngines = new();
        private IVerifier? _verifier;

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

        public DownloadManager(DownloadManagerConfiguration configuration) : 
            this(new DefaultDownloadManagerServices(), configuration)
        {
            
        }
        
        public DownloadManager(IDownloadManagerServices services, DownloadManagerConfiguration configuration)
        {
           
            Requires.NotNull(services, nameof(services));
            Requires.NotNull(configuration, nameof(configuration));
            _logger = services.Logger;
            _services = services;
            AddDownloadEngine(new WebClientDownloader(services));
            AddDownloadEngine(new FileDownloader(services));
            DefaultEngines = _allEngines.Select(e => e.Name);
            Configuration = configuration;
        }

        public DownloadManagerConfiguration Configuration { get; }

        public Task<DownloadSummary> DownloadAsync(Uri uri, Stream outputStream, ProgressUpdateCallback? progress, CancellationToken cancellationToken,
            VerificationContext? productComponent = default)
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
                        cancellationToken, productComponent), cancellationToken,
                    TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                _logger.LogTrace($"Unable to get download engine: {ex.Message}");
                throw;
            }
        }
        
        public void AddDownloadEngine(IDownloadEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (_allEngines.Any(e => string.Equals(e.Name, engine.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Engine " + engine.Name + " already exists.");
            _allEngines.Add(engine);
            _defaultEngines.Add(engine);
        }

        private DownloadSummary DownloadWithRetry(IDownloadEngine[] engines, Uri uri, Stream outputStream,
            ProgressUpdateCallback? progress, CancellationToken cancellationToken, VerificationContext? verificationContext = null)
        {
            if (Configuration.VerificationPolicy == VerificationPolicy.Enforce && verificationContext is null)
            {
                var exception = new VerificationFailedException(VerificationResult.VerificationContextError,
                    "No verification context available to verify the download.");
                _logger?.LogError(exception, exception.Message);
                throw exception;
            }
            
            var failureList = new List<DownloadFailureInformation>();
            foreach (var engine in engines)
            {
                var position = outputStream.Position;
                var length = outputStream.Length;
                try
                {
                    _logger?.LogTrace($"Attempting download '{uri.AbsoluteUri}' using engine '{engine.Name}'");
                    var engineSummary = engine.Download(uri, outputStream,
                        status =>
                        {
                            progress?.Invoke(new ProgressUpdateStatus(engine.Name, status.BytesRead, status.TotalBytes, status.BitRate));
                        }, cancellationToken);
                    if (outputStream.Length == 0 && !Configuration.AllowEmptyFileDownload)
                    {
                        var exception = new Exception($"Empty file downloaded on '{uri}'.");
                        _logger?.LogError(exception, exception.Message);
                        throw exception;
                    }

                    if (Configuration.VerificationPolicy != VerificationPolicy.Skip &&
                        verificationContext.HasValue &&
                        outputStream.Length != 0)
                    {
                        _verifier ??= _services.DownloadVerifier;

                        var valid = verificationContext.Value.Verify();
                        if (valid)
                        {
                            var verificationResult= _verifier.Verify(outputStream, verificationContext.Value);
                            engineSummary.ValidationResult = verificationResult;
                            if (verificationResult != VerificationResult.Success)
                            {
                                var exception = new VerificationFailedException(verificationResult,
                                    $"Hash on downloaded file '{uri.AbsoluteUri}' does not match expected value.");
                                _logger?.LogError(exception, exception.Message);
                                throw exception;
                            }
                        }
                        else
                        {
                            if (Configuration.VerificationPolicy == VerificationPolicy.Optional ||
                                Configuration.VerificationPolicy == VerificationPolicy.Enforce)
                                throw new VerificationFailedException(VerificationResult.VerificationContextError,
                                    "Download is missing or has an invalid VerificationContext");
                            _logger?.LogTrace("Skipping validation because verification context of is not valid.");
                        }
                    }

                    _logger?.LogInformation($"Download of '{uri.AbsoluteUri}' succeeded using engine '{engine.Name}'");
                    _preferredDownloadEngines.LastSuccessfulEngineName = engine.Name;

                    return engineSummary with {DownloadEngine = engine.Name};
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    failureList.Add(new DownloadFailureInformation(ex, engine.Name));
                    _logger?.LogTrace($"Download failed using {engine.Name} engine. {ex}");

                    if (engine.Equals(engines.LastOrDefault()))
                        throw new DownloadFailureException(failureList);

                    cancellationToken.ThrowIfCancellationRequested();
                    outputStream.SetLength(length);
                    outputStream.Seek(position, SeekOrigin.Begin);
                    var millisecondsTimeout = Configuration.DownloadRetryDelay;
                    if (millisecondsTimeout <= 0)
                        continue;
                    
                    _logger?.LogTrace($"Sleeping {millisecondsTimeout} before retrying download.");
                    Thread.Sleep(millisecondsTimeout);
                }
            }

            return null!;
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
            return _preferredDownloadEngines.GetEnginesInPriorityOrder(array).ToArray();
        }

        private static IEnumerable<IDownloadEngine> GetPreferredEngines(IReadOnlyCollection<IDownloadEngine> engines, IEnumerable<string> engineOrder)
        {
            if (engineOrder == null)
                throw new ArgumentNullException("Invalid engine prefernece.");
            if (engineOrder.Count() > engines.Count)
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