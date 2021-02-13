using System;
using Validation;

namespace ProductMetadata.Manifest
{
    public sealed class ManifestLocation
    {
        public Uri ManifestUri { get; }

        public IProductReference Product { get; }

        public ManifestLocation(IProductReference product, Uri manifestUri)
        {
            Requires.NotNull(product, nameof(product));
            Requires.NotNull(manifestUri, nameof(manifestUri));
            Product = product;
            ManifestUri = manifestUri;
        }
    }
}