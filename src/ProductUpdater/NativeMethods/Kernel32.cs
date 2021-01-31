using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ProductUpdater.NativeMethods
{
    internal static class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "FormatMessageW", SetLastError = true)]
        internal static extern int FormatMessage(WindowsException.FormatMessage dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}
