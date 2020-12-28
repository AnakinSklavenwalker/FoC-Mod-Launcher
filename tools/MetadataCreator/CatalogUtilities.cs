using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FocLauncher;
using FocLauncher.UpdateMetadata;
using FocLauncher.Utilities;
using FocLauncher.Xml;
using NLog;

namespace MetadataCreator
{
    internal static class CatalogUtilities
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static ProductCatalog? FindMatchingCatalog(this Catalogs catalogs, string productName, ApplicationType applicationType)
        {
            return catalogs.Products.Where(x =>
                x.Name.Equals(productName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault(x => x.ApplicationType == applicationType);
        }

        internal static bool DownloadCurrentCatalog(Uri currentMetadataUri, out Catalogs currentCatalog)
        {
            currentCatalog = default;
            using var metadataStream = new MemoryStream();
            Logger.Trace($"Downloading current metadata file from {currentMetadataUri}");
            if (!Downloader.Download(currentMetadataUri, metadataStream))
            {
                Logger.Error("Unable to get the current metadata file");
                return false;
            }

            try
            {
                var parser = new XmlObjectParser<Catalogs>(metadataStream);
                currentCatalog = parser.Parse();
            }
            catch (Exception e)
            {
                Logger.Fatal(e, $"Download failed: {e.Message}");
                return false;
            }
            Logger.Info("Succeeded download.");
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
            Logger.Debug($"Product created: {product}");
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
            Logger.Debug($"Dependency created: {dependency}");
            return dependency;
        }
    }
}
