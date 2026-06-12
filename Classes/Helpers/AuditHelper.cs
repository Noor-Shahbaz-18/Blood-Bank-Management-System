using BloodBankManagementSystem.Classes.Database;

namespace BloodBankManagementSystem.Classes.Helpers
{
    public static class AuditHelper
    {
        // Simple method to log actions
        public static void Log(string action, string entityType, string details)
        {
            try
            {
                AuditLogDAL.InsertLog(
                    SessionManager.CurrentUserID,
                    SessionManager.CurrentUsername,
                    SessionManager.CurrentRole.ToString(),
                    action,
                    entityType,
                    null,
                    null,
                    details,
                    "Success"
                );
            }
            catch
            {
                // Silent fail - logging should not break the application
            }
        }

        // For login (no current user yet)
        public static void LogLogin(string username, string role, bool success)
        {
            try
            {
                AuditLogDAL.InsertLog(
                    0,
                    username,
                    role,
                    "Login",
                    "System",
                    null,
                    null,
                    $"User {username} login {(success ? "successful" : "failed")}",
                    success ? "Success" : "Failed"
                );
            }
            catch { }
        }

        // For add operations
        public static void LogAdd(string entityType, string details)
        {
            Log("Add", entityType, $"Added new {entityType}: {details}");
        }

        // For edit operations
        public static void LogEdit(string entityType, string entityId, string details)
        {
            Log("Edit", entityType, $"Edited {entityType} (ID: {entityId}): {details}");
        }

        // For delete operations
        public static void LogDelete(string entityType, string entityId, string details)
        {
            Log("Delete", entityType, $"Deleted {entityType} (ID: {entityId}): {details}");
        }

        // For view operations
        public static void LogView(string entityType, string details)
        {
            Log("View", entityType, $"Viewed {entityType}: {details}");
        }
    }
}