namespace ProductUpdater.Restart
{
    public interface IRestoreOptions : IRestartOptions
    {
        bool Restore { get; set; }
    }
}