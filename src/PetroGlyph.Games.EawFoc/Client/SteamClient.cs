using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PetroGlyph.Games.EawFoc.Games;

namespace PetroGlyph.Games.EawFoc.Client
{
    public class SteamClient
    {
        private readonly SteamRegistry _registry;
        private static SteamClient? _instance;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static SteamClient Instance => _instance ??= new SteamClient();

        internal FileInfo? SteamExe { get; private set; }

        public bool Installed => SteamExe != null && SteamExe.Exists;

        public bool IsRunning
        {
            get
            {
                var pid = ProcessId;
                if (!pid.HasValue)
                    return false;
                if (pid.Value == 0)
                    return false;
                return ProcessHelper.GetProcessByPid(pid.Value) != null;
            }
        }

        internal bool IsUserLoggedIn => ActiveUser.HasValue && ActiveUser.Value != 0;

        private int? ActiveUser
        {
            get
            {
                _registry.GetValue("ActiveUser", "ActiveProcess", out int? user);
                return user;
            }
            set => _registry.WriteValue("ActiveUser", "ActiveProcess", value);
        }

        private int? ProcessId
        {
            get
            {
                _registry.GetValue("pid", "ActiveProcess", out int? pid);
                return pid;
            }
        }

        private bool? WantsOfflineMode
        {
            get
            {
                if (!Installed)
                    return null;
                var steamDirectory = SteamExe.Directory;
                if (steamDirectory == null || !steamDirectory.Exists)
                    return null;

                var configFile = Path.Combine(steamDirectory.FullName, "config\\loginusers.vdf");
                if (!File.Exists(configFile))
                    return null;

                var config = VdfConvert.Deserialize(File.ReadAllText(configFile)).ToJson();
                var usersWantingOffline = config.SelectTokens("$..[?(@.WantsOfflineMode=='1')]").ToList();
                if (!usersWantingOffline.Any())
                    return false;

                var anyMostRecent = config.SelectTokens("$..[?(@.mostrecent)]");
                if (!anyMostRecent.Any())
                    return true;

                var mostRecent = usersWantingOffline.FirstOrDefault(x => x.SelectToken("$..[?(@.mostrecent=='1')]") != null);
                return !(mostRecent is null);
            }
        }

        private SteamClient()
        {
            _registry = new SteamRegistry();
            Initialize();
        }

        private void Initialize()
        {
            Logger.Trace("Initializing Steam Client");
            try
            {
                if (_registry.GetValue<string>("SteamExe", out var path))
                {
                    Logger.Trace($"Steam installed under location: '{path}'");
                    SteamExe = new FileInfo(path);
                }
                else
                    Logger.Warn("Steam is not installed");
            }
            catch (Exception)
            {
                Logger.Warn("Erro checking whether steam is installed");
            }
        }

        public bool IsGameInstalled(int gameId)
        {
            ThrowIfSteamNotInstalled();
            return _registry.HasPath($"Apps\\{gameId}");
        }

        public void StartGame(int gameId)
        {
            ThrowIfSteamNotInstalled();
            if (!IsGameInstalled(gameId))
                throw new SteamClientException($"The game with the ID: {gameId} does not exists");
            Process.Start($"steam://rungameid/{gameId}");
        }

        internal void StartSteam()
        {
            ThrowIfSteamNotInstalled();
            var process = new Process
            {
                StartInfo =
                {
                    FileName = SteamExe!.FullName,
                    UseShellExecute = false
                }
            };
            process.Start();
        }

        public async Task WaitSteamRunningAndLoggedInAsync(CancellationToken token = default)
        {
            ThrowIfSteamNotInstalled();

            var running = IsRunning;
            if (!running)
            {
                // Required because a taskmgr kill does not reset this value, so we have to do this manually
                ActiveUser = 0;
                await WaitSteamRunningAsync(token);
            }
            if (IsUserLoggedIn)
                return;

            // Fix Issue #1
            if (WantsOfflineMode.HasValue && WantsOfflineMode.Value)
            {
                await Task.Delay(2000, token);
                await WaitSteamProcessCountChangeAsync(token);
                if (!IsRunning)
                    throw new SteamClientException("Steam is not running anymore.");
            }
            else
            {
                await WaitSteamUserLoggedInAsync(token);
            }
        }

        private async Task WaitSteamRunningAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (IsRunning)
                return;
            
            while (!token.IsCancellationRequested && !IsRunning)
            {
                var k = _registry.GetKey("ActiveProcess");
                if (k is null)
                    return;
                await k.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
            }
        }

        public async Task WaitSteamUserLoggedInAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (IsUserLoggedIn)
                return;

            while (!token.IsCancellationRequested && !IsUserLoggedIn)
            {
                var k = _registry.GetKey("ActiveProcess");
                if (k is null)
                    return;
                await k.WaitForChangeAsync(false, RegistryChangeNotificationFilters.Value, token);
            }
        }

        protected void ThrowIfSteamNotInstalled()
        {
            if (!Installed)
                throw new SteamClientException("Steam is not installed!");
        }



        private async Task WaitSteamProcessCountChangeAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            var initialProcesses = GetSteamProcesses();
            var initialWindowHandle = GetSteamMainWindowHandle();
            while (!token.IsCancellationRequested || IsRunning)
            {
                var current = GetSteamProcesses();
                var currentHandle = GetSteamMainWindowHandle();
                try
                {
                    if (initialProcesses != current || initialWindowHandle != currentHandle)
                        return;
                    
                    await Task.Delay(750, token);
                }
                finally
                {
                    //foreach (var process in current) 
                    //    process.Dispose();
                }
            }
            //foreach (var initialProcess in initialProcesses) 
            //    initialProcess.Dispose();

            static int GetSteamProcesses() => Process.GetProcesses().Count(x => x.ProcessName.Contains("steam") || x.ProcessName.Contains("Steam"));

            static IntPtr GetSteamMainWindowHandle()
            {
                var p = Process.GetProcessesByName("steam").FirstOrDefault();
                try
                {
                    return p?.MainWindowHandle ?? IntPtr.Zero;
                }
                finally
                {
                    p?.Dispose();
                }
            }
                
        }


        private class SteamRegistry : RegistryHelper
        {
            protected override string RegistryBasePath => "Software\\Valve\\Steam";
            protected override RegistryKey RootRegistryKey => Registry.CurrentUser;
        }
    }
}