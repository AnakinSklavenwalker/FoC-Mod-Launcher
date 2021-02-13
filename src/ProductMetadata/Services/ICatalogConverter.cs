namespace ProductMetadata.Services
{
    public interface ICatalogConverter<in TCatalogModel>
    {
        public ICatalog Convert(TCatalogModel catalogModel);

    }
}