using System;

namespace SimpleDownloadManager.Verification
{
    public class VerificationFailedException : Exception
    {
        public VerificationResult Result { get; }

        public VerificationFailedException(VerificationResult result, string message) : base(message)
        {
            Result = result;
            HResult = -2146869244;
        }
    }
}