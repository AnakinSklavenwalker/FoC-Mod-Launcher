using System.Collections.Generic;
using ProductMetadata;
using ProductMetadata.Component;

namespace ProductUpdater.New.Update
{
    public interface IUpdateCatalog : ICatalog
    {
        IProductReference Product { get; }
        IEnumerable<ProductComponent> ComponentsToInstall { get; }
        IEnumerable<ProductComponent> ComponentsToKeep { get; }
        IEnumerable<ProductComponent> ComponentsToDelete { get; }
    }
}