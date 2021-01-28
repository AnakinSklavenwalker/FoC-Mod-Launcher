namespace SimpleDownloadManager.Engines
{
    public static class DownloadEngineNames
    {
        private const string FileEngineName = "File";
        private const string WebClientEngineName = "WebClient";

        public static string FileEngine => FileEngineName;
        public static string WebClientEngine => WebClientEngineName;
    }
}