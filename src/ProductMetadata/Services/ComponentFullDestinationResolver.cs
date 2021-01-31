﻿using System;
using System.IO;
using System.IO.Abstractions;
using ProductMetadata.Component;
using Validation;

namespace ProductMetadata.Services
{
    public sealed class ComponentFullDestinationResolver : IFullDestinationResolver
    {
        private readonly IFileSystem _fileSystem;

        public ComponentFullDestinationResolver(IFileSystem fileSystem)
        {
            Requires.NotNull(fileSystem, nameof(fileSystem));
            _fileSystem = fileSystem;
        }

        public string GetFullDestination(string destination, string? basePath)
        {
            Requires.NotNull(destination, nameof(destination));
            var path = Environment.ExpandEnvironmentVariables(destination);
            if (_fileSystem.Path.IsPathRooted(path))
                return path;

            if (string.IsNullOrEmpty(basePath))
                throw new IOException($"base path must be defined for relative destination path'{path}'");
            basePath = Environment.ExpandEnvironmentVariables(basePath!);
            if (!_fileSystem.Path.IsPathRooted(basePath))
                throw new IOException($"{nameof(basePath)} must be rooted.");
            return _fileSystem.Path.Combine(basePath, path);
        }

        public string GetFullDestination(ProductComponent component, bool includeFileName, string? basePath)
        {
            Requires.NotNull(component, nameof(component));
            var input = includeFileName ? component.GetFilePath() : component.Destination;
            return GetFullDestination(input, basePath);
        }
    }
}