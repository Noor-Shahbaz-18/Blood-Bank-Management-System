using System;
using System.IO;
using System.Text;
using System.Threading;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string logFolder = "Logs";

        static Logger()
        {
            // Create logs folder if not exists
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
        }

        // Log information
        public static void Info(string message)
        {
            Log("INFO", message);
        }

        // Log warning
        public static void Warning(string message)
        {
            Log("WARNING", message);
        }

        // Log error
        public static void Error(string message)
        {
            Log("ERROR", message);
        }

        // Log error with exception
        public static void Error(string message, Exception ex)
        {
            Log("ERROR", $"{message} | Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
        }

        // Log debug
        public static void Debug(string message)
        {
#if DEBUG
            Log("DEBUG", message);
#endif
        }

        // Log user action
        public static void LogAction(string username, string action, string details)
        {
            Log("AUDIT", $"User: {username} | Action: {action} | Details: {details}");
        }

        // Log login attempt
        public static void LogLogin(string username, bool success, string ipAddress)
        {
            string status = success ? "SUCCESS" : "FAILED";
            Log("LOGIN", $"User: {username} | Status: {status} | IP: {ipAddress}");
        }

        // Main log method
        private static void Log(string level, string message)
        {
            try
            {
                string logFile = Path.Combine(logFolder, $"log_{DateTime.Now:yyyyMMdd}.txt");
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

                lock (_lock)
                {
                    File.AppendAllText(logFile, logEntry + Environment.NewLine);
                }

                // Also write to debug output
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logger failed: {ex.Message}");
            }
        }

        // Get log file content
        public static string GetLogs(string level = null, int days = 7)
        {
            try
            {
                StringBuilder logs = new StringBuilder();
                DateTime fromDate = DateTime.Today.AddDays(-days);

                for (DateTime date = fromDate; date <= DateTime.Today; date = date.AddDays(1))
                {
                    string logFile = Path.Combine(logFolder, $"log_{date:yyyyMMdd}.txt");

                    if (File.Exists(logFile))
                    {
                        string content = File.ReadAllText(logFile);

                        if (!string.IsNullOrEmpty(level))
                        {
                            // Filter by level
                            var lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var line in lines)
                            {
                                if (line.Contains($"[{level}]"))
                                {
                                    logs.AppendLine(line);
                                }
                            }
                        }
                        else
                        {
                            logs.AppendLine(content);
                        }
                    }
                }

                return logs.ToString();
            }
            catch (Exception ex)
            {
                return $"Error reading logs: {ex.Message}";
            }
        }

        // Clear old logs
        public static void ClearOldLogs(int daysToKeep = 30)
        {
            try
            {
                var files = Directory.GetFiles(logFolder, "log_*.txt");
                DateTime cutoff = DateTime.Today.AddDays(-daysToKeep);

                foreach (var file in files)
                {
                    if (File.GetCreationTime(file) < cutoff)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Error($"Failed to clear old logs: {ex.Message}");
            }
        }
    }
}