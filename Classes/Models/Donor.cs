using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Donor
    {
        public int DonorID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string CNIC { get; set; }
        public string BloodGroup { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Weight { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public int TotalDonations { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string EmergencyContact { get; set; }
        public string MedicalHistory { get; set; }
    }
}