using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class AuditLogDAL
    {
        // Get all logs
        public static DataTable GetAllLogs()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT TOP 100 LogID, Username, UserRole, Action, EntityType, 
                                  NewValue, OldValue, ActionDateTime, Status
                                  FROM AuditLogs 
                                  ORDER BY ActionDateTime DESC";
                    new SqlDataAdapter(new SqlCommand(sql, conn)).Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }
            return dt;
        }

        // ── Naya method jo AuditLog form use karta hai ──────────────
        public static DataTable GetLogsByDateRange(DateTime fromDate, DateTime toDate)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT LogID, Username, UserRole, Action, EntityType,
                                          NewValue, OldValue, ActionDateTime, Status
                                   FROM   AuditLogs
                                   WHERE  ActionDateTime >= @FromDate
                                     AND  ActionDateTime <= @ToDate
                                   ORDER  BY ActionDateTime DESC";

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);
                    new SqlDataAdapter(cmd).Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLogsByDateRange Error: {ex.Message}");
            }
            return dt;
        }

        // Insert log
        public static bool InsertLog(int userId, string username, string userRole,
            string action, string entityType, string entityId,
            string oldValue, string newValue, string status)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO AuditLogs 
                                   (UserID, Username, UserRole, Action, EntityType, EntityID, 
                                    OldValue, NewValue, ActionDateTime, Status)
                                   VALUES 
                                   (@UserID, @Username, @UserRole, @Action, @EntityType, @EntityID,
                                    @OldValue, @NewValue, GETDATE(), @Status)";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserID", userId > 0 ? (object)userId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Username", username ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserRole", userRole ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@EntityType", entityType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EntityID", entityId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OldValue", oldValue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NewValue", newValue ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", status ?? "Success");
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Insert Error: {ex.Message}");
                return false;
            }
        }
    }
}