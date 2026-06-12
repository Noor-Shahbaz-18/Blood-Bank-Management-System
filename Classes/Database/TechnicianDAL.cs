using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class TechnicianDAL
    {
        // =========================================================
        // INSERT TECHNICIAN (Simplified)
        // =========================================================
        public static bool Insert(Technician technician)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Technicians 
                    (UserID, FullName, EmployeeID, Designation, Specialization, 
                     Qualification, Phone, Email, Shift, JoiningDate, IsActive)
                    VALUES 
                    (@UserID, @FullName, @EmployeeID, @Designation, @Specialization,
                     @Qualification, @Phone, @Email, @Shift, @JoiningDate, @IsActive);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", technician.UserID);
                    cmd.Parameters.AddWithValue("@FullName", technician.FullName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeID", technician.EmployeeID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", technician.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Specialization", technician.Specialization ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Qualification", technician.Qualification ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", technician.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", technician.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Shift", technician.Shift ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JoiningDate", technician.JoiningDate);
                    cmd.Parameters.AddWithValue("@IsActive", technician.IsActive);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        technician.TechnicianID = Convert.ToInt32(result);
                        return true;
                    }
                    return false;
                }
            }
        }

        // =========================================================
        // GET TECHNICIAN BY TECHNICIAN ID
        // =========================================================
        public static Technician GetByTechnicianID(int technicianID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT TechnicianID, UserID, FullName, EmployeeID, Designation, 
                                      Specialization, Qualification, Phone, Email, 
                                      Shift, JoiningDate, IsActive
                               FROM Technicians 
                               WHERE TechnicianID = @TechnicianID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TechnicianID", technicianID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToTechnician(reader);
                        }
                    }
                }
            }
            return null;
        }

        // =========================================================
        // GET ALL TECHNICIANS
        // =========================================================
        public static DataTable GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                                TechnicianID, 
                                FullName, 
                                EmployeeID,
                                Designation, 
                                Specialization, 
                                Qualification,
                                Phone,
                                Email,
                                Shift,
                                FORMAT(JoiningDate, 'dd-MMM-yyyy') as JoiningDate,
                                IsActive
                             FROM Technicians 
                             ORDER BY FullName";

                using (var cmd = new SqlCommand(sql, conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        // =========================================================
        // GET ACTIVE TECHNICIANS
        // =========================================================
        public static DataTable GetActiveTechnicians()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                                TechnicianID, 
                                FullName, 
                                Designation, 
                                Specialization
                             FROM Technicians 
                             WHERE IsActive = 1
                             ORDER BY FullName";

                using (var cmd = new SqlCommand(sql, conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        // =========================================================
        // GET TECHNICIAN BY USER ID
        // =========================================================
        public static Technician GetByUserID(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT TechnicianID, UserID, FullName, EmployeeID, Designation, 
                                      Specialization, Qualification, Phone, Email, 
                                      Shift, JoiningDate, IsActive
                               FROM Technicians 
                               WHERE UserID = @UserID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapToTechnician(reader);
                        }
                    }
                }
                return null;
            }
        }

        // =========================================================
        // UPDATE TECHNICIAN (Simplified)
        // =========================================================
        public static bool Update(Technician technician)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Technicians SET 
                    FullName = @FullName,
                    EmployeeID = @EmployeeID,
                    Designation = @Designation,
                    Specialization = @Specialization,
                    Qualification = @Qualification,
                    Phone = @Phone,
                    Email = @Email,
                    Shift = @Shift,
                    IsActive = @IsActive
                    WHERE TechnicianID = @TechnicianID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TechnicianID", technician.TechnicianID);
                    cmd.Parameters.AddWithValue("@FullName", technician.FullName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeID", technician.EmployeeID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Designation", technician.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Specialization", technician.Specialization ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Qualification", technician.Qualification ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Phone", technician.Phone ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", technician.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Shift", technician.Shift ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsActive", technician.IsActive);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // =========================================================
        // DELETE / DEACTIVATE TECHNICIAN (Soft Delete)
        // =========================================================
        public static bool DeleteTechnician(int technicianID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Technicians SET IsActive = 0 WHERE TechnicianID = @TechnicianID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TechnicianID", technicianID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // =========================================================
        // ACTIVATE TECHNICIAN
        // =========================================================
        public static bool Activate(int technicianID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Technicians SET IsActive = 1 WHERE TechnicianID = @TechnicianID";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TechnicianID", technicianID);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // =========================================================
        // SEARCH TECHNICIANS
        // =========================================================
        public static DataTable Search(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                                TechnicianID, 
                                FullName, 
                                Designation, 
                                Specialization, 
                                Phone, 
                                Email,
                                IsActive
                             FROM Technicians 
                             WHERE FullName LIKE @Search 
                                OR Designation LIKE @Search 
                                OR Specialization LIKE @Search
                                OR Phone LIKE @Search
                             ORDER BY FullName";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        // =========================================================
        // GET TECHNICIAN STATISTICS
        // =========================================================
        public static (int total, int active) GetStatistics()
        {
            int total = 0, active = 0;

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                                COUNT(*) as Total,
                                SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as Active
                              FROM Technicians";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        total = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
                        active = reader["Active"] != DBNull.Value ? Convert.ToInt32(reader["Active"]) : 0;
                    }
                }
            }

            return (total, active);
        }

        // =========================================================
        // MAP TO TECHNICIAN MODEL
        // =========================================================
        private static Technician MapToTechnician(SqlDataReader reader)
        {
            return new Technician
            {
                TechnicianID = reader.GetInt32(reader.GetOrdinal("TechnicianID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? "" : reader.GetString(reader.GetOrdinal("FullName")),
                EmployeeID = reader.IsDBNull(reader.GetOrdinal("EmployeeID")) ? null : reader.GetString(reader.GetOrdinal("EmployeeID")),
                Designation = reader.IsDBNull(reader.GetOrdinal("Designation")) ? null : reader.GetString(reader.GetOrdinal("Designation")),
                Specialization = reader.IsDBNull(reader.GetOrdinal("Specialization")) ? null : reader.GetString(reader.GetOrdinal("Specialization")),
                Qualification = reader.IsDBNull(reader.GetOrdinal("Qualification")) ? null : reader.GetString(reader.GetOrdinal("Qualification")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Shift = reader.IsDBNull(reader.GetOrdinal("Shift")) ? null : reader.GetString(reader.GetOrdinal("Shift")),
                JoiningDate = reader.GetDateTime(reader.GetOrdinal("JoiningDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }
    }
}