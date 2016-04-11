// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: ForumClient.cs
// Version: 20160411

using AutoCheckIn.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Media;
using HttpRequest = AutoCheckIn.Net.HttpRequest;

namespace AutoCheckIn.Forum
{
    public class ServerSession
    {
        private int _verifiyCodeId;

        private ServerSession()
        {
        }

        public String ID { get; private set; }
        public SessionState State { get; private set; }
        public ImageSource VerifiyCode { get; private set; }
        public CookieCollection Cookies { get; private set; }
        public String UserAgent { get; set; }
        public UserInformation User { get; private set; }

        public static async Task<ServerSession> LoadCurrentSession()
        {
            CookieCollection cookies = new CookieCollection();
            if (!String.IsNullOrEmpty(Settings.Default.Cookies))
            {
                var cookiesList = JsonConvert.DeserializeObject<List<Cookie>>(Settings.Default.Cookies);
                foreach (var cookie in cookiesList)
                {
                    cookie.Domain = "www.tsdm.net";
                    cookies.Add(cookie);
                }
            }

            if (cookies.Count != 0)
            {
                ServerSession result = new ServerSession
                {
                    State = SessionState.Alive,
                    UserAgent =
                        "Mozilla/5.0 (Linux; Android 4.2.2) LENOVO Lenovo K860i (KHTML, like Gecko) net.tsdm.tut/1.2.1f Mobile",
                    Cookies = cookies
                };

                var info = result.User = await result.GetUserInformation();

                if (info.Status == 0)
                {
                    return result;
                }
            }

            return CreateNewSession();
        }

        public static ServerSession CreateNewSession()
        {
            ServerSession result = new ServerSession
            {
                State = SessionState.Available,
                UserAgent =
                    "Mozilla/5.0 (Linux; Android 4.2.2) LENOVO Lenovo K860i (KHTML, like Gecko) net.tsdm.tut/1.2.1f Mobile",
                Cookies = new CookieCollection()
            };

            return result;
        }

        private void SetCookies(HttpWebResponse response)
        {
            var cookieString = response.Headers[HttpResponseHeader.SetCookie];

            var matches = Regex.Matches(cookieString,
                @"(?<Name>[^=,;\s]*)=(?<Value>[^,;]*)(?:(?:; expires=(?<Expires>[^;]*))|(?:; Max-Age=(?<MaxAge>[0-9]*))|(?:; path=(?<Path>[^,;]*))|(?:; (?<HttpOnly>HttpOnly)))*,?");

            foreach (Match match in matches)
            {
                var cookie = new Cookie();
                if (match.Groups["Name"].Success)
                {
                    cookie.Name = match.Groups["Name"].Value;
                }
                if (match.Groups["Value"].Success)
                {
                    cookie.Value = match.Groups["Value"].Value;
                }
                if (match.Groups["Expires"].Success)
                {
                    cookie.Expires = DateTime.Parse(match.Groups["Expires"].Value);
                }
                if (match.Groups["MaxAge"].Success)
                {
                }
                if (match.Groups["Path"].Success)
                {
                    cookie.Path = match.Groups["Path"].Value;
                }
                cookie.HttpOnly = match.Groups["HttpOnly"].Success;

                Cookies.Add(cookie);
            }
        }

        public async Task<ImageSource> ReadyToLogin()
        {
            if (State != SessionState.Available && State != SessionState.CanLogin)
            {
                throw new NotSupportedException("只有为 Available 或者 CanLogin 的会话才能准备登录。");
            }

            var id = ++_verifiyCodeId;

            var request = await HttpRequest.Create(
                "http://www.tsdm.net/plugin.php?id=oracle:verify&mobile=yes&tsdmapp=1")
                .SetUserAgent(UserAgent).Get().Wait();

            if (_verifiyCodeId == id)
            {
                SetCookies(request.Response);
                SetCookies(request.Response);
                ID = Cookies["gkr8_2132_sid"].Value;

                Logger.Log(LogType.Information, $"已获取验证码ID:{ID}");

                VerifiyCode = await request.GetDataAsImage();
                State = SessionState.CanLogin;
                return VerifiyCode;
            }
            return null;
        }

        public async Task<TransportResult> Login(LoginMode mode, String accout, String password, String verifiyCode,
            int verifiyQuestion,
            String verifiyAnswer)
        {
            if (State != SessionState.CanLogin)
            {
                throw new NotSupportedException("只有为 CanLogin 的会话才能登录。");
            }

            String loginModeValue = null;

            switch (mode)
            {
                case LoginMode.UID:
                    loginModeValue = "uid";
                    break;

                case LoginMode.EMail:
                    loginModeValue = "email";
                    break;

                case LoginMode.UserName:
                default:
                    loginModeValue = "username";
                    break;
            }

            String submit =
                $"answer={verifiyAnswer}&fastloginfield={loginModeValue}&username={accout}&password={password}&tsdm_verify_prikey={ID}&questionid={verifiyQuestion}&tsdm_verify={verifiyCode}&";
            Stream submitStream = new MemoryStream(Encoding.UTF8.GetBytes(submit));

            Logger.Log(LogType.Information, $"已准备好登录数据，登录ID:{ID}");

            var request = await HttpRequest.Create(
                "http://www.tsdm.net/member.php?mod=logging&action=login&loginsubmit=yes&mobile=yes&tsdmapp=1")
                .SetUserAgent(UserAgent).SetContentType("application/x-www-form-urlencoded; charset=UTF-8")
                .AddCookies(new Uri("http://www.tsdm.net"), Cookies)
                .Post(submitStream)
                .Wait();

            SetCookies(request.Response);
            ID = Cookies["gkr8_2132_sid"].Value;

            var result =
                JsonConvert.DeserializeObject<TransportResult>(
                    request.GetDataAsString(Encoding.GetEncoding(936)).Replace("[]", "null"));
            
            if (result.Status == 0)
            {
                Logger.Log(LogType.Information, $"{accout}登录成功，信息:{result.Message}");
                State = SessionState.Alive;
            }
            else
            {
                Logger.Log(LogType.Warning, $"{accout}登录失败，信息:{result.Message}");
            }

            return result;
        }

        public void Logout()
        {
            User = null;
            Cookies = new CookieCollection();
            VerifiyCode = null;
            ID = null;
            State = SessionState.Available;

            Logger.Log(LogType.Information, $"已注销");
        }

        public async Task<UserInformation> GetUserInformation()
        {
            if (State != SessionState.Alive)
            {
                throw new NotSupportedException("只有为 Alive 的会话才能获取用户信息。");
            }

            var request = await HttpRequest.Create(
                "http://www.tsdm.net/home.php?mod=space&do=profile&mobile=yes&tsdmapp=1")
                .SetUserAgent(UserAgent)
                .AddCookies(new Uri("http://www.tsdm.net"), Cookies)
                .Get()
                .Wait();

            SetCookies(request.Response);
            ID = Cookies["gkr8_2132_sid"].Value;

            User = JsonConvert.DeserializeObject<UserInformation>(request.GetDataAsString(Encoding.GetEncoding(936)));

            if (User.Status == 0)
            {
                Logger.Log(LogType.Information, $"获取用户信息成功，信息:{User.Message}");
            }
            else
            {
                Logger.Log(LogType.Warning, $"获取用户信息失败，信息:{User.Message}");
            }

            return User;
        }

        public async Task<TransportResult> ReadyToSign()
        {
            if (State != SessionState.Alive)
            {
                throw new NotSupportedException("只有为 Alive 的会话才能准备签到。");
            }

            TransportResult result = new TransportResult {Url = "/plugin.php?id=dsu_paulsign:sign&mobile=yes"};
            var request = await HttpRequest.Create(
                "http://www.tsdm.net/plugin.php?id=dsu_paulsign:sign&mobile=yes")
                .SetUserAgent(UserAgent)
                .AddCookies(new Uri("http://www.tsdm.net"), Cookies)
                .Get()
                .Wait();

            SetCookies(request.Response);
            ID = Cookies["gkr8_2132_sid"].Value;
            var data = request.GetDataAsString();

            if (data.Contains("您今天已经签到过了或者签到时间还未开始"))
            {
                result.Message = "您今天已经签到过了或者签到时间还未开始";
                result.Status = -1;
                result.Url = "/plugin.php?id=dsu_paulsign:sign&mobile=yes";
                
                Logger.Log(LogType.Information, $"由于今天已经签到，签到失败");

                return result;
            }

            var match = Regex.Match(data, "<input type=\"hidden\" name=\"formhash\" value=\"(?<Hash>[a-zA-Z0-9]*)\">");
            if (!match.Success)
            {
                result.Message = "获取表格 Hash 失败，无法进行签到。";
                result.Status = -1;
                
                Logger.Log(LogType.Warning, $"无法获取表格 hash");

                return result;
            }

            var formHash = match.Groups["Hash"].Value;
            result.Values = new Dictionary<string, string>();
            result.Values.Add("formhash", formHash);


            Logger.Log(LogType.Information, $"获取签到表格hash:{formHash}");

            return result;
        }

        public async Task<TransportResult> Sign(SignMood mood, String content, String formHash)
        {
            //<input type="hidden" name="formhash" value="1c5d4ab8">

            if (State != SessionState.Alive)
            {
                throw new NotSupportedException("只有为 Alive 的会话才能签到。");
            }

            TransportResult result = new TransportResult
            {
                Url = "/plugin.php?id=dsu_paulsign:sign&operation=qiandao&infloat=0&inajax=0&mobile=yes"
            };
            String moodValue = null;

            switch (mood)
            {
                case SignMood.Sad:
                    moodValue = "ng";
                    break;

                case SignMood.Depressed:
                    moodValue = "ym";
                    break;

                case SignMood.Boring:
                    moodValue = "wl";
                    break;

                case SignMood.Anger:
                    moodValue = "nu";
                    break;

                case SignMood.Sweat:
                    moodValue = "ch";
                    break;

                case SignMood.Struggle:
                    moodValue = "fd";
                    break;

                case SignMood.Lazy:
                    moodValue = "yl";
                    break;

                case SignMood.Decline:
                    moodValue = "shuai";
                    break;

                case SignMood.Happy:
                default:
                    moodValue = "kx";
                    break;
            }

            String submit =
                $"qdmode=1&formhash={formHash}&todaysay={HttpUtility.UrlEncode(content)}&qdxq={moodValue}&";
            Stream submitStream = new MemoryStream(Encoding.UTF8.GetBytes(submit));

            Logger.Log(LogType.Information, $"开始签到，签到心情:{mood}，签到内容:{content}，表格hash:{formHash}");

            var request = await HttpRequest.Create(
                "http://www.tsdm.net/plugin.php?id=dsu_paulsign:sign&operation=qiandao&infloat=0&inajax=0&mobile=yes")
                .SetUserAgent(UserAgent).SetContentType("application/x-www-form-urlencoded; charset=UTF-8")
                .AddCookies(new Uri("http://www.tsdm.net"), Cookies)
                .Post(submitStream)
                .Wait();

            SetCookies(request.Response);
            ID = Cookies["gkr8_2132_sid"].Value;
            var data = request.GetDataAsString();

            var match = Regex.Match(data, "<div id=\"messagetext\">[\\s]*<p>(?<Message>[^<]*)</p>");
            if (!match.Success)
            {
                Logger.Log(LogType.Error, $"由于不明原因签到失败");
                result.Message = "获取签到结果失败。";
                result.Status = -1;
                return result;
            }

            result.Message = match.Groups["Message"].Value;

            if (result.Message.Contains("恭喜你签到成功!"))
            {
                result.Status = 0;

                match = Regex.Match(result.Message, @"恭喜你签到成功!获得随机奖励 (?<Reward>[^\.]*?) \.");
                if (match.Success)
                {
                    result.Values = new Dictionary<string, string>();
                    result.Values.Add("reward", match.Groups["Message"].Value);
                }

                Logger.Log(LogType.Information, $"签到成功，获得奖励{match.Groups["Message"].Value}");

                (App.Current as App).NotifyIcon.ShowBalloonTip(3000, "签到成功", $"签到成功，获得奖励{match.Groups["Message"].Value}", ToolTipIcon.Info);

                (App.Current as App).ViewModel.CheckSignTime = DateTime.Now;
                (App.Current as App).ViewModel.IsSignedToday = true;
            }
            else
            {
                result.Status = -1;

                Logger.Log(LogType.Error, $"签到失败，{result.Message}");
            }
            return result;
        }
    }

    public enum SessionState
    {
        Available,
        CanLogin,
        Alive,
        Ended
    }

    public class TransportResult
    {
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "url")]
        public String Url { get; set; }

        [JsonProperty(PropertyName = "message")]
        public String Message { get; set; }

        [JsonProperty(PropertyName = "extra")]
        public Dictionary<String, String> Extra { get; set; }

        [JsonProperty(PropertyName = "values")]
        public Dictionary<String, String> Values { get; set; }
    }

    public enum SignMood
    {
        Happy,
        Sad,
        Depressed,
        Boring,
        Anger,
        Sweat,
        Struggle,
        Lazy,
        Decline
    }

    public class UserInformation : TransportResult
    {
        [JsonProperty(PropertyName = "aid")]
        public int AID { get; set; }

        [JsonProperty(PropertyName = "avatar")]
        public String Avatar { get; set; }

        [JsonProperty(PropertyName = "cpuid")]
        public int CPUID { get; set; }

        [JsonProperty(PropertyName = "cpusername")]
        public String CPUserName { get; set; }

        [JsonProperty(PropertyName = "credits")]
        public int Credits { get; set; }

        [JsonProperty(PropertyName = "customstatus")]
        public object CustomStatus { get; set; }

        [JsonProperty(PropertyName = "extcredits1")]
        public String ExtCredits1 { get; set; }

        [JsonProperty(PropertyName = "extcredits2")]
        public String ExtCredits2 { get; set; }

        [JsonProperty(PropertyName = "extcredits3")]
        public String ExtCredits3 { get; set; }

        [JsonProperty(PropertyName = "extcredits4")]
        public String ExtCredits4 { get; set; }

        [JsonProperty(PropertyName = "extcredits5")]
        public String ExtCredits5 { get; set; }

        [JsonProperty(PropertyName = "extcredits6")]
        public String ExtCredits6 { get; set; }

        [JsonProperty(PropertyName = "extcredits7")]
        public String ExtCredits7 { get; set; }

        [JsonProperty(PropertyName = "gid")]
        public int GourpID { get; set; }

        [JsonProperty(PropertyName = "miku")]
        public int Miku { get; set; }

        [JsonProperty(PropertyName = "nickname")]
        public String NickName { get; set; }

        [JsonProperty(PropertyName = "posts")]
        public int Posts { get; set; }

        [JsonProperty(PropertyName = "readaccess")]
        public int ReadAccess { get; set; }

        [JsonProperty(PropertyName = "regdate")]
        public DateTime RegisterDate { get; set; }

        [JsonProperty(PropertyName = "threads")]
        public int Threads { get; set; }

        [JsonProperty(PropertyName = "uid")]
        public int UID { get; set; }

        [JsonProperty(PropertyName = "username")]
        public String UserName { get; set; }
    }

    public enum LoginMode
    {
        UserName,
        UID,
        EMail
    }
}