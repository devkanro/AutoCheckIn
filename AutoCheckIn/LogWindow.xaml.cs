using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoCheckIn
{
    /// <summary>
    /// LogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LogWindow : Window
    {
        private static LogWindow _signle;

        public static void Append(String log)
        {
            if (_signle != null)
            {
                _signle.Dispatcher.InvokeAsync(() =>
                {
                    _signle.Log.AppendText(log);
                });
            }
        }

        private LogWindow()
        {
            InitializeComponent();
            Log.Text = File.ReadAllText(Logger.LogFile);
        }

        public static LogWindow ShowLogWindow()
        {
            if (_signle != null)
            {
                _signle.Activate();
                return _signle;
            }

            _signle = new LogWindow();
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
