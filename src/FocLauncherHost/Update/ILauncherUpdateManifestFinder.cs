using FocLauncherHost.Update.Model;
using TaskBasedUpdater.New.Update;

namespace FocLauncherHost.Update
{
    internal interface ILauncherUpdateManifestFinder
    {
        LauncherUpdateManifestModel? FindMatching(LauncherUpdateManifestContainer container, UpdateRequest updateRequest);
    }
}