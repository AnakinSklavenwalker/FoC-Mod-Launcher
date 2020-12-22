using System;
using System.IO.Abstractions;

namespace TaskBasedUpdater.Component
{
    public interface IProductComponentBuilder
    {
        HashType HashType { get; }
        
        Version? GetVersion(IFileInfo file);
    }
}