using System;

namespace ProductMetadata.Component
{
    public sealed record OriginInfo(Uri Origin)
    {
        public long? Size { get; init; }

        public ComponentIntegrityInformation IntegrityInformation { get; init; }
    }
}