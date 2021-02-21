using System.Diagnostics;
using System.Threading;
using FocLauncherHost.Dialogs;

namespace FocLauncherHost
{
    internal sealed class LauncherRestartManager
    {
        internal static bool ShowElevateDialog()
        {
            var window = new RestartElevatedWindow();
            window.ShowDialog();
            return window.ElevateResult;
        }

        //internal static bool ShowProcessKillDialog(ILockingProcessManager processManager, CancellationToken token)
        //{
        //    return ShowKillDialogCore(processManager, true, false, token);
        //}

        //internal static bool ShowSelfKillDialog(ILockingProcessManager processManager, CancellationToken token)
        //{
        //    return ShowKillDialogCore(processManager, false, true, token);
        //}

        internal static bool ShowRestoreDialog(bool requiresRestore)
        {
            var dialog = new RestoreDialog(requiresRestore);
            dialog.ShowDialog();
            return dialog.RestoreNow;
        }

        //private static bool ShowKillDialogCore(ILockingProcessManager processManager, bool retry, bool showSelf, CancellationToken token)
        //{
        //    var currentProcessId = Process.GetCurrentProcess().Id;
        //    LockedFilesDialog.ProcessKillResult result;
        //    do
        //    {
        //        token.ThrowIfCancellationRequested();
        //        var processes = processManager.GetProcesses().Where(x => x.ApplicationStatus == ApplicationStatus.Running).ToList();
        //        if (!showSelf) 
        //            processes = processes.Where(x => x.Id != currentProcessId).ToList();
        //        if (!processes.Any())
        //            return true;
        //        var dialog = new LockedFilesDialog(processes, retry);
        //        dialog.ShowDialog();
        //        result = dialog.Result;
        //    } while (result == LockedFilesDialog.ProcessKillResult.Retry);

        //    return result == LockedFilesDialog.ProcessKillResult.Kill;
        //}
    }
}