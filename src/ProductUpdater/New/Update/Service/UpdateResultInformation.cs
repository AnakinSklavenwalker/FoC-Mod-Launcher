using System;
using ProductMetadata;
using Validation;

namespace ProductUpdater.New.Update.Service
{
    public record UpdateOperationResult
    {
        public IProductReference Product { get; }

        public UpdateResult Result { get; init; }

        public Exception? Error { get; init; }

        public UpdateOperationResult(IProductReference product)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }
    }
}