using FocLauncher;
using TaskBasedUpdater.New.Product;

namespace FocLauncherHost.Update
{
    public sealed record LauncherUpdateSearchSettings
    {
        public UpdateMode UpdateMode { get; init; }

        public ProductReleaseType UpdateBranch { get; init; }
    }
}