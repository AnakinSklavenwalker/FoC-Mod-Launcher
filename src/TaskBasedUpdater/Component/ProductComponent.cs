using System;
using Validation;

namespace TaskBasedUpdater.Component
{
    public sealed record ProductComponent
    {
        public string Destination { get; }

        public string Name { get; }

        public ComponentAction RequiredAction { get; init; }

        public CurrentState CurrentState { get; set; }

        public Version? CurrentVersion { get; set; }

        public OriginInfo? OriginInfo { get; init; }

        public ValidationContext? ValidationContext { get; init; }

        public long? DiskSize { get; init; }

        public ProductComponent(string name, string destination)
        {
            Requires.NotNullOrEmpty(name, nameof(name));
            Requires.NotNullOrEmpty(destination, nameof(destination));
            Name = name;
            Destination = destination;
        }

        public override string ToString()
        {
            return $"{Name}, destination='{Destination}'";
        }

        public bool Equals(ProductComponent? other)
        {
            return ProductComponentIdentityComparer.Default.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return ProductComponentIdentityComparer.Default.GetHashCode(this);
        }
    }
}
