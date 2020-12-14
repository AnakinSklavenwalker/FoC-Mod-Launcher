using System.Collections.Generic;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New.Update
{
    public interface IUpdateCatalog : ICatalog
    {
        IProductReference Product { get; }
        UpdateRequestAction RequestAction { get; }
        IEnumerable<ProductComponent> ComponentsToInstall { get; }
        IEnumerable<ProductComponent> ComponentsToKeep { get; }
        IEnumerable<ProductComponent> ComponentsToDelete { get; }
    }
}