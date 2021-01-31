using System.Collections.Generic;
using ProductMetadata.Component;

namespace ProductMetadata
{
    public interface ICatalog
    {
        IEnumerable<ProductComponent> Items { get; }
    }
}