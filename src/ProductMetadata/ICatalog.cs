using System.Collections.Generic;
using ProductMetadata.Component;

namespace ProductMetadata
{
    public interface ICatalog
    {
        IEnumerable<IProductComponentIdentity> Items { get; }
    }

    public interface ICatalog<T> : ICatalog where T : class, IProductComponentIdentity
    {
        new IEnumerable<T> Items { get; }
    }
}