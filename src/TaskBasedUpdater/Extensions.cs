using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskBasedUpdater.New.Product.Component;

namespace TaskBasedUpdater
{
    public static class Extensions
    {
        public static bool IsExceptionType<T>(this Exception error) where T : Exception
        {
            switch (error)
            {
                case T _:
                    return true;
                case AggregateException aggregateException:
                    return aggregateException.InnerExceptions.Any(p => p.IsExceptionType<T>());
                default:
                    return false;
            }
        }

        public static Exception? TryGetWrappedException(this Exception exception)
        {
            var wrappedExceptions = exception.TryGetWrappedExceptions();
            return wrappedExceptions != null && wrappedExceptions.Count == 1 ? wrappedExceptions.Single() : null;
        }

        public static IReadOnlyCollection<Exception>? TryGetWrappedExceptions(
            this Exception exception)
        {
            return exception is AggregateException aggregateException ? aggregateException.Flatten().InnerExceptions : null;
        }

        public static bool IsSuccess(this InstallResult result)
        {
            return result == InstallResult.Success || result == InstallResult.SuccessRestartRequired;
        }

        public static bool IsFailure(this InstallResult result)
        {
            return result == InstallResult.Failure || result == InstallResult.FailureException;
        }

        internal static string GetFailureSignature(this ProductComponent component, string action, string result)
        {
            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(component.Name)) 
                stringBuilder.Append("Item=" + component.Name);
            if (!string.IsNullOrEmpty(action))
            {
                if (stringBuilder.Length > 0) 
                    stringBuilder.Append(';');
                stringBuilder.Append("Action=" + action);
            }
            if (!string.IsNullOrEmpty(result))
            {
                if (stringBuilder.Length > 0) 
                    stringBuilder.Append(';');
                stringBuilder.Append("Result=" + result);
            }
            return stringBuilder.ToString();
        }

        public static string? TryJoin(this IEnumerable<string>? source, string separator)
        {
            return source == null ? null : string.Join(separator, source);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
        {
            return source == null || !FastAny<T>(source);
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
        
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return source.GroupBy(keySelector, comparer).Select(x => x.First());
        }

        private static bool FastAny<T>(IEnumerable<T> source)
        {
            switch (source)
            {
                case ICollection<T> collection:
                    return (uint) collection.Count > 0U;
                case ICollection collection:
                    return (uint)collection.Count > 0U;
                default:
                    using (var enumerator = source.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                            return true;
                    }
                    return false;
            }
        }
    }
}
