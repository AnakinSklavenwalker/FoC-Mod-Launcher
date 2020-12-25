namespace TaskBasedUpdater.Restart
{
    public interface IRestartOptions
    {
        int? Pid { get; set; }

        int Timeout { get; set; }

        string ExecutablePath { get; set; }

        string? LogFile { get; set; }

        string ToCommandLine();
    }
}