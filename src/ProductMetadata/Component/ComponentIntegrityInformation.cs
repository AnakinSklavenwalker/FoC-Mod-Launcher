using System;
using CommonUtilities;
using Validation;

namespace ProductMetadata.Component
{
    public readonly struct ComponentIntegrityInformation
    {
        public static readonly ComponentIntegrityInformation None = new(Array.Empty<byte>(), HashType.None);

        public byte[] Hash { get; }

        public HashType HashType { get; }

        public ComponentIntegrityInformation(byte[] hash, HashType hashType)
        {
            Requires.NotNull(hash, nameof(hash));
            Hash = hash;
            HashType = hashType;
        }
    }
}