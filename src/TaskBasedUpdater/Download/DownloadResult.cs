namespace TaskBasedUpdater.Download
{
    public enum DownloadResult
    {
        Success,
        NotSupported,
        Exception,
        MissingOrInvalidVerificationContext,
        HashMismatch,
    }
}