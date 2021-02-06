using System;
using Validation;

namespace ProductMetadata.Component
{
    public sealed record ProductComponent
    {
        private string? _realPath;
        
        public string Destination { get; internal init; }

        public string Name { get; }

        public ComponentAction RequiredAction { get; init; }

        public DetectionState DetectedState { get; set; }

        public Version? CurrentVersion { get; set; }

        public OriginInfo? OriginInfo { get; init; }

        public ComponentIntegrityInformation IntegrityInformation { get; init; }

        public long? DiskSize { get; init; }
        
        public ProductComponent(string name, string destination)
        {
            Requires.NotNullOrEmpty(name, nameof(name));
            Requires.NotNull(destination, nameof(destination));
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

        public string GetFilePath()
        {
            if (_realPath is null)
            {
                var fs = new System.IO.Abstractions.FileSystem();
                _realPath = fs.Path.Combine(Destination, Name);
            }
            return _realPath;
        }
    }
}
