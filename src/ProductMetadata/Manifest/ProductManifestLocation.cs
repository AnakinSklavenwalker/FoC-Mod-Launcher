using System;
using Validation;

namespace ProductMetadata.Manifest
{
    public sealed class ProductManifestLocation
    {
        public Uri UpdateManifestPath { get; }

        public IProductReference Product { get; }

        public ProductManifestLocation(IProductReference product, Uri updateManifestPath)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(updateManifestPath, nameof(updateManifestPath));
            Product = product;
            UpdateManifestPath = updateManifestPath;
        }
    }
}