// Project: AutoCheckIn (https://github.com/higankanshi/AutoCheckIn)
// Filename: HttpRequestProgressType.cs
// Version: 20160411

namespace AutoCheckIn.Net
{
    /// <summary>
    ///     表示 HTTP 请求的传输方向。
    /// </summary>
    public enum HttpRequestProgressType
    {
        /// <summary>
        ///     未确定
        /// </summary>
        Unkown,

        /// <summary>
        ///     上传
        /// </summary>
        Upload,

        /// <summary>
        ///     下载
        /// </summary>
        Download
    }
}