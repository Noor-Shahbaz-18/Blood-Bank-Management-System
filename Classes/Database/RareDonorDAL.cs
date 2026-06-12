using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class RareDonorDAL
    {
        // =========================================================
        // INSERT NEW RARE DONOR
        // =========================================================
        public static bool Insert(RareDonor donor)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // First check if table exists, if not create it
                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RareDonors' AND xtype='U')
                        CREATE TABLE RareDonors (
                            DonorID INT IDENTITY(1,1) PRIMARY KEY,
                            FullName NVARCHAR(200) NOT NULL,
                            CNIC NVARCHAR(20) NOT NULL,
                            BloodGroup NVARCHAR(10),
                            RareBloodType NVARCHAR(100) NOT NULL,
                            Phone NVARCHAR(20) NOT NULL,
                            Email NVARCHAR(100),
                            Address NVARCHAR(500),
                            City NVARCHAR(100),
                            Gender NVARCHAR(10),
                            DateOfBirth DATETIME,
                            Age INT,
                            Weight INT,
                            MedicalHistory NVARCHAR(MAX),
                            Notes NVARCHAR(MAX),
                            Status NVARCHAR(50) DEFAULT 'Active',
                            CreatedBy INT,
                            CreatedAt DATETIME DEFAULT GETDATE(),
                            LastDonationDate DATETIME,
                            TotalDonations INT DEFAULT 0
                        )";

                    using (var cmd = new SqlCommand(createTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Now insert the donor
                    string sql = @"INSERT INTO RareDonors 
                                  (FullName, CNIC, BloodGroup, RareBloodType, Phone, Email, 
                                   Address, City, Gender, DateOfBirth, Age, Weight, MedicalHistory, Notes, Status, CreatedBy, CreatedAt)
                                  VALUES 
                                  (@FullName, @CNIC, @BloodGroup, @RareBloodType, @Phone, @Email,
                                   @Address, @City, @Gender, @DateOfBirth, @Age, @Weight, @MedicalHistory, @Notes, @Status, @CreatedBy, @CreatedAt);
                                  SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", donor.FullName ?? "");
                        cmd.Parameters.AddWithValue("@CNIC", donor.CNIC ?? "");
                        cmd.Parameters.AddWithValue("@BloodGroup", donor.BloodGroup ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RareBloodType", donor.RareBloodType ?? "");
                        cmd.Parameters.AddWithValue("@Phone", donor.Phone ?? "");
                        cmd.Parameters.AddWithValue("@Email", donor.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", donor.Address ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@City", donor.City ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", donor.Gender ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DateOfBirth", donor.DateOfBirth);
                        cmd.Parameters.AddWithValue("@Age", donor.Age);
                        cmd.Parameters.AddWithValue("@Weight", donor.Weight);
                        cmd.Parameters.AddWithValue("@MedicalHistory", donor.MedicalHistory ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", donor.Notes ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", donor.Status ?? "Active");
                        cmd.Parameters.AddWithValue("@CreatedBy", donor.CreatedBy);
                        cmd.Parameters.AddWithValue("@CreatedAt", donor.CreatedAt);

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int newId = Convert.ToInt32(result);
                            return newId > 0;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RareDonorDAL.Insert Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // =========================================================
        // UPDATE RARE DONOR
        // =========================================================
        public static bool Update(RareDonor donor)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"UPDATE RareDonors SET 
                                    FullName = @FullName,
                                    CNIC = @CNIC,
                                    BloodGroup = @BloodGroup,
                                    RareBloodType = @RareBloodType,
                                    Phone = @Phone,
                                    Email = @Email,
                                    Address = @Address,
                                    City = @City,
                                    Gender = @Gender,
                                    DateOfBirth = @DateOfBirth,
                                    Age = @Age,
                                    Weight = @Weight,
                                    MedicalHistory = @MedicalHistory,
                                    Notes = @Notes,
                                    Status = @Status
                                  WHERE DonorID = @DonorID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", donor.DonorID);
                        cmd.Parameters.AddWithValue("@FullName", donor.FullName ?? "");
                        cmd.Parameters.AddWithValue("@CNIC", donor.CNIC ?? "");
                        cmd.Parameters.AddWithValue("@BloodGroup", donor.BloodGroup ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RareBloodType", donor.RareBloodType ?? "");
                        cmd.Parameters.AddWithValue("@Phone", donor.Phone ?? "");
                        cmd.Parameters.AddWithValue("@Email", donor.Email ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", donor.Address ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@City", donor.City ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", donor.Gender ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DateOfBirth", donor.DateOfBirth);
                        cmd.Parameters.AddWithValue("@Age", donor.Age);
                        cmd.Parameters.AddWithValue("@Weight", donor.Weight);
                        cmd.Parameters.AddWithValue("@MedicalHistory", donor.MedicalHistory ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", donor.Notes ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", donor.Status ?? "Active");

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RareDonorDAL.Update Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        // =========================================================
        // CHECK IF CNIC EXISTS
        // =========================================================
        public static bool IsCNICExists(string cnic)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM RareDonors WHERE CNIC = @CNIC";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CNIC", cnic);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IsCNICExists Error: {ex.Message}");
                return false;
            }
        }

        // =========================================================
        // GET ALL RARE DONORS
        // =========================================================
        public static DataTable GetAllRareDonors()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT 
                                    DonorID,
                                    FullName,
                                    CNIC,
                                    BloodGroup,
                                    RareBloodType,
                                    Phone,
                                    Email,
                                    City,
                                    Status,
                                    LastDonationDate,
                                    TotalDonations,
                                    FORMAT(CreatedAt, 'dd-MMM-yyyy') as CreatedDate
                                  FROM RareDonors 
                                  ORDER BY CreatedAt DESC";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllRareDonors Error: {ex.Message}");
            }
            return dt;
        }

        // =========================================================
        // GET RARE DONOR BY ID
        // =========================================================
        public static RareDonor GetByID(int donorId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM RareDonors WHERE DonorID = @DonorID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", donorId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapToRareDonor(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByID Error: {ex.Message}");
            }
            return null;
        }

        // =========================================================
        // GET RARE DONORS BY BLOOD GROUP
        // =========================================================
        public static DataTable GetByBloodGroup(string bloodGroup)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT 
                                    DonorID, 
                                    FullName, 
                                    Phone, 
                                    Email, 
                                    City, 
                                    RareBloodType
                                  FROM RareDonors 
                                  WHERE BloodGroup = @BloodGroup AND Status = 'Active'";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByBloodGroup Error: {ex.Message}");
            }
            return dt;
        }

        // =========================================================
        // GET RARE DONORS BY RARE TYPE
        // =========================================================
        public static DataTable GetByRareType(string rareType)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT 
                                    DonorID, 
                                    FullName, 
                                    BloodGroup, 
                                    Phone, 
                                    Email, 
                                    City
                                  FROM RareDonors 
                                  WHERE RareBloodType LIKE @RareType AND Status = 'Active'";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RareType", "%" + rareType + "%");
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByRareType Error: {ex.Message}");
            }
            return dt;
        }

        // =========================================================
        // UPDATE DONOR STATUS
        // =========================================================
        public static bool UpdateStatus(int donorId, string status)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE RareDonors SET Status = @Status WHERE DonorID = @DonorID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@DonorID", donorId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatus Error: {ex.Message}");
                return false;
            }
        }

        // =========================================================
        // UPDATE DONATION COUNT
        // =========================================================
        public static bool UpdateDonationCount(int donorId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE RareDonors SET 
                                    TotalDonations = TotalDonations + 1,
                                    LastDonationDate = @DonationDate
                                  WHERE DonorID = @DonorID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonationDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@DonorID", donorId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateDonationCount Error: {ex.Message}");
                return false;
            }
        }

        // =========================================================
        // SEARCH RARE DONORS
        // =========================================================
        public static DataTable Search(string searchTerm)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT 
                                    DonorID, 
                                    FullName, 
                                    BloodGroup, 
                                    RareBloodType, 
                                    Phone, 
                                    City, 
                                    Status
                                  FROM RareDonors 
                                  WHERE FullName LIKE @Search 
                                     OR CNIC LIKE @Search 
                                     OR RareBloodType LIKE @Search 
                                     OR City LIKE @Search
                                  ORDER BY FullName";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Search Error: {ex.Message}");
            }
            return dt;
        }

        // =========================================================
        // GET STATISTICS
        // =========================================================
        public static (int total, int active, string[] rareTypes) GetStatistics()
        {
            int total = 0, active = 0;
            string[] rareTypes = new string[0];

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"SELECT 
                                    COUNT(*) as Total,
                                    SUM(CASE WHEN Status = 'Active' THEN 1 ELSE 0 END) as Active
                                  FROM RareDonors";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                total = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
                                active = reader["Active"] != DBNull.Value ? Convert.ToInt32(reader["Active"]) : 0;
                            }
                        }
                    }

                    // Get unique rare types
                    string rareSql = "SELECT DISTINCT RareBloodType FROM RareDonors ORDER BY RareBloodType";
                    using (var rareCmd = new SqlCommand(rareSql, conn))
                    {
                        using (var rareReader = rareCmd.ExecuteReader())
                        {
                            var types = new System.Collections.Generic.List<string>();
                            while (rareReader.Read())
                            {
                                string type = rareReader["RareBloodType"]?.ToString();
                                if (!string.IsNullOrEmpty(type))
                                    types.Add(type);
                            }
                            rareTypes = types.ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetStatistics Error: {ex.Message}");
            }
            return (total, active, rareTypes);
        }

        // =========================================================
        // DELETE RARE DONOR
        // =========================================================
        public static bool Delete(int donorId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "DELETE FROM RareDonors WHERE DonorID = @DonorID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DonorID", donorId);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Delete Error: {ex.Message}");
                return false;
            }
        }

        // =========================================================
        // MAP TO RARE DONOR MODEL
        // =========================================================
        private static RareDonor MapToRareDonor(SqlDataReader reader)
        {
            return new RareDonor
            {
                DonorID = Convert.ToInt32(reader["DonorID"]),
                FullName = reader["FullName"].ToString(),
                CNIC = reader["CNIC"].ToString(),
                BloodGroup = reader["BloodGroup"] == DBNull.Value ? null : reader["BloodGroup"].ToString(),
                RareBloodType = reader["RareBloodType"].ToString(),
                Phone = reader["Phone"].ToString(),
                Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                Address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString(),
                City = reader["City"] == DBNull.Value ? null : reader["City"].ToString(),
                Gender = reader["Gender"] == DBNull.Value ? null : reader["Gender"].ToString(),
                DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["DateOfBirth"]),
                Age = reader["Age"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Age"]),
                Weight = reader["Weight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Weight"]),
                MedicalHistory = reader["MedicalHistory"] == DBNull.Value ? null : reader["MedicalHistory"].ToString(),
                Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                Status = reader["Status"].ToString(),
                CreatedBy = reader["CreatedBy"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CreatedBy"]),
                CreatedAt = reader["CreatedAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["CreatedAt"]),
                LastDonationDate = reader["LastDonationDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LastDonationDate"]),
                TotalDonations = reader["TotalDonations"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalDonations"])
            };
        }
    }
}