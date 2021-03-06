﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FocLauncher.Input;
using FocLauncher.Theming;

namespace FocLauncher.Dialogs
{
    public partial class ChangeThemeDialog : INotifyPropertyChanged
    {
        private ITheme _selectedTheme;
        public ICommand SubmitCommand => new UICommand(ApplyTheme, () => true);

        private readonly ThemeManager _themeManager;

        public ITheme SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (Equals(value, _selectedTheme))
                    return;
                _selectedTheme = value;
                OnPropertyChanged();
            }
        }

        private void ApplyTheme()
        {
            if (!SelectedTheme.Equals(_themeManager.Theme))
                _themeManager.Theme = SelectedTheme;
            Properties.Settings.Default.Save();
            Close();
        }

        public ChangeThemeDialog(Window owner) : this()
        {
            Owner = owner;
        }

        public ChangeThemeDialog()
        {
            _themeManager = ThemeManager.Instance;
            InitializeComponent();
            ListBox.Focus();
            SelectedTheme = _themeManager.Theme;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
            base.OnKeyDown(e);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
