using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using ProductUpdater.Restart;

namespace ProductUpdater.FileSystem
{
    internal class LockedFileLogger
    {
        private readonly ILogger? _logger;
        private static LockedFileLogger? _instance;

        public static LockedFileLogger Instance => _instance ??= new LockedFileLogger();

        private LockedFileLogger()
        {
        }

        public void Log(IEnumerable<string>? paths)
        {
            if (paths == null)
                return;
            var files = paths.Where(File.Exists).ToArray();
            if (files.Length != 0)
            {
                var processes = LockingProcessManager.GetProcesses(files).ToList();
                if (!processes.IsNullOrEmpty())
                    _logger?.LogWarning($"The following file(s) are locked: {files.TryJoin(", ")}, process(es): {processes.Select(Format).TryJoin(", ")}");
            }
            // TODO: Directories??
        }

        private static string? Format(ILockingProcessInfo? info)
        {
            if (info == null)
                return null;
            var str = info.Description;
            if (info.ApplicationType == ApplicationType.Service)
                str = info.ServiceName;
            else
            {
                var process = Process.GetProcessById(info.Id);
                if (process.StartTime == info.StartTime)
                    str = process.ProcessName;
            }
            return $"{str} ({info.Id})";
        }

    }
}