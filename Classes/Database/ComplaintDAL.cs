using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class ComplaintDAL
    {
        // Insert complaint
        public static bool Insert(Complaint complaint)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Complaints 
                               (UserID, UserName, ComplaintType, IssueDescription, 
                                FilePath, FileName, Status, CreatedAt)
                               VALUES (@UserID, @UserName, @ComplaintType, @IssueDescription, 
                                       @FilePath, @FileName, 'Pending', GETDATE())";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", complaint.UserID);
                cmd.Parameters.AddWithValue("@UserName", complaint.UserName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ComplaintType", complaint.ComplaintType);
                cmd.Parameters.AddWithValue("@IssueDescription", complaint.IssueDescription ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FilePath", complaint.FilePath ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FileName", complaint.FileName ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get all complaints (for Admin)
        public static DataTable GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT ComplaintID, UserName, ComplaintType, IssueDescription, 
                              Status, CreatedAt, FileName 
                              FROM Complaints 
                              ORDER BY CreatedAt DESC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get complaints by user
        public static DataTable GetByUser(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT ComplaintID, ComplaintType, IssueDescription, 
                              Status, CreatedAt, AdminResponse, FileName
                              FROM Complaints 
                              WHERE UserID = @UserID 
                              ORDER BY CreatedAt DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Update complaint status and response (for Admin)
        public static bool UpdateResponse(int complaintID, string response, string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Complaints 
                              SET AdminResponse = @Response, 
                                  Status = @Status, 
                                  ResolvedAt = CASE WHEN @Status = 'Resolved' THEN GETDATE() ELSE NULL END
                              WHERE ComplaintID = @ComplaintID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Response", response);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@ComplaintID", complaintID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Delete complaint
        public static bool Delete(int complaintID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Complaints WHERE ComplaintID = @ComplaintID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ComplaintID", complaintID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}