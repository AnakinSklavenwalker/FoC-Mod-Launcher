using System;
using FocLauncherHost.Update.Model;
using Requires = Validation.Requires;

namespace FocLauncherHost.Update
{
    internal class LauncherManifestFinder : ILauncherUpdateManifestFinder
    {
        public LauncherUpdateManifestModel FindMatching(LauncherUpdateManifestContainer container)
        {
            Requires.NotNull(container, nameof(container));
            throw new NotImplementedException();
        }
    }
}