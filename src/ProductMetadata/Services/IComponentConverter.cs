using ProductMetadata.Component;

namespace ProductMetadata.Services
{
    public interface IComponentConverter<T>
    {
        ProductComponent Convert(T metaModel);
    }
}