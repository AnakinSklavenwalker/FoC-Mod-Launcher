using System;
using System.Collections.Generic;
using TaskBasedUpdater.UpdateItem;

namespace TaskBasedUpdater.Elevation
{
    public static class ElevationUtilities
    {
        public static IEnumerable<IUpdateItem> AggregateItems(this ElevationRequireException exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            foreach (var requestData in exception.Requests)
                yield return requestData.UpdateItem;
        }
    }
}