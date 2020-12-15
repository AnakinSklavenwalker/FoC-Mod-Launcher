using System;

namespace TaskBasedUpdater.Component
{
    public sealed record OriginInfo(Uri Origin)
    {
        public long? Size { get; init; }

        public ValidationContext? ValidationContext { get; init; }
    }
}