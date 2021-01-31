using System;
using System.IO.Abstractions;
using CommonUtilities;
using FocLauncherHost.Utilities;
using ProductMetadata.Component;

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