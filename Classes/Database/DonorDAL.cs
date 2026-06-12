using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class DonorDAL
    {
        // =========================================================
        // GET DONOR BY USER ID (For BookAppointment)
        // =========================================================
        public static Donor GetByUserID(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Donors WHERE UserID = @UserID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return MapToDonor(reader);
                }
                return null;
            }
        }

        // =========================================================
        // GET DONOR BY USER ID (Alias for GetByUserID)
        // =========================================================
        public static Donor GetDonorByUserID(int userID)
        {
            return GetByUserID(userID);
        }

        // =========================================================
        // SEARCH DONORS
        // =========================================================
        public static DataTable SearchDonors(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT DonorID, FullName, CNIC, BloodGroup, Phone, Email, TotalDonations
                              FROM Donors 
                              WHERE FullName LIKE @Search OR CNIC LIKE @Search OR Phone LIKE @Search";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // SEARCH (Alias for SearchDonors)
        // =========================================================
        public static DataTable Search(string searchTerm)
        {
            return SearchDonors(searchTerm);
        }

        // =========================================================
        // INSERT DONOR
        // =========================================================
        public static bool InsertDonor(Donor donor)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Donors 
                    (UserID, FullName, CNIC, BloodGroup, DateOfBirth, Age, Gender, Address, 
                     Phone, Email, Weight, IsActive, RegistrationDate, EmergencyContact, MedicalHistory)
                    VALUES 
                    (@UserID, @FullName, @CNIC, @BloodGroup, @DateOfBirth, @Age, @Gender, @Address,
                     @Phone, @Email, @Weight, @IsActive, @RegistrationDate, @EmergencyContact, @MedicalHistory)";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", donor.UserID);
                cmd.Parameters.AddWithValue("@FullName", donor.FullName);
                cmd.Parameters.AddWithValue("@CNIC", donor.CNIC);
                cmd.Parameters.AddWithValue("@BloodGroup", donor.BloodGroup ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfBirth", donor.DateOfBirth);
                cmd.Parameters.AddWithValue("@Age", donor.Age);
                cmd.Parameters.AddWithValue("@Gender", donor.Gender ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", donor.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", donor.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", donor.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Weight", donor.Weight);
                cmd.Parameters.AddWithValue("@IsActive", donor.IsActive);
                cmd.Parameters.AddWithValue("@RegistrationDate", donor.RegistrationDate);
                cmd.Parameters.AddWithValue("@EmergencyContact", donor.EmergencyContact ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MedicalHistory", donor.MedicalHistory ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // INSERT (Alias for InsertDonor)
        // =========================================================
        public static bool Insert(Donor donor) => InsertDonor(donor);

        // =========================================================
        // GET ALL DONORS
        // =========================================================
        public static DataTable GetAllDonors()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT DonorID, FullName, CNIC, BloodGroup, Age, Gender, 
                              Phone, Email, TotalDonations, LastDonationDate, IsActive
                              FROM Donors ORDER BY DonorID DESC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET ALL (Alias for GetAllDonors)
        // =========================================================
        public static DataTable GetAll() => GetAllDonors();

        // =========================================================
        // GET DONOR BY ID
        // =========================================================
        public static Donor GetDonorByID(int donorID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Donors WHERE DonorID = @DonorID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DonorID", donorID);
                var reader = cmd.ExecuteReader();
                if (reader.Read()) return MapToDonor(reader);
                return null;
            }
        }

        // =========================================================
        // GET BY ID (Alias for GetDonorByID)
        // =========================================================
        public static Donor GetByID(int donorID) => GetDonorByID(donorID);

        // =========================================================
        // UPDATE DONOR
        // =========================================================
        public static bool UpdateDonor(Donor donor)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Donors SET 
                    FullName = @FullName,
                    BloodGroup = @BloodGroup,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    Weight = @Weight
                    WHERE DonorID = @DonorID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DonorID", donor.DonorID);
                cmd.Parameters.AddWithValue("@FullName", donor.FullName);
                cmd.Parameters.AddWithValue("@BloodGroup", donor.BloodGroup ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", donor.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", donor.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", donor.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Weight", donor.Weight);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // UPDATE (Alias for UpdateDonor)
        // =========================================================
        public static bool Update(Donor donor) => UpdateDonor(donor);

        // =========================================================
        // UPDATE DONATION COUNT
        // =========================================================
        public static bool UpdateDonationCount(int donorID, int units)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Donors SET 
                    TotalDonations = TotalDonations + 1,
                    LastDonationDate = @LastDonationDate
                    WHERE DonorID = @DonorID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LastDonationDate", DateTime.Today);
                cmd.Parameters.AddWithValue("@DonorID", donorID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // GET DONORS BY BLOOD GROUP
        // =========================================================
        public static DataTable GetDonorsByBloodGroup(string bloodGroup)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT DonorID, FullName, Phone, Email, LastDonationDate FROM Donors WHERE BloodGroup = @BloodGroup AND IsActive = 1";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET ELIGIBLE DONORS
        // =========================================================
        public static DataTable GetEligibleDonors()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT DonorID, FullName, BloodGroup, Phone, Email
                              FROM Donors 
                              WHERE IsActive = 1 
                              AND (LastDonationDate IS NULL OR LastDonationDate <= DATEADD(DAY, -90, GETDATE()))";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET DONOR STATISTICS
        // =========================================================
        public static DataTable GetDonorStatistics()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT BloodGroup, COUNT(*) as TotalDonors,
                                      SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveDonors
                               FROM Donors GROUP BY BloodGroup";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // DELETE DONOR (Soft Delete)
        // =========================================================
        public static bool DeleteDonor(int donorID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Donors SET IsActive = 0 WHERE DonorID = @DonorID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DonorID", donorID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // UPDATE DONOR PROFILE
        // =========================================================
        public static bool UpdateDonorProfile(int userID, string fullName, string email, string phone, string address)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Users 
                               SET FullName = @name, Email = @email, Phone = @phone
                               WHERE UserID = @id;
                               UPDATE Donors 
                               SET Address = @address 
                               WHERE UserID = @id";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", fullName);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@phone", phone);
                cmd.Parameters.AddWithValue("@address", address);
                cmd.Parameters.AddWithValue("@id", userID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // GET DONOR INFO AS SQLDATA READER
        // =========================================================
        public static SqlDataReader GetDonorByUserIDReader(int userID)
        {
            var conn = DatabaseHelper.GetConnection();
            conn.Open();
            string sql = @"SELECT u.FullName, u.Email, u.Phone, 
                                  d.BloodGroup, d.DOB, d.Gender, d.Address
                           FROM Users u
                           LEFT JOIN Donors d ON u.UserID = d.UserID
                           WHERE u.UserID = @id";
            var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userID);
            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }

        // =========================================================
        // GET DONOR BY CNIC
        // =========================================================
        public static Donor GetDonorByCNIC(string cnic)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Donors WHERE CNIC = @CNIC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CNIC", cnic);
                var reader = cmd.ExecuteReader();
                if (reader.Read()) return MapToDonor(reader);
                return null;
            }
        }

        // =========================================================
        // MAPPING HELPER
        // =========================================================
        private static Donor MapToDonor(SqlDataReader reader)
        {
            Donor donor = new Donor();

            donor.DonorID = reader.GetInt32(reader.GetOrdinal("DonorID"));
            donor.UserID = reader.GetInt32(reader.GetOrdinal("UserID"));
            donor.FullName = reader.GetString(reader.GetOrdinal("FullName"));
            donor.CNIC = reader.IsDBNull(reader.GetOrdinal("CNIC")) ? "" : reader.GetString(reader.GetOrdinal("CNIC"));
            donor.BloodGroup = reader.IsDBNull(reader.GetOrdinal("BloodGroup")) ? null : reader.GetString(reader.GetOrdinal("BloodGroup"));
            donor.DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("DateOfBirth"));
            donor.Age = reader.IsDBNull(reader.GetOrdinal("Age")) ? 0 : reader.GetInt32(reader.GetOrdinal("Age"));
            donor.Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : reader.GetString(reader.GetOrdinal("Gender"));
            donor.Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address"));
            donor.Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone"));
            donor.Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email"));
            donor.Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? 0 : reader.GetInt32(reader.GetOrdinal("Weight"));

            // Handle nullable DateTime
            int lastDonationIndex = reader.GetOrdinal("LastDonationDate");
            donor.LastDonationDate = reader.IsDBNull(lastDonationIndex) ? (DateTime?)null : reader.GetDateTime(lastDonationIndex);

            donor.TotalDonations = reader.IsDBNull(reader.GetOrdinal("TotalDonations")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalDonations"));
            donor.IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
            donor.RegistrationDate = reader.GetDateTime(reader.GetOrdinal("RegistrationDate"));

            // Optional fields
            if (ColumnExists(reader, "EmergencyContact"))
                donor.EmergencyContact = reader.IsDBNull(reader.GetOrdinal("EmergencyContact")) ? "" : reader.GetString(reader.GetOrdinal("EmergencyContact"));

            if (ColumnExists(reader, "MedicalHistory"))
                donor.MedicalHistory = reader.IsDBNull(reader.GetOrdinal("MedicalHistory")) ? "" : reader.GetString(reader.GetOrdinal("MedicalHistory"));

            return donor;
        }

        private static bool ColumnExists(SqlDataReader reader, string columnName)
        {
            try
            {
                return reader.GetOrdinal(columnName) >= 0;
            }
            catch
            {
                return false;
            }
        }
    }
}