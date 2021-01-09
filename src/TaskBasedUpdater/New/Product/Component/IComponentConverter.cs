namespace TaskBasedUpdater.New.Product.Component
{
    public interface IComponentConverter<T>
    {
        ProductComponent Convert(T metaModel);
    }
}