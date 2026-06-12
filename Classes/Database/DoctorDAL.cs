using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class DoctorDAL
    {
        // Insert doctor
        public static bool Insert(Doctor doctor)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Doctors 
                    (UserID, FullName, Specialization, PMDCNumber, Hospital, Department, 
                     Designation, Phone, Email, Qualification, Experience, JoiningDate, IsActive)
                    VALUES 
                    (@UserID, @FullName, @Specialization, @PMDCNumber, @Hospital, @Department,
                     @Designation, @Phone, @Email, @Qualification, @Experience, @JoiningDate, @IsActive)";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", doctor.UserID);
                cmd.Parameters.AddWithValue("@FullName", doctor.FullName);
                cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PMDCNumber", doctor.PMDCNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Hospital", doctor.Hospital ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Department", doctor.Department ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Designation", doctor.Designation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", doctor.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", doctor.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Qualification", doctor.Qualification ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Experience", doctor.Experience);
                cmd.Parameters.AddWithValue("@JoiningDate", doctor.JoiningDate);
                cmd.Parameters.AddWithValue("@IsActive", doctor.IsActive);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get all doctors
        public static DataTable GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Doctors WHERE IsActive = 1";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get doctor by UserID
        public static Doctor GetByUserID(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Doctors WHERE UserID = @UserID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return MapToDoctor(reader);
                }
                return null;
            }
        }

        // Update doctor
        public static bool Update(Doctor doctor)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Doctors SET 
                    FullName = @FullName,
                    Specialization = @Specialization,
                    PMDCNumber = @PMDCNumber,
                    Hospital = @Hospital,
                    Department = @Department,
                    Designation = @Designation,
                    Phone = @Phone,
                    Email = @Email,
                    Qualification = @Qualification,
                    Experience = @Experience
                    WHERE UserID = @UserID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", doctor.UserID);
                cmd.Parameters.AddWithValue("@FullName", doctor.FullName);
                cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PMDCNumber", doctor.PMDCNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Hospital", doctor.Hospital ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Department", doctor.Department ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Designation", doctor.Designation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", doctor.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", doctor.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Qualification", doctor.Qualification ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Experience", doctor.Experience);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Search doctors
        public static DataTable Search(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT * FROM Doctors 
                              WHERE FullName LIKE @Search 
                              OR Specialization LIKE @Search 
                              OR Hospital LIKE @Search";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get requisitions by doctor
        public static DataTable GetRequisitionsByDoctor(int doctorID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Requisitions WHERE DoctorID = @DoctorID ORDER BY RequestDate DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DoctorID", doctorID);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private static Doctor MapToDoctor(SqlDataReader reader)
        {
            return new Doctor
            {
                DoctorID = reader.GetInt32(0),
                UserID = reader.GetInt32(1),
                FullName = reader.GetString(2),
                Specialization = reader.IsDBNull(3) ? null : reader.GetString(3),
                PMDCNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                Hospital = reader.IsDBNull(5) ? null : reader.GetString(5),
                Department = reader.IsDBNull(6) ? null : reader.GetString(6),
                Designation = reader.IsDBNull(7) ? null : reader.GetString(7),
                Phone = reader.IsDBNull(8) ? null : reader.GetString(8),
                Email = reader.IsDBNull(9) ? null : reader.GetString(9),
                Qualification = reader.IsDBNull(10) ? null : reader.GetString(10),
                Experience = reader.GetInt32(11),
                JoiningDate = reader.GetDateTime(12),
                IsActive = reader.GetBoolean(13)
            };
        }
    }
}