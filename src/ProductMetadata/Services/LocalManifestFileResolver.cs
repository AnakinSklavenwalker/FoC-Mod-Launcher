using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Logging;
using ProductMetadata.Manifest;
using Validation;

namespace ProductMetadata.Services
{
    public class LocalManifestFileResolver : IManifestFileResolver
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILogger? _logger;

        public LocalManifestFileResolver(IFileSystem fileSystem, ILogger? logger = null)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public IFileInfo GetManifest(Uri manifestPath)
        {
            var manifestFilePath = manifestPath.LocalPath;
            var fileInfo = _fileSystem.FileInfo.FromFileName(manifestFilePath);
            if (!fileInfo.Exists)
                throw new ManifestNotFoundException($"Could not find manifest at {fileInfo.FullName}");
            return fileInfo;
        }
    }
}