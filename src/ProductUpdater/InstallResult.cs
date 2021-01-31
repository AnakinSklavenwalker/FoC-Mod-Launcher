namespace ProductUpdater
{
    public enum InstallResult
    {
        None,
        Success,
        SuccessRestartRequired,
        Failure,
        FailureException,
        Cancel,
    }
}