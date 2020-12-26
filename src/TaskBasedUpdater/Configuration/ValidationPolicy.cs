namespace TaskBasedUpdater.Configuration
{
    public enum ValidationPolicy
    {
        Skip,
        SkipWhenNoContextOrBroken,
        Optional,
        Enforce,
    }
}