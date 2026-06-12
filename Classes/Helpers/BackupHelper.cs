using BloodBankManagementSystem.Classes.Database;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class BackupHelper
    {
        private static string backupFolder = "C:\\Backups\\BloodBank\\";
        private static string connectionString = System.Configuration.ConfigurationManager
            .ConnectionStrings["BloodBankDB"].ConnectionString;

        static BackupHelper()
        {
            // Create backup folder if not exists
            if (!Directory.Exists(backupFolder))
            {
                try
                {
                    Directory.CreateDirectory(backupFolder);
                }
                catch { }
            }
        }

        public static bool CreateBackup(out string backupPath, out string errorMessage)
        {
            backupPath = "";
            errorMessage = "";

            try
            {
                // Get database name from connection string
                var builder = new SqlConnectionStringBuilder(connectionString);
                string databaseName = builder.InitialCatalog;

                // Generate backup filename
                string backupFileName = $"{databaseName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                backupPath = Path.Combine(backupFolder, backupFileName);

                // Create backup command
                string backupQuery = $"BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(backupQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Log the backup
                LogBackup(backupFileName, backupPath, new FileInfo(backupPath).Length, "Success");

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                LogBackup("", "", 0, $"Failed: {ex.Message}");
                return false;
            }
        }

        private static void LogBackup(string fileName, string path, long size, string status)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO BackupLogs (BackupFileName, BackupPath, BackupSize, BackupDate, BackedUpBy, Status)
                                   VALUES (@FileName, @Path, @Size, GETDATE(), @UserId, @Status)";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@FileName", fileName);
                    cmd.Parameters.AddWithValue("@Path", path);
                    cmd.Parameters.AddWithValue("@Size", size);
                    cmd.Parameters.AddWithValue("@UserId", SessionManager.CurrentUserID);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }

        public static string GetLastBackupInfo()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT TOP 1 BackupFileName, BackupDate, BackupSize FROM BackupLogs WHERE Status = 'Success' ORDER BY BackupDate DESC";
                    var cmd = new SqlCommand(sql, conn);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string fileName = reader["BackupFileName"].ToString();
                        DateTime backupDate = Convert.ToDateTime(reader["BackupDate"]);
                        long size = Convert.ToInt64(reader["BackupSize"]);
                        string sizeStr = FormatFileSize(size);
                        return $"{fileName}\n{backupDate:dd-MMM-yyyy HH:mm:ss}\nSize: {sizeStr}";
                    }
                }
            }
            catch { }
            return "No backup found";
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        public static void SetBackupPath(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch { }
            }
            backupFolder = path;
        }

        public static string GetBackupPath()
        {
            return backupFolder;
        }
    }
}