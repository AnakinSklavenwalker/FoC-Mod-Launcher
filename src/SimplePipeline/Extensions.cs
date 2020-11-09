using System;
using System.Collections.Generic;
using System.Linq;

namespace SimplePipeline
{
    internal static class Extensions
    {
        public static bool IsExceptionType<T>(this Exception error) where T : Exception
        {
            return error switch
            {
                T _ => true,
                AggregateException aggregateException => aggregateException.InnerExceptions.Any(p =>
                    p.IsExceptionType<T>()),
                _ => false
            };
        }

        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> items)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (items is null)
                throw new ArgumentNullException(nameof(items));
            foreach (var obj in items)
                source.Add(obj);
        }
    }
}
