using System;

namespace TaskBasedUpdater.Component
{
    // TODO: init
    public record ProductComponent
    {
        public string Destination { get; init; }

        public string Name { get; init; }

        // TODO: init
        public UpdateAction RequiredAction { get; set; }

        public CurrentState CurrentState { get; set; }

        public Version? CurrentVersion { get; set; }

        // TODO: init
        public OriginInfo? OriginInfo { get; set; }

        // TODO: init
        public long? DiskSize { get; set; }

        public ProductComponent(string name, string destination)
        {
            Name = name;
            Destination = destination;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? $"{Name}, destination='{Destination}'" : base.ToString();
        }

        public bool Equals(ProductComponent? other)
        {
            return UpdateItemIdentityComparer.Default.Equals(this, other);
        }
    }
}
