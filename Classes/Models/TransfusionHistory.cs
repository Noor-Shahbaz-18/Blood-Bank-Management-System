using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class TransfusionHistory
    {
        public int TransfusionID { get; set; }
        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public string PatientBloodGroup { get; set; }
        public string BagID { get; set; }
        public string DonorBloodGroup { get; set; }
        public int Units { get; set; }
        public DateTime TransfusionDate { get; set; }
        public string Hospital { get; set; }
        public string DoctorName { get; set; }
        public string Department { get; set; }
        public string CrossMatchResult { get; set; }
        public string ReactionType { get; set; } // None, Mild, Severe
        public string ReactionNotes { get; set; }
        public string Status { get; set; } // Completed, InProgress, Cancelled
        public string PrescribedBy { get; set; }
        public string PerformedBy { get; set; }
        public string VerifiedBy { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}