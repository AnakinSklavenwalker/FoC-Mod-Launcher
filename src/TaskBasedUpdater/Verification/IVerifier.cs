using System.IO;

namespace TaskBasedUpdater.Verification
{
    public interface IVerifier
    {
        public VerificationResult Verify(Stream file, VerificationContext verificationContext);
    }
}