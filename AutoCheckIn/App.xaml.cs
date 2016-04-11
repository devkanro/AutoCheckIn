// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: App.xaml.cs
// Version: 20160411

using AutoCheckIn.Properties;
using AutoCheckIn.ViewModels;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using AutoCheckIn.Forum;
using AutoCheckIn.Net;
using Application = System.Windows.Application;

namespace AutoCheckIn
{
    /// <summary>
    ///     App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private FloatingPopup _contextMenuHost;
        private FloatingPopup _signLogHost;

        public App()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            Logger.Log(LogType.Exception, dispatcherUnhandledExceptionEventArgs.Exception.ToString());
        }

        public ApplicationViewModel ViewModel { get; private set; }

        public NotifyIcon NotifyIcon { get; private set; }

        public AutoSigner Signer { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Settings.Default.Reload();
            ViewModel = await ApplicationViewModel.LoadViewModel();
            Signer = AutoSigner.Current;

            BuildSignLogHost();
            BuildContextMenuHost();
            BuildNotifyIcon();

            var update =
                JsonConvert.DeserializeObject<UpdateInfomation>(
                    (await HttpRequest.Create("http://higan.me/autocheckin.json").Get().Wait()).GetDataAsString());

            if (update?.Version != null && update.Version != AboutWindowViewModel.VersionString)
            {
                NotifyIcon.ShowBalloonTip(3000, "有新更新", "程序有新更新，请在关于页面检查更新下载。", ToolTipIcon.Info);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SaveSetting();
            NotifyIcon.Visible = false;
            NotifyIcon.Dispose();
            base.OnExit(e);
        }

        public void SaveSetting()
        {
            if (ViewModel.Session.State == SessionState.Alive)
            {
                UserSignLog signLog = JsonConvert.DeserializeObject<UserSignLog>(Settings.Default.SignLog) ??
                                      new UserSignLog();

                if (signLog.ContainsKey(ViewModel.Session.User.UID))
                {
                    signLog[ViewModel.Session.User.UID] = ViewModel.CheckInDates;
                }
                else
                {
                    signLog.Add(ViewModel.Session.User.UID, ViewModel.CheckInDates);
                }

                Settings.Default.SignLog = JsonConvert.SerializeObject(signLog);
            }
            Settings.Default.Cookies = JsonConvert.SerializeObject(ViewModel.Session.Cookies);
            Settings.Default.AutoFillData = JsonConvert.SerializeObject(ViewModel.AutofillList);
            Settings.Default.Test = "This is a string.";
            Settings.Default.Save();
        }

        private void BuildSignLogHost()
        {
            var log = new SignLog
            {
                DataContext = ViewModel
            };
            _signLogHost = new FloatingPopup(log);
        }

        private void BuildContextMenuHost()
        {
            var contextMenu = new AppContextMenu
            {
                DataContext = ViewModel
            };
            _contextMenuHost = new FloatingPopup(contextMenu);
        }

        private void BuildNotifyIcon()
        {
            NotifyIcon = new NotifyIcon();
            var scriptStream = GetResourceStream(new Uri("/Assets/Icon.ico", UriKind.Relative));
            NotifyIcon.Icon = new Icon(scriptStream.Stream);
            NotifyIcon.Visible = true;

            NotifyIcon.MouseClick += NotifyIconOnMouseClick;
        }

        private async void NotifyIconOnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            WebBrowser wb = new WebBrowser();

            switch (mouseEventArgs.Button)
            {
                case MouseButtons.Left:
                    await _signLogHost.Show();
                    break;

                case MouseButtons.Right:
                    var result = await _contextMenuHost.Show();
                    if (result is AppContextMenuAction)
                    {
                        switch ((AppContextMenuAction)result)
                        {
                            case AppContextMenuAction.Login:
                                LoginWindow.ShowLoginWindow(ViewModel);
                                break;

                            case AppContextMenuAction.Sign:
                                SignWindow.ShowSignWindow(ViewModel);
                                break;

                            case AppContextMenuAction.Logout:
                                SaveSetting();
                                ViewModel.Session.Logout();
                                ViewModel.Name = null;
                                ViewModel.AvatarUrl = null;
                                break;

                            case AppContextMenuAction.Setting:
                                SettingWindow.ShowSettingWindow(ViewModel);
                                break;

                            case AppContextMenuAction.AppLog:
                                LogWindow.ShowLogWindow();
                                break;

                            case AppContextMenuAction.About:
                                AboutWindow.ShowAboutWindow();
                                break;

                            case AppContextMenuAction.Close:
                                Shutdown();
                                break;

                            default:
                                break;
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }
}