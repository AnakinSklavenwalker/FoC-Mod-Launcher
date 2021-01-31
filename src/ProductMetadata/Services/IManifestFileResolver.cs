using System;
using System.IO.Abstractions;

namespace ProductMetadata.Services
{
    public interface IManifestFileResolver
    {
        IFileInfo GetManifest(Uri manifestPath);
    }
}