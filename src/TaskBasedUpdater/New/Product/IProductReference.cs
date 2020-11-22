using System;

namespace TaskBasedUpdater.New.Product
{
    public interface IProductReference
    {
        string Name { get; }

        Version? Version { get; }
        
        ProductReleaseType ReleaseType { get; }
    }
}