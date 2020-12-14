using System;
using System.Collections.Generic;
using TaskBasedUpdater.Component;

namespace TaskBasedUpdater.Elevation
{
    public static class ElevationUtilities
    {
        public static IEnumerable<ProductComponent> AggregateItems(this ElevationRequireException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            foreach (var requestData in exception.Requests)
                yield return requestData.ProductComponent;
        }
    }
}