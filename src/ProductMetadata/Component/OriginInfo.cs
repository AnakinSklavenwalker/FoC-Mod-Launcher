using System;

namespace ProductMetadata.Component
{
    public sealed record OriginInfo
    {
        public Uri Url { get; }

        public string FileName { get; }

        public long? Size { get; init; }

        public ComponentIntegrityInformation IntegrityInformation { get; init; }

        public OriginInfo(string fileName, Uri url)
        {
            FileName = fileName;
            Url = url;
        }
    }
}