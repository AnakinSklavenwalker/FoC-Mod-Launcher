using System;
using System.Collections.Generic;
using ProductMetadata.Component;
using Xunit;

namespace ProductMetadata.Tests
{
    public class ProductReferenceEqualityComparerTests
    {


        private static readonly IList<(IProductReference a, IProductReference b)> TestData =

            new List<(IProductReference a, IProductReference b)>
            {
                /*0*/(new ProductReference("A"), new ProductReference("A")),
                /*1*/(new ProductReference("A", new Version(1, 0)), new ProductReference("A")),
                /*2*/(new ProductReference("A", new Version(1, 0), "123"), new ProductReference("A")),
                /*3*/(new ProductReference("A", new Version(1, 0)), new ProductReference("A", new Version(2, 0))),
                /*4*/(new ProductReference("A", null, "123"), new ProductReference("A", null, "456")),
                /*5*/(new ProductReference("A", null, "123"), CreateProduct(new ProductReference("A"))),
                /**/
                /*6*/(new ProductReference("A"), new ProductReference("B")),
                /*7*/(new ProductReference("A"), CreateProduct(new ProductReference("B"))),
                /**/
                /*8*/(new ProductReference("A", new Version(1,0)), new ProductReference("A", new Version(1,0))),
                /*9*/(new ProductReference("A", new Version(1,0), "123"), new ProductReference("A", new Version(1,0), "456")),
                
                /*10*/(new ProductReference("A", new Version(1,0), "123"), new ProductReference("A", new Version(1,0), "123"))
            };

        public static IEnumerable<object[]> GetTestDataNameOnly_Equals()
        {
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[0].a, TestData[0].a, true };
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[0].a, TestData[0].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[1].a, TestData[1].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[2].a, TestData[2].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[3].a, TestData[3].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[4].a, TestData[4].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[5].a, TestData[5].b, true };

            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[10].a, TestData[10].b, true };
        }

        public static IEnumerable<object[]> GetTestDataNameOnly_NotEquals()
        {
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[6].a, TestData[6].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.NameOnly, TestData[7].a, TestData[7].b, false };
        }


        public static IEnumerable<object[]> GetTestDataVersion_Equals()
        {
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[0].a, TestData[0].a, true };
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[0].a, TestData[0].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[4].a, TestData[4].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[5].a, TestData[5].b, true };

            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[8].a, TestData[8].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[9].a, TestData[9].b, true };

            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[10].a, TestData[10].b, true };
        }

        public static IEnumerable<object[]> GetTestDataVersion_NotEquals()
        {
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[1].a, TestData[1].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[2].a, TestData[2].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[3].a, TestData[3].b, false };

            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[6].a, TestData[6].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.VersionAware, TestData[7].a, TestData[7].b, false };
        }


        public static IEnumerable<object[]> GetTestDataBranch_Equals()
        {
            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[0].a, TestData[0].a, true };
            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[0].a, TestData[0].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[1].a, TestData[1].b, true };
            
            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[3].a, TestData[3].b, true };

            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[8].a, TestData[8].b, true };

            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[10].a, TestData[10].b, true };
            
        }

        public static IEnumerable<object[]> GetTestDataBranch_NotEquals()
        {
            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[2].a, TestData[2].b, false };

            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[6].a, TestData[6].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[7].a, TestData[7].b, false };

            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[4].a, TestData[4].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[5].a, TestData[5].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.BranchAware, TestData[9].a, TestData[9].b, false };
        }


        public static IEnumerable<object[]> GetTestDataDefault_Equals()
        {
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[0].a, TestData[0].a, true };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[0].a, TestData[0].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[8].a, TestData[8].b, true };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[10].a, TestData[10].a, true };
        }

        public static IEnumerable<object[]> GetTestDataDefault_NotEquals()
        {
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[1].a, TestData[1].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[2].a, TestData[2].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[3].a, TestData[3].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[4].a, TestData[4].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[5].a, TestData[5].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[6].a, TestData[6].b, false };
            yield return new object[] { ProductReferenceEqualityComparer.Default, TestData[7].a, TestData[7].b, false };
            
        }


        [Theory]
        [MemberData(nameof(GetTestDataNameOnly_Equals))]
        [MemberData(nameof(GetTestDataNameOnly_NotEquals))]
        [MemberData(nameof(GetTestDataVersion_Equals))]
        [MemberData(nameof(GetTestDataVersion_NotEquals))]
        [MemberData(nameof(GetTestDataBranch_Equals))]
        [MemberData(nameof(GetTestDataBranch_NotEquals))]
        [MemberData(nameof(GetTestDataDefault_Equals))]
        [MemberData(nameof(GetTestDataDefault_NotEquals))]
        public void TestEquals(
            IEqualityComparer<IProductReference> comparer,
            IProductReference p1, IProductReference p2, bool equals)
        {
            Assert.Equal(equals, comparer.Equals(p1, p2));
        }


        private static IInstalledProduct CreateProduct(IProductReference reference)
        {
            return new InstalledProduct(reference, new Manifest.Manifest(reference, new List<IProductComponent>(0)),
                "./");
        }
    }
}
