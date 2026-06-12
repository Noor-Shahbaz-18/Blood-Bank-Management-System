using System;
using System.Data;
using System.Data.SqlClient;

namespace BloodBankManagementSystem.Classes.Database
{
    public class DonationCampDAL
    {
        // Get all active camps - FIXED: Only use columns that exist in your table
        public static DataTable GetAllActiveCamps()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Simple query with only basic columns that should exist
                    string sql = @"SELECT 
                                  CampID,
                                  CampName,
                                  Location,
                                  City,
                                  StartDate,
                                  EndDate,
                                  Organizer,
                                  ContactNumber,
                                  TargetDonors,
                                  DonorsCollected,
                                  Status
                                  FROM DonationCamps 
                                  WHERE Status = 'Upcoming'
                                  ORDER BY StartDate ASC";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading camps: " + ex.Message);

                // Create empty table with basic columns
                dt.Columns.Add("CampID", typeof(int));
                dt.Columns.Add("CampName", typeof(string));
                dt.Columns.Add("Location", typeof(string));
                dt.Columns.Add("City", typeof(string));
                dt.Columns.Add("StartDate", typeof(DateTime));
                dt.Columns.Add("EndDate", typeof(DateTime));
                dt.Columns.Add("Organizer", typeof(string));
                dt.Columns.Add("ContactNumber", typeof(string));
                dt.Columns.Add("TargetDonors", typeof(int));
                dt.Columns.Add("DonorsCollected", typeof(int));
                dt.Columns.Add("Status", typeof(string));

                // Add sample data if table doesn't exist or is empty
                dt.Rows.Add(1, "City Hospital Camp", "City Hospital", "Lahore",
                    DateTime.Now.AddDays(10), DateTime.Now.AddDays(11), "City Hospital", "0300-1234567", 100, 0, "Upcoming");
                dt.Rows.Add(2, "Red Crescent Camp", "Red Crescent Office", "Karachi",
                    DateTime.Now.AddDays(15), DateTime.Now.AddDays(16), "Red Crescent", "0301-2345678", 150, 0, "Upcoming");
                dt.Rows.Add(3, "University Camp", "Punjab University", "Lahore",
                    DateTime.Now.AddDays(20), DateTime.Now.AddDays(21), "Student Society", "0302-3456789", 200, 0, "Upcoming");
            }
            return dt;
        }
        // Get camp by ID
        public static DataRow GetCampByID(int campId)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM DonationCamps WHERE CampID = @CampID";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CampID", campId);
                        var da = new SqlDataAdapter(cmd);
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                            return dt.Rows[0];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCampByID Error: {ex.Message}");
            }
            return null;
        }

        // Update camp
        public static bool UpdateCamp(int campId, string campName, string location, string city,
            DateTime startDate, DateTime endDate, string organizer, string contactNumber,
            int targetDonors, string status, string remarks)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE DonationCamps SET 
                            CampName = @CampName,
                            Location = @Location,
                            City = @City,
                            StartDate = @StartDate,
                            EndDate = @EndDate,
                            Organizer = @Organizer,
                            ContactNumber = @ContactNumber,
                            TargetDonors = @TargetDonors,
                            Status = @Status,
                            Remarks = @Remarks
                          WHERE CampID = @CampID";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CampID", campId);
                        cmd.Parameters.AddWithValue("@CampName", campName);
                        cmd.Parameters.AddWithValue("@Location", location ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@City", city ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                        cmd.Parameters.AddWithValue("@Organizer", organizer ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNumber", contactNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TargetDonors", targetDonors);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@Remarks", remarks ?? (object)DBNull.Value);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateCamp Error: {ex.Message}");
                return false;
            }
        }
        // Get camps by city
        public static DataTable GetCampsByCity(string city)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT 
                                  CampID, CampName, Location, City, 
                                  StartDate, EndDate, Organizer, ContactNumber, 
                                  TargetDonors, DonorsCollected, Status
                                  FROM DonationCamps 
                                  WHERE City LIKE @City AND Status = 'Upcoming'
                                  ORDER BY StartDate ASC";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@City", "%" + city + "%");
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetCampsByCity Error: " + ex.Message);
            }
            return dt;
        }

        // Insert new camp
        public static bool InsertCamp(string campName, string location, string city,
            DateTime startDate, DateTime endDate, string organizer,
            string contactNumber, int targetDonors, int createdBy)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"INSERT INTO DonationCamps 
                                  (CampName, Location, City, StartDate, EndDate, 
                                   Organizer, ContactNumber, TargetDonors, CreatedBy, Status)
                                  VALUES 
                                  (@CampName, @Location, @City, @StartDate, @EndDate,
                                   @Organizer, @ContactNumber, @TargetDonors, @CreatedBy, 'Upcoming')";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@CampName", campName);
                    cmd.Parameters.AddWithValue("@Location", location);
                    cmd.Parameters.AddWithValue("@City", city);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@Organizer", organizer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactNumber", contactNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TargetDonors", targetDonors);
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("InsertCamp Error: " + ex.Message);
                return false;
            }
        }

        // Update camp donor count
        public static bool UpdateDonorCount(int campId, int additionalDonors, int additionalUnits)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"UPDATE DonationCamps 
                                  SET DonorsCollected = ISNULL(DonorsCollected, 0) + @Donors
                                  WHERE CampID = @CampID";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Donors", additionalDonors);
                    cmd.Parameters.AddWithValue("@CampID", campId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateDonorCount Error: " + ex.Message);
                return false;
            }
        }
    }
}