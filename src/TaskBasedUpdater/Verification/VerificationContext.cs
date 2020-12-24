using System;
using TaskBasedUpdater.Component;
using Validation;

namespace TaskBasedUpdater.Verification
{
    public readonly struct VerificationContext
    {
        public static readonly VerificationContext None = new(Array.Empty<byte>(), HashType.None);

        public byte[] Hash { get; }

        public HashType HashType { get; }

        public VerificationContext(byte[] hash, HashType hashType, bool verify = true)
        {
            Requires.NotNull(hash, nameof(hash));
            Hash = hash;
            HashType = hashType;
            if (verify && !Verify())
                throw new ArgumentException($"Supplied hash length does not match {hashType}");
        }
        
        internal bool Verify()
        {
            var hashLength = Hash.Length;
            return hashLength.CompareTo((byte) HashType) == 0;
        }
    }
}