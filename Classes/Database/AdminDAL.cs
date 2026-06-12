using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Enums;
using BloodBankManagementSystem.Classes.Models;
using BloodBankManagementSystem.Classes.Helpers;

namespace BloodBankManagementSystem.Classes.Database
{
    public class AdminDAL
    {
        // Get user by username - FIXED
        public static User GetUserByUsername(string username)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT UserID, FullName, Username, PasswordHash, Role, " +
                    "Email, Phone, IsActive FROM Users " +
                    "WHERE Username = @u", conn);
                cmd.Parameters.AddWithValue("@u", username);
                var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    string roleStr = r.GetString(4);
                    UserRole role;

                    // Try to parse role, default to Donor if invalid
                    if (!Enum.TryParse(roleStr, true, out role))
                    {
                        role = UserRole.Donor;
                    }

                    return new User
                    {
                        UserID = r.GetInt32(0),
                        FullName = r.GetString(1),
                        Username = r.GetString(2),
                        PasswordHash = r.GetString(3),
                        Role = role,
                        Email = r.IsDBNull(5) ? "" : r.GetString(5),
                        Phone = r.IsDBNull(6) ? "" : r.GetString(6),
                        IsActive = r.GetBoolean(7)
                    };
                }
                return null;
            }
        }

        // Get user by ID
        public static User GetUserByID(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT UserID, FullName, Username, PasswordHash, Role, " +
                    "Email, Phone, IsActive FROM Users WHERE UserID = @id", conn);
                cmd.Parameters.AddWithValue("@id", userID);
                var r = cmd.ExecuteReader();

                if (r.Read())
                {
                    string roleStr = r.GetString(4);
                    UserRole role;
                    if (!Enum.TryParse(roleStr, true, out role))
                    {
                        role = UserRole.Donor;
                    }

                    return new User
                    {
                        UserID = r.GetInt32(0),
                        FullName = r.GetString(1),
                        Username = r.GetString(2),
                        PasswordHash = r.GetString(3),
                        Role = role,
                        Email = r.IsDBNull(5) ? "" : r.GetString(5),
                        Phone = r.IsDBNull(6) ? "" : r.GetString(6),
                        IsActive = r.GetBoolean(7)
                    };
                }
                return null;
            }
        }

        // Get all users
        public static DataTable GetAllUsers()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT UserID, FullName, Username, Role, Email, Phone, IsActive, CreatedAt " +
                    "FROM Users ORDER BY CreatedAt DESC", conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Insert new user
        public static bool InsertUser(User user, string password)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string hashedPassword = EncryptionHelper.HashPassword(password);

                var cmd = new SqlCommand(
                    "INSERT INTO Users (FullName, Username, PasswordHash, Role, Email, Phone, IsActive, CreatedAt) " +
                    "VALUES (@FullName, @Username, @PasswordHash, @Role, @Email, @Phone, @IsActive, @CreatedAt); SELECT SCOPE_IDENTITY();", conn);

                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@Role", user.Role.ToString());
                cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", user.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    user.UserID = Convert.ToInt32(result);
                    return true;
                }
                return false;
            }
        }

        // Update user
        public static bool UpdateUser(User user)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "UPDATE Users SET FullName = @FullName, Email = @Email, Phone = @Phone, " +
                    "IsActive = @IsActive, Role = @Role WHERE UserID = @UserID", conn);

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
                string hashedPassword = EncryptionHelper.HashPassword(newPassword);

                var cmd = new SqlCommand(
                    "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID", conn);

                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@UserID", userID);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Delete user (soft delete)
        public static bool DeleteUser(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "UPDATE Users SET IsActive = 0 WHERE UserID = @UserID", conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get users by role
        public static DataTable GetUsersByRole(string role)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT UserID, FullName, Username, Email, Phone FROM Users " +
                    "WHERE Role = @Role AND IsActive = 1", conn);
                cmd.Parameters.AddWithValue("@Role", role);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Search users
        public static DataTable SearchUsers(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT UserID, FullName, Username, Role, Email, Phone, IsActive " +
                    "FROM Users WHERE FullName LIKE @Search OR Username LIKE @Search OR Email LIKE @Search",
                    conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get user count by role
        public static int GetUserCountByRole(string role)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE Role = @Role AND IsActive = 1", conn);
                cmd.Parameters.AddWithValue("@Role", role);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Get total user count
        public static int GetTotalUserCount()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Users WHERE IsActive = 1", conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Update last login
        public static bool UpdateLastLogin(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "UPDATE Users SET LastLogin = @LastLogin WHERE UserID = @UserID", conn);
                cmd.Parameters.AddWithValue("@LastLogin", DateTime.Now);
                cmd.Parameters.AddWithValue("@UserID", userID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}