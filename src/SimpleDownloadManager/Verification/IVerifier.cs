using System.IO;

namespace SimpleDownloadManager.Verification
{
    public interface IVerifier
    {
        public VerificationResult Verify(Stream file, VerificationContext verificationContext);
    }
}