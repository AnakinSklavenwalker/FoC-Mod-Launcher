using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading.Tasks;
using FocLauncher.Utilities;
using NLog;
#if !DEBUG
using dnlib.DotNet;
#endif


namespace FocLauncherHost.Utilities
{
    internal static class AssemblyExtractor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task WriteNecessaryAssembliesToDiskAsync(string fileDirectory, params string[] assemblyFiles)
        {
            foreach (var assemblyFile in assemblyFiles)
            {
                foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                {
                    if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(resourceName, assemblyFile,
                            CompareOptions.IgnoreCase) < 0)
                        continue;
                    var compressed = resourceName.EndsWith(".compressed");
                    await WriteToFileAsync(resourceName, assemblyFile, fileDirectory, compressed);
                }
            }
        }

        public static void WriteNecessaryAssembliesToDisk(string fileDirectory, params string[] assemblyFiles)
        {
            Task.Run(async () => await WriteNecessaryAssembliesToDiskAsync(fileDirectory, assemblyFiles)).Wait();
        }

        private static async Task WriteToFileAsync(string resourceName, string matching, string fileDirectory, bool compressed)
        {
            if (!Directory.Exists(fileDirectory) || !PathUtilities.UserHasDirectoryAccessRights(fileDirectory, FileSystemRights.Modify))
                throw new IOException("The Launcher's base directory does not exists");
            var filePath = Path.Combine(fileDirectory, matching);
            try
            {
                using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (rs == null)
                    throw new NullReferenceException(nameof(rs));

                var assemblyStream = new MemoryStream();
                await rs.CopyToAsync(assemblyStream);
                if (compressed)
                    assemblyStream = await rs.DecompressAsync();
                assemblyStream.Position = 0;
#if !DEBUG
                if (File.Exists(filePath))
                {
                    var embeddedAssembly =
                        AssemblyDef.Load(assemblyStream, new ModuleContext(new AssemblyResolver(), null));
                    var embeddedFileVersionString = embeddedAssembly.CustomAttributes
                        .Find(typeof(AssemblyFileVersionAttribute).FullName)?.ConstructorArguments[0].Value.ToString();

                    if (File.ReadAllBytes(filePath).Length > 0)
                    {
                        if (string.IsNullOrEmpty(embeddedFileVersionString))
                            return;
                        var embeddedAssemblyFileVersion = Version.Parse(embeddedFileVersionString!);

                        var existingVersion =
                            LauncherVersionUtilities.GetFileVersion(new PhysicalFileInfo(new FileInfo(filePath)));
                        if (embeddedAssemblyFileVersion <= existingVersion)
                            return;
                    }
                    assemblyStream.Position = 0;
                }
#endif
                Logger.Debug($"Writing file: '{filePath}'");
                await WriteToFileAsync(assemblyStream, filePath);
            }
            catch (Exception ex)
            {
                throw new IOException("Error writing necessary launcher files to disk!", ex);
            }
        }

        private static async Task WriteToFileAsync(Stream assemblyStream, string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await assemblyStream.CopyToAsync(fs);
        }

        private static async Task<MemoryStream> DecompressAsync(this Stream stream)
        {
            stream.Position = 0;
            using var decompressionStream = new DeflateStream(stream, CompressionMode.Decompress);
            var memoryStream = new MemoryStream();
            await decompressionStream.CopyToAsync(memoryStream);
            return memoryStream;
        }
    }
}