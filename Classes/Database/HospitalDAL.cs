using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class HospitalDAL
    {
        // Insert new hospital
        public static bool InsertHospital(string name, string city, string address, string phone, string email, string contactPerson)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Hospitals (Name, City, Address, Phone, Email, ContactPerson, IsActive, CreatedAt)
                               VALUES (@Name, @City, @Address, @Phone, @Email, @ContactPerson, 1, GETDATE())";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@City", city ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ContactPerson", contactPerson ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Update hospital
        public static bool UpdateHospital(int hospitalId, string name, string city, string address, string phone, string email, string contactPerson)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Hospitals SET 
                               Name = @Name,
                               City = @City,
                               Address = @Address,
                               Phone = @Phone,
                               Email = @Email,
                               ContactPerson = @ContactPerson
                               WHERE HospitalID = @HospitalID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@HospitalID", hospitalId);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@City", city ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ContactPerson", contactPerson ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get hospital by ID
        public static HospitalData GetHospitalByID(int hospitalId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Hospitals WHERE HospitalID = @HospitalID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@HospitalID", hospitalId);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new HospitalData
                    {
                        HospitalID = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        City = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Address = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Email = reader.IsDBNull(5) ? null : reader.GetString(5),
                        ContactPerson = reader.IsDBNull(6) ? null : reader.GetString(6),
                        IsActive = reader.GetBoolean(7)
                    };
                }
                return null;
            }
        }

        // Get all hospitals
        public static DataTable GetAllHospitals()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT HospitalID, Name, City, Phone, Email, ContactPerson, IsActive FROM Hospitals ORDER BY Name";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Delete hospital (soft delete)
        public static bool DeleteHospital(int hospitalId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Hospitals SET IsActive = 0 WHERE HospitalID = @HospitalID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@HospitalID", hospitalId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Search hospitals
        public static DataTable SearchHospitals(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT HospitalID, Name, City, Phone, Email, ContactPerson, IsActive 
                              FROM Hospitals 
                              WHERE Name LIKE @Search OR City LIKE @Search OR Phone LIKE @Search";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }

    // Hospital Data Model
    public class HospitalData
    {
        public int HospitalID { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public bool IsActive { get; set; }
    }
}