// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: HttpRequest.cs
// Version: 20160411

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoCheckIn.Net
{
    /// <summary>
    ///     表示一个 HTTP 请求，该类采用流式函数接口，请注意函数的先后顺序。
    /// </summary>
    public class HttpRequest : NotifyPropertyObject, IDisposable
    {
        internal readonly EventWaitHandle _progressWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        internal readonly EventWaitHandle _queueWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private Stream _customStream;
        private Exception _exception;
        private bool _progressing;
        private HttpWebRequest _request;
        private HttpWebResponse _response;
        private Stream _resultStream;

        private HttpRequestState _state;

        private HttpRequest(HttpWebRequest request)
        {
            Request = request;
        }

        /// <summary>
        ///     表示传输时缓冲区大小，该大小表示，每获取到指定字节大小的数据时就将其写入结果，并提供进度报告，初始值为 64 KB。
        /// </summary>
        public static int TransferBufferSize { get; set; } = 64*1024;

        /// <summary>
        ///     默认次数的请求重试，初始值为 3。
        /// </summary>
        public static int DefaultRetryCount { get; set; } = 3;

        /// <summary>
        ///     HTTP 请求的状态。
        /// </summary>
        public HttpRequestState State
        {
            get { return _state; }
            internal set
            {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     HTTP 请求的内容。
        /// </summary>
        public HttpWebRequest Request
        {
            get { return _request; }
            internal set
            {
                if (Equals(value, _request)) return;
                _request = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     HTTP 响应的内容。
        /// </summary>
        public HttpWebResponse Response
        {
            get { return _response; }
            internal set
            {
                if (Equals(value, _response)) return;
                _response = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     当网络请求出现异常时，所捕获的异常。
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
            private set
            {
                if (Equals(value, _exception)) return;
                _exception = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     HTTP 响应结果流。
        /// </summary>
        public Stream ResultStream
        {
            get { return _customStream ?? _resultStream; }
            private set
            {
                if (Equals(value, _resultStream)) return;
                _resultStream = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     设置或获取一个委托，当 HTTP 传输进度改变时触发该委托。不仅仅是下载进度改变会触发这个事件，同样的 HTTP 请求状态改变也会触发改事件。
        /// </summary>
        public EventHandler<EventArgs<HttpRequestStats>> TransferProgressChanged { get; set; }

        /// <summary>
        ///     释放该次网络请求的资源，在该操作后内部的<see cref="ResultStream" />与<see cref="Response" />不再有效，但不会释放<see cref="SetResultStream" />
        ///     所设置的流。
        /// </summary>
        public void Dispose()
        {
            Request?.Abort();
            Response?.Dispose();
            if (_customStream == null)
            {
                ((IDisposable) ResultStream)?.Dispose();
            }
        }

        /// <summary>
        ///     以 Url 字符串构建一个 HTTP 请求。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpRequest Create(String url)
        {
            return Create(new Uri(url));
        }

        /// <summary>
        ///     以 Uri 构建一个 HTTP 请求。
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static HttpRequest Create(Uri uri)
        {
            return new HttpRequest(WebRequest.CreateHttp(uri));
        }

        /// <summary>
        ///     向 HTTP 请求加入 Header。
        /// </summary>
        /// <param name="name">Header 名。</param>
        /// <param name="value">Header 值。</param>
        public HttpRequest AddHeader(String name, String value)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Headers[name] = value;
            return this;
        }

        /// <summary>
        ///     向 HTTP 请求加入 Header。
        /// </summary>
        /// <param name="header">Header 类型。</param>
        /// <param name="value">Header 值。</param>
        public HttpRequest AddHeader(HttpRequestHeader header, String value)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Headers[header] = value;
            return this;
        }

        /// <summary>
        ///     向 HTTP 请求加入 Header。
        /// </summary>
        /// <param name="header">Header 类型。</param>
        /// <param name="value">Header 值。</param>
        public HttpRequest AddHeader(HttpResponseHeader header, String value)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Headers[header] = value;
            return this;
        }

        public HttpRequest SetUserAgent(String value)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.UserAgent = value;
            return this;
        }

        /// <summary>
        /// </summary>
        /// 向 HTTP 请求加入 Cookie。
        /// <param name="uri">要加入 Cookie 的 Uri。</param>
        /// <param name="cookie">要加入的 Cookie。</param>
        /// <returns></returns>
        public HttpRequest AddCookie(Uri uri, Cookie cookie)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            if (Request.CookieContainer == null)
            {
                Request.CookieContainer = new CookieContainer();
            }
            Request.CookieContainer.Add(uri, cookie);
            return this;
        }

        /// <summary>
        ///     向 HTTP 请求加入 Cookies。
        /// </summary>
        /// <param name="uri">要加入 Cookie 的 Uri。</param>
        /// <param name="cookies">要加入的 Cookies。</param>
        /// <returns></returns>
        public HttpRequest AddCookies(Uri uri, CookieCollection cookies)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            if (Request.CookieContainer == null)
            {
                Request.CookieContainer = new CookieContainer();
            }
            Request.CookieContainer.Add(uri, cookies);
            return this;
        }

        public HttpRequest SetContentType(String contentType)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.ContentType = contentType;
            return this;
        }

        /// <summary>
        ///     提供相应的处理器来对该 HTTP 请求进行处理，例如添加凭证，Cookies，Header。
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public HttpRequest HandleRequest(HttpWebRequestHandler handler)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改，如果请求已经结束，请使用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            handler(Request);
            return this;
        }

        /// <summary>
        ///     设置 HTTP 请求的结果流，HTTP 请求的结果将会保留在流中。请注意：请确保流可读可写，并且流的位置在预定的位置，而且流的长度足够或是为可拓展流，否则将可能返回错误的结果。
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public HttpRequest SetResultStream(Stream stream)
        {
            if (!stream.CanWrite)
            {
                throw new ArgumentException("提供的流不可写。", nameof(stream));
            }

            OnPropertyChanging(nameof(ResultStream));
            _resultStream?.Dispose();
            _customStream = stream;
            OnPropertyChanged(nameof(ResultStream));

            return this;
        }

        /// <summary>
        ///     重新构造相同的 HTTP 请求，用于重复当前请求。
        /// </summary>
        public HttpRequest RebuildRequest()
        {
            if (State == HttpRequestState.Connecting || State == HttpRequestState.Progressing)
            {
                throw new InvalidOperationException("不能对传输中的请求进行修改。");
            }

            var oldRequest = Request;

            Request = WebRequest.CreateHttp(oldRequest.RequestUri);

            Request?.Abort();
            ResultStream?.Dispose();

            Request.Accept = oldRequest.Accept;
            Request.AllowReadStreamBuffering = oldRequest.AllowReadStreamBuffering;
            Request.ContentType = oldRequest.ContentType;
            Request.ContinueTimeout = oldRequest.ContinueTimeout;
            Request.CookieContainer = oldRequest.CookieContainer;
            Request.Credentials = oldRequest.Credentials;
            Request.Headers = oldRequest.Headers;
            Request.Method = oldRequest.Method;
            Request.Proxy = oldRequest.Proxy;
            Request.UseDefaultCredentials = oldRequest.UseDefaultCredentials;

            State = HttpRequestState.NothingSpecial;

            return this;
        }

        /// <summary>
        ///     使用默认的重试次数，Post 该请求。
        /// </summary>
        public HttpRequest Post(Stream content = null)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Post(DefaultRetryCount, content);
            return this;
        }

        /// <summary>
        ///     使用指定的重试次数，Post 该请求。
        /// </summary>
        public HttpRequest Post(int retryCount, Stream content = null)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Method = "POST";
            Start(retryCount, content);
            return this;
        }

        /// <summary>
        ///     使用默认的重试次数，Get 该请求。
        /// </summary>
        public HttpRequest Get()
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Get(DefaultRetryCount);
            return this;
        }

        /// <summary>
        ///     使用指定的重试次数，Get 该请求。
        /// </summary>
        public HttpRequest Get(int retryCount)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Request.Method = "GET";
            Start(retryCount);
            return this;
        }

        /// <summary>
        ///     使用默认的重试次数，Put 该请求。
        /// </summary>
        /// <param name="content">要 Put 的内容流</param>
        /// <returns></returns>
        public HttpRequest Put(Stream content)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            Put(DefaultRetryCount, content);
            return this;
        }

        /// <summary>
        ///     使用指定的重试次数，Put 该请求。
        /// </summary>
        /// <param name="retryCount"></param>
        /// <param name="content">要 Put 的内容流</param>
        /// <returns></returns>
        public HttpRequest Put(int retryCount, Stream content)
        {
            if (State != HttpRequestState.NothingSpecial)
            {
                throw new InvalidOperationException("不能对已进行过的请求再次发起请求，请考虑用 HttpRequest.RebuildRequest 方法重构请求。");
            }
            if (!content.CanRead)
            {
                throw new ArgumentException("内容流不可读。", nameof(content));
            }
            Request.Method = "PUT";
            Start(retryCount, content);
            return this;
        }

        /// <summary>
        ///     设置传输进度改变时的委托。
        /// </summary>
        public HttpRequest WhenTransferProgressChanged(EventHandler<EventArgs<HttpRequestStats>> handler)
        {
            TransferProgressChanged = handler;
            return this;
        }

        private void Start(int retryCount, Stream content = null)
        {
            _progressing = true;
            _progressWaitHandle.Reset();
            int retry = 0;

            Task.Run(async () =>
            {
                HttpRequestQueue.Current.Register(this);
                _queueWaitHandle.WaitOne();

                RETRY: //重试标签

                if (State == HttpRequestState.NothingSpecial)
                {
                    EventArgs<HttpRequestStats> progressInfo = null;

                    if (TransferProgressChanged != null)
                    {
                        progressInfo = new EventArgs<HttpRequestStats>(new HttpRequestStats());
                    }

                    try
                    {
                        State = HttpRequestState.Connecting;

                        if (TransferProgressChanged != null)
                        {
                            progressInfo.Value.Update();
                            TransferProgressChanged(this, progressInfo);
                        }

                        //数据上传过程，用于 Put 与 Post
                        if (content != null && (Request.Method == "PUT" || Request.Method == "POST"))
                        {
                            using (var requestStream = await Request.GetRequestStreamAsync())
                            {
                                State = HttpRequestState.Progressing;

                                if (TransferProgressChanged != null)
                                {
                                    progressInfo.Value.ProgressType = HttpRequestProgressType.Upload;
                                    progressInfo.Value.Update();
                                    progressInfo.Value.TotalBytes = content.Length;
                                    TransferProgressChanged(this, progressInfo);

                                    byte[] buffer = new byte[TransferBufferSize];
                                    int writeLength;

                                    while ((writeLength = content.Read(buffer, 0, buffer.Length)) != 0)
                                    {
                                        requestStream.Write(buffer, 0, writeLength);

                                        progressInfo.Value.Update(writeLength);
                                        TransferProgressChanged(this, progressInfo);
                                    }

                                    progressInfo = new EventArgs<HttpRequestStats>(new HttpRequestStats());
                                }
                                else
                                {
                                    content.CopyTo(requestStream);
                                }
                            }
                        }

                        //数据下载过程，用于 Post 与 Get
                        if (Request.Method == "GET" || Request.Method == "POST")
                        {
                            Response = (HttpWebResponse) await Request.GetResponseAsync();
                            State = HttpRequestState.Progressing;

                            #region 下载过程

                            using (var stream = Response.GetResponseStream())
                            {
                                State = HttpRequestState.Progressing;

                                if (_customStream == null)
                                {
                                    ResultStream =
                                        new MemoryStream((int) (Response.ContentLength > 0
                                            ? Response.ContentLength
                                            : 10*TransferBufferSize));
                                }

                                if (TransferProgressChanged != null)
                                {
                                    progressInfo.Value.ProgressType = HttpRequestProgressType.Download;
                                    progressInfo.Value.TotalBytes = Response.ContentLength > 0
                                        ? Response.ContentLength
                                        : 0;

                                    byte[] buffer = new byte[TransferBufferSize];

                                    long pos = 0;
                                    int readLength;
                                    if (ResultStream.CanSeek)
                                    {
                                        pos = ResultStream.Position;
                                    }

                                    while ((readLength = stream.Read(buffer, 0, buffer.Length)) != 0)
                                    {
                                        ResultStream.Write(buffer, 0, readLength);
                                        progressInfo.Value.Update(readLength);
                                        TransferProgressChanged(this, progressInfo);
                                    }

                                    if (ResultStream.CanSeek)
                                    {
                                        ResultStream.Position = pos;
                                    }
                                }
                                else
                                {
                                    long pos = 0;

                                    if (ResultStream.CanSeek)
                                    {
                                        pos = ResultStream.Position;
                                    }

                                    stream.CopyTo(ResultStream);

                                    if (ResultStream.CanSeek)
                                    {
                                        ResultStream.Position = pos;
                                    }
                                }
                            }

                            #endregion 下载过程
                        }

                        State = HttpRequestState.Completed;
                        if (TransferProgressChanged != null)
                        {
                            progressInfo.Value.Update();
                            TransferProgressChanged(this, progressInfo);
                        }
                        _progressWaitHandle.Set();
                        _progressing = false;

                        return;
                    }
                    catch (WebException webException)
                    {
                        Exception = webException;
                        State = HttpRequestState.ErrorOccurred;
                        if (TransferProgressChanged != null)
                        {
                            progressInfo.Value.Update();
                            TransferProgressChanged(this, progressInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Exception = ex;
                        _progressWaitHandle.Set();
                        _progressing = false;

                        throw;
                    }
                }

                if (retry < retryCount)
                {
                    retry++;
                    RebuildRequest(); //重构请求

                    goto RETRY; //重试请求
                }

                //取消等待
                _progressWaitHandle.Set();
                _progressing = false;
            });
        }

        /// <summary>
        ///     异步等待 HTTP 请求传输过程。
        /// </summary>
        public async Task<HttpRequest> Wait()
        {
            if (_progressing)
            {
                await Task.Run(() => { _progressWaitHandle.WaitOne(); });
            }
            return this;
        }

        /// <summary>
        ///     取消 HTTP 请求。
        /// </summary>
        public HttpRequest Canel()
        {
            Request.Abort();
            State = HttpRequestState.Cancelled;
            TransferProgressChanged?.Invoke(this, null);
            return this;
        }

        /// <summary>
        ///     将 HTTP 请求结果以 UTF-8 编码转换为字符串，并在结束后释放该次网络请求的资源，在该操作后<see cref="ResultStream" />与<see cref="Response" />不再有效。
        /// </summary>
        public String GetDataAsString()
        {
            return GetDataAsString(Encoding.UTF8);
        }

        /// <summary>
        ///     将 HTTP 请求结果以指定的代码页转换为字符串，并在结束后释放该次网络请求的资源，在该操作后内部的<see cref="ResultStream" />与<see cref="Response" />不再有效，但不会释放
        ///     <see cref="SetResultStream" />所设置的流。
        /// </summary>
        public String GetDataAsString(Encoding encoding)
        {
            if (State == HttpRequestState.Completed)
            {
                if (!ResultStream.CanRead)
                {
                    throw new ArgumentException("结果流不可读。", nameof(ResultStream));
                }

                if (ResultStream != null && ResultStream.Length > 0)
                {
                    byte[] data = new byte[ResultStream.Length];
                    ResultStream?.Read(data, 0, data.Length);
                    Dispose();
                    return encoding.GetString(data);
                }
                return null;
            }
            throw new InvalidOperationException("请在请求完成后调用 HttpRequest.GetDataAsString 方法。");
        }

        /// <summary>
        ///     将 HTTP 请求结果转换为<see cref="ImageSource" />，并在结束后释放该次网络请求的资源，在该操作后内部的<see cref="ResultStream" />与
        ///     <see cref="Response" />不再有效，但不会释放<see cref="SetResultStream" />所设置的流。
        /// </summary>
        public async Task<ImageSource> GetDataAsImage()
        {
            if (State == HttpRequestState.Completed)
            {
                if (!ResultStream.CanRead)
                {
                    throw new ArgumentException("结果流不可读。", nameof(ResultStream));
                }

                if (ResultStream != null && ResultStream.Length > 0)
                {
                    ResultStream.Position = 0;
                    var decoder = BitmapDecoder.Create(ResultStream, BitmapCreateOptions.DelayCreation,
                        BitmapCacheOption.Default);
                    var image = decoder.Frames[0];
                    WriteableBitmap wb = new WriteableBitmap(image);
                    Dispose();
                    return wb;
                }
                return null;
            }
            throw new InvalidOperationException("请在请求完成后调用 HttpRequest.GetDataAsString 方法。");
        }
    }
}