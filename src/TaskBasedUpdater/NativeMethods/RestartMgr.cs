using System.Runtime.InteropServices;
using System.Text;
using TaskBasedUpdater.Restart;

namespace TaskBasedUpdater.NativeMethods
{
    internal static class RestartMgr
    {
        internal delegate void RmWriteStatusCallback([MarshalAs(UnmanagedType.U4)] int nPercentComplete);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct RmProcessInfo
        {
            internal RmUniqueProcess Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            internal string strServiceShortName;
            internal ApplicationType ApplicationType;
            internal ApplicationStatus AppStatus;
            internal int TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            internal bool bRestartable;
        }

        internal struct RmUniqueProcess
        {
            public int DwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }
        

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmStartSession(out int pSessionHandle, int dwSessionFlags, [Out] StringBuilder strSessionKey);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmRegisterResources(int dwSessionHandle, int nFiles, string[]? fileNames, 
            int nApplications, RmUniqueProcess[]? rgApplications, int nServices, string[]? rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmGetList(int dwSessionHandle, out int pnProcInfoNeeded, ref int pnProcInfo, [Out] RmProcessInfo[] rgAffectedApps, out int lpdwRebootReasons);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmShutdown(int dwSessionHandle, WindowsRestartManagerShutdown lActionFlags, RmWriteStatusCallback fnStatus);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmRestart(int dwSessionHandle, int dwRestartFlags, RmWriteStatusCallback fnStatus);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmEndSession(int dwSessionHandle);
    }
}