using System.Collections.Generic;
using FocLauncherHost.Update.Model;

namespace FocLauncherHost.Update
{
    internal delegate LauncherUpdateManifestModel? FallbackSearchAction(IEnumerable<LauncherUpdateManifestModel> manifests);
}