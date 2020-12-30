using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FocLauncher;
using FocLauncher.Utilities;
using FocLauncher.Xml;
using FocLauncherHost.Update.Model;
using Microsoft.Extensions.Logging;

namespace MetadataCreator
{
    // TODO: switch-logging
    // TODO: make non-static
    internal static class CatalogUtilities
    {
        private static readonly ILogger? Logger;

        internal static ProductCatalog? FindMatchingCatalog(this Catalogs catalogs, string productName, ApplicationType applicationType)
        {
            return catalogs.Products.Where(x =>
                x.Name.Equals(productName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault(x => x.ApplicationType == applicationType);
        }

        internal static bool DownloadCurrentCatalog(Uri currentMetadataUri, out Catalogs? currentCatalog)
        {
            currentCatalog = default;
            using var metadataStream = new MemoryStream();
            Logger?.LogTrace($"Downloading current metadata file from {currentMetadataUri}");
            if (!Downloader.Download(currentMetadataUri, metadataStream))
            {
                Logger?.LogError("Unable to get the current metadata file");
                return false;
            }

            try
            {
                var parser = new XmlObjectParser<Catalogs>(metadataStream);
                currentCatalog = parser.Parse();
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e, $"Download failed: {e.Message}");
                return false;
            }
            Logger?.LogInformation("Succeeded download.");
            return true;
        }


        internal static ProductCatalog CreateProduct(ApplicationFiles applicationFiles)
        {
            var product = new ProductCatalog
            {
                Name = LauncherConstants.ProductName,
                Author = LauncherConstants.Author,
                ApplicationType = applicationFiles.Type,
                Dependencies = new List<Dependency> { CreateDependency(applicationFiles.Executable, applicationFiles.Type, true) }
            };
            foreach (var file in applicationFiles.Files)
                product.Dependencies.Add(CreateDependency(file, applicationFiles.Type));
            Logger?.LogDebug($"Product created: {product}");
            return product;
        }

        internal static Dependency CreateDependency(FileInfo file, ApplicationType application, bool isLauncherExecutable = false)
        {
            var dependency = new Dependency();
            dependency.Name = file.Name;
            var destination = isLauncherExecutable ? LauncherConstants.ExecutablePathVariable : LauncherConstants.ApplicationBaseVariable;
            dependency.Destination = $"%{destination}%";
            dependency.Version = FileVersionInfo.GetVersionInfo(file.FullName).FileVersion;
            dependency.Sha2 = FileHashHelper.GetFileHash(file.FullName, FileHashHelper.HashType.Sha256);
            dependency.Size = file.Length;
            dependency.Origin = UrlCombine.Combine(Program.LaunchOptions.OriginPathRoot, application.ToString(), file.Name);
            Logger?.LogDebug($"Dependency created: {dependency}");
            return dependency;
        }
    }
}
