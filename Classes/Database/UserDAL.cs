using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;
using BloodBankManagementSystem.Classes.Enums;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Classes.Database
{
    public class UserDAL
    {
        // Insert new user (with password)
        public static bool Insert(User user, string password)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string hashed = EncryptionHelper.HashPassword(password);
                string sql = @"INSERT INTO Users (FullName, Username, PasswordHash, Role, Email, Phone, IsActive, CreatedAt)
                               VALUES (@FullName, @Username, @PasswordHash, @Role, @Email, @Phone, @IsActive, @CreatedAt);
                               SELECT SCOPE_IDENTITY();";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", hashed);
                cmd.Parameters.AddWithValue("@Role", user.Role.ToString());
                cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    user.UserID = Convert.ToInt32(result);
                    return true;
                }
                return false;
            }
        }

        // Compatibility overload (for backward compatibility)
        public static bool Insert(User user)
        {
            const string defaultPassword = "ChangeMe@123";
            return Insert(user, defaultPassword);
        }

        // Get user by ID
        public static User GetByID(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT UserID, FullName, Username, PasswordHash, Role, Email, Phone, IsActive FROM Users WHERE UserID = @UserID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                var reader = cmd.ExecuteReader();
                if (reader.Read()) return MapToUser(reader);
                return null;
            }
        }

        // Get user by username
        public static User GetByUsername(string username)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT UserID, FullName, Username, PasswordHash, Role, Email, Phone, IsActive FROM Users WHERE Username = @Username";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", username);
                var reader = cmd.ExecuteReader();
                if (reader.Read()) return MapToUser(reader);
                return null;
            }
        }

        // Get all users
        public static DataTable GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT UserID, FullName, Username, Role, Email, Phone, IsActive, CreatedAt FROM Users";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get users by role
        public static DataTable GetByRole(string role)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT UserID, FullName, Username, Email, Phone FROM Users WHERE Role = @Role AND IsActive = 1";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Role", role);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Compatibility wrapper
        public static DataTable GetUsersByRole(string role)
        {
            return GetByRole(role);
        }

        // Update user
        public static bool Update(User user)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Users SET FullName = @FullName, Email = @Email, Phone = @Phone, IsActive = @IsActive, Role = @Role WHERE UserID = @UserID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", user.UserID);
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cmd.Parameters.AddWithValue("@Role", user.Role.ToString());
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Change password
        public static bool ChangePassword(int userID, string newPassword)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string hashed = EncryptionHelper.HashPassword(newPassword);
                string sql = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PasswordHash", hashed);
                cmd.Parameters.AddWithValue("@UserID", userID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Soft delete
        public static bool Delete(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Users SET IsActive = 0 WHERE UserID = @UserID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Search users
        public static DataTable Search(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT UserID, FullName, Username, Role, Email, Phone, IsActive FROM Users WHERE FullName LIKE @Search OR Username LIKE @Search OR Email LIKE @Search";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private static User MapToUser(SqlDataReader reader)
        {
            string roleStr = reader.GetString(4);
            UserRole role;
            if (!Enum.TryParse(roleStr, true, out role))
            {
                role = UserRole.Donor;
            }

            return new User
            {
                UserID = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Username = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Role = role,
                Email = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Phone = reader.IsDBNull(6) ? "" : reader.GetString(6),
                IsActive = reader.GetBoolean(7)
            };
        }
    }
}