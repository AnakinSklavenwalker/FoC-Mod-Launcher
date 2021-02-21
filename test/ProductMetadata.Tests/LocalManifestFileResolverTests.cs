using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using ProductMetadata.Manifest;
using ProductMetadata.Services;
using Xunit;

namespace ProductMetadata.Tests
{
    public class LocalManifestFileResolverTests
    {
        private readonly IServiceProvider _services;

        public LocalManifestFileResolverTests()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton<IFileSystem>(new MockFileSystem());
            _services = sc.BuildServiceProvider();
        }

        [Fact]
        public void TestFindManifest()
        {
            var resolver = new LocalManifestFileResolver(_services);
            Assert.Throws<ManifestNotFoundException>(() => resolver.GetManifest(new Uri(@"c:\test.xml")));
            
            var fs = _services.GetRequiredService<IFileSystem>() as MockFileSystem;
            fs!.AddFile("test.xml", MockFileData.NullObject);

            var file = resolver.GetManifest(new Uri(@"c:\test.xml"));
            Assert.NotNull(file);
            Assert.True(fs.FileExists(file.FullName));
        }
    }
}