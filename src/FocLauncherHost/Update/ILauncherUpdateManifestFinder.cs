using FocLauncherHost.Update.Model;

namespace FocLauncherHost.Update
{
    internal interface ILauncherUpdateManifestFinder
    {
        LauncherUpdateManifestModel FindMatching(LauncherUpdateManifestContainer container);
    }
}