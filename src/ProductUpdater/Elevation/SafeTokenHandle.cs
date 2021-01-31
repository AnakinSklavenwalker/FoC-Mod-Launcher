using System;
using System.Runtime.InteropServices;
using ProductUpdater.NativeMethods;

namespace ProductUpdater.Elevation
{
    internal sealed class SafeTokenHandle : SafeHandle
    {
        public SafeTokenHandle()
            : base(IntPtr.Zero, true)
        {
            SetHandle(handle);
        }

        public override bool IsInvalid
        {
            get
            {
                if (!(handle == IntPtr.Zero))
                    return handle == new IntPtr(-1);
                return true;
            }
        }

        protected override bool ReleaseHandle()
        {
            return Kernel32.CloseHandle(handle);
        }
    }
}