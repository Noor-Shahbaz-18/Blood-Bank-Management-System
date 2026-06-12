using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class TransfusionHistoryDAL
    {
        public static DataTable GetByPatientID(int patientID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT TransfusionID, PatientName, BloodGroup, Units, 
                              Hospital, DoctorName, TransfusionDate, Status, ReactionType, Notes
                              FROM TransfusionHistory 
                              WHERE PatientID = @PatientID
                              ORDER BY TransfusionDate DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientID", patientID);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public static DataTable GetByPatientName(string patientName)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT TransfusionID, PatientName, BloodGroup, Units, 
                              Hospital, DoctorName, TransfusionDate, Status, ReactionType, Notes
                              FROM TransfusionHistory 
                              WHERE PatientName LIKE @PatientName
                              ORDER BY TransfusionDate DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientName", "%" + patientName + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public static DataRow GetByID(int transfusionID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM TransfusionHistory WHERE TransfusionID = @TransfusionID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@TransfusionID", transfusionID);
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