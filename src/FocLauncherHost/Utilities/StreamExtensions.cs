using System.IO;
using System.Text;

namespace FocLauncherHost.Utilities
{
    internal static class StreamExtensions
    {
        internal static Stream ToStream(this string input)
        {
            return input.ToStream(Encoding.UTF8);
        }

        internal static Stream ToStream(this string input, Encoding encoding)
        {
            var e = encoding.GetBytes(input);
            return new MemoryStream(e);
        }
    }
}
