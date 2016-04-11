// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: SignWindow.xaml.cs
// Version: 20160411

using AutoCheckIn.ViewModels;
using System;
using System.Windows;

namespace AutoCheckIn
{
    /// <summary>
    ///     SignWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SignWindow : Window
    {
        private static SignWindow _signle;

        private SignWindow()
        {
            InitializeComponent();
        }

        public static SignWindow ShowSignWindow(ApplicationViewModel applicationViewModel)
        {
            if (_signle != null)
            {
                _signle.Activate();
                return _signle;
            }

            _signle = new SignWindow
            {
                DataContext = new SignWindowViewModel(applicationViewModel)
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