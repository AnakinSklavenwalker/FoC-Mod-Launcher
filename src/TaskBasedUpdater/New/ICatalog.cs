using System.Collections.Generic;
using TaskBasedUpdater.New.Product.Component;

namespace TaskBasedUpdater.New
{
    public interface ICatalog
    {
        IEnumerable<ProductComponent> Items { get; }
    }
}