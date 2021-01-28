using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleDownloadManager
{
    public class DownloadFailureException : Exception
    {
        public IEnumerable<DownloadFailureInformation> DownloadFailures { get; }

        public override string Message
        {
            get
            {
                var stringBuilder = new StringBuilder();
                foreach (var downloadFailure in DownloadFailures)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append(". ");
                    stringBuilder.Append(downloadFailure.Engine);
                    stringBuilder.Append(" download failed: ");
                    stringBuilder.Append(downloadFailure.Exception.Message);
                }
                return stringBuilder.ToString();
            }
        }


        public DownloadFailureException(IEnumerable<DownloadFailureInformation> downloadFailures)
        {
            DownloadFailures = downloadFailures;
        }
    }
}