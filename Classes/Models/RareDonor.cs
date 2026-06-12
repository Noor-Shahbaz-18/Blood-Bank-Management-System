using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class RareDonor
    {
        public int DonorID { get; set; }
        public string FullName { get; set; }
        public string CNIC { get; set; }
        public string BloodGroup { get; set; }
        public string RareBloodType { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public int Weight { get; set; }
        public string MedicalHistory { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public int TotalDonations { get; set; }
    }
}