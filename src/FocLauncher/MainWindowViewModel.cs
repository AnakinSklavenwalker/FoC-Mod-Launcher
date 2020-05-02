﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FocLauncher.Game;
using FocLauncher.Input;
using FocLauncher.Mods;
using FocLauncher.Properties;
using FocLauncher.Theming;

namespace FocLauncher
{
    public class MainWindowViewModel : ILauncherWindowModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _windowed;
        private GameType _gameType;
        private bool _useDebugBuild;
        private bool _ignoreAsserts = true;
        private bool _noArtProcess = true;
        private LauncherSession _session;
        private IGame _foc;

        internal LauncherSession LauncherSession
        {
            get
            {
                if (_session == null){
                    _session = new LauncherSession(this, _foc);
                    _session.Started += OnGameStarted;
                    _session.StartFailed += OnGameStartFailed;
                }
                return _session;
            }
        }
        
        public ICommand LaunchCommand => new UICommand(ExecutedLaunch, CanExecute);
        
        public PetroglyphInitialization PetroglyphInitialization { get; }

        public ObservableCollection<LauncherListItemModel> GameObjects { get; } = new ObservableCollection<LauncherListItemModel>();
        
        public GameType GameType
        {
            get => _gameType;
            set
            {
                if (value == _gameType)
                    return;
                _gameType = value;
                OnPropertyChanged();
            }
        }

        public bool UseDebugBuild
        {
            get => _useDebugBuild;
            set
            {
                if (value == _useDebugBuild) return;
                _useDebugBuild = value;
                OnPropertyChanged();
            }
        }

        public bool IgnoreAsserts
        {
            get => _ignoreAsserts;
            set
            {
                if (value == _ignoreAsserts)
                    return;
                _ignoreAsserts = value;
                OnPropertyChanged();
            }
        }

        public bool NoArtProcess
        {
            get => _noArtProcess;
            set
            {
                if (value == _noArtProcess) return;
                _noArtProcess = value;
                OnPropertyChanged();
            }
        }

        public bool Windowed
        {
            get => _windowed;
            set
            {
                if (value == _windowed)
                    return;
                _windowed = value;
                OnPropertyChanged();
            }
        }
        
        public GameProcessWatcher GameProcessWatcher { get; }

        public MainWindowViewModel()
        {
            // TODO: Replace PetroglyphInitialization with GameManager
            var initialization = new PetroglyphInitialization();
            initialization.Initialize();

            PetroglyphInitialization = initialization;
            GameType = initialization.FocGameType;

            _foc = initialization.FoC;
            GameProcessWatcher = initialization.FoC.GameProcessWatcher;

            foreach (var gameObject in initialization.SearchGameObjects())
                GameObjects.Add(new LauncherListItemModel(gameObject, LauncherSession));
        }
        
        private void OnGameStartFailed(object sender, IPetroglyhGameableObject e)
        {
            MessageBox.Show($"Unable to start {e.Name}", "FoC Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnGameStarted(object sender, IPetroglyhGameableObject e)
        {
            if (Settings.Default.AutoSwitchTheme &&
                ThemeManager.Instance.TryGetThemeByMod(e as IMod, out var theme))
                ThemeManager.Instance.Theme = theme;
        }

        private void ExecutedLaunch(object obj)
        {
            if (obj is IPetroglyhGameableObject gameable)
                _session.Invoke(new[] { gameable });
        }

        private static bool CanExecute(object obj)
        {
            return obj is IPetroglyhGameableObject;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
