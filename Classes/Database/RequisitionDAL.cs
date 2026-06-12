using System;
using System.Data;
using System.Data.SqlClient;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.Database
{
    public class RequisitionDAL
    {
        // =========================================================
        // INSERT METHOD (for BusinessLogic layer)
        // =========================================================
        public static bool Insert(Models.Requisition req)
        {
            if (req == null) return false;

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string reqNumber = GenerateRequisitionNumber();

                string sql = @"INSERT INTO Requisitions 
                               (RequisitionNumber, PatientName, CNIC, BloodGroup, UnitsNeeded, 
                                Urgency, DoctorID, PatientUserID, Hospital, Status, RequestDate, Remarks)
                               VALUES (@reqNumber, @patient, @cnic, @blood, @units, @urgency, 
                                       @doctorId, @patientUserId, @hospital, 'Pending', GETDATE(), @remarks)";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@reqNumber", reqNumber);
                cmd.Parameters.AddWithValue("@patient", req.PatientName);
                cmd.Parameters.AddWithValue("@cnic", req.CNIC ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@blood", req.BloodGroup);
                cmd.Parameters.AddWithValue("@units", req.UnitsNeeded);
                cmd.Parameters.AddWithValue("@urgency", req.Urgency);
                cmd.Parameters.AddWithValue("@doctorId", req.DoctorID > 0 ? (object)req.DoctorID : DBNull.Value);
                cmd.Parameters.AddWithValue("@patientUserId", req.PatientUserID > 0 ? (object)req.PatientUserID : DBNull.Value);
                cmd.Parameters.AddWithValue("@hospital", req.Hospital);
                cmd.Parameters.AddWithValue("@remarks", req.Remarks ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
        // Update requisition (full update)
        public static bool Update(Requisition req)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Requisitions SET 
                        PatientName = @PatientName,
                        CNIC = @CNIC,
                        BloodGroup = @BloodGroup,
                        UnitsNeeded = @UnitsNeeded,
                        Hospital = @Hospital,
                        Urgency = @Urgency,
                        Status = @Status,
                        Remarks = @Remarks,
                        UpdatedAt = GETDATE()
                      WHERE RequisitionID = @RequisitionID";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RequisitionID", req.RequisitionID);
                    cmd.Parameters.AddWithValue("@PatientName", req.PatientName);
                    cmd.Parameters.AddWithValue("@CNIC", req.CNIC ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BloodGroup", req.BloodGroup);
                    cmd.Parameters.AddWithValue("@UnitsNeeded", req.UnitsNeeded);
                    cmd.Parameters.AddWithValue("@Hospital", req.Hospital);
                    cmd.Parameters.AddWithValue("@Urgency", req.Urgency);
                    cmd.Parameters.AddWithValue("@Status", req.Status);
                    cmd.Parameters.AddWithValue("@Remarks", req.Remarks ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        // =========================================================
        // PATIENT REQUISITION (from Patient Dashboard)
        // =========================================================
        public static bool SavePatientRequisition(string patientName, string cnic, string bloodGroup,
            int unitsNeeded, string hospital, string urgency, int patientUserId, string remarks)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string reqNumber = GenerateRequisitionNumber();

                string sql = @"INSERT INTO Requisitions 
                               (RequisitionNumber, PatientName, CNIC, BloodGroup, UnitsNeeded, 
                                Urgency, PatientUserID, Hospital, Status, RequestDate, Remarks)
                               VALUES (@reqNumber, @patient, @cnic, @blood, @units, @urgency, 
                                       @patientUserId, @hospital, 'Pending', GETDATE(), @remarks)";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@reqNumber", reqNumber);
                cmd.Parameters.AddWithValue("@patient", patientName);
                cmd.Parameters.AddWithValue("@cnic", cnic ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@blood", bloodGroup);
                cmd.Parameters.AddWithValue("@units", unitsNeeded);
                cmd.Parameters.AddWithValue("@urgency", urgency);
                cmd.Parameters.AddWithValue("@patientUserId", patientUserId);
                cmd.Parameters.AddWithValue("@hospital", hospital);
                cmd.Parameters.AddWithValue("@remarks", remarks ?? (object)DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // DOCTOR REQUISITION (from Doctor Dashboard)
        // =========================================================
        public static bool SaveDoctorRequisition(string patientName, string cnic, string bloodGroup,
            int unitsNeeded, string hospital, string urgency, int doctorId, string remarks)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string reqNumber = GenerateRequisitionNumber();

                string sql = @"INSERT INTO Requisitions 
                               (RequisitionNumber, PatientName, CNIC, BloodGroup, UnitsNeeded, 
                                Urgency, DoctorID, Hospital, Status, RequestDate, Remarks)
                               VALUES (@reqNumber, @patient, @cnic, @blood, @units, @urgency, 
                                       @doctorId, @hospital, 'Pending', GETDATE(), @remarks)";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@reqNumber", reqNumber);
                cmd.Parameters.AddWithValue("@patient", patientName);
                cmd.Parameters.AddWithValue("@cnic", cnic ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@blood", bloodGroup);
                cmd.Parameters.AddWithValue("@units", unitsNeeded);
                cmd.Parameters.AddWithValue("@urgency", urgency);
                cmd.Parameters.AddWithValue("@doctorId", doctorId);
                cmd.Parameters.AddWithValue("@hospital", hospital);
                cmd.Parameters.AddWithValue("@remarks", remarks ?? (object)DBNull.Value);

                    return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // SAVE REQUISITION (Simple version - for backward compatibility)
        // =========================================================
        public static bool SaveRequisition(string patientName, string cnic, string bloodGroup,
                                           string hospital, string urgency, int doctorID)
        {
            return SaveDoctorRequisition(patientName, cnic, bloodGroup, 1, hospital, urgency, doctorID, null);
        }

        // =========================================================
        // SAVE REQUISITION WITH UNITS
        // =========================================================
        public static bool SaveRequisitionWithUnits(string patientName, string cnic, string bloodGroup,
                                                    int unitsNeeded, string hospital, string urgency, int doctorID)
        {
            return SaveDoctorRequisition(patientName, cnic, bloodGroup, unitsNeeded, hospital, urgency, doctorID, null);
        }

        // =========================================================
        // GET REQUISITIONS BY PATIENT
        // =========================================================
        public static DataTable GetRequisitionsByPatient(int patientUserId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT RequisitionID, RequisitionNumber, PatientName, BloodGroup, 
                              UnitsNeeded, Hospital, Urgency, Status, RequestDate, Remarks
                              FROM Requisitions 
                              WHERE PatientUserID = @PatientUserId
                              ORDER BY RequestDate DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@PatientUserId", patientUserId);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET REQUISITIONS BY CNIC
        // =========================================================
        public static DataTable GetRequisitionsByCNIC(string cnic)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT RequisitionID, RequisitionNumber, PatientName, BloodGroup, 
                              UnitsNeeded, Hospital, Urgency, Status, RequestDate, Remarks
                              FROM Requisitions 
                              WHERE CNIC = @CNIC
                              ORDER BY RequestDate DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CNIC", cnic);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET REQUISITIONS BY DOCTOR
        // =========================================================
        public static DataTable GetRequisitionsByDoctor(int doctorID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT RequisitionID, RequisitionNumber, PatientName, BloodGroup, 
                              UnitsNeeded, Hospital, Urgency, Status, RequestDate, Remarks
                              FROM Requisitions 
                              WHERE DoctorID = @DoctorID
                              ORDER BY RequestDate DESC";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DoctorID", doctorID);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET ALL PENDING REQUISITIONS
        // =========================================================
        public static DataTable GetPendingRequisitions()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT RequisitionID, RequisitionNumber, PatientName, 
                              BloodGroup, UnitsNeeded, Hospital, Urgency, RequestDate,
                              CASE 
                                  WHEN DoctorID IS NOT NULL AND DoctorID > 0 THEN 'Doctor Request' 
                                  WHEN PatientUserID IS NOT NULL AND PatientUserID > 0 THEN 'Patient Request'
                                  ELSE 'Direct Request'
                              END as RequestType
                              FROM Requisitions 
                              WHERE Status = 'Pending'
                              ORDER BY 
                                  CASE Urgency 
                                      WHEN 'Emergency' THEN 1 
                                      WHEN 'Urgent' THEN 2 
                                      ELSE 3 
                                  END,
                                  RequestDate ASC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET ALL REQUISITIONS
        // =========================================================
        public static DataTable GetAllRequisitions()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT RequisitionID, RequisitionNumber, PatientName, 
                              BloodGroup, UnitsNeeded, Hospital, Urgency, Status, RequestDate,
                              CASE 
                                  WHEN DoctorID IS NOT NULL THEN 'Doctor Request' 
                                  WHEN PatientUserID IS NOT NULL THEN 'Patient Request'
                                  ELSE 'Direct Request'
                              END as RequestType
                              FROM Requisitions 
                              ORDER BY RequestDate DESC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET ALL (Alias for GetAllRequisitions)
        // =========================================================
        public static DataTable GetAll()
        {
            return GetAllRequisitions();
        }

        // =========================================================
        // GET REQUISITIONS BY STATUS
        // =========================================================
        public static DataTable GetRequisitionsByStatus(string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT RequisitionID, RequisitionNumber, PatientName, 
                              BloodGroup, UnitsNeeded, Hospital, Urgency, RequestDate
                              FROM Requisitions 
                              WHERE Status = @Status
                              ORDER BY RequestDate DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET BY STATUS (Alias)
        // =========================================================
        public static DataTable GetByStatus(string status)
        {
            return GetRequisitionsByStatus(status);
        }

        // =========================================================
        // GET BY DOCTOR (Alias)
        // =========================================================
        public static DataTable GetByDoctor(int doctorID)
        {
            return GetRequisitionsByDoctor(doctorID);
        }

        // =========================================================
        // GET REQUISITION BY ID (Returns DataRow)
        // =========================================================
        public static DataRow GetRequisitionByID(int requisitionID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Requisitions WHERE RequisitionID = @RequisitionID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RequisitionID", requisitionID);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
                return null;
            }
        }

        // =========================================================
        // GET BY ID (Returns Requisition Model)
        // =========================================================
        public static Models.Requisition GetByID(int requisitionID)
        {
            var row = GetRequisitionByID(requisitionID);
            if (row == null) return null;
            return MapToRequisition(row);
        }

        // =========================================================
        // UPDATE STATUS (Synchronous - No async)
        // =========================================================
        public static bool UpdateStatus(int requisitionID, string status, string approvedBy = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand();
                cmd.Connection = conn;

                if (status == "Approved" || status == "Completed")
                {
                    cmd.CommandText = @"UPDATE Requisitions SET Status = @Status, ApprovalDate = GETDATE(), 
                                       ApprovedBy = @ApprovedBy WHERE RequisitionID = @RequisitionID";
                    cmd.Parameters.AddWithValue("@ApprovedBy", approvedBy ?? (object)DBNull.Value);
                }
                else
                {
                    cmd.CommandText = "UPDATE Requisitions SET Status = @Status WHERE RequisitionID = @RequisitionID";
                }

                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@RequisitionID", requisitionID);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // UPDATE REQUISITION STATUS (Alias with better name)
        // =========================================================
        public static bool UpdateRequisitionStatus(int requisitionID, string status, string approvedBy = null)
        {
            return UpdateStatus(requisitionID, status, approvedBy);
        }

        // =========================================================
        // UPDATE REMARKS ONLY
        // =========================================================
        public static bool UpdateRemarks(int requisitionID, string remarks)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Requisitions SET Remarks = @Remarks WHERE RequisitionID = @RequisitionID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Remarks", remarks);
                cmd.Parameters.AddWithValue("@RequisitionID", requisitionID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // APPROVE REQUISITION
        // =========================================================
        public static bool ApproveRequisition(int requisitionID, int approvedBy, string assignedBagID = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Requisitions SET 
                              Status = 'Approved', 
                              ApprovalDate = GETDATE(), 
                              ApprovedBy = @ApprovedBy,
                              AssignedBagID = @AssignedBagID
                              WHERE RequisitionID = @RequisitionID";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ApprovedBy", approvedBy);
                cmd.Parameters.AddWithValue("@AssignedBagID", assignedBagID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RequisitionID", requisitionID);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // REJECT REQUISITION
        // =========================================================
        public static bool RejectRequisition(int requisitionID, string reason)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Requisitions SET Status = 'Rejected', Remarks = @Reason WHERE RequisitionID = @RequisitionID";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Reason", reason);
                cmd.Parameters.AddWithValue("@RequisitionID", requisitionID);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // =========================================================
        // GET PENDING COUNT
        // =========================================================
        public static int GetPendingRequisitionCount()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Requisitions WHERE Status = 'Pending'";
                var cmd = new SqlCommand(sql, conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // =========================================================
        // GET URGENT REQUISITIONS
        // =========================================================
        public static DataTable GetUrgentRequisitions()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT RequisitionID, RequisitionNumber, PatientName, BloodGroup, 
                              UnitsNeeded, Hospital, RequestDate
                              FROM Requisitions 
                              WHERE Urgency IN ('Urgent', 'Emergency') AND Status = 'Pending'
                              ORDER BY CASE Urgency WHEN 'Emergency' THEN 1 WHEN 'Urgent' THEN 2 ELSE 3 END";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // SEARCH REQUISITIONS
        // =========================================================
        public static DataTable SearchRequisitions(string searchTerm)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT RequisitionID, RequisitionNumber, PatientName, BloodGroup, 
                              UnitsNeeded, Hospital, Status, RequestDate
                              FROM Requisitions 
                              WHERE PatientName LIKE @Search OR RequisitionNumber LIKE @Search 
                              OR Hospital LIKE @Search OR CNIC LIKE @Search
                              ORDER BY RequestDate DESC";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET REQUISITION STATISTICS
        // =========================================================
        public static DataTable GetRequisitionStatistics()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                              BloodGroup,
                              COUNT(*) as TotalRequests,
                              SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as Pending,
                              SUM(CASE WHEN Status = 'Approved' THEN 1 ELSE 0 END) as Approved,
                              SUM(CASE WHEN Urgency = 'Emergency' THEN 1 ELSE 0 END) as Emergency
                              FROM Requisitions 
                              GROUP BY BloodGroup
                              ORDER BY BloodGroup";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET REQUISITION COUNT BY DATE RANGE
        // =========================================================
        public static int GetRequisitionCount(DateTime fromDate, DateTime toDate)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Requisitions WHERE RequestDate BETWEEN @FromDate AND @ToDate";
                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FromDate", fromDate);
                cmd.Parameters.AddWithValue("@ToDate", toDate);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // =========================================================
        // GET ALL REQUISITIONS FOR ADMIN
        // =========================================================
        public static DataTable GetAllRequisitionsForAdmin()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT r.RequisitionID, r.RequisitionNumber, r.PatientName, 
                              r.BloodGroup, r.UnitsNeeded, r.Hospital, r.Urgency, r.Status, 
                              r.RequestDate, r.ApprovalDate, r.ApprovedBy,
                              d.FullName as DoctorName,
                              CASE 
                                  WHEN r.DoctorID IS NOT NULL THEN 'Doctor'
                                  WHEN r.PatientUserID IS NOT NULL THEN 'Patient'
                                  ELSE 'Direct'
                              END as RequestSource
                              FROM Requisitions r
                              LEFT JOIN Doctors d ON r.DoctorID = d.UserID
                              ORDER BY r.RequestDate DESC";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GET REQUISITIONS SUMMARY FOR DASHBOARD
        // =========================================================
        public static DataTable GetRequisitionsSummary()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT 
                              COUNT(*) as TotalRequisitions,
                              SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as PendingRequisitions,
                              SUM(CASE WHEN Status = 'Approved' THEN 1 ELSE 0 END) as ApprovedRequisitions,
                              SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) as CompletedRequisitions,
                              SUM(CASE WHEN Urgency = 'Emergency' THEN 1 ELSE 0 END) as EmergencyRequisitions
                              FROM Requisitions";
                var cmd = new SqlCommand(sql, conn);
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // =========================================================
        // GENERATE REQUISITION NUMBER
        // =========================================================
        private static string GenerateRequisitionNumber()
        {
            return $"REQ-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        // =========================================================
        // MAP TO REQUISITION MODEL
        // =========================================================
        private static Models.Requisition MapToRequisition(DataRow row)
        {
            var requisition = new Models.Requisition();

            if (row.Table.Columns.Contains("RequisitionID") && row["RequisitionID"] != DBNull.Value)
                requisition.RequisitionID = Convert.ToInt32(row["RequisitionID"]);

            if (row.Table.Columns.Contains("RequisitionNumber"))
                requisition.RequisitionNumber = row["RequisitionNumber"]?.ToString();

            if (row.Table.Columns.Contains("PatientName"))
                requisition.PatientName = row["PatientName"]?.ToString();

            if (row.Table.Columns.Contains("CNIC"))
                requisition.CNIC = row["CNIC"]?.ToString();

            if (row.Table.Columns.Contains("BloodGroup"))
                requisition.BloodGroup = row["BloodGroup"]?.ToString();

            if (row.Table.Columns.Contains("UnitsNeeded"))
                requisition.UnitsNeeded = Convert.ToInt32(row["UnitsNeeded"]);

            if (row.Table.Columns.Contains("Hospital"))
                requisition.Hospital = row["Hospital"]?.ToString();

            if (row.Table.Columns.Contains("Urgency"))
                requisition.Urgency = row["Urgency"]?.ToString();

            if (row.Table.Columns.Contains("Status"))
                requisition.Status = row["Status"]?.ToString();

            if (row.Table.Columns.Contains("RequestDate") && row["RequestDate"] != DBNull.Value)
                requisition.RequestDate = Convert.ToDateTime(row["RequestDate"]);

            if (row.Table.Columns.Contains("ApprovalDate") && row["ApprovalDate"] != DBNull.Value)
                requisition.ApprovalDate = Convert.ToDateTime(row["ApprovalDate"]);

            if (row.Table.Columns.Contains("ApprovedBy"))
                requisition.ApprovedBy = row["ApprovedBy"]?.ToString();

            if (row.Table.Columns.Contains("AssignedBagID"))
                requisition.AssignedBagID = row["AssignedBagID"]?.ToString();

            if (row.Table.Columns.Contains("Remarks"))
                requisition.Remarks = row["Remarks"]?.ToString();

            if (row.Table.Columns.Contains("DoctorID") && row["DoctorID"] != DBNull.Value)
                requisition.DoctorID = Convert.ToInt32(row["DoctorID"]);

            if (row.Table.Columns.Contains("PatientUserID") && row["PatientUserID"] != DBNull.Value)
                requisition.PatientUserID = Convert.ToInt32(row["PatientUserID"]);

            return requisition;
        }
    }
}