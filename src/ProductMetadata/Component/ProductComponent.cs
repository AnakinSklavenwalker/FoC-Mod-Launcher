using System;
using System.Collections.Generic;
using Validation;

namespace ProductMetadata.Component
{
    public enum ComponentType
    {
        None,
        File
    }

    public class FileCondition
    {
       public string FilePath { get; }

       public ComponentIntegrityInformation IntegrityInformation { get; }

       public FileCondition(string filePath) : this(filePath, ComponentIntegrityInformation.None)
       {
       }

       public FileCondition(string filePath, ComponentIntegrityInformation integrityInformation)
       {
           Requires.NotNullOrEmpty(filePath, nameof(filePath));
           FilePath = filePath;
           IntegrityInformation = integrityInformation;
       }
    }

    public interface IProductComponentIdentity : IEquatable<IProductComponentIdentity>
    {
        string Id { get; }
        Version Version { get; }

        string GetUniqueId();
    }

    public interface IProductComponent : IProductComponentIdentity
    {
        ComponentType Type { get; }
    }

    public interface IInstallableComponent : IProductComponent
    {
        ICollection<OriginInfo> OriginInfos { get; }

        ICollection<FileCondition> FileConditions { get; }
    }


    public class ProductComponentIdentity : IProductComponentIdentity
    {
        public string Id { get; }

        public Version Version { get; }

        public string GetUniqueId()
        { 
            throw new NotImplementedException();
        }

        public bool Equals(IProductComponentIdentity? other)
        {
            throw new NotImplementedException();
        }
    }






















    public sealed record ProductComponentOld
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
        
        public ProductComponentOld(string name, string destination)
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
