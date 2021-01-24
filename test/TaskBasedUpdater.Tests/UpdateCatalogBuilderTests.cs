using System;
using System.Collections.Generic;
using System.Linq;
using TaskBasedUpdater.New.Product;
using TaskBasedUpdater.New.Product.Component;
using TaskBasedUpdater.New.Product.Manifest;
using TaskBasedUpdater.New.Update;
using TaskBasedUpdater.Verification;
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
            var available = new AvailableProductManifest(p1, new ProductComponent[0]);
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
                new AvailableProductManifest(ProductRef, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductManifest(ProductRef, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductManifest(ProductRef, new ProductComponent[0]),
                new UpdateCatalogStub(new ProductComponent[0])
            };
        }

        public static IEnumerable<object[]> SameComponents()
        {
            var current = new ProductComponent("A", "D"){ DetectedState = DetectionState.Present };
            var avail = new ProductComponent("A", "D"){OriginInfo = CreateOriginInfo()};

            var expected = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep,
                DetectedState = DetectionState.Present
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{current}),
                new AvailableProductManifest(ProductRef, new []{avail}),
                new UpdateCatalogStub(new []{expected})
            };
        }

        public static IEnumerable<object[]> MissingComponent()
        {
            var current = new ProductComponent("A", "D") { DetectedState = DetectionState.Present };
            var availSame = new ProductComponent("A", "D") { OriginInfo = CreateOriginInfo() };
            var availNew = new ProductComponent("B", "D") { OriginInfo = CreateOriginInfo() };
            var expectedSame = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep,
                DetectedState = DetectionState.Present
            };
            var expectedNew = new ProductComponent("B", "D")
            {
                RequiredAction = ComponentAction.Update,
                DetectedState = DetectionState.Absent,
                OriginInfo = CreateOriginInfo()
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{current}),
                new AvailableProductManifest(ProductRef, new []{availSame, availNew}),
                new UpdateCatalogStub(new []{expectedSame, expectedNew})
            };
        }

        public static IEnumerable<object[]> AllDeprecatedComponent()
        {
            var currentA = new ProductComponent("A", "D") {DetectedState = DetectionState.Present};
            var currentB = new ProductComponent("B", "D") {DetectedState = DetectionState.Absent};
            var expectedA = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Delete,
                DetectedState = DetectionState.Present
            };
            var expectedB = new ProductComponent("B", "D")
            {
                RequiredAction = ComponentAction.Delete,
                DetectedState = DetectionState.Absent
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{currentA, currentB}),
                new AvailableProductManifest(ProductRef, new ProductComponent[0]),
                new UpdateCatalogStub(new []{ expectedA, expectedB})
            };
        }

        public static IEnumerable<object[]> SingleDeprecatedComponent()
        {
            var currentSame = new ProductComponent("A", "D") { DetectedState = DetectionState.Present };
            var currentDep = new ProductComponent("B", "D") { DetectedState = DetectionState.Present };
            var availSame = new ProductComponent("A", "D") { OriginInfo = CreateOriginInfo() };
            var expectedSame = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep,
                DetectedState = DetectionState.Present
            };
            var expectedDep = new ProductComponent("B", "D")
            {
                RequiredAction = ComponentAction.Delete,
                DetectedState = DetectionState.Present
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{currentSame, currentDep}),
                new AvailableProductManifest(ProductRef, new []{availSame}),
                new UpdateCatalogStub(new []{ expectedSame, expectedDep})
            };
        }

        public static IEnumerable<object[]> MissingAllComponent()
        {
            var avail = new ProductComponent("A", "D") { OriginInfo = CreateOriginInfo() };
            var expected = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Update,
                DetectedState = DetectionState.Absent,
                OriginInfo = CreateOriginInfo()
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new ProductComponent[0]),
                new AvailableProductManifest(ProductRef, new []{avail}),
                new UpdateCatalogStub(new []{expected})
            };
        }

        public static IEnumerable<object[]> DeleteUpdateKeep()
        {
            var currentKeep = new ProductComponent("A", "D") { DetectedState = DetectionState.Present };
            var currentDelete = new ProductComponent("B", "D") { DetectedState = DetectionState.Present };
            var availKeep = new ProductComponent("A", "D") { OriginInfo = CreateOriginInfo() };
            var availUpdate = new ProductComponent("C", "D") { OriginInfo = CreateOriginInfo() };
            var expectedKeep = new ProductComponent("A", "D")
            {
                RequiredAction = ComponentAction.Keep,
                DetectedState = DetectionState.Present
            };
            var expectedDelete = new ProductComponent("B", "D")
            {
                RequiredAction = ComponentAction.Delete,
                DetectedState = DetectionState.Present
            };
            var expectedUpdate = new ProductComponent("C", "D")
            {
                RequiredAction = ComponentAction.Update,
                DetectedState = DetectionState.Absent,
                OriginInfo = CreateOriginInfo()
            };

            yield return new object[]
            {
                new InstalledProductCatalog(Product, new []{ currentKeep, currentDelete}),
                new AvailableProductManifest(ProductRef, new []{availKeep, availUpdate}),
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
            IAvailableProductManifest availableProduct,
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
                var matching = catalog.Items.First(x => x.Name.Equals(item.Name));

                Assert.Equal(item.RequiredAction, matching.RequiredAction);
                Assert.Equal(item.DetectedState, matching.DetectedState);

                if (item.RequiredAction == ComponentAction.Update)
                    Assert.NotNull(item.OriginInfo);
            }
        }


        public static IEnumerable<object[]> ComponentsVersionCompare()
        {
            yield return new object[]
            {
                new ProductComponent("A", "D") {DetectedState = DetectionState.Present},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Keep
            };

            yield return new object[]
            {
                new ProductComponent("A", "D")
                {
                    DetectedState = DetectionState.Present,
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
                    DetectedState = DetectionState.Present,
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
                    DetectedState = DetectionState.Present,
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
                    DetectedState = DetectionState.Present,
                    DiskSize = 5, VerificationContext = context1
                },
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(5, context1)},
                ComponentAction.Keep
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {DiskSize = 5, VerificationContext = context1},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(5, context2)},
                ComponentAction.Update
            };

            yield return new object[]
            {
                new ProductComponent("A", "D")
                {
                    DetectedState = DetectionState.Present,
                    DiskSize = 5, VerificationContext = context1
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
                    DetectedState = DetectionState.Present,
                    VerificationContext = context1
                },
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(nullableVerificationContext: context1)},
                ComponentAction.Keep
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {VerificationContext = context1},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(nullableVerificationContext: context2)},
                ComponentAction.Update
            };

            var context3 = new VerificationContext((byte[]) context1.Hash.Clone(), HashType.MD5);
            yield return new object[]
            {
                new ProductComponent("A", "D") {VerificationContext = context1, DetectedState = DetectionState.Present},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo(nullableVerificationContext: context3)},
                ComponentAction.Keep
            };
        }

        public static IEnumerable<object[]> ComponentsStateCompare()
        {
            yield return new object[]
            {
                new ProductComponent("A", "D") {DetectedState = DetectionState.None},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {DetectedState = DetectionState.Absent},
                new ProductComponent("A", "D") {OriginInfo = CreateOriginInfo()},
                ComponentAction.Update
            };
            yield return new object[]
            {
                new ProductComponent("A", "D") {DetectedState = DetectionState.Present},
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


        private static OriginInfo CreateOriginInfo(long? size = null, VerificationContext? nullableVerificationContext = null)
        {
            VerificationContext verificationContext = nullableVerificationContext ?? VerificationContext.None;
            return new OriginInfo(new Uri("file://usr")) {Size = size, VerificationContext = verificationContext };
        }

        private static VerificationContext CreateRandomValidationContext()
        {
            var buffer = new byte[16];
            new Random().NextBytes(buffer);
            return new VerificationContext(buffer, HashType.MD5);
        }

        private class UpdateCatalogStub : IUpdateCatalog
        { 
            public IProductReference Product { get; }
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
