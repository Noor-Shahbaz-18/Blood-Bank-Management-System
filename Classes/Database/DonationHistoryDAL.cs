using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class DonationHistoryDAL
    {
        // Get donation history by donor ID
        public static DataTable GetByDonorID(int donorID)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT DonationID, DonorID, DonorName, BloodGroup, DonationDate, 
                                  DonationLocation, Units, Volume, BagID, CertificateIssued
                                  FROM DonationHistory 
                                  WHERE DonorID = @DonorID
                                  ORDER BY DonationDate DESC";

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@DonorID", donorID);
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    System.Diagnostics.Debug.WriteLine($"GetByDonorID: Found {dt.Rows.Count} records for DonorID {donorID}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByDonorID Error: {ex.Message}");
            }
            return dt;
        }

        // Get donation history by donor name (fallback)
        public static DataTable GetByDonorName(string donorName)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT DonationID, DonorID, DonorName, BloodGroup, DonationDate, 
                                  DonationLocation, Units, Volume, BagID, CertificateIssued
                                  FROM DonationHistory 
                                  WHERE DonorName = @DonorName
                                  ORDER BY DonationDate DESC";

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@DonorName", donorName);
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByDonorName Error: {ex.Message}");
            }
            return dt;
        }

        // Insert new donation record
        public static bool InsertDonation(int donorID, string donorName, string bloodGroup,
            DateTime donationDate, string location, int units, int volume, string bagId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO DonationHistory 
                              (DonorID, DonorName, BloodGroup, DonationDate, DonationLocation, 
                               Units, Volume, BagID, CertificateIssued, CreatedAt)
                              VALUES 
                              (@DonorID, @DonorName, @BloodGroup, @DonationDate, @Location,
                               @Units, @Volume, @BagID, 0, GETDATE())";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DonorID", donorID);
                cmd.Parameters.AddWithValue("@DonorName", donorName);
                cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                cmd.Parameters.AddWithValue("@DonationDate", donationDate);
                cmd.Parameters.AddWithValue("@Location", location);
                cmd.Parameters.AddWithValue("@Units", units);
                cmd.Parameters.AddWithValue("@Volume", volume);
                cmd.Parameters.AddWithValue("@BagID", bagId ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Mark certificate as issued
        public static bool MarkCertificateIssued(int donationID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE DonationHistory SET CertificateIssued = 1 WHERE DonationID = @DonationID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DonationID", donationID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get donation by ID
        public static DataRow GetDonationByID(int donationID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM DonationHistory WHERE DonationID = @DonationID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DonationID", donationID);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
                return null;
            }
        }
    }
}