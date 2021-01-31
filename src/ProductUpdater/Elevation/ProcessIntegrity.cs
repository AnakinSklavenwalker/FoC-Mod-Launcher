using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ProductUpdater.NativeMethods;

namespace ProductUpdater.Elevation
{
    internal static class ProcessIntegrity
    {
        private const int SecurityMandatoryUntrustedRid = 0x00000000;
        private const int SecurityMandatoryLowRid = 0x00001000;
        private const int SecurityMandatoryMediumRid = 0x00002000;

        internal static bool CurrentIntegrityIsMediumOrLower()
        {
            if (!GetCurrentProcessIntegrity(out var integrityLevel))
                return false;

            switch (integrityLevel)
            {
                case SecurityMandatoryUntrustedRid:
                case SecurityMandatoryLowRid:
                case SecurityMandatoryMediumRid:
                    return true;
                default:
                    return false;
            }
        }

        private static bool GetCurrentProcessIntegrity(out int integrityLevel)
        {
            integrityLevel = 0;
            SafeTokenHandle? safeTokenHandle = null;
            var num = IntPtr.Zero;
            try
            {
                if (!Process.GetCurrentProcess().SafeHandle.OpenAccessToken(8, out safeTokenHandle))
                    return false;
                if (!safeTokenHandle.GetIntegrityLevelTokenSize(out var tokenSize))
                    return false;
                num = Marshal.AllocHGlobal((int) tokenSize);
                if (num == IntPtr.Zero)
                    return false;
                if (!safeTokenHandle.GetIntegrityLevelToken(tokenSize, num))
                    return false;

                integrityLevel = num.GetIntegrityLevel();
                return true;
            }
            finally
            {
                safeTokenHandle?.CloseTokenHandleIfOpened();
                if (num != IntPtr.Zero) 
                    Marshal.FreeHGlobal(num);
            }
        }

        private static int GetIntegrityLevel(this IntPtr integrityLevelTokenBuffer)
        {
            if (integrityLevelTokenBuffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(integrityLevelTokenBuffer));
            return Marshal.ReadInt32(Advapi32.GetSidSubAuthority(Marshal.PtrToStructure<TokenMandatoryLabel>(integrityLevelTokenBuffer).Label.Sid, 0U));
        }

        private static bool GetIntegrityLevelToken(this SafeTokenHandle accessTokenHandle, uint tokenBufferLength, IntPtr tokenBuffer)
        {
            if (accessTokenHandle == null)
                throw new ArgumentNullException(nameof(accessTokenHandle));
            if (tokenBuffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(tokenBuffer));
            return Advapi32.GetTokenInformation(accessTokenHandle, Advapi32.TokenInformationClass.TokenIntegrityLevel, tokenBuffer, tokenBufferLength, out _);
        }

        private static bool GetIntegrityLevelTokenSize(this SafeTokenHandle accessTokenHandle, out uint tokenSize)
        {
            if (accessTokenHandle == null)
                throw new ArgumentNullException(nameof(accessTokenHandle));
            var success = Advapi32.GetTokenInformation(accessTokenHandle, Advapi32.TokenInformationClass.TokenIntegrityLevel, IntPtr.Zero, 0U, out tokenSize);
            var lastWin32Error = Marshal.GetLastWin32Error();
            return !success && lastWin32Error == 122;
        }

        private static bool OpenAccessToken(this SafeHandle processHandle, int desiredAccess, out SafeTokenHandle tokenHandle)
        {
            return Advapi32.OpenProcessToken(processHandle, desiredAccess, out tokenHandle) && tokenHandle != null;
        }

        private static void CloseTokenHandleIfOpened(this SafeTokenHandle tokenHandle)
        {
            tokenHandle?.Dispose();
        }
    }
}