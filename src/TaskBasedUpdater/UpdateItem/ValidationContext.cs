using System;

namespace TaskBasedUpdater.Component
{
    public class ValidationContext
    {
        public byte[] Hash { get; set; }

        public HashType HashType { get; set; }

        public bool Verify()
        {
            var hashLength = Hash.Length;
            var expected = GetHashSizeBytes(HashType);
            return hashLength.CompareTo(expected) == 0;
        }

        private static byte GetHashSizeBytes(HashType hashType)
        {
            switch (hashType)
            {
                case HashType.MD5:
                    return 16;
                case HashType.Sha1:
                    return 20;
                case HashType.Sha256:
                    return 32;
                case HashType.Sha384:
                    return 48;
                case HashType.Sha512:
                    return 64;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hashType), hashType, null);
            }
        }
    }
}