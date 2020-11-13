using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TaskBasedUpdater.Download
{
    internal class DownloadHelpers
    {
        private ILogger? _logger;

        public DownloadHelpers(IServiceProvider serviceProvider) => this._logger = serviceProvider.GetService<ILogger>();

        public void ThrowWrappedWebException(int errorCode, string functionName, string message)
        {
            var errorCode1 = -2147024896 | errorCode;
            if (string.IsNullOrEmpty(message))
                message = "unspecified";
            throw new WrappedWebException(errorCode1, $"Function: {functionName}, HR: {errorCode1}, Message: {message}");
        }
    }
}