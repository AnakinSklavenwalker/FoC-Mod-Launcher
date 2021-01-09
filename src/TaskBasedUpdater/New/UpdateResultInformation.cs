using System;
using System.Xml;
using TaskBasedUpdater.New.Product;
using Validation;

namespace TaskBasedUpdater.New
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