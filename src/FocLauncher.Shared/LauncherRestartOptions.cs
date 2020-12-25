using CommandLine;

namespace FocLauncher.Shared
{
    internal partial class LauncherRestartOptions
    {
        [Option('p', "pid", Required = false, HelpText = "The PID of the process to wait until terminated.")]
        public int? Pid { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "The maximum time in seconds to wait for the specified process to terminate.", Default = 10)]
        public int Timeout { get; set; }

        [Option('s', "startProcess", Required = true, HelpText = "The absolute path of the Foc Launcher.exe file.")]
        public string ExecutablePath { get; set; }

        [Option('l', "logfile", Required = false, HelpText = "The absolute path of the log file to use")]
        public string? LogFile { get; set; }

        [Option('u', "update", Group = "CommandType", Required = false, HelpText = "Tell the application to run the update procedure.")]
        public bool Update { get; set; }

        [Option('r', "restore", Group = "CommandType", Required = false, Default = false, HelpText = "When set the application will reset to it's initial state")]
        public bool Restore { get; set; }

        [Option('p', "payload", Required = false, HelpText = "Payload in base64 format which is required for the update")]
        public string Payload { get; set; }

        public string ToCommandLine()
        {
            return Parser.Default.FormatCommandLine(this, config => config.SkipDefault = true);
        }
    }
}