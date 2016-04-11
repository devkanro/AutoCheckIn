// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: ApplicationViewModel.cs
// Version: 20160411

using AutoCheckIn.Binding;
using AutoCheckIn.Forum;
using AutoCheckIn.Properties;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows;

namespace AutoCheckIn.ViewModels
{
    public class ApplicationViewModel : NotifyPropertyObject
    {
        private AutofillList _autofillList;
        private string _avatarUrl;
        private DatesCollection _checkInDates;
        private bool _checkSignResult;
        private DateTime _checkSignTime;
        private bool _isSignedToday;
        private string _name;
        private ServerSession _session;
        private UniversalCommand _showEditDialogCommand;

        private ApplicationViewModel()
        {
            ShowEditDialogCommand = new UniversalCommand(ShowEditDialogExecute);
        }

        public DatesCollection CheckInDates
        {
            get { return _checkInDates; }
            set
            {
                if (_checkInDates != null)
                {
                    _checkInDates.CollectionChanged -= CheckInDatesOnCollectionChanged;
                }
                _checkInDates = value;
                if (_checkInDates != null)
                {
                    _checkInDates.CollectionChanged += CheckInDatesOnCollectionChanged;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplaySignCountString));
            }
        }

        public ServerSession Session
        {
            get { return _session; }
            private set
            {
                _session = value;
                OnPropertyChanged();
            }
        }

        public AutofillList AutofillList
        {
            get { return _autofillList; }
            set
            {
                _autofillList = value;
                OnPropertyChanged();
            }
        }

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));

                OnPropertyChanged(nameof(DisplaySignCountString));
                OnPropertyChanged(nameof(LoginVisibility));
                OnPropertyChanged(nameof(LogoutVisibility));
            }
        }

        public String AvatarUrl
        {
            get { return _avatarUrl; }
            set
            {
                _avatarUrl = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayAvatarUrl));
            }
        }

        public DateTime CheckSignTime
        {
            get { return _checkSignTime; }
            set
            {
                _checkSignTime = value;
                OnPropertyChanged();
            }
        }

        public bool IsSignedToday
        {
            get
            {
                if (CheckSignTime.Date == DateTime.Today)
                {
                    return _isSignedToday;
                }
                return false;
            }
            set
            {
                _isSignedToday = value;
                if (value)
                {
                    CheckInDates.Add(DateTime.Today);
                }
                OnPropertyChanged();
            }
        }

        public String DisplayName => Name ?? "未登录";

        public String DisplayAvatarUrl
            => (AvatarUrl?.Contains("http://www.tsdm.net/uc_server/images/noavatar") ?? true) ? null : AvatarUrl;

        public String DisplaySignCountString => Name == null ? null : $"已经签到 {CheckInDates.Count} 天。";

        public Visibility LoginVisibility => Name == null ? Visibility.Collapsed : Visibility.Visible;

        public Visibility LogoutVisibility => Name == null ? Visibility.Visible : Visibility.Collapsed;

        public UniversalCommand ShowEditDialogCommand
        {
            get { return _showEditDialogCommand; }
            private set
            {
                _showEditDialogCommand = value;
                OnPropertyChanged();
            }
        }

        public static async Task<ApplicationViewModel> LoadViewModel()
        {
            var result = new ApplicationViewModel {Session = await ServerSession.LoadCurrentSession()};

            if ((result.Session.User?.Status ?? -1) == 0)
            {
                result.Name = result.Session.User.UserName;
                result.AvatarUrl = result.Session.User.Avatar;
                var signLog = JsonConvert.DeserializeObject<UserSignLog>(Settings.Default.SignLog);
                if (signLog != null && signLog.ContainsKey(result.Session.User.UID))
                {
                    result.CheckInDates = signLog[result.Session.User.UID];
                }
                else
                {
                    result.CheckInDates = new DatesCollection();
                }
            }
            else
            {
                result.CheckInDates = new DatesCollection();
            }

            result.AutofillList = JsonConvert.DeserializeObject<AutofillList>(Settings.Default.AutoFillData) ??
                                  new AutofillList
                                  {
                                      new AutofillDataViewModel
                                      {
                                          Content = "无聊...",
                                          SignMoodValue = (int) SignMood.Boring
                                      }
                                  };

            return result;
        }

        private void CheckInDatesOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            OnPropertyChanged(nameof(DisplaySignCountString));
        }

        private void ShowEditDialogExecute(object o)
        {
            var d = o as DialogHost;
            d.DialogContent = new AutofillDataViewModel {SignMoodValue = 0};
            d.IsOpen = true;
        }
    }
}