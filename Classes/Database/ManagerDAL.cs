using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class ManagerDAL
    {
        // Insert manager
        public static bool Insert(Manager manager)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Managers 
                    (UserID, FullName, Designation, Department, Phone, Email, 
                     EmployeeID, JoiningDate, YearsOfExperience, Qualification, 
                     Specialization, IsActive, ReportsTo, OfficeLocation)
                    VALUES 
                    (@UserID, @FullName, @Designation, @Department, @Phone, @Email,
                     @EmployeeID, @JoiningDate, @YearsOfExperience, @Qualification,
                     @Specialization, @IsActive, @ReportsTo, @OfficeLocation)";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", manager.UserID);
                cmd.Parameters.AddWithValue("@FullName", manager.FullName);
                cmd.Parameters.AddWithValue("@Designation", manager.Designation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Department", manager.Department ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", manager.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", manager.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EmployeeID", manager.EmployeeID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@JoiningDate", manager.JoiningDate);
                cmd.Parameters.AddWithValue("@YearsOfExperience", manager.YearsOfExperience);
                cmd.Parameters.AddWithValue("@Qualification", manager.Qualification ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Specialization", manager.Specialization ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", manager.IsActive);
                cmd.Parameters.AddWithValue("@ReportsTo", manager.ReportsTo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@OfficeLocation", manager.OfficeLocation ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get all managers
        public static DataTable GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Managers WHERE IsActive = 1";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get manager by UserID
        public static Manager GetByUserID(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Managers WHERE UserID = @UserID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return MapToManager(reader);
                }
                return null;
            }
        }

        // Update manager
        public static bool Update(Manager manager)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Managers SET 
                    FullName = @FullName,
                    Designation = @Designation,
                    Department = @Department,
                    Phone = @Phone,
                    Email = @Email,
                    Qualification = @Qualification,
                    Specialization = @Specialization
                    WHERE UserID = @UserID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", manager.UserID);
                cmd.Parameters.AddWithValue("@FullName", manager.FullName);
                cmd.Parameters.AddWithValue("@Designation", manager.Designation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Department", manager.Department ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", manager.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", manager.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Qualification", manager.Qualification ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Specialization", manager.Specialization ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get dashboard stats
        public static DataTable GetDashboardStats()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Metric");
            dt.Columns.Add("Value");

            dt.Rows.Add("Total Donors", CommonDAL.GetTotalDonors());
            dt.Rows.Add("Total Patients", CommonDAL.GetTotalPatients());
            dt.Rows.Add("Total Blood Bags", CommonDAL.GetTotalBloodBags());
            dt.Rows.Add("Pending Requisitions", CommonDAL.GetPendingRequisitions());

            return dt;
        }

        private static Manager MapToManager(SqlDataReader reader)
        {
            return new Manager
            {
                ManagerID = reader.GetInt32(0),
                UserID = reader.GetInt32(1),
                FullName = reader.GetString(2),
                Designation = reader.IsDBNull(3) ? null : reader.GetString(3),
                Department = reader.IsDBNull(4) ? null : reader.GetString(4),
                Phone = reader.IsDBNull(5) ? null : reader.GetString(5),
                Email = reader.IsDBNull(6) ? null : reader.GetString(6),
                EmployeeID = reader.IsDBNull(7) ? null : reader.GetString(7),
                JoiningDate = reader.GetDateTime(8),
                YearsOfExperience = reader.GetInt32(9),
                Qualification = reader.IsDBNull(10) ? null : reader.GetString(10),
                Specialization = reader.IsDBNull(11) ? null : reader.GetString(11),
                IsActive = reader.GetBoolean(12),
                ReportsTo = reader.IsDBNull(13) ? null : reader.GetString(13),
                OfficeLocation = reader.IsDBNull(14) ? null : reader.GetString(14)
            };
        }
    }
}