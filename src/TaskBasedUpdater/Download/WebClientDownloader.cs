using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskBasedUpdater.Component;
using Validation;

namespace TaskBasedUpdater.Download
{
    internal class WebClientDownloader: DownloadEngineBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger? _logger;
        private readonly DownloadHelpers _helper;

        static WebClientDownloader()
        {
            if (ServicePointManager.SecurityProtocol == 0)
                return;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public WebClientDownloader(IServiceProvider serviceProvider) : base("WebClient", new[] {DownloadSource.Internet})
        {
            Requires.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILogger>();
            _helper = new DownloadHelpers(serviceProvider);
        }

        protected override DownloadSummary DownloadCore(Uri uri, Stream outputStream, ProgressUpdateCallback progress,
            CancellationToken cancellationToken, IUpdateItem? updateItem)
        {
            var summary = new DownloadSummary();

            using var webResponse = GetWebResponse(uri, ref summary, out var webRequest, cancellationToken);
            if (webResponse != null)
            {
                var registration1 = cancellationToken.Register(() => webResponse.Close());
                try
                {
                    using var responseStream = webResponse.GetResponseStream();
                    var header = webResponse.Headers["Content-Length"];
                    if (string.IsNullOrEmpty(header))
                        throw new IOException("Error: Content-Length is missing from response header.");
                    var totalStreamLength = (long) Convert.ToInt32(header);
                    if (totalStreamLength.Equals(0L))
                        throw new IOException("Error: Response stream length is 0.");
                    var streamReadError = false;
                    var totalBytesRead = 0L;
                    var array = new byte[Math.Max(1024L, Math.Min(totalStreamLength, 32768L))];
                    var registration2 = cancellationToken.Register(() => webRequest.Abort());
                    try
                    {
                        while (true)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var bytesRead = responseStream.Read(array, 0, array.Length);
                            streamReadError = bytesRead < 0;
                            if (bytesRead <= 0)
                                break;
                            totalBytesRead += bytesRead;
                            outputStream.Write(array, 0, bytesRead);
                            if (totalStreamLength < totalBytesRead)
                                totalStreamLength = totalBytesRead;
                            progress?.Invoke(new ProgressUpdateStatus(totalBytesRead, totalStreamLength, 0));
                        }
                    }
                    finally
                    {
                        registration2.Dispose();
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    if (streamReadError)
                        throw new IOException("Internal error while downloading the stream.");
                    summary.DownloadedSize = totalBytesRead;
                    return summary;

                }
                catch (WebException ex)
                {
                    var message = cancellationToken.IsCancellationRequested
                        ? "DownloadCore failed along with a cancellation request."
                        : "DownloadCore failed";
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogTrace("WebClient error '" + ex.Status + "' with '" + uri.AbsoluteUri + "' - " +
                                     message);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    else
                    {
                        _logger.LogTrace("WebClient error '" + ex.Status + "' with '" + uri.AbsoluteUri + "'.");
                        throw;
                    }
                }
                finally
                {
                    registration1.Dispose();
                }
            }

            return summary;
        }

        private HttpWebResponse? GetWebResponse(Uri uri, ref DownloadSummary summary, out HttpWebRequest webRequest, CancellationToken cancellationToken)
        {
            var proxyResolution = ProxyResolution.Default;
            while (proxyResolution != ProxyResolution.Error)
            {
                HttpWebResponse? httpWebResponse = null;
                var successful = true;
                try
                {
                    webRequest = (HttpWebRequest) WebRequest.Create(uri);
                    webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    webRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
                    webRequest.KeepAlive = true;
                    webRequest.Timeout = 120000;

                    var requestCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                    webRequest.CachePolicy = requestCachePolicy;

                    switch (proxyResolution)
                    {
                        case ProxyResolution.DefaultCredentialsOrNoAutoProxy:
                            webRequest.UseDefaultCredentials = true;
                            break;
                        case ProxyResolution.NetworkCredentials:
                            webRequest.UseDefaultCredentials = false;
                            webRequest.Proxy = WebRequest.GetSystemWebProxy();
                            webRequest.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                            break;
                        case ProxyResolution.DirectAccess:
                            webRequest.Proxy = null;
                            break;
                    }
                    var registerWebRequest = webRequest;
                    using (cancellationToken.Register(() => registerWebRequest.Abort()))
                        httpWebResponse = (HttpWebResponse) webRequest.GetResponse();

                    var responseUri = httpWebResponse.ResponseUri.ToString();
                    if (!string.IsNullOrEmpty(responseUri) &&
                        !uri.ToString().EndsWith(responseUri, StringComparison.InvariantCultureIgnoreCase))
                    {
                        summary.FinalUri = responseUri;
                        _logger.LogTrace($"Uri '{uri}' + redirected to '{responseUri}'");
                    }

                    switch (httpWebResponse.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            summary.ProxyResolution = proxyResolution.ToString();
                            successful = false;
                            return httpWebResponse;
                        case HttpStatusCode.UseProxy:
                        case HttpStatusCode.ProxyAuthenticationRequired:
                        case HttpStatusCode.GatewayTimeout:
                            ++proxyResolution;
                            if (proxyResolution == ProxyResolution.Error)
                            {
                                _logger?.LogTrace($"WebResponse error '{httpWebResponse.StatusCode}' with '{uri}'.");
                                _helper.ThrowWrappedWebException((int) httpWebResponse.StatusCode, "WebRequest.GetResponse", summary.FinalUri);
                                continue;
                            }
                            _logger.LogTrace($"WebResponse error '{httpWebResponse.StatusCode}' - '{uri.AbsoluteUri}'. Reattempt with proxy set to '{proxyResolution}'");
                            continue;
                        default:
                            proxyResolution = ProxyResolution.Error;
                            _logger.LogTrace($"WebResponse error '{httpWebResponse.StatusCode}'  - '{uri.AbsoluteUri}'.");
                            _helper.ThrowWrappedWebException((int) httpWebResponse.StatusCode, "WebRequest.GetResponse", summary.FinalUri);
                            continue;
                    }
                }
                catch (WrappedWebException ex)
                {
                    if (proxyResolution == ProxyResolution.Error)
                    {
                        _logger.LogDebug($"WebResponse exception '{ex.Status}' with '{uri}'.");
                        throw;
                    }
                }
                catch (WebException ex)
                {
                    var errorMessage = cancellationToken.IsCancellationRequested ? "GetWebResponse failed along with a cancellation request" : "GetWebResponse failed";
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogTrace("WebClient error '" + ex.Status + "' with '" + uri.AbsoluteUri + "' - " + errorMessage);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    _logger.LogTrace("WebClient error '" + ex.Status + "' - proxy setting '" + proxyResolution + "' - '" + uri.AbsoluteUri + "'.");
                    switch (ex.Status)
                    {
                        case WebExceptionStatus.NameResolutionFailure:
                        case WebExceptionStatus.ConnectFailure:
                        case WebExceptionStatus.SendFailure:
                        case WebExceptionStatus.ProtocolError:
                        case WebExceptionStatus.ProxyNameResolutionFailure:
                            ++proxyResolution;
                            break;
                        default:
                            proxyResolution = ProxyResolution.Error;
                            break;
                    }
                    if (proxyResolution == ProxyResolution.Error)
                    {
                        _logger.LogTrace("WebClient failed in '" + uri.AbsoluteUri + "' with '" + ex.Message + "' - '" + uri.AbsoluteUri + "'.");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug(ex, "General exception error in web client.");
                    throw;
                }
                finally
                {
                    if (httpWebResponse != null && successful)
                        httpWebResponse.Close();
                }
            }
            webRequest = null;
            return null;
        }
    }
}