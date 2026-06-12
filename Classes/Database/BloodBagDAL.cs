using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class BloodBagDAL
    {
        // Insert new blood bag
        public static bool Insert(BloodBag bag)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO BloodBags 
                    (BagID, DonorID, DonorName, BloodGroup, ComponentType, Quantity, Volume, 
                     CollectionDate, ExpiryDate, StorageLocation, Status, ScreeningResult, 
                     PreparedBy, Remarks)
                    VALUES 
                    (@BagID, @DonorID, @DonorName, @BloodGroup, @ComponentType, @Quantity, @Volume,
                     @CollectionDate, @ExpiryDate, @StorageLocation, @Status, @ScreeningResult,
                     @PreparedBy, @Remarks)";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BagID", bag.BagID);
                cmd.Parameters.AddWithValue("@DonorID", bag.DonorID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DonorName", bag.DonorName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BloodGroup", bag.BloodGroup);
                cmd.Parameters.AddWithValue("@ComponentType", bag.ComponentType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", bag.Quantity);
                cmd.Parameters.AddWithValue("@Volume", bag.Volume);
                cmd.Parameters.AddWithValue("@CollectionDate", bag.CollectionDate);
                cmd.Parameters.AddWithValue("@ExpiryDate", bag.ExpiryDate);
                cmd.Parameters.AddWithValue("@StorageLocation", bag.StorageLocation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", bag.Status);
                cmd.Parameters.AddWithValue("@ScreeningResult", bag.ScreeningResult ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@PreparedBy", bag.PreparedBy ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks", bag.Remarks ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get all blood bags
        public static DataTable GetAll()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM BloodBags ORDER BY CollectionDate DESC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get blood bag by ID
        public static BloodBag GetByID(string bagID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM BloodBags WHERE BagID = @BagID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BagID", bagID);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return MapToBloodBag(reader);
                }
                return null;
            }
        }

        // Get blood bags by blood group
        public static DataTable GetByBloodGroup(string bloodGroup)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM BloodBags WHERE BloodGroup = @BloodGroup AND Status = 'Available'";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Update blood bag
        public static bool Update(BloodBag bag)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE BloodBags SET 
                    DonorID = @DonorID,
                    DonorName = @DonorName,
                    BloodGroup = @BloodGroup,
                    ComponentType = @ComponentType,
                    Quantity = @Quantity,
                    Volume = @Volume,
                    StorageLocation = @StorageLocation,
                    Status = @Status,
                    Remarks = @Remarks
                    WHERE BagID = @BagID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BagID", bag.BagID);
                cmd.Parameters.AddWithValue("@DonorID", bag.DonorID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DonorName", bag.DonorName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@BloodGroup", bag.BloodGroup);
                cmd.Parameters.AddWithValue("@ComponentType", bag.ComponentType ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Quantity", bag.Quantity);
                cmd.Parameters.AddWithValue("@Volume", bag.Volume);
                cmd.Parameters.AddWithValue("@StorageLocation", bag.StorageLocation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", bag.Status);
                cmd.Parameters.AddWithValue("@Remarks", bag.Remarks ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Update status
        public static bool UpdateStatus(string bagID, string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE BloodBags SET Status = @Status WHERE BagID = @BagID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@BagID", bagID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Delete blood bag
        public static bool Delete(string bagID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM BloodBags WHERE BagID = @BagID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BagID", bagID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get available count by blood group
        public static int GetAvailableCount(string bloodGroup)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM BloodBags WHERE BloodGroup = @BloodGroup AND Status = 'Available'";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Search blood bags
        public static DataTable Search(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT * FROM BloodBags 
                              WHERE BagID LIKE @Search 
                              OR DonorName LIKE @Search 
                              OR BloodGroup LIKE @Search";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get expiring bags (within next 7 days)
        public static DataTable GetExpiringBags()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT * FROM BloodBags 
                              WHERE ExpiryDate BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE())
                              AND Status = 'Available'";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Map reader to BloodBag object
        private static BloodBag MapToBloodBag(SqlDataReader reader)
        {
            return new BloodBag
            {
                BloodBagID = reader.GetInt32(0),
                BagID = reader.GetString(1),
                DonorID = reader.IsDBNull(2) ? null : reader.GetString(2),
                DonorName = reader.IsDBNull(3) ? null : reader.GetString(3),
                BloodGroup = reader.GetString(4),
                ComponentType = reader.IsDBNull(5) ? null : reader.GetString(5),
                Quantity = reader.GetInt32(6),
                Volume = reader.GetInt32(7),
                CollectionDate = reader.GetDateTime(8),
                ExpiryDate = reader.GetDateTime(9),
                StorageLocation = reader.IsDBNull(10) ? null : reader.GetString(10),
                Status = reader.GetString(11),
                ScreeningResult = reader.IsDBNull(12) ? null : reader.GetString(12),
                PreparedBy = reader.IsDBNull(13) ? null : reader.GetString(13),
                Remarks = reader.IsDBNull(14) ? null : reader.GetString(14)
            };
        }
    }
}