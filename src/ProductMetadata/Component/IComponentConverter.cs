namespace ProductMetadata.Component
{
    public interface IComponentConverter<T>
    {
        ProductComponent Convert(T metaModel);
    }
}