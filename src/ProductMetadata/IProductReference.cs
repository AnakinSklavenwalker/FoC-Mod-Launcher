using System;

namespace ProductMetadata
{
    public interface IProductReference
    {
        string Name { get; }

        Version? Version { get; }
        
        string? Branch { get; }
    }
}