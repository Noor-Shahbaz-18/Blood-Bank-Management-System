using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class AppointmentDAL
    {
        // Save appointment - FIXED with your exact column names
        public static bool SaveAppointment(int donorID, string location, DateTime date, string timeSlot)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Check if table exists, if not create it with your column names
                string createTable = @"IF NOT EXISTS (SELECT * FROM sysobjects 
                                       WHERE name='Appointments' AND xtype='U')
                                       CREATE TABLE Appointments (
                                           AppointmentID INT IDENTITY(1,1) PRIMARY KEY,
                                           DonorID INT,
                                           Location NVARCHAR(200),
                                           AppointmentDate DATE,
                                           TimeSlot NVARCHAR(20),
                                           Status NVARCHAR(20) DEFAULT 'Confirmed',
                                           CreatedAt DATETIME DEFAULT GETDATE()
                                       )";
                new SqlCommand(createTable, conn).ExecuteNonQuery();

                // Using exact column names: AppointmentDate, TimeSlot
                string sql = @"INSERT INTO Appointments (DonorID, Location, AppointmentDate, TimeSlot, Status)
                               VALUES (@donor, @loc, @date, @time, 'Confirmed')";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@donor", donorID);
                cmd.Parameters.AddWithValue("@loc", location);
                cmd.Parameters.AddWithValue("@date", date.Date);
                cmd.Parameters.AddWithValue("@time", timeSlot);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Get all appointments
        public static DataTable GetAllAppointments()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT a.AppointmentID, a.DonorID, d.FullName as DonorName, 
                              a.Location, a.AppointmentDate, a.TimeSlot, a.Status, a.CreatedAt
                              FROM Appointments a
                              LEFT JOIN Donors d ON a.DonorID = d.DonorID
                              ORDER BY a.AppointmentDate DESC, a.TimeSlot";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get appointments by donor
        public static DataTable GetAppointmentsByDonor(int donorID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT AppointmentID, Location, AppointmentDate, TimeSlot, Status, CreatedAt
                              FROM Appointments WHERE DonorID = @DonorID 
                              ORDER BY AppointmentDate DESC, TimeSlot";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DonorID", donorID);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get appointments by date
        public static DataTable GetAppointmentsByDate(DateTime date)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT a.AppointmentID, a.DonorID, d.FullName as DonorName,
                              a.Location, a.TimeSlot, a.Status
                              FROM Appointments a
                              LEFT JOIN Donors d ON a.DonorID = d.DonorID
                              WHERE a.AppointmentDate = @Date
                              ORDER BY a.TimeSlot";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Date", date.Date);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Update appointment status
        public static bool UpdateAppointmentStatus(int appointmentID, string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Appointments SET Status = @Status WHERE AppointmentID = @AppointmentID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@AppointmentID", appointmentID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Cancel appointment
        public static bool CancelAppointment(int appointmentID)
        {
            return UpdateAppointmentStatus(appointmentID, "Cancelled");
        }

        // Complete appointment
        public static bool CompleteAppointment(int appointmentID)
        {
            return UpdateAppointmentStatus(appointmentID, "Completed");
        }

        // Get upcoming appointments
        public static DataTable GetUpcomingAppointments()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT a.AppointmentID, a.DonorID, d.FullName as DonorName,
                              a.Location, a.AppointmentDate, a.TimeSlot, a.Status
                              FROM Appointments a
                              LEFT JOIN Donors d ON a.DonorID = d.DonorID
                              WHERE a.AppointmentDate >= @Today AND a.Status = 'Confirmed'
                              ORDER BY a.AppointmentDate, a.TimeSlot";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Today", DateTime.Today);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Get appointment count for today
        public static int GetTodayAppointmentCount()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Appointments WHERE AppointmentDate = @Today AND Status = 'Confirmed'";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Today", DateTime.Today);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Check if slot is available
        public static bool IsTimeSlotAvailable(DateTime date, string timeSlot)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Appointments WHERE AppointmentDate = @Date AND TimeSlot = @TimeSlot";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Date", date.Date);
                cmd.Parameters.AddWithValue("@TimeSlot", timeSlot);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count < 5;
            }
        }

        // Get available time slots for a date
        public static DataTable GetAvailableTimeSlots(DateTime date)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TimeSlot");

            string[] allSlots = { "09:00 AM", "10:00 AM", "11:00 AM", "12:00 PM",
                                  "01:00 PM", "02:00 PM", "03:00 PM", "04:00 PM", "05:00 PM" };

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT TimeSlot, COUNT(*) as Count FROM Appointments " +
                            "WHERE AppointmentDate = @Date GROUP BY TimeSlot";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Date", date.Date);
                var reader = cmd.ExecuteReader();

                var bookedSlots = new System.Collections.Generic.HashSet<string>();
                while (reader.Read())
                {
                    if (reader.GetInt32(1) >= 5)
                    {
                        bookedSlots.Add(reader.GetString(0));
                    }
                }
                reader.Close();

                foreach (string slot in allSlots)
                {
                    if (!bookedSlots.Contains(slot))
                    {
                        dt.Rows.Add(slot);
                    }
                }
            }
            return dt;
        }
    }
}