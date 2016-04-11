
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoCheckIn.JavaScript;
using mshtml;
using Newtonsoft.Json;
using WebBrowser = System.Windows.Forms.WebBrowser;
using HtmlElement = System.Windows.Forms.HtmlElement;

namespace AutoCheckIn
{
    /// <summary>
    /// WebWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WebWindow : Window
    {
        private WebWindow()
        {
            InitializeComponent();
        }

        public async static Task<WebWindow> CreateAndInitialize()
        {
            WebWindow result = new WebWindow();
            result.Show();
            await result.Navigate(MainPage);
            return result;
        }

        private const String MainPage = "http://www.tsdm.net/forum.php?mobile=yes&simpletype=yes";
        private const String LoginPage = "http://www.tsdm.net/member.php?mod=logging&action=login&mobile=yes&simpletype=yes";
        private const String SignPage = "http://www.tsdm.net/plugin.php?id=dsu_paulsign:sign&mobile=yes&simpletype=yes";

        private NavigateWaitHandle _firstWaitHandle;

        private List<NavigateWaitHandle> _waitHandles = new List<NavigateWaitHandle>();

        private void WebBrowser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            List<NavigateWaitHandle> setHandles = _waitHandles.Where(waitHandle => waitHandle.Set(e.Url.ToString())).ToList();

            foreach (var handle in setHandles)
            {
                _waitHandles.Remove(handle);
            }
        }

        private async Task Navigate(String url)
        {
            NavigateWaitHandle handle = new NavigateWaitHandle(url);
            _waitHandles.Add(handle);
            WebBrowser.Navigate(url);
            await handle.Wait();
        }

        private async Task HookScript(WebBrowser webBrowser, Uri uri)
        {
            var tmp = uri.ToString().Split(new[]
            {
                '/','\\'
            });

            var fileName = tmp[tmp.Length - 1];
            tmp = fileName.Split('.');
            fileName = tmp[0];

            if (WebBrowser.Document.GetElementById($"Hook_{fileName}Script") != null) return;

            var scriptStream = Application.GetResourceStream(uri);
            StreamReader streamReader = new StreamReader(scriptStream.Stream);
            var script = await streamReader.ReadToEndAsync();

            HtmlElement head = WebBrowser.Document.GetElementsByTagName("head")[0];
            HtmlElement scriptEl = WebBrowser.Document.CreateElement("script");
            IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
            ((IHTMLElement)element).id = $"Hook_{fileName}Script";
            element.text = script;
            head.AppendChild(scriptEl);
        }

        private async Task HookScript(WebBrowser webBrowser, string uri)
        {
            await HookScript(webBrowser, new Uri(uri, UriKind.Relative));
        }

        private async Task<LoginState> GetUserInformationInternal()
        {
            await HookScript(WebBrowser, "/Assets/Json.js");
            await HookScript(WebBrowser, "/Assets/GetUserInformation.js");
            var resultData = WebBrowser.Document.InvokeScript("GetUserInformation");

            var result = JsonConvert.DeserializeObject<LoginState>(resultData.ToString());

            return result;
        }

        public async Task<LoginState> GetUserInformation()
        {
            switch (WebBrowser.Url.ToString())
            {
                case MainPage:
                case SignPage:
                    break;
                case LoginPage:
                default:
                    await Navigate(MainPage);
                    break;
            }

            return await GetUserInformationInternal();
        }

        private async Task<ImageSource> GetVerifyCodeInternal()
        {
            //await HookScript(WebBrowser, "/Assets/Json.js");
            //await HookScript(WebBrowser, "/Assets/GetCode.js");
            //var resultData = WebBrowser.Document.InvokeScript("GetCode");

            //var result = JsonConvert.DeserializeObject<ImageSize>(resultData.ToString());

            //if (result.ErrorCode != ErrorCode.Ok)
            //{
            //    throw new Exception($"{result.ErrorCode}");
            //}


            foreach (HtmlElement image in WebBrowser.Document.Images)
            {
                var dom = (IHTMLImgElement)image.DomElement;
                if (dom.src == "http://www.tsdm.net/plugin.php?id=oracle:verify")
                {
                    HTMLBody body = (HTMLBody)WebBrowser.Document.Body.DomElement;
                    IHTMLControlRange rang = (IHTMLControlRange)body.createControlRange();
                    rang.add((IHTMLControlElement)dom);
                    var result = rang.execCommand("Copy");

                    await Task.Delay(1000);

                    Bitmap bitmap = (Bitmap)System.Windows.Forms.Clipboard.GetImage();
                    var s = BitmapToBitmapSource(bitmap);
                    return s;
                }
            }

            return null;
        }

        public async Task<ImageSource> GetVerifyCode()
        {
            switch (WebBrowser.Url.ToString())
            {
                case LoginPage:
                    break;
                case MainPage:
                case SignPage:
                default:
                    await Navigate(LoginPage);
                    break;
            }

            return await GetVerifyCodeInternal();
        }

        private async Task<LoginState> LoginInternal(int accoutType, string user, string pwd, string vc, int question, string answer)
        {
            await HookScript(WebBrowser, "/Assets/Json.js");
            await HookScript(WebBrowser, "/Assets/Login.js");
            int accoutTypeValue = 0;

            switch (accoutType)
            {
                case 1:
                    accoutTypeValue = 2;
                    break;
                case 2:
                    accoutTypeValue = 1;
                    break;
            }

            var resultData = WebBrowser.Document.InvokeScript("Login", new object[] { accoutTypeValue, user, pwd, vc, question, answer });

            var result = JsonConvert.DeserializeObject<ErrorObject>(resultData.ToString());

            if (result.ErrorCode != ErrorCode.Ok)
            {
                throw new Exception($"{result.ErrorCode}");
            }

            NavigateWaitHandle handle = new NavigateWaitHandle(MainPage);
            _waitHandles.Add(handle);
            await handle.Wait(5000);

            return await GetUserInformation();
        }

        public async Task<LoginState> Login(int accoutType, string user, string pwd, string vc, int question, string answer)
        {
            if (WebBrowser.Url.ToString() != LoginPage)
            {
                var info = await GetUserInformation();

                if (info.IsLogin)
                {
                    if (user == info.UserName)
                    {
                        return info;
                    }
                    else
                    {
                        return new LoginState()
                        {
                            ErrorCode = ErrorCode.HaveLogin,
                            IsLogin = true
                        };
                    }
                }
            }

            switch (WebBrowser.Url.ToString())
            {
                case LoginPage:
                    break;
                case MainPage:
                case SignPage:
                default:
                    await Navigate(LoginPage);
                    break;
            }

            return await LoginInternal(accoutType, user, pwd, vc, question, answer);
        }

        public bool CheckLogin()
        {
            return WebBrowser.Document.Cookie.IndexOf("gkr8_2132_ulastactivity") != -1;
        }

        private async Task LogoutInternal()
        {
            await HookScript(WebBrowser, "/Assets/Json.js");
            await HookScript(WebBrowser, "/Assets/Logout.js");
            var resultData = WebBrowser.Document.InvokeScript("Logout");

            var result = JsonConvert.DeserializeObject<ErrorObject>(resultData.ToString());

            if (result.ErrorCode != ErrorCode.Ok)
            {
                throw new Exception($"{result.ErrorCode}");
            }
        }

        public async Task Logout()
        {
            if (CheckLogin())
            {
                await LogoutInternal();
            }
        }

        private async Task<bool> IsSignedInternal()
        {
            await HookScript(WebBrowser, "/Assets/Json.js");
            await HookScript(WebBrowser, "/Assets/CheckSign.js");
            var resultData = WebBrowser.Document.InvokeScript("CheckSign");

            var result = JsonConvert.DeserializeObject<SignState>(resultData.ToString());

            if (result.ErrorCode != ErrorCode.Ok)
            {
                throw new Exception($"{result.ErrorCode}");
            }

            return result.IsSigned;
        }

        public async Task<bool> IsSigned()
        {
            switch (WebBrowser.Url.ToString())
            {
                case SignPage:
                    break;
                case LoginPage:
                case MainPage:
                default:
                    await Navigate(SignPage);
                    break;
            }

            return await IsSignedInternal();
        }

        private async Task<bool> SignInternal(int mode, string say)
        {
            await HookScript(WebBrowser, "/Assets/Json.js");
            await HookScript(WebBrowser, "/Assets/CheckIn.js");
            var resultData = WebBrowser.Document.InvokeScript("CheckIn", new object[] { say, mode });

            var result = JsonConvert.DeserializeObject<ErrorObject>(resultData.ToString());

            if (result.ErrorCode != ErrorCode.Ok)
            {
                throw new Exception($"{result.ErrorCode}");
            }

            return true;
        }

        public async Task<bool> Sign(int mode = -1, string say = null)
        {
            if (await IsSigned())
            {
                return false;
            }

            switch (WebBrowser.Url.ToString())
            {
                case SignPage:
                    break;
                case LoginPage:
                default:
                case MainPage:
                    await Navigate(SignPage);
                    break;
            }

            return await SignInternal(mode,say);
        }


        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            BitmapSource result =
                Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //release resource
            DeleteObject(ptr);

            return result;
        }
    }

    public class NavigateWaitHandle
    {
        public NavigateWaitHandle(PageType type)
        {
            PageType = type;
            WaitHandle = new AutoResetEvent(false);
        }

        public AutoResetEvent WaitHandle { get; private set; }

        public PageType PageType { get; private set; }

        public Task Wait()
        {
            return Task.Run(() =>
            {
                WaitHandle.WaitOne();
            });
        }

        public Task Wait(int ms)
        {
            return Task.Run(() =>
            {
                WaitHandle.WaitOne(ms);
            });
        }

        public bool Set(PageType type)
        {
            if (type == PageType)
            {
                WaitHandle.Set();
                return true;
            }

            return false;
        }
    }
}
