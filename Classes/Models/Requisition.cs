using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Requisition
    {
        // Primary Key
        public int RequisitionID { get; set; }

        // Requisition Details
        public string RequisitionNumber { get; set; }

        // Patient Information
        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public string CNIC { get; set; }
        public string BloodGroup { get; set; }
        public int UnitsNeeded { get; set; }

        // Doctor Information
        public int DoctorID { get; set; }
        public string DoctorName { get; set; }

        // Patient User (for self-requisitions)
        public int PatientUserID { get; set; }

        // Hospital Information
        public string Hospital { get; set; }
        public string WardNumber { get; set; }

        // Requisition Status
        public string Urgency { get; set; }  // Normal, Urgent, Emergency
        public string Status { get; set; }   // Pending, Approved, Rejected, Completed, Cross Matching, Cancelled

        // Dates
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? RequiredDate { get; set; }

        // Approval Information
        public string ApprovedBy { get; set; }
        public string AssignedBagID { get; set; }

        // Additional Information
        public string Remarks { get; set; }
        public string Attachments { get; set; }  // File paths for uploaded documents

        // Cross Matching
        public string CrossMatchResult { get; set; }  // Compatible, Incompatible, Pending
        public string CrossMatchPerformedBy { get; set; }
        public DateTime? CrossMatchDate { get; set; }

        // Transfusion Information
        public DateTime? TransfusionDate { get; set; }
        public string TransfusionPerformedBy { get; set; }
        public string TransfusionReaction { get; set; }

        // Audit Fields
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation Properties (not stored in DB)
        public string RequestType
        {
            get
            {
                if (DoctorID > 0) return "Doctor Request";
                if (PatientUserID > 0) return "Patient Request";
                return "Direct Request";
            }
        }

        public string StatusColor
        {
            get
            {
                switch (Status)
                {
                    case "Approved": return "#22c55e";  // Green
                    case "Pending": return "#f59e0b";   // Orange
                    case "Rejected": return "#ef4444";  // Red
                    case "Cross Matching": return "#3b82f6"; // Blue
                    case "Completed": return "#10b981"; // Emerald
                    case "Cancelled": return "#6b7280"; // Gray
                    default: return "#6b7280";
                }
            }
        }

        public bool IsPending => Status == "Pending";
        public bool IsApproved => Status == "Approved";
        public bool IsRejected => Status == "Rejected";
        public bool IsCompleted => Status == "Completed";
        public bool IsCancelled => Status == "Cancelled";
        public bool IsCrossMatching => Status == "Cross Matching";

        public bool IsEmergency => Urgency == "Emergency";
        public bool IsUrgent => Urgency == "Urgent";
        public bool IsNormal => Urgency == "Normal";
    }
}