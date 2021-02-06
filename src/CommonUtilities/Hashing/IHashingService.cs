using System.IO;
using System.IO.Abstractions;

#if NET
using System.Threading.Tasks;
#endif


namespace CommonUtilities
{
    public interface IHashingService
    {
        byte[] GetFileHash(IFileInfo file, HashType hashType);

        byte[] GetStreamHash(Stream stream, HashType hashType, bool keepOpen = false);

#if NET
        Task<byte[]> HashFileAsync(IFileInfo file, HashType hashType);
#endif
    }
}