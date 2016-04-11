using System;
using System.Windows.Threading;
using AutoCheckIn.Forum;

namespace AutoCheckIn
{
    public class AutoSigner
    {
        public static AutoSigner Current { get; } = new AutoSigner();

        public DispatcherTimer DispatcherTimer { get; private set; }

        private AutoSigner()
        {
            DispatcherTimer = new DispatcherTimer {Interval = new TimeSpan(0, 1, 0, 0)};
            DispatcherTimer.Tick += DispatcherTimerOnTick;
            DispatcherTimer.Start();

            Logger.Log(LogType.Information, $"自动签到计时开始，下一次尝试签到在{DispatcherTimer.Interval}后");
        }

        private async void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            DispatcherTimer.Stop();
            var session = (App.Current as App).ViewModel.Session;

            if (session.State != SessionState.Alive)
            {
                Logger.Log(LogType.Information, $"由于用户未登录，本次签到中止");

                DispatcherTimer.Interval = new TimeSpan(0, 1, 0, 0);
                DispatcherTimer.Start();
                Logger.Log(LogType.Information, $"自动签到计时开始，下一次尝试签到在{DispatcherTimer.Interval}后");
                return;
            }

            var ready = await session.ReadyToSign();

            if (ready.Status != 0)
            {
                if (ready.Message == "您今天已经签到过了或者签到时间还未开始")
                {
                    var nextTry = (DateTime.Now.Date + TimeSpan.FromDays(1) + TimeSpan.FromHours(1)) - DateTime.Now;
                    DispatcherTimer.Interval = nextTry;
                    DispatcherTimer.Start();
                    Logger.Log(LogType.Information, $"自动签到计时开始，下一次尝试签到在{DispatcherTimer.Interval}后");
                    return;
                }

                DispatcherTimer.Interval = new TimeSpan(0, 1, 0, 0);
                DispatcherTimer.Start();
                Logger.Log(LogType.Information, $"自动签到计时开始，下一次尝试签到在{DispatcherTimer.Interval}后");
                return;
            }

            String content = "无聊...";
            SignMood signMood = SignMood.Boring;

            if ((App.Current as App).ViewModel.AutofillList.Count > 0)
            {
                Random ran = new Random();

                var s = (App.Current as App).ViewModel.AutofillList[ran.Next((App.Current as App).ViewModel.AutofillList.Count)];
                content = s.Content;
                signMood = (SignMood)s.SignMoodValue;
            }

            var result = await (App.Current as App).ViewModel.Session.Sign(signMood, content, ready.Values["formhash"]);

            if (result.Status == 0)
            {
                var nextTry = (DateTime.Now.Date + TimeSpan.FromDays(1) + TimeSpan.FromHours(1)) - DateTime.Now;
                DispatcherTimer.Interval = nextTry;
                DispatcherTimer.Start();
                Logger.Log(LogType.Information, $"自动签到计时开始，下一次尝试签到在{DispatcherTimer.Interval}后");
                return;
            }

            DispatcherTimer.Interval = new TimeSpan(0, 1, 0, 0);
            DispatcherTimer.Start();
            Logger.Log(LogType.Information, $"自动签到计时开始，下一次尝试签到在{DispatcherTimer.Interval}后");
            return;
        }
        
    }
}
