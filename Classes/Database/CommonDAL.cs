using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class CommonDAL
    {
        // Get all blood groups
        public static DataTable GetBloodGroups()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BloodGroup");
            dt.Rows.Add("A+");
            dt.Rows.Add("A-");
            dt.Rows.Add("B+");
            dt.Rows.Add("B-");
            dt.Rows.Add("AB+");
            dt.Rows.Add("AB-");
            dt.Rows.Add("O+");
            dt.Rows.Add("O-");
            return dt;
        }

        // Get dashboard counts
        public static int GetTotalDonors()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Donors WHERE IsActive = 1";
                var cmd = new SqlCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int GetTotalPatients()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Patients WHERE IsActive = 1";
                var cmd = new SqlCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int GetTotalBloodBags()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM BloodBags WHERE Status = 'Available'";
                var cmd = new SqlCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int GetPendingRequisitions()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Requisitions WHERE Status = 'Pending'";
                var cmd = new SqlCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Check if table exists
        public static bool TableExists(string tableName)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@TableName", tableName);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // Execute non query
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteNonQuery();
            }
        }

        // Execute scalar
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteScalar();
            }
        }

        // Execute reader (returns DataTable)
        public static DataTable ExecuteReader(string query, SqlParameter[] parameters = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}