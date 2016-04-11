// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: SignWindowViewModel.cs
// Version: 20160411

using AutoCheckIn.Binding;
using AutoCheckIn.Forum;
using MaterialDesignThemes.Wpf;
using System;

namespace AutoCheckIn.ViewModels
{
    public class SignWindowViewModel : NotifyPropertyObject
    {
        private ApplicationViewModel _applicationViewModel;
        private UniversalCommand _autofillCommand;
        private UniversalCommand _cancelCommand;
        private string _formHash;
        private UniversalCommand _signCommand;
        private int _signMood;
        private string _todaySay;

        public SignWindowViewModel(ApplicationViewModel applicationViewModel)
        {
            _applicationViewModel = applicationViewModel;
            CancelCommand = new UniversalCommand(CancelExecute);
            SignCommand = new UniversalCommand(SignExecute, CanSign);
            AutofillCommand = new UniversalCommand(AutofillExecute);
        }

        public int SignMood
        {
            get { return _signMood; }
            set
            {
                _signMood = value;
                OnPropertyChanged();
                SignCommand.OnCanExecuteChanged();
            }
        }

        public String TodaySay
        {
            get { return _todaySay; }
            set
            {
                _todaySay = value;
                OnPropertyChanged();
                SignCommand.OnCanExecuteChanged();
            }
        }

        public String FormHash
        {
            get { return _formHash; }
            set
            {
                _formHash = value;
                OnPropertyChanged();
            }
        }

        public UniversalCommand AutofillCommand
        {
            get { return _autofillCommand; }
            set
            {
                _autofillCommand = value;
                OnPropertyChanged();
            }
        }

        public UniversalCommand SignCommand
        {
            get { return _signCommand; }
            set
            {
                _signCommand = value;
                OnPropertyChanged();
            }
        }

        public UniversalCommand CancelCommand
        {
            get { return _cancelCommand; }
            set
            {
                _cancelCommand = value;
                OnPropertyChanged();
            }
        }

        private void AutofillExecute(object o)
        {
            if (_applicationViewModel.AutofillList.Count <= 0)
                return;

            Random ran = new Random();

            var s = _applicationViewModel.AutofillList[ran.Next(_applicationViewModel.AutofillList.Count)];
            TodaySay = s.Content;
            SignMood = s.SignMoodValue;
        }

        private bool CanSign(object o)
        {
            if (SignMood < 0)
                return false;

            if (String.IsNullOrEmpty(TodaySay))
                return false;

            return true;
        }

        private async void SignExecute(object o)
        {
            var d = o as DialogHost;

            d.DialogContent = new WaitDialog {DataContext = "正在签到..."};
            d.IsOpen = true;

            var ready = await _applicationViewModel.Session.ReadyToSign();

            if (ready.Status != 0)
            {
                d.IsOpen = false;
                d.DialogContent = new ErrorMessageDialog("签到失败：", ready.Message);
                d.IsOpen = true;

                return;
            }

            FormHash = ready.Values["formhash"];

            var result = await _applicationViewModel.Session.Sign((SignMood) SignMood, TodaySay, FormHash);

            if (result.Status != 0)
            {
                d.IsOpen = false;
                d.DialogContent = new ErrorMessageDialog("签到失败：", ready.Message);
                d.IsOpen = true;

                return;
            }

            CancelCommand.Execute(d.Parent);
        }

        private void CancelExecute(object o)
        {
            (o as SignWindow)?.Close();
        }
    }
}