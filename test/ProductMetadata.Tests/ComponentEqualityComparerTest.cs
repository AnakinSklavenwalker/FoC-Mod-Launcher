using System;
using System.Collections.Generic;
using ProductMetadata.Component;
using Xunit;

namespace ProductMetadata.Tests
{
    public class ComponentEqualityComparerTests
    {
        private static readonly IList<(IProductComponentIdentity a, IProductComponentIdentity b)> TestData =

            new List<(IProductComponentIdentity a, IProductComponentIdentity b)>
            {
                /*0*/(new ProductComponentIdentity("0"), new ProductComponentIdentity("0")),
                /*1*/(new ProductComponentIdentity("1", new Version(1, 0)), new ProductComponentIdentity("1")),
                /*2*/(new ProductComponentIdentity("2", new Version(1, 0), "123"), new ProductComponentIdentity("2")),
                /*3*/(new ProductComponentIdentity("3", new Version(1, 0)), new ProductComponentIdentity("3", new Version(2, 0))),
                /*4*/(new ProductComponentIdentity("4", null, "123"), new ProductComponentIdentity("4", null, "456")),
                /**/
                /*5*/(new ProductComponentIdentity("5"), new ProductComponentIdentity("A")),
                /**/
                /*6*/(new ProductComponentIdentity("6", new Version(1,0)), new ProductComponentIdentity("6", new Version(1,0))),
                /*7*/(new ProductComponentIdentity("7", new Version(1,0), "123"), new ProductComponentIdentity("7", new Version(1,0), "456")),
                
                /*8*/(new ProductComponentIdentity("8", new Version(1,0), "123"), new ProductComponentIdentity("8", new Version(1,0), "123"))
            };


        public static IEnumerable<object[]> GetTestDataIgnoreVersion_Equals()
        {
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[0].a, TestData[0].a, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[0].a, TestData[0].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[1].a, TestData[1].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[3].a, TestData[3].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[6].a, TestData[6].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[8].a, TestData[8].b, true };
        }

        public static IEnumerable<object[]> GetTestDataIgnoreVersion_NotEquals()
        {
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[2].a, TestData[2].b, false };
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[4].a, TestData[4].b, false };
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[5].a, TestData[5].b, false };
            yield return new object[] { ProductComponentIdentityComparer.VersionIndependent, TestData[7].a, TestData[7].b, false };
        }


        public static IEnumerable<object[]> GetTestDataIgnoreBranchVersion_Equals()
        {
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[0].a, TestData[0].a, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[1].a, TestData[1].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[2].a, TestData[2].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[3].a, TestData[3].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[4].a, TestData[4].b, true };
            
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[6].a, TestData[6].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[7].a, TestData[7].b, true };
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[8].a, TestData[8].b, true };
           

        }

        public static IEnumerable<object[]> GetTestDataIgnoreBranchVersion_NotEquals()
        {
            yield return new object[] { ProductComponentIdentityComparer.VersionAndBranchIndependent, TestData[5].a, TestData[5].b, false };
        }


        public static IEnumerable<object[]> GetTestDataDefault_Equals()
        {
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[0].a, TestData[0].b, true };
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[6].a, TestData[6].b, true };
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[8].a, TestData[8].a, true };
        }

        public static IEnumerable<object[]> GetTestDataDefault_NotEquals()
        {
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[1].a, TestData[1].b, false };
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[2].a, TestData[2].b, false };
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[3].a, TestData[3].b, false };
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[4].a, TestData[4].b, false };
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[5].a, TestData[5].b, false };
            yield return new object[] { ProductComponentIdentityComparer.Default, TestData[7].a, TestData[7].b, false };

        }
        
        [Theory]
        [MemberData(nameof(GetTestDataIgnoreVersion_Equals))]
        [MemberData(nameof(GetTestDataIgnoreVersion_NotEquals))]
        [MemberData(nameof(GetTestDataIgnoreBranchVersion_Equals))]
        [MemberData(nameof(GetTestDataIgnoreBranchVersion_NotEquals))]
        [MemberData(nameof(GetTestDataDefault_Equals))]
        [MemberData(nameof(GetTestDataDefault_NotEquals))]
        public void TestEquals(
            IEqualityComparer<IProductComponentIdentity> comparer,
            IProductComponentIdentity p1, IProductComponentIdentity p2, bool equals)
        {
            Assert.Equal(equals, comparer.Equals(p1, p2));
        }

    }
}