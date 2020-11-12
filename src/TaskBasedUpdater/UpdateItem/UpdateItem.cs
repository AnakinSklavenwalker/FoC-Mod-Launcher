using System;

namespace TaskBasedUpdater.Component
{
    public class UpdateItem : IUpdateItem
    {
        public string Destination { get; set; }

        public string Name { get; set; }

        public UpdateAction RequiredAction { get; set; }

        public CurrentState CurrentState { get; set; }

        public Version? CurrentVersion { get; set; }

        public OriginInfo? OriginInfo { get; set; }

        public long? DiskSize { get; set; }
        
        public override string ToString()
        {
            return !string.IsNullOrEmpty(Name) ? $"{Name},destination='{Destination}'" : base.ToString();
        }

        public bool Equals(IUpdateItem other)
        {
            return UpdateItemIdentityComparer.Default.Equals(this, other);
        }
    }
}
