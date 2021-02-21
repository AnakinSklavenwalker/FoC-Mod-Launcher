using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FocLauncherHost.Dialogs
{
    public partial class LockedFilesDialog : INotifyPropertyChanged
    {
        private string _buttonText;
        private bool _retry;
        private string _description;
        public event PropertyChangedEventHandler PropertyChanged;

        public ProcessKillResult Result { get; private set; }

        public string ButtonText
        {
            get => _buttonText;
            set
            {
                if (_buttonText == value)
                    return;
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description == value)
                    return;
                _description = value;
                OnPropertyChanged();
            }
        }

        public bool Retry
        {
            get => _retry;
            private set
            {
                if (value == _retry)
                    return;
                _retry = value;
                OnPropertyChanged();
            }
        }

        //public ObservableCollection<ILockingProcessInfo> Processes { get; }

        //public LockedFilesDialog(IEnumerable<ILockingProcessInfo> processes) : this(processes, true) 
        //{
        //}

        //public LockedFilesDialog(IEnumerable<ILockingProcessInfo> processes, bool retry)
        //{
        //    Retry = retry;
        //    Processes = new ObservableCollection<ILockingProcessInfo>(processes);
        //    ButtonText = Processes.Count == 1 ? "End Process" : "End Processes";
        //    InitializeComponent();

        //    if (retry)
        //    {
        //        Description = "Please end the following processes to continue the update.";
        //        ButtonText = Processes.Count == 1 ? "End Process" : "End Processes";
        //    }
        //    else if (Processes.Count == 1)
        //    {
        //        Description = "The launcher needs be restarted.";
        //        ButtonText = "Restart Launcher";
        //    }
        //    else
        //    {
        //        Description = "The launcher needs to be restarted. Other processes need to be ended.";
        //        ButtonText = "End and restart";
        //    }

        //}

        public override void ShowDialog()
        {
            HostWindow.Topmost = true;
            HostApplication.SplashVisibleResetEvent.WaitOne(TimeSpan.FromSeconds(2));
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
                HostWindow.Owner = Application.Current.MainWindow;
            base.ShowDialog();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnRetry(object sender, RoutedEventArgs e)
        {
            Result = ProcessKillResult.Retry;
            HostWindow.Close();
        }

        private void OnProceed(object sender, RoutedEventArgs e)
        {
            Result = ProcessKillResult.Kill;
            HostWindow.Close();
        }

        public enum ProcessKillResult
        {
            Abort,
            Kill,
            Retry
        }
    }
}
