using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using TaskBasedUpdater.Component;
using Validation;

namespace TaskBasedUpdater.FileSystem
{
    public class FileHasher
    {
        // TODO: split-projects
        // Remove this overload
        public byte[] GetFileHash(string file, HashType hashType)
        {
            return GetFileHash(new PhysicalFileInfo(new FileInfo(file)), hashType);
        }


        public byte[] GetFileHash(IFileInfo file, HashType hashType)
        {
            Requires.NotNull(file, nameof(file));
            if (!file.Exists)
                throw new FileNotFoundException(nameof(file));
            if (file.IsDirectory)
                throw new InvalidOperationException("Unable to compute hash from a directory.");
            return HashFileInternal(file, GetAlgorithm(hashType));
        }

        private static byte[] HashFileInternal(IFileInfo file, HashAlgorithm algorithm)
        {
            using (algorithm)
            {
                using var fs = file.CreateReadStream();
                fs.Position = 0;
                return algorithm.ComputeHash(fs);
            }
        }

        private static HashAlgorithm GetAlgorithm(HashType hashType)
        {
            switch (hashType)
            {
                case HashType.MD5:
                    return HashAlgorithm.Create(HashAlgorithmName.MD5.Name!)!;
                case HashType.Sha1:
                    return HashAlgorithm.Create(HashAlgorithmName.SHA1.Name!)!;
                case HashType.Sha256:
                    return HashAlgorithm.Create(HashAlgorithmName.SHA256.Name!)!;
                case HashType.Sha512:
                    return HashAlgorithm.Create(HashAlgorithmName.SHA512.Name!)!;
                default:
                    throw new InvalidOperationException("Unable to find a hashing algorithm");
            }
        }

#if NET5_0
        public Task<byte[]> HashFileAsync(IFileInfo file, HashType hashType)
        {
            Requires.NotNull(file, nameof(file));
            if (!file.Exists)
                throw new FileNotFoundException(nameof(file));
            if (file.IsDirectory)
                throw new InvalidOperationException("Unable to compute hash from a directory.");
            return HashFileInternalAsync(file, GetAlgorithm(hashType));
        }

        private static Task<byte[]> HashFileInternalAsync(IFileInfo file, HashAlgorithm algorithm)
        {
            using (algorithm)
            {
                using var fs = file.CreateReadStream();
                fs.Position = 0;
                return algorithm.ComputeHashAsync(fs);
            }
        }
#endif
    }
}