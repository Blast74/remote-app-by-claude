using System;
using System.IO;

namespace RemoteDesktopApp.Services
{
    public static class LoggingService
    {
        private static readonly string LogFilePath = "RemoteDesktopApp.log";

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogWarning(string message)
        {
            Log("WARNING", message);
        }

        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        public static void LogCritical(string message, Exception ex)
        {
            Log("CRITICAL", $"{message} - Exception: {ex.Message} - StackTrace: {ex.StackTrace}");
        }

        private static void Log(string level, string message)
        {
            try
            {
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Fail silently to avoid recursive logging errors
            }
        }
    }
}
