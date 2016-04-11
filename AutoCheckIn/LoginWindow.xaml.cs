// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: LoginWindow.xaml.cs
// Version: 20160411

using AutoCheckIn.ViewModels;
using System;
using System.Windows;

namespace AutoCheckIn
{
    /// <summary>
    ///     LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static LoginWindow _signle;

        private LoginWindow()
        {
            InitializeComponent();
        }

        public static LoginWindow ShowLoginWindow(ApplicationViewModel applicationViewModel)
        {
            if (_signle != null)
            {
                _signle.Activate();
                return _signle;
            }

            _signle = new LoginWindow
            {
                DataContext = new LoginWindowViewModel(applicationViewModel)
            };
            _signle.Show();
            return _signle;
        }

        protected override void OnClosed(EventArgs e)
        {
            _signle = null;
            base.OnClosed(e);
        }
    }
}