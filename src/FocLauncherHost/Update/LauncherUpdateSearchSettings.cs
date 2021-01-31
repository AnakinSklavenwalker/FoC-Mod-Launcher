using FocLauncher;
using ProductMetadata;

namespace FocLauncherHost.Update
{
    public sealed record LauncherUpdateSearchSettings
    {
        public UpdateMode UpdateMode { get; init; }

        public ProductReleaseType UpdateBranch { get; init; }
    }
}