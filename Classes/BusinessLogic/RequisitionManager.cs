using System;
using System.Data;
using BloodBankManagementSystem.Classes.Database;
using BloodBankManagementSystem.Classes.Models;

namespace BloodBankManagementSystem.Classes.BusinessLogic
{
    public class RequisitionManager
    {
        public static bool CreateRequisition(Requisition req)
        {
            if (req == null) return false;

            // Validation
            if (string.IsNullOrEmpty(req.PatientName)) return false;
            if (string.IsNullOrEmpty(req.BloodGroup)) return false;
            if (req.UnitsNeeded <= 0) return false;

            req.Status = "Pending";
            req.RequestDate = DateTime.Now;

            return RequisitionDAL.Insert(req);
        }

        public static DataTable GetAllRequisitions()
        {
            return RequisitionDAL.GetAll();
        }

        public static DataTable GetRequisitionsByDoctor(int doctorID)
        {
            return RequisitionDAL.GetByDoctor(doctorID);
        }

        public static DataTable GetPendingRequisitions()
        {
            return RequisitionDAL.GetByStatus("Pending");
        }

        public static bool UpdateRequisitionStatus(int reqID, string status, string approvedBy)
        {
            return RequisitionDAL.UpdateStatus(reqID, status, approvedBy);
        }

        public static bool ApproveRequisition(int reqID, int approvedBy)
        {
            return RequisitionDAL.UpdateStatus(reqID, "Approved", approvedBy.ToString());
        }

        public static bool RejectRequisition(int reqID, string reason)
        {
            return RequisitionDAL.UpdateStatus(reqID, "Rejected", reason);
        }

        public static Requisition GetRequisitionByID(int reqID)
        {
            return RequisitionDAL.GetByID(reqID);
        }
    }
}