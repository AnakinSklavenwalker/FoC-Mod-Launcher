using System.Collections.Generic;
using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New.Update
{
    public interface IUpdateCatalog : ICatalog
    {
        IProductReference Product { get; }
        IEnumerable<ProductComponent> ComponentsToInstall { get; }
        IEnumerable<ProductComponent> ComponentsToKeep { get; }
        IEnumerable<ProductComponent> ComponentsToDelete { get; }
    }
}