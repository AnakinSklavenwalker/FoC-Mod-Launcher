namespace ProductUpdater.Configuration
{
    public enum ValidationPolicy
    {
        Skip,
        SkipWhenNoContextOrBroken,
        Optional,
        Enforce,
    }
}