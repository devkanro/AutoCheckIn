// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: AboutWindow.xaml.cs
// Version: 20160411

using AutoCheckIn.ViewModels;
using System;
using System.Windows;

namespace AutoCheckIn
{
    /// <summary>
    ///     AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        private static AboutWindow _signle;

        private AboutWindow()
        {
            InitializeComponent();
        }

        public static AboutWindow ShowAboutWindow()
        {
            if (_signle != null)
            {
                _signle.Activate();
                return _signle;
            }

            _signle = new AboutWindow
            {
                DataContext = new AboutWindowViewModel()
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