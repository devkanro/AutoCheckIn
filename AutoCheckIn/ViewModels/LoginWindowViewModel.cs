// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: LoginWindowViewModel.cs
// Version: 20160411

using AutoCheckIn.Binding;
using AutoCheckIn.Forum;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows.Forms;
using System.Windows.Media;
using AutoCheckIn.Properties;
using Newtonsoft.Json;
using Application = System.Windows.Application;

namespace AutoCheckIn.ViewModels
{
    public class LoginWindowViewModel : NotifyPropertyObject
    {
        private string _accout;
        private int _accoutType;
        private ApplicationViewModel _applicationViewModel;
        private bool _isVerifyCodeEnable;
        private string _password;
        private string _verifyAnswer;
        private string _verifyCode;
        private ImageSource _verifyCodeImage;
        private int _verifyQuestionIndex;

        public LoginWindowViewModel(ApplicationViewModel applicationViewModel)
        {
            _applicationViewModel = applicationViewModel;
            CancelCommand = new UniversalCommand(CancelExecute);
            RefreshVerifyCodeCommand = new UniversalCommand(RefreshVerifyCode);
            LoginCommand = new UniversalCommand(LoginExecute, CanLogin);
            VerifyQuestionIndex = -1;
            RefreshVerifyCodeCommand.Execute(null);
        }

        public string Accout
        {
            get { return _accout; }
            set
            {
                _accout = value;
                OnPropertyChanged();
                LoginCommand.OnCanExecuteChanged();
            }
        }

        public int AccoutType
        {
            get { return _accoutType; }
            set
            {
                _accoutType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayAccoutType));
            }
        }

        public String DisplayAccoutType
        {
            get
            {
                switch (AccoutType)
                {
                    case 0:
                        return "账号";

                    case 1:
                        return "邮箱";

                    case 2:
                        return "UID";
                }
                return null;
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
                LoginCommand.OnCanExecuteChanged();
            }
        }

        public int VerifyQuestionIndex
        {
            get { return _verifyQuestionIndex; }
            set
            {
                _verifyQuestionIndex = value;
                OnPropertyChanged();
                LoginCommand.OnCanExecuteChanged();
            }
        }

        public String VerifyAnswer
        {
            get { return _verifyAnswer; }
            set
            {
                _verifyAnswer = value;
                OnPropertyChanged();
                LoginCommand.OnCanExecuteChanged();
            }
        }

        public ImageSource VerifyCodeImage
        {
            get { return _verifyCodeImage; }
            set
            {
                _verifyCodeImage = value;
                OnPropertyChanged();
            }
        }

        public String VerifyCode
        {
            get { return _verifyCode; }
            set
            {
                _verifyCode = value;
                OnPropertyChanged();
                LoginCommand.OnCanExecuteChanged();
            }
        }

        public UniversalCommand LoginCommand { get; }

        public UniversalCommand CancelCommand { get; }

        public UniversalCommand RefreshVerifyCodeCommand { get; }

        private bool CanLogin(object o)
        {
            if (String.IsNullOrEmpty(Accout))
                return false;

            if (String.IsNullOrEmpty(Password))
                return false;

            if (VerifyQuestionIndex > 0 && String.IsNullOrEmpty(VerifyAnswer))
                return false;

            if (String.IsNullOrEmpty(VerifyCode))
                return false;

            return true;
        }

        private async void LoginExecute(object o)
        {
            var d = o as DialogHost;

            d.DialogContent = new WaitDialog {DataContext = "正在登录..."};
            d.IsOpen = true;

            var result =
                await
                    (Application.Current as App).ViewModel.Session.Login((LoginMode) AccoutType, Accout, Password,
                        VerifyCode, VerifyQuestionIndex,
                        VerifyAnswer);

            d.IsOpen = false;
            if (result.Status == 0)
            {
                var info = await (Application.Current as App).ViewModel.Session.GetUserInformation();

                _applicationViewModel.Name = info.UserName;
                _applicationViewModel.AvatarUrl = info.Avatar;

                UserSignLog signLog = JsonConvert.DeserializeObject<UserSignLog>(Settings.Default.SignLog) ??
                                      new UserSignLog();
                if (signLog.ContainsKey(_applicationViewModel.Session.User.UID))
                {
                    _applicationViewModel.CheckInDates = signLog[_applicationViewModel.Session.User.UID];
                }
                else
                {
                    _applicationViewModel.CheckInDates = new DatesCollection();
                }

                CancelExecute(d.Parent);
                (Application.Current as App).NotifyIcon.ShowBalloonTip(3000, "登录成功",
                    $"欢迎回来，{result.Values["username"]}。", ToolTipIcon.Info);
            }
            else
            {
                switch (result.Message)
                {
                    case "login_strike":
                        d.DialogContent = new ErrorMessageDialog("登录失败：", "由于多次尝试登陆失败，请稍后再尝试。");
                        break;

                    case "login_invalid":
                        d.DialogContent = new ErrorMessageDialog("登录失败：", "账户密码有误，请核对后再尝试。");
                        break;

                    default:
                        d.DialogContent = new ErrorMessageDialog("登录失败：",
                            $"由于未知原因登录失败，这些信息可能为你提供了一些信息：\r\n{result.Message}。");
                        break;
                }
                d.IsOpen = true;
                RefreshVerifyCodeCommand.Execute(null);
                VerifyCode = "";
            }
        }

        private async void RefreshVerifyCode(object o)
        {
            var image = await (Application.Current as App).ViewModel.Session.ReadyToLogin();

            if (image != null)
            {
                VerifyCodeImage = (Application.Current as App).ViewModel.Session.VerifiyCode;
            }
        }

        private void CancelExecute(object o)
        {
            (o as LoginWindow)?.Close();
        }
    }
}