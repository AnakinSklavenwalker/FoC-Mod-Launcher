using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using ProductMetadata.Component;
using ProductMetadata.Manifest;
using ProductMetadata.Services;
using Xunit;

namespace ProductMetadata.Tests
{
    public class ManifestBuilderTests
    {
        private class MockManifestBuilder : ManifestBuilder<object>
        {
            private bool _serialized;
            private bool _built;

            private object? _model;

            protected override IManifest BuildManifestCatalog(object manifestModel, IProductReference product)
            {
                Assert.True(_serialized);
                Assert.Same(_model, manifestModel);
                _built = true;
                return new Manifest.Manifest(product, new List<IProductComponent>());
            }

            protected override object SerializeManifestModel(Stream manifestData, IProductReference productReference)
            {
                Assert.False(_serialized);
                Assert.False(_built);
                _serialized = true;
                _model = new object();
                return _model;
            }
        }


        [Fact]
        public void TestInputValidation()
        {
            var builder = new MockManifestBuilder();

            var file = new FileSystem().FileInfo.FromFileName("test.xml");

            Assert.Throws<ArgumentNullException>(() => builder.Build(null, new ProductReference("A")));
            Assert.Throws<ArgumentNullException>(() => builder.Build(file, null));
            Assert.Throws<FileNotFoundException>(() => builder.Build(file, new ProductReference("A")));
        }

        [Fact]
        public void TestCallOrderValidation()
        {
            var builder = new MockManifestBuilder();

            var fs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {"test.xml", MockFileData.NullObject}
            });

            builder.Build(fs.FileInfo.FromFileName("test.xml"), new ProductReference("A"));
        }
    }
}
