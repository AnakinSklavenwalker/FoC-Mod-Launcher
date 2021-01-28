using System;

namespace SimpleDownloadManager
{
    public class DownloadFailureInformation
    {
        public Exception Exception { get; }

        public string Engine { get; }

        public DownloadFailureInformation(Exception exception, string engine)
        {
            Exception = exception;
            Engine = engine;
        }
    }
}