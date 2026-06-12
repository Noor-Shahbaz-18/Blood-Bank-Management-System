using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class ScreeningRecord
    {
        public int ScreeningID { get; set; }
        public int DonorID { get; set; }
        public string DonorName { get; set; }
        public DateTime ScreeningDate { get; set; }
        public string TechnicianName { get; set; }

        // Medical Parameters
        public decimal Weight { get; set; } // in kg
        public decimal Temperature { get; set; } // in Celsius
        public string BloodPressure { get; set; } // e.g., "120/80"
        public int Pulse { get; set; }
        public decimal Hemoglobin { get; set; } // in g/dL

        // Screening Questions
        public bool HasInfectiousDisease { get; set; }
        public bool HasRecentSurgery { get; set; }
        public bool HasRecentTattoo { get; set; }
        public bool HasRecentPregnancy { get; set; }
        public bool IsOnMedication { get; set; }
        public string MedicationsList { get; set; }

        // Results
        public string ScreeningResult { get; set; } // Fit, Defer, Referred
        public string DeferralReason { get; set; }
        public DateTime? DeferralUntil { get; set; }

        public string Notes { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}