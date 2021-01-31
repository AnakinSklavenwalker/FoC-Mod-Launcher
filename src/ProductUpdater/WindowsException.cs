using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ProductUpdater
{
    internal class WindowsException : UpdaterException
    {
        private string _message;

        public int NativeErrorCode { get; }

        public override string Message
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _message, () => GetMessage(NativeErrorCode));
            }
        }

        public WindowsException(int error)
        {
            NativeErrorCode = error;
            HResult = error > 0 && error <= ushort.MaxValue ? -2147024896 | error : error;
        }

        private static string GetMessage(int error, bool englishOnly = false)
        {
            var languageId = englishOnly ? 1033 : 0;
            var stringBuilder = new StringBuilder(256);
            while (!TryGetMessage(error, ref languageId, stringBuilder))
            {
                if (stringBuilder.Capacity > 65535)
                    return $"Unknown error: {error}";
            }
            var text = stringBuilder.ToString().Trim();
            return string.IsNullOrEmpty(text) ? $"Unknown error: {error}" : text;
        }

        private static bool TryGetMessage(int error, ref int languageId, StringBuilder sb)
        {
            if (NativeMethods.Kernel32.FormatMessage(FormatMessage.FormatMessageIgnoreInserts | FormatMessage.FormatMessageFromSystem | FormatMessage.FormatMessageArgumentArray, IntPtr.Zero, error, languageId, sb, sb.Capacity + 1, IntPtr.Zero) == 0)
            {
                var lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error == 122)
                {
                    sb.Capacity *= 4;
                    return false;
                }
                if (languageId != 0)
                {
                    switch (lastWin32Error)
                    {
                        case 1813:
                            if ((languageId & 0xFC00) != 0)
                            {
                                languageId &= 1023;
                                return false;
                            }
                            break;
                        case 1815:
                        case 15100:
                        case 15105:
                            languageId = 0;
                            return false;
                    }
                }
                sb.Clear();
            }
            return true;
        }

        [Flags]
        internal enum FormatMessage
        {
            FormatMessageIgnoreInserts = 0x200,
            FormatMessageFromSystem = 0x1000,
            FormatMessageArgumentArray = 0x2000
        }
    }
}
