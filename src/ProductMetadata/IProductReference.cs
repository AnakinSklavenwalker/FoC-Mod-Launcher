using System;

namespace ProductMetadata
{
    public interface IProductReference
    {
        string Name { get; }

        Version? Version { get; }
        
        ProductReleaseType ReleaseType { get; }

        //string? Branch { get; }
    }
}