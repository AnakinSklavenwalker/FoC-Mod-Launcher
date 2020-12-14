using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Update;
using Xunit;

namespace TaskBasedUpdater.Tests
{
    public class UpdateCatalogBuilderTests
    {
        private readonly IUpdateCatalogBuilder _builder;

        private const string ProductName = "Product";

        private static readonly IInstalledProduct Product = CreateProduct(ProductName);
        private static readonly IInstalledProduct NewProductVersion = CreateProduct(ProductName, new Version(1, 0));
        private static readonly IInstalledProduct NewProductBeta = CreateProduct(ProductName, releaseType: ProductReleaseType.Beta);
        private static readonly IInstalledProduct NewProductBetaVersion = CreateProduct(ProductName, new Version(1, 0), ProductReleaseType.Beta);


        public UpdateCatalogBuilderTests()
        {
            _builder = new UpdateCatalogBuilder();
        }


        [Fact]
        public void TestIncompatible()
        {
            var p1 = CreateProduct("A");
            var p2 = CreateProduct("B");
            var available = new AvailableProductCatalog(p1, new ProductComponent[0]);
            var installed = new InstalledProductCatalog(p2, new ProductComponent[0]);

            Assert.Throws<InvalidOperationException>(() =>
                _builder.Build(installed, available, UpdateRequestAction.Repair));

            Assert.Throws<InvalidOperationException>(() =>
                _builder.Build(installed, available, UpdateRequestAction.Update));

            Assert.Throws<InvalidOperationException>(() =>
                _builder.Build(installed, available, UpdateRequestAction.Update | UpdateRequestAction.Repair));
        }


        public static IEnumerable<object[]> BothEmpty()
        {
            yield return new object[]
            {
                UpdateRequestAction.Update,
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductCatalog(Product, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };

            yield return new object[]
            {
                UpdateRequestAction.Repair,
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductCatalog(Product, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };

            yield return new object[]
            {
                UpdateRequestAction.Update | UpdateRequestAction.Repair,
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductCatalog(Product, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };
        }

        public static IEnumerable<object[]> SameComponents()
        {

            var current = new ProductComponent("A", "D");
            var avail = new ProductComponent("A", "D");

            var expected = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep
            };

            yield return new object[]
            {
                UpdateRequestAction.Update,
                new InstalledProductCatalog(Product, new []{current}),
                new AvailableProductCatalog(Product, new []{avail}),
                new UpdateCatalogStub(new []{expected})
            };
        }

        [Theory]
        //[MemberData(nameof(BothEmpty))]
        [MemberData(nameof(SameComponents))]
        public void Test(
            UpdateRequestAction updateRequest,
            IInstalledProductCatalog installedProductCatalog,
            IAvailableProductCatalog availableProduct,
            IUpdateCatalog expected)
        {
            Assert.NotNull(installedProductCatalog);
            Assert.NotNull(availableProduct);
            Assert.NotNull(expected);

            var catalog = _builder.Build(installedProductCatalog, availableProduct, updateRequest);

            Assert.Equal(availableProduct.Product, catalog.Product, ProductReferenceEqualityComparer.Default);
            Assert.Equal(expected.Items.Count(), catalog.Items.Count());
            Assert.Equal(updateRequest, catalog.RequestAction);

            foreach (var item in expected.Items)
            {
                switch (item.RequiredAction)
                {
                    case ComponentAction.Keep:
                        Assert.Contains(catalog.ComponentsToKeep, x => x.Name.Equals(item.Name));
                        break;
                    case ComponentAction.Update:
                        Assert.Contains(catalog.ComponentsToInstall, x => x.Name.Equals(item.Name));
                        break;
                    case ComponentAction.Delete:
                        Assert.Contains(catalog.ComponentsToDelete, x => x.Name.Equals(item.Name));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static IInstalledProduct CreateProduct(
            string name, 
            Version? version = null, 
            ProductReleaseType releaseType = ProductReleaseType.Stable)
        {
            return new InstalledProduct(name, Directory.GetCurrentDirectory())
            {
                Version = version,
                ReleaseType = releaseType
            };
        }

        private class UpdateCatalogStub : IUpdateCatalog
        { 
            public IProductReference Product { get; }
            public UpdateRequestAction RequestAction { get; }
            public IEnumerable<ProductComponent> ComponentsToInstall { get; }
            public IEnumerable<ProductComponent> ComponentsToKeep { get; }
            public IEnumerable<ProductComponent> ComponentsToDelete { get; }
            public IEnumerable<ProductComponent> Items { get; }

            public UpdateCatalogStub(IEnumerable<ProductComponent> items)
            {
                Items = items;
            }
        }
    }
}
