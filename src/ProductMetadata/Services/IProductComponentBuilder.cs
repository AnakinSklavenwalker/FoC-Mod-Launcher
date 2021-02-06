using System;
using System.IO.Abstractions;
using ProductMetadata.Component;

namespace ProductMetadata.Services
{
    public interface IProductComponentBuilder
    {
        // TODO: Change to ComponentIdentity
        string ResolveComponentDestination(ProductComponent component, IInstalledProduct product);

        ComponentIntegrityInformation GetIntegrityInformation(IFileInfo file);
        
        Version? GetVersion(IFileInfo file);

        long GetSize(IFileInfo file);
    }
}