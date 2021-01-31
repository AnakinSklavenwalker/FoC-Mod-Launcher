using System;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using Validation;

#if NET
using System.Threading.Tasks;
#endif

namespace CommonUtilities
{
    public class HashingService
    {
        public byte[] GetFileHash(IFileInfo file, HashType hashType)
        {
            Requires.NotNull(file, nameof(file));
            if (!file.Exists)
                throw new FileNotFoundException(nameof(file));

            using var stream = file.FileSystem.FileStream.Create(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            return GetStreamHash(stream, hashType, true);
        }

        public byte[] GetStreamHash(Stream stream, HashType hashType, bool keepOpen = false)
        {
            Requires.NotNull(stream, nameof(stream));
            return HashFileInternal(stream, GetAlgorithm(hashType), keepOpen);
        }

        private static byte[] HashFileInternal(Stream inputStream, HashAlgorithm algorithm, bool keepOpen)
        {
            if (!inputStream.CanRead)
                throw new InvalidOperationException("Cannot hash unreadable stream");
            inputStream.Position = 0;
            try
            {
                using (algorithm)
                    return algorithm.ComputeHash(inputStream);
            }
            finally
            {
                if (!keepOpen)
                    inputStream.Dispose();
            }
        }

        private static HashAlgorithm GetAlgorithm(HashType hashType)
        {
            return hashType switch
            {
                HashType.MD5 => MD5.Create(),
                HashType.Sha1 => SHA1.Create(),
                HashType.Sha256 => SHA256.Create(),
                HashType.Sha512 => SHA512.Create(),
                HashType.Sha384 => SHA384.Create(),
                _ => throw new InvalidOperationException("Unable to find a hashing algorithm")
            };
        }

#if NET
        public Task<byte[]> HashFileAsync(IFileInfo file, HashType hashType)
        {
            Requires.NotNull(file, nameof(file));
            if (!file.Exists)
                throw new FileNotFoundException(nameof(file));
            return HashFileInternalAsync(file, GetAlgorithm(hashType));
        }

        private static Task<byte[]> HashFileInternalAsync(IFileInfo file, HashAlgorithm algorithm)
        {
            using (algorithm)
            {
                using var fs = file.OpenRead();
                fs.Position = 0;
                return algorithm.ComputeHashAsync(fs);
            }
        }
#endif
    }
}