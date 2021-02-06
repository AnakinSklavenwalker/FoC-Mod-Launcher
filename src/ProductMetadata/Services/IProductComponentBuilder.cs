using System;
using System.IO.Abstractions;
using ProductMetadata.Component;

namespace ProductMetadata.Services
{
    public interface IProductComponentBuilder
    {
        ComponentIntegrityInformation GetIntegrityInformation(IFileInfo file);
        
        Version? GetVersion(IFileInfo file);

        long GetSize(IFileInfo file);
    }
}