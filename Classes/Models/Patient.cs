using System;

namespace BloodBankManagementSystem.Classes.Models
{
    public class Patient
    {
        public int PatientID { get; set; }
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
        public string Disease { get; set; }
        public string Hospital { get; set; }
        public string WardNumber { get; set; }
        public string DoctorName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }

        // Additional fields
        public string EmergencyContact { get; set; }
        public string MedicalHistory { get; set; }
        public byte[] ProfilePicture { get; set; }
    }
}