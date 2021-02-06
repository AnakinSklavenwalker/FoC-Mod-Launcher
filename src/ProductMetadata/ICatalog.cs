using System.Collections.Generic;

namespace ProductMetadata
{
    public interface ICatalog
    {
        IEnumerable<ProductComponent> Items { get; }
    }
}