﻿using System.Diagnostics;
using System.Windows.Navigation;

namespace FocLauncher
{
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void OpenLicenseSite(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
