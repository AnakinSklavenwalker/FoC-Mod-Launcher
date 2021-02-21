using System;
using System.Collections.Generic;
using ProductMetadata.Services;
using Xunit;

namespace ProductMetadata.Tests
{
    public class VariableResolverTests
    {
        private readonly VariableResolver _resolver;

        private readonly Dictionary<string, string?> _variables = new ();

        public VariableResolverTests()
        {
            _resolver = new VariableResolver();
            _variables.Add("Test_Var", "I'm a test");


            _variables.Add("A", "[B]");
            _variables.Add("B", "C");

            _variables.Add("D", "%temp%");

            Environment.SetEnvironmentVariable("TestEnvVar", "Hello Var", EnvironmentVariableTarget.Process);
        }


        [Theory]
        [InlineData("test", "test")]
        [InlineData("[test", "[test")]
        [InlineData("[test]", "")]
        [InlineData("[Test_Var]", "I'm a test")]
        [InlineData("[Test_Var]123", "I'm a test123")]
        [InlineData("[Test_Var]XX[Test_Var]", "I'm a testXXI'm a test")]
        [InlineData("[D]", "%temp%")]
        [InlineData(@"[WindowsFolder]Test", @"C:\Windows\Test")]
        [InlineData(@"[windir]", @"C:\Windows")]
        [InlineData(@"[TestEnvVar]", @"Hello Var")]
        public void TestResolver(string input, string output)
        {
            Assert.Equal(output, _resolver.ResolveVariables(input, _variables));
        }
    }
}