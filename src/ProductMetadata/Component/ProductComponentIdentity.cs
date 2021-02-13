using System;
using Validation;

namespace ProductMetadata.Component
{
    public class ProductComponentIdentity : IProductComponentIdentity
    {
        public string Id { get; }
        public Version? Version { get; }
        public string? Branch { get; }
        
        public ProductComponentIdentity(string id, Version? version = null, string? branch = null)
        {
            Requires.NotNullOrEmpty(id, nameof(id));
            Id = id;
            Version = version;
            Branch = branch;
        }

        public override string ToString()
        {
            return GetUniqueId(false);
        }

        public string GetUniqueId() => GetUniqueId(false);

        public string GetUniqueId(bool excludeVersion) => Format(this, excludeVersion);

        public bool Equals(IProductComponentIdentity? other)
        {
            return ProductComponentIdentityComparer.Default.Equals(this, other);
        }

        internal static string Format(IProductComponentIdentity identity, bool excludeVersion = false)
        {
            Requires.NotNull(identity, nameof(identity));
            return Utilities.FormatIdentity(identity.Id, excludeVersion ? null : identity.Version, identity.Branch);
        }
    }
}