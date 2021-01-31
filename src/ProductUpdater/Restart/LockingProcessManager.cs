using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Logging;
using ProductUpdater.NativeMethods;

namespace ProductUpdater.Restart
{
    internal class LockingProcessManager : ILockingProcessManager
    {
        private static readonly ILogger? Logger;

        private int _sessionId;
        private string _sessionKey;
        private bool _registered;

        public bool IsDisposed { get; private set; }

        internal LockingProcessManager(int sessionId, string sessionKey)
        {
            _sessionId = sessionId;
            _sessionKey = sessionKey;
        }

        ~LockingProcessManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        public void Restart()
        {
            if (!_registered)
                return;
            ThrowOnError(RestartMgr.RmRestart(_sessionId, 0, null));
        }

        public void Shutdown(WindowsRestartManagerShutdown action = WindowsRestartManagerShutdown.ForceShutdown)
        {
            if (!_registered)
                return;
            ThrowOnError(RestartMgr.RmShutdown(_sessionId, action, null));
        }


        public static IEnumerable<ILockingProcessInfo> GetProcesses(IEnumerable<string> paths)
        {
            IEnumerable<ILockingProcessInfo>? source = null;
            try
            {
                using var manager = LockingProcessManagerFactory.Create();
                manager.Register(paths);
                source = manager.GetProcesses();
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to query processes with open handles (0x{ex.HResult:x8}): {ex.Message}");
            }

            if (source != null)
                return source.Where(process => process.ApplicationStatus != ApplicationStatus.Stopped && process.ApplicationStatus != ApplicationStatus.StoppedOther).ToArray();

            return Enumerable.Empty<ILockingProcessInfo>();
        }

        public void Register(IEnumerable<string>? files = null, IEnumerable<ILockingProcessInfo>? processes = null)
        {
            var fileNames = files?.ToArray();
            var fileCount = fileNames?.Length ?? 0;
            var processArray = processes?.ToArray();
            var processCount = processArray?.Length ?? 0;
            if (fileCount == 0 && processCount == 0)
                return;
            _registered = true;
            var rgProcesses = processArray?.Select(Convert).ToArray();
            ThrowOnError(RestartMgr.RmRegisterResources(_sessionId, fileCount, fileNames, processCount, rgProcesses, 0, null));
        }

        public IEnumerable<ILockingProcessInfo> GetProcesses()
        {
            if (_registered)
            {
                var num = 0;
                var pnProcInfo = 0;
                RestartMgr.RmProcessInfo[] array = null;
                do
                {
                    num = RestartMgr.RmGetList(_sessionId, out var pnProcInfoNeeded, ref pnProcInfo, array, out var _);
                    switch (num)
                    {
                        case 234:
                            pnProcInfo = pnProcInfoNeeded;
                            array = new RestartMgr.RmProcessInfo[pnProcInfo];
                            continue;
                        default:
                            ThrowOnError(num);
                            continue;
                        case 0:
                            break;
                    }
                    break;
                }
                while (num == 234);
                if (array != null && array.Length != 0)
                    return array.Select(process => new LockingProcessInfo(process)).ToArray();
            }

            return Enumerable.Empty<ILockingProcessInfo>();
        }

        internal static void ThrowOnError(int result)
        {
            if (result != 0)
                throw new WindowsException(result);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            var result = RestartMgr.RmEndSession(_sessionId);
            IsDisposed = true;
            ThrowOnError(result);
        }

        private static RestartMgr.RmUniqueProcess Convert(ILockingProcessInfo process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));
            var fileTimeUtc = process.StartTime.ToFileTimeUtc();
            var filetime = new FILETIME()
            {
                dwHighDateTime = (int)(fileTimeUtc >> 32),
                dwLowDateTime = (int)(fileTimeUtc & (long)uint.MaxValue)
            };
            return new RestartMgr.RmUniqueProcess()
            {
                DwProcessId = process.Id,
                ProcessStartTime = filetime
            };
        }
    }
}