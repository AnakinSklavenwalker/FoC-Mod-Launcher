using System;

namespace FocLauncherHost.Update.Model
{
    public static class LauncherComponentHelper
    {
        public static Version? GetVersion(this LauncherComponent dependency)
        {
            if (string.IsNullOrEmpty(dependency.Version))
                return null;
            return !Version.TryParse(dependency.Version, out var version) ? null : version;
        }
    }
}
