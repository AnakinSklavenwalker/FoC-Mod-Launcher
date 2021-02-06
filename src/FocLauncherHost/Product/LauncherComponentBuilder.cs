using System;
using System.IO.Abstractions;
using CommonUtilities;
using FocLauncherHost.Utilities;
using ProductMetadata.Component;
using ProductMetadata.Services;

namespace FocLauncherHost.Product
{
    internal class LauncherComponentBuilder : IProductComponentBuilder
    {
        public HashType HashType => HashType.Sha256;

        public ComponentIntegrityInformation GetIntegrityInformation(IFileInfo file)
        {
            var hashingService = new HashingService();
            var hash = hashingService.GetFileHash(file, HashType);
            return new ComponentIntegrityInformation(hash, HashType);
        }

        public Version? GetVersion(IFileInfo file)
        {
            return LauncherVersionUtilities.GetFileVersionSafe(file);
        }

        public long GetSize(IFileInfo file)
        {
            return file.Length;
        }
    }
}