namespace TaskBasedUpdater.Component
{
    public interface IComponentConverter<T>
    {
        ProductComponent Convert(T metaModel);
    }
}