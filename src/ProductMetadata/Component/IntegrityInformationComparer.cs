using System.Collections.Generic;
using System.Linq;

namespace ProductMetadata.Component
{
    public class IntegrityInformationComparer : IEqualityComparer<ComponentIntegrityInformation>
    {
        public bool Equals(ComponentIntegrityInformation x, ComponentIntegrityInformation y)
        {
            return x.HashType == y.HashType && x.Hash.SequenceEqual(y.Hash);
        }

        public int GetHashCode(ComponentIntegrityInformation obj)
        {
            unchecked
            {
                return (obj.Hash.GetHashCode() * 397) ^ (int)obj.HashType;
            }
        }
    }
}