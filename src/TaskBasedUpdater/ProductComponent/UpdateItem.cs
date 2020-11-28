using System;

namespace TaskBasedUpdater.ProductComponent
{
    // TODO: init
    public record UpdateItem : IUpdateItem
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

        public UpdateItem(string name, string destination)
        {
            Name = name;
            Destination = destination;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? $"{Name}, destination='{Destination}'" : base.ToString();
        }

        public bool Equals(IUpdateItem other)
        {
            return UpdateItemIdentityComparer.Default.Equals(this, other);
        }
    }
}
