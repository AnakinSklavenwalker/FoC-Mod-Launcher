using System;
using System.Runtime.InteropServices;
using ProductUpdater.Elevation;

namespace ProductUpdater.NativeMethods
{
    internal static class Advapi32
    {
        internal enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups = 2,
            TokenPrivileges = 3,
            TokenOwner = 4,
            TokenPrimaryGroup = 5,
            TokenDefaultDacl = 6,
            TokenSource = 7,
            TokenType = 8,
            TokenImpersonationLevel = 9,
            TokenStatistics = 10, // 0x0000000A
            TokenRestrictedSids = 11, // 0x0000000B
            TokenSessionId = 12, // 0x0000000C
            TokenGroupsAndPrivileges = 13, // 0x0000000D
            TokenSessionReference = 14, // 0x0000000E
            TokenSandBoxInert = 15, // 0x0000000F
            TokenAuditPolicy = 16, // 0x00000010
            TokenOrigin = 17, // 0x00000011
            TokenElevationType = 18, // 0x00000012
            TokenLinkedToken = 19, // 0x00000013
            TokenElevation = 20, // 0x00000014
            TokenHasRestrictions = 21, // 0x00000015
            TokenAccessInformation = 22, // 0x00000016
            TokenVirtualizationAllowed = 23, // 0x00000017
            TokenVirtualizationEnabled = 24, // 0x00000018
            TokenIntegrityLevel = 25, // 0x00000019
            TokenUIAccess = 26, // 0x0000001A
            TokenMandatoryPolicy = 27, // 0x0000001B
            TokenLogonSid = 28, // 0x0000001C
            TokenIsAppContainer = 29, // 0x0000001D
            TokenCapabilities = 30, // 0x0000001E
            TokenAppContainerSid = 31, // 0x0000001F
            TokenAppContainerNumber = 32, // 0x00000020
            TokenUserClaimAttributes = 33, // 0x00000021
            TokenDeviceClaimAttributes = 34, // 0x00000022
            TokenRestrictedUserClaimAttributes = 35, // 0x00000023
            TokenRestrictedDeviceClaimAttributes = 36, // 0x00000024
            TokenDeviceGroups = 37, // 0x00000025
            TokenRestrictedDeviceGroups = 38, // 0x00000026
            TokenSecurityAttributes = 39, // 0x00000027
            TokenIsRestricted = 40, // 0x00000028
            MaxTokenInfoClass = 41, // 0x00000029
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern IntPtr GetSidSubAuthority(IntPtr pSid, uint nSubAuthority);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool OpenProcessToken(SafeHandle processHandle, int desiredAccess, out SafeTokenHandle tokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetTokenInformation(SafeHandle tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength);
    }
}
