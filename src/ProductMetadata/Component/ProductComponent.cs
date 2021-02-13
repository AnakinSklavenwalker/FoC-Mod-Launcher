using System;
using Validation;

namespace ProductMetadata.Component
{
    public abstract class ProductComponent : IProductComponent
    {
        public string Id { get; }
        public Version? Version { get; }
        public string? Branch { get; }
        public abstract ComponentType Type { get; }
        public DetectionState DetectedState { get; private protected set; }

        protected ProductComponent(IProductComponentIdentity identity)
        {
            Requires.NotNull(identity, nameof(identity));
            Id = identity.Id;
            Version = identity.Version;
            Branch = identity.Branch;
        }

        public override string ToString()
        {
            return (!string.IsNullOrEmpty(Id) ? $"{GetUniqueId()},type={Type}" : base.ToString()) ?? GetType().Name;
        }

        public string GetUniqueId()
        {
            return ProductComponentIdentity.Format(this);
        }

        public bool Equals(IProductComponentIdentity? other)
        {
            return ProductComponentIdentityComparer.Default.Equals(this, other);
        }
    }
}
