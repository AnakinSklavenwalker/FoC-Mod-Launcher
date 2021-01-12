using System;
using TaskBasedUpdater.New.Product.Component;

namespace TaskBasedUpdater.Elevation
{
    public class ElevationRequestData : IEquatable<ElevationRequestData>
    {
        public ProductComponent ProductComponent { get; }

        public Exception Exception { get; }

        public ElevationRequestData(Exception exception, ProductComponent productComponent)
        {
            Exception = exception;
            ProductComponent = productComponent;
        }

        public bool Equals(ElevationRequestData? other)
        {
            if (other is null)
                return false;
            return ReferenceEquals(this, other) || ProductComponent.Equals(other.ProductComponent);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) 
                return false;
            if (ReferenceEquals(this, obj)) 
                return true;
            return obj.GetType() == GetType() && Equals((ElevationRequestData) obj);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(ProductComponent);
        }
    }
}