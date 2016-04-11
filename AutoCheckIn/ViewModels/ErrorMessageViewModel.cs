// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: ErrorMessageViewModel.cs
// Version: 20160411

using System;

namespace AutoCheckIn.ViewModels
{
    public class ErrorMessageViewModel : NotifyPropertyObject
    {
        private string _content;
        private string _title;

        public String Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public String Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }
    }
}