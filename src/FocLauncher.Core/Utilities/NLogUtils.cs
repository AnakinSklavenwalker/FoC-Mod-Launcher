namespace FocLauncher.Utilities
{
    public static class NLogUtils
    {
        public static void SetLoggingForAppDomain()
        {
            //var config = new NLog.Config.LoggingConfiguration();
            //var logFilePath = LauncherConstants.LogFilePath;
            //var logfile = new NLog.Targets.FileTarget("logfile")
            //{
            //    FileName = logFilePath,
            //    Layout = new LayoutWithHeaderAndFooter
            //    {
            //        Header = new SimpleLayout($"/**************** {LauncherConstants.ProductName} by {LauncherConstants.Author} ****************/"),
            //        Layout = new SimpleLayout("${longdate}|${level:uppercase=true}|${logger}|${message}")
            //    }
            //};
            //config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            //LogManager.Configuration = config;
        }

        public static void DeleteOldLogFile()
        {
            //var logFilePath = LauncherConstants.LogFilePath;
            //try
            //{
            //    if (File.Exists(logFilePath))
            //        File.Delete(logFilePath);
            //}
            //catch (IOException)
            //{
            //}
        }
    }
}
