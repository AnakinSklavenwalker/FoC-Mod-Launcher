using TaskBasedUpdater.New.Product.Component;

namespace TaskBasedUpdater.New.Update
{
    public delegate ComponentAction CompareComponentDelegate(ProductComponent currentComponent, ProductComponent availableComponent);
}