using System;
using TaskBasedUpdater.New.Product;

namespace TaskBasedUpdater.New.Update
{
    public sealed record UpdateRequest
    {
        public Uri UpdateManifestPath { get; init; }
        public IProductReference Product { get; init; }
    }
}