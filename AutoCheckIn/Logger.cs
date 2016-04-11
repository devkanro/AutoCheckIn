using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCheckIn
{
    public class Logger
    {
        public static String LogFile => "autocheckin.log";

        public static void Log(LogType type, String message)
        {
            string line = $"[{DateTime.Now}] {type}:{message}\r\n";
            File.AppendAllText(LogFile, line);
            LogWindow.Append(line);
        }
    }

    public enum LogType
    {
        Information,
        Warning,
        Error,
        Exception
    }
}
