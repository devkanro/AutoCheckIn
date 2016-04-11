// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: AppContextMenu.xaml.cs
// Version: 20160411

using System.Windows.Controls;

namespace AutoCheckIn
{
    /// <summary>
    ///     AppContextMenu.xaml 的交互逻辑
    /// </summary>
    public partial class AppContextMenu : UserControl
    {
        public AppContextMenu()
        {
            InitializeComponent();
        }
    }

    public enum AppContextMenuAction
    {
        Login,
        Sign,
        Logout,
        Setting,
        AppLog,
        About,
        Close
    }
}