using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class PatientDAL
    {
        // =========================================================
        // INSERT PATIENT
        // =========================================================
        public static bool Insert(Patient patient)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Patients 
                    (UserID, FullName, CNIC, BloodGroup, DateOfBirth, Gender, 
                     Address, Phone, Email, Disease, Hospital, WardNumber, 
                     DoctorName, RegistrationDate, IsActive, Age, EmergencyContact, MedicalHistory)
                    VALUES 
                    (@UserID, @FullName, @CNIC, @BloodGroup, @DateOfBirth, @Gender,
                     @Address, @Phone, @Email, @Disease, @Hospital, @WardNumber,
                     @DoctorName, @RegistrationDate, @IsActive, @Age, @EmergencyContact, @MedicalHistory)";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", patient.UserID);
                cmd.Parameters.AddWithValue("@FullName", patient.FullName);
                cmd.Parameters.AddWithValue("@CNIC", (object)patient.CNIC ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BloodGroup", (object)patient.BloodGroup ?? DBNull.Value);

                // Handle DateOfBirth properly
                if (patient.DateOfBirth < new DateTime(1753, 1, 1) || patient.DateOfBirth == DateTime.MinValue)
                {
                    cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Now.AddYears(-25));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);
                }

                cmd.Parameters.AddWithValue("@Gender", (object)patient.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)patient.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", (object)patient.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)patient.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Disease", (object)patient.Disease ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Hospital", (object)patient.Hospital ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WardNumber", (object)patient.WardNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DoctorName", (object)patient.DoctorName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RegistrationDate", patient.RegistrationDate);
                cmd.Parameters.AddWithValue("@IsActive", patient.IsActive);
                cmd.Parameters.AddWithValue("@Age", patient.Age);
                cmd.Parameters.AddWithValue("@EmergencyContact", (object)patient.EmergencyContact ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MedicalHistory", (object)patient.MedicalHistory ?? DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // GET PATIENT BY USER ID
        // =========================================================
        public static Patient GetByUserID(int userID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Patients WHERE UserID = @UserID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userID);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return MapToPatient(reader);
                }
                return null;
            }
        }

        // =========================================================
        // GET PATIENT BY ID
        // =========================================================
        public static Patient GetByID(int patientID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Patients WHERE PatientID = @PatientID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientID", patientID);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return MapToPatient(reader);
                }
                return null;
            }
        }

        // =========================================================
        // GET PATIENT BY CNIC
        // =========================================================
        public static Patient GetByCNIC(string cnic)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Patients WHERE CNIC = @CNIC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CNIC", cnic);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return MapToPatient(reader);
                }
                return null;
            }
        }

        // =========================================================
        // GET ALL PATIENTS
        // =========================================================
        public static DataTable GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Patients WHERE IsActive = 1 ORDER BY RegistrationDate DESC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // UPDATE PATIENT (Basic Update)
        // =========================================================
        public static bool Update(Patient patient)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Patients SET 
                    FullName = @FullName,
                    BloodGroup = @BloodGroup,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    Disease = @Disease,
                    Hospital = @Hospital,
                    WardNumber = @WardNumber,
                    DoctorName = @DoctorName
                    WHERE PatientID = @PatientID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientID", patient.PatientID);
                cmd.Parameters.AddWithValue("@FullName", patient.FullName);
                cmd.Parameters.AddWithValue("@BloodGroup", (object)patient.BloodGroup ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)patient.Address ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", (object)patient.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)patient.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Disease", (object)patient.Disease ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Hospital", (object)patient.Hospital ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WardNumber", (object)patient.WardNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DoctorName", (object)patient.DoctorName ?? DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // UPDATE PATIENT PROFILE (Full Profile Update)
        // =========================================================
        public static bool UpdatePatientProfile(Patient patient)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Patients SET 
                    FullName = @FullName,
                    Email = @Email,
                    Phone = @Phone,
                    Address = @Address,
                    DateOfBirth = @DateOfBirth,
                    Age = @Age,
                    Gender = @Gender
                    WHERE PatientID = @PatientID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientID", patient.PatientID);
                cmd.Parameters.AddWithValue("@FullName", patient.FullName);
                cmd.Parameters.AddWithValue("@Email", (object)patient.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", (object)patient.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", (object)patient.Address ?? DBNull.Value);

                // Handle DateOfBirth properly
                if (patient.DateOfBirth < new DateTime(1753, 1, 1) || patient.DateOfBirth == DateTime.MinValue)
                {
                    cmd.Parameters.AddWithValue("@DateOfBirth", DateTime.Now.AddYears(-25));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);
                }

                cmd.Parameters.AddWithValue("@Age", patient.Age);
                cmd.Parameters.AddWithValue("@Gender", (object)patient.Gender ?? DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // UPDATE PROFILE PICTURE
        // =========================================================
        public static bool UpdateProfilePicture(int patientID, byte[] imageData)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Patients SET ProfilePicture = @Picture WHERE PatientID = @PatientID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Picture", imageData ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PatientID", patientID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // GET PROFILE PICTURE
        // =========================================================
        public static byte[] GetProfilePicture(int patientID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT ProfilePicture FROM Patients WHERE PatientID = @PatientID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientID", patientID);
                var result = cmd.ExecuteScalar();
                return result as byte[];
            }
        }

        // =========================================================
        // SEARCH PATIENTS
        // =========================================================
        public static DataTable Search(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT PatientID, FullName, CNIC, BloodGroup, Phone, Email, 
                              Hospital, IsActive FROM Patients 
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
        // GET PATIENTS BY BLOOD GROUP
        // =========================================================
        public static DataTable GetByBloodGroup(string bloodGroup)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT PatientID, FullName, Phone, Hospital FROM Patients WHERE BloodGroup = @BloodGroup AND IsActive = 1";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET PATIENT STATISTICS
        // =========================================================
        public static DataTable GetPatientStatistics()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                              BloodGroup,
                              COUNT(*) as TotalPatients,
                              SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActivePatients
                              FROM Patients 
                              GROUP BY BloodGroup
                              ORDER BY BloodGroup";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // DELETE PATIENT (Soft Delete)
        // =========================================================
        public static bool DeletePatient(int patientID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Patients SET IsActive = 0 WHERE PatientID = @PatientID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientID", patientID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // GET TOTAL PATIENTS COUNT
        // =========================================================
        public static int GetTotalPatientCount()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Patients WHERE IsActive = 1";
                var cmd = new SqlCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // =========================================================
        // MAP TO PATIENT MODEL
        // =========================================================
        private static Patient MapToPatient(SqlDataReader reader)
        {
            Patient patient = new Patient();

            patient.PatientID = reader.GetInt32(0);
            patient.UserID = reader.GetInt32(1);
            patient.FullName = reader.GetString(2);
            patient.CNIC = reader.IsDBNull(3) ? "" : reader.GetString(3);
            patient.BloodGroup = reader.IsDBNull(4) ? null : reader.GetString(4);

            // Handle DateOfBirth - prevent SQL DateTime overflow
            if (reader.IsDBNull(5) || reader.GetDateTime(5) < new DateTime(1753, 1, 1))
            {
                patient.DateOfBirth = DateTime.Now.AddYears(-25);
            }
            else
            {
                patient.DateOfBirth = reader.GetDateTime(5);
            }

            patient.Gender = reader.IsDBNull(6) ? null : reader.GetString(6);
            patient.Address = reader.IsDBNull(7) ? null : reader.GetString(7);
            patient.Phone = reader.IsDBNull(8) ? null : reader.GetString(8);
            patient.Email = reader.IsDBNull(9) ? null : reader.GetString(9);
            patient.Disease = reader.IsDBNull(10) ? null : reader.GetString(10);
            patient.Hospital = reader.IsDBNull(11) ? null : reader.GetString(11);
            patient.WardNumber = reader.IsDBNull(12) ? null : reader.GetString(12);
            patient.DoctorName = reader.IsDBNull(13) ? null : reader.GetString(13);
            patient.RegistrationDate = reader.GetDateTime(14);
            patient.IsActive = reader.GetBoolean(15);

            // Optional columns
            if (ColumnExists(reader, "Age"))
                patient.Age = reader.IsDBNull(reader.GetOrdinal("Age")) ? 0 : reader.GetInt32(reader.GetOrdinal("Age"));

            if (ColumnExists(reader, "EmergencyContact"))
                patient.EmergencyContact = reader.IsDBNull(reader.GetOrdinal("EmergencyContact")) ? null : reader.GetString(reader.GetOrdinal("EmergencyContact"));

            if (ColumnExists(reader, "MedicalHistory"))
                patient.MedicalHistory = reader.IsDBNull(reader.GetOrdinal("MedicalHistory")) ? null : reader.GetString(reader.GetOrdinal("MedicalHistory"));

            if (ColumnExists(reader, "ProfilePicture"))
                patient.ProfilePicture = reader.IsDBNull(reader.GetOrdinal("ProfilePicture")) ? null : (byte[])reader["ProfilePicture"];

            return patient;
        }

        // =========================================================
        // CHECK IF COLUMN EXISTS
        // =========================================================
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