using System;
using Validation;

namespace ProductMetadata
{
    public class ProductReference : IProductReference
    {
        public string Name { get; }
        public Version? Version { get; init; }
        public string? Branch { get; }

        public ProductReference(string name, Version? version = null, string? branch = null)
        {
            Requires.NotNullOrEmpty(name, nameof(name));
            Name = name;
            Version = version;
            Branch = branch;
        }

        public override string ToString()
        {
            return $"Product {Name};v{Version};branch:{Branch}";
        }
    }
}