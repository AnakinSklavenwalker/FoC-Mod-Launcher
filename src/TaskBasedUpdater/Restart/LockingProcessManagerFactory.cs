using System.Text;
using TaskBasedUpdater.NativeMethods;

namespace TaskBasedUpdater.Restart
{
    public static class LockingProcessManagerFactory
    {
        public static ILockingProcessManager Create()
        {
            var strSessionKey = new StringBuilder(32);
            LockingProcessManager.ThrowOnError(RestartMgr.RmStartSession(out var pSessionHandle, 0, strSessionKey));
            return new LockingProcessManager(pSessionHandle, strSessionKey.ToString());
        }
    }
}