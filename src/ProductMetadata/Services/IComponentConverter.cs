using ProductMetadata.Component;

namespace ProductMetadata.Services
{
    public interface IComponentConverter<in TModel, out TComponent> where TComponent : IProductComponentIdentity
    {
        TComponent Convert(TModel metaModel);
    }
}