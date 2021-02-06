using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace SimpleDownloadManager.Verification
{
    public class HashVerifier : IVerifier
    {
        private readonly ILogger? _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IHashingService _hashingService;

        public HashVerifier(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger>();
            _fileSystem = serviceProvider.GetService<IFileSystem>() ?? new FileSystem();
            _hashingService = new HashingService();
        }

        public VerificationResult Verify(Stream file, VerificationContext verificationContext)
        {
            Requires.NotNull(file, nameof(file));
            if (file is not FileStream fileStream)
                throw new ArgumentException("The stream does not represent a file", nameof(file));
            string path = fileStream.Name;
            if (path is null || !_fileSystem.File.Exists(path))
                throw new InvalidOperationException("Cannot verify a non-existing file.");
            try
            {
                if (!verificationContext.Verify())
                    return VerificationResult.VerificationContextError;
                
                if (verificationContext.HashType == HashType.None)
                    return VerificationResult.Success;
                
                return CompareHashes(fileStream, verificationContext.HashType, verificationContext.Hash)
                    ? VerificationResult.Success
                    : VerificationResult.VerificationFailed;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, e.Message);
                return VerificationResult.Exception;
            }
        }

        private bool CompareHashes(Stream fileStream, HashType hashType, byte[] expected)
        {
            fileStream.Seek(0L, SeekOrigin.Begin);
            var actualHash = _hashingService.GetStreamHash(fileStream, hashType, true);
#if NET || NETSTANDARD2_1
            return actualHash.AsSpan().SequenceEqual(expected);
#else
            return actualHash.SequenceEqual(expected);
#endif
        }
    }
}
