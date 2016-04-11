// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: HttpRequestQueue.cs
// Version: 20160411

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoCheckIn.Net
{
    public class HttpRequestQueue
    {
        private object _lockQueueObject = new object();
        private object _lockWindowObject = new object();

        private HttpRequestQueue(int concurrentConnection)
        {
            ConcurrentConnection = concurrentConnection;
            RequestWindow = new List<HttpRequest>(ConcurrentConnection);
            RequestQueue = new Queue<HttpRequest>();
        }

        public static HttpRequestQueue Current { get; private set; } = new HttpRequestQueue(5);

        /// <summary>
        ///     并发连接数，初始值为5，表示同时最多只能有 5 个 HTTP 请求在进行。
        /// </summary>
        public int ConcurrentConnection { get; set; }

        public List<HttpRequest> RequestWindow { get; private set; }

        public Queue<HttpRequest> RequestQueue { get; private set; }

        public void Register(HttpRequest request)
        {
            if (RequestQueue.Contains(request) || RequestWindow.Contains(request))
            {
                throw new InvalidOperationException("HTTP 请求已经在处理中，或正在队列中。");
            }
            request._queueWaitHandle.Reset();
            Task.Run(() => { AddRequestToQueue(request); });
        }

        private void AddRequestToQueue(HttpRequest request)
        {
            lock (_lockQueueObject)
            {
                RequestQueue.Enqueue(request);
            }
            UpdateState();
        }

        private void UpdateState()
        {
            HttpRequest now = null;

            lock (_lockQueueObject)
            {
                lock (_lockWindowObject)
                {
                    if (RequestWindow.Count < ConcurrentConnection)
                    {
                        if (RequestQueue.Count != 0)
                        {
                            now = RequestQueue.Dequeue();
                            System.Diagnostics.Debug.Assert(now != null);
                        }
                    }
                }
            }

            if (now != null)
            {
                HandleRequest(now);
            }
        }

        private void HandleRequest(HttpRequest request)
        {
            lock (_lockWindowObject)
            {
                RequestWindow.Add(request);
            }
            request._queueWaitHandle.Set();

            Task.Run(() =>
            {
                request._progressWaitHandle.WaitOne();
                lock (_lockWindowObject)
                {
                    RequestWindow.Remove(request);
                }
                UpdateState();
            });
        }
    }
}