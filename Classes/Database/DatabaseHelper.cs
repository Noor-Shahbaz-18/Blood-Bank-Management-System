using System;
using System.Configuration;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class DatabaseHelper
    {
        private static readonly string _conn =
            ConfigurationManager.ConnectionStrings["BloodBankDB"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(_conn);
        }

        // 🆕 Test connection
        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // 🆕 Get connection string (for debugging)
        public static string GetConnectionString()
        {
            return _conn;
        }

        // 🆕 Execute scalar (helper method)
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        // 🆕 Execute non query (helper method)
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        // 🆕 Execute reader (helper method)
        public static SqlDataReader ExecuteReader(string query, SqlParameter[] parameters = null)
        {
            var conn = GetConnection();
            conn.Open();
            var cmd = new SqlCommand(query, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
    }
}