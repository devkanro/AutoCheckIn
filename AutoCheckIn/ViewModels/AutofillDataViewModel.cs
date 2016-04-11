// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: AutofillDataViewModel.cs
// Version: 20160411

using AutoCheckIn.Binding;
using AutoCheckIn.Forum;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Windows;

namespace AutoCheckIn.ViewModels
{
    public class AutofillDataViewModel : NotifyPropertyObject
    {
        [JsonIgnore] private UniversalCommand _addCommand;

        [JsonIgnore] private string _content;

        [JsonIgnore] private int _signMoodValue;

        public AutofillDataViewModel()
        {
            AddCommand = new UniversalCommand(AddExecute, CanAdd);
        }

        public int SignMoodValue
        {
            get { return _signMoodValue; }
            set
            {
                _signMoodValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SignMood));
                OnPropertyChanged(nameof(DisplaySignMood));
                AddCommand.OnCanExecuteChanged();
            }
        }

        public String Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
                AddCommand.OnCanExecuteChanged();
            }
        }

        [JsonIgnore]
        public SignMood SignMood => (SignMood) SignMoodValue;

        [JsonIgnore]
        public String DisplaySignMood
        {
            get
            {
                switch (SignMood)
                {
                    case SignMood.Happy:
                        return "开心";

                    case SignMood.Sad:
                        return "难过";

                    case SignMood.Depressed:
                        return "郁闷";

                    case SignMood.Boring:
                        return "无聊";

                    case SignMood.Anger:
                        return "怒";

                    case SignMood.Sweat:
                        return "擦汗";

                    case SignMood.Struggle:
                        return "奋斗";

                    case SignMood.Lazy:
                        return "慵懒";

                    case SignMood.Decline:
                        return "衰";
                }

                return "无效心情";
            }

            set
            {
                switch (value)
                {
                    case "开心":
                        SignMoodValue = 0;
                        break;

                    case "难过":
                        SignMoodValue = 1;
                        break;

                    case "郁闷":
                        SignMoodValue = 2;
                        break;

                    case "无聊":
                        SignMoodValue = 3;
                        break;

                    case "怒":
                        SignMoodValue = 4;
                        break;

                    case "擦汗":
                        SignMoodValue = 5;
                        break;

                    case "奋斗":
                        SignMoodValue = 6;
                        break;

                    case "慵懒":
                        SignMoodValue = 7;
                        break;

                    case "衰":
                        SignMoodValue = 8;
                        break;
                }
            }
        }

        public UniversalCommand AddCommand
        {
            get { return _addCommand; }
            private set
            {
                _addCommand = value;
                OnPropertyChanged();
            }
        }

        private void ShowEditDialogExecute(object o)
        {
            var d = o as DialogHost;
            d.DialogContent = new AutofillDataViewModel {SignMoodValue = 0};
            d.IsOpen = true;
        }

        private bool CanAdd(object o)
        {
            if (SignMoodValue < 0)
                return false;

            if (String.IsNullOrEmpty(Content))
                return false;

            return true;
        }

        private void AddExecute(object o)
        {
            var d = o as DialogHost;
            (Application.Current as App).ViewModel.AutofillList.Add(this);
            d.IsOpen = false;
        }
    }
}