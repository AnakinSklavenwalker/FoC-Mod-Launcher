using System;

namespace TaskBasedUpdater.Component
{
    public record ProductComponent
    {
        public string Destination { get; }

        public string Name { get; }

        public ComponentAction RequiredAction { get; init; }

        public CurrentState CurrentState { get; set; }

        public Version? CurrentVersion { get; set; }

        public OriginInfo? OriginInfo { get; init; }

        public long? DiskSize { get; init; }

        public ProductComponent(string name, string destination)
        {
            Name = name;
            Destination = destination;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? $"{Name}, destination='{Destination}'" : base.ToString();
        }

        public virtual bool Equals(ProductComponent? other)
        {
            return ProductComponentIdentityComparer.Default.Equals(this, other);
        }

        public override int GetHashCode()
        {
            return ProductComponentIdentityComparer.Default.GetHashCode(this);
        }
    }
}
