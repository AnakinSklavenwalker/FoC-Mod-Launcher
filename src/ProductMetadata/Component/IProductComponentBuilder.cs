using System;
using System.IO.Abstractions;
using CommonUtilities;

namespace ProductMetadata.Component
{
    public interface IProductComponentBuilder
    {
        HashType HashType { get; }
        
        Version? GetVersion(IFileInfo file);
    }
}