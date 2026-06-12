using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class ReportDAL
    {
        // Donation report by date range - FIXED
        public static DataTable GetDonationReport(DateTime fromDate, DateTime toDate)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                    DonationID, DonorName, BloodGroup, DonationDate, 
                    DonationLocation, Units, Volume
                    FROM DonationHistory 
                    WHERE DonationDate BETWEEN @FromDate AND @ToDate
                    ORDER BY DonationDate DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Inventory report
        public static DataTable GetInventoryReport()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                    BloodGroup, ComponentType, COUNT(*) as Quantity,
                    SUM(CASE WHEN Status = 'Available' THEN 1 ELSE 0 END) as Available,
                    SUM(CASE WHEN ExpiryDate < GETDATE() THEN 1 ELSE 0 END) as Expired
                    FROM BloodBags 
                    GROUP BY BloodGroup, ComponentType
                    ORDER BY BloodGroup";

                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Requisition report by date range
        public static DataTable GetRequisitionReport(DateTime fromDate, DateTime toDate)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                    RequisitionNumber, PatientName, BloodGroup, UnitsNeeded,
                    Hospital, Urgency, Status, RequestDate, ApprovalDate
                    FROM Requisitions 
                    WHERE RequestDate BETWEEN @FromDate AND @ToDate
                    ORDER BY RequestDate DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Donor report
        public static DataTable GetDonorReport()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                    DonorID, FullName, BloodGroup, TotalDonations, LastDonationDate,
                    RegistrationDate, IsActive
                    FROM Donors 
                    ORDER BY TotalDonations DESC";

                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Blood group summary report
        public static DataTable GetBloodGroupSummary()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                    BloodGroup,
                    COUNT(CASE WHEN Status = 'Available' THEN 1 END) as Available,
                    COUNT(CASE WHEN Status = 'Issued' THEN 1 END) as Issued,
                    COUNT(CASE WHEN ExpiryDate < GETDATE() THEN 1 END) as Expired
                    FROM BloodBags 
                    GROUP BY BloodGroup";

                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Monthly donation trends
        public static DataTable GetMonthlyDonationTrends(int year)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                    MONTH(DonationDate) as Month,
                    COUNT(*) as Donations,
                    SUM(Units) as TotalUnits
                    FROM DonationHistory 
                    WHERE YEAR(DonationDate) = @Year
                    GROUP BY MONTH(DonationDate)
                    ORDER BY Month";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", year);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Feedback summary
        public static DataTable GetFeedbackSummary()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                    Rating,
                    COUNT(*) as Count,
                    AVG(CAST(Rating AS FLOAT)) as AverageRating
                    FROM Feedback 
                    GROUP BY Rating
                    ORDER BY Rating DESC";

                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}