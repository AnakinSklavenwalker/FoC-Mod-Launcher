namespace SimpleDownloadManager.Configuration
{
    public enum VerificationPolicy
    {
        Skip,
        SkipWhenNoContextOrBroken,
        Optional,
        Enforce,
    }
}