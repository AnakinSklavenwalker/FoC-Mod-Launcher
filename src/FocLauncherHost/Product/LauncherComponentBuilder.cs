using System;
using System.IO.Abstractions;
using FocLauncherHost.Utilities;
using TaskBasedUpdater.New.Product.Component;
using TaskBasedUpdater.Verification;

namespace FocLauncherHost.Product
{
    internal class LauncherComponentBuilder : IProductComponentBuilder
    {
        public HashType HashType => HashType.Sha256;
        
        public Version? GetVersion(IFileInfo file)
        {
            return LauncherVersionUtilities.GetFileVersionSafe(file);
        }
    }
}