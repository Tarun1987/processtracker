using System;
using System.IO;

namespace ProcessTracker.Helpers
{
    public class Logger
    {
        private static void Log(string Message)
        {
            var logPath = string.Format($"{AppDomain.CurrentDomain.BaseDirectory}\\Logs");
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string filePath = string.Format($"{AppDomain.CurrentDomain.BaseDirectory}\\Logs\\log_{DateTime.Now.Date.ToShortDateString().Replace('/', '_')}.txt");
            if (!File.Exists(filePath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filePath))
                    sw.WriteLine(Message);
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filePath))
                    sw.WriteLine(Message);
            }
        }


        public static void LogError(string Message)
        {
            Log($"Error: {Message}");
        }

        public static void LogInfo(string Message)
        {
            Log($"Info: {Message}");
        }
    }
}
