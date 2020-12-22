using System;
using System.Collections.Generic;
using System.Linq;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Product.Manifest;
using TaskBasedUpdater.New.Update;
using Xunit;

namespace TaskBasedUpdater.Tests
{
    public class UpdateCatalogBuilderTests
    {
        private readonly UpdateCatalogBuilder _builder;

        private const string ProductName = "Product";

        private static readonly IInstalledProduct Product = CreateProduct(ProductName);
        private static readonly IProductReference ProductRef = CreateProductRef(ProductName);
    
        public UpdateCatalogBuilderTests()
        {
            _builder = new UpdateCatalogBuilder();
        }


        [Fact]
        public void TestIncompatible()
        {
            var p1 = CreateProductRef("A");
            var p2 = CreateProduct("B");
            var available = new AvailableProductCatalog(p1, new ProductComponent[0]);
            var installed = new InstalledProductCatalog(p2, new ProductComponent[0]);

            Assert.Throws<InvalidOperationException>(() =>
                _builder.Build(installed, available));

            Assert.Throws<InvalidOperationException>(() =>
                _builder.Build(installed, available));

            Assert.Throws<InvalidOperationException>(() =>
                _builder.Build(installed, available));
        }


        public static IEnumerable<object[]> BothEmpty()
        {
            yield return new object[]
            {
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductCatalog(ProductRef, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductCatalog(ProductRef, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductCatalog(ProductRef, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };
        }

        public static IEnumerable<object[]> SameComponents()
        {
            var current = new ProductComponent("A", "D"){CurrentState = CurrentState.Installed};
            var avail = new ProductComponent("A", "D"){OriginInfo = CreateOriginInfo()};

            var expected = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{current}),
                new AvailableProductCatalog(ProductRef, new []{avail}),
                new UpdateCatalogStub(new []{expected})
            };
        }

        public static IEnumerable<object[]> MissingComponent()
        {
            var current = new ProductComponent("A", "D") { CurrentState = CurrentState.Installed }; ;
            var availSame = new ProductComponent("A", "D") { OriginInfo = CreateOriginInfo() };
            var availNew = new ProductComponent("B", "D") { OriginInfo = CreateOriginInfo() };
            var expectedSame = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep
            };
            var expectedNew = new ProductComponent("B", "D")
            {
                RequiredAction = ComponentAction.Update,
                OriginInfo = CreateOriginInfo()
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{current}),
                new AvailableProductCatalog(ProductRef, new []{availSame, availNew}),
                new UpdateCatalogStub(new []{expectedSame, expectedNew})
            };
        }

        public static IEnumerable<object[]> AllDeprecatedComponent()
        {
            var current = new ProductComponent("A", "D");
            var expected = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Delete
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{current}),
                new AvailableProductCatalog(ProductRef, new ProductComponent[0]),
                new UpdateCatalogStub(new []{ expected})
            };
        }

        public static IEnumerable<object[]> SingleDeprecatedComponent()
        {
            var currentSame = new ProductComponent("A", "D") { CurrentState = CurrentState.Installed }; ;
            var currentDep = new ProductComponent("B", "D") { CurrentState = CurrentState.Installed }; ;
            var availSame = new ProductComponent("A", "D") { OriginInfo = CreateOriginInfo() };
            var expectedSame = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep
            };
            var expectedDep = new ProductComponent("B", "D")
            {
                RequiredAction = ComponentAction.Delete
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{currentSame, currentDep}),
                new AvailableProductCatalog(ProductRef, new []{availSame}),
                new UpdateCatalogStub(new []{ expectedSame, expectedDep})
            };
        }

        public static IEnumerable<object[]> MissingAllComponent()
        {
            var avail = new ProductComponent("A", "D") { OriginInfo = CreateOriginInfo() };
            var expected = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Update,
                OriginInfo = CreateOriginInfo()
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductCatalog(ProductRef, new []{avail}),
                new UpdateCatalogStub(new []{expected})
            };
        }

        public static IEnumerable<object[]> DeleteUpdateKeep()
        {
            var currentKeep = new ProductComponent("A", "D") { CurrentState = CurrentState.Installed }; ;
            var currentDelete = new ProductComponent("B", "D") { CurrentState = CurrentState.Installed }; ;
            var availKeep = new ProductComponent("A", "D") { OriginInfo = CreateOriginInfo() };
            var availUpdate = new ProductComponent("C", "D") { OriginInfo = CreateOriginInfo() };
            var expectedKeep = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep
            };
            var expectedDelete = new ProductComponent("B", "D")
            {
                RequiredAction = ComponentAction.Delete
            };
            var expectedUpdate = new ProductComponent("C", "D")
            {
                RequiredAction = ComponentAction.Update,
                OriginInfo = CreateOriginInfo()
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{ currentKeep, currentDelete}),
                new AvailableProductCatalog(ProductRef, new []{availKeep, availUpdate}),
                new UpdateCatalogStub(new []{ expectedKeep, expectedDelete, expectedUpdate})
            };
        }
        
        [Theory]
        [MemberData(nameof(BothEmpty))]
        [MemberData(nameof(SameComponents))]
        [MemberData(nameof(MissingAllComponent))]
        [MemberData(nameof(MissingComponent))]
        [MemberData(nameof(AllDeprecatedComponent))]
        [MemberData(nameof(SingleDeprecatedComponent))]
        [MemberData(nameof(DeleteUpdateKeep))]
        public void Test(
            IInstalledProductCatalog installedProductCatalog,
            IAvailableProductCatalog availableProduct,
            IUpdateCatalog expected)
        {
            Assert.NotNull(installedProductCatalog);
            Assert.NotNull(availableProduct);
            Assert.NotNull(expected);

            var catalog = _builder.Build(installedProductCatalog, availableProduct);

            Assert.Equal(availableProduct.Product, catalog.Product, ProductReferenceEqualityComparer.Default);
            Assert.Equal(expected.Items.Count(), catalog.Items.Count());
            
            foreach (var item in expected.Items)
            {
                switch (item.RequiredAction)
                {
                    case ComponentAction.Keep:
                        Assert.Contains(catalog.ComponentsToKeep, x => x.Name.Equals(item.Name));
                        break;
                    case ComponentAction.Update:
                        Assert.Contains(catalog.ComponentsToInstall, x => x.Name.Equals(item.Name));
                        Assert.NotNull(item.OriginInfo);
                        break;
                    case ComponentAction.Delete:
                        Assert.Contains(catalog.ComponentsToDelete, x => x.Name.Equals(item.Name));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        public static IEnumerable<object[]> ComponentsVersionCompare()
        {
            yield return new object[]
            {
                new ProductComponent("A", "D") {CurrentState = CurrentState.Installed},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Keep
            };

            yield return new object[]
            {
                new ProductComponent("A", "D")
                {
                    CurrentState = CurrentState.Installed,
                    CurrentVersion = new Version(1, 0, 0)
                },
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Keep
            };
            yield return new object[]
            {
                new ProductComponent("A", "D"),
                new ProductComponent("A", "D")
                {
                    CurrentVersion = new Version(1, 0, 0),
                    OriginInfo = CreateOriginInfo()
                },
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D")
                {
                    CurrentState = CurrentState.Installed,
                    CurrentVersion = new Version(1, 0, 0)
                },
                new ProductComponent("A", "D")
                {
                    CurrentVersion = new Version(1, 0, 0),
                    OriginInfo = CreateOriginInfo()
                },
                ComponentAction.Keep
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {CurrentVersion = new Version(2, 0, 0)},
                new ProductComponent("A", "D")
                {
                    CurrentVersion = new Version(1, 0, 0),
                    OriginInfo = CreateOriginInfo()
                },
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {CurrentVersion = new Version(1, 0, 0)},
                new ProductComponent("A", "D")
                {
                    CurrentVersion = new Version(2, 0, 0),
                    OriginInfo = CreateOriginInfo()
                },
                ComponentAction.Update
            };
        }

        public static IEnumerable<object[]> ComponentsOriginCompare()
        {
            yield return new object[]
            {
                new ProductComponent("A", "D")
                {
                    CurrentState = CurrentState.Installed,
                    DiskSize = 5
                },
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(5)},
                ComponentAction.Keep
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {DiskSize = 4},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(5)},
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {DiskSize = 5},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(4)},
                ComponentAction.Update
            };

            var context1 = CreateRandomValidationContext();
            var context2 = CreateRandomValidationContext();
            yield return new object[]
            {
                new ProductComponent("A", "D")
                {
                    CurrentState = CurrentState.Installed,
                    DiskSize = 5, ValidationContext = context1
                },
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(5, context1)},
                ComponentAction.Keep
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {DiskSize = 5, ValidationContext = context1},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(5, context2)},
                ComponentAction.Update
            };

            yield return new object[]
            {
                new ProductComponent("A", "D")
                {
                    CurrentState = CurrentState.Installed,
                    DiskSize = 5, ValidationContext = context1
                },
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(5)},
                ComponentAction.Keep
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {DiskSize = 5},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(5, context1)},
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D")
                {
                    CurrentState = CurrentState.Installed,
                    ValidationContext = context1
                },
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(validationContext: context1)},
                ComponentAction.Keep
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {ValidationContext = context1},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(validationContext: context2)},
                ComponentAction.Update
            };
        }

        public static IEnumerable<object[]> ComponentsStateCompare()
        {
            yield return new object[]
            {
                new ProductComponent("A", "D") {CurrentState = CurrentState.Removed},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {CurrentState = CurrentState.Downloaded},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {CurrentState = CurrentState.None},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {CurrentState = CurrentState.Installed},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Keep
            };
        }


        [Theory]
        [MemberData(nameof(ComponentsVersionCompare))]
        [MemberData(nameof(ComponentsOriginCompare))]
        [MemberData(nameof(ComponentsStateCompare))]
        public void TestCompareComponents(
            ProductComponent current,
            ProductComponent available,
            ComponentAction expected)
        {           
            Assert.NotNull(current);
            Assert.NotNull(available);
            var result = _builder.GetComponentAction(current, available);
            Assert.Equal(expected, result);
        }



        private static IProductReference CreateProductRef(string name,
            Version? version = null,
            ProductReleaseType releaseType = ProductReleaseType.Stable)
        {
            return new ProductReference(name, version, releaseType);
        }
        
        private static IInstalledProduct CreateProduct(
            string name, 
            Version? version = null, 
            ProductReleaseType releaseType = ProductReleaseType.Stable)
        {
            return new InstalledProduct(CreateProductRef(name, version, releaseType), new Manifest(), "usr/");
        }


        private static OriginInfo CreateOriginInfo(long? size = null, ValidationContext? validationContext = null)
        {
            return new OriginInfo(new Uri("file://usr")) {Size = size, ValidationContext = validationContext};
        }

        private static ValidationContext CreateRandomValidationContext()
        {
            var buffer = new byte[16];
            new Random().NextBytes(buffer);
            return new ValidationContext(buffer, HashType.MD5);
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
        
        private class Manifest : IInstalledProductManifest
        {
            public IEnumerable<ProductComponent> Items { get; }
            public IProductReference Product { get; }
        }
    }
}
