using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class SettingsDAL
    {
        // Get setting value by key
        public static string GetSetting(string settingKey)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    // ✅ Table name: SystemSettings (as per your database)
                    string sql = "SELECT SettingValue FROM SystemSettings WHERE SettingKey = @Key";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Key", settingKey);
                    var result = cmd.ExecuteScalar();

                    System.Diagnostics.Debug.WriteLine($"GetSetting - Key: {settingKey}, Value: {result}");
                    return result?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSetting Error: {ex.Message}");
                return "";
            }
        }

        // Update setting value
        public static bool UpdateSetting(string settingKey, string settingValue, int updatedBy)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE SystemSettings 
                                   SET SettingValue = @Value, 
                                       UpdatedAt = GETDATE(), 
                                       UpdatedBy = @UpdatedBy 
                                   WHERE SettingKey = @Key";

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Key", settingKey);
                    cmd.Parameters.AddWithValue("@Value", settingValue);
                    cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateSetting Error: {ex.Message}");
                return false;
            }
        }

        // Insert or update setting (if not exists, insert)
        public static bool SetSetting(string settingKey, string settingValue, int updatedBy)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Check if setting exists
                    string checkSql = "SELECT COUNT(*) FROM SystemSettings WHERE SettingKey = @Key";
                    var checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@Key", settingKey);
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (exists > 0)
                    {
                        // Update existing
                        string updateSql = @"UPDATE SystemSettings 
                                             SET SettingValue = @Value, 
                                                 UpdatedAt = GETDATE(), 
                                                 UpdatedBy = @UpdatedBy 
                                             WHERE SettingKey = @Key";
                        var updateCmd = new SqlCommand(updateSql, conn);
                        updateCmd.Parameters.AddWithValue("@Key", settingKey);
                        updateCmd.Parameters.AddWithValue("@Value", settingValue);
                        updateCmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                        return updateCmd.ExecuteNonQuery() > 0;
                    }
                    else
                    {
                        // Insert new
                        string insertSql = @"INSERT INTO SystemSettings (SettingKey, SettingValue, CreatedAt, UpdatedAt, UpdatedBy) 
                                             VALUES (@Key, @Value, GETDATE(), GETDATE(), @UpdatedBy)";
                        var insertCmd = new SqlCommand(insertSql, conn);
                        insertCmd.Parameters.AddWithValue("@Key", settingKey);
                        insertCmd.Parameters.AddWithValue("@Value", settingValue);
                        insertCmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                        return insertCmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetSetting Error: {ex.Message}");
                return false;
            }
        }

        // Get all settings
        public static DataTable GetAllSettings()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT SettingKey, SettingValue FROM SystemSettings";
                    var cmd = new SqlCommand(sql, conn);
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllSettings Error: {ex.Message}");
            }
            return dt;
        }

        // Save all settings at once
        public static bool SaveAllSettings(string systemName, string bankCode, string email, string phone, string address, int updatedBy)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Update System Name
                            UpdateSettingTx(conn, transaction, "SystemName", systemName, updatedBy);
                            // Update Bank Code
                            UpdateSettingTx(conn, transaction, "BankCode", bankCode, updatedBy);
                            // Update Email
                            UpdateSettingTx(conn, transaction, "ContactEmail", email, updatedBy);
                            // Update Phone
                            UpdateSettingTx(conn, transaction, "ContactPhone", phone, updatedBy);
                            // Update Address
                            UpdateSettingTx(conn, transaction, "Address", address, updatedBy);

                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SaveAllSettings Error: {ex.Message}");
                return false;
            }
        }

        private static void UpdateSettingTx(SqlConnection conn, SqlTransaction transaction, string key, string value, int updatedBy)
        {
            string sql = @"UPDATE SystemSettings 
                           SET SettingValue = @Value, 
                               UpdatedAt = GETDATE(), 
                               UpdatedBy = @UpdatedBy 
                           WHERE SettingKey = @Key";

            var cmd = new SqlCommand(sql, conn, transaction);
            cmd.Parameters.AddWithValue("@Key", key);
            cmd.Parameters.AddWithValue("@Value", value);
            cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
            cmd.ExecuteNonQuery();
        }
    }
}