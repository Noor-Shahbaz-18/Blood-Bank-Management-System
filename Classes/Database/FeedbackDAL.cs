using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class FeedbackDAL
    {
        // Insert feedback
        public static bool Insert(Feedback feedback)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Feedback 
                               (UserID, UserName, Rating, Comment, Status, CreatedAt)
                               VALUES (@UserID, @UserName, @Rating, @Comment, 'Pending', GETDATE())";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", feedback.UserID);
                cmd.Parameters.AddWithValue("@UserName", feedback.UserName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Rating", feedback.Rating);
                cmd.Parameters.AddWithValue("@Comment", feedback.Comment ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get all feedback (for Admin)
        public static DataTable GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT FeedbackID, UserName, Rating, Comment, Status, Response, CreatedAt FROM Feedback ORDER BY CreatedAt DESC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get feedback by user
        public static DataTable GetByUser(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT FeedbackID, Rating, Comment, Status, Response, CreatedAt FROM Feedback WHERE UserID = @UserID ORDER BY CreatedAt DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Update response (for Admin)
        public static bool UpdateResponse(int feedbackID, string response)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Feedback SET Response = @Response, Status = 'Resolved', RespondedAt = GETDATE() WHERE FeedbackID = @FeedbackID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Response", response);
                cmd.Parameters.AddWithValue("@FeedbackID", feedbackID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get average rating
        public static double GetAverageRating()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT AVG(CAST(Rating AS FLOAT)) FROM Feedback WHERE Status = 'Resolved'";
                var cmd = new SqlCommand(sql, conn);
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDouble(result);
            }
        }

        // Delete feedback
        public static bool Delete(int feedbackID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Feedback WHERE FeedbackID = @FeedbackID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FeedbackID", feedbackID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}