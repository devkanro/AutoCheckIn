// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: ErrorMessageDialog.xaml.cs
// Version: 20160411

using AutoCheckIn.ViewModels;
using System;
using System.Windows.Controls;

namespace AutoCheckIn
{
    /// <summary>
    ///     LoginFailDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorMessageDialog : UserControl
    {
        public ErrorMessageDialog(String title, String content)
            : this(new ErrorMessageViewModel {Title = title, Content = content})
        {
        }

        public ErrorMessageDialog(ErrorMessageViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}