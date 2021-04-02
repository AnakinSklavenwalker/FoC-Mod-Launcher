using System;
using System.Diagnostics;
using System.Linq;

namespace PetroGlyph.Games.EawFoc.Client
{
    public static class ProcessHelper
    {
        public static Process? FindProcess(string name)
        {
            return Process.GetProcessesByName(name).FirstOrDefault();
        }

        public static Process? GetProcessByPid(int pid)
        {
            try
            {
                return Process.GetProcessById(pid);
            }
            catch
            {
                return null;
            }

        }

        public static bool IsProcessWithNameAlive(Process process)
        {
            try
            {
                return Process.GetProcesses().Any(x => x.ProcessName == process.ProcessName);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
