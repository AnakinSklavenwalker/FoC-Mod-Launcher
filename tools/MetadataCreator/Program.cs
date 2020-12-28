using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CommandLine;
using FocLauncher;
using FocLauncher.Properties;
using FocLauncher.UpdateMetadata;
using FocLauncher.Utilities;
using FocLauncher.Xml;
using NLog;
using NLog.Conditions;
using NLog.Targets;

namespace MetadataCreator
{
    internal class Program
    {
        private const string DefaultFileRootPath = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher-Builds/master";
        public static readonly string[] SupportedFileEndings = {".exe", ".dll"};

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        internal static LaunchOptions LaunchOptions;


        private static int Main(string[] args)
        {
            SetLogging();
            Parser.Default.ParseArguments<LaunchOptions>(args).WithParsed(launchOptions =>
            {
                if (string.IsNullOrEmpty(launchOptions.XmlOutput))
                    launchOptions.XmlOutput = Directory.GetCurrentDirectory();
                if (string.IsNullOrEmpty(launchOptions.OriginPathRoot))
                    launchOptions.OriginPathRoot = DefaultFileRootPath;
                if (string.IsNullOrEmpty(launchOptions.SourceDirectory))
                    launchOptions.SourceDirectory = Directory.GetCurrentDirectory();
                if (string.IsNullOrEmpty(launchOptions.XmlOutput))
                    launchOptions.XmlOutput = Directory.GetCurrentDirectory();
                LaunchOptions = launchOptions;
            });
            if (LaunchOptions is null)
            {
                Logger.Fatal("Failed parsing arguments.");
                return -1;
            }
            
            try
            {
                var dir = new DirectoryInfo(LaunchOptions.SourceDirectory);
                var files = dir.GetFilesByExtensions(true, SupportedFileEndings);
                var applicationFileInfos = FileUtilities.GetApplicationFiles(files.ToList(), LaunchOptions.BuildType).ToList();
                if (applicationFileInfos.Count != LauncherConstants.ApplicationFileNames.Length)
                    throw new InvalidOperationException("Unexpected number of applications files found.");

                if (!Enum.TryParse<ApplicationType>(LaunchOptions.ApplicationType, true, out var applicationType))
                    throw new InvalidOperationException(
                        $"Could not parse '{LaunchOptions.ApplicationType}' into a real ApplicationType");


                var applicationFiles = new ApplicationFiles(applicationType);
                FillData(applicationFileInfos, applicationFiles);

                if (!applicationFiles.Validate())
                    throw new InvalidOperationException("The file set was not valid");

                var product = CatalogUtilities.CreateProduct(applicationFiles);
                var catalog = CreateCatalogOrIntegrate(in product, out var actualNewDependencies);

                WriteXmlFile(catalog, LaunchOptions.XmlOutput);

                if (!string.IsNullOrEmpty(LaunchOptions.FilesCopyLocation))
                {
                    product = catalog.FindMatchingCatalog(product.Name, product.ApplicationType);
                    var filesToCopy = applicationFiles.AllFiles.Where(x => actualNewDependencies.Contains(x.Name));
                    CopyFiles(product, filesToCopy, LaunchOptions.FilesCopyLocation);
                }

                Logger.Info("Operation succeeded successfully!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e, $"The tool failed with an error: {e.Message}");
                Console.ReadKey();
                return e.HResult;
            }
            return 0;
        }

        private static void CopyFiles(ProductCatalog product, IEnumerable<FileInfo> newDependencyFiles, string filesLocation)
        {
            var typeName = Enum.GetName(typeof(ApplicationType), product.ApplicationType);
            if (typeName == null)
                throw new InvalidOperationException();
            var path = Path.Combine(filesLocation, typeName);
            var directory = new DirectoryInfo(path);
            Directory.CreateDirectory(path);

            foreach (var file in directory.GetFiles())
            {
                if (file.Name.Equals(".gitkeep"))
                    continue;
                if (!product.Dependencies.Any(x => x.Name.Equals(file.Name)))
                {
                    Logger.Warn($"Deleting '{file.Name}' because it was not present in the product catalog");
                    file.Delete();
                }
            }

            foreach (var newFile in newDependencyFiles)
            {
                Logger.Debug($"Copying file '{newFile.Name}' to location {directory.FullName}");
                newFile.CopyTo(Path.Combine(directory.FullName, newFile.Name), true);
            }
        }

        private static Catalogs CreateCatalogOrIntegrate(in ProductCatalog product, out ICollection<string> newDependencyNames)
        {
            var catalog = new Catalogs {Products = new List<ProductCatalog> {product}};
            newDependencyNames = product.Dependencies.Select(x => x.Name).ToList();

            if (LaunchOptions.XmlIntegrationMode == IntegrationMode.None)
            {
                if (product.ApplicationType != ApplicationType.Stable)
                    throw new NotSupportedException("Only having preview versions in the metadata file is not valid");
                Logger.Info($"Created new Catalog. Option: {LaunchOptions.XmlIntegrationMode}");
                return catalog;
            }


            if (!Uri.TryCreate(LaunchOptions.CurrentMetadataFile, UriKind.RelativeOrAbsolute, out var metadataUri))
            {
                Logger.Warn("Unable to get the current metadata file. Returning new catalog instead");
                return catalog;
            }
            
            catalog = Integrate(metadataUri, product, LaunchOptions.XmlIntegrationMode, out newDependencyNames);
            return catalog;
        }

        private static void WriteXmlFile(Catalogs catalog, string location)
        {
            var serializer = new XmlSerializer(typeof(Catalogs));
            var outputFile = Path.Combine(location, LauncherConstants.UpdateMetadataFileName);
            Directory.CreateDirectory(location);

            if (File.Exists(outputFile))
            {
                Logger.Trace("Deleteing old xml file");
                File.Delete(outputFile);
            }

            using var file = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new XmlTextWriter(file, Encoding.UTF8) { Formatting = Formatting.Indented };
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            serializer.Serialize(writer, catalog, ns);
            writer.Dispose();
            file.Dispose();

            var schemeStream = Resources.UpdateValidator.ToStream();
            var validator = new XmlValidator(schemeStream);
            if (!validator.Validate(outputFile).IsValid)
            {
                if (File.Exists(outputFile))
                    File.Delete(outputFile);
                throw new InvalidOperationException("Created .xml is not valid. File deleted");
            }

            Logger.Info($"Wrote {LauncherConstants.UpdateMetadataFileName} to path '{outputFile}'");
            Logger.Trace(File.ReadAllText(outputFile));
        }

        private static Catalogs Integrate(Uri currentMetadataUri, ProductCatalog newProduct, IntegrationMode integrationMode, out ICollection<string> newDependencyNames)
        {
            newDependencyNames = newProduct.Dependencies.Select(x => x.Name).ToList();

            if (!CatalogUtilities.DownloadCurrentCatalog(currentMetadataUri, out var currentCatalog))
                return new Catalogs {Products = new List<ProductCatalog> {newProduct}};

            var currentProduct = currentCatalog.FindMatchingCatalog(newProduct.Name, newProduct.ApplicationType);
            if (currentProduct == null)
            {
                if (currentCatalog.Products == null)
                    currentCatalog.Products = new List<ProductCatalog>();
                currentCatalog.Products.Add(newProduct);
                Logger.Info($"Created new Product. Option: {LaunchOptions.XmlIntegrationMode}");
                return currentCatalog;
            }


            switch (integrationMode)
            {
                case IntegrationMode.Product:
                    currentCatalog.Products.Remove(currentProduct);
                    currentCatalog.Products.Add(newProduct);
                    Logger.Info($"Added Product. Option: {LaunchOptions.XmlIntegrationMode}");
                    break;
                case IntegrationMode.Dependency:
                    currentProduct.Dependencies = new List<Dependency>();
                    currentProduct.Dependencies.AddRange(newProduct.Dependencies);
                    Logger.Info($"Replaced missmatching product dependencies. Option: {LaunchOptions.XmlIntegrationMode}");
                    break;
                case IntegrationMode.DependencyVersion:
                {
                    var currentDependencies = currentProduct.Dependencies;
                    var newDependencies = newProduct.Dependencies;
                    newDependencyNames = new List<string>();
               
                    currentProduct.Dependencies = new List<Dependency>();

                    foreach (var newDependency in newDependencies)
                    {
                        var oldDependency = currentDependencies.FirstOrDefault(x => DependencyComparer.Name.Equals(x, newDependency));
                        if (oldDependency != null)
                        {
                            if (DependencyComparer.NameAndVersion.Equals(oldDependency, newDependency))
                            {
                                Logger.Trace($"Keeping old dependency {newDependency.Name} because version are equal");
                                currentProduct.Dependencies.Add(oldDependency);
                                continue;
                            }
                        }
                        Logger.Trace($"Adding new dependency '{newDependency.Name}' because version are different");
                        newDependencyNames.Add(newDependency.Name);
                        currentProduct.Dependencies.Add(newDependency);
                    }
                    Logger.Info($"Replaced missmatching product dependencies. Option: {LaunchOptions.XmlIntegrationMode}");
                    break;
                }
            }

            return currentCatalog;
        }

        internal static void FillData(IEnumerable<FileInfo> files, in ApplicationFiles data)
        {
            Logger.Trace("Added application files to an ApplicationFiles");
            foreach (var file in files)
            {
                if (file.Name.Equals(LauncherConstants.LauncherFileName))
                {
                    if (data.Executable != null)
                        throw new InvalidOperationException("Cannot set the main executable file twice for a data set");
                    data.Executable = file;
                    continue;
                }
                data.Files.Add(file);
            }
        }
        
        private static void SetLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            var highlightRule = new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Info"),
                ForegroundColor = ConsoleOutputColor.Green
            };
            consoleTarget.RowHighlightingRules.Add(highlightRule);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget);
            LogManager.Configuration = config;
        }
    }
}
