using FocLauncherHost.Update.Model;
using ProductMetadata.Manifest;

namespace FocLauncherHost.Update
{
    internal interface ILauncherUpdateManifestFinder
    {
        LauncherUpdateManifestModel? FindMatching(LauncherUpdateManifestContainer container, ManifestLocation manifestLocation);
    }
}