using System;
using Validation;

namespace TaskBasedUpdater.New.Product
{
    public class ProductReference : IProductReference
    {
        public string Name { get; }
        public Version? Version { get; init; }
        public ProductReleaseType ReleaseType { get; init; }

        public ProductReference(string name, Version? version = null, ProductReleaseType releaseType = ProductReleaseType.Stable)
        {
            Requires.NotNullOrEmpty(name, nameof(name));
            Name = name;
            Version = version;
            ReleaseType = releaseType;
        }
    }
}