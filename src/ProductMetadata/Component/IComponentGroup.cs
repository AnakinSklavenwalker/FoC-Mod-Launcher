using System.Collections.Generic;

namespace ProductMetadata.Component
{
    public interface IComponentGroup : IProductComponent
    {
        IList<IProductComponent> Components { get; } 
    }
}