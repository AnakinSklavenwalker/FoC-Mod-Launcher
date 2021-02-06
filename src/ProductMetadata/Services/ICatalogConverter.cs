namespace ProductMetadata.Services
{
    public interface ICatalogConverter<in TCatalogModel, TComponentModel>
    {
        public IComponentConverter<TComponentModel> ComponentConverter { get; }

        public ICatalog Convert(TCatalogModel catalogModel);

    }
}