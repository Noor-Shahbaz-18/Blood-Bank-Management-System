using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class DonationHistory
    {
        public int DonationID { get; set; }
        public int DonorID { get; set; }
        public string DonorName { get; set; }
        public string BloodGroup { get; set; }
        public DateTime DonationDate { get; set; }
        public string DonationLocation { get; set; }
        public string BagID { get; set; }
        public int Units { get; set; }
        public int Volume { get; set; }
        public string DonationType { get; set; } // Whole Blood, Apheresis, etc.
        public string ScreeningResult { get; set; }
        public string StaffName { get; set; }
        public string Notes { get; set; }
        public bool IsCertificateIssued { get; set; }
        public string CertificatePath { get; set; }
        public DateTime NextEligibleDate { get; set; }
        public int DonationNumber { get; set; } // 1st, 2nd, 3rd donation
    }
}