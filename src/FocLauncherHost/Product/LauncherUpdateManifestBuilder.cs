using System;
using System.IO;
using FocLauncher.Properties;
using FocLauncher.Xml;
using FocLauncherHost.Update;
using FocLauncherHost.Update.Model;
using FocLauncherHost.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductMetadata;
using ProductMetadata.Component;
using ProductMetadata.Manifest;
using Validation;

namespace FocLauncherHost.Product
{
    internal class LauncherUpdateManifestBuilder : UpdateManifestBuilder<LauncherUpdateManifestContainer>
    {
        private readonly ILauncherUpdateManifestFinder _updateManifestFinder;
        private readonly ICatalogConverter<LauncherUpdateManifestModel, LauncherComponent> _catalogConverter;
        private readonly ILogger? _logger;

        public LauncherUpdateManifestBuilder(
            ILauncherUpdateManifestFinder updateManifestFinder,
            ICatalogConverter<LauncherUpdateManifestModel, LauncherComponent> catalogConverter, 
            IServiceProvider serviceProvider)
        {
            Requires.NotNull(updateManifestFinder, nameof(updateManifestFinder));
            Requires.NotNull(catalogConverter, nameof(catalogConverter));
            _updateManifestFinder = updateManifestFinder;
            _catalogConverter = catalogConverter;
            _logger = serviceProvider.GetService<ILogger>();
        }

        protected override ICatalog BuildManifestCatalog(LauncherUpdateManifestContainer manifestContainer, ProductManifestLocation manifestLocation)
        {
            var manifest = _updateManifestFinder.FindMatching(manifestContainer, manifestLocation);
            if (manifest is null)
                throw new ManifestNotFoundException($"Unable to find matching manifest from {manifestContainer} for product {manifestLocation.Product}.");

            return _catalogConverter.Convert(manifest);
        }

        protected override LauncherUpdateManifestContainer SerializeManifestModel(Stream manifestData)
        {
            if (manifestData is not FileStream manifestFile)
                throw new InvalidOperationException();

            ValidateManifestFile(manifestFile);
            try
            {
                return LauncherUpdateManifestContainer.FromStream(manifestFile);
            }
            catch (Exception e)
            {
                throw new ManifestException(e.Message, e);
            }
        }

        private void ValidateManifestFile(FileStream manifestStream)
        {
            var fileName = manifestStream.Name;
            _logger?.LogTrace($"Validating manifest file {fileName}");

            var schema = Resources.UpdateValidator.ToStream();
            using var validator = new XmlValidator(schema);

            var result = validator.Validate(manifestStream).IsValid;
            if (!result)
                throw new ManifestException($"Manifest file '{fileName}' is not valid.");
            _logger?.LogTrace($"Validation of '{fileName}' successful)");
        }
    }
}