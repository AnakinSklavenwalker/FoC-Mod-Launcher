using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using Microsoft.Extensions.Logging;

namespace MetadataCreator
{
    // TODO: Remove and use existing....
    public static class Downloader
    {
        private static readonly ILogger? Logger;

        public static bool Download(Uri uri, Stream outputStream)
        {
            try
            {
                if (!uri.IsFile && !uri.IsUnc)
                {
                    if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(uri.Scheme, "ftp", StringComparison.OrdinalIgnoreCase) || uri.AbsoluteUri.Length < 7)
                        return false;
                    DownloadWebFile(uri, outputStream);
                }
                else
                    DownloadLocalFile(uri, outputStream);
            }
            catch (Exception e)
            {
                Logger?.LogError(e, e.Message);
                return false;
            }
            return true;
        }

        internal static void DownloadLocalFile(Uri uri, Stream outputStream)
        {
            if (!uri.IsFile && !uri.IsUnc)
                throw new ArgumentException("Expected file or UNC path", nameof(uri));
            CopyFileToStream(uri.LocalPath, outputStream);
        }

        internal static void DownloadWebFile(Uri uri, Stream outputStream)
        {
            using var webResponse = GetWebResponse(uri);
            if (webResponse != null)
            {
                try
                {
                    using var responseStream = webResponse.GetResponseStream();
                    var header = webResponse.Headers["Content-Length"];
                    if (string.IsNullOrEmpty(header))
                        throw new IOException("Error: Content-Length is missing from response header.");
                    var totalStreamLength = (long) Convert.ToInt32(header);
                    if (totalStreamLength.Equals(0L))
                        throw new IOException("Error: Response stream length is 0.");
                    bool streamReadError;
                    var totalBytesRead = 0L;
                    var array = new byte[Math.Max(1024L, Math.Min(totalStreamLength, 32768L))];
                    while (true)
                    {
                        var bytesRead = responseStream.Read(array, 0, array.Length);
                        streamReadError = bytesRead < 0;
                        if (bytesRead <= 0)
                            break;
                        totalBytesRead += bytesRead;
                        outputStream.Write(array, 0, bytesRead);
                        if (totalStreamLength < totalBytesRead)
                            totalStreamLength = totalBytesRead;
                    }

                    if (streamReadError)
                        throw new IOException("Internal error while downloading the stream.");

                }
                catch (WebException ex)
                {
                    Logger?.LogError("WebClient error '" + ex.Status + "' with '" + uri.AbsoluteUri + "'.");
                    throw;
                }
            }
        }

        private static HttpWebResponse? GetWebResponse(Uri uri)
        {
            var proxyResolution = ProxyResolution.Default;
            while (proxyResolution != ProxyResolution.Error)
            {
                var httpWebResponse = (HttpWebResponse)null;
                var successful = true;
                try
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(uri);
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

                    httpWebResponse = (HttpWebResponse)webRequest.GetResponse();

                    var responseUri = httpWebResponse.ResponseUri.ToString();
                    if (!string.IsNullOrEmpty(responseUri) &&
                        !uri.ToString().EndsWith(responseUri, StringComparison.InvariantCultureIgnoreCase))
                        Logger?.LogDebug($"Uri '{uri}' + redirected to '{responseUri}'");

                    switch (httpWebResponse.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            successful = false;
                            return httpWebResponse;
                        case HttpStatusCode.UseProxy:
                        case HttpStatusCode.ProxyAuthenticationRequired:
                        case HttpStatusCode.GatewayTimeout:
                            ++proxyResolution;
                            if (proxyResolution == ProxyResolution.Error)
                            {
                                Logger?.LogWarning($"WebResponse error '{httpWebResponse.StatusCode}' with '{uri}'.");
                                throw new WrappedWebException();
                            }
                            Logger?.LogWarning($"WebResponse error '{httpWebResponse.StatusCode}' - '{uri.AbsoluteUri}'. Reattempt with proxy set to '{proxyResolution}'");
                            continue;
                        default:
                            proxyResolution = ProxyResolution.Error;
                            Logger?.LogWarning($"WebResponse error '{httpWebResponse.StatusCode}'  - '{uri.AbsoluteUri}'.");
                            throw new WrappedWebException();
                    }
                }
                catch (WrappedWebException ex)
                {
                    if (proxyResolution == ProxyResolution.Error)
                    {
                        Logger?.LogError($"WebResponse exception '{ex.Status}' with '{uri}'.");
                        throw;
                    }
                }
                catch (WebException ex)
                {
                    Logger?.LogError("WebClient error '" + ex.Status + "' - proxy setting '" + proxyResolution + "' - '" + uri.AbsoluteUri + "'.");
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
                        Logger?.LogWarning("WebClient failed in '" + uri.AbsoluteUri + "' with '" + ex.Message + "' - '" + uri.AbsoluteUri + "'.");
                        throw;
                    }
                }
                catch (Exception)
                {
                    Logger?.LogError("General exception error in web client.");
                    throw;
                }
                finally
                {
                    if (httpWebResponse != null && successful)
                        httpWebResponse.Close();
                }
            }
            return null;
        }

        private static void CopyFileToStream(string filePath, Stream outStream)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(nameof(filePath));
            var downloadSize = 0L;
            var array = new byte[32768];
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            while (true)
            {
                var readSize = fileStream.Read(array, 0, array.Length);
                if (readSize <= 0)
                    break;
                outStream.Write(array, 0, readSize);
                downloadSize += readSize;
            }
            if (downloadSize != fileStream.Length)
                throw new IOException("Internal error copying streams. Total read bytes does not match stream Length.");
        }

        private enum ProxyResolution
        {
            Default,
            DefaultCredentialsOrNoAutoProxy,
            NetworkCredentials,
            DirectAccess,
            Error,
        }

        public class WrappedWebException : WebException
        {
        }
    }
}