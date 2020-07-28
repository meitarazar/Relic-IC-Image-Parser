using Relic_IC_Image_Parser.cSharp.data;
using System;
using System.IO;

namespace Relic_IC_Image_Parser.cSharp.util
{
    class Logger
    {
        private static StreamWriter logFileStreamWriter;
        
        public static void InitLog()
        {
            if (DataManager.logTake)
            {
                string prefix = DataManager.logPath.Replace(".\\", App.Location);
                string logFilePath = Path.GetFullPath(Path.Combine(prefix, DataManager.logFile));
                try
                {
                    if (!File.Exists(logFilePath))
                    {
                        File.Create(logFilePath).Close();
                    }
                    logFileStreamWriter = File.AppendText(logFilePath);

                    int padding = 12;
                    string title = App.AppName + " - v" + App.VersionName;
                    string msg = "\n" + new string('#', title.Length + (padding + 1) * 2) + "\n" +
                        new string('#', padding) + " " + title + " " + new string('#', padding) + "\n" +
                        new string('#', title.Length + (padding + 1) * 2) + "\n";
                    logFileStreamWriter.WriteLine(msg);
                    logFileStreamWriter.Flush();
                }
                catch { }
            }
        }
        
        public static void Append(string tag, string method, string msg)
        {
            if (!DataManager.logTake || logFileStreamWriter == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(tag))
            {
                tag = "";
            }
            else
            {
                tag += " | ";
            }

            if (string.IsNullOrEmpty(method))
            {
                method = "";
            }
            else
            {
                method += " > ";
            }

            if (string.IsNullOrEmpty(msg))
            {
                msg = "";
            }

            logFileStreamWriter.WriteLine(TimeStampPrefix(tag + method + msg));
            logFileStreamWriter.Flush();
        }

        private static string TimeStampPrefix(string str)
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + str;
        }
    }
}
