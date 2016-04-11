// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: AboutWindowViewModel.cs
// Version: 20160411

using AutoCheckIn.Binding;
using System;
using System.Diagnostics;
using AutoCheckIn.Net;
using Newtonsoft.Json;

namespace AutoCheckIn.ViewModels
{
    public class AboutWindowViewModel : NotifyPropertyObject
    {
        public static String VersionString { get; } = "v1.16.0412";

        private UniversalCommand _checkUpdateCommand;
        private string _version;

        public AboutWindowViewModel()
        {
            _version = VersionString;
            _checkUpdateCommand = new UniversalCommand(CheckUpdateExecute);
        }

        public String Version
        {
            get { return _version; }
            private set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        public UniversalCommand CheckUpdateCommand
        {
            get { return _checkUpdateCommand; }
            private set
            {
                _checkUpdateCommand = value;
                OnPropertyChanged();
            }
        }

        private async void CheckUpdateExecute(object o)
        {
            var update =
                JsonConvert.DeserializeObject<UpdateInfomation>(
                    (await HttpRequest.Create("http://higan.me/autocheckin.json").Get().Wait()).GetDataAsString());

            if (update?.Version != null && update.Version != Version)
            {
                var process = Process.Start("explorer.exe", update.DownloadUrl);

                Logger.Log(LogType.Information, "程序有新更新，请在关于页面检查更新下载");
            }
        }
    }

    public class UpdateInfomation
    {
        public String Version { get; set; }

        public String DownloadUrl { get; set; }
    }
}