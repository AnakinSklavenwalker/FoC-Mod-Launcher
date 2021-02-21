using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using ProductMetadata.Component;
using ProductMetadata.Services.Detectors;
using Xunit;

namespace ProductMetadata.Tests
{
    public class FileComponentDetectorTests
    {
        private readonly IServiceProvider _services;

        public FileComponentDetectorTests()
        {
            var sc = new ServiceCollection();
            sc.AddScoped<IFileSystem>(_ => new MockFileSystem());
            _services = sc.BuildServiceProvider();
        }

        [Fact]
        public void TestThrowNoServices()
        {
            Assert.Throws<ArgumentNullException>(() => new FileComponentDetector(null!));
        }


        [Fact]
        public void TestNotSupported_WrongComponent()
        {
            var detector = new FileComponentDetector(_services);
            Assert.Throws<InvalidOperationException>(
                () => detector.Find(new ComponentGroup(new ProductComponentIdentity("A"), new List<IProductComponent>()), GetProduct()));
        }

        [Fact]
        public void TestNotSupported_ComponentMalformed()
        {
            var detector = new FileComponentDetector(_services);

            var path = "[InstallDir]";
            var manifestFile =
                new SingleFileComponent(new ProductComponentIdentity("A.File"), path, new List<FileItem>(0));
            manifestFile.OriginInfos.Add(new OriginInfo("A.txt", new Uri("file://ignored")));
            manifestFile.OriginInfos.Add(new OriginInfo("B.txt", new Uri("file://ignored")));

            Assert.Throws<InvalidOperationException>(
                () => detector.Find(manifestFile, GetProduct()));
        }


        [Fact]
        public void TestNotPresent()
        {
            var detector = new FileComponentDetector(_services);

            const string? path = "[InstallDir]";
            const string? fileName = "File.txt";

            var manifestFile =
                new SingleFileComponent(new ProductComponentIdentity("A.File"), path, new List<FileItem>(0));
            manifestFile.OriginInfos.Add(new OriginInfo(fileName, new Uri("file://ignored")));

            var detectedComponent = detector.Find(manifestFile, GetProduct());
            Assert.NotNull(detectedComponent);
            Assert.Equal(manifestFile, detectedComponent, ProductComponentIdentityComparer.Default);
            var detectedFileComponent = Assert.IsType<SingleFileComponent>(detectedComponent);
            Assert.Equal(DetectionState.Absent, detectedFileComponent.DetectedState);
            Assert.Equal(0, detectedFileComponent.Files.Count);
        }

        [Fact]
        public void TestPresent()
        {
            using var scope = _services.CreateScope();

            var detector = new FileComponentDetector(scope.ServiceProvider);

            const string? path = "[InstallDir]";
            const string? fileName = "File.txt";

            var product = GetProduct();
            var manifestFile =
                new SingleFileComponent(new ProductComponentIdentity("A.File"), path, new List<FileItem>(0));
            manifestFile.OriginInfos.Add(new OriginInfo(fileName, new Uri("file://ignored")));

            var fs = scope.ServiceProvider.GetRequiredService<IFileSystem>() as MockFileSystem;
            fs?.AddFile(fs.Path.Combine(product.InstallationPath, fileName), MockFileData.NullObject);


            var detectedComponent = detector.Find(manifestFile, product);
            Assert.NotNull(detectedComponent);
            Assert.Equal(manifestFile, detectedComponent, ProductComponentIdentityComparer.Default);
            var detectedFileComponent = Assert.IsType<SingleFileComponent>(detectedComponent);
            Assert.Equal(DetectionState.Present, detectedFileComponent.DetectedState);
            Assert.Equal(1, detectedFileComponent.Files.Count);
        }

        private static IInstalledProduct GetProduct()
        {
            var pRef = new ProductReference("A");
            var product = new InstalledProduct(
                pRef, 
                new Manifest.Manifest(pRef, new List<IProductComponent>()), 
                "C:\\A");

            product.ProductVariables.Add("InstallDir", product.InstallationPath);
            return product;
        }
    }
}