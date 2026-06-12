using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class BloodComponentDAL
    {
        // Insert new blood component
        public static bool InsertComponent(string componentName, string bloodGroup, int quantity,
            DateTime expiryDate, string storageLocation, string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO BloodComponents 
                               (ComponentName, BloodGroup, Quantity, ExpiryDate, StorageLocation, Status, CreatedAt)
                               VALUES (@ComponentName, @BloodGroup, @Quantity, @ExpiryDate, @StorageLocation, @Status, GETDATE())";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ComponentName", componentName);
                cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                cmd.Parameters.AddWithValue("@StorageLocation", storageLocation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", status);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Update blood component
        public static bool UpdateComponent(int componentId, string componentName, string bloodGroup,
            int quantity, DateTime expiryDate, string storageLocation, string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE BloodComponents SET 
                               ComponentName = @ComponentName,
                               BloodGroup = @BloodGroup,
                               Quantity = @Quantity,
                               ExpiryDate = @ExpiryDate,
                               StorageLocation = @StorageLocation,
                               Status = @Status
                               WHERE ComponentID = @ComponentID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ComponentID", componentId);
                cmd.Parameters.AddWithValue("@ComponentName", componentName);
                cmd.Parameters.AddWithValue("@BloodGroup", bloodGroup);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                cmd.Parameters.AddWithValue("@StorageLocation", storageLocation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", status);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get component by ID
        public static ComponentData GetComponentByID(int componentId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM BloodComponents WHERE ComponentID = @ComponentID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ComponentID", componentId);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new ComponentData
                    {
                        ComponentID = reader.GetInt32(0),
                        ComponentName = reader.GetString(1),
                        BloodGroup = reader.GetString(2),
                        Quantity = reader.GetInt32(3),
                        ExpiryDate = reader.GetDateTime(4),
                        StorageLocation = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Status = reader.GetString(6),
                        CreatedAt = reader.GetDateTime(7)
                    };
                }
                return null;
            }
        }

        // Get all components
        public static DataTable GetAllComponents()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT ComponentID, ComponentName, BloodGroup, Quantity, ExpiryDate, StorageLocation, Status FROM BloodComponents ORDER BY ComponentID DESC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Delete component
        public static bool DeleteComponent(int componentId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM BloodComponents WHERE ComponentID = @ComponentID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ComponentID", componentId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Search components
        public static DataTable SearchComponents(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT ComponentID, ComponentName, BloodGroup, Quantity, ExpiryDate, Status 
                              FROM BloodComponents 
                              WHERE ComponentName LIKE @Search OR BloodGroup LIKE @Search";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }

    // Component Data Model
    public class ComponentData
    {
        public int ComponentID { get; set; }
        public string ComponentName { get; set; }
        public string BloodGroup { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string StorageLocation { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}