using System;
using System.IO.Abstractions;
using TaskBasedUpdater.Verification;

namespace TaskBasedUpdater.Component
{
    public interface IProductComponentBuilder
    {
        HashType HashType { get; }
        
        Version? GetVersion(IFileInfo file);
    }
}