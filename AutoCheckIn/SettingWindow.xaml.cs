// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: SettingWindow.xaml.cs
// Version: 20160411

using AutoCheckIn.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace AutoCheckIn
{
    /// <summary>
    ///     SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        private static SettingWindow _signle;

        private SettingWindow()
        {
            InitializeComponent();
        }

        public static SettingWindow ShowSettingWindow(ApplicationViewModel applicationViewModel)
        {
            if (_signle != null)
            {
                _signle.Activate();
                return _signle;
            }

            _signle = new SettingWindow
            {
                DataContext = applicationViewModel
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

    public class StringCollection : ObservableCollection<String>
    {
    }
}