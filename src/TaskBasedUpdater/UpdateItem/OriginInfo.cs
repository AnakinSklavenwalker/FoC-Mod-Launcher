using System;

namespace TaskBasedUpdater.Component
{
    public class OriginInfo
    {
        public Uri Origin { get; }

        public Version? Version { get; }

        public long? Size { get; }

        public ValidationContext? ValidationContext { get; }

        public OriginInfo(Uri origin, Version? version = null, long? size = null, ValidationContext? validationContext = null)
        {
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
            Version = version;
            Size = size;
            ValidationContext = validationContext;
        }
    }
}