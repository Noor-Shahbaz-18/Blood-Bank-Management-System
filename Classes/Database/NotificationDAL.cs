using BloodBankManagementSystem.Classes.Helpers;
using BloodBankManagementSystem.Classes.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class NotificationDAL
    {
        // =====================================================
        // GET NOTIFICATIONS BY USER ID
        // =====================================================
        public static DataTable GetByUser(int userID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT NotificationID, UserID, Title, Message, Type, Priority, 
                                          IsRead, CreatedAt, ReadAt, ActionUrl, RelatedID, RelatedType, SentBy
                                  FROM Notifications 
                                  WHERE UserID = @UserID 
                                  ORDER BY 
                                      CASE WHEN IsRead = 0 THEN 0 ELSE 1 END,
                                      CASE Priority 
                                          WHEN 'Emergency' THEN 1 
                                          WHEN 'Urgent' THEN 2 
                                          WHEN 'Important' THEN 3 
                                          ELSE 4 
                                      END,
                                      CreatedAt DESC";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetByUser Error: " + ex.Message);
            }
            return dt;
        }

        // =====================================================
        // GET DONOR NOTIFICATIONS (By DonorID)
        // =====================================================
        public static DataTable GetDonorNotifications(int donorID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string getUserSql = "SELECT UserID FROM Donors WHERE DonorID = @DonorID";
                    var getUserCmd = new SqlCommand(getUserSql, conn);
                    getUserCmd.Parameters.AddWithValue("@DonorID", donorID);
                    object result = getUserCmd.ExecuteScalar();

                    if (result != null)
                    {
                        int userID = Convert.ToInt32(result);
                        dt = GetByUser(userID);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetDonorNotifications Error: " + ex.Message);
            }
            return dt;
        }

        // =====================================================
        // GET UNREAD NOTIFICATIONS
        // =====================================================
        public static DataTable GetUnreadByUser(int userID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT TOP 20 NotificationID, Title, Message, Type, Priority, CreatedAt, ActionUrl, RelatedID, RelatedType
                                  FROM Notifications 
                                  WHERE UserID = @UserID AND IsRead = 0 
                                  ORDER BY 
                                      CASE Priority 
                                          WHEN 'Emergency' THEN 1 
                                          WHEN 'Urgent' THEN 2 
                                          WHEN 'Important' THEN 3 
                                          ELSE 4 
                                      END,
                                      CreatedAt DESC";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetUnreadByUser Error: " + ex.Message);
            }
            return dt;
        }

        // =====================================================
        // GET UNREAD COUNT
        // =====================================================
        public static int GetUnreadCount(int userID)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetUnreadCount Error: " + ex.Message);
                return 0;
            }
        }

        // =====================================================
        // INSERT NOTIFICATION
        // =====================================================
        public static bool Insert(Notification notification)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO Notifications 
                                  (UserID, Title, Message, Type, Priority, IsRead, CreatedAt, 
                                   ActionUrl, RelatedID, RelatedType, SentBy)
                                  VALUES 
                                  (@UserID, @Title, @Message, @Type, @Priority, @IsRead, @CreatedAt,
                                   @ActionUrl, @RelatedID, @RelatedType, @SentBy)";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserID", notification.UserID);
                    cmd.Parameters.AddWithValue("@Title", notification.Title ?? "");
                    cmd.Parameters.AddWithValue("@Message", notification.Message ?? "");
                    cmd.Parameters.AddWithValue("@Type", notification.Type ?? "General");
                    cmd.Parameters.AddWithValue("@Priority", notification.Priority ?? "Normal");
                    cmd.Parameters.AddWithValue("@IsRead", notification.IsRead);
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ActionUrl", notification.ActionUrl ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelatedID", notification.RelatedID > 0 ? (object)notification.RelatedID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelatedType", notification.RelatedType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SentBy", notification.SentBy ?? SessionManager.CurrentUsername);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Insert Error: " + ex.Message);
                return false;
            }
        }

        // =====================================================
        // INSERT BULK NOTIFICATIONS
        // =====================================================
        public static int InsertBulk(DataTable recipients, string title, string message, string type, string priority, string sentBy)
        {
            int inserted = 0;
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string sql = @"INSERT INTO Notifications 
                                          (UserID, Title, Message, Type, Priority, IsRead, CreatedAt, SentBy)
                                          VALUES 
                                          (@UserID, @Title, @Message, @Type, @Priority, 0, @CreatedAt, @SentBy)";

                            foreach (DataRow row in recipients.Rows)
                            {
                                int userId = Convert.ToInt32(row["UserID"]);
                                var cmd = new SqlCommand(sql, conn, transaction);
                                cmd.Parameters.AddWithValue("@UserID", userId);
                                cmd.Parameters.AddWithValue("@Title", title);
                                cmd.Parameters.AddWithValue("@Message", message);
                                cmd.Parameters.AddWithValue("@Type", type);
                                cmd.Parameters.AddWithValue("@Priority", priority);
                                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                                cmd.Parameters.AddWithValue("@SentBy", sentBy);

                                if (cmd.ExecuteNonQuery() > 0)
                                    inserted++;
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("InsertBulk Error: " + ex.Message);
            }
            return inserted;
        }

        // =====================================================
        // MARK AS READ
        // =====================================================
        public static bool MarkAsRead(int notificationID)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE Notifications SET IsRead = 1, ReadAt = @ReadAt WHERE NotificationID = @NotificationID";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ReadAt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@NotificationID", notificationID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MarkAsRead Error: " + ex.Message);
                return false;
            }
        }

        // =====================================================
        // MARK ALL AS READ
        // =====================================================
        public static bool MarkAllAsRead(int userID)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE Notifications SET IsRead = 1, ReadAt = @ReadAt WHERE UserID = @UserID AND IsRead = 0";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ReadAt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MarkAllAsRead Error: " + ex.Message);
                return false;
            }
        }

        // =====================================================
        // DELETE NOTIFICATION
        // =====================================================
        public static bool Delete(int notificationID)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "DELETE FROM Notifications WHERE NotificationID = @NotificationID";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@NotificationID", notificationID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Delete Error: " + ex.Message);
                return false;
            }
        }

        // =====================================================
        // DELETE ALL BY USER (Clear All Notifications)
        // =====================================================
        public static bool DeleteAllByUser(int userID)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "DELETE FROM Notifications WHERE UserID = @UserID";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeleteAllByUser Error: " + ex.Message);
                return false;
            }
        }

        // =====================================================
        // CLEAR ALL NOTIFICATIONS (Alias for DeleteAllByUser)
        // =====================================================
        public static bool ClearAllNotifications(int userID)
        {
            return DeleteAllByUser(userID);
        }

        // =====================================================
        // BROADCAST TO ROLE
        // =====================================================
        public static int BroadcastToRole(string role, string title, string message, string priority)
        {
            DataTable users = AdminDAL.GetUsersByRole(role);
            if (users == null || users.Rows.Count == 0)
                return 0;
            return InsertBulk(users, title, message, "Broadcast", priority, SessionManager.CurrentUsername);
        }

        // =====================================================
        // SEND BLOOD REQUEST ALERT
        // =====================================================
        public static int SendBloodRequestAlert(string bloodGroup, int unitsNeeded, string hospital, string message = "")
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string getDonorsSql = @"SELECT u.UserID, u.FullName 
                                           FROM Donors d
                                           INNER JOIN Users u ON d.UserID = u.UserID
                                           WHERE d.BloodGroup = @BloodGroup AND d.IsActive = 1 AND u.IsActive = 1";
                    var getDonorsCmd = new SqlCommand(getDonorsSql, conn);
                    getDonorsCmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                    var reader = getDonorsCmd.ExecuteReader();

                    var users = new DataTable();
                    users.Columns.Add("UserID");
                    users.Columns.Add("FullName");

                    while (reader.Read())
                    {
                        users.Rows.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                    reader.Close();

                    if (users.Rows.Count == 0)
                        return 0;

                    string title = $"🚨 URGENT: {bloodGroup} Blood Needed";
                    string fullMessage = string.IsNullOrEmpty(message)
                        ? $"URGENT: {unitsNeeded} unit(s) of {bloodGroup} blood needed immediately at {hospital}. Please donate if eligible."
                        : message;

                    return InsertBulk(users, title, fullMessage, "Emergency", "Emergency", SessionManager.CurrentUsername);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendBloodRequestAlert Error: " + ex.Message);
                return 0;
            }
        }

        // =====================================================
        // DELETE OLD NOTIFICATIONS
        // =====================================================
        public static int DeleteOldNotifications(int daysToKeep = 30)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    DateTime cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                    string sql = "DELETE FROM Notifications WHERE CreatedAt < @CutoffDate AND IsRead = 1";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@CutoffDate", cutoffDate);
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DeleteOldNotifications Error: " + ex.Message);
                return 0;
            }
        }
    }
}